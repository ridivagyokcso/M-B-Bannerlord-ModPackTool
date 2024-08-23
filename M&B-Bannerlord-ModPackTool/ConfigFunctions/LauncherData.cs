using System;
using System.IO;
using System.Xml.Linq;

namespace M_B_Bannerlord_ModPackTool.ConfigFunctions
{
    internal class LauncherData
    {

        private XDocument launcherData;

        public void CreateLauncherXml()
        {
            launcherData = new XDocument(
                new XDeclaration("1.0", "utf-8", "yes"),
                new XElement("UserData",
                    new XAttribute(XNamespace.Xmlns + "xsd", "http://www.w3.org/2001/XMLSchema"),
                    new XAttribute(XNamespace.Xmlns + "xsi", "http://www.w3.org/2001/XMLSchema-instance"),
                    new XElement("GameType", "Singleplayer"),
                    new XElement("SingleplayerData",
                        new XElement("ModDatas",
                            CreateUserModData("Native", "v1.2.9.38256", true),
                            CreateUserModData("SandBoxCore", "v1.2.9.38256", true),
                            CreateUserModData("BirthAndDeath", "v1.2.9.38256", true),
                            CreateUserModData("CustomBattle", "v1.2.9.38256", true),
                            CreateUserModData("Sandbox", "v1.2.9.38256", true),
                            CreateUserModData("StoryMode", "v1.2.9.38256", true)
                        )
                    ),
                    new XElement("MultiplayerData",
                        new XElement("ModDatas")
                    ),
                    new XElement("DLLCheckData",
                        new XElement("DLLData")
                    )
                )
            );
        }

        public void CreateEmptyLauncherXml()
        {
            launcherData = new XDocument(
                new XDeclaration("1.0", "utf-8", "yes"),
                new XElement("UserData",
                    new XAttribute(XNamespace.Xmlns + "xsd", "http://www.w3.org/2001/XMLSchema"),
                    new XAttribute(XNamespace.Xmlns + "xsi", "http://www.w3.org/2001/XMLSchema-instance"),
                    new XElement("GameType", "Singleplayer"),
                    new XElement("SingleplayerData",
                        new XElement("ModDatas")
                    ),
                    new XElement("MultiplayerData",
                        new XElement("ModDatas")
                    ),
                    new XElement("DLLCheckData",
                        new XElement("DLLData")
                    )
                )
            );
        }

        public XDocument GetLauncherXml()
        {
            return launcherData;
        }

        public void CreateLauncherXmlFile() {
            string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string mbdocPath = Path.Combine(documentsPath, @"Mount and Blade II Bannerlord");
            string configsPath = Path.Combine(mbdocPath, @"Configs");
            string filePath = Path.Combine(configsPath, @"LauncherData.xml");

            if (!Directory.Exists(mbdocPath))
            {
                Directory.CreateDirectory(mbdocPath);
            }

            if (!Directory.Exists(configsPath))
            {
                Directory.CreateDirectory(configsPath);
            }

            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            launcherData.Save(filePath);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("LauncherData file creation completed.");
        }

        public void CreateAddUserModData(string id, string version, bool isSelected)
        {
            XElement newModData = new XElement("UserModData",
                new XElement("Id", id),
                new XElement("LastKnownVersion", version),
                new XElement("IsSelected", isSelected)
            );

            XElement singleplayerModDatas = launcherData.Root
                .Element("SingleplayerData")
                .Element("ModDatas");

            singleplayerModDatas.Add(newModData);
        }

        private static XElement CreateUserModData(string id, string version, bool isSelected)
        {
            return new XElement("UserModData",
                new XElement("Id", id),
                new XElement("LastKnownVersion", version),
                new XElement("IsSelected", isSelected.ToString().ToLower())
            );
        }

    }
}
