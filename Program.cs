using System.IO;
using System.Text.RegularExpressions;
using System.Text.Json;
using System.Collections.Generic;
using System.Linq;
using System.Text.Encodings.Web;
using System.Text.Unicode;

public class Program {
    public static void Main(string[] args) {
        JsonSerializerOptions serializerOptions = new JsonSerializerOptions() {
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };
        string enText = File.ReadAllText(args[0]);
        string oldEnText = File.ReadAllText(args[1]);
        List<string> ChangedStrings = new();
        Dictionary<string, string> newF = JsonSerializer.Deserialize<Dictionary<string,string>>(enText);
        if (enText != oldEnText) {
            Console.WriteLine("Changes of original file detected");
            Dictionary<string, string> oldF = JsonSerializer.Deserialize<Dictionary<string,string>>(oldEnText);
            foreach (string k in oldF.Keys)
            {
                if (!newF.ContainsKey(k)) {
                    Console.WriteLine($"Key {k} was removed");
                }
                else if(newF[k] != oldF[k]) {
                    Console.WriteLine($"Key {k} was changed.");
                    ChangedStrings.Add(k);
                }
            }
        }
        string ruText = File.ReadAllText(args[2]);
        Dictionary<string, string> translationKeys = new();
        translationKeys = JsonSerializer.Deserialize<Dictionary<string,string>>(ruText);
        string pattern = @"""(.+)"":(?:\s+)""(.+)""";
        Regex regex = new(pattern, RegexOptions.Multiline);
        string o = regex.Replace(enText, (m) => {
            string key = m.Groups[1].Value;
            if(translationKeys.ContainsKey(key) && !ChangedStrings.Any((s) => key == s) && translationKeys[key] != newF[key]) {
                return $"\"{key}\": {JsonSerializer.Serialize(translationKeys[key], options: serializerOptions)}";
            }
            Console.WriteLine($"Key {key} not translated");
            return m.Groups[0].Value;
        });
		
		File.WriteAllText(args[3], o);
    }
}
