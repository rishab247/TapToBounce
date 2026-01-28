using UnityEditor;
using UnityEngine;

public class BuildScript
{
    public static void BuildAndroid()
    {
        string[] scenes = { "Assets/Scenes/Main.unity" };
        string buildPath = "TapToBounce_Mega.apk";
        
        BuildPipeline.BuildPlayer(scenes, buildPath, BuildTarget.Android, BuildOptions.None);
    }

    public static void BuildSkyline()
    {
        string[] scenes = { "Assets/Scenes/Main.unity" };
        string buildPath = "TapToBounce_Skyline.apk";
        
        BuildPipeline.BuildPlayer(scenes, buildPath, BuildTarget.Android, BuildOptions.None);
    }

    public static void BuildPhysics()
    {
        string[] scenes = { "Assets/Scenes/Main.unity" };
        string buildPath = "TapToBounce_Physics.apk";
        
        BuildPipeline.BuildPlayer(scenes, buildPath, BuildTarget.Android, BuildOptions.None);
    }

    public static void BuildRobot()
    {
        string[] scenes = { "Assets/Scenes/Main.unity" };
        string buildPath = "TapToBounce_Robot.apk";
        
        BuildPipeline.BuildPlayer(scenes, buildPath, BuildTarget.Android, BuildOptions.None);
    }
}
