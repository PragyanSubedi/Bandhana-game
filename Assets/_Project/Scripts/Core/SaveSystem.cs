using System.IO;
using UnityEngine;

namespace Bandhana.Core
{
    public static class SaveSystem
    {
        const string FileName = "bandhana_save.json";

        static string Path => System.IO.Path.Combine(Application.persistentDataPath, FileName);

        public static void Save(SaveData data)
        {
            data.saveTimestamp = System.DateTime.UtcNow.ToString("o");
            File.WriteAllText(Path, JsonUtility.ToJson(data, prettyPrint: true));
        }

        public static SaveData Load()
        {
            if (!File.Exists(Path)) return null;
            return JsonUtility.FromJson<SaveData>(File.ReadAllText(Path));
        }

        public static bool HasSave() => File.Exists(Path);

        public static void Delete()
        {
            if (File.Exists(Path)) File.Delete(Path);
        }
    }
}
