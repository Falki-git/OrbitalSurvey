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
    
    public void DoScan(MapType mapType, double longitude, double latitude, double altitude, ScanningStats scanningStats, bool isRetroActiveScanning)
    {
        var map = Maps[mapType];

        // if map is fully scanned, don't do anything
        if (map.IsFullyScanned)
            return;
        
        // calculate what the scanning radius is given the altitude and the field of view of the scanner
        var scanningRadius = ScanUtility.GetScanRadius(Radius, altitude, scanningStats);

        // need to adjust the scanning radius to take into account the resolution of the texture
        var scanningRadiusForTexture =
            ScanUtility.ConvertRealScanningRadiusToTextureScanningRadius(scanningRadius, Radius);

        var (textureX, textureY) =
            ScanUtility.GetTextureCoordinatesFromGeographicCoordinates(latitude, longitude);
        
        map.MarkAsScanned(textureX, textureY, (int)scanningRadiusForTexture, isRetroActiveScanning);
    }

    public void ClearMap(MapType mapType)
    {
        var map = Maps[mapType];
        map.ClearMap();
    }

    public ExperimentLevel CheckIfExperimentNeedsToTrigger(MapType mapType)
    {
        var map = Maps[mapType];
        return map.CheckIfExperimentNeedsToBeTriggered();
    }
}