using UnityEngine;

public class ButtonFX : MonoBehaviour
{
    public void ButtonHoverSound()
    {
        SoundManager.Instance.PlaySoundFXClip(SoundType.MenuHover, Camera.main.transform, AudioGroup.SFX, 1.0f, 0.0f, 1.5f, 0.25f);
    }

    public void ButtonUnhoverSound()
    {
        SoundManager.Instance.PlaySoundFXClip(SoundType.MenuHover, Camera.main.transform, AudioGroup.SFX, 1.0f, 0.0f, 0.5f, 0.25f);
    }
}
