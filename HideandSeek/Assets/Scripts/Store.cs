using System.Xml.Serialization;
using UnityEngine;
using UnityEngine.UI;

public class Store : MonoBehaviour, IInteractable
{
    public int wallet;
    public int HealthPack;
    public int Granade;
    public int GunUpgrade;

    void Start()
    {

    }

    public void BuyHealth()
    {
        if(GameManager.Instance.playerScript.CheckFunds() > 50)
        {
            GameManager.Instance.playerScript.Heal(5);
        }
    }

    public void Interact()
    {
        GameManager.Instance.OpenStore();
    }

   
}
