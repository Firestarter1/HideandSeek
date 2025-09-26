using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class HealthBarScript : MonoBehaviour
{
    [SerializeField] Material fillMaterial;
    [SerializeField] Gradient healthGradient;
    [SerializeField] Image fillImage;
    [SerializeField] float lerpSpeed = 0.5f;
    [SerializeField] RectTransform healthIcon;
    [SerializeField] Vector2 spinSpeed;
    Tween fillTween;
    Tween colorTween;

    float percent = 1f;

    void Start()
    {
        GameManager.Instance.playerScript.healthUpdated.AddListener(UpdateMaterialFloat);
    }

    private void Update()
    {
        healthIcon.Rotate(Vector3.up, Mathf.Lerp(spinSpeed.x, spinSpeed.y, percent) * Time.deltaTime, Space.Self);
    }

    void UpdateMaterialFloat(float percent)
    {
        this.percent = percent;
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
