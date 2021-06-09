using IronUtilites;
using IronWeb.Data;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace IronWeb.Shared
{
    public partial class MainLayout
    {
        public string XmlPath { get; set; } = "";
        bool collapsed = false;

        protected override void OnInitialized()
        {
            DriverManager.LoadXML();
        }

        protected void CollapsedChanged(bool collapsed)
        {
            this.collapsed = collapsed;
        }

        protected void ChangePath(string Name)
        {
            XmlPath = Name;
        }
    }
}
