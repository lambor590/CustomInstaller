using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Diagnostics;

namespace Custom_Installer
{
    internal class DownloadMode
    {
        public static async Task Run(string configFile, string downloadsDir)
        {
            string configFileText = File.ReadAllText(configFile);
            Dictionary<string, object>? json = new();

            try
            {
                json = JsonConvert.DeserializeObject<Dictionary<string, object>>(configFileText);
            }
            catch
            {
                Logger.Error("El formato del archivo de configuración no es válido.\nComprueba que al modificarlo manualmente mantenga el mismo formato.\nPuedes usar jsonlint.com para comprobar el archivo de configuración, o volver a generarlo.", true);
                Logger.Info("Presiona cualquier tecla para salir.", true);
                Console.ReadKey();
                Environment.Exit(0);
            }

            JObject categories = JObject.Parse(JsonConvert.SerializeObject(json));

            Logger.Info("Lista de categorías:\n");

            categories.Properties().Select((c, i) => new { Key = c.Name, Index = i + 1 })
                .ToList()
                .ForEach(item => Logger.List(item.Key, item.Index, true));

            string response = Logger.Ask("¿Qué quieres descargar? Escribe 'todas' o '0' para descargar toda la lista.\nEjemplo: 1,2,3", true);
            bool downloadAll = false;

            try
            {
                if (response.ToString().ToLower() == "todas" || response.Equals("0"))
                {
                    downloadAll = true;
                }
                Console.Clear();
                Logger.Info("Iniciando descarga...\n");

                IList<int> categoriesToDownload = response.Split(',').Select(int.Parse).ToList();
                var takeValue = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, string>>>(configFileText) ?? new Dictionary<string, Dictionary<string, string>>();

                List<Task> downloadTasks = new();

                using (var client = new HttpClient())
                {
                    var categoriesToProcess = downloadAll ?
                        categories.Properties() :
                        categoriesToDownload.Select(catNumber => categories.Properties()
                        .Skip(catNumber - 1).First());

                    foreach (var category in categoriesToProcess)
                    {
                        var categoryObject = category.Value as JObject;
                        foreach (var (fileName, fileLink) in from property in categoryObject?.Properties()
                                                             let fileName = property.Name
                                                             let fileLink = new Uri(takeValue[category.Name][property.Name])
                                                             select (fileName, fileLink))
                        {
                            var downloadTask = Utils.Download(client, fileName, fileLink, downloadsDir);
                            if (downloadTask != null)
                            {
                                downloadTasks.Add(downloadTask);
                            }
                        }
                    }

                    await Task.WhenAll(downloadTasks);
                }

                Thread.Sleep(1000);
                Console.Clear();
                Logger.Info("Se han terminado de descargar todos los archivos.");

                response = Logger.Ask("¿Quieres abrir todos los archivos ejecutables descargados? [s/N]");
                try
                {
                    if (response.ToString().ToLower() == "s")
                    {
                        Console.WriteLine("\n");
                        Logger.Info("Abriendo archivos...");
                        string[] files = Directory.GetFiles(downloadsDir, "*.exe");

                        try
                        {
                            foreach (string file in files)
                            {
                                ProcessStartInfo start = new()
                                {
                                    FileName = file
                                };
                                Process.Start(start);
                            }
                            Logger.Info("Presiona cualquier tecla para salir.");
                            Console.ReadKey();
                            Environment.Exit(0);
                        }
                        catch (Exception error)
                        {
                            if (error.ToString().Contains("La operación solicitada requiere elevación."))
                            {
                                Console.WriteLine("\n");
                                Logger.Error("Uno de los archivos requiere permisos de administrador.\nVuelve a abrir el programa como administrador para que pueda abrirlo, o ábrelo tú.", true);
                                Console.ReadKey();
                                Environment.Exit(0);
                            }
                        }
                    }
                    else
                    {
                        Logger.Info("Presiona cualquier tecla para salir.");
                        Console.ReadKey();
                        Environment.Exit(0);
                    }
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
                await Run(configFile, downloadsDir);
            }
        }
    }
}
