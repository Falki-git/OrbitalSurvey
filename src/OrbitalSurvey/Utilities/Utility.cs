using System.Reflection;
using KSP.Game;
using KSP.Sim.impl;
using UnityEngine;

namespace OrbitalSurvey.Utilities;

public static class Utility
{
    public static GameStateConfiguration GameState => GameManager.Instance?.Game?.GlobalGameState?.GetGameState();
    
    private static readonly string _IMPORT_PATH = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "data");

    public static Texture2D ImportTexture(string filename)
    {
        var importedData = File.ReadAllBytes(Path.Combine(_IMPORT_PATH, filename));
        var toReturn = new Texture2D(1, 1);
        toReturn.LoadImage(importedData);
        return toReturn;
    }

    public static void CopyFieldAndPropertyDataFromSourceToTargetObject(object source, object target)
    {
        foreach (FieldInfo field in source.GetType().GetFields())
        {
            object value = field.GetValue(source);

            try
            {
                field.SetValue(target, value);
            }
            catch (FieldAccessException)
            { /* some fields are constants */ }
        }

        foreach (PropertyInfo property in source.GetType().GetProperties())
        {
            object value = property.GetValue(source);
            property.SetValue(target, value);
        }
    }
    
    public static string SessionGuidString => GameManager.Instance?.Game?.SessionGuidString;
    
    public static List<CelestialBodyComponent> GetAllCelestialBodies() => GameManager.Instance.Game?.UniverseModel?.GetAllCelestialBodies();

    public static List<string> GetAllCelestialBodyNames() => GetAllCelestialBodies().Select(body => body.Name).ToList();
}
