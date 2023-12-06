using KSP.Sim.impl;
using OrbitalSurvey.Utilities;

namespace OrbitalSurvey.Models;

public class CelestialData
{
    public CelestialData()
    {
        Maps = new();
        Maps.Add(MapType.Visual, new MapData());
        Maps.Add(MapType.Biome, new MapData());
    }

    public CelestialBodyComponent Body { get; set; }
    public Dictionary<MapType, MapData> Maps { get; set; }

    public bool ContainsData
    {
        get
        {
            var trueFound = false;
            
            foreach (var map in Maps.Values)
            {
                if (trueFound) break;

                if (map.HasData)
                {
                    trueFound = true;
                }
            }

            return trueFound;
        }
    }
    
    public string Name => Body.Name;
    public string DisplayName => Body.DisplayName;
    public double Radius => Body.radius;
    public double AtmosphereDepth => Body.atmosphereDepth;

    public double MinimumScanningAltitude => AtmosphereDepth; // TODO
    public double MaximumScanningAltitude => Body.sphereOfInfluence; // TODO
    
    public void DoScan(MapType mapType, double longitude, double latitude, double altitude, float scanningCone)
    {
        var map = Maps[mapType];

        // if map is fully scanned, don't do anything
        if (map.IsFullyScanned)
            return;
        
        // calculate what the scanning radius is given the altitude and the field of view of the scanner
        var scanningRadius = ScanUtility.GetScanRadius(mapType, Radius, altitude, scanningCone);

        // need to adjust the scanning radius to take into account the resolution of the texture
        var scanningRadiusForTexture =
            ScanUtility.ConvertRealScanningRadiusToTextureScanningRadius(scanningRadius, Radius);

        var (textureX, textureY) =
            ScanUtility.GetTextureCoordinatesFromGeographicCoordinates(longitude, latitude,  Settings.ActiveResolution, Settings.ActiveResolution);
        
        map.MarkAsScanned(textureX, textureY, (int)scanningRadiusForTexture);
    }

    public void ClearMap(MapType mapType)
    {
        var map = Maps[mapType];
        map.ClearMap();
    }
}