using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml.Serialization;

namespace IronUtilites
{
    public class Serialize
    {
        public static void SerializeXML(string dirName, string fileName, Type type, object obj)
        {
            if (dirName.Length == 0)
            {
                dirName = ".\\";
            }

            if (dirName.LastIndexOf("\\") != dirName.Length - 1)
            {
                dirName += "\\";
            }

            DirectoryInfo dir = new DirectoryInfo(dirName);

            if (dir.Exists == false)
            {
                dir.Create();
            }

            using (Stream stream = new FileStream(dirName + fileName, FileMode.Create, FileAccess.Write))
            {
                XmlSerializer ser = new XmlSerializer(type);
                ser.Serialize(stream, obj);
            }
        }

        public static object DeserializeXML(string fileName, Type type)
        {
            FileInfo file = new FileInfo(fileName);

            if (file.Exists)
            {
                try
                {
                    using (Stream stream = new FileStream(fileName, FileMode.Open, FileAccess.Read))
                    {
                        XmlSerializer ser = new XmlSerializer(type);
                        return ser.Deserialize(stream);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }

            return null;
        }
    }
}
