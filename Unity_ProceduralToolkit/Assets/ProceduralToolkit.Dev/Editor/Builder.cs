using System.IO;
using System.Reflection;
using UnityEditor;

public static class Builder
{
    private const string sceneNameTag = "SCENENAME";
    private const string versionTag = "VERSION";
    private const string buildFolderKey = "PT_WebGL_BuildFolder";

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
        string buildFolder = EditorPrefs.GetString(buildFolderKey);
        if (string.IsNullOrEmpty(buildFolder))
        {
            buildFolder = EditorUtility.OpenFolderPanel("Select the build folder", null, "PTGitHubPages");
            if (string.IsNullOrEmpty(buildFolder))
            {
                return;
            }

            EditorPrefs.SetString(buildFolderKey, buildFolder);
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

            BuildPipeline.BuildPlayer(new[] {scenePath}, buildPath, BuildTarget.WebGL, BuildOptions.None);
        }

        SetTemplateTag(sceneNameTag, "");
        SetTemplateTag(versionTag, "");

        EditorUtility.ClearProgressBar();
    }

    private static void SetTemplateTag(string key, string value)
    {
        MethodInfo setTemplateTag = typeof(PlayerSettings).GetMethod("SetTemplateCustomValue",
            BindingFlags.NonPublic | BindingFlags.Static);
        setTemplateTag.Invoke(null, new object[] {key, value});
    }
}
