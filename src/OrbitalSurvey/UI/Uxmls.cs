using BepInEx.Logging;
using SpaceWarp.API.Assets;
using UnityEngine.UIElements;

namespace OrbitalSurvey.UI;

public class Uxmls
{
    public static Uxmls Instance { get; } = new();
    
    public VisualTreeAsset MainGui;
    
    private const string _MAIN_GUI_PATH = "/orbitalsurvey_ui/ui/orbitalsurvey.uxml";
    
    private static readonly ManualLogSource _LOGGER = Logger.CreateLogSource("OrbitalSurvey.Uxmls");

    private Uxmls()
    {
        Initialize();
    }

    private void Initialize()
    {
        MainGui = LoadAsset($"{_MAIN_GUI_PATH}");
    }

    private VisualTreeAsset LoadAsset(string path)
    {
        try
        {
            return AssetManager.GetAsset<VisualTreeAsset>($"{OrbitalSurveyPlugin.ModGuid}{path}");
        }
        catch (Exception ex)
        {
            _LOGGER.LogError($"Failed to load VisualTreeAsset at path \"{OrbitalSurveyPlugin.ModGuid}{path}\"\n" + ex.Message);
            return null;
        }
    }
}