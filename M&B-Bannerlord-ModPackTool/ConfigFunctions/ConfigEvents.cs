using System;
using System.IO;
using System.Text.Json;

namespace M_B_Bannerlord_ModPackTool.ConfigFunctions
{
    internal class ConfigEvents
    {
        public static void CheckConfig()
        {
            string configfile = @"config.json";
            if (!File.Exists(configfile))
            {
                GenerateConfig(configfile);
            }
            else
            {
                CheckConfigSettings(configfile);
            }
        }

        public static Config LoadConfig()
        {
            string configfile = @"config.json";
            string jsonString = File.ReadAllText(configfile);
            Config config = JsonSerializer.Deserialize<Config>(jsonString);
            return config;
        }

        private static void CheckConfigSettings(string configfile)
        {
            try
            {
                string jsonString = File.ReadAllText(configfile);
                Config config = JsonSerializer.Deserialize<Config>(jsonString);

                if (!ValidateConfig(config))
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"An error occurred while reading the configuration file, so it will be regenerated.");
                    GenerateConfig(configfile);
                }
            }
            catch (JsonException ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"An error occurred while reading the configuration file, so it will be regenerated: {ex.Message}");
                GenerateConfig(configfile);
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"An error occurred while reading the configuration file, so it will be regenerated: {ex.Message}");
                GenerateConfig(configfile);
            }
        }

        private static void GenerateConfig(string configfile)
        {
            if (File.Exists(configfile))
            {
                File.Delete(configfile);
            }

            var config = new Config
            {
                CustomInstallation = false,
                CustomInstallationPath = "C:/Program Files (x86)/Steam/steamapps/common/MB",
                HashFile = "HashList.txt",
                ModPackXmlFile = "ModPack.xml",
                NexusModManagerApiKey = "APIKEY"
            };

            string jsonString = JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true });
            try
            {
                File.WriteAllText(configfile, jsonString);
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Configuration file successfully created.");
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"An error occurred while creating the configuration file: {ex.Message}");
            }
        }

        private static bool ValidateConfig(Config config)
        {
            int warning = 0;

            if (config == null)
            {
                warning = 1;
            }

            if (config.CustomInstallation)
            {
                if (string.IsNullOrEmpty(config.CustomInstallationPath))
                {
                    warning = 1;
                }
            }

            if (string.IsNullOrEmpty(config.HashFile))
            {
                warning = 1;
            }

            if (string.IsNullOrEmpty(config.ModPackXmlFile))
            {
                warning = 1;
            }

            if (string.IsNullOrEmpty(config.NexusModManagerApiKey))
            {
                warning = 1;
            }

            if (warning == 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
