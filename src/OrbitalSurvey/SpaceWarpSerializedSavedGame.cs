using KSP.Sim;
using Newtonsoft.Json;
using System.Reflection;

namespace OrbitalSurvey
{
    [Serializable]
    public class SpaceWarpSerializedSavedGame : SerializedSavedGame
    {
        public List<PluginSaveData<object>> PluginSaveData = new();

        public void CopyBaseData(SerializedSavedGame source)
        {
            foreach (FieldInfo field in source.GetType().GetFields())
            {
                object value = field.GetValue(source);

                try
                {
                    field.SetValue(this, value);
                }
                catch (FieldAccessException)
                { /* some fields are constants */ }
            }
        }

        /*
        void temp()
        {
            Appbar.RegisterAppButton(
            ModName,
            ToolbarFlightButtonID,
            AssetManager.GetAsset<Texture2D>($"{Info.Metadata.GUID}/images/icon.png"),
            isOpen =>
            {
                DEBUG_UI.Instance.IsDebugWindowOpen = isOpen;
                GameObject.Find(ToolbarFlightButtonID)?.GetComponent<UIValue_WriteBool_Toggle>()?.SetValue(isOpen);
            }
            );
        }
        */
    }

    public delegate void CallbackFunctionDelegate(object data);

    [Serializable]
    public class PluginSaveData
    {        
        public string ModGuid {get; set; }
        public object SaveData { get; set; }
        public CallbackFunctionDelegate CallBackFunction { get; set; }
    }

    public static class ModSaves
    {
        public static List<PluginSaveData> PluginSaveData = new();

        public static void RegisterSaveLoadGameData<T>(string modGuid, ref T saveData, Action<T> function)
        {
            // Create an adapter function to convert Action<T> to CallbackFunctionDelegate
            CallbackFunctionDelegate callbackAdapter = (object data) =>
            {
                if (data is T typedData)
                {
                    function(typedData);
                }
            };

            PluginSaveData.Add(new PluginSaveData { ModGuid = modGuid, SaveData = saveData, CallBackFunction = callbackAdapter });
        }
    }
}
