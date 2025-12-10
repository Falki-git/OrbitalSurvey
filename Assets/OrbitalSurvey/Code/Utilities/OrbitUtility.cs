using KSP.Sim;
using KSP.Sim.impl;

namespace OrbitalSurvey.Utilities;

public static class OrbitUtility
{
    public static Vector3d WorldPositionAtUT(this PatchedConicsOrbit o, double UT)
    {
        return o.referenceBody.transform.celestialFrame.ToLocalPosition(o.ReferenceFrame, o.referenceBody.Position.localPosition + o.GetRelativePositionAtUTZup(UT).SwapYAndZ);
    }
    
    public static void GetOrbitalParametersAtUT(VesselComponent vessel, double UT, out double latitude, out double longitude, out double altitude)
    {
        var position = new Position(vessel.Orbit.ReferenceFrame, vessel.Orbit.GetRelativePositionAtUT(UT));
        vessel.Orbit.referenceBody.GetLatLonAltFromRadius(position, out latitude, out longitude, out altitude);
        longitude += GetLongitudeOffsetDueToRotationForAGivenUT(vessel.Orbit.referenceBody, UT);

        // correct longitude if it dropped below -180°
        while (longitude < -180f)
        {
            longitude += 360f;
        }
    }

    public static double GetLongitudeOffsetDueToRotationForAGivenUT(CelestialBodyComponent body, double UT)
    {
        // C (circumference) = 2rπ
        // length of day = time it takes for 1 full rotation, i.e. C)
        // dt = delta T from now to the given UT
        // longitude difference = (horizontal distance / radius of the planet) * (180 / π)
        
        var circumference = 2 * body.radius * Math.PI;
        var lengthOfDay = body.rotationPeriod;
        var deltaUT = Utility.UT - UT;
        var rotationDifferenceAtEquator = (deltaUT * circumference) / lengthOfDay;
        
        return (rotationDifferenceAtEquator / body.radius) * (180 / Math.PI);
    }
}