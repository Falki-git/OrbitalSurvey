using BepInEx.Logging;
using HarmonyLib;
using KSP.Game.Load;
using KSP.IO;
using KSP.Sim;
using Newtonsoft.Json;
using System.Reflection;

namespace OrbitalSurvey
{
    public class SaveLoadPatches
    {
        private static readonly ManualLogSource _logger = BepInEx.Logging.Logger.CreateLogSource("OrbitalSurvey.SaveLoadPatches");

        #region testbed
        /*
        [HarmonyPatch(typeof(SequentialFlow), "AddAction"), HarmonyPrefix]
        private static bool SaveOrbitalSurveyData(FlowAction action, SequentialFlow __instance)
        {
            _logger.LogDebug($"Action name: {action.Name}");

            if (action.Name == "Serializing Save File Contents")
            {
                _logger.LogDebug("Let's see what we have here...");
            }

            return true;
        }
        */

        /*
        [HarmonyPatch(typeof(SerializeGameDataFlowAction), "DoAction"), HarmonyPrefix]
        private static bool SaveOrbitalSurveyData2(Action resolve, Action<string> reject, SerializeGameDataFlowAction __instance)
        {
            _logger.LogDebug($"SerializeGameDataFlowAction.DoAction triggered.");

            return true;
        }
        */
        #endregion

        [HarmonyPatch(typeof(SerializeGameDataFlowAction), MethodType.Constructor), HarmonyPostfix]
        [HarmonyPatch(new Type[] { typeof(string), typeof(LoadGameData) })]
        private static void InjectPluginSaveGameData(string filename, LoadGameData data, SerializeGameDataFlowAction __instance)
        {
            _logger.LogDebug("SerializeGameDataFlowAction constructor postfix triggered");

            if (ModSaves.PluginSaveData.Count == 0)
                return;

            SpaceWarpSerializedSavedGame modSaveData = new();
            Utility.CopyFieldAndPropertyDataFromSourceToTargetObject(data.SavedGame, modSaveData);
            modSaveData.PluginSaveData = ModSaves.PluginSaveData;
            data.SavedGame = modSaveData;

            // Initiate save callbacks
            foreach (var plugin in ModSaves.PluginSaveData)
            {
                plugin.SaveEventCallback(plugin.SaveData);
            }
        }

        //////////////////  LOADING //////////////////

        #region testbed
        /*
        [HarmonyPatch(typeof(SaveLoadManager), "PrivateLoadCommon"), HarmonyPrefix]
        private static bool LoadTest_prefix(
            LoadOrSaveCampaignTicket loadOrSaveCampaignTicket,
            LoadGameData loadGameData,
            SequentialFlow loadingFlow,
            OnLoadOrSaveCampaignFinishedCallback onLoadOrSaveCampaignFinishedCallback,
            SaveLoadManager __instance)
        {
            _logger.LogDebug("SaveLoadManager.PrivateLoadCommon prefix triggered.");

            loadGameData.SavedGame = new MySerializedSavedGame();

            return true;
        }
        */

        /*
        [HarmonyPatch(typeof(SaveLoadManager), "PrivateLoadCommon"), HarmonyPostfix]
        private static void LoadTest_Postfix(
            LoadOrSaveCampaignTicket loadOrSaveCampaignTicket,
            LoadGameData loadGameData,
            SequentialFlow loadingFlow,
            OnLoadOrSaveCampaignFinishedCallback onLoadOrSaveCampaignFinishedCallback,
            SaveLoadManager __instance)
        {
            _logger.LogDebug("SaveLoadManager.PrivateLoadCommon postfix triggered.");
        }
        */
        #endregion

        [HarmonyPatch(typeof(DeserializeContentsFlowAction), "DoAction"), HarmonyPrefix]
        private static bool DeserializeLoadedPluginData(Action resolve, Action<string> reject, DeserializeContentsFlowAction __instance)
        {
            // Skip plugin deserialization if there are no mods that have registered for save/load actions
            if (ModSaves.PluginSaveData.Count == 0)
                return true;

            __instance._game.UI.SetLoadingBarText(__instance.Description);
            try
            {
                SpaceWarpSerializedSavedGame serializedSavedGame = new();
                IOProvider.FromJsonFile<SpaceWarpSerializedSavedGame>(__instance._filename, out serializedSavedGame);
                __instance._data.SavedGame = serializedSavedGame;
                __instance._data.DataLength = IOProvider.GetFileSize(__instance._filename);

                // Perform plugin load data if plugin data is found in the save file
                if (serializedSavedGame.PluginSaveData.Count > 0)
                {
                    // Iterate through each plugin
                    foreach (var loadedData in serializedSavedGame.PluginSaveData)
                    {
                        // Match registered plugin GUID with the GUID found in the save file
                        var existingData = ModSaves.PluginSaveData.Find(p => p.ModGuid == loadedData.ModGuid);                        
                        if (existingData == null)
                        {
                            _logger.LogWarning($"Saved data for plugin '{loadedData.ModGuid}' found during a load event, however that plugin isn't registered for save/load events. Skipping load for this plugin.");
                            continue;
                        }

                        // Perform a callback if plugin specified a callback function
                        existingData.LoadEventCallback(loadedData.SaveData);

                        // Copy loaded data to the SaveData object plugin registered
                        Utility.CopyFieldAndPropertyDataFromSourceToTargetObject(loadedData.SaveData, existingData.SaveData);
                    }
                }
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogException(ex);
                reject(ex.Message);
            }
            resolve();

            return false;
        }

    }

    public class PatchTest
    {
        private static readonly ManualLogSource _logger = BepInEx.Logging.Logger.CreateLogSource("OrbitalSurvey.PatchTest");

        // EMPTY ON PURPOSE
    }

    [Serializable]
    public class MySerializedSavedGame : SerializedSavedGame
    {
        [JsonProperty("MySaveGameField")]
        public string MyValue = "MyDefaultValue";

        public static MySerializedSavedGame CreateDerivedInstanceFromBase<T>(T baseInstance) where T : SerializedSavedGame
        {
            Type baseType = typeof(T);
            Type derivedType = typeof(MySerializedSavedGame);

            if (baseType.IsAssignableFrom(derivedType))
            {
                // Create an instance of the derived class using reflection
                object derivedObj = Activator.CreateInstance(derivedType);

                // Copy the properties from the base class instance to the derived class instance
                foreach (FieldInfo field in derivedType.GetFields())
                {
                    FieldInfo baseField = baseType.GetField(field.Name);
                    if (baseField != null)
                    {
                        object value = baseField.GetValue(baseInstance);
                        field.SetValue(derivedObj, value);
                    }
                }

                return (MySerializedSavedGame)derivedObj;
            }
            else
            {
                throw new InvalidOperationException("The provided type does not derive from the expected base class.");
            }
        }
    }
}
