using System;
using System.IO;
using System.Reflection;
using UnityEditor;

public static class Builder
{
    private const string sceneNameTag = "SCENENAME";
    private const string versionTag = "VERSION";
    private const string webglBuildFolderKey = "PT_WebGL_BuildFolder";
    private const string androidBuildFolderKey = "PT_Android_BuildFolder";
    private const string iosBuildFolderKey = "PT_iOS_BuildFolder";

    [MenuItem("Procedural Toolkit/Build Android")]
    public static void BuildAndroid()
    {
        string buildPath = GetBuildFile(androidBuildFolderKey, "ProceduralToolkit.apk", "apk");
        if (string.IsNullOrEmpty(buildPath))
        {
            return;
        }
        EditorUtility.DisplayProgressBar("Building...", null, 0);

        BuildPipeline.BuildPlayer(new BuildPlayerOptions
        {
            target = BuildTarget.Android,
            locationPathName = buildPath,
            scenes = Array.ConvertAll(EditorBuildSettings.scenes, s => s.path),
        });

        EditorUtility.ClearProgressBar();
    }

    [MenuItem("Procedural Toolkit/Build iOS")]
    public static void BuildIOS()
    {
        string buildPath = GetBuildFolder(iosBuildFolderKey, "iOS");
        if (string.IsNullOrEmpty(buildPath))
        {
            return;
        }
        EditorUtility.DisplayProgressBar("Building...", null, 0);

        BuildPipeline.BuildPlayer(new BuildPlayerOptions
        {
            target = BuildTarget.iOS,
            locationPathName = buildPath,
            scenes = Array.ConvertAll(EditorBuildSettings.scenes, s => s.path),
        });

        EditorUtility.ClearProgressBar();
    }

    [MenuItem("Assets/Build scene(s)", true)]
    public static bool BuildSceneTest()
    {
        foreach (var o in Selection.objects)
        {
            var path = AssetDatabase.GetAssetPath(o);
            if (!path.EndsWith(".unity"))
            {
                return false;
            }
        }
        return true;
    }

    [MenuItem("Assets/Build scene(s)", priority = 0)]
    public static void BuildScene()
    {
        string buildFolder = GetBuildFolder(webglBuildFolderKey, "PTGitHubPages");
        if (string.IsNullOrEmpty(buildFolder))
        {
            return;
        }

        EditorUtility.DisplayProgressBar("Building...", null, 0);

        foreach (var scene in Selection.objects)
        {
            string scenePath = AssetDatabase.GetAssetPath(scene);

            SetTemplateTag(sceneNameTag, scene.name);
            SetTemplateTag(versionTag, ProceduralToolkit.Editor.ProceduralToolkitMenu.version);

            string buildPath = Path.Combine(buildFolder, scene.name);

            var directoryInfo = new DirectoryInfo(buildPath);
            if (!directoryInfo.Exists)
            {
                directoryInfo.Create();
            }
            foreach (var info in directoryInfo.GetFiles())
            {
                info.Delete();
            }
            foreach (var info in directoryInfo.GetDirectories())
            {
                info.Delete(true);
            }

            BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                target = BuildTarget.WebGL,
                locationPathName = buildPath,
                scenes = new[] {scenePath},
            });
        }

        SetTemplateTag(sceneNameTag, "");
        SetTemplateTag(versionTag, "");

        EditorUtility.ClearProgressBar();
    }

    private static string GetBuildFolder(string key, string defaultName)
    {
        string buildPath = EditorPrefs.GetString(key);
        if (string.IsNullOrEmpty(buildPath))
        {
            buildPath = EditorUtility.OpenFolderPanel("Select the build folder", null, defaultName);
            if (string.IsNullOrEmpty(buildPath))
            {
                return null;
            }
            EditorPrefs.SetString(key, buildPath);
        }
        return buildPath;
    }

    private static string GetBuildFile(string key, string defaultName, string extension)
    {
        string buildPath = EditorPrefs.GetString(key);
        //if (string.IsNullOrEmpty(buildPath))
        {
            buildPath = EditorUtility.SaveFilePanel("Select the build file", null, defaultName, extension);
            if (string.IsNullOrEmpty(buildPath))
            {
                return null;
            }
            EditorPrefs.SetString(key, buildPath);
        }
        return buildPath;
    }

    private static void SetTemplateTag(string key, string value)
    {
        MethodInfo setTemplateTag = typeof(PlayerSettings).GetMethod("SetTemplateCustomValue",
            BindingFlags.NonPublic | BindingFlags.Static);
        setTemplateTag.Invoke(null, new object[] {key, value});
    }
}
