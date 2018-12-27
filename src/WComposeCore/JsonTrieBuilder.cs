using System;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.IO;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace WCompose
{
    internal class SettingsConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType) =>
            typeof(Settings).IsAssignableFrom(objectType);

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var obj = JObject.Load(reader);
            var version = obj.Value<string>("version");

            if (version == "1.0")
            {
                var result = new V1Settings();
                serializer.Populate(obj.CreateReader(), result);
                return result;
            }

            throw new InvalidDataException($"Unknown version: {version}");
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotSupportedException();
        }
    }

    internal abstract class Settings
    {
        public abstract string Version { get; }

        public abstract Trie<char, string> BuildTrie();
    }

    internal class V1Settings : Settings
    {
        public override string Version => "1.0";
    
        public Dictionary<string, string> Mappings { get; } = new Dictionary<string, string>();

        public override Trie<char, string> BuildTrie()
        {
            var result = new Trie<char, string>();
            foreach (var mapping in Mappings)
            {
                result.Insert(mapping.Key, mapping.Value);
            }

            return result;
        }
    }

    internal class JsonTrieBuilder : ITrieBuilder
    {
        private class V1Settings
        {
            public string Version { get; }
        }

        private async Task<Settings> ParseFile(TextReader file)
        {
            var text = await file.ReadToEndAsync();
            return JsonConvert.DeserializeObject<Settings>(text, new SettingsConverter());
        }
         
        public async Task<Trie<char, string>> Build(TextReader input)
        {
            var settings = await ParseFile(input);
                
            return settings.BuildTrie();
        }
    }
}
