using UnityEngine;

public class Store : MonoBehaviour, IInteractable
{
    public void Interact()
    {
        GameManager.Instance.OpenStore();
    }
}
