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

    public void loadLevel(int lvl)
    {
        SceneManager.LoadScene(lvl);
        GameManager.Instance.stateUnpause();
    }

    public void LoadStart()
    {
        SceneManager.LoadScene(1);
    }

    public void LoadWaveTest()
    {
        SceneManager.LoadScene(2);
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


