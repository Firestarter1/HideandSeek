using UnityEngine;

public class loot : MonoBehaviour
{
    [SerializeField] Item item;

    private void OnTriggerEnter(Collider other)
    {
        IPickup pickupable = other.GetComponent<IPickup>();

        if(pickupable != null )
        {
            SoundManager.Instance.PlaySoundFXClip(SoundType.Item_Pickup, transform.position, AudioGroup.SFX, 0.75f, 0.05f, 1f, 0.15f);
            InventoryManager.Instance.AddItem(item);
            Destroy(gameObject);
        }
    }
}
