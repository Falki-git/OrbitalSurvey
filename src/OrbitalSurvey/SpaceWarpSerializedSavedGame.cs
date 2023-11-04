using KSP.Sim;

namespace OrbitalSurvey
{
    [Serializable]
    public class SpaceWarpSerializedSavedGame : SerializedSavedGame
    {
        public List<PluginSaveData> PluginSaveData = new();
    }

    public delegate void SaveGameCallbackFunctionDelegate(object data);

    [Serializable]
    public class PluginSaveData
    {
        public string ModGuid {get; set; }
        public object SaveData { get; set; }

        [NonSerialized]
        public SaveGameCallbackFunctionDelegate SaveEventCallback;
        [NonSerialized]
        public SaveGameCallbackFunctionDelegate LoadEventCallback;
    }

    public static class ModSaves
    {
        public static List<PluginSaveData> PluginSaveData = new();

        public static void RegisterSaveLoadGameData<T>(string modGuid, T saveData, Action<T> saveEventCallback, Action<T> loadEventCallback)
        {
            // Create adapter functions to convert Action<T> to CallbackFunctionDelegate
            SaveGameCallbackFunctionDelegate saveCallbackAdapter = (object saveData) =>
            {
                if (saveEventCallback != null && saveData is T data)
                {
                    saveEventCallback(data);
                }
            };

            SaveGameCallbackFunctionDelegate loadCallbackAdapter = (object saveData) =>
            {
                if (loadEventCallback != null && saveData is T data)
                {
                    loadEventCallback(data);
                }
            };

            PluginSaveData.Add(new PluginSaveData { ModGuid = modGuid, SaveData = saveData, SaveEventCallback = saveCallbackAdapter, LoadEventCallback = loadCallbackAdapter });
        }
    }
}
