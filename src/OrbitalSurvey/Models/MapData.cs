using UnityEngine;
using OrbitalSurvey.Utilities;

namespace OrbitalSurvey.Models;

public class MapData
{
    public MapData()
    {
        ScannedMap = AssetUtility.GenerateHiddenMap();
        HiddenMap = AssetUtility.GenerateHiddenMap();
        CurrentMap = AssetUtility.GenerateHiddenMap();
        DiscoveredPixels = new bool[Settings.MAP_RESOLUTION.Item1, Settings.MAP_RESOLUTION.Item2];
    }
    
    public Texture2D ScannedMap { get; set; }
    public Texture2D HiddenMap { get; set; }
    public Texture2D CurrentMap { get; set; }
    public bool[,] DiscoveredPixels { get; set; }

    public void MarkAsScanned(int x, int y, int radius)
    {
        // TODO rewrite this to take into account mercator projection where radius width needs to shrink when going towards poles
        for (int i = x - radius; i < x + radius; i++) // TODO make scanning cone circular
        {
            for (int j = y - radius; j < y + radius; j++)
            {
                // Handle edge cases: if x is near the edges of the texture we need to paint other sides too   
                var xPixel = i < 0 ? CurrentMap.width + i : i > CurrentMap.width-1 ? i - CurrentMap.width : i;

                // TODO handle out of bounds for Y better 
                var yPixel = j < 0 ? j = 0 : j > CurrentMap.height ? j = CurrentMap.height : j = j;
                
                // Mark pixels as discovered
                DiscoveredPixels[xPixel, yPixel] = true; // TODO optimize with lower resolution of the array
                
                // Update the current map with discovered pixels
                CurrentMap.SetPixel(xPixel, yPixel, ScannedMap.GetPixel(xPixel, yPixel));
            }
        }
    }
}