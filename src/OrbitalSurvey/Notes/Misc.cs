/*
 *
 * Unity UI colors:
 * - <color=red>red \u26a0</color><color=yellow>yellow \u26a0</color><color=grey>grey </color><color=white>white </color><color=blue>blue </color>
 *   <color=black>black </color><color=green>green </color><color=#00ffffff>cyan </color>
 *   <color=#c0c0c0ff>silver </color><color=lightblue>lightblue </color><size=10>size10</size>
 *
 *
 * 
 * if (GUILayout.Button("QuickSave sound"))
 * {
 *      KSPAudioEventManager.onGameQuickSave();
 *      KSPAudioEventManager.OnResourceTransfertStart();
 * }
 *
 *
 *
 *
 *
 * ATTRIBUTION:
 * - icon by flaticon.com - <a href="https://www.flaticon.com/free-icons/satellite" title="satellite icons">Satellite icons created by mattbadal - Flaticon</a>
 *
 *
 * FOR DOCS/WIKI:
 *
 *
 
 
 //OAB PART MODULE'S DESCRIPTION
 
 public override List<OABPartData.PartInfoModuleEntry> GetPartInfoEntries(Type partBehaviourModuleType,
        List<OABPartData.PartInfoModuleEntry> delegateList)
    {
        if (partBehaviourModuleType == ModuleType)
        {
            delegateList.Add(new OABPartData.PartInfoModuleEntry("Only display name"));
            delegateList.Add(new OABPartData.PartInfoModuleEntry("Single entry", new OABPartData.PartInfoModuleEntryValueDelegate(TestSingleEntryValue)));
            delegateList.Add(new OABPartData.PartInfoModuleEntry("Multi entries", TestMultipleEntries));
            delegateList.Add(new("", new OABPartData.PartInfoModuleEntryValueDelegate(TestOnlyEntryValue)));
        }

        return delegateList;
    }
    
    private string TestSingleEntryValue(OABPartData.OABSituationStats oabSituationStats)
    {
        return "Single value";
    }
    
    private string TestOnlyEntryValue(OABPartData.OABSituationStats oabSituationStats)
    {
        return "Only display value";
    }
    
    private List<OABPartData.PartInfoModuleSubEntry> TestMultipleEntries(OABPartData.OABSituationStats oabSituationStats)
    {
        List<OABPartData.PartInfoModuleSubEntry> list = new List<OABPartData.PartInfoModuleSubEntry>();
        List<OABPartData.PartInfoModuleSubEntry> list2 = new List<OABPartData.PartInfoModuleSubEntry>();
        list2.Add(new OABPartData.PartInfoModuleSubEntry("sub-sub-entry (2nd level)"));
        
        list.Add(new OABPartData.PartInfoModuleSubEntry("Only subentry's name"));
        list.Add(new OABPartData.PartInfoModuleSubEntry("Sub entry name", "sub entry value"));
        list.Add(new OABPartData.PartInfoModuleSubEntry("New sub entry level", list2));

        return list;
    }
 *
 *
 *

var notification = GameManager.Instance.Game.Notifications;

var tex = SpaceWarp.API.Assets.AssetManager.GetAsset<Texture2D>(
$"falki.orbital_survey/images/icon.png");
var sprite = UnityEngine.Sprite.Create(tex, new UnityEngine.Rect(0, 0, tex.width, tex.height), new UnityEngine.Vector2(0.5f, 0.5f));

var notificationData = new KSP.Game.NotificationData()
{
    Importance = NotificationImportance.Low,
    Tier = NotificationTier.Alert,
    TimeStamp = OrbitalSurvey.Utilities.Utility.UT,
    AlertTitle = new NotificationLineItemData()
        {
            Icon = sprite,
            ObjectParams = new object[]{ "test" },
            LocKey = "OrbitalSurvey/Experiments/Notification"
        },
        FirstLine = new NotificationLineItemData()
            {
                LocKey = "something goes here"
            }
        };
    notification.ProcessNotification(notificationData);
}


var notification = GameManager.Instance.Game.Notifications;

var tex = SpaceWarp.API.Assets.AssetManager.GetAsset<Texture2D>(
$"falki.orbital_survey/images/icon.png");
var sprite = UnityEngine.Sprite.Create(tex, new UnityEngine.Rect(0, 0, tex.width, tex.height), new UnityEngine.Vector2(0.5f, 0.5f));

var notificationData = new KSP.Game.NotificationData()
    {
       Importance = NotificationImportance.Low,
       Tier = NotificationTier.Alert,
       TimeStamp = OrbitalSurvey.Utilities.Utility.UT,
       AdminTitle = new NotificationLineItemData()
                {
                    Icon = sprite,
                    ObjectParams = new object[]{ "test" },
                    LocKey = "OrbitalSurvey/Experiments/Notification"
                },
       FirstLine = new NotificationLineItemData()
                {
                    LocKey = "something goes here"
                }
,
            };
            notification.ProcessNotification(notificationData);

Visual:
antenna_0v_dish_ra-2,RA-2 [LONG-RANGE PROBES]
antenna_1v_parabolic_dts-m1,Communotron DTS-M1 [ENHANCED ELECTRONICS]
antenna_1v_dish_hg55,Communotron HG-55 [DEEP-SPACE PROBES]
antenna_1v_dish_88-88,Communotron 88-88 [LONG-RANGE GENERATION]

Biome:
antenna_0v_dish_ra-15,RA-15 [DURABLE POWER SYSTEMS]
antenna_1v_dish_ra-100,RA-100 [LONG-RANGE GENERATION]

n/a:
antenna_0v_16,Communotron 16 [PROBES]
antenna_0v_16s,Communotron 16-S [PROBES]
antenna_1v_dish_hg5,Communotron HG-5 [LONG-RANGE PROBES]










 *
 *
 *
 *
 *
 *
 *
 *
 *
 *
 *
 *
 * 
*/