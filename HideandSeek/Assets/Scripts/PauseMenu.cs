using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{
    [SerializeField] Animator animator;

    [System.NonSerialized] public bool transitioning = false;
    [System.NonSerialized] public bool settingsOpen = false;

    public void OpenMenu()
    {
        animator.ResetTrigger("Close");
        animator.SetTrigger("Open");
        transitioning = true;
        SoundManager.Instance.PlaySoundFXClip(SoundType.Menu_In, transform.position, AudioGroup.SFX, 1, 0, 1, 0);
    }

    public void OpenSettingsMenu()
    {
        animator.ResetTrigger("Settings Close");
        animator.SetTrigger("Settings Open");
        transitioning = true;
        settingsOpen = true;
        SoundManager.Instance.PlaySoundFXClip(SoundType.Menu_OtherIn, transform.position, AudioGroup.SFX, 1, 0, 1, 0);
    }

    public void CloseSettingsMenu()
    {
        animator.ResetTrigger("Settings Open");
        animator.SetTrigger("Settings Close");
        transitioning = true;
        SoundManager.Instance.PlaySoundFXClip(SoundType.Menu_OtherOut, transform.position, AudioGroup.SFX, 1, 0, 1, 0);
    }

    public void CloseMenu()
    {
        animator.ResetTrigger("Open");
        animator.SetTrigger("Close");
        transitioning = true;
        SoundManager.Instance.PlaySoundFXClip(SoundType.Menu_Out, transform.position, AudioGroup.SFX, 1, 0, 1, 0);
    }

    public void FinishTransition()
    {
        transitioning = false;
        EventSystem.current?.SetSelectedGameObject(null);
    }

    public void CloseSettings()
    {
        settingsOpen = false;
    }
    public void Unpause()
    {
        GameManager.Instance.stateUnpause();
    }
}
