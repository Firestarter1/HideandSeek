using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(AudioSource))]

public class SoundManager : MonoBehaviour
{
    public enum SoundType { Pistol, Shotgun, Machinegun, Heal, Damage, Death, Jump, Walk, Sprint, Explosion, BGM }

    private static SoundManager instance;

    [SerializeField] AudioClip[] soundList;
    private AudioSource audioSource;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    public static void PlaySound(SoundType sound, float volume = 1)
    {
        instance.audioSource.PlayOneShot(instance.soundList[(int)sound], volume);
    }
}
