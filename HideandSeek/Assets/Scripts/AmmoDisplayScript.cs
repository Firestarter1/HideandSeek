using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AmmoDisplayScript : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI currentAmmoText;
    [SerializeField] TextMeshProUGUI storedAmmoText;

    private void Start()
    {
        GameManager.Instance.playerScript.ammoUpdated.AddListener(UpdateAmmoText);
    }

    void UpdateAmmoText(int current, int stored)
    {
        if (current < 0)
        {
            currentAmmoText.text = "-";
        } else
        {
            currentAmmoText.text = current.ToString();
        }
        if (stored < 0)
        {
            storedAmmoText.text = "";
        } else
        {
            storedAmmoText.text = stored.ToString();
        }
    }
}
