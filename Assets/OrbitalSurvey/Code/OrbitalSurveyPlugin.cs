using SpaceWarp2.API.Mods;
namespace OrbitalSurvey
{
    /* Extend KerbalMod instead if you need the MonoBehaviour update loop/references to game stuff like SW 1.x mods */
    public class OrbitalSurveyPlugin : GeneralMod
    {
        public override void OnPreInitialized() {
            /*
                Code that runs before addressables/assets are loaded goes here
                This is where you want to register loading actions or other such things 
            */
        }

        public override void OnInitialized() {
            /*
                Code that runs after addressables/assets are loaded goes here
                You are also generally free to interact with game code here
            */
            SWLogger.LogInfo("Hello World!");
        }

        public override void OnPostInitialized() {
            /*
                Code that runs after all mods have been initialized goes here
            */
        }
    }
}