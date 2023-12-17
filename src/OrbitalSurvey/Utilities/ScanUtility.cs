using BepInEx.Logging;
using KSP.Game;
using OrbitalSurvey.Models;
using UnityEngine;
using Logger = BepInEx.Logging.Logger;

namespace OrbitalSurvey.Utilities;

public static class ScanUtility
{
    public static double UT => GameManager.Instance.Game.UniverseModel.UniverseTime;
    
    private static readonly ManualLogSource _LOGGER = Logger.CreateLogSource("OrbitalSurvey.ScanUtility");
    
    public static double GetScanRadius(MapType mapType, double bodyRadius, double altitude, float scanningCone)
    {
        var r = bodyRadius;
        var h = altitude;
        var alpha = DegreesToRadians((scanningCone / 2f));
        
        // the Law of sines: alpha / sin(r) = beta / sin(r + h)
        var beta = DegreesToRadians(180f) - Math.Asin(((r + h) * Math.Sin(alpha)) / r);

        // gamma = angle at the center of the celestial body from the point on the surface closest to the orbiting
        // vessel to the point on surface where the scanning cone finishes 
        var gamma = DegreesToRadians(180f) - alpha - beta;

        // gamma can be NaN in case the scanning cone exceeds the radius of the planet. In that case we set the angle
        // to 90° (half the sphere)
        if (double.IsNaN(gamma))
            gamma = DegreesToRadians(90);
        
        // clamp the angle to a maximum of 90° since that's the maximum that an orbiting vessel can see (half the sphere) 
        gamma = Math.Clamp(gamma, 0f, DegreesToRadians(90f));

        // radius from the point on the surface closest to the orbiting vessel
        var radiusOfScanningCone = (gamma * 2f * r * Math.PI) / DegreesToRadians(360);

        // apply a reduction factor if vessel is not at ideal altitude
        
        var factor = GetMinMaxReductionFactor(mapType, altitude);
        //_LOGGER.LogDebug($"Radius: {radiusOfScanningCone}. Factor: {factor}. Radius /w factor: {radiusOfScanningCone * factor}");
        radiusOfScanningCone *= factor;
        
        return radiusOfScanningCone;
    }

    public static (int x, int y) GetTextureCoordinatesFromGeographicCoordinates
        (double longitude, double latitude, int textureWidth, int textureHeight)
    {
        var x = textureWidth * ((longitude + 180) / 360f);
        var y = textureHeight * ((latitude + 90) / 180f);

        return ((int)x, (int)y);
    }

    public static double ConvertRealScanningRadiusToTextureScanningRadius(double realScanningRadius, double bodyRadius)
    {
        return (realScanningRadius / (2 * bodyRadius * Math.PI)) * Settings.ActiveResolution;
    }

    public static double RadiansToDegrees(double radians)
    {
        return radians * (180f / Math.PI);
    }

    public static double DegreesToRadians(double degrees)
    {
        return degrees * (Math.PI / 180f);
    }
    
    public static Texture2D ConvertToReadableTexture(Texture2D texture)
    {
        // Create a temporary RenderTexture of the same size as the texture
        RenderTexture tmp = RenderTexture.GetTemporary(
            texture.width,
            texture.height,
            0,
            RenderTextureFormat.Default,
            RenderTextureReadWrite.Linear);

        // Blit the pixels on texture to the RenderTexture
        Graphics.Blit(texture, tmp);

        // Backup the currently set RenderTexture
        RenderTexture previous = RenderTexture.active;

        // Set the current RenderTexture to the temporary one we created
        RenderTexture.active = tmp;

        // Create a new readable Texture2D to copy the pixels to it
        Texture2D myTexture2D = new Texture2D(texture.width, texture.height);

        // Copy the pixels from the RenderTexture to the new Texture
        myTexture2D.ReadPixels(new Rect(0, 0, tmp.width, tmp.height), 0, 0);
        myTexture2D.Apply();

        // Reset the active RenderTexture
        RenderTexture.active = previous;

        // Release the temporary RenderTexture
        RenderTexture.ReleaseTemporary(tmp);

        // "myTexture2D" now has the same pixels from "texture" and it's readable
        return myTexture2D;
    }

    public static double GetMercatorWidthCorrectionFactor(double latitude)
    {
        return MathF.Cos((float)latitude * Mathf.Deg2Rad);
    }

    public static double TextureYToLatitude(int y, int textureHeight)
    {
        // inverse of GetTextureCoordinatesFromGeographicCoordinates
        return ((180f * (double)y) / (double)textureHeight) - 90f;
    }
    
    private static void GetMinMaxIdealAltitudes(
        MapType mapType, out double minAlt, out double idealAlt, out double maxAlt)
    {
        minAlt = Settings.ModeScanningStats[mapType].MinAltitude;
        idealAlt = Settings.ModeScanningStats[mapType].IdealAltitude;
        maxAlt = Settings.ModeScanningStats[mapType].MaxAltitude;
    }

    private static double GetMinMaxReductionFactor(MapType mapType, double altitude)
    {
        double totalRange;

        GetMinMaxIdealAltitudes(mapType, out var minAlt, out var idealAlt, out var maxAlt);

        var currentAltDif = Math.Abs(idealAlt - altitude);
        
        if (altitude < idealAlt)
        {
            totalRange = Math.Abs(idealAlt - minAlt);
        }
        else
        {
            totalRange = Math.Abs(maxAlt - idealAlt);
        }
        
        var factor = Math.Clamp(1 - (currentAltDif / totalRange), 0, 1);

        return factor;
    }

    public static double GetRetroactiveTimeBetweenScans(double timeSinceLastScan)
    {
        if (timeSinceLastScan > 1000)
            return (double)Settings.TimeBetweenRetroactiveScansLow.Value;
        
        if (timeSinceLastScan > 100)
            return (double)Settings.TimeBetweenRetroactiveScansMid.Value;

        return (double)Settings.TimeBetweenRetroactiveScansHigh.Value;
    }

    /// <summary>
    /// Returns the state of the vessel in terms of its relation to ideal scanning altitude
    /// </summary>
    public static State GetAltitudeState(MapType mapType, double altitude)
    {
        GetMinMaxIdealAltitudes(mapType, out var minAlt, out var idealAlt, out var maxAlt);

        if (altitude < idealAlt * 0.95f)
        {
            if (altitude < minAlt)
            {
                return State.BelowMin;
            }
            
            return State.BelowIdeal;
        }

        if (altitude > idealAlt * 1.05f)
        {
            if (altitude > maxAlt)
            {
                return State.AboveMax;
            }
            
            return State.AboveIdeal;
        }

        return State.Ideal;
    }
}