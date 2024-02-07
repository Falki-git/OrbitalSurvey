using OrbitalSurvey.UI;
using UnityEngine;

namespace OrbitalSurvey.Utilities;

public static class UiUtility
{
    public static Vector2 GetAdjustedCanvasCoordinatesFromLocalPosition(Vector2 localPosition, float canvasWidth, float canvasHeight)
    {
        var mapPositionPercentage = new Vector2(localPosition.x / canvasWidth, localPosition.y / canvasHeight);

        return GetAdjustedCanvasCoordinatesFromMapPositionPercentage(mapPositionPercentage, canvasWidth, canvasHeight);
    }
    
    public static Vector2 GetAdjustedCanvasCoordinatesFromMapPositionPercentage(Vector2 mapPositionPercentage,
        float canvasWidth, float canvasHeight)
    {
        var scaledDistanceToCenter = ScaledDistanceToCenter(mapPositionPercentage.x, mapPositionPercentage.y, canvasWidth, canvasHeight);

        var scaledTextureCoordinates = new Vector2
        {
            x = scaledDistanceToCenter.x + canvasWidth / 2,
            y = scaledDistanceToCenter.y + canvasHeight / 2
        };

        return scaledTextureCoordinates;
    }

    private static Vector2 ScaledDistanceToCenter(float percentX, float percentY, float canvasWidth, float canvasHeight)
    {
        var distanceToCenter = DistanceToCenter(percentX, percentY, canvasWidth, canvasHeight);
        distanceToCenter.x *= ZoomAndPanController.Instance.ZoomFactor;
        distanceToCenter.y *= ZoomAndPanController.Instance.ZoomFactor;
        return distanceToCenter;
    }

    private static Vector2 DistanceToCenter(float percentX, float percentY, float canvasWidth, float canvasHeight)
    {
        var distanceToCenter = new Vector2(
            percentX * canvasWidth - canvasWidth / 2,
            percentY * canvasHeight - canvasHeight / 2);
        
        // apply panning offset
        distanceToCenter.x += ZoomAndPanController.Instance.PanOffset.x;
        distanceToCenter.y += ZoomAndPanController.Instance.PanOffset.y;

        return distanceToCenter;
    }
    
    public static (double latitude, double longitude) GetGeographicCoordinatesFromPositionPercent(double percentX, double percentY)
    {
        var latitude = 180f * percentY - 90f;
        var longitude = 360f * percentX - 180f;

        return (latitude, longitude);
    }
    
    public static Vector2 GetPositionPercentageFromGeographicCoordinates(double latitude, double longitude)
    {
        var x = (float)(longitude + 180f) / 360f;
        var y = (float)(latitude + 90f) / 360f;

        return new Vector2(x, y);
    }
}