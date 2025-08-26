using UnityEngine;

[CreateAssetMenu]

public class gunStats : ScriptableObject
{
    public GameObject model;
    [Range(1, 10)] public int shootDamage;
    [Range(5, 1000)] public int shootDist;
    [Range(0.1f, 3)] public float shootRate;
    public int ammoCurr;
    [Range(5, 100)] public int ammoMax;

    public ParticleSystem hitEffect;
    public AudioClip[] shootSound;
    [Range(0, 1)] public float shootVol;
}
