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

    public void MarkAsScanned(int x, int y, int scanningRadius, double latitude)
    {
        // start with Y coordinate cause the width of the scanning area depends on latitude, due to mercator projection
        for (int j = y - scanningRadius; j < y + scanningRadius; j++)
        {
            // if pixel is out of bounds of the texture, don't paint it
            if (j < 0 || j > CurrentMap.height)
                continue;
         
            // divide the width radius by 2 cause the texture's AR is 1:1, but the real AR should be 2:1
            var trueWidthRadius = (int)((scanningRadius / 2f));
            
            // due to mercator projection, area towards the poles gets distorted, so we need to apply a correction
            var latitudeOfYPixel= ScanUtility.TextureYToLatitude(j, Settings.MAP_RESOLUTION.Item2);
            trueWidthRadius = (int)(trueWidthRadius / ScanUtility.GetMercatorWidthCorrectionFactor(latitudeOfYPixel));

            // clamp the width's scanning radius to a maximum of half the width of the texture (diameter = entire body)
            var halfOfTextureWidth = Settings.MAP_RESOLUTION.Item1 / 2;
            if(trueWidthRadius > halfOfTextureWidth) 
                trueWidthRadius = halfOfTextureWidth;
            
            for (int i = x - trueWidthRadius; i < x + trueWidthRadius; i++)
            {
                // handle edge case: if x is near the edge of the texture we need to paint the other side too   
                var xPixel = i < 0 ? CurrentMap.width + i : i > CurrentMap.width-1 ? i - CurrentMap.width : i;
                
                // Mark pixels as discovered
                //DiscoveredPixels[xPixel, yPixel] = true; // TODO optimize with lower resolution of the array
                
                // Update the current map with discovered pixels
                CurrentMap.SetPixel(xPixel, j, ScannedMap.GetPixel(xPixel, j));
            }
        }

        CurrentMap.Apply();
    }

    public void ClearMap()
    {
        for (int i = 0; i < CurrentMap.width; i++)
        {
            for (int j = 0; j < CurrentMap.height; j++)
            {
                DiscoveredPixels[i, j] = false;
                CurrentMap.SetPixel(i, j, Color.clear);
            }
        }
        CurrentMap.Apply();
    }
}