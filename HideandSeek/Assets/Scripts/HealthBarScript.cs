using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class HealthBarScript : MonoBehaviour
{
    [SerializeField] Material fillMaterial;
    [SerializeField] Gradient healthGradient;
    [SerializeField] Image fillImage;
    [SerializeField] float lerpSpeed = 0.5f;
    Tween fillTween;
    Tween colorTween;

    void Start()
    {
        GameManager.Instance.playerScript.healthUpdated.AddListener(UpdateMaterialFloat);
    }

    void UpdateMaterialFloat(float percent)
    {
        if (fillTween != null)
        {
            fillTween.Kill();
        }
        if (colorTween != null)
        {
            colorTween.Kill();
        }
        fillTween = fillMaterial.DOFloat(percent, "_Fill", lerpSpeed).OnComplete( () =>
        {
            fillTween = null;
        });
        colorTween = fillImage.DOColor(healthGradient.Evaluate(percent), lerpSpeed).OnComplete(() =>
        {
            colorTween = null;
        });
    }

}
