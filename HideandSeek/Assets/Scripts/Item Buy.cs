using System.Collections;
using UnityEngine;
[RequireComponent(typeof(VendingMachine))]
public class ItemBuy : MonoBehaviour
{
    public Item itemToDispense;

    public void Buy()
    {
        InventoryManager.Instance.AddItem(itemToDispense);
    }
}

