using System.Threading.Tasks;
using KSP.Game;
using KSP.Messages;
using OrbitalSurvey.Debug;
using OrbitalSurvey.UI;
using OrbitalSurvey.Utilities;
using UnityEngine;


#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

namespace OrbitalSurvey.Managers
{
    public class MessageListener
    {
        private static readonly ReduxLib.Logging.ILogger Logger = ReduxLib.ReduxLib.GetLogger($"OrbitalSurvey|{typeof(MessageListener).Name}");
        private static MessageListener _instance;
        public MessageCenter MessageCenter => GameManager.Instance.Game.Messages;

        private MessageListener()
        {
        }

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
            Logger.LogInfo("Subscribed to GameLoadFinishedMessage.");
            MessageCenter.PersistentSubscribe<GameStateChangedMessage>(OnGameStateChangedMessage);
            Logger.LogInfo("Subscribed to GameStateChangedMessage.");
            MessageCenter.PersistentSubscribe<MapCelestialBodyAddedMessage>(OnMapCelestialBodyAddedMessage);
            Logger.LogInfo("Subscribed to MapCelestialBodyAddedMessage.");
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
                // Read category definitions from config first. This runs here (on game load)
                // rather than in OnInitialized so PatchManager has finished binding the
                // config values its Lua patches define before we read them.
                CelestialCategoryManager.Instance.InitializeConfigs();
                CelestialCategoryManager.Instance.InitializeCelestialBodyCategories();
            }

            DebugUI.Instance.InitializeControls();
            DebugUI.Instance.IsDebugWindowOpen = Settings.WILL_DEBUG_WINDOW_OPEN_ON_GAME_LOAD;
        }

        private void OnGameStateChangedMessage(MessageCenterMessage obj)
        {
            var msg = obj as GameStateChangedMessage;
            
            // Enable/disable scanning cone overlay when entering/leaving Map view
            GroundTrackingRenderer.Instance?.SetMapViewActive(msg.CurrentState == GameState.Map3DView);

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
}