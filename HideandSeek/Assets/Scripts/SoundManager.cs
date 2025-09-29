using UnityEngine;
using System.Collections;

using System;

using UnityEngine.Audio;

[Serializable]
public struct Sound
{
    public SoundType type;
    public AudioClip[] clips;
}

public enum SoundType { Pistol, Shotgun, Machinegun, Aim, Reload, Heal, Damage, Death, Jump, Walk, Sprint, Explosion, BGM, MenuHover, Footstep, Item_Pickup, Menu_Click, Menu_In, Menu_Out, Vending_Use, Vending_Success, Vending_Fail, Hit_Success, Death_Static, Menu_OtherIn, Menu_OtherOut, Menu_FaderIn, Menu_FaderOut, Exploder_ChargeUp, Exploder_Explosion }

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
    [SerializeField] MusicSO[] music;
    [SerializeField] private AudioSource soundObject;
    [SerializeField] private AudioSource musicObject;

    [Header("Mixer Groups")]
    [SerializeField] private AudioMixerGroup masterGroup;
    [SerializeField] private AudioMixerGroup musicGroup;
    [SerializeField] private AudioMixerGroup sfxGroup;
    [SerializeField] private AudioMixerGroup gunSFXGroup;
    [System.NonSerialized] public SoundMixerManager soundMixerManager;

    // Music stuff
    
    private AudioSource musicA;
    private AudioSource musicB;
    bool musicRunning;
    MusicSO currentMusic;
    double nextMusicStart;
    double loopLength;
    AudioSource currentlyUsed;
    private void Awake()
    {
        if (instance != null)
        {
            Destroy(this.gameObject);
            return;
        }
        instance = this;
        soundMixerManager = GetComponent<SoundMixerManager>();
        DontDestroyOnLoad(gameObject);

        musicA = Instantiate(musicObject, transform);
        musicB = Instantiate(musicObject, transform);
        
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
    public void PlaySoundFXClip(SoundType type, Vector3 spawnLocation, AudioGroup audioGroup, float volume = 1.0f, float pitch = 1.0f)
    {
        if (!TryGetSound(type, out Sound sound)) return;

        AudioSource audioSource = Instantiate(soundObject, spawnLocation, Quaternion.identity);

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
    public void PlaySoundFXClip(SoundType type, Vector3 spawnLocation, AudioGroup audioGroup, float volume, float volumeRange, float pitch, float pitchRange)
    {
        if (!TryGetSound(type, out Sound sound)) return;

        AudioSource audioSource = Instantiate(soundObject, spawnLocation, Quaternion.identity);

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

    public void PlayMusic(string trackName, float volume)
    {
        MusicSO musicSO = null;
        for (int i = 0; i < music.Length; i++)
        {
            if (music[i].name == trackName)
            {
                musicSO = music[i];
                break;
            }
        }
        if (musicSO == null) return;

        if (musicRunning) StopMusic(true);

        StartMusic(musicSO, volume);
    }

    void StartMusic(MusicSO musicSO, float volume)
    {
        currentMusic = musicSO;
        loopLength = musicSO.GetLoopTime();

        musicA.clip = musicSO.clip;
        musicB.clip = musicSO.clip;
        musicA.volume = volume;
        musicB.volume = volume;

        double currentTime = AudioSettings.dspTime;
        double firstStart = currentTime + 0.1;
        currentlyUsed = musicA;

        nextMusicStart = firstStart;
        musicRunning = true;

        StartCoroutine(ScheduleMusic());
    }

    IEnumerator ScheduleMusic()
    {
        double now;
        while (musicRunning)
        {
            now = AudioSettings.dspTime;
            if (nextMusicStart - now < loopLength)
            {
                ScheduleNextLoop();
            }
            yield return null;
        }
    }

    void ScheduleNextLoop()
    {
        AudioSource source = currentlyUsed == musicA ? musicB : musicA;
        /*if (currentlyUsed == musicA)
        {
            Debug.Log("Switching to B");
        } else
        {
            Debug.Log("Switching to A");
        }*/
        currentlyUsed = source;

        source.clip = currentMusic.clip;
        source.loop = false;

        source.PlayScheduled(nextMusicStart);

        double endAt = nextMusicStart + source.clip.length;
        source.SetScheduledEndTime(endAt);

        nextMusicStart += loopLength;
    }

    public void StopMusic(bool immediate)
    {
        if (!musicRunning) return;

        if (immediate)
        {
            musicA.Stop();
            musicB.Stop();
        } else
        {
            musicA.SetScheduledEndTime(AudioSettings.dspTime + 0.05);
            musicB.SetScheduledEndTime(AudioSettings.dspTime + 0.05);
        }
        musicRunning = false;
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
