#pragma warning disable CS8602
using System;
using System.IO;
using System.Text.Json;
using Json.Schema;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net.Http;

namespace Instalador_personalizable
{
    public class fileSample
    {
        public string? Discord { get; set; }
        public string? OperaGX { get; set; }
    }

    public class schema
    {
        public string? program1 { get; set; }
        public string? program2 { get; set; }
    }

    class Program
    {
        static void Main()
        {
            var version = "1.0";
            Console.Title = $"Instalador de programas personalizable v{version} | By The Ghost";

            Logger.Info("Comprobando actualizaciones...");

            checkUpdates(version);
            void checkUpdates(string version)
            {
                var httpClient = new HttpClient();
                var url = httpClient.GetStringAsync("");
            }

            string configFile = Directory.GetCurrentDirectory() + "\\config.json";
            string downloadsDir = Directory.GetCurrentDirectory() + "\\Descargas";
            string schemaFile = Directory.GetCurrentDirectory() + "\\esquema.json";

            if (!File.Exists(configFile))
            {
                Logger.Error("El archivo de configuración no existe, creando uno de ejemplo...");
                var fileSample = new fileSample
                {
                    Discord = "https://discord.com/api/downloads/distributions/app/installers/latest?channel=stable&platform=win&arch=x86",
                    OperaGX = "https://net.geo.opera.com/opera_gx/stable/windows"
                };
                var opciones = new JsonSerializerOptions { WriteIndented = true };
                string serializedJson = System.Text.Json.JsonSerializer.Serialize(fileSample, opciones);
                string readyJson = serializedJson.Replace("\\u0026", "&");
                File.WriteAllText(configFile, readyJson);
                Logger.Info("Archivo de configuración creado correctamente.");
            }
            else
            {
                Logger.Info("Archivo de configuración OK.");
            }

            if (!File.Exists(schemaFile))
            {
                Logger.Error("El archivo de esquema no existe, creando uno...");
                var fileSchema = new schema
                {
                    program1 = "link",
                    program2 = "link"
                };
                var opciones = new JsonSerializerOptions { WriteIndented = true, AllowTrailingCommas = true };
                string serializedJson = System.Text.Json.JsonSerializer.Serialize(fileSchema, opciones);
                File.WriteAllText(schemaFile, serializedJson);
                Logger.Info("Archivo de esquema creado correctamente.");
            }
            else
            {
                Logger.Info("Archivo de esquema OK.");
            }

            if (!Directory.Exists(downloadsDir))
            {
                Logger.Error("No hay carpeta de descargas, creando una...");
                Directory.CreateDirectory(downloadsDir);
                Logger.Info("Carpeta de descargas creada correctamente.");
            }
            else
            {
                Logger.Info("Carpeta de descargas OK.");
            }
            Iniciar();

            async void Iniciar()
            {
                Thread.Sleep(1500);
                Console.Clear();

                Logger.Info("Comprobando formato del archivo de configuración...");

                try
                {
                    var schema = JsonSchema.FromFile(schemaFile);
                    var file = File.OpenRead(configFile);
                    var json = await JsonDocument.ParseAsync(file);
                    var result = schema.Validate(json.RootElement);

                    if (result.IsValid)
                    {
                        Logger.Info("Formato del archivo de configuración OK.");
                    }

                }
                catch
                {
                    Logger.Error("El formato del archivo de configuración no es válido.\nComprueba que el formato del archivo de esquema coincide con el de configuración.\nSe incluyen las comas finales.\n");
                    Logger.Info("Presiona cualquier tecla para salir.");
                    Console.ReadKey();
                    Environment.Exit(0);
                }


                Logger.Info("Leyendo archivo de configuración...");
                Thread.Sleep(1000);

                Programas();
                void Programas()
                {
                    Console.Clear();
                    string configFileText = File.ReadAllText(configFile);
                    JObject jobject = JObject.Parse(configFileText);
                    var values = JObject.Parse(JsonConvert.SerializeObject(jobject));

                    IList<string> programs = values.Properties().Select(c => c.Name).ToList();

                    Logger.Info("Esta es la lista de programas disponibles para descargar:\n");

                    int i = 0;
                    foreach (string key in programs)
                    {
                        i++;
                        Logger.Listar(key, i);
                    }


                    Logger.Ask("Escribe el número de los programas que quieras descargar, separados por comas.");
                    var response = Console.ReadLine();

                    try
                    {
                        IList<int> programsToDownload = response.Split(',').Select(int.Parse).ToList();

                        foreach (int programNumber in programsToDownload)
                        {
                            var takeValue = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(configFileText);
                            string programName = programs[programNumber - 1];
                            Logger.Info(takeValue[programName]);
                            // Crear WebClient para empezar a descargar :)
                        }
                    }
                    catch
                    {
                        Logger.Error("Has escrito los números de forma incorrecta o has puesto números no listados.\nComprueba que tenga el siguiente formato: 1,2,3,4...");
                        Logger.Info("Presiona cualquier tecla para volver a intentarlo.");
                        Console.ReadKey();
                        Programas();
                    }
                }
            }

            Console.ReadKey();
        }
    }
}