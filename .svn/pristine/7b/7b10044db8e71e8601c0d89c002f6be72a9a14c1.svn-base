using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IronWeb.Helpers
{
    public static class IJSExtensions
    {
        public async static void SaveFile(this IJSRuntime js, string fileName, byte[] contents)
        {
            await js.InvokeAsync<object>("saveAsFile", fileName, Convert.ToBase64String(contents));
        }
    }
}
