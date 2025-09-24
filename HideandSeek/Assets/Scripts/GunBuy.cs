using UnityEngine;
[RequireComponent(typeof(VendingMachine))]
public class GunBuy : MonoBehaviour
{
    public Item gunToDispense;
    public int ammoCost;
    public int ammoRefilled = 10;

    public void Buy()
    {
        if (!InventoryManager.Instance.HasItemInInventory(gunToDispense))
        {
            InventoryManager.Instance.AddItem(gunToDispense);
        } else
        {
            GetComponent<VendingMachine>().interactCost = ammoCost;
            if (gunToDispense is GunStates)
            {
                ((GunStates)gunToDispense).ammoStored += ammoRefilled;
            }
            GameManager.Instance.playerScript.ammoUpdated.Invoke(((GunStates)gunToDispense).ammoCurr, ((GunStates)gunToDispense).ammoStored);
        }
        GetComponent<VendingMachine>().costText.text = "$" + ammoCost.ToString();
        
    }
}
