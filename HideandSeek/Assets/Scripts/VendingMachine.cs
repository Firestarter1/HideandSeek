using DG.Tweening;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class VendingMachine : MonoBehaviour, IInteractable
{
    [SerializeField] public TextMeshProUGUI costText;
    //[SerializeField] private MeshRenderer sellingMesh;
   // [SerializeField] private MeshFilter sellingMeshFilter;
    [SerializeField] float cooldown = 1.0f;

    bool onCooldown = false;

    public int interactCost;
    //public GameObject model;
    public UnityEvent functionCallOnInteract;

    public void Interact()
    {
        if (onCooldown) return;
        StartCoroutine(DoCooldown());
        if (GameManager.Instance.playerScript.CheckFunds() >= interactCost)
        {
            functionCallOnInteract.Invoke();
            GameManager.Instance.playerScript.UpdateWallet(-interactCost);
            SoundManager.Instance.PlaySoundFXClip(SoundType.Vending_Success, transform.position, AudioGroup.SFX, 1f, 0.05f, 1.0f, 0.1f);
            StartCoroutine(DelayedUseSound());
            transform.DOShakeScale(0.25f, 0.2f, 20);
        } else
        {
            SoundManager.Instance.PlaySoundFXClip(SoundType.Vending_Fail, transform.position, AudioGroup.SFX, 1f, 0.05f, 1.0f, 0.1f);
        }
        transform.DOShakeRotation(0.25f, 5f, 50, 90);
        
    }

    IEnumerator DelayedUseSound()
    {
        yield return new WaitForSeconds(0.1f);
        SoundManager.Instance.PlaySoundFXClip(SoundType.Vending_Use, transform.position, AudioGroup.SFX, 0.5f, 0.05f, 1.0f, 0.1f);
    }

    IEnumerator DoCooldown()
    {
        onCooldown = true;
        yield return new WaitForSeconds(cooldown);
        onCooldown = false;
    }

    void Awake()
    {
        costText.text = "$" + interactCost.ToString();
        //sellingMesh.sharedMaterials = model.GetComponent<MeshRenderer>().sharedMaterials;
        //sellingMeshFilter.sharedMesh = model.GetComponent<MeshFilter>().sharedMesh;
    }
}
