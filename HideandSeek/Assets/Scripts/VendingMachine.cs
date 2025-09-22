using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class VendingMachine : MonoBehaviour, IInteractable
{
    [SerializeField] public TextMeshProUGUI costText;
    [SerializeField] private MeshRenderer sellingMesh;
    [SerializeField] private MeshFilter sellingMeshFilter;

    public int interactCost;
    public GameObject model;
    public UnityEvent functionCallOnInteract;

    public void Interact()
    {
        if (GameManager.Instance.playerScript.CheckFunds() >= interactCost)
        {
            functionCallOnInteract.Invoke();
            GameManager.Instance.playerScript.UpdateWallet(-interactCost);
        }
    }

    void Awake()
    {
        costText.text = "$" + interactCost.ToString();
        sellingMesh.sharedMaterials = model.GetComponent<MeshRenderer>().sharedMaterials;
        sellingMeshFilter.sharedMesh = model.GetComponent<MeshFilter>().sharedMesh;
    }
}
