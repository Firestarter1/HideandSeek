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
    [SerializeField] float textSpacingMagnitude = 2.0f;
    [SerializeField] float textSpacingSpeed = 1.0f;

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
        DoTextSpacing(textSpacingMagnitude);
        DoTextSpacing2(textSpacingMagnitude, textSpacingSpeed);
    }

    void DoLogoSpin()
    {
        logo.DOLocalRotate(new Vector3(0,360F,0f), logoSpinSpeed, RotateMode.LocalAxisAdd).SetUpdate(true).SetEase(Ease.InOutExpo).SetLoops(-1, LoopType.Incremental);
    }

    void DoTextSpacing(float amount)
    {
        DOTween.To(() => missionText.characterSpacing, x => { missionText.characterSpacing = x; missionText.havePropertiesChanged = true; }, missionText.characterSpacing + amount, textSpacingSpeed).SetUpdate(true).SetEase(Ease.InOutSine).OnComplete( () => DoTextSpacing(-amount));
    }
    //this is how you know i've reached my limit when it comes to naming methods
    void DoTextSpacing2(float amount, float delay)
    {
        DOTween.To(() => completeText.characterSpacing, x => { completeText.characterSpacing = x; completeText.havePropertiesChanged = true; }, completeText.characterSpacing + amount, textSpacingSpeed).SetUpdate(true).SetEase(Ease.InOutSine).SetDelay(delay).OnComplete(() => DoTextSpacing2(-amount, 0));
    }
}
