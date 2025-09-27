using UnityEngine;
using UnityEngine.SceneManagement;

public static class Loader 
{
    public const string loadingSceneName = "Loading Scene";

    static string nextScene;

    public static void Load(string sceneName)
    {
        nextScene = sceneName;
        SceneManager.LoadScene(loadingSceneName, LoadSceneMode.Single);
    }

    public static string GetNextScene()
    {
        string name = nextScene;
        nextScene = null;
        return name;
    }
}
