using System;
using System.IO;
using UnityEngine;

namespace ComputerExplorer.Progress
{
    /// <summary>
    /// Local JSON persistence in Application.persistentDataPath. Writes go to a temp file first so a
    /// crash mid-write never corrupts saved progress. Swap this class for a backend later without
    /// touching the UI.
    /// </summary>
    public static class SaveSystem
    {
        private static string PathFor(string fileName) => Path.Combine(Application.persistentDataPath, fileName);

        public static T Load<T>(string fileName) where T : class
        {
            try
            {
                var path = PathFor(fileName);
                if (!File.Exists(path)) return null;
                return JsonUtility.FromJson<T>(File.ReadAllText(path));
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[SaveSystem] Could not read {fileName}: {e.Message}");
                return null;
            }
        }

        public static void Save<T>(string fileName, T data)
        {
            try
            {
                var path = PathFor(fileName);
                var tmp = path + ".tmp";
                File.WriteAllText(tmp, JsonUtility.ToJson(data, true));
                if (File.Exists(path)) File.Delete(path);
                File.Move(tmp, path);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[SaveSystem] Could not write {fileName}: {e.Message}");
            }
        }

        public static void Delete(string fileName)
        {
            var path = PathFor(fileName);
            if (File.Exists(path)) File.Delete(path);
        }
    }
}
