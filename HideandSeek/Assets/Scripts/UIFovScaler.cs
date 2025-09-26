using UnityEngine;

[ExecuteAlways]
public class UIFovScaler : MonoBehaviour
{
    public Camera uiCamera;          
    public float referenceFOV = 60f; 
    public Transform canvasRoot;     

    Vector3 baseScale;

    void Awake()
    {
        if (!canvasRoot) canvasRoot = transform;
        baseScale = canvasRoot.localScale;
        if (!uiCamera) uiCamera = Camera.main;
    }

    void LateUpdate()
    {
        if (!uiCamera || !canvasRoot) return;

        float s = Mathf.Tan(Mathf.Deg2Rad * uiCamera.fieldOfView * 0.5f)
                / Mathf.Tan(Mathf.Deg2Rad * referenceFOV * 0.5f);

        canvasRoot.localScale = baseScale * s;
    }
}
