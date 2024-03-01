using KSP.Game;
using OrbitalSurvey.Models;

namespace OrbitalSurvey.Utilities;

public class NotificationUtility
{
    private NotificationUtility() { }
    public static NotificationUtility Instance { get; } = new();

    private readonly NotificationManager _manager = GameManager.Instance.Game.Notifications; 

    public void NotifyExperimentComplete(string body, ExperimentLevel level)
    {
        string title;
        string firstLine;
        
        if (level == ExperimentLevel.Full)
        {
            title = "OrbitalSurvey/Experiments/Notification/Complete/Title";
            firstLine = "OrbitalSurvey/Experiments/Notification/Complete/FirstLine";
        }
        else
        {
            title = "OrbitalSurvey/Experiments/Notification/Milestone/Title";
            firstLine = "OrbitalSurvey/Experiments/Notification/Milestone/FirstLine";
        }
        
        var notificationData = new NotificationData()
        {
            Importance = NotificationImportance.Low,
            Tier = NotificationTier.Alert,
            TimeStamp = Utility.UT,
            AlertTitle = new NotificationLineItemData()
            {
                ObjectParams = new object[]{ body },
                LocKey = title
            },
            FirstLine = new NotificationLineItemData()
            {
                LocKey = firstLine
            }
        };
        
        _manager.ProcessNotification(notificationData);
    }
}