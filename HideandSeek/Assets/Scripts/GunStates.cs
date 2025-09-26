using System.Collections;
using UnityEngine;

[CreateAssetMenu]

public class GunStates : Item
{
    public GameObject model;
    [Range(1, 10)] public int shootDamage;
    [Range(1, 1000)] public int shootDist;
    [Range(0.1f, 3)] public float shootRate;
    public int ammoCurr;
    public int clipSize;
    public int ammoStored;
    public int startingAmmo;
    [SerializeField] public BulletTracer tracer;
    public ParticleSystem muzzleFlash;
    public ParticleSystem hitEffect;
    public SoundType shootSound;
    public SoundType equipSound;
    [RangeAttribute(0, 1)] public float shootVal;
    public LayerMask ignoreLayers;
    public float spreadPerShot = 0.5f;
    public float spreadRecoveryPerSecond = 0.75f;
    public float maxSpread = 3.0f;
    protected float currentSpread = 0f;
    Coroutine updateCoroutine;
    MonoBehaviour updateCaster;
    [SerializeField] protected ScreenShakeSettings shootScreenShake;
    public Vector3 RandomInCone(Vector3 forward, float angle)
    {
        forward = forward.normalized;
        float radius = angle * Mathf.Deg2Rad;

        float x = Random.value;
        float y = Random.value;
        float cos = Mathf.Cos(radius);
        float theta = Mathf.Lerp(cos, 1f, x);
        float sin = Mathf.Sqrt(1f - theta * theta);
        float phi = 2f * Mathf.PI * y;

        Vector3 a = (Mathf.Abs(forward.y) < 0.999f) ? Vector3.up : Vector3.right;
        Vector3 xAxis = Vector3.Cross(a, forward).normalized;
        Vector3 yAxis = Vector3.Cross(forward, xAxis);

        return (xAxis * (Mathf.Cos(phi) * sin)) + (yAxis * (Mathf.Sin(phi) * sin)) + (forward * theta);
    }

    private void OnEnable()
    {
        if (startingAmmo <= clipSize)
        {
            ammoCurr = startingAmmo;
            ammoStored = 0;
        } else
        {
            int ammoToStore = startingAmmo - clipSize;
            ammoCurr = clipSize;
            ammoStored = ammoToStore;
        }
    }

    public void Reload()
    {
        int ammoToFill = clipSize - ammoCurr;
        if (ammoStored >= ammoToFill)
        {
            ammoStored -= ammoToFill;
            ammoCurr = clipSize;
        } else
        {
            ammoCurr += ammoStored;
            ammoStored = 0;
        }
    }

    public void Equip(MonoBehaviour caster)
    {
        if (equipSound != default) SoundManager.Instance.PlaySoundFXClip(equipSound, caster.transform.position, AudioGroup.GunSFX, 1.0f, 0.05f, 1.0f, 0.05f);
        currentSpread = maxSpread;
        updateCaster = caster;
        if (updateCoroutine != null) caster.StopCoroutine(updateCoroutine);
        updateCoroutine = caster.StartCoroutine(GunUpdate(caster));
    }

    public void Unequip()
    {
        updateCaster.StopCoroutine(updateCoroutine);
    }

    IEnumerator GunUpdate(MonoBehaviour caster)
    {
        yield return new WaitForEndOfFrame();
        currentSpread -= spreadRecoveryPerSecond * Time.deltaTime;
        currentSpread = Mathf.Max(0, currentSpread);
        updateCoroutine = caster.StartCoroutine(GunUpdate(caster));
    }
}
