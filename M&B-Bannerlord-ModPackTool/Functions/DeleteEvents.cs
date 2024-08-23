using System;
using System.IO;
using System.Linq;

namespace M_B_Bannerlord_ModPackTool.Functions
{
    internal class DeleteEvents
    {
        public static void DeleteAllMods(string bannerlordPath)
        {
            string modulesPath = Path.Combine(bannerlordPath, @"Modules");
            string binPath = Path.Combine(bannerlordPath, @"bin");

            string[] defaultModules = new string[]
            {
                "Native",
                "SandBox",
                "SandBoxCore",
                "StoryMode",
                "CustomBattle",
                "BirthAndDeath",
                "Multiplayer"
            };

            string[] blseFiles = new string[]
            {
                "Bannerlord.BLSE.Standalone.exe.config",
                "Bannerlord.BLSE.Standalone.exe",
                "Bannerlord.BLSE.Shared.dll",
                "Bannerlord.BLSE.LauncherEx.exe.config",
                "Bannerlord.BLSE.LauncherEx.exe",
                "Bannerlord.BLSE.Launcher.exe.config",
                "Bannerlord.BLSE.Launcher.exe"
            };

            string blsedirectory = Path.Combine(binPath, @"Gaming.Desktop.x64_Shipping_Client");
            if (Directory.Exists(blsedirectory))
            {
                Directory.Delete(blsedirectory, true);
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"Deleted blse folder: Gaming.Desktop.x64_Shipping_Client");
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"BLSE Bin folder not found, so no deletion is necessary.");
            }

            string bingamedirectory = Path.Combine(binPath, @"Win64_Shipping_Client");
            foreach (var file in blseFiles)
            {
                string filep = Path.Combine(bingamedirectory, file);
                if (File.Exists(filep))
                {
                    try
                    {
                        File.Delete(filep);
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine($"Deleted blse file: {file}");
                    }
                    catch (Exception ex)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"An error occurred while deleting {file}: {ex.Message}");
                    }
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"BLSE file ({file}) not found, so no deletion is necessary.");
                }
            }

            var directories = Directory.GetDirectories(modulesPath);

            foreach (var directory in directories)
            {
                string folderName = Path.GetFileName(directory);

                if (!defaultModules.Contains(folderName))
                {
                    try
                    {
                        Directory.Delete(directory, true);
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine($"Deleted module: {folderName}");
                    }
                    catch (Exception ex)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"An error occurred while deleting {folderName}: {ex.Message}");
                    }
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"Default module: {folderName}, not deleting.");
                }
            }
            Console.ForegroundColor = ConsoleColor.White;
        }
    }
}
