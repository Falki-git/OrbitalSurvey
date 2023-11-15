namespace OrbitalSurvey.Utilities;

public static class ScanUtility
{
    public static int GetScanRadius(double bodyRadius, double altitude, double scanningCone)
    {
        var r = bodyRadius;
        var h = altitude;
        var alpha = DegreesToRadians((scanningCone / 2f));
        
        // the Law of sines: alpha / sin(r) = beta / sin(r + h)
        var beta = Math.Asin(((r + h) * Math.Sin(alpha)) / r);

        // gamma = angle at the center of the celestial body from the point on the surface closest to the orbiting
        // vessel to the point on surface where the scanning cone finishes 
        var gamma = 180f - alpha - beta;

        // radius from the point on the surface closest to the orbiting vessel
        var radiusOfScanningCone = (360f * 2f * r * Math.PI) / gamma;

        return (int)radiusOfScanningCone;
    }

    public static (int x, int y) GetTextureCoordinatesFromGeographicCoordinates
        (double longitude, double latitude, int textureWidth, int textureHeight)
    {
        var x = textureWidth * ((longitude + 180) / 360f);
        var y = textureHeight * ((latitude + 90) / 180f);

        return ((int)x, (int)y);
    }

    public static double RadiansToDegrees(double radians)
    {
        return radians * (180f / Math.PI);
    }

    public static double DegreesToRadians(double degrees)
    {
        return degrees * (Math.PI / 180f);
    }
    
}