using UnityEngine;

public class AudioSliderFunctions : MonoBehaviour
{
    public void SetMasterVolume(float value)
    {
        SoundManager.Instance.soundMixerManager.SetMasterVolume(value);
    }

    public void SetSFXVolume(float value)
    {
        SoundManager.Instance.soundMixerManager.SetSFXVolume(value);
    }

    public void SetMusicVolume(float value)
    {
        SoundManager.Instance.soundMixerManager.SetMusicVoume(value);
    }

    public void SetGunSFXVolume(float value)
    {
        SoundManager.Instance.soundMixerManager.SetGunSFXVolume(value);
    }
}
