using System.IO;
using System.Text.Json;

namespace AeroNotify
{
    public static class ConfigService
    {
        private const string FilePath = "config.json";

        public static Dictionary<string, object> Data { get; private set; } = new();


        public static void Load()
        {
            if (!File.Exists(FilePath))
            {
                Save(); 
                Console.WriteLine("config.json criado com estrutura vazia.");
                return;
            }

            try
            {
                string json = File.ReadAllText(FilePath);
                Data = JsonSerializer.Deserialize<Dictionary<string, object>>(json)
                       ?? new Dictionary<string, object>();

                Console.WriteLine("Configurações carregadas.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao carregar config.json: {ex.Message}");
            }
        }


        public static void Save()
        {
            try
            {
                string json = JsonSerializer.Serialize(Data, new JsonSerializerOptions
                {
                    WriteIndented = true
                });

                File.WriteAllText(FilePath, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao salvar config.json: {ex.Message}");
            }
        }


        public static void Add(string path, object value)
        {
            SetValue(path, value);
            Save();
            Console.WriteLine($"Config adicionada/atualizada: {path}");
        }

        public static void Remove(string path)
        {
            if (RemoveValue(path))
            {
                Save();
                Console.WriteLine($"Config removida: {path}");
            }
        }


        private static void SetValue(string path, object value)
        {
            var parts = path.Split(':');
            Dictionary<string, object> current = Data;

            for (int i = 0; i < parts.Length - 1; i++)
            {
                if (!current.ContainsKey(parts[i]))
                    current[parts[i]] = new Dictionary<string, object>();

                current = (Dictionary<string, object>)current[parts[i]];
            }

            current[parts[^1]] = value;
        }


        private static bool RemoveValue(string path)
        {
            var parts = path.Split(':');
            Dictionary<string, object> current = Data;

            for (int i = 0; i < parts.Length - 1; i++)
            {
                if (!current.ContainsKey(parts[i]))
                    return false;

                current = (Dictionary<string, object>)current[parts[i]];
            }

            return current.Remove(parts[^1]);
        }


        public static object? Get(string path)
        {
            var parts = path.Split(':');
            Dictionary<string, object> current = Data;

            for (int i = 0; i < parts.Length - 1; i++)
            {
                if (!current.ContainsKey(parts[i]))
                    return null;

                current = (Dictionary<string, object>)current[parts[i]];
            }

            current.TryGetValue(parts[^1], out object? value);
            return value;
        }
    }
}
