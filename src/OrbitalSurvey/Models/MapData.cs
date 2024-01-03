using OrbitalSurvey.Managers;
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
        DiscoveredPixels = new bool[Settings.ActiveResolution, Settings.ActiveResolution];
    }

    public Texture2D ScannedMap { get; set; }
    public Texture2D HiddenMap { get; set; }
    public Texture2D CurrentMap { get; set; }
    public bool[,] DiscoveredPixels { get; set; }
    private readonly List<(int, int)> _bufferedDiscoveredPixels = new();
    public bool IsFullyScanned { get; set; }

    public int DiscoveredPixelsCount;
    public int TotalPixelCount => Settings.ActiveResolution * Settings.ActiveResolution;
    public float PercentDiscovered => (float)DiscoveredPixelsCount / TotalPixelCount;

    public ExperimentLevel ExperimentLevel;

    public delegate void DiscoveredPixelCountChanged(float percentDiscovered);
    public event DiscoveredPixelCountChanged OnDiscoveredPixelCountChanged;

    public bool HasData
    {
        get => _hasData;
        set
        {
            if (value != _hasData)
            {
                _hasData = value;
                Core.Instance.InvokeOnMapHasDataValueChanged();
            }
        }
    }
    
    private bool _hasData;

    public void MarkAsScanned(int x, int y, int scanningRadius, bool isRetroActiveScanning)
    {
        // set the radius to at least 1 pixel
        scanningRadius = scanningRadius == 0 ? 1 : scanningRadius;
        
        int newlyDiscoveredPixelCount = 0;
        // start with Y coordinate cause the width of the scanning area depends on latitude, due to mercator projection
        for (int j = y - scanningRadius; j < y + scanningRadius; j++)
        {
            // if pixel is out of bounds of the texture, don't paint it. Could be done better, but good enough for now
            if (j < 0 || j > CurrentMap.height)
                continue;
         
            // divide the width radius by 2 cause the texture's AR is 1:1, but the real AR should be 2:1
            var trueWidthRadius = (int)((scanningRadius / 2f));
            if (trueWidthRadius == 0)
            {
                trueWidthRadius = 1;
            }
            
            // due to mercator projection, area towards the poles gets distorted, so we need to apply a correction
            var latitudeOfYPixel= ScanUtility.TextureYToLatitude(j, Settings.ActiveResolution);
            trueWidthRadius = (int)(trueWidthRadius / ScanUtility.GetMercatorWidthCorrectionFactor(latitudeOfYPixel));

            // clamp the width's scanning radius to a maximum of half the width of the texture (diameter = entire body)
            var halfOfTextureWidth = Settings.ActiveResolution / 2;
            if(trueWidthRadius > halfOfTextureWidth) 
                trueWidthRadius = halfOfTextureWidth;
            
            for (int i = x - trueWidthRadius; i < x + trueWidthRadius; i++)
            {
                // handle edge case: if x is near the edge of the texture we need to paint the other side too   
                var xPixel = i < 0 ? CurrentMap.width + i : i > CurrentMap.width-1 ? i - CurrentMap.width : i;
                
                /*
                // Mark pixels as discovered
                DiscoveredPixels[xPixel, j] = true;
                
                // Update the current map with discovered pixels, but only if it's NOT a retroactive scan
                CurrentMap.SetPixel(xPixel, j, ScannedMap.GetPixel(xPixel, j));
                */

                if (DiscoveredPixels[xPixel, j] == false)
                {
                    // If it's retroactive scanning, we'll buffer the newly discovered pixels until a full 
                    // retroactive scan is complete. Only then we'll update the texture.
                    // This is done to increase performance
                    if (isRetroActiveScanning)
                    {
                        _bufferedDiscoveredPixels.Add((xPixel, j));
                    }
                    else
                    {
                        CurrentMap.SetPixel(xPixel, j, ScannedMap.GetPixel(xPixel, j));    
                    }
                    
                    DiscoveredPixels[xPixel, j] = true;
                    newlyDiscoveredPixelCount++;

                    if (PercentDiscovered >= 0.95f)
                    {
                        SetAsFullyScanned();
                        OnDiscoveredPixelCountChanged?.Invoke(PercentDiscovered);
                        return;
                    }
                }
            }
        }

        if (newlyDiscoveredPixelCount > 0)
        {
            DiscoveredPixelsCount += newlyDiscoveredPixelCount;
            HasData = true;
            OnDiscoveredPixelCountChanged?.Invoke(PercentDiscovered);
        }

        // Skip applying new pixels if it's a retroactive scan (will be applied later)
        if (!isRetroActiveScanning)
        {
            // If there are buffered pixels after retroactive scanning, paint them now
            if (_bufferedDiscoveredPixels.Count > 0)
            {
                foreach (var pixel in _bufferedDiscoveredPixels)
                {
                    CurrentMap.SetPixel(pixel.Item1, pixel.Item2,
                        ScannedMap.GetPixel(pixel.Item1, pixel.Item2));
                }
                
                _bufferedDiscoveredPixels.Clear();
            }
            
            CurrentMap.Apply();
        }
    }

    public void ClearMap()
    {
        Array.Clear(DiscoveredPixels, 0, DiscoveredPixels.Length);
        DiscoveredPixelsCount = 0;
        HasData = false;
        UpdateCurrentMapAsPerDiscoveredPixels();
        IsFullyScanned = false;
        ExperimentLevel = ExperimentLevel.None;
    }

    public void UpdateDiscoveredPixels(bool[,] loadedPixels, bool loadedDataIsFullyScanned = false)
    {
        // check if loaded data says that map is fully scanned
        if (loadedDataIsFullyScanned)
        {
            if (this.IsFullyScanned)
            {
                // local map is already fully revealed, do nothing
            }
            else
            {
                // loaded data indicates that map is fully scanned, but local map isn't fully revealed yet.
                // set the local map to fully scanned and update the texture to fully revealed 
                SetAsFullyScanned();
            }
        }
        else
        {
            // map is partially scanned, update the map accordingly
            this.IsFullyScanned = false;
            this.HasData = true;
            DiscoveredPixels = SaveUtility.CopyArrayData(loadedPixels, out DiscoveredPixelsCount);
            UpdateCurrentMapAsPerDiscoveredPixels();    
        }
    }

    public void UpdateCurrentMapAsPerDiscoveredPixels()
    {
        Color emptyPixel = HiddenMap.GetPixel(0, 0);
        for (int i = 0; i < CurrentMap.width; i++)
        {
            for (int j = 0; j < CurrentMap.height; j++)
            {
                Color pixelColor = DiscoveredPixels[i, j] ? ScannedMap.GetPixel(i, j) : emptyPixel;
                CurrentMap.SetPixel(i, j, pixelColor);
            }
        }
        
        CurrentMap.Apply();
    }

    [Obsolete]
    public bool CheckIfMapIsFullyScannedNow()
    {
        var truePixels = 0;
        var allPixelCount = DiscoveredPixels.GetLength(0) * DiscoveredPixels.GetLength(1);
        
        for (int i = 0; i < DiscoveredPixels.GetLength(0); i++)
        {
            for (int j = 0; j < DiscoveredPixels.GetLength(1); j++)
            {
                // check if pixel is discovered
                if (DiscoveredPixels[i, j])
                {
                    // pixel is discovered. Increase discovered pixel count and move to the next pixel.
                    truePixels++;
                }
            }
        }
        
        if ((float)truePixels / allPixelCount >= 0.95f)
        {
            // more than 95% is discovered
            // we'll reward the extra 5% for free and mark this as fully discovered
            SetAsFullyScanned();
            return true;
        }

        // less than 95% is discovered
        return false;
    }

    private void SetAsFullyScanned()
    {
        this.IsFullyScanned = true;
        this.HasData = true;
        this.DiscoveredPixelsCount = TotalPixelCount;
        Graphics.CopyTexture(ScannedMap, CurrentMap);
        CurrentMap.Apply();
    }

    public ExperimentLevel CheckIfExperimentNeedsToBeTriggered()
    {
        var mapPercentage = PercentDiscovered;
        bool levelChanged = false;
        
        switch (ExperimentLevel)
        {
            case ExperimentLevel.Full: return ExperimentLevel.None;
            
            case ExperimentLevel.ThreeQuarters:
                if (IsFullyScanned)
                {
                    ExperimentLevel = ExperimentLevel.Full;
                    levelChanged = true;
                }
                break;
            case ExperimentLevel.Half:
                if (mapPercentage >= 0.75f)
                {
                    ExperimentLevel = ExperimentLevel.ThreeQuarters;
                    levelChanged = true;
                }
                break;
            case ExperimentLevel.Quarter:
                if (mapPercentage >= 0.50f)
                {
                    ExperimentLevel = ExperimentLevel.Half;
                    levelChanged = true;
                }
                break;
            case ExperimentLevel.None:
                if (mapPercentage >= 0.25f)
                {
                    ExperimentLevel = ExperimentLevel.Quarter;
                    levelChanged = true;
                }
                break;
        }

        return levelChanged ? ExperimentLevel : ExperimentLevel.None; 
    }
}