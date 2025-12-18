using System;
using System.IO;
using System.Text.Json;
using static CVG.Form1;

namespace CVG
{
    public static class ProfileStore
    {
        public static string GetProfilePath()
        {
            string dir = AppDomain.CurrentDomain.BaseDirectory;
            return Path.Combine(dir, "profile.json");
        }

        public static Profile LoadOrCreate()
        {
            string path = GetProfilePath();

            if (!File.Exists(path))
            {
                var p = new Profile();
                Save(p);
                return p;
            }

            string json = File.ReadAllText(path);
            var profile = JsonSerializer.Deserialize<Profile>(json);

            return profile ?? new Profile();
        }

        public static void Save(Profile profile)
        {
            string path = GetProfilePath();

            var opts = new JsonSerializerOptions
            {
                WriteIndented = true
            };

            string json = JsonSerializer.Serialize(profile, opts);
            File.WriteAllText(path, json);
        }
    }
}
