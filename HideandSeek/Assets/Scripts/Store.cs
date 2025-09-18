using System.Xml.Serialization;
using UnityEngine;
using UnityEngine.UI;

public class Store : MonoBehaviour, IInteractable
{
    [SerializeField] Item HealthPack;

    [SerializeField] GameObject[] areas;

    void Start()
    {

    }

    public void Interact()
    {
        GameManager.Instance.OpenStore();
    }

    public void FillHealth()
    {
        if (GameManager.Instance.playerScript.CheckFunds() >= 50)
        {
            GameManager.Instance.playerScript.UpdateWallet(-50);
            GameManager.Instance.playerScript.Heal(5);
        }
    }

    public void BuyHealthPack()
    {
        if (GameManager.Instance.playerScript.CheckFunds() >= 50)
        {
            GameManager.Instance.playerScript.UpdateWallet(-50);
            InventoryManager.Instance.AddItem(HealthPack);
        }
    }

    public void UnlockArea1()
    {
        if(GameManager.Instance.playerScript.CheckFunds() >= 50)
        {
            GameManager.Instance.playerScript.UpdateWallet(50);
            areas[0].SetActive(false);
        }
    }
}
