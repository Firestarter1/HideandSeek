using UnityEngine;

public class loot : MonoBehaviour
{
    [SerializeField] Item item;

    private void OnTriggerEnter(Collider other)
    {
        IPickup pickupable = other.GetComponent<IPickup>();

        if(pickupable != null )
        {
            InventoryManager.Instance.AddItem(item);
            Destroy(gameObject);
        }
    }
}
