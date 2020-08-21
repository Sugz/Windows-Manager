using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace WindowsManager.Helpers
{
    public static class Constants
    {
        internal static string Company => Assembly.GetEntryAssembly().GetCustomAttribute<AssemblyCompanyAttribute>().Company;

        internal static string Product => Assembly.GetEntryAssembly().GetCustomAttribute<AssemblyProductAttribute>().Product;

        internal static string Version => Assembly.GetEntryAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;

        internal static string SettingsFolder => $"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\\{Company}\\{Product}";

        internal static string SettingsFile => $"{SettingsFolder}\\Settings_v{Version}.xml";
    }
}
