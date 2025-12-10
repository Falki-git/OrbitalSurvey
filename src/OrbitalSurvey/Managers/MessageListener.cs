using BepInEx.Logging;
using KSP.Game;
using KSP.Messages;
using OrbitalSurvey.Debug;
using OrbitalSurvey.Missions.Managers;
using OrbitalSurvey.UI;
using OrbitalSurvey.Utilities;
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

namespace OrbitalSurvey.Managers;

public class MessageListener : ManagerBase<MessageListener>
{
    private MessageListener() { }
    
    public MessageCenter MessageCenter => GameManager.Instance.Game.Messages;

    public override void Initialize() { }

    public void SubscribeToMessages() => _ = Subscribe();

    private async Task Subscribe()
    {
        await Task.Delay(100);

        MessageCenter.PersistentSubscribe<GameLoadFinishedMessage>(OnGameLoadFinishedMessage);
        Logger.LogInfo("Subscribed to GameLoadFinishedMessage.");
        MessageCenter.PersistentSubscribe<GameStateChangedMessage>(OnGameStateChangedMessage);
        Logger.LogInfo("Subscribed to GameStateChangedMessage.");
        MessageCenter.PersistentSubscribe<MapCelestialBodyAddedMessage>(OnMapCelestialBodyAddedMessage);
        Logger.LogInfo("Subscribed to MapCelestialBodyAddedMessage.");
        MessageCenter.PersistentSubscribe<OnMissionTriumphDismissed>(DebugManager.Instance.OnMissionTriumphDismissed);
    }

    private void OnGameLoadFinishedMessage(MessageCenterMessage message)
    {
        Logger.LogDebug("GameLoadFinishedMessage triggered.");

        if (!Core.Instance.MapsInitialized)
        { 
            // OrbitalSurveyPlugin.Instance.assetUtility.InitializeVisualTextures();
            Core.Instance.InitializeCelestialData();
        }

        // if another session's data is loaded, need to reinitialize data
        if (Core.Instance.SessionGuidString != Utility.SessionGuidString)
        {
            Logger.LogInfo("New SessionGuidString detected. Resetting data.");
            Core.Instance.InitializeCelestialData();
        }

        if (!CelestialCategoryManager.Instance.IsCelestialBodyCategoryInitialized)
        {
            CelestialCategoryManager.Instance.InitializeCelestialBodyCategories();
        }
        
        DebugUI.Instance.InitializeControls();
        DebugUI.Instance.IsDebugWindowOpen = Settings.WILL_DEBUG_WINDOW_OPEN_ON_GAME_LOAD;
        
        // initialize missions
        MissionManager.Instance.Initialize();
        // TODO load mission data ??? or we'll load them via SaveManager?
    }
    
    private void OnGameStateChangedMessage(MessageCenterMessage obj)
    {
        var msg = obj as GameStateChangedMessage;
        
        // Close GUI and remove overlay on every scene change except Flight <-> Map
        if (msg.PreviousState == GameState.FlightView && 
            (msg.CurrentState == GameState.Map3DView || msg.CurrentState == GameState.FlightView))
        {
            return;
        }
        
        if (msg.PreviousState == GameState.Map3DView && 
            (msg.CurrentState == GameState.Map3DView || msg.CurrentState == GameState.FlightView))
        {
            return;
        }

        if (SceneController.Instance.ShowMainGui)
            SceneController.Instance.ToggleUI(false);
        
        OverlayManager.Instance.RemoveOverlay();
    }
    
    private void OnMapCelestialBodyAddedMessage(MessageCenterMessage obj)
    {
        var bodyName = ((MapCelestialBodyAddedMessage)obj).bodyData.Data.bodyName;
        OverlayManager.Instance.DrawMap3dOverlayOnMapCelestialBodyAddedMessage(bodyName);
    }
}