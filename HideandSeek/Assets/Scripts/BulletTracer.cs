using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;
[CreateAssetMenu] 
public class BulletTracer : ScriptableObject
{
    public TrailRenderer bulletTrail;
    public ParticleSystem impactParticles;
    public DecalProjector decal;
    public float decalLength;

    public void CreateTrail(Vector3 startPos, Vector3 endPos, MonoBehaviour caster)
    {
        TrailRenderer renderer = Instantiate(bulletTrail, startPos, Quaternion.identity);
        caster.StartCoroutine(SpawnTrail(renderer , endPos));
    }

    public void CreateTrail(RaycastHit hit, Vector3 startPos, MonoBehaviour caster)
    {
        TrailRenderer renderer = Instantiate(bulletTrail, startPos, Quaternion.identity);
        caster.StartCoroutine(SpawnTrailHit(renderer, startPos, hit));
    }

    IEnumerator SpawnTrail(TrailRenderer trail, Vector3 endPos)
    {
        float time = 0;
        Vector3 startPosition = trail.transform.position;

        while (time < 1)
        {
            trail.transform.position = Vector3.Lerp(startPosition, endPos, time);
            time += Time.deltaTime / trail.time;
            yield return null;
        }

        trail.transform.position = endPos;
        Destroy(trail.gameObject, trail.time);
    }

    IEnumerator SpawnTrailHit(TrailRenderer trail, Vector3 startPos, RaycastHit hit)
    {
        float time = 0;
        Vector3 startPosition = trail.transform.position;

        while (time < 1)
        {
            trail.transform.position = Vector3.Lerp(startPosition, hit.point, time);
            time += Time.deltaTime / trail.time;
            yield return null;
        }

        trail.transform.position = hit.point;
        Instantiate(impactParticles, hit.point, Quaternion.LookRotation(hit.normal));
        Destroy(trail.gameObject, trail.time);
        GameObject decalInst = Instantiate(decal, hit.point, Quaternion.identity).gameObject;
        decalInst.transform.forward = -hit.normal;
        Destroy(decalInst, decalLength);
    }
}
