using System.Reflection;
using KSP.Game;
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

    // Define a recursive function to search for an object by name
    public static Transform FindObjectByNameRecursively(Transform parent, string name)
    {
        foreach (Transform child in parent)
        {
            if (child.name == name)
            {
                return child;
            }

            // Recursively search in the child's children
            Transform found = FindObjectByNameRecursively(child, name);
            if (found != null)
            {
                return found;
            }
        }

        return null; // Return null if the object was not found in the hierarchy
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
}
