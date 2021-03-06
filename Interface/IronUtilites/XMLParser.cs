using System;
using System.Diagnostics;
using System.IO;
using System.Xml.Serialization;

namespace IronUtilites
{
    public class XMLParser
    {
        public static void SerializeXML(string dirName, string fileName, Type type, object obj)
        {
            if (dirName.Length == 0)
            {
                dirName = "." + Path.DirectorySeparatorChar;
            }

            if (dirName.LastIndexOf(Path.DirectorySeparatorChar) != dirName.Length - 1)
            {
                dirName += Path.DirectorySeparatorChar;
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
                    IronUtilites.LogManager.Manager.WriteLog("Exception", "DeserializeXML Failed : " + e.Message);
                    Debug.WriteLine("DeserializeXML Failed : " + e.Message);
                }
            }

            return null;
        }
    }
}
