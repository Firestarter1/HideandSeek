using DG.Tweening;
using UnityEngine;

using UnityEngine.UI;

public class LoadingSceneController : MonoBehaviour
{
    [SerializeField] LoadingScreen loadingScreen;
    [SerializeField] Image glowImage;
    [SerializeField] RectTransform logo;
    [SerializeField] float glowSpeed;
    [SerializeField] float logoSpinSpeed;
    [SerializeField] float logoFloatSpeed;
    [SerializeField] float logoFloatMagnitude = 1.0f;
    Tween logoSpin;
    Tween logoFloat;
    Tween glow;

    void Start()
    {
        string target = Loader.GetNextScene();
        if (string.IsNullOrEmpty(target))
        {
            target = "Main Menu";
        }
        loadingScreen.StartLoad(target);
        DoLogoSpin();
        DoGlow();
        logoFloat = logo.DOAnchorPosY(logo.transform.position.y + (logoFloatMagnitude/4f), logoFloatSpeed / 2f).SetEase(Ease.InOutSine).OnComplete( () => DoLogoFloat());
    }

    private void OnDestroy()
    {
        logoSpin?.Kill();
        glow?.Kill();
        logoFloat?.Kill();
    }

    void DoLogoSpin()
    {
        logoSpin = logo.DORotate(logo.rotation.eulerAngles + (Vector3.up * 180f), logoSpinSpeed).SetEase(Ease.InOutExpo).OnComplete(() => DoLogoSpin());
    }

    void DoLogoFloat()
    {
        logoFloat = logo.DOAnchorPosY(logo.transform.position.y - (logoFloatMagnitude/2f), logoFloatSpeed).SetEase(Ease.InOutSine).OnComplete(() => logoFloat = logo.DOAnchorPosY(logo.transform.position.y + (logoFloatMagnitude/2f), logoFloatSpeed).SetEase(Ease.InOutSine).OnComplete(() => DoLogoFloat()));
    }
    void DoGlow()
    {
        glow = glowImage.DOFade(1f, glowSpeed/2f).SetEase(Ease.OutSine).OnComplete( () => glow = glowImage.DOFade(0f, glowSpeed / 2f).SetEase(Ease.InSine).OnComplete(() => DoGlow()));
    }
}
