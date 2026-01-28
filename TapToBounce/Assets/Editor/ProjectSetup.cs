using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public class ProjectSetup
{
    [MenuItem("Tools/Setup Project")]
    public static void Setup()
    {
        // 1. Create Scene
        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
        
        // 2. Create Floor
        GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Quad);
        floor.name = "Floor";
        floor.transform.position = new Vector3(0, -4, 0);
        floor.transform.localScale = new Vector3(10, 1, 1);
        Object.DestroyImmediate(floor.GetComponent<MeshCollider>());
        floor.AddComponent<BoxCollider2D>();
        
        // 3. Create Player (Circle)
        // Since PrimitiveType doesn't have Circle, we'll use a Sprite or just a Quad with a circle look
        GameObject player = GameObject.CreatePrimitive(PrimitiveType.Quad);
        player.name = "Player";
        player.transform.position = new Vector3(0, 0, 0);
        Object.DestroyImmediate(player.GetComponent<MeshCollider>());
        player.AddComponent<CircleCollider2D>();
        var rb = player.AddComponent<Rigidbody2D>();
        rb.gravityScale = 1.5f;
        player.AddComponent<Bounce>();

        // 4. Save Scene
        EditorSceneManager.SaveScene(scene, "Assets/Scenes/Main.unity");
        
        // 5. Add to Build Settings
        EditorBuildSettings.scenes = new EditorBuildSettingsScene[] {
            new EditorBuildSettingsScene("Assets/Scenes/Main.unity", true)
        };

        // 6. Player Settings
        PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, "com.boss.taptobounce");
        PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;
        PlayerSettings.bundleVersion = "1.0";

        // Set Scripting Backend to Mono
        PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.Mono2x);
        // Target ARMv7
        PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARMv7;
        
        Debug.Log("Project Setup Complete");
    }

    [MenuItem("Tools/Build APK")]
    public static void BuildAPK()
    {
        string[] scenes = { "Assets/Scenes/Main.unity" };
        string outputPath = "TapToBounce.apk";
        
        BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
        buildPlayerOptions.scenes = scenes;
        buildPlayerOptions.locationPathName = outputPath;
        buildPlayerOptions.target = BuildTarget.Android;
        buildPlayerOptions.options = BuildOptions.Development;

        BuildPipeline.BuildPlayer(buildPlayerOptions);
        Debug.Log("Build Complete: " + outputPath);
    }
}
