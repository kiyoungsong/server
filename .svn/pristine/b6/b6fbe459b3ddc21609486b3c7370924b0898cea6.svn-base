using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IronWeb.Shared
{
    public partial class SideBar
    {
        [Inject]
        NavigationManager Url { get; set; }

        [Parameter]
        public bool Collapsed { get; set; }
        
        RenderFragment channelTitle;

        [Parameter]
        public EventCallback<string> SendPath { get; set; }

        protected void ChangedPage(string pageName)
        {
            if (pageName == "Index")
            {
                SendPath.InvokeAsync(pageName);
            }
            else
            {
                Data.SettingInfo.SelectedDriverName = pageName;
                SendPath.InvokeAsync(pageName);
            }
        }
    }
}
