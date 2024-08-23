using M_B_Bannerlord_ModPackTool.ConfigFunctions;
using M_B_Bannerlord_ModPackTool.Functions;
using Microsoft.Win32;
using System.IO;
using System.Security.Principal;
using System.Threading.Tasks;
using System;

namespace M_B_Bannerlord_ModPackTool
{
    internal class ClassModPackTool
    {
        private static async Task Main(string[] args)
        {

#if !DEBUG 
            if (!IsAdministrator())
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("This program is not running as an administrator. Please restart it with administrative privileges.");
                Console.ReadLine();
                return;
            }
#endif
            ConfigEvents.CheckConfig();
            Config config = ConfigEvents.LoadConfig();
            string hashfile = config.HashFile;
            string modpackFile = config.ModPackXmlFile;


            string steamKey = null;
            string steamPath = null;
            if (!config.CustomInstallation)
            {
                steamKey = @"HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\Valve\Steam";
                steamPath = (string)Registry.GetValue(steamKey, "InstallPath", null);
            }



            if (SteamEvents.SteamInstalled(steamPath) || config.CustomInstallation)
            {
                string bannerlordPath = null;
                if (config.CustomInstallation)
                {
                    bannerlordPath = config.CustomInstallationPath;
                }
                else
                {
                    bannerlordPath = Path.Combine(steamPath, @"steamapps\common\Mount & Blade II Bannerlord");
                }


                if (MBEvents.MBInstalled(bannerlordPath))
                {
                    while (true)
                    {
                        Console.ForegroundColor = ConsoleColor.Gray;
                        Console.WriteLine("Please choose an option:");
                        Console.WriteLine("1. Checking the integrity of game files");
                        Console.WriteLine("2. Complete cleanup of the game directory");
                        Console.WriteLine("3. Deleting the game directory found in Documents");
                        Console.WriteLine("4. Removing all mods from the game");
                        Console.WriteLine("5. Installing mods found in the modpack");
                        Console.WriteLine("6. Regenerating the mod load order in the game launcher.");
#if !DEBUG
                        Console.WriteLine("7. Unblock DLL files");
#endif

                        Console.WriteLine("8. Exit");

                        Console.Write("Choice: ");
                        string input = Console.ReadLine();

                        switch (input)
                        {
                            case "1":
                                CheckGamefilesIntegrity(hashfile, bannerlordPath, config);
                                break;
                            case "2":
                                GameFolderCleanup(hashfile, bannerlordPath);
                                break;
                            case "3":
                                RemoveDocumentDir();
                                break;
                            case "4":
                                RemoveGameMods(bannerlordPath);
                                break;
                            case "5":
                                await InstallGameMods(config, bannerlordPath);
                                break;
                            case "6":
                                ModOrderGenerate(config, modpackFile, bannerlordPath);
                                break;
#if !DEBUG
                            case "7":
                                UnblockDllFiles(bannerlordPath);
                                break;
#endif
                            case "8":
                                return;
                            default:
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine("----------------------------------------------------------------------------------------");
                                Console.WriteLine("Invalid choice. Please try again.");
                                Console.WriteLine("----------------------------------------------------------------------------------------");
                                break;
                        }
                    }
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("----------------------------------------------------------------------------------------");
                    Console.WriteLine("Mount & Blade: Bannerlord game installation directory not found! Press Enter key to exit!");
                    Console.WriteLine("----------------------------------------------------------------------------------------");
                    Console.ReadLine();
                }
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("----------------------------------------------------------------------------------------");
                Console.WriteLine("Steam installation directory not found! Press Enter key to exit!");
                Console.WriteLine("----------------------------------------------------------------------------------------");
                Console.ReadLine();
            }
        }

        private static void CheckGamefilesIntegrity(string hashfile, string bannerlordPath, Config config)
        {
            if (File.Exists(hashfile))
            {

                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("----------------------------------------------------------------------------------------");
                Console.WriteLine("Verifying the integrity of game files.");
                Console.WriteLine("----------------------------------------------------------------------------------------");

                if (!MBEvents.VerifyFiles(config.HashFile, bannerlordPath))
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("----------------------------------------------------------------------------------------");
                    Console.WriteLine("An error occurred during file verification. Please check the integrity of the game files!");
                    Console.WriteLine("----------------------------------------------------------------------------------------");
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("----------------------------------------------------------------------------------------");
                    Console.WriteLine("No errors were found during file verification. Verification completed successfully.");
                    Console.WriteLine("----------------------------------------------------------------------------------------");                    
                }
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("----------------------------------------------------------------------------------------");
                Console.WriteLine("Hash file not found in the program directory, so verification is not possible.");
                Console.WriteLine("----------------------------------------------------------------------------------------");
            }
        }

        private static void GameFolderCleanup(string hashfile, string bannerlordPath)
        {
            if (File.Exists(hashfile))
            {

                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("----------------------------------------------------------------------------------------");
                Console.WriteLine("Starting the cleanup of the game folder.");
                Console.WriteLine("----------------------------------------------------------------------------------------");

                MBEvents.CleanInstallation(hashfile, bannerlordPath);

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("----------------------------------------------------------------------------------------");
                Console.WriteLine("Game folder successfully cleaned.");
                Console.WriteLine("----------------------------------------------------------------------------------------");
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("----------------------------------------------------------------------------------------");
                Console.WriteLine("Hash file not found in the program directory, so cleanup is not possible.");
                Console.WriteLine("----------------------------------------------------------------------------------------");
            }
        }

        private static void RemoveDocumentDir()
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("----------------------------------------------------------------------------------------");
            Console.WriteLine("Deleting game folder from Documents.");
            Console.WriteLine("----------------------------------------------------------------------------------------");

            MBEvents.CleanDocumentFolder();

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("----------------------------------------------------------------------------------------");
            Console.WriteLine("Game folder successfully deleted from Documents.");
            Console.WriteLine("----------------------------------------------------------------------------------------");
        }

        private static void RemoveGameMods(string bannerlordPath)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("----------------------------------------------------------------------------------------");
            Console.WriteLine("Starting the deletion of game mods.");
            Console.WriteLine("----------------------------------------------------------------------------------------");

            DeleteEvents.DeleteAllMods(bannerlordPath);

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("----------------------------------------------------------------------------------------");
            Console.WriteLine("Mods successfully deleted from the game.");
            Console.WriteLine("----------------------------------------------------------------------------------------");
        }

        private static async Task InstallGameMods(Config config, string bannerlordPath)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("----------------------------------------------------------------------------------------");
            Console.WriteLine("Starting the download and installation of mods.");
            Console.WriteLine("----------------------------------------------------------------------------------------");

            bool installstatus = await ModEvents.InstallingMods(config, bannerlordPath);

            if (installstatus)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("----------------------------------------------------------------------------------------");
                Console.WriteLine("The modpack has been successfully installed.");
                Console.WriteLine("----------------------------------------------------------------------------------------");
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("----------------------------------------------------------------------------------------");
                Console.WriteLine("The modpack installation failed.");
                Console.WriteLine("----------------------------------------------------------------------------------------");
            }
            
        }

        private static void ModOrderGenerate(Config config, string modpackFile, string bannerlordPath)
        {
            if (File.Exists(modpackFile))
            {

                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("----------------------------------------------------------------------------------------");
                Console.WriteLine("Regenerating the mod load order in the game launcher.");
                Console.WriteLine("----------------------------------------------------------------------------------------");

                ModEvents.ModOrderGenerate(config, bannerlordPath);

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("----------------------------------------------------------------------------------------");
                Console.WriteLine("Mod load order regeneration completed successfully.");
                Console.WriteLine("----------------------------------------------------------------------------------------");
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("----------------------------------------------------------------------------------------");
                Console.WriteLine("ModPack XML file not found in the program directory, so regenerating is not possible.");
                Console.WriteLine("----------------------------------------------------------------------------------------");
            }
        }

        private static void UnblockDllFiles(string bannerlordPath)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("----------------------------------------------------------------------------------------");
            Console.WriteLine("Unlocking DLL files...");
            Console.WriteLine("----------------------------------------------------------------------------------------");

            MBEvents.UnblockDllFiles(bannerlordPath);

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("----------------------------------------------------------------------------------------");
            Console.WriteLine("Successful DLL file unlocking process.");
            Console.WriteLine("----------------------------------------------------------------------------------------");
        }

        private static bool IsAdministrator()
        {
            var identity = WindowsIdentity.GetCurrent();
            var principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }
    }
}
