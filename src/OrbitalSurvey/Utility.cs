using Newtonsoft.Json;
using System.Reflection;
using UnityEngine;

namespace OrbitalSurvey
{
    public static class Utility
    {
        private static string _importPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

        public static Texture2D ImportTexture(string filename)
        {
            var importedData = File.ReadAllBytes(Path.Combine(_importPath, filename));
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
    }
}
