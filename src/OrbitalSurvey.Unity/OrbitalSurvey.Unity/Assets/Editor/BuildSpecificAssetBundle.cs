using UnityEditor;
using System.IO;

public class BuildSpecificAssetBundles
{
    private const string AssetBundleDirectory = "Assets/AssetBundles";
    // Relative path from the Unity project directory to the target directory
    private const string TargetDirectory = "../../../plugin_template/assets/bundles";
    private const string BundleName = "orbitalsurvey_ui.bundle";

    [MenuItem("Assets/Build Specific AssetBundle")]
    static void BuildAllAssetBundles()
    {
        string assetBundleDirectory = "Assets/AssetBundles";
        if (!Directory.Exists(assetBundleDirectory))
        {
            Directory.CreateDirectory(assetBundleDirectory);
        }

        string[] assets = new string[3];
        assets[0] = "Assets/UI/OrbitalSurvey.uss";
        assets[1] = "Assets/UI/OrbitalSurvey.uxml";
        assets[2] = "Assets/Images/Other/static_background.jpeg";

        AssetBundleBuild[] buildMap = new AssetBundleBuild[1];        
        buildMap[0].assetNames = assets;
        buildMap[0].assetBundleName = BundleName;

        BuildPipeline.BuildAssetBundles(
            assetBundleDirectory,
            buildMap,
            BuildAssetBundleOptions.None,
            BuildTarget.StandaloneWindows);

        // Delete existing bundles in the target directory
        if (Directory.Exists(TargetDirectory))
        {
            var files = Directory.GetFiles(TargetDirectory, BundleName);
            foreach (var file in files)
            {
                File.Delete(file);
            }
        }
        else
        {
            Directory.CreateDirectory(TargetDirectory);
        }

        // Copy the newly built bundles to the target directory
        var newBundles = Directory.GetFiles(AssetBundleDirectory, BundleName);
        foreach (var bundle in newBundles)
        {
            var destFile = Path.Combine(TargetDirectory, Path.GetFileName(bundle));
            File.Copy(bundle, destFile, overwrite: true);
        }
    }
}