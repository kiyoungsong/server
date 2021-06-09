using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
namespace IronWeb.Pages
{
    public partial class BtnFunction
    {
        IronClient.IronOPCUAClient ironClient;

        [Parameter]
        public EventCallback<List<string>> EditSettingInfo { get; set; }

        [Parameter]
        public EventCallback<IronClient.IronOPCUAClient> ClientObject { get; set; }

        public bool IsConnected { get; set; } = false;

        protected override void OnInitialized()
        {
            base.OnInitialized();

            if(ironClient == null)
            {
                Data.SettingInfo.IsConnect = false;
                IsConnected = false;

                // Index Page Loading to Connect
                //InvokeAsync(Connect);
            }
            else
            {
                Data.SettingInfo.IsConnect = ironClient.IsConnected;
                IsConnected = ironClient.IsConnected;
            }

            ClientObject.InvokeAsync(ironClient);
        }

        protected void Connect()
        {
            if(ironClient == null)
            {
                ironClient = new IronClient.IronOPCUAClient();
                ironClient.ServerUrl = $"opc.tcp://{Data.SettingInfo.ServerAddr}:{Data.SettingInfo.Port}";
                ironClient.SessionID = System.Security.Principal.WindowsIdentity.GetCurrent().Name;
                ironClient.NamespaceNumber = 2;
            }

            if (!ironClient.IsConnected)
            {
                ironClient.Connect();

                if (ironClient.IsConnected)
                {
                    IsConnected = true;
                    Data.SettingInfo.IsConnect = ironClient.IsConnected;
                    ClientObject.InvokeAsync(ironClient);
                    ironClient.StatusNotificationEvent += CheckServerStatus;

                    Data.LogManager.WriteLog($"Connected, Server : {ironClient.ServerUrl}", false);
                }
                else
                {
                    Data.LogManager.WriteLog($"Failed Connecting, Server : {ironClient.ServerUrl}", false);
                }
            }
            else
            {
                Data.LogManager.WriteLog($"Already Connected, Server : {ironClient.ServerUrl}", false);
            }
        }

        protected void Disconnect()
        {
            if(ironClient != null)
            {
                ironClient.Disconnect();

                if (!ironClient.IsConnected)
                {
                    ironClient.StatusNotificationEvent -= CheckServerStatus;

                    IsConnected = false;
                    Data.SettingInfo.IsConnect = ironClient.IsConnected;
                    Data.DriverManager.ResetTags();
                    ClientObject.InvokeAsync(ironClient);

                    Data.LogManager.WriteLog($"Disconnected, Server : {ironClient.ServerUrl}", false);
                }
                else
                {
                    Data.LogManager.WriteLog($"Failed Disconnecting, Server : {ironClient.ServerUrl}", false);
                }
            }
            else
            {
                Data.LogManager.WriteLog($"Already Disconnected, Server : {ironClient.ServerUrl}", false);
            }
        }

        protected void CheckServerStatus(string status)
        {
            if(status == "Good")
            {
                if(ironClient != null)
                {
                    Data.SettingInfo.IsConnect = true;
                }
            }
            else if(status == "Bad")
            {
                if (ironClient != null)
                {
                    Data.SettingInfo.IsConnect = false;
                    InvokeAsync(Disconnect);
                    InvokeAsync(StateHasChanged);
                }
            }
        }

        void OpenDialog()
        {
            List<string> settingList = new List<string>();
            settingList.Add("true");
            settingList.Add(Data.SettingInfo.ServerAddr);
            settingList.Add(Data.SettingInfo.Port.ToString());
            settingList.Add(Data.SettingInfo.ClientAddr);
            EditSettingInfo.InvokeAsync(settingList);
        }
    }
}
