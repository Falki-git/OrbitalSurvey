using BepInEx.Logging;
using KSP.Game;
using KSP.Messages;

namespace OrbitalSurvey
{
    public class MessageListener
    {
        private ManualLogSource _logger = BepInEx.Logging.Logger.CreateLogSource("OrbitalSurvey.MessageListener");
        private static MessageListener _instance;
        public MessageCenter MessageCenter => GameManager.Instance.Game.Messages;
        public GameStateConfiguration GameState => GameManager.Instance.Game.GlobalGameState.GetGameState();

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
            await Task.Delay(1000);

            MessageCenter.PersistentSubscribe<GameLoadFinishedMessage>(OnGameLoadFinishedMessage);
            _logger.LogInfo("Subscribed to GameLoadFinishedMessage.");
        }

        private void OnGameLoadFinishedMessage(MessageCenterMessage message)
        {
            _logger.LogDebug("GameLoadFinishedMessage triggered.");
            DEBUG_UI.Instance.IsDebugWindowOpen = DEBUG_UI.WILL_DEBUG_WINDOW_OPEN_ON_GAME_LOAD;
        }
    }
}
