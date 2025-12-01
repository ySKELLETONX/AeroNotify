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
                CreateNewConfig();
                Save();
                return;
            }

            try
            {
                string json = File.ReadAllText(FilePath);
                var rawData = JsonSerializer.Deserialize<Dictionary<string, object>>(json);

                if (rawData != null)
                    Data = ConvertJsonElements(rawData);
                else
                    Data = new Dictionary<string, object>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao carregar config.json: {ex.Message}");
            }
        }

        private static void CreateNewConfig()
        {
            Add("AeroNotify:Debug", 1);
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
                if (!current.ContainsKey(parts[i]) || current[parts[i]] is not Dictionary<string, object>)
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
                if (!current.ContainsKey(parts[i]) || current[parts[i]] is not Dictionary<string, object>)
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
                if (!current.ContainsKey(parts[i]) || current[parts[i]] is not Dictionary<string, object>)
                    return null;

                current = (Dictionary<string, object>)current[parts[i]];
            }

            current.TryGetValue(parts[^1], out object? value);
            return value;
        }

        private static Dictionary<string, object> ConvertJsonElements(Dictionary<string, object> dict)
        {
            var converted = new Dictionary<string, object>();

            foreach (var item in dict)
            {
                if (item.Value is JsonElement jsonElement)
                {
                    converted[item.Key] = ConvertElement(jsonElement);
                }
                else
                {
                    converted[item.Key] = item.Value;
                }
            }

            return converted;
        }

        private static object ConvertElement(JsonElement element)
        {
            return element.ValueKind switch
            {
                JsonValueKind.Object => ConvertJsonElements(
                    JsonSerializer.Deserialize<Dictionary<string, object>>(element.GetRawText())!
                ),

                JsonValueKind.Array => element.EnumerateArray()
                    .Select(e => ConvertElement(e))
                    .ToList(),

                JsonValueKind.String => element.GetString()!,
                JsonValueKind.Number => element.TryGetInt64(out long l) ? l : element.GetDouble(),
                JsonValueKind.True => true,
                JsonValueKind.False => false,
                JsonValueKind.Null => null!,
                _ => element.GetRawText()
            };
        }
    }
}
