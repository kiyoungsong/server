using BlazorInputFile;
using IronWeb.Helpers;
using Microsoft.AspNetCore.Hosting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace IronWeb.Data
{
    public class FileUpload : IFileUpload
    {
        public async Task<string> Upload(IFileListEntry file)
        {
            using(MemoryStream memoryStream = new MemoryStream())
            {
                await file.Data.CopyToAsync(memoryStream);
                memoryStream.Position = 0;

                using (var reader = new StreamReader(memoryStream))
                {
                    return reader.ReadToEnd();
                }
            }
        }
    }
}
