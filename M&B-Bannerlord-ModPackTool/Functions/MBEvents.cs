using System.Text;
using System.Security.Cryptography;
using System.Xml.Linq;
using System.Runtime.InteropServices;
using System.IO;
using System.Collections.Generic;
using System;

namespace M_B_Bannerlord_ModPackTool.Functions
{
    internal class MBEvents
    {
        public static bool MBInstalled(string bannerlordPath)
        {
            if (Directory.Exists(bannerlordPath))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static bool VerifyFiles(string hashfile, string bannerlordPath) {
            Dictionary<string, string> hashDictionary = LoadHashFile(hashfile);
            if (hashDictionary != null) {
                string directoryPath2 = Path.Combine(bannerlordPath, @"bin\Win64_Shipping_Client");
                string versionFilePath = Path.Combine(directoryPath2, "Version.xml");

                XDocument xdoc = XDocument.Load(versionFilePath);
                var versionElement = xdoc.Root.Element("Singleplayer");
                string version = versionElement.Attribute("Value")?.Value;

                int hcount = 1;
                int error = 0;
                foreach (var hash in hashDictionary)
                {
                    if (hcount == 1)
                    {
                        if (version != hash.Value) {
                            error = 1;
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine("The game version number differs from the version number found in the hash file!");
                        }
                    }
                    else
                    {
                        if (error == 0)
                        {
                            string file = Path.Combine(bannerlordPath, hash.Key);
                            if (File.Exists(file))
                            {
                                string fileHash = ComputeFileHash(file);
                                if (fileHash == hash.Value)
                                {
                                    Console.ForegroundColor = ConsoleColor.Green;
                                    Console.WriteLine($"{hash.Key} successfully checked and valid.");
                                }
                                else
                                {
                                    Console.ForegroundColor = ConsoleColor.Red;
                                    Console.WriteLine($"{hash.Key} successfully checked and invalid.");
                                }
                            }
                            else
                            {
                                error = 1;
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine($"{hash.Key} not found.");
                            }
                        }
                    }
                    hcount++;
                }

                if (error == 1)
                {
                    return false;
                }
                else
                {
                    return true;
                }

            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("The hash file content is empty, so the verification is skipped!");
                return true;
            }
        }

        public static void CleanInstallation(string hashfile, string bannerlordPath)
        {
            Dictionary<string, string> hashDictionary = LoadHashFile(hashfile);
            foreach (string file in Directory.GetFiles(bannerlordPath, "*.*", SearchOption.AllDirectories))
            {
                string filec = file.Replace(bannerlordPath+"\\","");
                if (!hashDictionary.ContainsKey(filec))
                {
                    File.Delete(file);
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"{filec} deleted from the installation folder.");
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"{filec} included in the hash collection, so it will not be deleted.");
                }
            }
        }

        public static void CleanDocumentFolder()
        {
            string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string mbdocPath = Path.Combine(documentsPath, @"Mount and Blade II Bannerlord");

            if (Directory.Exists(mbdocPath))
            {
                Directory.Delete(mbdocPath, true);
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"The game's folder has been successfully deleted from Documents.");
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"The game does not have a folder in the Documents, so deletion is not necessary.");
            }
        }

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool DeleteFile(string path);

        public static void UnblockDllFiles(string directory)
        {
            foreach (var file in Directory.GetFiles(directory, "*.dll", SearchOption.AllDirectories))
            {
                string zoneIdentifier = file + ":Zone.Identifier";
                if (File.Exists(zoneIdentifier))
                {
                    if (DeleteFile(zoneIdentifier))
                    {
                        Console.WriteLine($"Unlocked: {file}");
                    }
                    else
                    {
                        Console.WriteLine($"Failed to unlock: {file}");
                    }
                }
            }
        }

        private static string ComputeFileHash(string filePath)
        {
            using (var hashAlgorithm = SHA256.Create())
            {
                using (var stream = File.OpenRead(filePath))
                {
                    byte[] hashBytes = hashAlgorithm.ComputeHash(stream);
                    StringBuilder hashString = new StringBuilder();
                    foreach (byte b in hashBytes)
                    {
                        hashString.Append(b.ToString("x2"));
                    }
                    return hashString.ToString();
                }
            }
        }

        private static Dictionary<string, string> LoadHashFile(string filePath)
        {
            var hashDict = new Dictionary<string, string>();

            if (File.Exists(filePath))
            {
                try
                {
                    int line_count = 1;

                    foreach (var line in File.ReadLines(filePath))
                    {
                        var parts = line.Split(';', (char)StringSplitOptions.RemoveEmptyEntries);
                        if (parts.Length == 2)
                        {
                            string fileName = parts[0].Trim();
                            string hashValue = parts[1].Trim();
                            hashDict[fileName] = hashValue;
                        }
                        else
                        {
                            if (line_count != 1)
                            {
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine($"Warning: The format of line '{line}' is incorrect.");
                            }
                        }
                        line_count++;
                    }
                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"An error occurred while reading the hash file: {ex.Message}");
                }
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("The hash file not found.");
            }

            return hashDict;
        }
    }
}
