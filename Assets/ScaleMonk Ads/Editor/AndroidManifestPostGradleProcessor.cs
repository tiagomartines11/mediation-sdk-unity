using System.IO;
using System.Text;
using System.Xml;
using UnityEditor.Android;

namespace ScaleMonk_Ads.Editor
{
    public class AndroidManifestPostGradleProcessor : IPostGenerateGradleAndroidProject
    {
        public readonly string AppIdKey = "com.scalemonk.libs.ads.applicationId";
        public readonly string AppIdValue = "sm-test-app-scalemonk-6521838781";

        public void OnPostGenerateGradleAndroidProject(string basePath)
        {
            // If needed, add condition checks on whether you need to run the modification routine.
            // For example, specific configuration/app options enabled

            var androidManifest = new AndroidManifest(GetManifestPath(basePath));

            // androidManifest.SetApplicationTheme(ThemeName);
            androidManifest.SetApplicationIdMetadata(AppIdKey, AppIdValue);
            // Add your XML manipulation routines

            androidManifest.Save();
        }

        public int callbackOrder => 1;

        private string _manifestFilePath;

        private string GetManifestPath(string basePath)
        {
            if (string.IsNullOrEmpty(_manifestFilePath))
            {
                var pathBuilder = new StringBuilder(basePath);
                pathBuilder.Append(Path.DirectorySeparatorChar).Append("src");
                pathBuilder.Append(Path.DirectorySeparatorChar).Append("main");
                pathBuilder.Append(Path.DirectorySeparatorChar).Append("AndroidManifest.xml");
                _manifestFilePath = pathBuilder.ToString();
            }
            return _manifestFilePath;
        }
    }


    internal class AndroidXmlDocument : XmlDocument
    {
        private string m_Path;
        protected XmlNamespaceManager nsMgr;
        public readonly string AndroidXmlNamespace = "http://schemas.android.com/apk/res/android";
        public AndroidXmlDocument(string path)
        {
            m_Path = path;
            using (var reader = new XmlTextReader(m_Path))
            {
                reader.Read();
                Load(reader);
            }
            nsMgr = new XmlNamespaceManager(NameTable);
            nsMgr.AddNamespace("android", AndroidXmlNamespace);
        }

        public string Save()
        {
            return SaveAs(m_Path);
        }

        public string SaveAs(string path)
        {
            using (var writer = new XmlTextWriter(path, new UTF8Encoding(false)))
            {
                writer.Formatting = Formatting.Indented;
                Save(writer);
            }
            return path;
        }
    }


    internal class AndroidManifest : AndroidXmlDocument
    {
        private readonly XmlElement ApplicationElement;

        public AndroidManifest(string path) : base(path)
        {
            ApplicationElement = SelectSingleNode("/manifest/application") as XmlElement;
        }

        private XmlAttribute CreateAndroidAttribute(string key, string value)
        {
            XmlAttribute attr = CreateAttribute("android", key, AndroidXmlNamespace);
            attr.Value = value;
            return attr;
        }
    
        private XmlElement CreateAndroidElement(string key)
        {
            XmlElement elem = CreateElement(key, AndroidXmlNamespace);
            return elem;
        }

        internal void SetApplicationIdMetadata(string appIdKey, string appIdValue)
        {
            XmlElement metadataElem = CreateAndroidElement("meta-data");
            ApplicationElement.AppendChild(metadataElem);
            metadataElem.Attributes.Append(CreateAndroidAttribute("name", appIdKey));
            metadataElem.Attributes.Append(CreateAndroidAttribute("value", appIdValue));
        
        }
    }
}