using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;
[CreateAssetMenu] 
public class BulletTracer : ScriptableObject
{
    public TrailRenderer bulletTrail;
    public ParticleSystem impactParticles;
    public DecalProjector decal;
    public LayerMask holeDecalLayerMask;
    public DecalProjector decalEnemy;
    public LayerMask bloodDecalLayerMask;
    public float decalLength;

    public void CreateTrail(Transform startPos, Vector3 endPos, MonoBehaviour caster)
    {
        TrailRenderer renderer = Instantiate(bulletTrail, startPos.position, Quaternion.identity);
        caster.StartCoroutine(SpawnTrail(renderer ,startPos, endPos));
    }

    public void CreateTrail(RaycastHit hit, Transform startPos, MonoBehaviour caster)
    {
        TrailRenderer renderer = Instantiate(bulletTrail, startPos.position, Quaternion.identity);
        caster.StartCoroutine(SpawnTrailHit(renderer, startPos, hit));
    }

    IEnumerator SpawnTrail(TrailRenderer trail, Transform startPos, Vector3 endPos)
    {
        float time = 0;
        Vector3 startPosition = startPos.position;

        while (time < 1)
        {
            trail.transform.position = Vector3.Lerp(startPosition, endPos, time);
            time += Time.deltaTime / trail.time;
            yield return null;
        }

        trail.transform.position = endPos;
        Destroy(trail.gameObject, trail.time);
    }

    IEnumerator SpawnTrailHit(TrailRenderer trail, Transform startPos, RaycastHit hit)
    {
        float time = 0;
        Vector3 startPosition = startPos.position;

        while (time < 1)
        {
            trail.transform.position = Vector3.Lerp(startPosition, hit.point, time);
            time += Time.deltaTime / trail.time;
            yield return null;
        }

        trail.transform.position = hit.point;
        Instantiate(impactParticles, hit.point, Quaternion.LookRotation(hit.normal));
        Destroy(trail.gameObject, trail.time);

        DoDecal(hit);
    }

    void DoDecal(RaycastHit hit)
    {
        if (hit.collider == null) return;
        if ( (holeDecalLayerMask.value & (1 << hit.collider.gameObject.layer)) != 0)
        {
            GameObject decalInst = Instantiate(decal, hit.point, Quaternion.identity).gameObject;
            decalInst.transform.forward = -hit.normal;
            Destroy(decalInst, decalLength);
        }
        if ((bloodDecalLayerMask.value & (1 << hit.collider.gameObject.layer)) != 0)
        {
            GameObject decalInst = Instantiate(decalEnemy, hit.point, Quaternion.identity).gameObject;
            decalInst.transform.forward = -hit.normal;
            decalInst.transform.parent = hit.transform;
            Destroy(decalInst, decalLength);
        }
    }
}
