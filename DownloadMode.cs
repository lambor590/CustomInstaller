using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Diagnostics;
using System.ComponentModel;

namespace Custom_Installer
{
    internal class DownloadMode
    {
        public static async Task Run(string configFile, string downloadsDir, Dictionary<string, object> json, string configFileText)
        {
            JObject categories = JObject.Parse(JsonConvert.SerializeObject(json));

            Logger.Info("Lista de categorías:\n");

            categories.Properties().Select((c, i) => new { Key = c.Name, Index = i + 1 })
                .ToList()
                .ForEach(item => Logger.List(item.Key, item.Index, true));

            string response = Logger.Ask("¿Qué quieres descargar? Escribe 'todas' o '0' para descargar toda la lista.\nEjemplo: 1,2,3", true);
            bool downloadAll = false;

            try
            {
                if (response.ToString().Equals("todas", StringComparison.CurrentCultureIgnoreCase) || response.Equals("0"))
                {
                    downloadAll = true;
                }
                Console.Clear();
                Logger.Info("Iniciando descarga...\n");

                IList<int> categoriesToDownload = response.Split(',').Select(int.Parse).ToList();
                var takeValue = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, string>>>(configFileText) ?? [];

                List<Task> downloadTasks = [];

                using (var client = new HttpClient())
                {
                    var categoriesToProcess = downloadAll ?
                        categories.Properties() :
                        categoriesToDownload.Select(catNumber => categories.Properties().Skip(catNumber - 1).First());
                    Parallel.ForEach(categoriesToProcess, category =>
                    {
                        var categoryObject = category.Value as JObject;
                        var categoryDownloadTasks = categoryObject?.Properties().AsParallel().Select(property =>
                        {
                            var fileName = property.Name;
                            var fileLink = new Uri(takeValue[category.Name][property.Name]);
                            return Utils.Download(client, fileName, fileLink, downloadsDir);
                        }).ToList();

                        Task.WhenAll(categoryDownloadTasks ?? Enumerable.Empty<Task>()).Wait();
                    });
                }


                Thread.Sleep(1000);
                Console.Clear();
                Logger.Info("Se han terminado de descargar todos los archivos.");

                response = Logger.Ask("¿Quieres abrir todos los archivos ejecutables descargados? [s/N]");
                try
                {
                    if (!response.ToString().Equals("s", StringComparison.CurrentCultureIgnoreCase))
                    {
                        Logger.Info("Presiona cualquier tecla para salir.");
                        Console.ReadKey();
                        Environment.Exit(0);
                    }

                    Console.WriteLine("\n");
                    Logger.Info("Abriendo archivos...");
                    string[] files = Directory.GetFiles(downloadsDir, "*.exe");

                    foreach (string file in files)
                    {
                        try
                        {
                            await Task.Run(() => Process.Start(file));
                        }
                        catch (Win32Exception error) when (error.NativeErrorCode == 1223)
                        {
                            Console.WriteLine("\n");
                            Logger.Error("Uno de los archivos requiere permisos de administrador.\nVuelve a abrir el programa como administrador para que pueda abrirlo, o ábrelo tú.", true);
                            Console.ReadKey();
                            Environment.Exit(0);
                        }
                    }

                    Logger.Info("Presiona cualquier tecla para salir.");
                    Console.ReadKey();
                    Environment.Exit(0);
                }
                catch (Exception error)
                {
                    Logger.Error("Ha sucedido un error inesperado: " + error);
                    Console.ReadKey();
                    Environment.Exit(0);
                }
            }
            catch
            {
                Logger.Error("Has escrito los números de forma incorrecta, o ha sucedido un error con algún archivo.\nComprueba que el order de los programas que quieres descargar tenga el siguiente formato: 1,2,3,4...", true);
                Logger.Info("Presiona cualquier tecla para volver a intentarlo.", true);
                Console.ReadKey();
                Console.Clear();
                await Run(configFile, downloadsDir, json, configFileText);
            }
        }
    }
}
