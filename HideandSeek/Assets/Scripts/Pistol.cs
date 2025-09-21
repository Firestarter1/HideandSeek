using System.Diagnostics;
using UnityEngine;
[CreateAssetMenu]
public class Pistol : GunStates, IShoot
{
    
    public void Shoot(Transform transform)
    {
        ammoCurr--;
        currentSpread += spreadPerShot;
        SoundManager.Instance.PlaySoundFXClip(shootSound, transform.position, AudioGroup.GunSFX, 1f, 0.1f, 1f, 0.1f);

        Vector3 start = transform.position;
        float maxDist = shootDist;
        RaycastHit hit;
        bool inRange = Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit, maxDist, ~ignoreLayers, QueryTriggerInteraction.Ignore);
        Vector3 end = inRange ? hit.point : start + Camera.main.transform.forward * maxDist;

        Instantiate(muzzleFlash, transform.position, 
            transform.rotation);
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
