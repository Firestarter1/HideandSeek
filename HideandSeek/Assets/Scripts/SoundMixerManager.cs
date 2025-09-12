using UnityEngine;
using UnityEngine.Audio;

public class SoundMixerManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField] private AudioMixer audioMixer;


    /*
     * All functions here should have their value set between 1-0.0001. 
     */

    public void SetMasterVolume(float level)
    {
        audioMixer.SetFloat("MasterVolume", Mathf.Log10(level) * 20f);
    }

    public void SetMusicVoume(float level)
    {
        audioMixer.SetFloat("MusicVolume", Mathf.Log10(level) * 20f);
    }

    public void SetSFXVolume(float level)
    {
        audioMixer.SetFloat("SFXVolume", Mathf.Log10(level) * 20f);
    }

    public void SetGunSFXVolume(float level)
    {
        audioMixer.SetFloat("GunSFXVolume", Mathf.Log10(level) * 20f);
    }
}
