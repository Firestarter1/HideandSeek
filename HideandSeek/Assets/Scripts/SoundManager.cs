using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Rendering;
using System;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Unity.VisualScripting;
using UnityEngine.Audio;

[Serializable]
public struct Sound
{
    public SoundType type;
    public AudioClip[] clips;
}

public enum SoundType { Pistol, Shotgun, Machinegun, Aim, Reload, Heal, Damage, Death, Jump, Walk, Sprint, Explosion, BGM, MenuHover }

public enum AudioGroup { SFX, GunSFX, Music}

public class SoundManager : MonoBehaviour
{
    private static SoundManager instance;

    public static SoundManager Instance
    {
        get
        {
            return instance;
        }
    }
    [SerializeField] Sound[] sounds;
    [SerializeField] private AudioSource soundObject;

    [Header("Mixer Groups")]
    [SerializeField] private AudioMixerGroup masterGroup;
    [SerializeField] private AudioMixerGroup musicGroup;
    [SerializeField] private AudioMixerGroup sfxGroup;
    [SerializeField] private AudioMixerGroup gunSFXGroup;


    private void Awake()
    {
        if (instance != null)
        {
            Destroy(this);
            return;
        }
        instance = this;
    }

    /* private void Start()
     {
         audioSource = GetComponent<AudioSource>();
     }*/

    /*public static void PlaySound(SoundType sound, float volume = 1)
    {
        instance.audioSource.PlayOneShot(instance.soundList[(int)sound], volume);
    }*/



    /*
     * Main function to call when playing sounds. Access using SoundManager.Instance.PlaySoundFXClip.
     * 
     * Sound type is the Type to look for in our "Sound[] sounds" array
     * 
     * Spawn location is where in world space the sound will spawn
     * 
     * AudioGroup is what bus the audio will go through (SFX, GunSFX, Music)
     */
    public void PlaySoundFXClip(SoundType type, Transform spawnLocation, AudioGroup audioGroup, float volume = 1.0f, float pitch = 1.0f)
    {
        if (!TryGetSound(type, out Sound sound)) return;

        AudioSource audioSource = Instantiate(soundObject, spawnLocation.position, Quaternion.identity);

        AudioClip clip = sound.clips.Length == 1 ? sound.clips[0] : GetRandomSound(sound);

        audioSource.clip = clip;
        audioSource.volume = volume;
        audioSource.pitch = pitch;
        audioSource.outputAudioMixerGroup = GetAudioGroup(audioGroup);

        audioSource.Play();

        float clipLen = clip.length;

        Destroy(audioSource.gameObject, clipLen);
    }
    /*
     * Variation to call when wanting to add random ranges to pitch and volume.
     */
    public void PlaySoundFXClip(SoundType type, Transform spawnLocation, AudioGroup audioGroup, float volume, float volumeRange, float pitch, float pitchRange)
    {
        if (!TryGetSound(type, out Sound sound)) return;

        AudioSource audioSource = Instantiate(soundObject, spawnLocation.position, Quaternion.identity);

        AudioClip clip = sound.clips.Length == 1 ? sound.clips[0] : GetRandomSound(sound);

        volume += UnityEngine.Random.Range(-volumeRange, volumeRange); 
        pitch += UnityEngine.Random.Range(-pitchRange, pitchRange);

        audioSource.clip = clip;
        audioSource.volume = volume;
        audioSource.pitch = pitch;
        audioSource.outputAudioMixerGroup = GetAudioGroup(audioGroup);

        audioSource.Play();

        float clipLen = clip.length;

        Destroy(audioSource.gameObject, clipLen);
    }

    public bool TryGetSound(SoundType type, out Sound sound)
    {
        foreach (Sound s in sounds)
        {
            if (s.type == type)
            {
                sound = s;
                return true;
            }
        }
        sound = default;
        return false;
    }

    AudioClip GetRandomSound(Sound sound)
    {
        int noSoundsOfType = sound.clips.Length;
        int randomIndex = UnityEngine.Random.Range(0, noSoundsOfType);
        return sound.clips[randomIndex];
    }

    AudioMixerGroup GetAudioGroup(AudioGroup audioGroup)
    {
        switch (audioGroup)
        {
            case AudioGroup.SFX:
                return sfxGroup;
            case AudioGroup.GunSFX:
                return gunSFXGroup;
            case AudioGroup.Music:
                return musicGroup;
        }
        return masterGroup;
    }

}
