using UnityEngine;

public class ButtonFX : MonoBehaviour
{
    public void ButtonHoverSound()
    {
        SoundManager.Instance.PlaySoundFXClip(SoundType.MenuHover, Camera.main.transform.position, AudioGroup.SFX, 1.0f, 0.0f, 1.5f, 0.25f);
    }

    public void ButtonUnhoverSound()
    {
        SoundManager.Instance.PlaySoundFXClip(SoundType.MenuHover, Camera.main.transform.position, AudioGroup.SFX, 0.25f, 0.0f, 0.5f, 0.25f);
    }

    public void ButtonClickSound()
    {
        SoundManager.Instance.PlaySoundFXClip(SoundType.Menu_Click, Camera.main.transform.position, AudioGroup.SFX, 1.0f, 0f, 1.0f, 0.05f);
    }
}
