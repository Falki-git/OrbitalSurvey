using UnityEngine;

namespace OrbitalSurvey.Models;

public class MapData
{
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
                // Handle edge cases: if x or y are near the edges of the texture we need to paint other sides too   
                var xPixel = i < 0 ? CurrentMap.width + i : i > CurrentMap.width-1 ? i - CurrentMap.width : i;
                var yPixel = j < 0 ? CurrentMap.height + j : j > CurrentMap.height-1 ? j - CurrentMap.height : j;
                
                // Mark pixels as discovered
                DiscoveredPixels[xPixel, yPixel] = true; // TODO optimize with lower resolution of the array
                
                // Update the current map with discovered pixels
                CurrentMap.SetPixel(xPixel, yPixel, ScannedMap.GetPixel(xPixel, yPixel));
            }
        }
    }
}