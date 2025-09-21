using System.Diagnostics;
using UnityEngine;
[CreateAssetMenu]
public class Shotgun : GunStates, IShoot
{
    public int bullets = 8;
    [Range(0f, 20f)] public float spread = 5f;

    public void Shoot(Transform transform)
    {
        ammoCurr--;
        SoundManager.Instance.PlaySoundFXClip(shootSound, transform.position, AudioGroup.GunSFX, 1f, 0.1f, 1f, 0.1f);
        Instantiate(muzzleFlash, transform.position, transform.rotation);
        Vector3 start = transform.position;
        float maxDist = shootDist;
        RaycastHit hit;
        for (int i = 0; i < bullets; i++)
        {
            Vector3 direction = RandomInCone(Camera.main.transform.forward, spread);
            bool inRange = Physics.Raycast(Camera.main.transform.position, direction, out hit, maxDist, ~ignoreLayers, QueryTriggerInteraction.Ignore);
            Vector3 end = inRange ? hit.point : start + Camera.main.transform.forward * maxDist;
            if (inRange)
            {
                //Debug.Log(hit.collider.name);
                //Instantiate(currGun.hitEffect, hit.point, Quaternion.identity);

                IDamage dmg = hit.collider.GetComponent<IDamage>();

                if (dmg != null)
                {
                    dmg.takeDamage(shootDamage);
                }

                tracer.CreateTrail(hit, start, GameManager.Instance.player.GetComponent<MonoBehaviour>());
            }
            else
            {
                tracer.CreateTrail(start, end, GameManager.Instance.player.GetComponent<MonoBehaviour>());
            }
        }
    }

    

}
