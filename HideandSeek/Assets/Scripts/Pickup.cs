using UnityEngine;

public class Pickup : MonoBehaviour
{
    [SerializeField] GunStates gun;

    private void OnTriggerEnter(Collider other)
    {
        IPickup pickupable = other.GetComponent<IPickup>();

        if(pickupable != null)
        {
            pickupable.getGunStats(gun);
            gun.ammoCur = gun.ammoMax;
            Destroy(gameObject);
        }
    }
}
