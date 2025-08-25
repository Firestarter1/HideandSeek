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

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetButtonDown("Interact"))
        {
            Ray r = new Ray(interactSource.position, interactSource.forward);
            if(Physics.Raycast(r, out RaycastHit hitInfo, interactRange))
            {
                if (hitInfo.collider.TryGetComponent(out IInteractable interactObj))
                    interactObj.Interact();
            }

        }
    }
}
