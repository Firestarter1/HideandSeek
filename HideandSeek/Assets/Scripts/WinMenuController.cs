using DG.Tweening;
using TMPro;
using UnityEngine;

public class WinMenuController : MonoBehaviour
{
    Animator animator;
    public bool transitioning = false;
    [SerializeField] RectTransform logo;
    [SerializeField] TextMeshProUGUI missionText;
    [SerializeField] TextMeshProUGUI completeText;
    [SerializeField] float logoSpinSpeed;
    [SerializeField] float logoFloatSpeed;
    [SerializeField] float logoFloatMagnitude = 1.0f;


    void Start()
    {
        animator = GetComponent<Animator>();
    }

    public void WinMenuIn()
    {
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }
        animator.SetTrigger("Menu In");
        
        transitioning = true;
    }

    public void FinishTransition()
    {
        transitioning = false;
        DoLogoSpin();
    }

    void DoLogoSpin()
    {
        logo.DOLocalRotate(new Vector3(0,360F,0f), logoSpinSpeed, RotateMode.LocalAxisAdd).SetUpdate(true).SetEase(Ease.InOutExpo).SetLoops(-1, LoopType.Incremental);
    }

}
