using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class BulletTracer : MonoBehaviour
{
    public TrailRenderer bulletTrail;
    public ParticleSystem impactParticles;
    public DecalProjector decal;
    public float decalLength;

    public void CreateTrail(RaycastHit hit, Vector3 startPos)
    {
        TrailRenderer renderer = Instantiate(bulletTrail, startPos, Quaternion.identity);
        StartCoroutine(SpawnTrail(renderer, hit));
    }

    IEnumerator SpawnTrail(TrailRenderer trail, RaycastHit hit)
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
