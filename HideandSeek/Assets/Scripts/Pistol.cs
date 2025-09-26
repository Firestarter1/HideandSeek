using Unity.Cinemachine;
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
        Vector3 direction = RandomInCone(Camera.main.transform.forward, currentSpread);
        RaycastHit hit;
        bool inRange = Physics.Raycast(Camera.main.transform.position, direction, out hit, maxDist, ~ignoreLayers, QueryTriggerInteraction.Ignore);
        Vector3 end = inRange ? hit.point : start + Camera.main.transform.forward * maxDist;


        CinemachineImpulseSource impulse;
        if (!transform.parent.gameObject.TryGetComponent<CinemachineImpulseSource>(out impulse))
        {
            impulse = transform.parent.gameObject.AddComponent<CinemachineImpulseSource>();
            impulse.ImpulseDefinition.ImpulseType = CinemachineImpulseDefinition.ImpulseTypes.Uniform;
        }
        CameraShakeManager.Instance.ScreenShakeFromSettings(shootScreenShake, impulse);
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

            tracer.CreateTrail(hit, transform, GameManager.Instance.player.GetComponent<MonoBehaviour>());
        }
        else
        {
            tracer.CreateTrail(transform, end, GameManager.Instance.player.GetComponent<MonoBehaviour>());
        }

    }

}
