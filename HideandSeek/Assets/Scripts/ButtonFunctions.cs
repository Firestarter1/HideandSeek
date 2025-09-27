using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class buttonFunctions : MonoBehaviour
{
    [SerializeField] GameObject settingsMenu;
    public void resume()
    {
        GameManager.Instance.menuPause.CloseMenu();
    }

    public void restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        GameManager.Instance.stateUnpause();
    }
    public void quite()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public void respawn()
    {
        GameManager.Instance.playerScript.spawnPlayer();
        GameManager.Instance.stateUnpause();
    }

    public void loadLevel(string sceneName)
    {
        StartCoroutine(FadeInRoutine(sceneName));
    }

    IEnumerator FadeInRoutine(string sceneName)
    {
        yield return SceneFader.Instance.FadeOut();
        yield return new WaitForEndOfFrame();
        Loader.Load(sceneName);
    }

    public void LoadStart()
    {
        loadLevel("01 - Main");
    }

    public void LoadWaveTest()
    {
        loadLevel("02 - Lab");
    }

    public void OpenSettingsMenu()
    {
        GameManager.Instance.menuPause.OpenSettingsMenu();
    }

    public void CloseSettingsMenu()
    {
        GameManager.Instance.menuPause.CloseSettingsMenu();
    }
}


