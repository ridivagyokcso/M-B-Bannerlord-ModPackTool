using System;
using System.Linq;
using System.Xml.Linq;

namespace M_B_Bannerlord_ModPackTool.ConfigFunctions
{
    internal class ModData
    {
        private XDocument modData;

        public void CreateModDataXml()
        {
            modData = new XDocument(
                new XDeclaration("1.0", "utf-8", "yes"),
                new XElement("ModData",
                    new XAttribute(XNamespace.Xmlns + "xsd", "http://www.w3.org/2001/XMLSchema"),
                    new XAttribute(XNamespace.Xmlns + "xsi", "http://www.w3.org/2001/XMLSchema-instance"),
                    new XElement("ModDatas",
                        CreateModData("Native", "", "", "Native", false, "", false, "", true, false),
                        CreateModData("SandBoxCore", "", "", "SandBoxCore", false, "", false, "", true, false),
                        CreateModData("BirthAndDeath", "", "", "BirthAndDeath", false, "", false, "", true, false),
                        CreateModData("CustomBattle", "", "", "CustomBattle", false, "", false, "", true, false),
                        CreateModData("Sandbox", "", "", "Sandbox", false, "", false, "", true, false),
                        CreateModData("StoryMode", "", "", "StoryMode", false, "", false, "", true, false)
                    )
                )
            );
        }

        public bool ValidateModData(Config config)
        {
            string filePath = config.ModPackXmlFile;
            try
            {
                XDocument modXml = XDocument.Load(filePath);
                if (IsValidLauncherData(modXml))
                {
                    return true;
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"The {filePath} XML structure is invalid.");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"An error occurred while reading the {filePath} XML: " + ex.Message);
                return false;
            }
        }

        public XDocument GetModDataXml(Config config)
        {
            string filePath = config.ModPackXmlFile;
            XDocument modXml = XDocument.Load(filePath);
            modData = modXml;
            return modData;
        }

        public void CreateModDataXmlFile(Config config)
        {
            string filePath = config.ModPackXmlFile;

            modData.Save(filePath);

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"{config.ModPackXmlFile} file successfully created!");
        }

        private static XElement CreateModData(string name, string modid, string modfileid, string datadir, bool customdownload, string customdownloadurl, bool assets, string modassetsmodule, bool basemodule, bool blse)
        {
            return new XElement("UserModData",
                new XElement("Name", name),
                new XElement("NexusModId", modid),
                new XElement("NexusModFileId", modfileid),
                new XElement("DataDir", datadir),
                new XElement("CustomDownload", customdownload.ToString().ToLower()),
                new XElement("CustomDownloadUrl", customdownloadurl),
                new XElement("ModAssets", assets.ToString().ToLower()),
                new XElement("ModAssetsModule", modassetsmodule),
                new XElement("BaseModule", basemodule.ToString().ToLower()),
                new XElement("BLSE", blse.ToString().ToLower())
            );
        }

        private static bool IsValidLauncherData(XDocument doc)
        {
            XElement root = doc.Root;
            if (root == null)
                return false;

            XElement modDatas = root.Element("ModDatas");
            if (modDatas == null)
                return false;

            if (!modDatas.Elements("UserModData").Any())
                return false;

            return true;
        }
    }
}
