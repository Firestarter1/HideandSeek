using UnityEngine;

[CreateAssetMenu]

public class GunStates : Item
{
    [Range(1, 10)] public int shootDamage;
    [Range(1, 1000)] public int shootDist;
    [Range(0.1f, 3)] public float shootRate;
    public int ammoCurr;
    [Range(5, 50)] public int ammoMax;

    public ParticleSystem muzzleFlash;
    public ParticleSystem hitEffect;
    public SoundType shootSound;
    [RangeAttribute(0, 1)] public float shootVal;
}
