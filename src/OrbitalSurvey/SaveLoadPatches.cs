using BepInEx.Logging;
using HarmonyLib;
using KSP.Game.Flow;
using KSP.Game.Load;
using KSP.Sim;
using Newtonsoft.Json;
using System.Reflection;

namespace OrbitalSurvey
{
    public class SaveLoadPatches
    {
        private static readonly ManualLogSource _logger = BepInEx.Logging.Logger.CreateLogSource("OrbitalSurvey.SaveLoadPatches");

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

        [HarmonyPatch(typeof(SerializeGameDataFlowAction), "DoAction"), HarmonyPrefix]
        private static bool SaveOrbitalSurveyData2(Action resolve, Action<string> reject, SerializeGameDataFlowAction __instance)
        {
            _logger.LogDebug($"SerializeGameDataFlowAction.DoAction triggered.");

            return true;
        }

        [HarmonyPatch(typeof(SerializeGameDataFlowAction), MethodType.Constructor), HarmonyPostfix]
        [HarmonyPatch(new Type[] { typeof(string), typeof(LoadGameData) })]
        private static void InjectCustomLoadGameData(string filename, LoadGameData data, SerializeGameDataFlowAction __instance)
        {
            _logger.LogDebug("SerializeGameDataFlowAction contructor postfix triggered");

            MySerializedSavedGame myData = MySerializedSavedGame.CreateDerivedInstanceFromBase(data.SavedGame);
            myData.MyValue = "Hello World";

            data.SavedGame = myData;
        }
    }

    [Serializable]
    public class MySerializedSavedGame : SerializedSavedGame
    {
        [JsonProperty("MySaveGameField")]
        public string MyValue = "my value";

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
