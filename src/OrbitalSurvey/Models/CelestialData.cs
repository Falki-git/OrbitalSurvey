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
    
    public CelestialBodyComponent Body { get; set;  }
    public Dictionary<MapType, MapData> Maps { get; set; }
    
    public string Name => Body.Name;
    public string DisplayName => Body.DisplayName;
    public double Radius => Body.radius;
    public double AtmosphereDepth => Body.atmosphereDepth;

    public double MinimumScanningAltitude => AtmosphereDepth; // TODO
    public double MaximumScanningAltitude => Body.sphereOfInfluence; // TODO
    
    public void DoScan(MapType mapType, double longitude, double latitude, double altitude, double scanningCone)
    {
        var scanningRadius = ScanUtility.GetScanRadius(Radius, altitude, scanningCone);

        var (textureX, textureY) = ScanUtility.GetTextureCoordinatesFromGeographicCoordinates(longitude, latitude, 4096, 4096);

        var map = Maps[mapType];
        
        map.MarkAsScanned(textureX, textureY, scanningRadius);
    }
}