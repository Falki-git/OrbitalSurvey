using KSP.Sim.impl;

namespace OrbitalSurvey.Utilities;

public static class Extensions
{
    public static string AddSpaceBeforeUppercase(this string input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return input;
        }

        var charList = new List<char>(input.ToCharArray());

        for (var i = 1; i < charList.Count; i++)
        {
            if (char.IsUpper(charList[i]))
            {
                charList.Insert(i++, ' ');
            }
        }

        return new string(charList.ToArray());
    }
    
    public static T GetModule<T>(this VesselComponent vessel) where T : PartComponentModule
    {
        var modules = vessel.SimulationObject.PartOwner.GetPartModules<T>();

        if (modules != null && modules.Count >= 1)
        {
            return modules.First();
        }
        
        return null;
    }
}