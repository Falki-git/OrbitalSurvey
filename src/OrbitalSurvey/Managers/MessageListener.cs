using BepInEx.Logging;
using KSP.Game;
using KSP.Messages;
using OrbitalSurvey.UI;
using OrbitalSurvey.Utilities;

namespace OrbitalSurvey.Managers;

public class MessageListener
{
    private ManualLogSource _logger = Logger.CreateLogSource("OrbitalSurvey.MessageListener");
    private static MessageListener _instance;
    public MessageCenter MessageCenter => GameManager.Instance.Game.Messages;
    //public GameStateConfiguration GameState => GameManager.Instance.Game.GlobalGameState.GetGameState();

    private MessageListener()
    { }

    public static MessageListener Instance
    {
        get
        {
            if (_instance == null)
                _instance = new MessageListener();
            return _instance;
        }
    }

    public void SubscribeToMessages() => _ = Subscribe();

    private async Task Subscribe()
    {
        await Task.Delay(100);

        MessageCenter.PersistentSubscribe<GameLoadFinishedMessage>(OnGameLoadFinishedMessage);
        _logger.LogInfo("Subscribed to GameLoadFinishedMessage.");
        MessageCenter.PersistentSubscribe<GameStateChangedMessage>(OnGameStateChangedMessage);
        _logger.LogInfo("Subscribed to GameStateChangedMessage.");
    }
    
    private void OnGameLoadFinishedMessage(MessageCenterMessage message)
    {
        _logger.LogDebug("GameLoadFinishedMessage triggered.");

        if (!Core.Instance.MapsInitialized)
        { 
            // OrbitalSurveyPlugin.Instance.assetUtility.InitializeVisualTextures();
            Core.Instance.InitializeCelestialData();
        }
        
        DEBUG_UI.Instance.IsDebugWindowOpen = Settings.WILL_DEBUG_WINDOW_OPEN_ON_GAME_LOAD;
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

        if(SceneController.Instance.ShowMainGui)
            SceneController.Instance.ShowMainGui = false;
        
        OverlayManager.Instance.RemoveOverlay();
    }
}