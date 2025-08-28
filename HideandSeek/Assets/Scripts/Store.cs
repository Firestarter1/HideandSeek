using System.Xml.Serialization;
using UnityEngine;
using UnityEngine.UI;

public class Store : MonoBehaviour, IInteractable
{
    [SerializeField] Item HealthPack;

    void Start()
    {

    }

    public void FillHealth()
    {
        if(GameManager.Instance.playerScript.CheckFunds() > 50)
        {
            GameManager.Instance.playerScript.Heal(5);
        }
    }

    public void BuyHealthPack()
    {
        InventoryManager.Instance.AddItem(HealthPack);
    }

    public void Interact()
    {
        GameManager.Instance.OpenStore();
    }

   
}
