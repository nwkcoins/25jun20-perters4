using MessagePack;
using System;
using System.IO;

namespace CryptoTraderScanner
{
    public static class SettingsStore
    {
        private const string SETTINGS_FILE = "settings3.dat";

        static public Settings Load()
        {
            try
            {
                if (File.Exists(SETTINGS_FILE))
                {
                    // load candles from disk
                    using (var file = File.OpenRead(SETTINGS_FILE))
                    {
                        var bytes = new byte[file.Length];
                        file.Read(bytes, 0, bytes.Length);
                        return MessagePackSerializer.Deserialize<Settings>(bytes);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"{DateTime.Now.ToString("ddMMMyy.HHmmss")} SettingsStore.Load ERROR: {e.Message}  at '{e.StackTrace.Substring(Math.Max(0, e.StackTrace.Length - 40))}'");
            }
            return new Settings();
        }

        static public void Save(Settings settings)
        {
            try
            {
                if (File.Exists(SETTINGS_FILE))
                {
                    File.Delete(SETTINGS_FILE);
                }
                var bytes = MessagePackSerializer.Serialize(settings);
                using (var file = File.OpenWrite(SETTINGS_FILE))
                {
                    file.Write(bytes, 0, bytes.Length);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"{DateTime.Now.ToString("ddMMMyy.HHmmss")} SettingsStore.Save ERROR: {e.Message} at '{e.StackTrace.Substring(Math.Max(0, e.StackTrace.Length - 40))}'");
            }
        }
    }
}