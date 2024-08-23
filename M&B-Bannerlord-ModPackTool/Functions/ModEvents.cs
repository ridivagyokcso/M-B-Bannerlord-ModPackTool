using M_B_Bannerlord_ModPackTool.ConfigFunctions;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Xml.Linq;
using IWshRuntimeLibrary;
using SevenZip;
using System.Reflection;
using Aspose.Zip.Rar;
using Aspose.Zip;
using System.Threading.Tasks;
using System.IO;
using System;
using System.Collections.Generic;
using System.Net.Http;

namespace M_B_Bannerlord_ModPackTool.Functions
{
    internal class ModEvents
    {
        public static async Task<bool> InstallingMods(Config config, string bannerlordPath)
        {
            string modFile = config.ModPackXmlFile;
            if (!System.IO.File.Exists(modFile)) {
                ModData modData = new ModData();
                modData.CreateModDataXml();
                modData.CreateModDataXmlFile(config);
                return false;
            }
            else
            {
                ModData modData = new ModData();
                if (modData.ValidateModData(config))
                {

                    string downloadPath = "downloads";
                    if (!Directory.Exists(downloadPath))
                    {
                        Directory.CreateDirectory(downloadPath);
                    }
                    else
                    {
                        Directory.Delete(downloadPath, true);
                        Directory.CreateDirectory(downloadPath);
                    }


                    LauncherData launcherData = new LauncherData();
                    launcherData.CreateEmptyLauncherXml();

                    XDocument mods = modData.GetModDataXml(config);
                    foreach (XElement Module in mods.Descendants("UserModData"))
                    {
                        string ModuleName = Module.Element("Name")?.Value;
                        string NexusModId = Module.Element("NexusModId")?.Value;
                        string NexusModFileId = Module.Element("NexusModFileId")?.Value;
                        string DataDir = Module.Element("DataDir")?.Value;
                        bool BaseModule = bool.Parse(Module.Element("BaseModule")?.Value ?? "false");
                        bool BLSE = bool.Parse(Module.Element("BLSE")?.Value ?? "false");
                        bool ModAssets = bool.Parse(Module.Element("ModAssets")?.Value ?? "false");
                        string ModAssetsModule = Module.Element("ModAssetsModule")?.Value;
                        bool CustomDownload = bool.Parse(Module.Element("CustomDownload")?.Value ?? "false");
                        string CustomDownloadUrl = Module.Element("CustomDownloadUrl")?.Value;

                        if (ModuleName == null)
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine($"The ModuleName is not specified in the XML structure.");
                            return false;
                        }

                        if (BaseModule)
                        {
                            string mbmodulesPath = Path.Combine(bannerlordPath, @"Modules");
                            string dirPath = Path.Combine(mbmodulesPath,ModuleName);
                            if (Directory.Exists(dirPath))
                            {
                                Console.ForegroundColor = ConsoleColor.Green;
                                Console.WriteLine($"The {ModuleName} is located in the modules folder.");
                            }
                            else
                            {
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine($"The {ModuleName} is not located in the modules folder. Please check the integrity of the game.");
                                return false;
                            }

                            
                            string submodulxml = Path.Combine(dirPath, @"SubModule.xml");

                            if (!System.IO.File.Exists(submodulxml))
                            {
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine($"SubModul file not found. Module: {ModuleName}");
                                return false;
                            }
                            XDocument doc = XDocument.Load(submodulxml);

                            XElement versionElement = doc.Root.Element("Version");
                            string versionValue = "v1.0.0";
                            if (versionElement != null)
                            {
                                versionValue = versionElement.Attribute("value")?.Value;
                            }

                            launcherData.CreateAddUserModData(ModuleName, versionValue, true);

                        }
                        else
                        {
                            if (NexusModId == null)
                            {
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine($"The NexusModId is not specified in the XML structure for the following mod:{ModuleName}");
                                return false;
                            }
                            if (NexusModFileId == null)
                            {
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine($"The NexusModFileId is not specified in the XML structure for the following mod:{ModuleName}");
                                return false;
                            }

                            List<string> downloadLinksList = new List<string>();
                            string[] downloadLinks = null;
                            if (CustomDownload)
                            {
                                downloadLinksList.Add(CustomDownloadUrl);
                                downloadLinks = downloadLinksList.ToArray();
                            }
                            else
                            {
                                downloadLinks = await GetDownloadLinksFromApi(NexusModId, NexusModFileId, config.NexusModManagerApiKey);
                            }

                            if (downloadLinks.Length == 0)
                            {
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine($"There is no download link for the mod: {ModuleName}");
                                return false;
                            }
                            else
                            {
                                string downloadlink = downloadLinks[0];
                                (string fileName, string fileExtension) = GetFileNameAndExtension(downloadlink);

                                string filePath = Path.Combine(downloadPath, fileName+fileExtension);

                                Console.ForegroundColor = ConsoleColor.Cyan;
                                Console.WriteLine($"Downloading: {ModuleName}");

                                try
                                {
                                    await DownloadFileAsync(downloadlink, filePath);
                                    Console.ForegroundColor = ConsoleColor.Green;
                                    Console.WriteLine($"{ModuleName} successfully downloaded.");
                                }
                                catch (Exception ex)
                                {
                                    Console.ForegroundColor = ConsoleColor.Red;
                                    Console.WriteLine($"An error occurred while downloading the file: {ex.Message}");
                                    return false;
                                }

                                string extractPath = @"tmp";
                                extractPath = Path.Combine(extractPath, ModuleName);


                                Console.ForegroundColor = ConsoleColor.Cyan;
                                Console.WriteLine($"Installing into the game: {ModuleName}");

                                if (!Directory.Exists(extractPath))
                                {
                                    Directory.CreateDirectory(extractPath);
                                }

                                var assemblyDllPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "7z.dll");
                                SevenZipExtractor.SetLibraryPath(assemblyDllPath);



                                if (IsRarFile(filePath))
                                {
                                    using (RarArchive archive = new RarArchive(filePath))
{
                                        archive.ExtractToDirectory(extractPath);
                                    }
                                }
                                else if (IsZipFile(filePath))
                                {
                                    using (FileStream zipFile = System.IO.File.Open(filePath, FileMode.Open))
                                    {
                                        using (var archive = new Archive(zipFile))
                                        {
                                            archive.ExtractToDirectory(extractPath);
                                        }
                                    }
                                }
                                else
                                {
                                    using (var extractor = new SevenZipExtractor(filePath))
                                    {
                                        extractor.ExtractArchive(extractPath);
                                    }
                                }

                                if (BLSE)
                                {
                                    if (DataDir == "-")
                                    {
                                        MoveDirectoryContents(extractPath, bannerlordPath);
                                        Console.ForegroundColor = ConsoleColor.Green;
                                        Console.WriteLine($"Successfully installed into the game: {fileName}");

                                    }
                                    else
                                    {
                                        string path = Path.Combine(extractPath, DataDir);
                                        MoveDirectoryContents(path, bannerlordPath);
                                        Console.ForegroundColor = ConsoleColor.Green;
                                        Console.WriteLine($"Successfully installed into the game: {fileName}");
                                    }


                                    string bmBinPath = Path.Combine(bannerlordPath, @"bin\Win64_Shipping_Client");

                                    string exePath = Path.Combine(bmBinPath, @"Bannerlord.BLSE.LauncherEx.exe");
                                    string shortcutName = "Bannerlord BLSE Launcher";

                                    string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
                                    string shortcutPath = Path.Combine(desktopPath, $"{shortcutName}.lnk");

                                    CreateShortcut(exePath, shortcutPath, shortcutName);

                                }else if (ModAssets)
                                {
                                    string modulesPath = Path.Combine(bannerlordPath, @"Modules");
                                    string modulePath = Path.Combine(modulesPath, ModAssetsModule);
                                    if (DataDir == "-")
                                    {
                                        MoveDirectoryContents(extractPath, modulePath);
                                        Console.ForegroundColor = ConsoleColor.Green;
                                        Console.WriteLine($"Successfully installed into the game: {fileName}");
                                    }
                                    else
                                    {
                                        string path = Path.Combine(extractPath, DataDir);
                                        MoveDirectoryContents(path, modulePath);
                                        Console.ForegroundColor = ConsoleColor.Green;
                                        Console.WriteLine($"Successfully installed into the game: {fileName}");
                                    }
                                }
                                else
                                {
                                    string modulesPath = Path.Combine(bannerlordPath, @"Modules");
                                    string modulePath = Path.Combine(modulesPath, ModuleName);
                                    if (DataDir == "-")
                                    {
                                        MoveDirectoryContents(extractPath, modulePath);
                                        Console.ForegroundColor = ConsoleColor.Green;
                                        Console.WriteLine($"Successfully installed into the game: {fileName}");
                                    }
                                    else
                                    {
                                        string path = Path.Combine(extractPath, DataDir);
                                        MoveDirectoryContents(path, modulePath);
                                        Console.ForegroundColor = ConsoleColor.Green;
                                        Console.WriteLine($"Successfully installed into the game: {fileName}");
                                    }

                                    string submodulxml = Path.Combine(modulePath, @"SubModule.xml");
                                    XDocument doc = XDocument.Load(submodulxml);

                                    XElement versionElement = doc.Root.Element("Version");
                                    string versionValue = "v1.0.0";
                                    if (versionElement != null)
                                    {
                                        versionValue = versionElement.Attribute("value")?.Value;
                                    }

                                    launcherData.CreateAddUserModData(ModuleName, versionValue, true);
                                }

                                if (Directory.Exists(extractPath))
                                {
                                    Directory.Delete(extractPath, true);
                                }
                            }
                        }
                    }
                    launcherData.CreateLauncherXmlFile();
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public static void ModOrderGenerate(Config config, string bannerlordPath)
        {
            ModData modData = new ModData();
            if (modData.ValidateModData(config))
            {
                LauncherData launcherData = new LauncherData();
                launcherData.CreateEmptyLauncherXml();

                int order = 1;

                XDocument mods = modData.GetModDataXml(config);
                foreach (XElement Module in mods.Descendants("UserModData"))
                {
                    string ModuleName = Module.Element("Name")?.Value;
                    string NexusModId = Module.Element("NexusModId")?.Value;
                    string NexusModFileId = Module.Element("NexusModFileId")?.Value;
                    string DataDir = Module.Element("DataDir")?.Value;
                    bool BaseModule = bool.Parse(Module.Element("BaseModule")?.Value ?? "false");
                    bool BLSE = bool.Parse(Module.Element("BLSE")?.Value ?? "false");
                    bool ModAssets = bool.Parse(Module.Element("ModAssets")?.Value ?? "false");
                    string ModAssetsModule = Module.Element("ModAssetsModule")?.Value;
                    bool CustomDownload = bool.Parse(Module.Element("CustomDownload")?.Value ?? "false");
                    string CustomDownloadUrl = Module.Element("CustomDownloadUrl")?.Value;

                    string modulesPath = Path.Combine(bannerlordPath, @"Modules");
                    string modulePath = Path.Combine(modulesPath, ModuleName);

                    string submodulxml = Path.Combine(modulePath, @"SubModule.xml");
                    XDocument doc = XDocument.Load(submodulxml);

                    XElement versionElement = doc.Root.Element("Version");
                    string versionValue = "v1.0.0";
                    if (versionElement != null)
                    {
                        versionValue = versionElement.Attribute("value")?.Value;
                    }

                    launcherData.CreateAddUserModData(ModuleName, versionValue, true);
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"{order}: {ModuleName}");

                    order++;
                }

                launcherData.CreateLauncherXmlFile();

            }
        }

        private static async Task<string[]> GetDownloadLinksFromApi(string modId, string modFileId, string apiKey)
        {
            string url = $"https://api.nexusmods.com/v1/games/mountandblade2bannerlord/mods/{modId}/files/{modFileId}/download_link.json";

            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Add("apikey", apiKey);

                try
                {
                    HttpResponseMessage response = await client.GetAsync(url);
                    response.EnsureSuccessStatusCode();


                    string responseBody = await response.Content.ReadAsStringAsync();

                    // JSON válasz feldolgozása
                    var downloadLinks = new List<string>();
                    using (JsonDocument doc = JsonDocument.Parse(responseBody))
                    {
                        foreach (var element in doc.RootElement.EnumerateArray())
                        {
                            if (element.TryGetProperty("URI", out var uriProperty))
                            {
                                downloadLinks.Add(uriProperty.GetString());
                            }
                            else
                            {
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine("The response does not contain a URI field.");
                            }
                        }
                    }
                    return downloadLinks.ToArray();
                }
                catch (HttpRequestException e)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"HTTP request error: {e.Message}");
                    return Array.Empty<string>();
                }
                catch (JsonException e)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"JSON processing error: {e.Message}");
                    return Array.Empty<string>();
                }
                catch (Exception e)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"General error: {e.Message}");
                    return Array.Empty<string>();
                }
            }
        }
        private static async Task DownloadFileAsync(string url, string outputPath)
        {
            using (HttpClient client = new HttpClient())
            {
                HttpResponseMessage response = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
                response.EnsureSuccessStatusCode();

                using (Stream responseStream = await response.Content.ReadAsStreamAsync())
                {
                    using (FileStream fileStream = new FileStream(outputPath, FileMode.Create, FileAccess.Write))
                    {
                        await responseStream.CopyToAsync(fileStream);
                    }
                }
            }
        }
        private static (string fileName, string fileExtension) GetFileNameAndExtension(string url)
        {
            // URL bontása
            Uri uri = new Uri(url);
            string path = uri.AbsolutePath;

            // Fájl név és kiterjesztés
            string fileName = Path.GetFileNameWithoutExtension(path);
            string fileExtension = Path.GetExtension(path);

            return (fileName, fileExtension);
        }


        private static bool IsZipFile(string filePath)
        {
            byte[] buffer = new byte[4];
            using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                fs.Read(buffer, 0, 4);
            }
            return buffer[0] == 0x50 && buffer[1] == 0x4B && buffer[2] == 0x03 && buffer[3] == 0x04;
        }

        private static bool IsGZipFile(string filePath)
        {
            byte[] buffer = new byte[2];
            using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                fs.Read(buffer, 0, 2);
            }
            return buffer[0] == 0x1F && buffer[1] == 0x8B;
        }

        private static bool IsTarFile(string filePath)
        {
            byte[] buffer = new byte[5];
            using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                fs.Read(buffer, 0, 5);
            }
            return buffer[0] == 0x75 && buffer[1] == 0x73 && buffer[2] == 0x74 && buffer[3] == 0x61 && buffer[4] == 0x72;
        }

        private static bool IsRarFile(string filePath)
        {
            byte[] buffer = new byte[4];
            using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                fs.Read(buffer, 0, 4);
            }
            return buffer[0] == 0x52 && buffer[1] == 0x61 && buffer[2] == 0x72 && buffer[3] == 0x21;
        }

        private static bool Is7zFile(string filePath)
        {
            byte[] buffer = new byte[6];
            using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                fs.Read(buffer, 0, 6);
            }
            return buffer[0] == 0x37 && buffer[1] == 0x7A && buffer[2] == 0xBC && buffer[3] == 0xAF && buffer[4] == 0x27 && buffer[5] == 0x1C;
        }

        private static void MoveDirectoryContents(string sourceDir, string destDir)
        {
            if (!Directory.Exists(sourceDir))
            {
                throw new DirectoryNotFoundException($"The source folder not found: {sourceDir}");
            }

            if (!Directory.Exists(destDir))
            {
                Directory.CreateDirectory(destDir);
            }

            foreach (string filePath in Directory.GetFiles(sourceDir))
            {
                string fileName = Path.GetFileName(filePath);
                string destFilePath = Path.Combine(destDir, fileName);
                System.IO.File.Move(filePath, destFilePath);
            }

            foreach (string dirPath in Directory.GetDirectories(sourceDir))
            {
                string dirName = Path.GetFileName(dirPath);
                string destDirPath = Path.Combine(destDir, dirName);
                MoveDirectoryContents(dirPath, destDirPath);
            }
            Directory.Delete(sourceDir);
        }

        private static void CreateShortcut(string exePath, string shortcutPath, string shortcutName)
        {
            var shell = new WshShell();
            var shortcut = (IWshShortcut)shell.CreateShortcut(shortcutPath);
            shortcut.TargetPath = exePath;
            shortcut.WorkingDirectory = Path.GetDirectoryName(exePath);
            shortcut.IconLocation = exePath;
            shortcut.Description = $"Parancsikon az {shortcutName} alkalmazáshoz";
            shortcut.Save();
            Console.WriteLine("Shortcut successfully created.");
        }
    }
}
