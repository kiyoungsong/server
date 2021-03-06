/* ========================================================================
 * Copyright (c) 2005-2019 The OPC Foundation, Inc. All rights reserved.
 *
 * OPC Foundation MIT License 1.00
 * 
 * Permission is hereby granted, free of charge, to any person
 * obtaining a copy of this software and associated documentation
 * files (the "Software"), to deal in the Software without
 * restriction, including without limitation the rights to use,
 * copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the
 * Software is furnished to do so, subject to the following
 * conditions:
 * 
 * The above copyright notice and this permission notice shall be
 * included in all copies or substantial portions of the Software.
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
 * EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
 * OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
 * NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
 * HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
 * WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
 * FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
 * OTHER DEALINGS IN THE SOFTWARE.
 *
 * The complete license agreement can be found here:
 * http://opcfoundation.org/License/MIT/1.00/
 * ======================================================================*/

using System;
using System.Collections.Generic;
using System.Threading;
using Opc.Ua;
using Opc.Ua.Server;
using Range = Opc.Ua.Range;
using System.Diagnostics;
using System.Linq;
using System.Text;
using IronUtilites;
using System.Threading.Tasks;
using System.Collections.Concurrent;

namespace IronServer
{
    /// <summary>
    /// A node manager for a server that exposes several variables.
    /// </summary>
    public class IronNodeManager : CustomNodeManager2
    {
        #region Constructors
        /// <summary>
        /// Initializes the node manager.
        /// </summary>
        public IronNodeManager(IServerInternal server, ApplicationConfiguration configuration)
            : base(server, configuration, Namespaces.ReferenceApplications)
        {
            SystemContext.NodeIdFactory = this;

            // get the configuration for the node manager.
            m_configuration = configuration.ParseExtension<IronServerConfiguration>();

            // use suitable defaults if no configuration exists.
            if (m_configuration == null)
            {
                m_configuration = new IronServerConfiguration();
            }

            m_dynamicNodes = new List<BaseDataVariableState>();

            m_deviceManager = new DeviceManager();
            readTasks = new List<Task>();
            startPath = System.IO.Directory.GetCurrentDirectory();
            m_deviceManager.Start(startPath);

            _information = new Dictionary<string, InformationValue>();
            rootFolderDic = new Dictionary<string, FolderState>();
        }
        #endregion

        #region IDisposable Members
        /// <summary>
        /// An overrideable version of the Dispose.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // TBD
                foreach (InformationValue information in _information.Values)
                {
                    foreach (ConcurrentDictionary<double,Task> agent in information.AgentTasks.Values)
                    {
                        foreach (Task task in agent.Values)
                        {
                            task.Dispose();
                        }
                    }
                }

                foreach (Task task in readTasks)
                {
                    task.Dispose();
                }

                taskFlag = false;
                readTasks.Clear();
            }
        }
        #endregion

        #region INodeIdFactory Members
        /// <summary>
        /// Creates the NodeId for the specified node.
        /// </summary>
        public override NodeId New(ISystemContext context, NodeState node)
        {
            BaseInstanceState instance = node as BaseInstanceState;

            if (instance != null && instance.Parent != null)
            {
                string id = instance.Parent.NodeId.Identifier as string;

                if (id != null)
                {
                    return new NodeId(id + "_" + instance.SymbolicName, instance.Parent.NodeId.NamespaceIndex);
                }
            }

            return node.NodeId;
        }
        #endregion

        #region Private Helper Functions
        private static bool IsUnsignedAnalogType(BuiltInType builtInType)
        {
            if (builtInType == BuiltInType.Byte ||
                builtInType == BuiltInType.UInt16 ||
                builtInType == BuiltInType.UInt32 ||
                builtInType == BuiltInType.UInt64)
            {
                return true;
            }
            return false;
        }

        private static bool IsAnalogType(BuiltInType builtInType)
        {
            switch (builtInType)
            {
                case BuiltInType.Byte:
                case BuiltInType.UInt16:
                case BuiltInType.UInt32:
                case BuiltInType.UInt64:
                case BuiltInType.SByte:
                case BuiltInType.Int16:
                case BuiltInType.Int32:
                case BuiltInType.Int64:
                case BuiltInType.Float:
                case BuiltInType.Double:
                    return true;
            }
            return false;
        }

        private static Opc.Ua.Range GetAnalogRange(BuiltInType builtInType)
        {
            switch (builtInType)
            {
                case BuiltInType.UInt16:
                    return new Range(System.UInt16.MaxValue, System.UInt16.MinValue);
                case BuiltInType.UInt32:
                    return new Range(System.UInt32.MaxValue, System.UInt32.MinValue);
                case BuiltInType.UInt64:
                    return new Range(System.UInt64.MaxValue, System.UInt64.MinValue);
                case BuiltInType.SByte:
                    return new Range(System.SByte.MaxValue, System.SByte.MinValue);
                case BuiltInType.Int16:
                    return new Range(System.Int16.MaxValue, System.Int16.MinValue);
                case BuiltInType.Int32:
                    return new Range(System.Int32.MaxValue, System.Int32.MinValue);
                case BuiltInType.Int64:
                    return new Range(System.Int64.MaxValue, System.Int64.MinValue);
                case BuiltInType.Float:
                    return new Range(System.Single.MaxValue, System.Single.MinValue);
                case BuiltInType.Double:
                    return new Range(System.Double.MaxValue, System.Double.MinValue);
                case BuiltInType.Byte:
                    return new Range(System.Byte.MaxValue, System.Byte.MinValue);
                default:
                    return new Range(System.SByte.MaxValue, System.SByte.MinValue);
            }
        }
        #endregion

        #region INodeManager Members
        /// <summary>
        /// Does any initialization required before the address space can be used.
        /// </summary>
        /// <remarks>
        /// The externalReferences is an out parameter that allows the node manager to link to nodes
        /// in other node managers. For example, the 'Objects' node is managed by the CoreNodeManager and
        /// should have a reference to the root folder node(s) exposed by this node manager.  
        /// </remarks>
        public override void CreateAddressSpace(IDictionary<NodeId, IList<IReference>> externalReferences)
        {
            lock (Lock)
            {
                IList<IReference> references = null;

                if (!externalReferences.TryGetValue(ObjectIds.ObjectsFolder, out references))
                {
                    externalReferences[ObjectIds.ObjectsFolder] = references = new List<IReference>();
                }

                foreach (string channelName in m_deviceManager.DeviceInfoDic.Keys)
                {
                    FolderState channelFolder = CreateFolder(null, channelName, channelName);
                    channelFolder.AddReference(ReferenceTypes.Organizes, true, ObjectIds.ObjectsFolder);
                    references.Add(new NodeStateReference(ReferenceTypes.Organizes, false, channelFolder.NodeId));
                    channelFolder.EventNotifier = EventNotifiers.SubscribeToEvents;
                    AddRootNotifier(channelFolder);

                    foreach (string driverName in m_deviceManager.DeviceInfoDic[channelName].Keys)
                    {
                        FolderState deviceFolder = CreateFolder(channelFolder, channelName + "_" +  driverName, driverName);

                        _information.Add(channelName + "." + driverName, new InformationValue());

                        CreateVariables(channelName, driverName, deviceFolder);

                        string tagName = connectionTag;
                        FolderState statisticsFolder = CreateFolder(deviceFolder, channelName + "_" + driverName + "_" + "_Statistics" , "_Statistics");
                        _information[channelName + "." + driverName].Variables.Add(tagName, CreateVariable(statisticsFolder, channelName + "." + driverName + "._Statistics." + tagName, tagName, BuiltInType.Int16, ValueRanks.Scalar));
                        m_deviceManager.DeviceDic[channelName][driverName].TagInfoDic.Add(tagName, new _tagIOInfo(){ StrName = tagName, TType = typeof(short), BRedis = true, ISize = 1 } );
                    }

                    AddPredefinedNode(SystemContext, channelFolder);
                }
            }

            taskFlag = true;

            foreach (string channel in m_deviceManager.DeviceDic.Keys)
            {
                foreach (string device in m_deviceManager.DeviceDic[channel].Keys)
                {
                    if(m_deviceManager.DeviceInfoDic[channel][device].TagDeviceInfo.UScanMode == 1)
                    {
                        uint scanTime = m_deviceManager.DeviceInfoDic[channel][device].TagDeviceInfo.UScanRate;
                        readTasks.Add(Task.Factory.StartNew(() => ReadCommunicationStatus(channel, device, scanTime)));
                    }
                }
            }
        }

        private void ReadCommunicationStatus(string channel, string device, uint scanTime)
        {
            while (taskFlag)
            {
                Stopwatch watch = new Stopwatch();
                watch.Start();
                bool status = true;
                if (m_deviceManager.DeviceInfoDic[channel][device].TagDeviceInfo.BReal)
                {
                    status = m_deviceManager.DeviceDic[channel][device].CommunicationStatus();
                }

                _information[channel + "." + device].Variables[connectionTag].Value = status;
                _information[channel + "." + device].Variables[connectionTag].Timestamp = DateTime.UtcNow;
                _information[channel + "." + device].Variables[connectionTag].ClearChangeMasks(SystemContext, false);

                if (status)
                {
                    GetValues(m_deviceManager.DeviceDic[channel][device].TagInfoDic.Keys.ToList(), channel, device);
                }
                else
                {
                    m_deviceManager.DeviceDic[channel][device].DisConnect();
                    m_deviceManager.DeviceDic[channel][device].Connect();
                }

                double checkTime = (double)watch.ElapsedMilliseconds;
                int setInterval = Convert.ToInt32(Math.Max(1, scanTime - checkTime));

                Thread.Sleep(setInterval);
            }
        }

        private void CreateVariables(string channelName, string driverName, FolderState deviceFolder)
        {
            try
            {
                foreach (string tag in m_deviceManager.DeviceDic[channelName][driverName].TagInfoDic.Keys)
                {
                    try
                    {
                        string nodePath = channelName + "." + driverName + "." + tag;
                        _tagIOInfo taginfo = m_deviceManager.DeviceDic[channelName][driverName].TagInfoDic[tag];
                        BuiltInType builtType;
                        if (taginfo.TType == typeof(float))
                        {
                            builtType = (Opc.Ua.BuiltInType)Enum.Parse(typeof(BuiltInType), "Float");
                        }
                        else
                        {
                            builtType = (Opc.Ua.BuiltInType)Enum.Parse(typeof(BuiltInType), taginfo.TType.Name);
                        }

                        if (taginfo.ISize > 1)
                        {
                            if (taginfo.TType == typeof(float))
                            {
                                _information[channelName + "." + driverName].Variables.Add(tag, CreateVariable(deviceFolder, nodePath, tag, builtType, ValueRanks.OneDimension));
                            }
                            else if (taginfo.TType == typeof(string))
                            {
                                _information[channelName + "." + driverName].Variables.Add(tag, CreateVariable(deviceFolder, nodePath, tag, builtType, ValueRanks.Scalar));
                            }
                            else
                            {
                                _information[channelName + "." + driverName].Variables.Add(tag, CreateVariable(deviceFolder, nodePath, tag, builtType, ValueRanks.OneDimension));
                            }
                        }
                        else if (taginfo.ISize == 1)
                        {
                            if (taginfo.TType == typeof(float))
                            {
                                _information[channelName + "." + driverName].Variables.Add(tag, CreateVariable(deviceFolder, nodePath, tag, builtType, ValueRanks.Scalar));
                            }
                            else
                            {
                                _information[channelName + "." + driverName].Variables.Add(tag, CreateVariable(deviceFolder, nodePath, tag, builtType, ValueRanks.Scalar));
                            }
                        }
                        else
                        {
                            continue;
                        }

                        //_information[channelName + "." + driverName].TagNameValues.Add(tag, 0);
                    }
                    catch (Exception ex)
                    {
                        IronUtilites.LogManager.Manager.WriteLog("Exception", "Error creating the address space : " + ex.Message + ", TagName : " + tag);
                    }
                }
            }
            catch (Exception ex)
            {
                Utils.Trace(ex, "Error creating the address space.");
                IronUtilites.LogManager.Manager.WriteLog("Exception", "Error creating the address space : " + ex.Message);
            }
        }

        private ServiceResult OnWriteInterval(ISystemContext context, NodeState node, ref object value)
        {
            try
            {
                m_simulationInterval = (UInt16)value;

                if (m_simulationEnabled)
                {
                    m_simulationTimer.Change(100, (int)m_simulationInterval);
                }

                return ServiceResult.Good;
            }
            catch (Exception e)
            {
                Utils.Trace(e, "Error writing Interval variable.");
                return ServiceResult.Create(e, StatusCodes.Bad, "Error writing Interval variable.");
            }
        }

        private ServiceResult OnWriteEnabled(ISystemContext context, NodeState node, ref object value)
        {
            try
            {
                m_simulationEnabled = (bool)value;

                if (m_simulationEnabled)
                {
                    m_simulationTimer.Change(100, (int)m_simulationInterval);
                }
                else
                {
                    m_simulationTimer.Change(100, 0);
                }

                return ServiceResult.Good;
            }
            catch (Exception e)
            {
                Utils.Trace(e, "Error writing Enabled variable.");
                return ServiceResult.Create(e, StatusCodes.Bad, "Error writing Enabled variable.");
            }
        }

        /// <summary>
        /// Creates a new folder.
        /// </summary>
        private FolderState CreateFolder(NodeState parent, string path, string name)
        {
            FolderState folder = new FolderState(parent);

            folder.SymbolicName = name;
            folder.ReferenceTypeId = ReferenceTypes.Organizes;
            folder.TypeDefinitionId = ObjectTypeIds.FolderType;
            folder.NodeId = new NodeId(path, NamespaceIndex);
            folder.BrowseName = new QualifiedName(path, NamespaceIndex);
            folder.DisplayName = new LocalizedText("en", name);
            folder.WriteMask = AttributeWriteMask.None;
            folder.UserWriteMask = AttributeWriteMask.None;
            folder.EventNotifier = EventNotifiers.None;

            if (parent != null)
            {
                parent.AddChild(folder);
            }

            return folder;
        }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        private BaseObjectState CreateObject(NodeState parent, string path, string name)
        {
            BaseObjectState folder = new BaseObjectState(parent);

            folder.SymbolicName = name;
            folder.ReferenceTypeId = ReferenceTypes.Organizes;
            folder.TypeDefinitionId = ObjectTypeIds.BaseObjectType;
            folder.NodeId = new NodeId(path, NamespaceIndex);
            folder.BrowseName = new QualifiedName(name, NamespaceIndex);
            folder.DisplayName = folder.BrowseName.Name;
            folder.WriteMask = AttributeWriteMask.None;
            folder.UserWriteMask = AttributeWriteMask.None;
            folder.EventNotifier = EventNotifiers.None;

            if (parent != null)
            {
                parent.AddChild(folder);
            }

            return folder;
        }

        /// <summary>
        /// Creates a new object type.
        /// </summary>
        private BaseObjectTypeState CreateObjectType(NodeState parent, IDictionary<NodeId, IList<IReference>> externalReferences, string path, string name)
        {
            BaseObjectTypeState type = new BaseObjectTypeState();

            type.SymbolicName = name;
            type.SuperTypeId = ObjectTypeIds.BaseObjectType;
            type.NodeId = new NodeId(path, NamespaceIndex);
            type.BrowseName = new QualifiedName(name, NamespaceIndex);
            type.DisplayName = type.BrowseName.Name;
            type.WriteMask = AttributeWriteMask.None;
            type.UserWriteMask = AttributeWriteMask.None;
            type.IsAbstract = false;

            IList<IReference> references = null;

            if (!externalReferences.TryGetValue(ObjectTypeIds.BaseObjectType, out references))
            {
                externalReferences[ObjectTypeIds.BaseObjectType] = references = new List<IReference>();
            }

            references.Add(new NodeStateReference(ReferenceTypes.HasSubtype, false, type.NodeId));

            if (parent != null)
            {
                parent.AddReference(ReferenceTypes.Organizes, false, type.NodeId);
                type.AddReference(ReferenceTypes.Organizes, true, parent.NodeId);
            }

            AddPredefinedNode(SystemContext, type);
            return type;
        }

        /// <summary>
        /// Creates a new variable.
        /// </summary>
        private BaseDataVariableState CreateMeshVariable(NodeState parent, string path, string name, params NodeState[] peers)
        {
            BaseDataVariableState variable = CreateVariable(parent, path, name, BuiltInType.Double, ValueRanks.Scalar);

            if (peers != null)
            {
                foreach (NodeState peer in peers)
                {
                    peer.AddReference(ReferenceTypes.HasCause, false, variable.NodeId);
                    variable.AddReference(ReferenceTypes.HasCause, true, peer.NodeId);
                    peer.AddReference(ReferenceTypes.HasEffect, true, variable.NodeId);
                    variable.AddReference(ReferenceTypes.HasEffect, false, peer.NodeId);
                }
            }

            return variable;
        }

        /// <summary>
        /// Creates a new variable.
        /// </summary>
        private DataItemState CreateDataItemVariable(NodeState parent, string path, string name, BuiltInType dataType, int valueRank)
        {
            DataItemState variable = new DataItemState(parent);
            variable.ValuePrecision = new PropertyState<double>(variable);
            variable.Definition = new PropertyState<string>(variable);

            variable.Create(
                SystemContext,
                null,
                variable.BrowseName,
                null,
                true);

            variable.SymbolicName = name;
            variable.ReferenceTypeId = ReferenceTypes.Organizes;
            variable.NodeId = new NodeId(path, NamespaceIndex);
            variable.BrowseName = new QualifiedName(path, NamespaceIndex);
            variable.DisplayName = new LocalizedText("en", name);
            variable.WriteMask = AttributeWriteMask.None;
            variable.UserWriteMask = AttributeWriteMask.None;
            variable.DataType = (uint)dataType;
            variable.ValueRank = valueRank;
            variable.AccessLevel = AccessLevels.CurrentReadOrWrite;
            variable.UserAccessLevel = AccessLevels.CurrentReadOrWrite;
            variable.Historizing = false;
            variable.Value = Opc.Ua.TypeInfo.GetDefaultValue((uint)dataType, valueRank, Server.TypeTree);
            variable.StatusCode = StatusCodes.Good;
            variable.Timestamp = DateTime.UtcNow;

            if (valueRank == ValueRanks.OneDimension)
            {
                variable.ArrayDimensions = new ReadOnlyList<uint>(new List<uint> { 0 });
            }
            else if (valueRank == ValueRanks.TwoDimensions)
            {
                variable.ArrayDimensions = new ReadOnlyList<uint>(new List<uint> { 0, 0 });
            }

            variable.ValuePrecision.Value = 2;
            variable.ValuePrecision.AccessLevel = AccessLevels.CurrentReadOrWrite;
            variable.ValuePrecision.UserAccessLevel = AccessLevels.CurrentReadOrWrite;
            variable.Definition.Value = String.Empty;
            variable.Definition.AccessLevel = AccessLevels.CurrentReadOrWrite;
            variable.Definition.UserAccessLevel = AccessLevels.CurrentReadOrWrite;

            if (parent != null)
            {
                parent.AddChild(variable);
            }

            return variable;
        }

        private DataItemState[] CreateDataItemVariables(NodeState parent, string path, string name, BuiltInType dataType, int valueRank, UInt16 numVariables)
        {
            List<DataItemState> itemsCreated = new List<DataItemState>();
            // create the default name first:
            itemsCreated.Add(CreateDataItemVariable(parent, path, name, dataType, valueRank));
            // now to create the remaining NUMBERED items
            for (uint i = 0; i < numVariables; i++)
            {
                string newName = string.Format("{0}{1}", name, i.ToString("000"));
                string newPath = string.Format("{0}/Mass/{1}", path, newName);
                itemsCreated.Add(CreateDataItemVariable(parent, newPath, newName, dataType, valueRank));
            }//for i
            return (itemsCreated.ToArray());
        }

        private ServiceResult OnWriteDataItem(
            ISystemContext context,
            NodeState node,
            NumericRange indexRange,
            QualifiedName dataEncoding,
            ref object value,
            ref StatusCode statusCode,
            ref DateTime timestamp)
        {
            DataItemState variable = node as DataItemState;

            // verify data type.
            Opc.Ua.TypeInfo typeInfo = Opc.Ua.TypeInfo.IsInstanceOfDataType(
                value,
                variable.DataType,
                variable.ValueRank,
                context.NamespaceUris,
                context.TypeTable);

            if (typeInfo == null || typeInfo == Opc.Ua.TypeInfo.Unknown)
            {
                return StatusCodes.BadTypeMismatch;
            }

            if (typeInfo.BuiltInType != BuiltInType.DateTime)
            {
                double number = Convert.ToDouble(value);
                number = Math.Round(number, (int)variable.ValuePrecision.Value);
                value = Opc.Ua.TypeInfo.Cast(number, typeInfo.BuiltInType);
            }

            return ServiceResult.Good;
        }

        /// <summary>
        /// Creates a new variable.
        /// </summary>
        private AnalogItemState CreateAnalogItemVariable(NodeState parent, string path, string name, BuiltInType dataType, int valueRank)
        {
            return (CreateAnalogItemVariable(parent, path, name, dataType, valueRank, null));
        }

        private AnalogItemState CreateAnalogItemVariable(NodeState parent, string path, string name, BuiltInType dataType, int valueRank, object initialValues)
        {
            return (CreateAnalogItemVariable(parent, path, name, dataType, valueRank, initialValues, null));
        }

        private AnalogItemState CreateAnalogItemVariable(NodeState parent, string path, string name, BuiltInType dataType, int valueRank, object initialValues, Range customRange)
        {
            return CreateAnalogItemVariable(parent, path, name, (uint)dataType, valueRank, initialValues, customRange);
        }

        private AnalogItemState CreateAnalogItemVariable(NodeState parent, string path, string name, NodeId dataType, int valueRank, object initialValues, Range customRange)
        {
            AnalogItemState variable = new AnalogItemState(parent);
            variable.BrowseName = new QualifiedName(path, NamespaceIndex);
            variable.EngineeringUnits = new PropertyState<EUInformation>(variable);
            variable.InstrumentRange = new PropertyState<Range>(variable);

            variable.Create(
                SystemContext,
                new NodeId(path, NamespaceIndex),
                variable.BrowseName,
                null,
                true);

            variable.NodeId = new NodeId(path, NamespaceIndex);
            variable.SymbolicName = name;
            variable.DisplayName = new LocalizedText("en", name);
            variable.WriteMask = AttributeWriteMask.None;
            variable.UserWriteMask = AttributeWriteMask.None;
            variable.ReferenceTypeId = ReferenceTypes.Organizes;
            variable.DataType = dataType;
            variable.ValueRank = valueRank;
            variable.AccessLevel = AccessLevels.CurrentReadOrWrite;
            variable.UserAccessLevel = AccessLevels.CurrentReadOrWrite;
            variable.Historizing = false;

            if (valueRank == ValueRanks.OneDimension)
            {
                variable.ArrayDimensions = new ReadOnlyList<uint>(new List<uint> { 0 });
            }
            else if (valueRank == ValueRanks.TwoDimensions)
            {
                variable.ArrayDimensions = new ReadOnlyList<uint>(new List<uint> { 0, 0 });
            }

            BuiltInType builtInType = Opc.Ua.TypeInfo.GetBuiltInType(dataType, Server.TypeTree);

            // Simulate a mV Voltmeter
            Range newRange = GetAnalogRange(builtInType);
            // Using anything but 120,-10 fails a few tests
            newRange.High = Math.Min(newRange.High, 120);
            newRange.Low = Math.Max(newRange.Low, -10);
            variable.InstrumentRange.Value = newRange;

            if (customRange != null)
            {
                variable.EURange.Value = customRange;
            }
            else
            {
                variable.EURange.Value = new Range(100, 0);
            }

            if (initialValues == null)
            {
                variable.Value = Opc.Ua.TypeInfo.GetDefaultValue(dataType, valueRank, Server.TypeTree);
            }
            else
            {
                variable.Value = initialValues;
            }

            variable.StatusCode = StatusCodes.Good;
            variable.Timestamp = DateTime.UtcNow;
            // The latest UNECE version (Rev 11, published in 2015) is available here:
            // http://www.opcfoundation.org/UA/EngineeringUnits/UNECE/rec20_latest_08052015.zip
            variable.EngineeringUnits.Value = new EUInformation("mV", "millivolt", "http://www.opcfoundation.org/UA/units/un/cefact");
            // The mapping of the UNECE codes to OPC UA(EUInformation.unitId) is available here:
            // http://www.opcfoundation.org/UA/EngineeringUnits/UNECE/UNECE_to_OPCUA.csv
            variable.EngineeringUnits.Value.UnitId = 12890; // "2Z"
            variable.OnWriteValue = OnWriteAnalog;
            variable.EURange.OnWriteValue = OnWriteAnalogRange;
            variable.EURange.AccessLevel = AccessLevels.CurrentReadOrWrite;
            variable.EURange.UserAccessLevel = AccessLevels.CurrentReadOrWrite;
            variable.EngineeringUnits.AccessLevel = AccessLevels.CurrentReadOrWrite;
            variable.EngineeringUnits.UserAccessLevel = AccessLevels.CurrentReadOrWrite;
            variable.InstrumentRange.OnWriteValue = OnWriteAnalogRange;
            variable.InstrumentRange.AccessLevel = AccessLevels.CurrentReadOrWrite;
            variable.InstrumentRange.UserAccessLevel = AccessLevels.CurrentReadOrWrite;

            if (parent != null)
            {
                parent.AddChild(variable);
            }

            return variable;
        }

        /// <summary>
        /// Creates a new variable.
        /// </summary>
        private DataItemState CreateTwoStateDiscreteItemVariable(NodeState parent, string path, string name, string trueState, string falseState)
        {
            TwoStateDiscreteState variable = new TwoStateDiscreteState(parent);

            variable.NodeId = new NodeId(path, NamespaceIndex);
            variable.BrowseName = new QualifiedName(path, NamespaceIndex);
            variable.DisplayName = new LocalizedText("en", name);
            variable.WriteMask = AttributeWriteMask.None;
            variable.UserWriteMask = AttributeWriteMask.None;

            variable.Create(
                SystemContext,
                null,
                variable.BrowseName,
                null,
                true);

            variable.SymbolicName = name;
            variable.ReferenceTypeId = ReferenceTypes.Organizes;
            variable.DataType = DataTypeIds.Boolean;
            variable.ValueRank = ValueRanks.Scalar;
            variable.AccessLevel = AccessLevels.CurrentReadOrWrite;
            variable.UserAccessLevel = AccessLevels.CurrentReadOrWrite;
            variable.Historizing = false;
            variable.Value = (bool)GetNewValue(variable);
            variable.StatusCode = StatusCodes.Good;
            variable.Timestamp = DateTime.UtcNow;

            variable.TrueState.Value = trueState;
            variable.TrueState.AccessLevel = AccessLevels.CurrentReadOrWrite;
            variable.TrueState.UserAccessLevel = AccessLevels.CurrentReadOrWrite;

            variable.FalseState.Value = falseState;
            variable.FalseState.AccessLevel = AccessLevels.CurrentReadOrWrite;
            variable.FalseState.UserAccessLevel = AccessLevels.CurrentReadOrWrite;

            if (parent != null)
            {
                parent.AddChild(variable);
            }

            return variable;
        }

        /// <summary>
        /// Creates a new variable.
        /// </summary>
        private DataItemState CreateMultiStateDiscreteItemVariable(NodeState parent, string path, string name, params string[] values)
        {
            MultiStateDiscreteState variable = new MultiStateDiscreteState(parent);

            variable.NodeId = new NodeId(path, NamespaceIndex);
            variable.BrowseName = new QualifiedName(path, NamespaceIndex);
            variable.DisplayName = new LocalizedText("en", name);
            variable.WriteMask = AttributeWriteMask.None;
            variable.UserWriteMask = AttributeWriteMask.None;

            variable.Create(
                SystemContext,
                null,
                variable.BrowseName,
                null,
                true);

            variable.SymbolicName = name;
            variable.ReferenceTypeId = ReferenceTypes.Organizes;
            variable.DataType = DataTypeIds.UInt32;
            variable.ValueRank = ValueRanks.Scalar;
            variable.AccessLevel = AccessLevels.CurrentReadOrWrite;
            variable.UserAccessLevel = AccessLevels.CurrentReadOrWrite;
            variable.Historizing = false;
            variable.Value = (uint)0;
            variable.StatusCode = StatusCodes.Good;
            variable.Timestamp = DateTime.UtcNow;
            variable.OnWriteValue = OnWriteDiscrete;

            LocalizedText[] strings = new LocalizedText[values.Length];

            for (int ii = 0; ii < strings.Length; ii++)
            {
                strings[ii] = values[ii];
            }

            variable.EnumStrings.Value = strings;
            variable.EnumStrings.AccessLevel = AccessLevels.CurrentReadOrWrite;
            variable.EnumStrings.UserAccessLevel = AccessLevels.CurrentReadOrWrite;

            if (parent != null)
            {
                parent.AddChild(variable);
            }

            return variable;
        }

        /// <summary>
        /// Creates a new UInt32 variable.
        /// </summary>
        private DataItemState CreateMultiStateValueDiscreteItemVariable(NodeState parent, string path, string name, params string[] enumNames)
        {
            return CreateMultiStateValueDiscreteItemVariable(parent, path, name, null, enumNames);
        }

        /// <summary>
        /// Creates a new variable.
        /// </summary>
        private DataItemState CreateMultiStateValueDiscreteItemVariable(NodeState parent, string path, string name, NodeId nodeId, params string[] enumNames)
        {
            MultiStateValueDiscreteState variable = new MultiStateValueDiscreteState(parent);

            variable.NodeId = new NodeId(path, NamespaceIndex);
            variable.BrowseName = new QualifiedName(path, NamespaceIndex);
            variable.DisplayName = new LocalizedText("en", name);
            variable.WriteMask = AttributeWriteMask.None;
            variable.UserWriteMask = AttributeWriteMask.None;

            variable.Create(
                SystemContext,
                null,
                variable.BrowseName,
                null,
                true);

            variable.SymbolicName = name;
            variable.ReferenceTypeId = ReferenceTypes.Organizes;
            variable.DataType = (nodeId == null) ? DataTypeIds.UInt32 : nodeId;
            variable.ValueRank = ValueRanks.Scalar;
            variable.AccessLevel = AccessLevels.CurrentReadOrWrite;
            variable.UserAccessLevel = AccessLevels.CurrentReadOrWrite;
            variable.Historizing = false;
            variable.Value = (uint)0;
            variable.StatusCode = StatusCodes.Good;
            variable.Timestamp = DateTime.UtcNow;
            variable.OnWriteValue = OnWriteValueDiscrete;

            // there are two enumerations for this type:
            // EnumStrings = the string representations for enumerated values
            // ValueAsText = the actual enumerated value

            // set the enumerated strings
            LocalizedText[] strings = new LocalizedText[enumNames.Length];
            for (int ii = 0; ii < strings.Length; ii++)
            {
                strings[ii] = enumNames[ii];
            }

            // set the enumerated values
            EnumValueType[] values = new EnumValueType[enumNames.Length];
            for (int ii = 0; ii < values.Length; ii++)
            {
                values[ii] = new EnumValueType();
                values[ii].Value = ii;
                values[ii].Description = strings[ii];
                values[ii].DisplayName = strings[ii];
            }
            variable.EnumValues.Value = values;
            variable.EnumValues.AccessLevel = AccessLevels.CurrentReadOrWrite;
            variable.EnumValues.UserAccessLevel = AccessLevels.CurrentReadOrWrite;
            variable.ValueAsText.Value = variable.EnumValues.Value[0].DisplayName;

            if (parent != null)
            {
                parent.AddChild(variable);
            }

            return variable;
        }

        private ServiceResult OnWriteDiscrete(
            ISystemContext context,
            NodeState node,
            NumericRange indexRange,
            QualifiedName dataEncoding,
            ref object value,
            ref StatusCode statusCode,
            ref DateTime timestamp)
        {
            MultiStateDiscreteState variable = node as MultiStateDiscreteState;

            // verify data type.
            Opc.Ua.TypeInfo typeInfo = Opc.Ua.TypeInfo.IsInstanceOfDataType(
                value,
                variable.DataType,
                variable.ValueRank,
                context.NamespaceUris,
                context.TypeTable);

            if (typeInfo == null || typeInfo == Opc.Ua.TypeInfo.Unknown)
            {
                return StatusCodes.BadTypeMismatch;
            }

            if (indexRange != NumericRange.Empty)
            {
                return StatusCodes.BadIndexRangeInvalid;
            }

            double number = Convert.ToDouble(value);

            if (number >= variable.EnumStrings.Value.Length | number < 0)
            {
                return StatusCodes.BadOutOfRange;
            }

            return ServiceResult.Good;
        }

        private ServiceResult OnWriteValueDiscrete(
            ISystemContext context,
            NodeState node,
            NumericRange indexRange,
            QualifiedName dataEncoding,
            ref object value,
            ref StatusCode statusCode,
            ref DateTime timestamp)
        {
            MultiStateValueDiscreteState variable = node as MultiStateValueDiscreteState;

            TypeInfo typeInfo = TypeInfo.Construct(value);

            if (variable == null ||
                typeInfo == null ||
                typeInfo == Opc.Ua.TypeInfo.Unknown ||
                !TypeInfo.IsNumericType(typeInfo.BuiltInType))
            {
                return StatusCodes.BadTypeMismatch;
            }

            if (indexRange != NumericRange.Empty)
            {
                return StatusCodes.BadIndexRangeInvalid;
            }

            Int32 number = Convert.ToInt32(value);
            if (number >= variable.EnumValues.Value.Length || number < 0)
            {
                return StatusCodes.BadOutOfRange;
            }

            if (!node.SetChildValue(context, BrowseNames.ValueAsText, variable.EnumValues.Value[number].DisplayName, true))
            {
                return StatusCodes.BadOutOfRange;
            }

            node.ClearChangeMasks(context, true);

            return ServiceResult.Good;
        }

        private ServiceResult OnWriteAnalog(
            ISystemContext context,
            NodeState node,
            NumericRange indexRange,
            QualifiedName dataEncoding,
            ref object value,
            ref StatusCode statusCode,
            ref DateTime timestamp)
        {
            AnalogItemState variable = node as AnalogItemState;

            // verify data type.
            Opc.Ua.TypeInfo typeInfo = Opc.Ua.TypeInfo.IsInstanceOfDataType(
                value,
                variable.DataType,
                variable.ValueRank,
                context.NamespaceUris,
                context.TypeTable);

            if (typeInfo == null || typeInfo == Opc.Ua.TypeInfo.Unknown)
            {
                return StatusCodes.BadTypeMismatch;
            }

            // check index range.
            if (variable.ValueRank >= 0)
            {
                if (indexRange != NumericRange.Empty)
                {
                    object target = variable.Value;
                    ServiceResult result = indexRange.UpdateRange(ref target, value);

                    if (ServiceResult.IsBad(result))
                    {
                        return result;
                    }

                    value = target;
                }
            }

            // check instrument range.
            else
            {
                if (indexRange != NumericRange.Empty)
                {
                    return StatusCodes.BadIndexRangeInvalid;
                }

                double number = Convert.ToDouble(value);

                if (variable.InstrumentRange != null && (number < variable.InstrumentRange.Value.Low || number > variable.InstrumentRange.Value.High))
                {
                    return StatusCodes.BadOutOfRange;
                }
            }

            return ServiceResult.Good;
        }

        private ServiceResult OnWriteAnalogRange(
            ISystemContext context,
            NodeState node,
            NumericRange indexRange,
            QualifiedName dataEncoding,
            ref object value,
            ref StatusCode statusCode,
            ref DateTime timestamp)
        {
            PropertyState<Range> variable = node as PropertyState<Range>;
            ExtensionObject extensionObject = value as ExtensionObject;
            TypeInfo typeInfo = TypeInfo.Construct(value);

            if (variable == null ||
                extensionObject == null ||
                typeInfo == null ||
                typeInfo == Opc.Ua.TypeInfo.Unknown)
            {
                return StatusCodes.BadTypeMismatch;
            }

            Range newRange = extensionObject.Body as Range;
            AnalogItemState parent = variable.Parent as AnalogItemState;
            if (newRange == null ||
                parent == null)
            {
                return StatusCodes.BadTypeMismatch;
            }

            if (indexRange != NumericRange.Empty)
            {
                return StatusCodes.BadIndexRangeInvalid;
            }

            TypeInfo parentTypeInfo = TypeInfo.Construct(parent.Value);
            Range parentRange = GetAnalogRange(parentTypeInfo.BuiltInType);
            if (parentRange.High < newRange.High ||
                parentRange.Low > newRange.Low)
            {
                return StatusCodes.BadOutOfRange;
            }

            value = newRange;

            return ServiceResult.Good;
        }

        /// <summary>
        /// Creates a new variable.
        /// </summary>
        private BaseDataVariableState CreateVariable(NodeState parent, string path, string name, BuiltInType dataType, int valueRank)
        {
            return CreateVariable(parent, path, name, (uint)dataType, valueRank);
        }

        /// <summary>
        /// Creates a new variable.
        /// </summary>
        private BaseDataVariableState CreateVariable(NodeState parent, string path, string name, NodeId dataType, int valueRank)
        {
            BaseDataVariableState variable = new BaseDataVariableState(parent);

            variable.SymbolicName = name;
            variable.ReferenceTypeId = ReferenceTypes.Organizes;
            variable.TypeDefinitionId = VariableTypeIds.BaseDataVariableType;
            variable.NodeId = new NodeId(path, NamespaceIndex);
            variable.BrowseName = new QualifiedName(path, NamespaceIndex);
            variable.DisplayName = new LocalizedText("en", name);
            variable.WriteMask = AttributeWriteMask.DisplayName | AttributeWriteMask.Description;
            variable.UserWriteMask = AttributeWriteMask.DisplayName | AttributeWriteMask.Description;
            variable.DataType = dataType;
            variable.ValueRank = valueRank;
            variable.AccessLevel = AccessLevels.CurrentReadOrWrite;
            variable.UserAccessLevel = AccessLevels.CurrentReadOrWrite;
            variable.Historizing = false;
            variable.Value = GetNewValue(variable);
            variable.StatusCode = StatusCodes.Good;
            variable.Timestamp = DateTime.UtcNow;

            if (valueRank == ValueRanks.OneDimension)
            {
                variable.ArrayDimensions = new ReadOnlyList<uint>(new List<uint> { 0 });
            }
            else if (valueRank == ValueRanks.TwoDimensions)
            {
                variable.ArrayDimensions = new ReadOnlyList<uint>(new List<uint> { 0, 0 });
            }

            if (parent != null)
            {
                parent.AddChild(variable);
            }

            return variable;
        }

        private BaseDataVariableState[] CreateVariables(NodeState parent, string path, string name, BuiltInType dataType, int valueRank, UInt16 numVariables)
        {
            return CreateVariables(parent, path, name, (uint)dataType, valueRank, numVariables);
        }

        private BaseDataVariableState[] CreateVariables(NodeState parent, string path, string name, NodeId dataType, int valueRank, UInt16 numVariables)
        {
            // first, create a new Parent folder for this data-type
            FolderState newParentFolder = CreateFolder(parent, path, name);

            List<BaseDataVariableState> itemsCreated = new List<BaseDataVariableState>();
            // now to create the remaining NUMBERED items
            for (uint i = 0; i < numVariables; i++)
            {
                string newName = string.Format("{0}_{1}", name, i.ToString("00"));
                string newPath = string.Format("{0}_{1}", path, newName);
                itemsCreated.Add(CreateVariable(newParentFolder, newPath, newName, dataType, valueRank));
            }
            return (itemsCreated.ToArray());
        }

        /// <summary>
        /// Creates a new variable.
        /// </summary>
        private BaseDataVariableState CreateDynamicVariable(NodeState parent, string path, string name, BuiltInType dataType, int valueRank)
        {
            return CreateDynamicVariable(parent, path, name, (uint)dataType, valueRank);
        }

        /// <summary>
        /// Creates a new variable.
        /// </summary>
        private BaseDataVariableState CreateDynamicVariable(NodeState parent, string path, string name, NodeId dataType, int valueRank)
        {
            BaseDataVariableState variable = CreateVariable(parent, path, name, dataType, valueRank);
            m_dynamicNodes.Add(variable);
            return variable;
        }

        private BaseDataVariableState[] CreateDynamicVariables(NodeState parent, string path, string name, BuiltInType dataType, int valueRank, uint numVariables)
        {
            return CreateDynamicVariables(parent, path, name, (uint)dataType, valueRank, numVariables);

        }

        private BaseDataVariableState[] CreateDynamicVariables(NodeState parent, string path, string name, NodeId dataType, int valueRank, uint numVariables)
        {
            // first, create a new Parent folder for this data-type
            FolderState newParentFolder = CreateFolder(parent, path, name);

            List<BaseDataVariableState> itemsCreated = new List<BaseDataVariableState>();
            // now to create the remaining NUMBERED items
            for (uint i = 0; i < numVariables; i++)
            {
                string newName = string.Format("{0}_{1}", name, i.ToString("00"));
                string newPath = string.Format("{0}_{1}", path, newName);
                itemsCreated.Add(CreateDynamicVariable(newParentFolder, newPath, newName, dataType, valueRank));
            }//for i
            return (itemsCreated.ToArray());
        }

        /// <summary>
        /// Creates a new variable type.
        /// </summary>
        private BaseVariableTypeState CreateVariableType(NodeState parent, IDictionary<NodeId, IList<IReference>> externalReferences, string path, string name, BuiltInType dataType, int valueRank)
        {
            BaseDataVariableTypeState type = new BaseDataVariableTypeState();

            type.SymbolicName = name;
            type.SuperTypeId = VariableTypeIds.BaseDataVariableType;
            type.NodeId = new NodeId(path, NamespaceIndex);
            type.BrowseName = new QualifiedName(name, NamespaceIndex);
            type.DisplayName = type.BrowseName.Name;
            type.WriteMask = AttributeWriteMask.None;
            type.UserWriteMask = AttributeWriteMask.None;
            type.IsAbstract = false;
            type.DataType = (uint)dataType;
            type.ValueRank = valueRank;
            type.Value = null;

            IList<IReference> references = null;

            if (!externalReferences.TryGetValue(VariableTypeIds.BaseDataVariableType, out references))
            {
                externalReferences[VariableTypeIds.BaseDataVariableType] = references = new List<IReference>();
            }

            references.Add(new NodeStateReference(ReferenceTypes.HasSubtype, false, type.NodeId));

            if (parent != null)
            {
                parent.AddReference(ReferenceTypes.Organizes, false, type.NodeId);
                type.AddReference(ReferenceTypes.Organizes, true, parent.NodeId);
            }

            AddPredefinedNode(SystemContext, type);
            return type;
        }

        /// <summary>
        /// Creates a new data type.
        /// </summary>
        private DataTypeState CreateDataType(NodeState parent, IDictionary<NodeId, IList<IReference>> externalReferences, string path, string name)
        {
            DataTypeState type = new DataTypeState();

            type.SymbolicName = name;
            type.SuperTypeId = DataTypeIds.Structure;
            type.NodeId = new NodeId(path, NamespaceIndex);
            type.BrowseName = new QualifiedName(name, NamespaceIndex);
            type.DisplayName = type.BrowseName.Name;
            type.WriteMask = AttributeWriteMask.None;
            type.UserWriteMask = AttributeWriteMask.None;
            type.IsAbstract = false;

            IList<IReference> references = null;

            if (!externalReferences.TryGetValue(DataTypeIds.Structure, out references))
            {
                externalReferences[DataTypeIds.Structure] = references = new List<IReference>();
            }

            references.Add(new NodeStateReference(ReferenceTypeIds.HasSubtype, false, type.NodeId));

            if (parent != null)
            {
                parent.AddReference(ReferenceTypes.Organizes, false, type.NodeId);
                type.AddReference(ReferenceTypes.Organizes, true, parent.NodeId);
            }

            AddPredefinedNode(SystemContext, type);
            return type;
        }

        /// <summary>
        /// Creates a new reference type.
        /// </summary>
        private ReferenceTypeState CreateReferenceType(NodeState parent, IDictionary<NodeId, IList<IReference>> externalReferences, string path, string name)
        {
            ReferenceTypeState type = new ReferenceTypeState();

            type.SymbolicName = name;
            type.SuperTypeId = ReferenceTypeIds.NonHierarchicalReferences;
            type.NodeId = new NodeId(path, NamespaceIndex);
            type.BrowseName = new QualifiedName(name, NamespaceIndex);
            type.DisplayName = type.BrowseName.Name;
            type.WriteMask = AttributeWriteMask.None;
            type.UserWriteMask = AttributeWriteMask.None;
            type.IsAbstract = false;
            type.Symmetric = true;
            type.InverseName = name;

            IList<IReference> references = null;

            if (!externalReferences.TryGetValue(ReferenceTypeIds.NonHierarchicalReferences, out references))
            {
                externalReferences[ReferenceTypeIds.NonHierarchicalReferences] = references = new List<IReference>();
            }

            references.Add(new NodeStateReference(ReferenceTypeIds.HasSubtype, false, type.NodeId));

            if (parent != null)
            {
                parent.AddReference(ReferenceTypes.Organizes, false, type.NodeId);
                type.AddReference(ReferenceTypes.Organizes, true, parent.NodeId);
            }

            AddPredefinedNode(SystemContext, type);
            return type;
        }

        /// <summary>
        /// Creates a new view.
        /// </summary>
        private ViewState CreateView(NodeState parent, IDictionary<NodeId, IList<IReference>> externalReferences, string path, string name)
        {
            ViewState type = new ViewState();

            type.SymbolicName = name;
            type.NodeId = new NodeId(path, NamespaceIndex);
            type.BrowseName = new QualifiedName(name, NamespaceIndex);
            type.DisplayName = type.BrowseName.Name;
            type.WriteMask = AttributeWriteMask.None;
            type.UserWriteMask = AttributeWriteMask.None;
            type.ContainsNoLoops = true;

            IList<IReference> references = null;

            if (!externalReferences.TryGetValue(ObjectIds.ViewsFolder, out references))
            {
                externalReferences[ObjectIds.ViewsFolder] = references = new List<IReference>();
            }

            type.AddReference(ReferenceTypeIds.Organizes, true, ObjectIds.ViewsFolder);
            references.Add(new NodeStateReference(ReferenceTypeIds.Organizes, false, type.NodeId));

            if (parent != null)
            {
                parent.AddReference(ReferenceTypes.Organizes, false, type.NodeId);
                type.AddReference(ReferenceTypes.Organizes, true, parent.NodeId);
            }

            AddPredefinedNode(SystemContext, type);
            return type;
        }

        /// <summary>
        /// Creates a new method.
        /// </summary>
        private MethodState CreateMethod(NodeState parent, string path, string name)
        {
            MethodState method = new MethodState(parent);

            method.SymbolicName = name;
            method.ReferenceTypeId = ReferenceTypeIds.HasComponent;
            method.NodeId = new NodeId(path, NamespaceIndex);
            method.BrowseName = new QualifiedName(path, NamespaceIndex);
            method.DisplayName = new LocalizedText("en", name);
            method.WriteMask = AttributeWriteMask.None;
            method.UserWriteMask = AttributeWriteMask.None;
            method.Executable = true;
            method.UserExecutable = true;

            if (parent != null)
            {
                parent.AddChild(method);
            }

            return method;
        }

        private ServiceResult OnVoidCall(
            ISystemContext context,
            MethodState method,
            IList<object> inputArguments,
            IList<object> outputArguments)
        {
            return ServiceResult.Good;
        }

        private ServiceResult OnAddCall(
            ISystemContext context,
            MethodState method,
            IList<object> inputArguments,
            IList<object> outputArguments)
        {

            // all arguments must be provided.
            if (inputArguments.Count < 2)
            {
                return StatusCodes.BadArgumentsMissing;
            }

            try
            {
                float floatValue = (float)inputArguments[0];
                UInt32 uintValue = (UInt32)inputArguments[1];

                // set output parameter
                outputArguments[0] = (float)(floatValue + uintValue);
                return ServiceResult.Good;
            }
            catch
            {
                return new ServiceResult(StatusCodes.BadInvalidArgument);
            }
        }

        private ServiceResult OnMultiplyCall(
            ISystemContext context,
            MethodState method,
            IList<object> inputArguments,
            IList<object> outputArguments)
        {

            // all arguments must be provided.
            if (inputArguments.Count < 2)
            {
                return StatusCodes.BadArgumentsMissing;
            }

            try
            {
                Int16 op1 = (Int16)inputArguments[0];
                UInt16 op2 = (UInt16)inputArguments[1];

                // set output parameter
                outputArguments[0] = (Int32)(op1 * op2);
                return ServiceResult.Good;
            }
            catch
            {
                return new ServiceResult(StatusCodes.BadInvalidArgument);
            }
        }

        private ServiceResult OnDivideCall(
            ISystemContext context,
            MethodState method,
            IList<object> inputArguments,
            IList<object> outputArguments)
        {

            // all arguments must be provided.
            if (inputArguments.Count < 2)
            {
                return StatusCodes.BadArgumentsMissing;
            }

            try
            {
                Int32 op1 = (Int32)inputArguments[0];
                UInt16 op2 = (UInt16)inputArguments[1];

                // set output parameter
                outputArguments[0] = (float)((float)op1 / (float)op2);
                return ServiceResult.Good;
            }
            catch
            {
                return new ServiceResult(StatusCodes.BadInvalidArgument);
            }
        }

        private ServiceResult OnSubstractCall(
            ISystemContext context,
            MethodState method,
            IList<object> inputArguments,
            IList<object> outputArguments)
        {

            // all arguments must be provided.
            if (inputArguments.Count < 2)
            {
                return StatusCodes.BadArgumentsMissing;
            }

            try
            {
                Int16 op1 = (Int16)inputArguments[0];
                Byte op2 = (Byte)inputArguments[1];

                // set output parameter
                outputArguments[0] = (Int16)(op1 - op2);
                return ServiceResult.Good;
            }
            catch
            {
                return new ServiceResult(StatusCodes.BadInvalidArgument);
            }
        }

        private ServiceResult OnHelloCall(
            ISystemContext context,
            MethodState method,
            IList<object> inputArguments,
            IList<object> outputArguments)
        {

            // all arguments must be provided.
            if (inputArguments.Count < 1)
            {
                return StatusCodes.BadArgumentsMissing;
            }

            try
            {
                string op1 = (string)inputArguments[0];

                // set output parameter
                outputArguments[0] = (string)("hello " + op1);
                return ServiceResult.Good;
            }
            catch
            {
                return new ServiceResult(StatusCodes.BadInvalidArgument);
            }
        }

        private ServiceResult OnInputCall(
            ISystemContext context,
            MethodState method,
            IList<object> inputArguments,
            IList<object> outputArguments)
        {

            // all arguments must be provided.
            if (inputArguments.Count < 1)
            {
                return StatusCodes.BadArgumentsMissing;
            }

            return ServiceResult.Good;
        }

        private ServiceResult OnOutputCall(
            ISystemContext context,
            MethodState method,
            IList<object> inputArguments,
            IList<object> outputArguments)
        {
            // all arguments must be provided.
            try
            {
                // set output parameter
                outputArguments[0] = (string)("Output");
                return ServiceResult.Good;
            }
            catch
            {
                return new ServiceResult(StatusCodes.BadInvalidArgument);
            }
        }

        private object GetNewValue(BaseVariableState variable)
        {
            if (m_generator == null)
            {
                m_generator = new Opc.Ua.Test.DataGenerator(null);
                m_generator.BoundaryValueFrequency = 0;
            }

            object value = null;
            int retryCount = 0;
            string channelName = variable.Parent.GetHierarchyRoot().DisplayName.Text;
            string DriverName = variable.Parent.DisplayName.Text;
            string tagName = variable.DisplayName.Text;

            while (value == null && retryCount < 10)
            {
                if (m_deviceManager.DeviceDic[channelName].ContainsKey(DriverName))
                {
                    if (m_deviceManager.DeviceDic[channelName][DriverName].TagInfoDic.ContainsKey(tagName))
                    {
                        uint.TryParse(m_deviceManager.DeviceDic[channelName][DriverName].TagInfoDic[tagName].ISize.ToString(), out uint size);
                        value = new int[size];

                        switch (m_deviceManager.DeviceDic[channelName][DriverName].TagInfoDic[tagName].TType)
                        {
                            case Type t when t == typeof(bool):
                                if (size > 1)
                                {
                                    value = new bool[size];
                                }
                                else
                                {
                                    bool bValue = false;
                                    value = bValue;
                                }
                                break;
                            case Type t1 when t1 == typeof(string):
                                if (size > 1)
                                {
                                    value = new string[size];
                                }
                                else
                                {
                                    string strValue = "";
                                    value = strValue;
                                }
                                break;
                            case Type t when t == typeof(short):
                                if (size > 1)
                                {
                                    value = new short[size];
                                }
                                else
                                {
                                    short sValue = 0;
                                    value = sValue;
                                }
                                break;
                            case Type t when t == typeof(int):
                                if (size > 1)
                                {
                                    value = new int[size];
                                }
                                else
                                {
                                    int iValue = 0;
                                    value = iValue;
                                }
                                break;
                            case Type t when t == typeof(long):
                                if (size > 1)
                                {
                                    value = new long[size];
                                }
                                else
                                {
                                    long lValue = 0;
                                    value = lValue;
                                }
                                break;
                            case Type t when t == typeof(float):
                                if (size > 1)
                                {
                                    value = new float[size];
                                }
                                else
                                {
                                    float fValue = 0f;
                                    value = fValue;
                                }
                                break;
                            case Type t when t == typeof(double):
                                if (size > 1)
                                {
                                    value = new double[size];
                                }
                                else
                                {
                                    double dValue = 0;
                                    value = dValue;
                                }
                                break;
                            case Type t when t == typeof(ushort):
                                if (size > 1)
                                {
                                    value = new ushort[size];
                                }
                                else
                                {
                                    ushort usValue = 0;
                                    value = usValue;
                                }
                                break;
                            case Type t when t == typeof(uint):
                                if (size > 1)
                                {
                                    value = new uint[size];
                                }
                                else
                                {
                                    uint uiValue = 0;
                                    value = uiValue;
                                }
                                break;
                            case Type t when t == typeof(ulong):
                                if (size > 1)
                                {
                                    value = new ulong[size];
                                }
                                else
                                {
                                    ulong ulValue = 0;
                                    value = ulValue;
                                }
                                break;
                        }
                    }
                    else
                    {
                        value = 0;
                    }
                }
                else
                {
                    value = m_generator.GetRandom(variable.DataType, variable.ValueRank, new uint[] { 10 }, Server.TypeTree);
                }

                retryCount++;
            }

            return value;
        }

        private void DoSimulation(object state)
        {
            try
            {
                lock (Lock)
                {
                    foreach (BaseDataVariableState variable in m_dynamicNodes)
                    {
                        variable.Value = GetNewValue(variable);
                        variable.Timestamp = DateTime.UtcNow;
                        variable.ClearChangeMasks(SystemContext, false);
                    }
                }
            }
            catch (Exception e)
            {
                Utils.Trace(e, "Unexpected error doing simulation.");
            }
        }

        /// <summary>
        /// Frees any resources allocated for the address space.
        /// </summary>
        public override void DeleteAddressSpace()
        {
            lock (Lock)
            {
                // TBD
            }
        }

        /// <summary>
        /// Returns a unique handle for the node.
        /// </summary>
        protected override NodeHandle GetManagerHandle(ServerSystemContext context, NodeId nodeId, IDictionary<NodeId, NodeState> cache)
        {
            lock (Lock)
            {
                // quickly exclude nodes that are not in the namespace. 
                if (!IsNodeIdInNamespace(nodeId))
                {
                    return null;
                }

                NodeState node = null;

                if (!PredefinedNodes.TryGetValue(nodeId, out node))
                {
                    return null;
                }

                NodeHandle handle = new NodeHandle();

                handle.NodeId = nodeId;
                handle.Node = node;
                handle.Validated = true;

                return handle;
            }
        }

        /// <summary>
        /// Verifies that the specified node exists.
        /// </summary>
        protected override NodeState ValidateNode(
           ServerSystemContext context,
           NodeHandle handle,
           IDictionary<NodeId, NodeState> cache)
        {
            // not valid if no root.
            if (handle == null)
            {
                return null;
            }

            // check if previously validated.
            if (handle.Validated)
            {
                return handle.Node;
            }

            // TBD

            return null;
        }
        #endregion

        #region Overrides
        protected override void OnMonitoredItemCreated(ServerSystemContext context, NodeHandle handle, MonitoredItem monitoredItem)
        {
            base.OnMonitoredItemCreated(context, handle, monitoredItem);

            // CreateMonitoredItemsComplete
            string[] strSplit = handle.NodeId.Identifier.ToString().Split('.');
            string channelName = strSplit[0];
            string driverName = strSplit[1];
            string tagName = "";
            StringBuilder sb = new StringBuilder();
            for (int i = 2; i < strSplit.Length; i++)
            {
                sb.Append(strSplit[i]);
                sb.Append('.');
            }
            tagName = sb.ToString().TrimEnd('.');

            if(tagName.Split('.').Length == 2)
            {
                tagName = tagName.Split('.')[1];
            }

            uint scanTime = Convert.ToUInt32(monitoredItem.SamplingInterval);
            string device = channelName + "." + driverName;
            bool bExistence = true;

            //handle.Node.OnStateChanged = OnMonitoredNodeChanged;
            
            if (!_information[device].MonitoredItems.ContainsKey(device))
            {
                _information[device].MonitoredItems[device] = new ConcurrentDictionary<string, List<MonitoredItem>>() { [tagName] = new List<MonitoredItem>() };
            }
            else if (!_information[device].MonitoredItems[device].ContainsKey(tagName))
            {
                bExistence = false;
                _information[device].MonitoredItems[device][tagName] = new List<MonitoredItem>();
            }
            _information[device].MonitoredItems[device][tagName].Add(monitoredItem);

            if (m_deviceManager.DeviceInfoDic[channelName][driverName].TagDeviceInfo.UScanMode != 1)
            {
                if (!_information[device].AgentDic.ContainsKey(device))
                {
                    _information[device].AgentDic[device] = new ConcurrentDictionary<double, List<string>>() { [scanTime] = new List<string>() { tagName } };
                    _information[device].AgentFlag[device] = new ConcurrentDictionary<double, bool>() { [scanTime] = true };
                    _information[device].AgentTasks[device] = new ConcurrentDictionary<double, Task>()
                    {
                        [scanTime] = Task.Factory.StartNew(() => ReadUAStatus(channelName, driverName, scanTime, _information[device].AgentFlag[device]))
                    };
                }
                else if (!bExistence)
                {
                    if (!_information[device].AgentDic[device].ContainsKey(scanTime))
                    {
                        _information[device].AgentDic[device][scanTime] = new List<string>() { tagName };
                        _information[device].AgentFlag[device][scanTime] = true;
                        _information[device].AgentTasks[device][scanTime] = Task.Factory.StartNew(() => ReadUAStatus(channelName, driverName, scanTime, _information[device].AgentFlag[device]));
                    }
                    else
                    {
                        if (!_information[device].AgentDic[device][scanTime].Contains(tagName))
                        {
                            _information[device].AgentDic[device][scanTime].Add(tagName);
                        }
                    }
                }
            }
        }
        protected override void OnMonitoredItemModified(ServerSystemContext context, NodeHandle handle, MonitoredItem monitoredItem)
        {
            base.OnMonitoredItemModified(context, handle, monitoredItem);
            
            // ChangeMonitoredItemsComplete
            string[] strSplit = handle.NodeId.Identifier.ToString().Split('.');
            string channelName = strSplit[0];
            string driverName = strSplit[1];
            string tagName = "";
            StringBuilder sb = new StringBuilder();
            for (int i = 2; i < strSplit.Length; i++)
            {
                sb.Append(strSplit[i]);
                sb.Append('.');
            }
            tagName = sb.ToString().TrimEnd('.');

            if (tagName.Split('.').Length == 2)
            {
                tagName = tagName.Split('.')[1];
            }

            uint scanTime = Convert.ToUInt32(monitoredItem.SamplingInterval);
            string device = channelName + "." + driverName;

            #region Modifing
            // Modifing
            try
            {
                if (m_deviceManager.DeviceInfoDic[channelName][driverName].TagDeviceInfo.UScanMode != 1)
                {
                    bool bExistence = false;
                    foreach (var scantimeKey in _information[device].AgentDic[device].Keys)
                    {
                        //remove
                        if (_information[device].AgentDic[device][scantimeKey].IndexOf(tagName) >= 0)
                        {
                            if (scantimeKey > scanTime)
                            {
                                bExistence = true;

                                _information[device].AgentDic[device][scantimeKey].Remove(tagName);

                                if (_information[device].AgentDic[device][scantimeKey].Count == 0)
                                {
                                    _information[device].AgentFlag[device][scantimeKey] = false;
                                    _information[device].AgentTasks[device][scantimeKey].Wait();

                                    bool boolTemp;
                                    Task taskTemp;
                                    while (!_information[device].AgentFlag[device].TryRemove(scantimeKey, out boolTemp)) { }
                                    while (!_information[device].AgentTasks[device].TryRemove(scantimeKey, out taskTemp)) { }

                                    List<string> listTemp;
                                    while (!_information[device].AgentDic[device].TryRemove(scantimeKey, out listTemp)) { }
                                }

                                break;
                            }
                        }
                    }

                    //Add
                    if (bExistence)
                    {
                        // Compare MonitoredItem ScanTime And Register The shortest Tag among MonitoredItem in SyncRead Task
                        double compareScanTime = scanTime;

                        foreach (var monitorItem in _information[device].MonitoredItems[device][tagName])
                        {
                            if(compareScanTime > monitorItem.SamplingInterval)
                            {
                                compareScanTime = monitoredItem.SamplingInterval;
                            }
                        }

                        if (!_information[device].AgentTasks[device].ContainsKey(compareScanTime))
                        {
                            _information[device].AgentDic[device][compareScanTime] = new List<string>() { tagName };
                            _information[device].AgentFlag[device][compareScanTime] = true;
                            _information[device].AgentTasks[device][compareScanTime] = Task.Factory.StartNew(() => ReadUAStatus(channelName, driverName, (uint)compareScanTime, _information[device].AgentFlag[device]));
                        }
                        else
                        {
                            _information[device].AgentDic[device][compareScanTime].Add(tagName);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Modify Error : " + ex.Message);
            }
            #endregion
        }
        protected override void OnMonitoredItemDeleted(ServerSystemContext context, NodeHandle handle, MonitoredItem monitoredItem)
        {
            base.OnMonitoredItemDeleted(context, handle, monitoredItem);

            // DeleteMonitoredItemsComplete
            string[] strSplit = handle.NodeId.Identifier.ToString().Split('.');
            string channelName = strSplit[0];
            string driverName = strSplit[1];
            string tagName = "";
            StringBuilder sb = new StringBuilder();
            for (int i = 2; i < strSplit.Length; i++)
            {
                sb.Append(strSplit[i]);
                sb.Append('.');
            }
            tagName = sb.ToString().TrimEnd('.');

            if (tagName.Split('.').Length == 2)
            {
                tagName = tagName.Split('.')[1];
            }

            uint scanTime = Convert.ToUInt32(monitoredItem.SamplingInterval);
            string device = channelName + "." + driverName;
            double modifyScanTime = 0;
            //handle.Node.OnStateChanged = null;

            if (_information[device].MonitoredItems.ContainsKey(device))
            {
                if (_information[device].MonitoredItems[device].ContainsKey(tagName))
                {
                    _information[device].MonitoredItems[device][tagName].Remove(monitoredItem);
                    if (_information[device].MonitoredItems[device][tagName].Count == 0)
                    {
                        List<MonitoredItem> temp;
                        while (!_information[device].MonitoredItems[device].TryRemove(tagName, out temp)) { }
                    }
                    else
                    {
                        if (_information[device].AgentDic.ContainsKey(device))
                        {
                            if (_information[device].AgentDic[device].ContainsKey(scanTime))
                            {
                                if (_information[device].AgentDic[device][scanTime].Contains(tagName))
                                {
                                    modifyScanTime = _information[device].MonitoredItems[device][tagName][0].SamplingInterval;
                                    for (int i = 1; i < _information[device].MonitoredItems[device][tagName].Count; i++)
                                    {
                                        if (modifyScanTime > _information[device].MonitoredItems[device][tagName][i].SamplingInterval)
                                        {
                                            modifyScanTime = _information[device].MonitoredItems[device][tagName][i].SamplingInterval;
                                        }
                                    }
                                }
                            } 
                        }
                    }
                }
            }

            if (m_deviceManager.DeviceInfoDic[channelName][driverName].TagDeviceInfo.UScanMode != 1)
            {
                if (_information[device].AgentDic.ContainsKey(device))
                {
                    if (_information[device].AgentDic[device].ContainsKey(scanTime))
                    {
                        if(modifyScanTime > scanTime || modifyScanTime == 0)
                        {
                            _information[device].AgentDic[device][scanTime].Remove(tagName);

                            if (modifyScanTime != 0)
                            {
                                if (!_information[device].AgentTasks[device].ContainsKey(modifyScanTime))
                                {
                                    _information[device].AgentDic[device][modifyScanTime] = new List<string>() { tagName };
                                    _information[device].AgentFlag[device][modifyScanTime] = true;
                                    _information[device].AgentTasks[device][modifyScanTime] = Task.Factory.StartNew(() => ReadUAStatus(channelName, driverName, (uint)modifyScanTime, _information[device].AgentFlag[device]));
                                }
                                else
                                {
                                    _information[device].AgentDic[device][modifyScanTime].Add(tagName);
                                } 
                            }
                        }

                        if (_information[device].AgentDic[device][scanTime].Count == 0)
                        {
                            _information[device].AgentFlag[device][scanTime] = false;
                            _information[device].AgentTasks[device][scanTime].Dispose();

                            Task taskTemp;
                            bool boolTemp;
                            while (!_information[device].AgentTasks[device].TryRemove(scanTime, out taskTemp)) { }
                            while (!_information[device].AgentFlag[device].TryRemove(scanTime, out boolTemp)) { }

                            List<string> listTemp;
                            while (!_information[device].AgentDic[device].TryRemove(scanTime, out listTemp)) { }
                        }
                    }
                }
            }
            
        }
        #endregion

        #region Agent / Delegate
        #endregion

        #region ReadValue
        private void GetValues(object state, object chName, object drName) // Stack Value Get
        {
            //lock (this)
            {
                if (drName is string && state is List<string>)
                {
                    string driverName = (string)drName;
                    string channelName = (string)chName;
                    string[] tags;

                    bool result = false;
                    object[] datas = new object[1];

                    try
                    {
                        string[] strTemp = new string[((List<string>)state).Count];
                        ((List<string>)state).CopyTo(strTemp);
                        List<string> connect = strTemp.ToList();

                        foreach (string tagName in (List<string>)state)
                        {
                            if (m_deviceManager.DeviceDic[channelName][driverName].TagInfoDic[tagName].StrMemory == "")
                            {
                                connect.Remove(tagName);
                            }
                            else if (tagName == connectionTag)
                            {
                                connect.Remove(connectionTag);
                            }
                        }

                        tags = new string[connect.Count];
                        connect.CopyTo(tags);

                        datas = new object[tags.Length];
                    }
                    catch (Exception e)
                    {
                        LogManager.Manager.WriteLog("Exception", e.Message);
                        return;
                    }

                    if (!m_deviceManager.DeviceInfoDic[channelName][driverName].TagDeviceInfo.BReal) //Simulation
                    {
                        return;
                    }

                    if (tags.Length > 0)
                    {
                        try
                        {
                            if (tags.Length > 1)
                            {
                                result = m_deviceManager.DeviceDic[channelName][driverName].ReadTags(tags.ToArray(), ref datas);
                            }
                            else
                            {
                                result = m_deviceManager.DeviceDic[channelName][driverName].ReadTag(tags[0], ref datas[0]);
                            }

                            if (result)
                            {
                                for (int i = 0; i < tags.Length; i++)
                                {
                                    if (datas[i] is byte[] a)
                                    {
                                        _tagIOInfo taginfo = m_deviceManager.DeviceDic[channelName][driverName].TagInfoDic[tags[i]];
                                        try
                                        {
                                            DriverInterface.Definition.GetTypeValue(m_deviceManager.DeviceInfoDic[channelName][driverName].TagDeviceInfo.BSwap, taginfo.ISize, taginfo.TType, ref datas[i]);
                                            _information[channelName + "." + driverName].Variables[tags[i]].Value = datas[i];
                                            _information[channelName + "." + driverName].Variables[tags[i]].StatusCode = new StatusCode(good);
                                        }
                                        catch (Exception)
                                        {
                                            _information[channelName + "." + driverName].Variables[tags[i]].StatusCode = new StatusCode(uncertain);
                                        }
                                        finally
                                        {
                                            _information[channelName + "." + driverName].Variables[tags[i]].Timestamp = DateTime.UtcNow;
                                            _information[channelName + "." + driverName].Variables[tags[i]].ClearChangeMasks(SystemContext, false);
                                        }
                                    }
                                    
                                }
                            }
                            else
                            {
                                for (int i = 0; i < tags.Length; i++)
                                {
                                    _information[channelName + "." + driverName].Variables[tags[i]].StatusCode = new StatusCode(uncertain);
                                    _information[channelName + "." + driverName].Variables[tags[i]].Timestamp = DateTime.UtcNow;
                                    _information[channelName + "." + driverName].Variables[tags[i]].ClearChangeMasks(SystemContext, false);
                                }

                            }
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine(ex.Message);
                            IronUtilites.LogManager.Manager.WriteLog("Exception", "Tag Read Exception : " + ex.Message + ",,"+tags.Length);
                            return;
                        }
                    }
                }
            }
        }

        // Client Sync Read In Func
        public override void Read(OperationContext context, double maxAge, IList<ReadValueId> nodesToRead, IList<DataValue> values, IList<ServiceResult> errors)
        {
            if(nodesToRead[0].NodeId.NamespaceIndex == 2)
            {
                Dictionary<string, List<string>> tagDic = new Dictionary<string, List<string>>();

                foreach (ReadValueId node in nodesToRead)
                {
                    if(node.NodeId.NamespaceIndex == 2)
                    {
                        string[] strSplit = node.NodeId.Identifier.ToString().Split('.');
                        
                        if(strSplit.Length <= 2)
                        {
                            continue;
                        }

                        string channelName = strSplit[0];
                        string driverName = strSplit[1];
                        string tagName = "";
                        StringBuilder sb = new StringBuilder();
                        for (int i = 2; i < strSplit.Length; i++)
                        {
                            sb.Append(strSplit[i]);
                            sb.Append('.');
                        }
                        tagName = sb.ToString().TrimEnd('.');


                        if (m_deviceManager.DeviceInfoDic[channelName][driverName].TagDeviceInfo.BReal && m_deviceManager.DeviceInfoDic[channelName][driverName].TagDeviceInfo.UScanMode != 1)
                        {
                            if(!tagDic.ContainsKey(channelName + "." + driverName))
                            {
                                tagDic[channelName + "." + driverName] = new List<string>() { tagName };
                            }
                            else
                            {
                                if(!tagDic[channelName + "." + driverName].Contains(tagName))
                                {
                                    tagDic[channelName + "." + driverName].Add(tagName);
                                }
                            }
                        }
                    }
                }

                try
                {
                    bool[] readComplete = new bool[tagDic.Count];
                    int count = 0;
                    
                    foreach (string driver in tagDic.Keys)
                    {
                        string[] strSplit = driver.Split('.');
                        string channelName = strSplit[0];
                        string driverName = strSplit[1];

                        if (tagDic[driver].Contains("_Staticstics"))
                        {
                            tagDic.Remove("_Staticstics");
                        }

                        Task.Factory.StartNew(() => ReadDriver(tagDic[driver], channelName, driverName, ref readComplete[count++]));
                    }

                    while (readComplete.Contains(false) && readComplete.Length != 0){}
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                    IronUtilites.LogManager.Manager.WriteLog("Exception", "Tag overrid Read Exception : " + ex.Message);
                }
            }

            base.Read(context, maxAge, nodesToRead, values, errors);
        }

        private void ReadDriver(List<string> tags, string channel, string device, ref bool complete)
        {
            if(tags.Contains(connectionTag))
            {
                bool status = m_deviceManager.DeviceDic[channel][device].CommunicationStatus();
                _information[channel + "." + device].Variables[connectionTag].Value = status;
                _information[channel + "." + device].Variables[connectionTag].Timestamp = DateTime.UtcNow;
                _information[channel + "." + device].Variables[connectionTag].ClearChangeMasks(SystemContext, false);
            }

            GetValues(tags, channel, device);

            complete = true;
        }

        // UAExpert MonitoredItem in Func (Event - Subscribe?)
        #region OnMonitoredNodeChanged = Subscribe
        //public void OnMonitoredNodeChanged(ISystemContext context, NodeState node, NodeStateChangeMasks changes)
        //{
        //    lock (Lock)
        //    {
        //        string[] strSplit = node.NodeId.Identifier.ToString().Split('.');
        //        string channelName = strSplit[0];
        //        string driverName = strSplit[1];
        //        string tagName = "";
        //        StringBuilder sb = new StringBuilder();
        //        for (int i = 2; i < strSplit.Length; i++)
        //        {
        //            sb.Append(strSplit[i]);
        //            sb.Append('.');
        //        }
        //        tagName = sb.ToString().TrimEnd('.');

        //        // UA Agent(MonitoredItem)
        //        if (!_information[channelName + "." + driverName].MonitoredItems.ContainsKey(tagName))
        //        {
        //            return;
        //        }

        //        switch (m_deviceManager.DeviceInfoDic[channelName][driverName].TagDeviceInfo.UScanMode)
        //        {
        //            case 1:
        //                // Polling --> Request Data = Server Stack Data
        //                break;
        //            case 2:
        //                // Request Data no faster than x : Sync Version
        //                TimeSpan s = DateTime.UtcNow - _information[channelName + "." + driverName].Variables[tagName].Timestamp;

        //                if (s.TotalMilliseconds < m_deviceManager.DeviceInfoDic[channelName][driverName].TagDeviceInfo.UScanRate)
        //                {
        //                    return;
        //                }
        //                GetValues(new List<string>() { tagName }, channelName, driverName);
        //                break;
        //            case 3:
        //                // Sync PLC Data Read
        //                GetValues(new List<string>() { tagName }, channelName, driverName);
        //                break;
        //            default:
        //                break;
        //        }

        //        MonitoredItem monitoredItem = _information[channelName + "." + driverName].MonitoredItems[channelName+"."+driverName][tagName][0];
        //        {
        //            DataValue dataValue = new DataValue();
        //            dataValue.Value = null;
        //            dataValue.ServerTimestamp = DateTime.UtcNow;
        //            dataValue.SourceTimestamp = DateTime.MinValue;
        //            dataValue.StatusCode = StatusCodes.Good;

        //            ServiceResult error = node.ReadAttribute(context, monitoredItem.AttributeId, monitoredItem.IndexRange, monitoredItem.DataEncoding, dataValue);

        //            if (ServiceResult.IsBad(error))
        //            {
        //                dataValue = null;
        //            }

        //            monitoredItem.QueueValue(dataValue, error);
        //        }
        //    }
        //}
        #endregion

        private void ReadUAStatus(string channel, string driver, uint scanTime, ConcurrentDictionary<double, bool> agentFlag)
        {
            lock (_information[channel + "." + driver].AgentFlag)
            {
                while (agentFlag[scanTime])
                {
                    Stopwatch watch = new Stopwatch();
                    watch.Start();

                    bool status = true;

                    if (_information[channel + "." + driver].AgentDic[channel + "." + driver][scanTime].Contains(connectionTag))
                    {
                        if (m_deviceManager.DeviceInfoDic[channel][driver].TagDeviceInfo.BReal)
                        {
                            status = m_deviceManager.DeviceDic[channel][driver].CommunicationStatus();
                        }

                        _information[channel + "." + driver].Variables[connectionTag].Value = status;
                        _information[channel + "." + driver].Variables[connectionTag].Timestamp = DateTime.UtcNow;
                        _information[channel + "." + driver].Variables[connectionTag].ClearChangeMasks(SystemContext, false);
                    }

                    if (status)
                    {
                        GetValues(_information[channel + "." + driver].AgentDic[channel + "." + driver][scanTime], channel, driver);
                    }
                    else
                    {
                        m_deviceManager.DeviceDic[channel][driver].DisConnect();
                        m_deviceManager.DeviceDic[channel][driver].Connect();
                    }



                    double checkTime = (double)watch.ElapsedMilliseconds;
                    int setInterval = Convert.ToInt32(Math.Max(1, scanTime - checkTime));

                    Thread.Sleep(setInterval);
                } 
            }
        }

        #endregion

        #region WriteVale
        public override void Write(OperationContext context, IList<WriteValue> nodesToWrite, IList<ServiceResult> errors)
        {
            lock (Lock)
            {
                try
                {
                    bool bReturn = false;

                    if (nodesToWrite.Count > 1)
                    {
                        Dictionary<string, List<string>> tagDic = new Dictionary<string, List<string>>();
                        Dictionary<string, List<object>> valueDic = new Dictionary<string, List<object>>();

                        string channelName = nodesToWrite[0].NodeId.Identifier.ToString().Split('.')[0];

                        foreach (WriteValue node in nodesToWrite)
                        {
                            string[] strSplit = node.NodeId.Identifier.ToString().Split('.');
                            string tagName = "";
                            StringBuilder sb = new StringBuilder();
                            for (int i = 2; i < strSplit.Length; i++)
                            {
                                sb.Append(strSplit[i]);
                                sb.Append('.');
                            }
                            tagName = sb.ToString().TrimEnd('.');
                            string driverName = strSplit[1];

                            if (m_deviceManager.DeviceInfoDic[channelName][driverName].TagDeviceInfo.BReal)
                            {
                                if (m_deviceManager.DeviceDic[channelName][driverName].TagInfoDic[tagName].StrMemory == "")
                                {
                                    tagDic[driverName].Add(tagName);
                                    valueDic[driverName].Add(node.Value.Value);
                                }
                                else
                                {
                                    _information[channelName + "." + driverName].Variables[tagName].Value = node.Value.Value;
                                    _information[channelName + "." + driverName].Variables[tagName].Timestamp = DateTime.UtcNow;
                                    _information[channelName + "." + driverName].Variables[tagName].ClearChangeMasks(SystemContext, false);
                                }
                            }
                            else
                            {
                                _information[channelName + "." + driverName].Variables[tagName].Value = node.Value.Value;
                                _information[channelName + "." + driverName].Variables[tagName].Timestamp = DateTime.UtcNow;
                                _information[channelName + "." + driverName].Variables[tagName].ClearChangeMasks(SystemContext, false);
                            }
                        }

                        foreach (string driverName in tagDic.Keys)
                        {
                            if (m_deviceManager.DeviceInfoDic[channelName][driverName].TagDeviceInfo.BReal)
                            {
                                bReturn = m_deviceManager.DeviceDic[channelName][driverName].WriteTags(m_deviceManager.DeviceInfoDic[channelName][driverName].TagDeviceInfo.BSwap, tagDic[driverName].ToArray(), valueDic[driverName].ToArray());

                                if (!bReturn)
                                {
                                    IronUtilites.LogManager.Manager.WriteLog("Error", "Tag Write result Return fail");
                                    return;
                                }
                            }
                        }
                    }
                    else
                    {
                        string[] strSplit = nodesToWrite[0].NodeId.Identifier.ToString().Split('.');
                        string channelName = strSplit[0];
                        string driverName = strSplit[1];
                        string tagName = "";
                        StringBuilder sb = new StringBuilder();
                        for (int i = 2; i < strSplit.Length; i++)
                        {
                            sb.Append(strSplit[i]);
                            sb.Append('.');
                        }
                        tagName = sb.ToString().TrimEnd('.');

                        if (m_deviceManager.DeviceInfoDic[channelName][driverName].TagDeviceInfo.BReal)
                        {
                            if (m_deviceManager.DeviceDic[channelName][driverName].TagInfoDic[tagName].StrMemory == "")
                            {
                                _information[channelName + "." + driverName].Variables[tagName].Value = nodesToWrite[0].Value.Value;
                                _information[channelName + "." + driverName].Variables[tagName].Timestamp = DateTime.UtcNow;
                                _information[channelName + "." + driverName].Variables[tagName].ClearChangeMasks(SystemContext, false);
                            }
                            else
                            {
                                bReturn = m_deviceManager.DeviceDic[channelName][driverName].WriteTag(m_deviceManager.DeviceInfoDic[channelName][driverName].TagDeviceInfo.BSwap, tagName, nodesToWrite[0].Value.Value);

                                if (!bReturn)
                                {
                                    IronUtilites.LogManager.Manager.WriteLog("Error", "Tag Write result Return fail");
                                    return;
                                }
                            }
                        }
                        else
                        {
                            _information[channelName + "." + driverName].Variables[tagName].Value = nodesToWrite[0].Value.Value;
                            _information[channelName + "." + driverName].Variables[tagName].Timestamp = DateTime.UtcNow;
                            _information[channelName + "." + driverName].Variables[tagName].ClearChangeMasks(SystemContext, false);
                        }
                    }
                }
                catch (Exception ex)
                {
                    IronUtilites.LogManager.Manager.WriteLog("Exception", "Tag Write Exception : " + ex.Message);
                }
            }

            //base.Write(context, nodesToWrite, errors);
        }
        #endregion

        #region Private Fields
        private IronServerConfiguration m_configuration;
        private Opc.Ua.Test.DataGenerator m_generator;
        private Timer m_simulationTimer;
        private UInt16 m_simulationInterval = 1000;
        private bool m_simulationEnabled = true;
        private List<BaseDataVariableState> m_dynamicNodes;
        private DeviceManager m_deviceManager;
        private Dictionary<string, InformationValue> _information; // key : channelName.driverName, value : agent/variables/ ...
        private Dictionary<string, FolderState> rootFolderDic; // key : driverName, value : rootFolder
        private bool taskFlag = false;
        private List<Task> readTasks;

        private string startPath;

        // UA Status Code
        private const string connectionTag = "_commStatus";
        private const uint bad = 0x80000000;
        private const uint uncertain = 0x40000000;
        private const uint good = 0x00000000;
        #endregion
    }

    public class InformationValue
    {
        private Dictionary<string, BaseDataVariableState> _variables; // key : tagName
        private ConcurrentDictionary<string, ConcurrentDictionary<double, List<string>>> agentDic; // key : channel.driver | key : scanTime , value : tagNames
        private ConcurrentDictionary<string, ConcurrentDictionary<double, bool>> agentFlag; // key : channel.driver | key : scanTime , value : Task_flag
        private ConcurrentDictionary<string, ConcurrentDictionary<double, Task>> agentTasks; // key : channel.driver | key : scanTime , value : Task
        private ConcurrentDictionary<string, ConcurrentDictionary<string, List<MonitoredItem>>> monitoreditems; // key : channel.driver | key : tagName , value : monitoritem

        public Dictionary<string, BaseDataVariableState> Variables
        {
            get
            {
                if (_variables == null)
                {
                    _variables = new Dictionary<string, BaseDataVariableState>();
                }

                return _variables;
            }
            set => _variables = value;
        }
        public ConcurrentDictionary<string, ConcurrentDictionary<double, List<string>>> AgentDic
        {
            get
            {
                if (agentDic == null)
                {
                    agentDic = new ConcurrentDictionary<string, ConcurrentDictionary<double, List<string>>>();
                }

                return agentDic;
            }
            set => agentDic = value;
        }
        public ConcurrentDictionary<string, ConcurrentDictionary<double, bool>> AgentFlag
{
            get
            {
                if (agentFlag == null)
                {
                    agentFlag = new ConcurrentDictionary<string, ConcurrentDictionary<double, bool>>();
                }

                return agentFlag;
            }
            set => agentFlag = value;
        }
        public ConcurrentDictionary<string, ConcurrentDictionary<double, Task>> AgentTasks
        {
            get
            {
                if (agentTasks == null)
                {
                    agentTasks = new ConcurrentDictionary<string, ConcurrentDictionary<double,Task>>();
                }

                return agentTasks;
            }

            set => agentTasks = value;
        }
        public ConcurrentDictionary<string, ConcurrentDictionary<string, List<MonitoredItem>>> MonitoredItems {
            get
            {
                if (monitoreditems == null)
                {
                    monitoreditems = new ConcurrentDictionary<string, ConcurrentDictionary<string, List<MonitoredItem>>>();
                }

                return monitoreditems;
            }
            set => monitoreditems = value;
        }
    }
}
