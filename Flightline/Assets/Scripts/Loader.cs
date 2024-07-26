using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class Loader
{
    public enum Scene // Contains all the scenes
    {
        MainMenuScene,
        MainGameScene,
        LoadingScene,
        AuthenticationScene,
        TutorialScene
    }
    private static Scene targetScene;

    public static void Load(Scene targetScene)
    {
        Loader.targetScene = targetScene; // Set the target scene to the given scene
        SceneManager.LoadScene(Scene.LoadingScene.ToString()); // Load the loading scene
    }

    public static void LoaderCallback()
    {
        SceneManager.LoadScene(targetScene.ToString()); // Once the loaderCallback in the loadingScene completes its first update, load the targetScene
    }
}