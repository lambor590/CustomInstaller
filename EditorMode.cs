using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Custom_Installer
{
    internal class EditorMode
    {
        public static async Task Menu(string configFile, string downloadsDir)
        {
            Console.Clear();
            Logger.Info("Editor de la configuración.");
            string r = Logger.Ask("¿Qué quieres hacer en la configuración?\nDeja la respuesta vacía para volver al inicio.\n\n1 - Añadir\n2 - Modificar\n3 - Eliminar", true);
            Console.Clear();
            var configJson = JsonConvert.DeserializeObject<Dictionary<string, object>>(File.ReadAllText(configFile)) ?? new Dictionary<string, object>();

            if (r == "1") { await Add(configFile, configJson, downloadsDir); }

            else if (r == "2") { await Modify(configFile, configJson, downloadsDir); }

            else if (r == "3") { await Delete(configFile, configJson, downloadsDir); }

            else { await Program.Start(configFile, downloadsDir); }
        }

        private static async Task Add(string configFile, Dictionary<string, object> configJson, string downloadsDir)
        {
            string r = Logger.Ask("¿Qué quieres añadir?\n\n1 - Categoría\n2 - Archivo");
            Console.Clear();

            if (r == "1")
            {
                r = Logger.Ask("¿Qué nombre le quieres poner?");
                configJson.Add(r, new Dictionary<string, object>());
            }
            else if (r == "2")
            {
                Utils.ListConfig(configJson);
                r = Logger.Ask("¿En qué categoría lo quieres poner?");
                int chosenNum = r[0] - '0';
                Console.Clear();
                
                string category = configJson.Keys.ElementAt(chosenNum - 1);

                string name = Logger.Ask("¿Qué nombre le quieres poner al archivo?");
                Console.Clear();
                string link = Logger.Ask("¿Cuál es el enlace directo de descarga?");
                Console.Clear();
                ((JObject)configJson[category])[name] = link;
            }

            Save(configFile, configJson);
            await ReturnToMenu(configFile, downloadsDir);
        }

        private static async Task Modify(string configFile, Dictionary<string, object> configJson, string downloadsDir)
        {
            Utils.ListConfig(configJson);
            string r = Logger.Ask("¿Cuál quieres modificar?");
            int chosenNum = r[0] - '0';

            Console.Clear();
            string category = configJson.Keys.ElementAt(chosenNum - 1);
            Logger.Info("Modificando categoría: " + category);

            r = Logger.Ask("¿Qué quieres cambiar?\nDeja la respuesta vacía para volver atrás.\n\n1 - Nombre de la categoría\n2 - Nombre de elemento\n3 - Enlace de elemento\n4 - Nombre y enlace de elemento", true);
            Console.Clear();

            JObject config = (JObject)configJson[category];
            Dictionary<string, string> categories = Utils.ToDictionary(config);
            Dictionary<string, string>.KeyCollection keys = Utils.GetKeys(category, configJson);
            Dictionary<string, string>.ValueCollection values = Utils.GetValues(category, configJson);

            if (r == "1")
            {
                Dictionary<string, string> data = Utils.GetCategoryData(category, configJson);
                r = Logger.Ask("¿Cuál es el nuevo nombre?");
                configJson.Remove(category);
                configJson.Add(r, data);
            }
            else if (r == "2")
            {
                Utils.ListKeys(categories);
                r = Logger.Ask("¿A qué elemento le quieres cambiar el nombre?");
                chosenNum = r[0] - '0';
                Console.Clear();
                string r2 = Logger.Ask("¿Cuál es el nuevo nombre?");
                string link = values.ElementAt(chosenNum - 1);
                config.Property(keys.ElementAt(chosenNum - 1))?.Remove();
                config[r2] = link;
            }
            else if (r == "3")
            {
                Utils.ListKeys(categories);
                r = Logger.Ask("¿A qué elemento le quieres cambiar el enlace?");
                chosenNum = r[0] - '0';
                Console.Clear();
                r = Logger.Ask("¿Cuál es el nuevo enlace?");
                string key = keys.ElementAt(chosenNum - 1);
                config[key] = r;
            }
            else if (r == "4")
            {
                Utils.ListKeys(categories);
                r = Logger.Ask("¿Cuál quieres modificar?");
                chosenNum = r[0] - '0';
                Console.Clear();
                string r2 = Logger.Ask("¿Cuál es el nuevo nombre?");
                Console.Clear();
                string r3 = Logger.Ask("¿Cuál es el nuevo enlace?");
                Console.Clear();
                config.Property(keys.ElementAt(chosenNum - 1))?.Remove();
                config[r2] = r3;
            }
            else { await ReturnToMenu(configFile, downloadsDir); }

            Save(configFile, configJson);
            await ReturnToMenu(configFile, downloadsDir);
        }

        private static async Task Delete(string configFile, Dictionary<string, object> configJson, string downloadsDir)
        {
            string r = Logger.Ask("¿Qué quieres eliminar?\nDeja la respuesta vacía para volver atrás.\n\n1 - Categoría\n2 - Elementos de una categoría", true);
            Console.Clear();

            if (r == "1")
            {
                Utils.ListConfig(configJson);
                r = Logger.Ask("¿Cuáles quieres eliminar?\nEjemplo: 1,2,3");
                IList<int> chosenKeys = r.Split(',').Select(int.Parse).ToList();
                var keysToRemove = chosenKeys.Select(fileNum => configJson.Keys.ElementAt(fileNum - 1)).ToList();

                foreach (string key in keysToRemove)
                {
                    configJson.Remove(key);
                };
            }
            else if (r == "2")
            {
                Utils.ListConfig(configJson);
                r = Logger.Ask("¿De qué categoría?");
                int n = r[0] - '0';
                string category = configJson.Keys.ElementAt(n - 1);
                Console.Clear();

                JObject config = (JObject)configJson[category];
                Dictionary<string, string>.KeyCollection keys = Utils.GetKeys(category, configJson);

                Utils.ListKeys(Utils.ToDictionary(config));
                
                r = Logger.Ask("¿Cuáles quieres eliminar?\nEjemplo: 1,2,3");
                
                IList<int> chosenKeys = r.Split(',').Select(int.Parse).ToList();
                var keysToRemove = chosenKeys.Select(fileNum => keys.ElementAt(fileNum - 1)).ToList();

                foreach (string key in keysToRemove)
                {
                    config.Property(key)?.Remove();
                }
            }
            else { await ReturnToMenu(configFile, downloadsDir); }

            Save(configFile, configJson);
            await ReturnToMenu(configFile, downloadsDir);
        }

        private static async Task ReturnToMenu(string configFile, string downloadsDir)
        {
            Console.Clear();
            await Menu(configFile, downloadsDir);
        }

        public static void Save(string configFile, Dictionary<string, object> json)
        {
            try
            {
                Console.Clear();
                string result = JsonConvert.SerializeObject(json, Formatting.Indented);
                File.WriteAllText(configFile, result);
                Logger.Ok("Cambios guardados correctamente.", true);
                Thread.Sleep(1000);
            }
            catch
            {
                Logger.Error("Ha sucedido un error al intentar guardar los cambios.", true);
            }
        }
    }
}
