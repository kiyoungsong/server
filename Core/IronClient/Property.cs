using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using Opc.Ua;
using Opc.Ua.Client;
using Opc.Ua.Configuration;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.ComponentModel.Design;

namespace IronClient
{
    public partial class IronOPCUAClient
    {
        public delegate void Session_NotificationArray(string[] tags, object[] values, string[] dataTypes, DateTime[] timeStemp, DateTime[] serverTime, string[] status);
        public delegate void Session_StatusNotification(string status);

        public event Session_NotificationArray ArrayChangeEvent = null;
        public event Session_StatusNotification StatusNotificationEvent = null;
        
        ApplicationInstance application = new ApplicationInstance();
        ConfiguredEndpoint m_endpoint;
        Session m_session;

        Encoding m_encoding = Encoding.Default;
        enum_SecurityMode securitymode = enum_SecurityMode.None;
        enum_SecurityPolicy securitypolicy = enum_SecurityPolicy.None;
        UserTokenType usertokentype = UserTokenType.UserName;
        
        private Dictionary<string, string[]> SecurityInfo = new Dictionary<string, string[]>();
        private List<string> SecurityInfo_m = new List<string>();
        private List<string> SecurityInfo_p = new List<string>();
        //private DynamicList<string> m_monitoredInfo = new DynamicList<string>();

        private Dictionary<uint, string> m_monitoredIdxTag = new Dictionary<uint, string>();
        private Dictionary<string, object> m_monitoredTagValue = new Dictionary<string, object>();

        private List<MonitoredItem> m_monitoredItem = new List<MonitoredItem>();
        private ServiceMessageContext m_messageContext;
        private SessionReconnectHandler m_reconnectHandler;
        private IList<string> m_preferredLocales;
        private ApplicationConfiguration m_configuration;
        private Dictionary<string, UserIdentityToken> m_userIdentities = new Dictionary<string, UserIdentityToken>();
        private EndpointDescriptionCollection m_availableEndpoints;
        private EndpointDescription m_currentDescription;
        private EndpointConfiguration m_endpointConfiguration;
        //private EndpointComIdentity m_comIdentity;
        //private Dictionary<object, TreeNode> m_eventRegistrations;
        //private EventHandler m_SessionSubscriptionsChanged;
        private SubscriptionStateChangedEventHandler m_SubscriptionStateChanged;
        private EventHandler m_PublishStatusChanged;
        private NotificationEventHandler m_SessionNotification;
        private Uri m_discoveryUrl;
        private Subscription m_subscription = new Subscription();
        private ushort namespacenum = 2;
        private string username, password = "";
        private string sessionId = "My Session 1";
        private string protocol = "";
        private int m_discoverCount;
        private bool m_checkDomain = true;
        private int m_reconnectPeriod = 10;
        private bool m_updating = false;
        private bool m_discoverySucceeded;
        private int m_discoveryTimeout = 0;

        private string serverUrl = "";
        private int publishingInterval = 500;
        private uint keepAliveCount = 10;
        private uint lifetimeCount = 1000;
        private uint maxNotifications = 0;
        private byte priority = 255;
        private bool publishingenable = true;
        private uint subscribeId = 1;

        Browser m_browser = null;


        public enum enum_SecurityMode
        {
            None,
            Sign,
            SignAndEncrypt
        }
        public enum Encoding
        {
            Default,
            Xml,
            Binary
        }
        public enum enum_SecurityPolicy
        {
            None,
            Basic128Rsa15,
            Basic256,
            Basic256Sha256
        }
        public enum UserToken
        {
            Anonymous = 0,
            UserName = 1,
            Certificate = 2,
            IssuedToken = 3
        }

        public ushort NamespaceNumber
        {
            get { return namespacenum; }
            set { namespacenum = value; }
        }
        public IList<string> PreferredLocales
        {
            get { return m_preferredLocales; }
            set { m_preferredLocales = value; }
        }
        public String ProtocolName
        {
            get { return protocol; }
            set
            {
                protocol = value;
            }
        }
        public string ServerUrl
        {
            get {
                if(serverUrl == "")
                {
                    return "opc.tcp://192.168.0.14:48020";
                }
                return serverUrl; }
            set
            {
                serverUrl = value;
                //Init();
            }
        }
        public Session mSession
        {
            get { return m_session; }
        }
        public Encoding Encodings
        {
            get { return m_encoding; }
            set { m_encoding = value; }
        }
        public string SessionID
        {
            get { return sessionId; }
            set { sessionId = value; }
        }
        public string Username
        {
            get { return username; }
            set { username = value; }
        }
        public string Password
        {
            get { return password; }
            set { password = value; }
        }

        public int PublishingInterval
        {
            get { return publishingInterval; }
            set { publishingInterval = value; }
        }

        public UserToken UserTypeToken
        {
            get
            {
                UserToken user = (UserToken)usertokentype;
                return user;
            }
            set
            {
                UserToken user = value;
                usertokentype = (UserTokenType)user;
            }
        }

        string _ItemMode = "";
        string _ItemPolicy = "";
        internal class HE_GlobalVars
        {
            internal static string[] _ListofRules;
            internal static string[] _ListofRules2;
        }

        public class RuleConverter : StringConverter
        {
            public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
            {
                return true;
            }

            public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
            {
                return true;
            }

            public override System.ComponentModel.TypeConverter.StandardValuesCollection
                   GetStandardValues(ITypeDescriptorContext context)
            {
                return new StandardValuesCollection(HE_GlobalVars._ListofRules);
            }
        }
        public class RuleConverter2 : StringConverter
        {
            public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
            {
                return true;
            }

            public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
            {
                return true;
            }

            public override System.ComponentModel.TypeConverter.StandardValuesCollection
                   GetStandardValues(ITypeDescriptorContext context)
            {
                return new StandardValuesCollection(HE_GlobalVars._ListofRules2);
            }
        }

        public Dictionary<string, object> MonitoredTagValue { get => m_monitoredTagValue; set => m_monitoredTagValue = value; }

        [Browsable(true)]
        [TypeConverter(typeof(RuleConverter))]
        public string SecurityMODE
        {
            get
            {
            //    if (serverUrl != "")
            //    {
            //        updatePropertyItem();
            //    }

                string S = "";
                if (_ItemMode != null)
                {
                    S = _ItemMode;
                }
                else
                {
                    if (HE_GlobalVars._ListofRules.Length > 0)
                    {
                        Array.Sort(HE_GlobalVars._ListofRules);
                        S = HE_GlobalVars._ListofRules[0];
                    }
                }
                return S;
            }
            set
            {
                _ItemMode = value;

                if (enum_SecurityMode.None.ToString() == value)
                {
                    securitymode = enum_SecurityMode.None;
                }
                else if (enum_SecurityMode.Sign.ToString() == value)
                {
                    securitymode = enum_SecurityMode.Sign;
                }
                else if (enum_SecurityMode.SignAndEncrypt.ToString() == value)
                {
                    securitymode = enum_SecurityMode.SignAndEncrypt;
                }

                if (SecurityInfo.ContainsKey(_ItemMode))
                {

                    HE_GlobalVars._ListofRules2 = SecurityInfo[_ItemMode];
                }
                _ItemPolicy = "";



            }
        }
        [Browsable(true)]
        [TypeConverter(typeof(RuleConverter2))]
        public string SecurityPOLICY
        {
            get
            {
                string S = "";
                if (_ItemPolicy != null)
                {
                    S = _ItemPolicy;
                }
                else
                {
                    if (HE_GlobalVars._ListofRules2.Length > 0)
                    {
                        Array.Sort(HE_GlobalVars._ListofRules2);
                        S = HE_GlobalVars._ListofRules2[0];

                    }
                }
                

                return S;
            }
            set
            {
                _ItemPolicy = value;

                if (enum_SecurityPolicy.None.ToString() == value)
                {
                    securitypolicy = enum_SecurityPolicy.None;
                }
                else if (enum_SecurityPolicy.Basic128Rsa15.ToString() == value)
                {
                    securitypolicy = enum_SecurityPolicy.Basic128Rsa15;
                }
                else if (enum_SecurityPolicy.Basic256.ToString() == value)
                {
                    securitypolicy = enum_SecurityPolicy.Basic256;
                }
                else if (enum_SecurityPolicy.Basic256Sha256.ToString() == value)
                {
                    securitypolicy = enum_SecurityPolicy.Basic256Sha256;
                }
            }
        }
    }
}
