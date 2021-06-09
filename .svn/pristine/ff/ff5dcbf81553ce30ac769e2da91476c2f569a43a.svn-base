using System;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using Opc.Ua;
using Opc.Ua.Client;
using Opc.Ua.Configuration;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace IronClient
{
    public partial class IronOPCUAClient
    {
        public bool IsConnected
        {
            get
            {
                if (m_session != null)
                {
                    return m_session.Connected;
                }
                return false;
            }
        }

        public bool IsSubscribe
        {
            get
            {
                
                if (m_subscription != null)
                {
                    return m_subscription.Created;
                }
                return false;
            }
        }

        public int CountMonitoredItem
        {
            get
            {
                if (m_monitoredItem != null)
                {
                    return m_monitoredItem.Count;
                }
                return 0;
            }
        }

        public IronOPCUAClient()
        {
        }

        /// <summary>
        /// Handles a certificate validation error.
        /// </summary>
        private void CertificateValidator_CertificateValidation(CertificateValidator validator, CertificateValidationEventArgs e)
        {
            try
            {
                if(e.Error != null && e.Error.Code == StatusCodes.BadCertificateUntrusted)
                {
                    e.Accept = true;
                    IronUtilites.LogManager.Manager.WriteLog("OPC", (int)Utils.TraceMasks.Security + "Automatically accepted certificate: {" + e.Certificate.Subject + "}");
                }
            }
            catch (Exception except)
            {
                IronUtilites.LogManager.Manager.WriteLog("Exception", except.ToString());
            }
        }

        private void Init()
        {

            application.ApplicationType = ApplicationType.Client;
            application.ConfigSectionName = "Opc.Ua.IronClient";
            // use a custom transport channel
            //WcfChannelBase.g_CustomTransportChannel = new CustomTransportChannelFactory();
                
            application.LoadApplicationConfiguration(false).Wait();

            ChangeCerificatePath(application);

            // check the application certificate.
            bool certOK = application.CheckApplicationInstanceCertificate(false, 0).Result;

            if (!certOK)
            {
                Trace.WriteLine("Application instance certificate invalid!");

                throw new Exception("Application instance certificate invalid!");
            }
            
            m_configuration = application.ApplicationConfiguration;
            m_configuration.CertificateValidator.CertificateValidation += CertificateValidator_CertificateValidation;

            
            m_SessionNotification = new NotificationEventHandler(Session_Notification);
            m_SubscriptionStateChanged = new SubscriptionStateChangedEventHandler(Subscription_StateChanged);
            m_PublishStatusChanged = new EventHandler(Subscription_PublishStatusChanged);



        }

        private static void ChangeCerificatePath(ApplicationInstance application)
        {
            if (!System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows))
            {
                IronUtilites.LogManager.Manager.WriteLog("Config", "Pre : ApplicationCertificate.StorePath : " + application.ApplicationConfiguration.SecurityConfiguration.ApplicationCertificate.StorePath);
                IronUtilites.LogManager.Manager.WriteLog("Config", "Pre : TrustedIssuerCertificates.StorePath : " + application.ApplicationConfiguration.SecurityConfiguration.TrustedIssuerCertificates.StorePath);
                IronUtilites.LogManager.Manager.WriteLog("Config", "Pre : TrustedPeerCertificates.StorePath : " + application.ApplicationConfiguration.SecurityConfiguration.TrustedPeerCertificates.StorePath);
                IronUtilites.LogManager.Manager.WriteLog("Config", "Pre : RejectedCertificateStore.StorePath : " + application.ApplicationConfiguration.SecurityConfiguration.RejectedCertificateStore.StorePath);
                IronUtilites.LogManager.Manager.WriteLog("Config", "Pre : UserIssuerCertificates.StorePath : " + application.ApplicationConfiguration.SecurityConfiguration.UserIssuerCertificates.StorePath);
                IronUtilites.LogManager.Manager.WriteLog("Config", "Pre : TrustedUserCertificates.StorePath : " + application.ApplicationConfiguration.SecurityConfiguration.TrustedUserCertificates.StorePath);

                application.ApplicationConfiguration.SecurityConfiguration.ApplicationCertificate.StorePath = application.ApplicationConfiguration.SecurityConfiguration.ApplicationCertificate.StorePath.Replace("%AppData%", Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData));
                application.ApplicationConfiguration.SecurityConfiguration.TrustedIssuerCertificates.StorePath = application.ApplicationConfiguration.SecurityConfiguration.TrustedIssuerCertificates.StorePath.Replace("%AppData%", Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData));
                application.ApplicationConfiguration.SecurityConfiguration.TrustedPeerCertificates.StorePath = application.ApplicationConfiguration.SecurityConfiguration.TrustedPeerCertificates.StorePath.Replace("%AppData%", Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData));
                application.ApplicationConfiguration.SecurityConfiguration.RejectedCertificateStore.StorePath = application.ApplicationConfiguration.SecurityConfiguration.RejectedCertificateStore.StorePath.Replace("%AppData%", Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData));
                application.ApplicationConfiguration.SecurityConfiguration.UserIssuerCertificates.StorePath = application.ApplicationConfiguration.SecurityConfiguration.UserIssuerCertificates.StorePath.Replace("%AppData%", Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData));
                application.ApplicationConfiguration.SecurityConfiguration.TrustedUserCertificates.StorePath = application.ApplicationConfiguration.SecurityConfiguration.TrustedUserCertificates.StorePath.Replace("%AppData%", Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData));

                application.ApplicationConfiguration.SecurityConfiguration.ApplicationCertificate.StorePath = application.ApplicationConfiguration.SecurityConfiguration.ApplicationCertificate.StorePath.Replace('\\', Path.DirectorySeparatorChar);
                application.ApplicationConfiguration.SecurityConfiguration.TrustedIssuerCertificates.StorePath = application.ApplicationConfiguration.SecurityConfiguration.TrustedIssuerCertificates.StorePath.Replace('\\', Path.DirectorySeparatorChar);
                application.ApplicationConfiguration.SecurityConfiguration.TrustedPeerCertificates.StorePath = application.ApplicationConfiguration.SecurityConfiguration.TrustedPeerCertificates.StorePath.Replace('\\', Path.DirectorySeparatorChar);
                application.ApplicationConfiguration.SecurityConfiguration.RejectedCertificateStore.StorePath = application.ApplicationConfiguration.SecurityConfiguration.RejectedCertificateStore.StorePath.Replace('\\', Path.DirectorySeparatorChar);
                application.ApplicationConfiguration.SecurityConfiguration.UserIssuerCertificates.StorePath = application.ApplicationConfiguration.SecurityConfiguration.UserIssuerCertificates.StorePath.Replace('\\', Path.DirectorySeparatorChar);
                application.ApplicationConfiguration.SecurityConfiguration.TrustedUserCertificates.StorePath = application.ApplicationConfiguration.SecurityConfiguration.TrustedUserCertificates.StorePath.Replace('\\', Path.DirectorySeparatorChar);

                IronUtilites.LogManager.Manager.WriteLog("Config", "Aft : ApplicationCertificate.StorePath : " + application.ApplicationConfiguration.SecurityConfiguration.ApplicationCertificate.StorePath);
                IronUtilites.LogManager.Manager.WriteLog("Config", "Aft : TrustedIssuerCertificates.StorePath : " + application.ApplicationConfiguration.SecurityConfiguration.TrustedIssuerCertificates.StorePath);
                IronUtilites.LogManager.Manager.WriteLog("Config", "Aft : TrustedPeerCertificates.StorePath : " + application.ApplicationConfiguration.SecurityConfiguration.TrustedPeerCertificates.StorePath);
                IronUtilites.LogManager.Manager.WriteLog("Config", "Aft : RejectedCertificateStore.StorePath : " + application.ApplicationConfiguration.SecurityConfiguration.RejectedCertificateStore.StorePath);
                IronUtilites.LogManager.Manager.WriteLog("Config", "Aft : UserIssuerCertificates.StorePath : " + application.ApplicationConfiguration.SecurityConfiguration.UserIssuerCertificates.StorePath);
                IronUtilites.LogManager.Manager.WriteLog("Config", "Aft : TrustedUserCertificates.StorePath : " + application.ApplicationConfiguration.SecurityConfiguration.TrustedUserCertificates.StorePath);
            }
        }

        private void Subscription_PublishStatusChanged(object sender, EventArgs e)
        {
        }

        private void Subscription_StateChanged(Subscription subscription, SubscriptionStateChangedEventArgs e)
        {

        }

        //string[] tags;
        //object[] changeValues;
        //DateTime[] timeStemp;

        private void Session_Notification(Session session, NotificationEventArgs e)
        {

            int eventCount = e.NotificationMessage.GetDataChanges(false).Count;

            if (eventCount > 0)
            {
                string[] tags = new string[eventCount];
                string[] status = new string[eventCount];
                object[] changeValues = new object[eventCount];
                DateTime[] timeStemp = new DateTime[eventCount];
                DateTime[] serverTimeStemp = new DateTime[eventCount];
                string[] dataTypes = new string[eventCount];
                //edit
                int dataIndex = 0;

                foreach (MonitoredItemNotification change in e.NotificationMessage.GetDataChanges(false))
                {
                    try
                    {
                        object valueObj = change.Value.Value;
                        
                        if (m_monitoredIdxTag.TryGetValue(change.ClientHandle, out string tag))
                        {
                            MonitoredTagValue[tag] = valueObj;
                        }
                        
                        //idx = m_monitoredInfo.FindIndex(0, change.ClientHandle.ToString());
                        //m_monitoredInfo.Items[2][idx] = change.Value.Value.ToString();

                        int valueRank = Opc.Ua.TypeInfo.GetValueRank(valueObj);

                        if (valueRank == 2)
                        {
                            if (valueObj is Opc.Ua.Matrix mat)
                            {
                                //mat.TypeInfo.BuiltInType;
                                changeValues[dataIndex] = GetMatrixObjectData(mat);
                            }
                            else
                            {
                                changeValues[dataIndex] = valueObj;
                            }
                        }
                        else
                        {
                            changeValues[dataIndex] = valueObj;
                        }
                        tags[dataIndex] = tag;
                        //clientHandle[dataIndex] = change.ClientHandle;
                        status[dataIndex] = change.Value.StatusCode.ToString();
                        dataTypes[dataIndex] = change.Value.WrappedValue.TypeInfo.BuiltInType.ToString();
                        timeStemp[dataIndex] = change.Value.SourceTimestamp;
                        serverTimeStemp[dataIndex] = change.Value.ServerTimestamp;
                        

                        dataIndex++;

                    }
                    catch (Exception except)
                    {
                        IronUtilites.LogManager.Manager.WriteLog("Exception", MethodBase.GetCurrentMethod() + " : " + except.ToString());
                    }

                }
                ArrayChangeEvent?.Invoke(tags, changeValues, dataTypes, timeStemp, serverTimeStemp, status);
            }

        }

        public async void Connect()
        {
            if (!IsConnected)
            {
                Init();
                m_endpoint = CreateEndPoint(ServerUrl);
                Session session = await Connects();
                if (session != null)
                {
                    // stop any reconnect operation.
                    if (m_reconnectHandler != null)
                    {
                        m_reconnectHandler.Dispose();
                        m_reconnectHandler = null;
                    }

                    m_session = session;
                    m_session.KeepAlive += new KeepAliveEventHandler(StandardClient_KeepAlive);
                    StandardClient_KeepAlive(m_session, null);
                }
            }
            else
            {
                IronUtilites.LogManager.Manager.WriteLog("Else", "Already Connected Session");
            }
        }

        private async Task<Session> Connects()
        {
            if (m_endpoint == null) throw new ArgumentNullException("endpoint");

            EndpointDescriptionCollection availableEndpoints = null;

            // check if the endpoint needs to be updated.
            if (m_endpoint.UpdateBeforeConnect)
            {
                m_endpoint = availableendpoints();

                //이부분 처리
                if (m_endpoint == null)
                {
                    return null;
                }
                availableEndpoints = m_availableEndpoints;
            }

            // copy the message context.
            m_messageContext = m_configuration.CreateMessageContext();


            X509Certificate2 clientCertificate = null;
            X509Certificate2Collection clientCertificateChain = null;

            if (m_endpoint.Description.SecurityPolicyUri != SecurityPolicies.None)
            {
                if (m_configuration.SecurityConfiguration.ApplicationCertificate == null)
                {
                    throw ServiceResultException.Create(StatusCodes.BadConfigurationError, "ApplicationCertificate must be specified.");
                }

                clientCertificate = await m_configuration.SecurityConfiguration.ApplicationCertificate.Find(true);

                if (clientCertificate == null)
                {
                    throw ServiceResultException.Create(StatusCodes.BadConfigurationError, "ApplicationCertificate cannot be found.");
                }

                // load certificate chain
                clientCertificateChain = new X509Certificate2Collection(clientCertificate);
                List<CertificateIdentifier> issuers = new List<CertificateIdentifier>();
                await m_configuration.CertificateValidator.GetIssuers(clientCertificate, issuers);
                for (int i = 0; i < issuers.Count; i++)
                {
                    clientCertificateChain.Add(issuers[i].Certificate);
                }
            }

            // create the channel.
            ITransportChannel channel = SessionChannel.Create(
                m_configuration,
                m_endpoint.Description,
                m_endpoint.Configuration,
                clientCertificate,
                m_configuration.SecurityConfiguration.SendCertificateChain ? clientCertificateChain : null,
                m_messageContext); // m_messageContext

            // create the session.
            return Connect(channel, availableEndpoints);
        }

        void StandardClient_KeepAlive(Session sender, KeepAliveEventArgs e)
        {
            if (e != null && m_session != null)
            {
                if (!ServiceResult.IsGood(e.Status))
                {
                    if (m_reconnectPeriod <= 0)
                    {
                        return;
                    }

                    if (m_reconnectHandler == null && m_reconnectPeriod > 0)
                    {
                        m_reconnectHandler = new SessionReconnectHandler();
                        m_reconnectHandler.BeginReconnect(m_session, m_reconnectPeriod * 1000, StandardClient_Server_ReconnectComplete);
                    }

                }

                if (ServiceResult.IsGood(e.Status))
                {
                    StatusNotificationEvent?.Invoke("Good");
                }
                else if (ServiceResult.IsBad(e.Status))
                {
                    StatusNotificationEvent?.Invoke("Bad");
                }
                else if (ServiceResult.IsNotBad(e.Status))
                {
                    StatusNotificationEvent?.Invoke("NotBad");
                }
                else if (ServiceResult.IsNotGood(e.Status))
                {
                    StatusNotificationEvent?.Invoke("NotGood");
                }
                else if (ServiceResult.IsNotUncertain(e.Status))
                {
                    StatusNotificationEvent?.Invoke("NotUncertain");
                }
                else if (ServiceResult.IsUncertain(e.Status))
                {
                    StatusNotificationEvent?.Invoke("Uncertain");
                }

            }
        }

        private void StandardClient_Server_ReconnectComplete(object sender, EventArgs e)
        {

            try
            {
                // ignore callbacks from discarded objects.
                if (!Object.ReferenceEquals(sender, m_reconnectHandler))
                {
                    return;
                }

                m_session = m_reconnectHandler.Session;
                m_reconnectHandler.Dispose();
                m_reconnectHandler = null;

                //BrowseCTRL.SetView(m_session, BrowseViewType.Objects, null);

                //SessionsCTRL.Reload(m_session);

                StandardClient_KeepAlive(m_session, null);
            }
            catch (Exception except)
            {
                IronUtilites.LogManager.Manager.WriteLog("Exception", MethodBase.GetCurrentMethod() + " : " + except.ToString());
            }
        }

        public Session Connect(ITransportChannel channel, EndpointDescriptionCollection availableEndpoints)
        {
            if (channel == null) throw new ArgumentNullException("channel");

            try
            {
                // create the session.
                Session session = new Session(channel, application.ApplicationConfiguration, m_endpoint, null);
                session.ReturnDiagnostics = DiagnosticsMasks.All;

                IUserIdentity identity = null;

                if (usertokentype == UserTokenType.UserName)
                {
                    if (String.IsNullOrEmpty(username))
                    {
                    }

                    if (!String.IsNullOrEmpty(username) || !String.IsNullOrEmpty(password))
                    {
                        identity = new UserIdentity(username, password);
                    }
                }

                SessionOpen(new object[] { session, sessionId, identity, m_preferredLocales, m_checkDomain });

                if (!session.Connected)
                {
                    SessionOpen(new object[] { session, sessionId, identity, m_preferredLocales, false });
                }

                // session now owns the channel.
                channel = null;

                // delete the existing session.
                //Close();
                if (m_session != null)
                {
                    m_session.Close();
                }
                // return the new session.
                return session;
            }
            finally
            {
                // ensure the channel is closed on error.
                if (channel != null)
                {
                    channel.Close();
                }
            }
        }

        public ConfiguredEndpoint availableendpoints()
        {
            if (m_endpoint == null) throw new ArgumentNullException("endpoint");

            // construct a list of available endpoint descriptions for the application.
            m_availableEndpoints = new EndpointDescriptionCollection();

            m_availableEndpoints.Add(m_endpoint.Description);
            m_currentDescription = m_endpoint.Description;
            m_endpointConfiguration = m_endpoint.Configuration;

            if (m_endpointConfiguration == null)
            {
                m_endpointConfiguration = EndpointConfiguration.Create(m_configuration);
            }

            if (m_endpoint.Collection != null)
            {
                foreach (ConfiguredEndpoint existingEndpoint in m_endpoint.Collection.Endpoints)
                {
                    if (existingEndpoint.Description.Server.ApplicationUri == m_endpoint.Description.Server.ApplicationUri)
                    {
                        m_availableEndpoints.Add(existingEndpoint.Description);
                    }
                }
            }

            //BuildEndpointDescriptionStrings(m_availableEndpoints);

            UserTokenPolicy policy = m_endpoint.SelectedUserTokenPolicy;

            if (policy == null)
            {
                if (m_endpoint.Description.UserIdentityTokens.Count > 0)
                {
                    policy = m_endpoint.Description.UserIdentityTokens[0];
                }
            }

            if (policy != null)
            {
                UserTokenItem userTokenItem = new UserTokenItem(policy);

                if (policy.TokenType == UserTokenType.UserName && m_endpoint.UserIdentity is UserNameIdentityToken)
                {
                    m_userIdentities[userTokenItem.ToString()] = m_endpoint.UserIdentity;
                }

                if (policy.TokenType == UserTokenType.Certificate && m_endpoint.UserIdentity is X509IdentityToken)
                {
                    m_userIdentities[userTokenItem.ToString()] = m_endpoint.UserIdentity;
                }

                if (policy.TokenType == UserTokenType.IssuedToken && m_endpoint.UserIdentity is IssuedIdentityToken)
                {
                    m_userIdentities[userTokenItem.ToString()] = m_endpoint.UserIdentity;
                }
            }

            // copy com identity.
            //m_comIdentity = m_endpoint.ComIdentity;d

            // initializing the protocol will trigger an update to all other controls.
            //InitializeProtocols(m_availableEndpoints);

            // check if the current settings match the defaults.
            EndpointConfiguration defaultConfiguration = EndpointConfiguration.Create(m_configuration);

            // discover endpoints in the background.
            m_discoverCount++;
            //ThreadPool.QueueUserWorkItem(new WaitCallback(OnDiscoverEndpoints), m_endpoint.Description.Server);
            OnDiscoverEndpoints(m_endpoint.Description.Server);

            try
            {
                // check that discover has completed.
                if (!m_discoverySucceeded)
                {
                    Trace.WriteLine("Endpoint information may be out of date because the discovery process has not completed. Continue anyways?");
                }

                EndpointConfiguration configuration = m_endpointConfiguration;

                if (configuration == null)
                {
                    configuration = EndpointConfiguration.Create(m_configuration);
                }

                if (m_currentDescription == null)
                {
                    m_currentDescription = CreateDescriptionFromSelections();
                }

                // the discovery endpoint should always be on the same machine as the server.
                // if there is a mismatch it is likely because the server has multiple addresses
                // and was not configured to return the current address to the client.
                // The code automatically updates the domain in the url. 
                Uri endpointUrl = Utils.ParseUri(m_currentDescription.EndpointUrl);

                if (m_discoverySucceeded)
                {
                    if (!Utils.AreDomainsEqual(endpointUrl, m_discoveryUrl))
                    {
                        UriBuilder url = new UriBuilder(endpointUrl);

                        url.Host = m_discoveryUrl.DnsSafeHost;

                        if (url.Scheme == m_discoveryUrl.Scheme)
                        {
                            url.Port = m_discoveryUrl.Port;
                        }

                        endpointUrl = url.Uri;

                        m_currentDescription.EndpointUrl = endpointUrl.ToString();
                    }
                }

                // set the encoding.
                Encoding encoding = m_encoding;
                configuration.UseBinaryEncoding = encoding != Encoding.Xml;

                if (m_endpoint == null)
                {
                    m_endpoint = new ConfiguredEndpoint(null, m_currentDescription, configuration);
                }
                else
                {
                    m_endpoint.Update(m_currentDescription);
                    m_endpoint.Update(configuration);
                }

            }
            catch (Exception except)
            {
                IronUtilites.LogManager.Manager.WriteLog("Exception", MethodBase.GetCurrentMethod() + " : " + except.ToString());
            }
            
            return m_endpoint;
        }

        private void OnDiscoverEndpoints(object state)
        {
            int discoverCount = m_discoverCount;

            // do nothing if a valid list is not provided.
            ApplicationDescription server = state as ApplicationDescription;

            if (server == null)
            {
                return;

            }
            
            String discoveryMessage = String.Empty;

            // process each url.
            foreach (string discoveryUrl in server.DiscoveryUrls)
            {
                Uri url = Utils.ParseUri(discoveryUrl);

                if (url != null)
                {
                    if (DiscoverEndpoints(url, out discoveryMessage))
                    {
                        m_discoverySucceeded = true;
                        m_discoveryUrl = url;
                        return;
                    }

                    // check if another discover operation has started.
                    if (discoverCount != m_discoverCount)
                    {
                        return;
                    }
                }
            }

            //OnUpdateEndpoints(m_availableEndpoints);
        }

        //public void Close()
        //{

        //    // close all active sessions.
        //    foreach (TreeNode root in NodesTV.Nodes)
        //    {
        //        Session session = root.Tag as Session;

        //        if (session != null)
        //        {
        //            session.Close();
        //        }
        //    }

        //    Clear();
        //}
        private EndpointDescription CreateDescriptionFromSelections()
        {
            Protocol currentProtocol = new Protocol(protocol);

            EndpointDescription endpoint = null;

            for (int ii = 0; ii < m_availableEndpoints.Count; ii++)
            {
                Uri url = Utils.ParseUri(m_availableEndpoints[ii].EndpointUrl);

                if (url == null)
                {
                    continue;
                }

                if (endpoint == null)
                {
                    endpoint = m_availableEndpoints[ii];
                }

                if (currentProtocol.Matches(url))
                {
                    endpoint = m_availableEndpoints[ii];
                    break;
                }
            }

            UriBuilder builder = null;
            string scheme = Utils.UriSchemeOpcTcp;

            if (currentProtocol != null && currentProtocol.Url != null)
            {
                scheme = currentProtocol.Url.Scheme;
            }

            if (endpoint == null)
            {
                builder = new UriBuilder();
                builder.Host = "localhost";

                if (scheme == Utils.UriSchemeOpcTcp)
                {
                    builder.Port = Utils.UaTcpDefaultPort;
                }
            }
            else
            {
                builder = new UriBuilder(endpoint.EndpointUrl);
            }

            builder.Scheme = scheme;

            endpoint = new EndpointDescription();
            endpoint.EndpointUrl = builder.ToString();
            endpoint.SecurityMode = (MessageSecurityMode)securitymode;
            endpoint.SecurityPolicyUri = SecurityPolicies.GetUri(securitypolicy.ToString("G"));
            endpoint.Server.ApplicationName = endpoint.EndpointUrl;
            endpoint.Server.ApplicationType = ApplicationType.Server;
            endpoint.Server.ApplicationUri = endpoint.EndpointUrl;

            return endpoint;
        }

        private bool DiscoverEndpoints(Uri discoveryUrl, out String message)
        {
            // use a short timeout.
            EndpointConfiguration configuration = EndpointConfiguration.Create(m_configuration);
            configuration.OperationTimeout = m_discoveryTimeout;

            DiscoveryClient client = DiscoveryClient.Create(
                discoveryUrl,
                EndpointConfiguration.Create(m_configuration));

            try
            {
                EndpointDescriptionCollection endpoints = client.GetEndpoints(null);
                OnUpdateEndpoints(endpoints);

                message = String.Empty;
                return true;
            }
            catch (Exception except)
            {
                IronUtilites.LogManager.Manager.WriteLog("Exception", discoveryUrl + " : " + except.ToString());

                message = except.Message;
                return false;
            }
            finally
            {
                client.Close();
            }
        }

        private void updatePropertyItem()
        {
            try
            {

                if (!m_updating)
                {
                    try
                    {
                        if (m_configuration == null)
                        {
                            Init();
                        }
                        SecurityInfo.Clear();
                        Uri discoveryUrl = new Uri(serverUrl);
                        m_updating = true;
                        DiscoveryClient client = DiscoveryClient.Create(
                        discoveryUrl,
                        EndpointConfiguration.Create(m_configuration));
                        EndpointDescriptionCollection endpoints = client.GetEndpoints(null);

                        for (int i = 0; i < endpoints.Count; i++)
                        {

                            if (SecurityInfo.ContainsKey(endpoints[i].SecurityMode.ToString()))
                            {
                                for (int idx = 0; idx < 4; idx++)
                                {

                                    if (SecurityInfo[endpoints[i].SecurityMode.ToString()][idx] == "" || SecurityInfo[endpoints[i].SecurityMode.ToString()][idx] == null)
                                    {
                                        SecurityInfo[endpoints[i].SecurityMode.ToString()][idx] = endpoints[i].SecurityPolicyUri.Split('#')[1];
                                        break;
                                    }
                                }

                            }
                            else
                            {
                                SecurityInfo.Add(endpoints[i].SecurityMode.ToString(), new string[4]);
                                SecurityInfo[endpoints[i].SecurityMode.ToString()][0] = endpoints[i].SecurityPolicyUri.Split('#')[1];
                            }
                        }
                        HE_GlobalVars._ListofRules = new string[SecurityInfo.Keys.Count];
                        int ii = 0;
                        foreach (string s in SecurityInfo.Keys)
                        {
                            HE_GlobalVars._ListofRules[ii++] = s;
                        }


                    }
                    finally
                    {
                        m_updating = false;
                    }
                }

            }
            catch (Exception except)
            {
                IronUtilites.LogManager.Manager.WriteLog("Exception", MethodBase.GetCurrentMethod() + " : " + except.ToString());

            }
        }

        private void OnUpdateEndpoints(object state)
        {
            try
            {
                // get the updated descriptions.
                EndpointDescriptionCollection endpoints = state as EndpointDescriptionCollection;
                int endpointidx = 0;

                if (endpoints == null)
                {
                    //m_showAllOptions = true;
                    //InitializeProtocols(m_availableEndpoints);
                }
                else
                {
                    m_availableEndpoints = endpoints;
                    BuildEndpointDescriptionStrings(m_availableEndpoints);
                    for (int i = 0; i < endpoints.Count; i++)
                    {

                        if (m_endpoint.Description.SecurityMode != endpoints[i].SecurityMode)
                        {
                            continue;
                        }

                        if (m_endpoint.Description.SecurityPolicyUri != endpoints[i].SecurityPolicyUri)
                        {
                            continue;
                        }

                        endpointidx = i;
                    }

                    if (endpoints.Count > 0)
                    {
                        m_currentDescription = endpoints[endpointidx];
                    }

                    // initializing the protocol will trigger an update to all other controls.
                    //InitializeProtocols(m_availableEndpoints);
                }

            }
            catch (Exception e)
            {
                Utils.Trace(e, "Unexpected error updating endpoints.");
            }
        }

        private void BuildEndpointDescriptionStrings(EndpointDescriptionCollection endpoints)
        {
            //lock (m_availableEndpointsDescriptions)
            //{
            //    m_availableEndpointsDescriptions.Clear();

            //    foreach (EndpointDescription endpoint in endpoints)
            //    {
            //        m_availableEndpointsDescriptions.Add(new EndpointDescriptionString(endpoint));
            //    }

            //}
        }

        private EndpointDescription FindBestEndpointDescription(EndpointDescriptionCollection endpoints)
        {
            // filter by the current protocol.
            Protocol currentProtocol = new Protocol(protocol);

            // filter by the current security mode.
            MessageSecurityMode currentMode = MessageSecurityMode.None;

            if (securitymode == enum_SecurityMode.None)
            {
                currentMode = MessageSecurityMode.None;
            }
            else if (securitymode == enum_SecurityMode.Sign)
            {
                currentMode = MessageSecurityMode.Sign;
            }
            else if (securitymode == enum_SecurityMode.SignAndEncrypt)
            {
                currentMode = MessageSecurityMode.SignAndEncrypt;
            }
            // filter by the current security policy.


            string currentPolicy = "None";

            if (securitypolicy == enum_SecurityPolicy.None)
            {
                currentPolicy = "None";
            }
            else if (securitypolicy == enum_SecurityPolicy.Basic256)
            {
                currentPolicy = "Basic256";
            }
            else if (securitypolicy == enum_SecurityPolicy.Basic128Rsa15)
            {
                currentPolicy = "Basic128Rsa15";
            }
            else if (securitypolicy == enum_SecurityPolicy.Basic256Sha256)
            {
                currentPolicy = "Basic256Sha256";
            }
            // find all matching descriptions.      
            EndpointDescriptionCollection matches = new EndpointDescriptionCollection();

            if (endpoints != null)
            {
                foreach (EndpointDescription endpoint in endpoints)
                {
                    Uri url = Utils.ParseUri(endpoint.EndpointUrl);

                    if (url == null)
                    {
                        continue;
                    }

                    if ((currentProtocol != null) && (!currentProtocol.Matches(url)))
                    {
                        continue;
                    }

                    if (currentMode != endpoint.SecurityMode)
                    {
                        continue;
                    }

                    if (currentPolicy != SecurityPolicies.GetDisplayName(endpoint.SecurityPolicyUri))
                    {
                        continue;
                    }

                    matches.Add(endpoint);
                }
            }

            // check for no matches.
            if (matches.Count == 0)
            {
                return null;
            }

            // check for single match.
            if (matches.Count == 1)
            {
                return matches[0];
            }

            // choose highest priority.
            EndpointDescription bestMatch = matches[0];

            for (int ii = 1; ii < matches.Count; ii++)
            {
                if (bestMatch.SecurityLevel < matches[ii].SecurityLevel)
                {
                    bestMatch = matches[ii];
                }
            }

            return bestMatch;
        }

        private ConfiguredEndpoint CreateEndPoint(string url)
        {
            // check for security parameters appended to the URL
            string parameters = null;

            int index = url.IndexOf("- [", StringComparison.Ordinal);

            if (index != -1)
            {
                parameters = url.Substring(index + 3);
                url = url.Substring(0, index).Trim();
            }

            MessageSecurityMode currentMode = MessageSecurityMode.SignAndEncrypt;
            string currentPolicyUri = SecurityPolicies.Basic256Sha256;
            bool useBinaryEncoding = true;

            if (!String.IsNullOrEmpty(parameters))
            {
                string[] fields = parameters.Split(new char[] { '-', '[', ':', ']' }, StringSplitOptions.RemoveEmptyEntries);

                try
                {
                    if (fields.Length > 0)
                    {
                        currentMode = (MessageSecurityMode)Enum.Parse(typeof(MessageSecurityMode), fields[0], false);
                    }
                    else
                    {
                        currentMode = MessageSecurityMode.None;
                    }
                }
                catch
                {
                    currentMode = MessageSecurityMode.None;
                } 

                try
                {
                    if (fields.Length > 1)
                    {
                        currentPolicyUri = SecurityPolicies.GetUri(fields[1]);
                    }
                    else
                    {
                        currentPolicyUri = SecurityPolicies.None;
                    }
                }
                catch
                {
                    currentPolicyUri = SecurityPolicies.None;
                }

                try
                {
                    if (fields.Length > 2)
                    {
                        useBinaryEncoding = fields[2] == "Binary";
                    }
                    else
                    {
                        useBinaryEncoding = false;
                    }
                }
                catch
                {
                    useBinaryEncoding = false;
                }
            }



            // filter by the current security mode.
            if (securitymode == enum_SecurityMode.None)
            {
                currentMode = MessageSecurityMode.None;
            }
            else if (securitymode == enum_SecurityMode.Sign)
            {
                currentMode = MessageSecurityMode.Sign;
            }
            else if (securitymode == enum_SecurityMode.SignAndEncrypt)
            {
                currentMode = MessageSecurityMode.SignAndEncrypt;
            }
            // filter by the current security policy.



            if (securitypolicy == enum_SecurityPolicy.None)
            {
                currentPolicyUri = SecurityPolicies.None;
            }
            else if (securitypolicy == enum_SecurityPolicy.Basic256)
            {
                currentPolicyUri = SecurityPolicies.Basic256;
            }
            else if (securitypolicy == enum_SecurityPolicy.Basic128Rsa15)
            {
                currentPolicyUri = SecurityPolicies.Basic128Rsa15;
            }
            else if (securitypolicy == enum_SecurityPolicy.Basic256Sha256)
            {
                currentPolicyUri = SecurityPolicies.Basic256Sha256;
            }








            Uri uri = new Uri(url);

            EndpointDescription description = new EndpointDescription();

            description.EndpointUrl = uri.ToString();
            description.SecurityMode = currentMode;
            description.SecurityPolicyUri = currentPolicyUri;
            description.Server.ApplicationUri = Utils.UpdateInstanceUri(uri.ToString());
            description.Server.ApplicationName = uri.AbsolutePath;

            if (description.EndpointUrl.StartsWith(Utils.UriSchemeOpcTcp, StringComparison.Ordinal))
            {
                description.TransportProfileUri = Profiles.UaTcpTransport;
                description.Server.DiscoveryUrls.Add(description.EndpointUrl);
            }
            else if (description.EndpointUrl.StartsWith(Utils.UriSchemeHttps, StringComparison.Ordinal))
            {
                description.TransportProfileUri = Profiles.HttpsBinaryTransport;
                description.Server.DiscoveryUrls.Add(description.EndpointUrl);
            }

            ConfiguredEndpoint endpoint = new ConfiguredEndpoint(new ConfiguredEndpointCollection(), description, null);
            endpoint.Configuration.UseBinaryEncoding = useBinaryEncoding;
            endpoint.UpdateBeforeConnect = true;
            return endpoint;
        }

        public void Disconnect()
        {
            StatusCode status;
            //m_subscription.Delete(true);
            //m_subscription = null;
            //m_browser.Session.Close();
            status = m_session.Close();
            m_monitoredItem.Clear();
            m_monitoredTagValue.Clear();
            m_monitoredIdxTag.Clear();
            
            Utils.Trace("Disconnect Status : " + status.ToString());
            m_session = null;
            m_browser = null;
            tempDelete = null;
        }

        private Exception SessionOpen(object state)
        {
            try
            {
                Session session = ((object[])state)[0] as Session;
                string sessionName = ((object[])state)[1] as string;
                IUserIdentity identity = ((object[])state)[2] as IUserIdentity;
                IList<string> preferredLocales = ((object[])state)[3] as IList<string>;
                bool? checkDomain = ((object[])state)[4] as bool?;

                // open the session.
                session.Open(sessionName, (uint)session.SessionTimeout, identity, preferredLocales, checkDomain ?? true);

                return null;
            }
            catch (Exception except)
            {
                IronUtilites.LogManager.Manager.WriteLog("Exception", except.ToString());
                return except;
            }
        }
            
        public object ReadAny(string tag)
        {
            ReadValueIdCollection nodesToRead = new ReadValueIdCollection();

            VariableNode node = m_session.NodeCache.Find("ns=" + namespacenum + ";s=" + tag) as VariableNode;

            if (node == null)
            {
                return null;
            }

            ReadValueId valueId = new ReadValueId();

            DataValueCollection values = null;

            DiagnosticInfoCollection diagnosticInfos = null;

            valueId.NodeId = node.NodeId;
            valueId.AttributeId = Attributes.Value;
            nodesToRead.Add(valueId);

            ResponseHeader responseHeader = m_session.Read(
                null,
                0,
                TimestampsToReturn.Both,
                nodesToRead,
                out values,
                out diagnosticInfos);

            ClientBase.ValidateResponse(values, nodesToRead);
            ClientBase.ValidateDiagnosticInfos(diagnosticInfos, nodesToRead);


            return values[0].Value;
        }

        public object[] ReadAny(string[] tags)
        {
            if (tags == null)
            {
                return null;
            }

            ReadValueIdCollection nodesToRead = new ReadValueIdCollection();

            //VariableNode node = null;


            DataValueCollection values = null;

            DiagnosticInfoCollection diagnosticInfos = null;


            foreach (string s in tags)
            {
                //VariableNode node = m_session.NodeCache.Find("ns=" + namespacenum + ";s=" + tag) as VariableNode;

                //node = m_session.NodeCache.Find("ns=" + namespacenum + ";s=" + s) as VariableNode;
                ReadValueId valueId = new ReadValueId();

                //NodeId node = new NodeId(s, namespacenum);
                valueId.NodeId = new NodeId(s, namespacenum);
                //valueId.NodeId = node.NodeId;
                valueId.AttributeId = Attributes.Value;
                nodesToRead.Add(valueId);
            }


            ResponseHeader responseHeader = m_session.Read(
                null,
                0,
                TimestampsToReturn.Both,
                nodesToRead,
                out values,
                out diagnosticInfos);

            ClientBase.ValidateResponse(values, nodesToRead);
            ClientBase.ValidateDiagnosticInfos(diagnosticInfos, nodesToRead);

            object[] returnvalue = new object[tags.Length];

            for (int i = 0; i < values.Count; i++)
            {
                if (i < returnvalue.Length)
                {
                    returnvalue[i] = values[i].Value;
                }
            }

            //foreach (DataValue s in values)
            //{
            //    returnvalue.Add(s.Value);
            //}

            return returnvalue;
        }

        public string[] WriteData(string[] tags, object[] data)
        {
            WriteValueCollection nodesToWrite = new WriteValueCollection();
            
            for (int i = 0; i < tags.Length; i++)
            {
                WriteValue valueId = new WriteValue();
                DataValue vData = new DataValue();

                VariableNode node = m_session.NodeCache.Find("ns=" + namespacenum + ";s=" + tags[i]) as VariableNode;

                if (node == null)
                {
                    nodesToWrite.Add(null);
                }
                else
                {

                    vData.Value = data[i];
                    valueId.AttributeId = Attributes.Value;
                    valueId.Value = vData;
                    valueId.NodeId = node.NodeId;

                    nodesToWrite.Add(valueId);
                }
            }

            if (nodesToWrite.Count == 0)
            {
                return null;
            }

            StatusCodeCollection results = null;
            DiagnosticInfoCollection diagnosticInfos = null;



            ResponseHeader responseHeader = m_session.Write(
                    null,
                    nodesToWrite,
                    out results,
                    out diagnosticInfos);

            ClientBase.ValidateResponse(results, nodesToWrite);
            ClientBase.ValidateDiagnosticInfos(diagnosticInfos, nodesToWrite);

            string[] returnvalue = null;

            if (results != null)
            {
                returnvalue = new string[results.Count];
                
                for (int i = 0; i < results.Count; i++)
                {
                    returnvalue[i] = results[i] + "";
                }
            }
            return returnvalue;
        }

        public string WriteData(string tag, object data)
        {

            WriteValueCollection nodesToWrite = new WriteValueCollection();

            //VariableNode node = null;

            WriteValue valueId = new WriteValue();

            DataValue vData = new DataValue();


            //node = m_session.NodeCache.Find("ns=" + namespacenum + ";s=" + tag) as VariableNode;

            NodeId node = new NodeId(tag, namespacenum);

            vData.Value = data;
            valueId.AttributeId = Attributes.Value;
            valueId.Value = vData;
            valueId.NodeId = node;

            nodesToWrite.Add(valueId);

            if (nodesToWrite.Count == 0)
            {
                return null;
            }

            StatusCodeCollection results = null;
            DiagnosticInfoCollection diagnosticInfos = null;



            ResponseHeader responseHeader = m_session.Write(
                    null,
                    nodesToWrite,
                    out results,
                    out diagnosticInfos);

            ClientBase.ValidateResponse(results, nodesToWrite);
            ClientBase.ValidateDiagnosticInfos(diagnosticInfos, nodesToWrite);

            if (!StatusCode.IsGood(results[0].Code))
            {
                Utils.Trace("WriteData Error >>>>>>>>" + tag);
            }

            return results[0].ToString();
        }
        public List<Subscription> sub = null;
        private void CreateSubscribe(string displayname)
        {

            if (m_session == null)
            {
                return;
            }
            if (m_subscription != null && m_subscription.Created)
            {
                return;
            }

            Subscription subscription = new Subscription(m_session.DefaultSubscription);
            
            subscription.DisplayName = displayname;
            subscription.PublishingInterval = publishingInterval;
            subscription.KeepAliveCount = keepAliveCount;
            subscription.LifetimeCount = lifetimeCount;
            subscription.MaxNotificationsPerPublish = maxNotifications;
            subscription.Priority = priority;
            

            if (subscription.Created)
            {
                subscription.SetPublishingMode(publishingenable);
            }
            else
            {
                subscription.PublishingEnabled = publishingenable;
            }

            m_session.AddSubscription(subscription);
            m_subscription = subscription;
            m_subscription.Create();


            Subscription duplicateSubscription = m_session.Subscriptions.FirstOrDefault(s => s.Id != 0 && s.Id.Equals(subscription.Id) && s != subscription);
            if (duplicateSubscription != null)
            {
                Utils.Trace("Duplicate subscription was created with the id: {0}", duplicateSubscription.Id);

                //DialogResult result = MessageBox.Show("Duplicate subscription was created with the id: " + duplicateSubscription.Id + ". Do you want to keep it?", "Warning", MessageBoxButtons.YesNo);
                //if (result == System.Windows.Forms.DialogResult.No)
                //{
                //    duplicateSubscription.Delete(false);
                //    m_session.RemoveSubscription(subscription);

                //    return;
                //}
            }


        }

        public void SetSubscribeOption(
            int publishingInterval,
            uint keepAliveCount,
            uint lifetimeCount,
            uint maxNotifications,
            byte priority,
            bool publishingenable)
        {
            if (m_subscription != null)
            {
                this.PublishingInterval = publishingInterval;
                this.keepAliveCount = keepAliveCount;
                this.lifetimeCount = lifetimeCount;
                this.maxNotifications = maxNotifications;
                this.priority = priority;
                this.publishingenable = publishingenable;
            }

        }

        public void Subscribe()
        {
            if (m_subscription != null && m_subscription.Created)
            {
                if (m_session != null)
                {
                    m_session.RemoveSubscription(m_subscription);
                }
            }

            if (!m_subscription.Created)
            {
                CreateSubscribe("Subscribe " + subscribeId++);
            }
            // remove previous subscription.

            if (m_subscription != null)
            {
                m_subscription.StateChanged -= m_SubscriptionStateChanged;
                m_subscription.PublishStatusChanged -= m_PublishStatusChanged;
                m_subscription.Session.Notification -= m_SessionNotification;

            }

            // start receiving notifications from the new subscription.

            if (m_subscription != null)
            {
                m_subscription.StateChanged += m_SubscriptionStateChanged;
                m_subscription.PublishStatusChanged += m_PublishStatusChanged;
                m_subscription.Session.Notification += m_SessionNotification;
            }

            if (m_subscription == null)
            {
                return;
            }

            m_subscription.DeleteItems();


            foreach (MonitoredItem m in m_monitoredItem)
            {
                m_subscription.AddItem(m);
            }

            m_subscription.ApplyChanges();
        }

        public uint Subscribe(string tag)
        {

            try
            {
                if (CheckSubscribeTag(tag))
                {
                    return GetSubscribeClientHandle(tag);
                }

                ReferenceDescription reference = null;

                //VariableNode node = null;

                MonitoredItem monitoredItem = null;


                if (m_subscription == null)
                {
                    m_subscription = new Subscription();
                }

                reference = new ReferenceDescription();
                monitoredItem = new MonitoredItem(m_subscription.DefaultItem);
                VariableNode node = m_session.NodeCache.Find("ns=" + namespacenum + ";s=" + tag) as VariableNode;

                //NodeId node = new NodeId(tag, namespacenum);

                reference.NodeId = node.NodeId;
                reference.NodeClass = NodeClass.Variable;

                //monitoredItem.DisplayName = m_session.NodeCache.GetDisplayText(reference);
                monitoredItem.DisplayName = tag;
                monitoredItem.StartNodeId = (NodeId)reference.NodeId;
                monitoredItem.NodeClass = reference.NodeClass;
                monitoredItem.AttributeId = Attributes.Value;
                monitoredItem.SamplingInterval = 0;
                monitoredItem.QueueSize = 1;
                //m_PublishStatusChanged
                // add condition fields to any event filter.
                EventFilter filter = monitoredItem.Filter as EventFilter;

                if (filter != null)
                {
                    monitoredItem.AttributeId = Attributes.EventNotifier;
                    monitoredItem.QueueSize = 0;
                }
                m_monitoredIdxTag[monitoredItem.ClientHandle] = tag;
                MonitoredTagValue[tag] = null;
                //m_monitoredInfo.Add(0, monitoredItem.ClientHandle.ToString());
                //m_monitoredInfo.Add(1, tag);
                //m_monitoredInfo.Add(2, "0");
                m_monitoredItem.Add(monitoredItem);
                m_subscription.AddItem(monitoredItem);
                
                m_subscription.ApplyChanges();
                return monitoredItem.ClientHandle;

            }
            catch (Exception except)
            {
                IronUtilites.LogManager.Manager.WriteLog("Exception", MethodBase.GetCurrentMethod() + " : " + except.ToString());

                return uint.MaxValue;
            }



        }
        string[] tempDelete;
        public uint[] Subscribe(string[] tags)
        {

            List<uint> clientid = new List<uint>();
            
            ReferenceDescription reference = null;

            VariableNode node = null;
            
            MonitoredItem monitoredItem = null;
            tempDelete = tags;
            for(int i=0;i<tags.Length;i++)
            {
                try
                {
                    if (CheckSubscribeTag(tags[i]))
                    {
                        continue;//clientid[i] = GetSubscribeClientHandle(tags[i]);
                    }

                    reference = new ReferenceDescription();
                    monitoredItem = new MonitoredItem(m_subscription.DefaultItem);
                    node = m_session.NodeCache.Find("ns=" + namespacenum + ";s=" + tags[i]) as VariableNode;

                    //node = new NodeId(s, namespacenum);

                    if (node == null)
                    {
                        Trace.WriteLine("Tag Error >>>>" + tags[i] + "<<<< Fail subScribe");
                        continue;
                    }
                    reference.NodeId = node.NodeId;
                    reference.NodeClass = NodeClass.Variable;

                    //monitoredItem.DisplayName = m_session.NodeCache.GetDisplayText(reference);
                    monitoredItem.DisplayName = tags[i];
                    monitoredItem.StartNodeId = (NodeId)reference.NodeId;
                    monitoredItem.NodeClass = reference.NodeClass;
                    monitoredItem.AttributeId = Attributes.Value;
                    monitoredItem.SamplingInterval = 0;
                    monitoredItem.QueueSize = 1;

                    // add condition fields to any event filter.
                    EventFilter filter = monitoredItem.Filter as EventFilter;

                    if (filter != null)
                    {
                        monitoredItem.AttributeId = Attributes.EventNotifier;
                        monitoredItem.QueueSize = 0;
                    }

                    m_monitoredIdxTag[monitoredItem.ClientHandle] = tags[i];
                    MonitoredTagValue[tags[i]] = null;
                    //m_monitoredInfo.Add(0, monitoredItem.ClientHandle.ToString());
                    //m_monitoredInfo.Add(1, s);
                    //m_monitoredInfo.Add(2, "0");
                    m_monitoredItem.Add(monitoredItem);
                    clientid.Add(monitoredItem.ClientHandle);
                    m_subscription.AddItem(monitoredItem);
                }
                catch (Exception except)
                {
                    IronUtilites.LogManager.Manager.WriteLog("Exception", MethodBase.GetCurrentMethod() + " : " + except.ToString());

                    IronUtilites.LogManager.Manager.WriteLog("Error", "Subscribe Tag Matching Error >>>>>>>>>>>>>>>>>>" + tags[i]);
                }

            }
            m_subscription.ApplyChanges();
            return clientid.ToArray();
        }

        //public int Subscribe(string variable, UpdateMode mode, int updateTime, ReturnValues CallBack)
        //{

        //    RegisterVariable varInfo = new RegisterVariable();

        //    varInfo.Name = variable;
        //    varInfo.Mode = mode;
        //    varInfo.UpdateTime = updateTime;
        //    varInfo.CallBack = new List<ReturnValues>();


        //    publishingInterval = updateTime;



        //    int id = Subscribe(variable);


        //    if (id == -1)
        //    {
        //        return id;
        //    }

        //    varInfo.CallBack.Add(CallBack);
        //    registerDic.Add(id.ToString(), varInfo);

        //    return id;

        //}

        public void Unsubscribe()
        {


            if (m_session != null && m_subscription != null)
            {
                if (m_subscription.Created)
                {
                    try
                    {
                        m_session.RemoveSubscription(m_subscription);
                        m_subscription.Delete(true);
                    }
                    catch (Exception except)
                    {
                        System.Diagnostics.Trace.WriteLine(except.ToString());
                    }
                }
            }
            //m_monitoredInfo.Clear();
            m_monitoredIdxTag.Clear();
            MonitoredTagValue.Clear();
            m_monitoredItem.Clear();
        }

        public void Unsubscribe(uint id)
        {

            if (m_monitoredIdxTag.TryGetValue(id, out string tag))
            {

                int removeIdx = -1;

                for (int i = 0; i < m_monitoredItem.Count; i++)
                {
                    if (m_monitoredItem[i].ClientHandle == id)
                    {
                        removeIdx = (int)id;
                    }
                }
                if (removeIdx >= 0)
                {
                    m_monitoredItem.RemoveAt(removeIdx);
                    m_subscription.DeleteItems();

                    foreach (MonitoredItem m in m_monitoredItem)
                    {
                        m_subscription.AddItem(m);
                    }

                    m_subscription.ApplyChanges();

                    m_monitoredIdxTag.Remove(id);
                    MonitoredTagValue.Remove(tag);
                    //m_monitoredInfo.RemoveAt((m_monitoredInfo.FindIndex(0, id.ToString())));

                }
            }
        }

        public object ReadAny(string variable, Type type)
        {
            throw new NotImplementedException();
        }

        public object ReadBuffer(string device, int type, int addr, int size)
        {
            try
            {
                CallMethodRequest request = new CallMethodRequest();
                VariantCollection inputArguments = new VariantCollection();

                inputArguments.Add(new Variant(device));
                inputArguments.Add(new Variant(type));
                inputArguments.Add(new Variant(addr));
                inputArguments.Add(new Variant(size));



                request.ObjectId = new NodeId("Methods", namespacenum);
                request.MethodId = new NodeId("ReadBuffer", namespacenum);
                request.InputArguments = inputArguments;

                CallMethodRequestCollection requests = new CallMethodRequestCollection();
                requests.Add(request);

                CallMethodResultCollection results;
                DiagnosticInfoCollection diagnosticInfos;

                ResponseHeader responseHeader = m_session.Call(
                    null,
                    requests,
                    out results,
                    out diagnosticInfos);

                if (StatusCode.IsBad(results[0].StatusCode))
                {
                    throw new ServiceResultException(new ServiceResult(results[0].StatusCode, 0, diagnosticInfos, responseHeader.StringTable));
                }


                if (results[0].OutputArguments.Count == 0)
                {
                    Utils.Trace("Method executed successfully.");
                }
                return results[0].OutputArguments[0].Value;
            }
            catch (Exception except)
            {
                IronUtilites.LogManager.Manager.WriteLog("Exception", MethodBase.GetCurrentMethod() + " : " + except.ToString());
                return null;
            }

        }
        
        public int WriteBuffer(string device, int type, int addr, int size, object data)
        {
            try
            {
                CallMethodRequest request = new CallMethodRequest();
                VariantCollection inputArguments = new VariantCollection();


                inputArguments.Add(new Variant(device));
                inputArguments.Add(new Variant(type));
                inputArguments.Add(new Variant(addr));
                inputArguments.Add(new Variant(size));
                inputArguments.Add(new Variant((byte[])data));



                request.ObjectId = new NodeId("Methods", namespacenum);
                request.MethodId = new NodeId("WriteBuffer", namespacenum);
                request.InputArguments = inputArguments;

                CallMethodRequestCollection requests = new CallMethodRequestCollection();
                requests.Add(request);

                CallMethodResultCollection results;
                DiagnosticInfoCollection diagnosticInfos;

                ResponseHeader responseHeader = m_session.Call(
                    null,
                    requests,
                    out results,
                    out diagnosticInfos);

                if (StatusCode.IsBad(results[0].StatusCode))
                {
                    throw new ServiceResultException(new ServiceResult(results[0].StatusCode, 0, diagnosticInfos, responseHeader.StringTable));
                }


                if (results[0].OutputArguments.Count == 0)
                {
                    Utils.Trace("Method executed successfully.");
                }
                return 0;
            }
            catch (Exception except)
            {
                IronUtilites.LogManager.Manager.WriteLog("Exception", MethodBase.GetCurrentMethod() + " : " + except.ToString());
                return -1;
            }
        }

        private bool CheckSubscribeTag(string tag)
        {
            if (m_monitoredItem != null)
            {
                for(int i=0;i< m_monitoredItem.Count;i++)
                {
                    if (m_monitoredItem[i].DisplayName == tag)
                    {
                        return true;
                    }
                }
            }
            
            return false;
        }

        private uint GetSubscribeClientHandle(string tag)
        {
            if (m_monitoredItem != null)
            {
                for (int i = 0; i < m_monitoredItem.Count; i++)
                {
                    if (m_monitoredItem[i].DisplayName == tag)
                    {
                        return m_monitoredItem[i].ClientHandle;
                    }
                }
            }

            return 0;
        }

        private ReferenceDescriptionCollection GetNodeList(object rootId = null)
        {
            try
            {
                if (m_browser == null)
                {
                    InitBrowse();

                }


                // check if session is connected.
                if (m_browser == null || !m_browser.Session.Connected)
                {
                    return null;
                }

                if (rootId == null)
                {
                    rootId = new NodeId(Objects.ObjectsFolder);
                }

                if (m_browser != null)
                {
                    INode node = m_browser.Session.NodeCache.Find(Objects.ObjectsFolder);

                    if (node == null)
                    {
                        return null;
                    }

                    // fetch references.

                    if (rootId != null)
                    {
                        return m_browser.Browse((NodeId)rootId);
                    }
                    else
                    {
                        return m_browser.Browse((NodeId)node.NodeId);
                    }
                }
            }
            catch (Exception except)
            {
                IronUtilites.LogManager.Manager.WriteLog("Exception", MethodBase.GetCurrentMethod() + " : " + except.ToString());
            }
            return null;
        }

        public string[] GetNodeNameList(object rootId = null)
        {
            List<string> nodeList = new List<string>();

            ReferenceDescriptionCollection objectCollection = GetNodeList(rootId);
            if (objectCollection == null)
            {
                return null;
            }
            for (int i = 0; i < objectCollection.Count; i++)
            {
                nodeList.Add(objectCollection[i].DisplayName.Text);
            }

            return nodeList.ToArray();
        }

        public string[] GetAllVariableTagList(object rootId, List<string> variableList, int depth)
        {

            if (depth <= 0)
            {
                return null;
            }

            if (variableList == null)
            {
                variableList = new List<string>();
            }
            ReferenceDescriptionCollection nodeList = GetNodeList(rootId);

            if (nodeList == null)
            {
                return null;
            }

            List<NodeId> browseNodeIds = new List<NodeId>();

            foreach (ReferenceDescription nodeItem in nodeList)
            {
                if (nodeItem.NodeId.NamespaceIndex == namespacenum && nodeItem.NodeClass == NodeClass.Variable)
                {
                    variableList.Add(nodeItem.NodeId.Identifier + "");
                }
                else
                {
                    browseNodeIds.Add((NodeId)nodeItem.NodeId);
                }
            }

            depth--;

            foreach (NodeId node in browseNodeIds)
            {
                GetAllVariableTagList(node, variableList, depth);
            }
            return variableList.ToArray();
            
        }

        public string[] GetAllVariableNodeList(object rootId, List<string> variableList, int depth)
        {

            if (depth <= 0)
            {
                return null;
            }

            if (variableList == null)
            {
                variableList = new List<string>();
            }
            ReferenceDescriptionCollection nodeList = GetNodeList(rootId);

            if (nodeList == null)
            {
                return null;
            }

            List<NodeId> browseNodeIds = new List<NodeId>();

            foreach (ReferenceDescription nodeItem in nodeList)
            {
                if (nodeItem.NodeId.NamespaceIndex == namespacenum && nodeItem.NodeClass == NodeClass.Variable)
                {
                    variableList.Add(nodeItem.NodeId.Identifier + "");
                }
                else
                {
                    browseNodeIds.Add((NodeId)nodeItem.NodeId);
                }
            }

            depth--;

            foreach (NodeId node in browseNodeIds)
            {
                GetAllVariableNodeList(node, variableList, depth);
            }
            return variableList.ToArray();

        }

        public ReferenceDescription[] GetAllVariableNodeDescList(object rootId, List<ReferenceDescription> variableList, int depth)
        {

            if (depth <= 0)
            {
                return null;
            }

            if (variableList == null)
            {
                variableList = new List<ReferenceDescription>();
            }
            ReferenceDescriptionCollection nodeList = GetNodeList(rootId);

            if (nodeList == null)
            {
                return null;
            }

            List<NodeId> browseNodeIds = new List<NodeId>();

            foreach (ReferenceDescription nodeItem in nodeList)
            {
                if (nodeItem.NodeId.NamespaceIndex == namespacenum && nodeItem.NodeClass == NodeClass.Variable)
                {
                    variableList.Add(nodeItem);
                }
                else
                {
                    browseNodeIds.Add((NodeId)nodeItem.NodeId);
                }
            }

            depth--;

            foreach (NodeId node in browseNodeIds)
            {
                GetAllVariableNodeDescList(node, variableList, depth);
            }
            return variableList.ToArray();

        }


        public DataTree GetMakeTree(object rootId, int depth, DataTree dataTree, Dictionary<string, DataTree> arrayTree)
        {
            if (depth <= 0)
            {
                return null;
            }

            ReferenceDescriptionCollection nodeList = GetNodeList(rootId);

            if (nodeList == null)
            {
                return null;
            }

            List<NodeId> browseNodeIds = new List<NodeId>();
            List<DataTree> dataTrees = new List<DataTree>();
            MonitoredItemNotification monitoredItem = new MonitoredItemNotification();
            foreach (ReferenceDescription nodeItem in nodeList)
            {
                if (nodeItem.NodeId.NamespaceIndex == namespacenum && nodeItem.NodeClass == NodeClass.Variable)
                {
                    DataTree setData = new DataTree();
                    setData.nodeId = nodeItem.NodeId.Identifier + "";
                    setData.idType = nodeItem.NodeId.IdType + "";
                    setData.browseName = nodeItem.BrowseName.Name + "";
                    setData.displayName = nodeItem.DisplayName.Text + "";
                    setData.value = monitoredItem.Value.Value + "";
                    setData.sourceTimestamp = DateTime.Now + "";
                    setData.serverTimestamp = DateTime.Now + "";
                    setData.statusCode = monitoredItem.Value.StatusCode + "";
                    setData.dataType = monitoredItem.TypeId.IdType + "";
                    setData.isFolder = false;
                    dataTree.ChildNodes.Add(setData);
                    arrayTree.Add(nodeItem.NodeId.Identifier + "", setData);
                }
                else if (nodeItem.NodeId.NamespaceIndex == namespacenum && nodeItem.NodeClass == NodeClass.Object)
                {
                    DataTree setData = new DataTree();
                    setData.nodeId = nodeItem.NodeId.Identifier + "";
                    setData.idType = nodeItem.NodeId.IdType + "";
                    setData.browseName = nodeItem.BrowseName.Name + "";
                    setData.displayName = nodeItem.DisplayName.Text + "";
                    setData.value = monitoredItem.Value.Value + "";
                    setData.sourceTimestamp = DateTime.Now + "";
                    setData.serverTimestamp = DateTime.Now + "";
                    setData.statusCode = monitoredItem.Value.StatusCode + "";
                    setData.dataType = nodeItem.TypeId.IdType + "";
                    setData.isFolder = true;
                    dataTree.ChildNodes.Add(setData);
                    dataTrees.Add(setData);
                    arrayTree.Add(nodeItem.NodeId.Identifier + "", setData);
                    browseNodeIds.Add((NodeId)nodeItem.NodeId);
                }
            }

            depth--;
            int idx = 0;
            foreach (NodeId node in browseNodeIds)
            {
                GetMakeTree(node, depth, dataTrees[idx++], arrayTree);
            }
            return dataTree;
        }


        public INode FindNode(ReferenceDescription referenceDescription)
        {
            ReferenceDescription node = new ReferenceDescription();

            return m_session.NodeCache.Find(referenceDescription.NodeId);
        }

        private object GetMatrixObjectData(Opc.Ua.Matrix matrix)
        {
            try
            {
                switch (matrix.TypeInfo.BuiltInType)
                {
                    case BuiltInType.Boolean:

                        if (matrix.Dimensions.Length == 2)
                        {
                            bool[,] array = new bool[matrix.Dimensions[0], matrix.Dimensions[1]];

                            for (int i = 0; i < matrix.Dimensions.Length; i++)
                            {
                                for (int j = 0; j < matrix.Dimensions[i]; j++)
                                {
                                    array[i, j] = (bool)matrix.Elements.GetValue((i * matrix.Dimensions[1]) + j);
                                }
                            }
                            return array;
                        }

                        return null;
                    case BuiltInType.SByte:

                        if (matrix.Dimensions.Length == 2)
                        {
                            sbyte[,] array = new sbyte[matrix.Dimensions[0], matrix.Dimensions[1]];

                            for (int i = 0; i < matrix.Dimensions.Length; i++)
                            {
                                for (int j = 0; j < matrix.Dimensions[i]; j++)
                                {
                                    array[i, j] = (sbyte)matrix.Elements.GetValue((i * matrix.Dimensions[1]) + j);
                                }
                            }
                            return array;
                        }

                        return null;
                    case BuiltInType.Byte:
                        if (matrix.Dimensions.Length == 2)
                        {
                            byte[,] array = new byte[matrix.Dimensions[0], matrix.Dimensions[1]];

                            for (int i = 0; i < matrix.Dimensions.Length; i++)
                            {
                                for (int j = 0; j < matrix.Dimensions[i]; j++)
                                {
                                    array[i, j] = (byte)matrix.Elements.GetValue((i * matrix.Dimensions[1]) + j);
                                }
                            }
                            return array;
                        }

                        return null;
                    case BuiltInType.Int16:
                        if (matrix.Dimensions.Length == 2)
                        {
                            Int16[,] array = new Int16[matrix.Dimensions[0], matrix.Dimensions[1]];

                            for (int i = 0; i < matrix.Dimensions.Length; i++)
                            {
                                for (int j = 0; j < matrix.Dimensions[i]; j++)
                                {
                                    array[i, j] = (Int16)matrix.Elements.GetValue((i * matrix.Dimensions[1]) + j);
                                }
                            }
                            return array;
                        }

                        return null;
                    case BuiltInType.UInt16:
                        if (matrix.Dimensions.Length == 2)
                        {
                            UInt16[,] array = new UInt16[matrix.Dimensions[0], matrix.Dimensions[1]];

                            for (int i = 0; i < matrix.Dimensions.Length; i++)
                            {
                                for (int j = 0; j < matrix.Dimensions[i]; j++)
                                {
                                    array[i, j] = (UInt16)matrix.Elements.GetValue((i * matrix.Dimensions[1]) + j);
                                }
                            }
                            return array;
                        }

                        return null;
                    case BuiltInType.Int32:
                        if (matrix.Dimensions.Length == 2)
                        {
                            Int32[,] array = new Int32[matrix.Dimensions[0], matrix.Dimensions[1]];

                            int dLength = matrix.Dimensions.Length;

                            for (int i = 0; i < dLength; i++)
                            {
                                for (int j = 0; j < matrix.Dimensions[i]; j++)
                                {
                                    object dataObj = matrix.Elements.GetValue((i * matrix.Dimensions[1]) + j);
                                    array[i, j] = dataObj == null ? 0 : (Int32)dataObj;
                                }
                            }
                            return array;
                        }

                        return null;
                    case BuiltInType.UInt32:
                        if (matrix.Dimensions.Length == 2)
                        {
                            UInt32[,] array = new UInt32[matrix.Dimensions[0], matrix.Dimensions[1]];

                            int dLength = matrix.Dimensions.Length;

                            for (int i = 0; i < dLength; i++)
                            {
                                for (int j = 0; j < matrix.Dimensions[i]; j++)
                                {
                                    object dataObj = matrix.Elements.GetValue((i * matrix.Dimensions[1]) + j);
                                    array[i, j] = dataObj == null ? 0 : (UInt32)dataObj;
                                }
                            }
                            return array;
                        }

                        return null;
                    case BuiltInType.Int64:
                        if (matrix.Dimensions.Length == 2)
                        {
                            Int64[,] array = new Int64[matrix.Dimensions[0], matrix.Dimensions[1]];

                            int dLength = matrix.Dimensions.Length;

                            for (int i = 0; i < dLength; i++)
                            {
                                for (int j = 0; j < matrix.Dimensions[i]; j++)
                                {
                                    object dataObj = matrix.Elements.GetValue((i * matrix.Dimensions[1]) + j);
                                    array[i, j] = dataObj == null ? 0 : (Int64)dataObj;
                                }
                            }
                            return array;
                        }

                        return null;
                    case BuiltInType.UInt64:
                        if (matrix.Dimensions.Length == 2)
                        {
                            UInt64[,] array = new UInt64[matrix.Dimensions[0], matrix.Dimensions[1]];

                            int dLength = matrix.Dimensions.Length;

                            for (int i = 0; i < dLength; i++)
                            {
                                for (int j = 0; j < matrix.Dimensions[i]; j++)
                                {
                                    object dataObj = matrix.Elements.GetValue((i * matrix.Dimensions[1]) + j);
                                    array[i, j] = dataObj == null ? 0 : (UInt64)dataObj;
                                }
                            }
                            return array;
                        }

                        return null;
                    case BuiltInType.Float:
                        if (matrix.Dimensions.Length == 2)
                        {
                            float[,] array = new float[matrix.Dimensions[0], matrix.Dimensions[1]];

                            int dLength = matrix.Dimensions.Length;

                            for (int i = 0; i < dLength; i++)
                            {
                                for (int j = 0; j < matrix.Dimensions[i]; j++)
                                {
                                    object dataObj = matrix.Elements.GetValue((i * matrix.Dimensions[1]) + j);
                                    array[i, j] = dataObj == null ? 0 : (float)dataObj;
                                }
                            }
                            return array;
                        }

                        return null;
                    case BuiltInType.Double:
                        if (matrix.Dimensions.Length == 2)
                        {
                            double[,] array = new double[matrix.Dimensions[0], matrix.Dimensions[1]];

                            int dLength = matrix.Dimensions.Length;

                            for (int i = 0; i < dLength; i++)
                            {
                                for (int j = 0; j < matrix.Dimensions[i]; j++)
                                {
                                    object dataObj = matrix.Elements.GetValue((i * matrix.Dimensions[1]) + j);
                                    array[i, j] = dataObj == null ? 0 : (double)dataObj;
                                }
                            }
                            return array;
                        }

                        return null;
                    case BuiltInType.String:
                        if (matrix.Dimensions.Length == 2)
                        {
                            string[,] array = new string[matrix.Dimensions[0], matrix.Dimensions[1]];

                            int dLength = matrix.Dimensions.Length;

                            for (int i = 0; i < matrix.Dimensions[0]; i++)
                            {
                                for (int j = 0; j < matrix.Dimensions[1]; j++)
                                {
                                    object dataObj = matrix.Elements.GetValue((i * matrix.Dimensions[1]) + j);
                                    array[i, j] = dataObj == null ? null : (string)dataObj;
                                }
                            }
                            return array;
                        }

                        return null;
                    case BuiltInType.DateTime:
                        if (matrix.Dimensions.Length == 2)
                        {
                            DateTime[,] array = new DateTime[matrix.Dimensions[0], matrix.Dimensions[1]];

                            int dLength = matrix.Dimensions.Length;

                            for (int i = 0; i < dLength; i++)
                            {
                                for (int j = 0; j < matrix.Dimensions[i]; j++)
                                {
                                    object dataObj = matrix.Elements.GetValue((i * matrix.Dimensions[1]) + j);
                                    array[i, j] = dataObj == null ? DateTime.MinValue : (DateTime)dataObj;
                                }
                            }
                            return array;
                        }

                        return null;
                    default: return null;
                }
            }
            catch (Exception except)
            {
                IronUtilites.LogManager.Manager.WriteLog("Exception", MethodBase.GetCurrentMethod() + " : " + except.ToString());
                return null;
            }
        }

        private NodeId FindNodeId(string nodestr, NodeId rootId = null)
        {

            if (m_browser == null)
            {
                InitBrowse();

            }

            // check if session is connected.
            if (m_browser == null || !m_browser.Session.Connected)
            {
                return null;
            }
            if (NodeId.IsNull(rootId))
            {
                rootId = Objects.ObjectsFolder;
            }

            if (m_browser != null)
            {
                //INode node = browser.Session.NodeCache.Find(rootId);
                INode node = m_browser.Session.NodeCache.Find(Objects.ObjectsFolder);

                if (node == null)
                {
                    return null;
                }


                // fetch references.
                ReferenceDescriptionCollection objectCollection = null;

                if (rootId != null)
                {
                    objectCollection = m_browser.Browse((NodeId)rootId);
                }
                else
                {
                    objectCollection = m_browser.Browse((NodeId)node.NodeId);
                }

                for (int i = 0; i < objectCollection.Count; i++)
                {
                    if (objectCollection[i].DisplayName.Text == nodestr)
                    {

                        return (NodeId)objectCollection[i].NodeId;
                    }
                }

                return null;
            }

            return null;
        }

        public void InitBrowse()
        {
            // check if session is connected.
            if (m_session == null || !m_session.Connected)
            {
                return;
            }

            Browser browser = new Browser(m_session);

            browser.BrowseDirection = BrowseDirection.Forward;
            browser.ReferenceTypeId = null;
            browser.IncludeSubtypes = true;
            browser.NodeClassMask = 0;
            browser.ContinueUntilDone = false;
            
            m_browser = browser;
        }

        //public string FindSubscriptTag(uint index)
        //{
        //    int f_index = m_monitoredInfo.FindIndex(0, index.ToString());
        //    if (f_index != -1)
        //    {
        //        return m_monitoredInfo.Items[1][f_index];
        //    }
        //    else
        //    {
        //        return "fail : " + index;
        //    }
        //}

        public class UserTokenItem
        {
            public UserTokenPolicy Policy;

            public UserTokenItem(UserTokenPolicy policy)
            {
                Policy = policy;
            }

            public UserTokenItem(UserTokenType tokenType)
            {
                Policy = new UserTokenPolicy(tokenType);
            }

            public override string ToString()
            {
                if (Policy != null)
                {
                    if (String.IsNullOrEmpty(Policy.PolicyId))
                    {
                        return Policy.TokenType.ToString();
                    }

                    return Utils.Format("{0} [{1}]", Policy.TokenType, Policy.PolicyId);
                }

                return UserTokenType.Anonymous.ToString();
            }
        }
        public class Protocol
        {
            public Uri Url;
            public string Profile;

            public Protocol(string url)
            {
                Url = Utils.ParseUri(url);
            }

            public Protocol(EndpointDescription url)
            {
                Url = null;

                if (url != null)
                {
                    Url = Utils.ParseUri(url.EndpointUrl);

                    if ((Url != null) && (Url.Scheme == Utils.UriSchemeHttps))
                    {
                        switch (url.TransportProfileUri)
                        {
                            case Profiles.HttpsBinaryTransport:
                                {
                                    Profile = "REST";
                                    break;
                                }
                        }
                    }
                }
            }

            public bool Matches(Uri url)
            {
                if (url == null || Url == null)
                {
                    return false;
                }

                if (url.Scheme != Url.Scheme)
                {
                    return false;
                }

                if (url.DnsSafeHost != Url.DnsSafeHost)
                {
                    return false;
                }

                if (url.Port != Url.Port)
                {
                    return false;
                }

                return true;
            }

            public override string ToString()
            {
                if (Url == null)
                {
                    return String.Empty;
                }

                StringBuilder builder = new StringBuilder();
                builder.Append(Url.Scheme);

                if (!String.IsNullOrEmpty(Profile))
                {
                    builder.Append(" ");
                    builder.Append(Profile);
                }

                builder.Append(" [");
                builder.Append(Url.DnsSafeHost);

                if (Url.Port != -1)
                {
                    builder.Append(":");
                    builder.Append(Url.Port);
                }

                builder.Append("]");

                return builder.ToString();
            }
        }
    }
}
