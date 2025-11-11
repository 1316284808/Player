using System.IO; 

namespace Player.Core.Models
{
    public class SettingPath
    {
         public static   string JsonDirectory { get; } = Path.Combine( AppDomain.CurrentDomain.BaseDirectory, "JSON");

        public static   string SettingsPath { get; }=Path.Combine(JsonDirectory, "settings.json");
        public static   string ThemePath { get; } = Path.Combine(JsonDirectory, "theme.json");
        public static   string HardwarePath { get; } = Path.Combine(JsonDirectory, "hardware.json");
        public static   string HistoryPath { get; } = Path.Combine(JsonDirectory, "history.json");
    }
}
