using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IInteractable
{
    public void Interact();
}

public class Interactor : MonoBehaviour
{
    [SerializeField] GameObject interactPrompt;

    public Transform interactSource;
    public float interactRange;
    [SerializeField] LayerMask ignoreLayers;

    private void Start()
    {
        if (!interactPrompt)
        {
            interactPrompt = GameManager.Instance.interactPrompt;
        }
    }

    void Update()
    {
        Ray r = new Ray(interactSource.position, interactSource.forward);
        bool hit = Physics.Raycast(r, out RaycastHit hitInfo, interactRange, ~ignoreLayers, QueryTriggerInteraction.Ignore);
        if (!hit)
        {
            interactPrompt.SetActive(false);
            return;
        }

        IInteractable interact = hitInfo.collider.GetComponent<IInteractable>();

        if (interact != null)
        {
            interactPrompt.SetActive(true);
        }
        else if (!hit || interact == null)
        {
            interactPrompt.SetActive(false);
        }
        if (Input.GetButtonDown("Interact"))
        {
            if (interact != null)
            {
                interact.Interact();
            }
        }
    }
}
