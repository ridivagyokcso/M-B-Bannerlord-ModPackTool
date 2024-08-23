namespace M_B_Bannerlord_ModPackTool.Functions
{
    internal class SteamEvents
    {
        public static bool SteamInstalled(string steamPath)
        {
            if (steamPath != null)
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
