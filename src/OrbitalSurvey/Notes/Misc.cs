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
 * OAB part description:
 * - scanning cone
 * - minimum altitude
 * - maximum altitude
 * - ideal altitude (do we need this?)
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