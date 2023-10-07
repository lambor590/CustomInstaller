using Newtonsoft.Json.Linq;
using System.Diagnostics;
using System.Reflection;

namespace Custom_Installer
{
    internal class Logger
    {
        public static void Info(string msg, bool fast = false)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Utils.Write("[", fast ? 6 : 18);
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Utils.Write("INFO", fast ? 6 : 18);
            Console.ForegroundColor = ConsoleColor.Cyan;
            Utils.Write("] ", fast ? 6 : 18);
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Utils.Write(msg + "\n", fast ? 6 : 18);
        }

        public static void Error(string msg, bool fast = false)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Utils.Write("[", fast ? 6 : 18);
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Utils.Write("ERROR", fast ? 6 : 18);
            Console.ForegroundColor = ConsoleColor.Red;
            Utils.Write("] ", fast ? 6 : 18);
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Utils.Write(msg + "\n", fast ? 6 : 18);
        }

        public static void List(string msg, int order, bool fast = false)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Utils.Write("[", fast ? 6 : 18);
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Utils.WriteNum(order, fast ? 6 : 18);
            Console.ForegroundColor = ConsoleColor.Cyan;
            Utils.Write("] ", fast ? 6 : 18);
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Utils.Write(msg + "\n", fast ? 6 : 18);
        }

        public static string Ask(string msg, bool fast = false)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Utils.Write("\n[", fast ? 6 : 18);
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Utils.Write("PREGUNTA", fast ? 6 : 18);
            Console.ForegroundColor = ConsoleColor.Yellow;
            Utils.Write("]\n", fast ? 6 : 18);
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Utils.Write(msg, fast ? 6 : 18);
            Console.ForegroundColor = ConsoleColor.DarkGreen;
            Utils.Write("\n\n» ", fast ? 6 : 18);
            return Console.ReadLine() ?? string.Empty;
        }

        public static void Ok(string mensaje, bool fast = false)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Utils.Write("[", fast ? 6 : 18);
            Console.ForegroundColor = ConsoleColor.DarkGreen;
            Utils.Write("OK", fast ? 6 : 18);
            Console.ForegroundColor = ConsoleColor.Green;
            Utils.Write("] ", fast ? 6 : 18);
            Console.ForegroundColor = ConsoleColor.DarkGreen;
            Utils.Write(mensaje + "\n", fast ? 6 : 18);
        }
    }

    internal class Utils
    {
        public static void Write(string msg, int sleep = 18)
        {
            for (int i = 0; i < msg.Length; i++)
            {
                Console.Write(msg[i]);
                Thread.Sleep(sleep);
            }
        }

        public static void WriteNum(int num, int sleep = 18)
        {
            Console.Write(num);
            Thread.Sleep(sleep);
        }

        public static string GetLocalVersion()
        {
            try
            {
                var a = Assembly.GetExecutingAssembly().GetName().Version;
                return $"{a?.Major}.{a?.Minor}.{a?.Build}";
            }
            catch
            {
                return "desconocida";
            }
        }

        public static async Task CheckUpdates()
        {
            Console.Title = $"Instalador personalizable v{GetLocalVersion()} | The Ghost";

            var httpClient = new HttpClient();
            var latestVersionTask = httpClient.GetStringAsync("https://otcr.tk/version.txt");
            Logger.Info("Comprobando actualizaciones...", true);
            var latestVersion = await latestVersionTask;

            if (latestVersion != GetLocalVersion())
            {
                Logger.Info($"Hay una actualización disponible ({GetLocalVersion()} -> {latestVersion}), instalando...");
                InstallUpdate();
            }
            else
            {
                Logger.Ok("Estás usando la última versión.", true);
                Thread.Sleep(500);
                Console.Clear();
                Logger.Info("Iniciando comprobaciones...");
            }
        }

        public static async void InstallUpdate()
        {
            string currentDir = Directory.GetCurrentDirectory() + "\\";
            string temp = Path.GetTempPath();

            using (var client = new HttpClient())
            {
                byte[] updaterBytes = await client.GetByteArrayAsync("https://otcr.tk/updater.bat");
                await File.WriteAllBytesAsync(currentDir + "updater.bat", updaterBytes);

                byte[] installerBytes = await client.GetByteArrayAsync("https://otcr.tk/Custom%20Installer.exe");
                await File.WriteAllBytesAsync(temp + "Custom Installer.exe", installerBytes);

            }

            ProcessStartInfo start = new();
            string selfName = AppDomain.CurrentDomain.FriendlyName + ".exe";
            start.Arguments = string.Format($"\"{selfName}\" \"{temp + "Custom Installer.exe"}\"");
            start.FileName = currentDir + "updater.bat";
            Process.Start(start);
            Environment.Exit(0);
        }

        public static async Task Download(HttpClient client, string fileName, Uri fileLink, string downloadsDir)
        {
            using (var response = await client.GetAsync(fileLink, HttpCompletionOption.ResponseHeadersRead))
            {
                var originalName = response.Content.Headers.ContentDisposition?.FileName;
                var fileStream = File.Create(downloadsDir + "\\" + originalName ?? fileName + ".exe");
                await response.Content.CopyToAsync(fileStream);
            }

            Logger.Ok(fileName, true);
        }

        public static void ListConfig(Dictionary<string, object> configJson)
        {
            Logger.Info("Configuración actual\n");

            configJson.Select((c, i) => new { c.Key, Index = i + 1 })
                .ToList()
                .ForEach(item => Logger.List(item.Key, item.Index, true));
        }

        public static void ListKeys(Dictionary<string, string> category)
        {
            Logger.Info("Configuración actual\n");

            category.Select((c, i) => new { c.Key, Index = i + 1 })
                .ToList()
                .ForEach(item => Logger.List(item.Key, item.Index, true));
        }

        public static Dictionary<string, string> ToDictionary(JObject category)
        {
            return category.ToObject<Dictionary<string, string>>() ?? new Dictionary<string, string>();
        }

        public static Dictionary<string, string>.ValueCollection GetValues(string category, Dictionary<string, object> configJson)
        {
            return ToDictionary((JObject)configJson[category]).Values ?? new Dictionary<string, string>().Values;
        }

        public static Dictionary<string, string>.KeyCollection GetKeys(string category, Dictionary<string, object> configJson)
        {
            return ToDictionary((JObject)configJson[category]).Keys ?? new Dictionary<string, string>().Keys;
        }

        public static Dictionary<string, string> GetCategoryData(string category, Dictionary<string, object> configJson)
        {
            return ToDictionary((JObject)configJson[category]) ?? new Dictionary<string, string>();
        }
    }
}
