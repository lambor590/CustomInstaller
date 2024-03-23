using Newtonsoft.Json;

namespace Custom_Installer
{
    class Program
    {
        static async Task Main(string[] args)
        {
            if (args.Length > 0 && args[0] == "--openfiles")
            {
                DownloadMode.OpenFiles(Path.Combine(Directory.GetCurrentDirectory(), "Descargas"));
                return;
            }

            try
            {
                await Utils.CheckUpdates();
            }
            catch
            {
                Logger.Error("No se han podido comprobar las actualizaciones. ¿Tienes conexión a Internet?\nContinuando...");
                Thread.Sleep(2000);
            }

            string configFile = Path.Combine(Directory.GetCurrentDirectory(), "config.json");
            string downloadsDir = Path.Combine(Directory.GetCurrentDirectory(), "Descargas");

            if (Directory.Exists(downloadsDir))
            {
                Parallel.ForEach(Directory.EnumerateFiles(downloadsDir), file =>
                {
                    File.Delete(file);
                });
            }

            if (!File.Exists(configFile))
            {
                Logger.Error("El archivo de configuración no existe, creando uno...", true);
                Dictionary<string, object> json = new()
                {
                    {"General", new Dictionary<string, string>()
                    {
                        {"Discord", "https://discord.com/api/downloads/distributions/app/installers/latest?channel=stable&platform=win&arch=x86"},
                        {"OperaGX", "https://net.geo.opera.com/opera_gx/stable/windows"}
                    }},

                    {"Launchers", new Dictionary<string, string>()
                    {
                        {"Steam", "https://cdn.cloudflare.steamstatic.com/client/installer/SteamSetup.exe"},
                        {"Rockstar Launcher", "https://gamedownloads.rockstargames.com/public/installer/Rockstar-Games-Launcher.exe"}
                    }}
                };

                EditorMode.Save(configFile, json, false);
                Logger.Info("Archivo de configuración creado correctamente.", true);
            }
            else
            {
                Logger.Ok("Archivo de configuración.", true);
            }

            if (!Directory.Exists(downloadsDir))
            {
                Logger.Error("No hay carpeta de descargas, creando una...", true);
                Directory.CreateDirectory(downloadsDir);
                Logger.Info("Carpeta de descargas creada correctamente.", true);
            }
            else
            {
                Logger.Ok("Carpeta de descargas.", true);
            }

            Thread.Sleep(500);
            Console.Clear();
            await Start(configFile, downloadsDir);
        }

        public static async Task Start(string configFile, string downloadsDir)
        {
            string configFileText = File.ReadAllText(configFile);
            Dictionary<string, object> configJson = JsonConvert.DeserializeObject<Dictionary<string, object>>(configFileText) ?? [];

            if (configJson.Count == 0)
            {
                Logger.Error("El formato del archivo de configuración no es válido.\nComprueba que al modificarlo manualmente mantenga el mismo formato.\nPuedes usar jsonlint.com para comprobar el archivo de configuración, o volver a generarlo.", true);
                Logger.Info("Presiona cualquier tecla para salir.", true);
                Console.ReadKey();
                Environment.Exit(0);
            }

            Logger.Info("Inicio");
            var r = Logger.Ask("¿Qué quieres hacer?\n\n1 - Descargar archivos\n2 - Editar configuración");
            Console.Clear();

            if (r == "1") { await DownloadMode.Run(configFile, downloadsDir, configJson, configFileText); }
            else if (r == "2") { await EditorMode.Menu(configFile, downloadsDir, configJson); }
            else { await DownloadMode.Run(configFile, downloadsDir, configJson, configFileText); }

            Console.ReadKey(true);
        }
    }
}