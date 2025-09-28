using System;
using UnityEngine;

public class ModelPreviewRenderer : MonoBehaviour
{
    public Camera cam;
    public Transform modelTransform;
    public Light modelLight;
    public float edgePadding = 1.1f;

    private void Reset()
    {
        cam = GetComponentInChildren<Camera>();
        modelTransform = (new GameObject("Model Anchor")).transform;
        modelTransform.SetParent(transform,false);
    }

    public void Render(GameObject model, RenderTexture rt)
    {
        
        FitCameraToModel(model);
        cam.targetTexture = rt;
        cam.Render();
        cam.targetTexture = null;
    }

    void FitCameraToModel(GameObject model)
    {
        Renderer[] renderers = model.GetComponentsInChildren<Renderer>(true);
        if (renderers.Length == 0) return;

        Bounds b = new Bounds(renderers[0].bounds.center, Vector3.zero);
        foreach (Renderer renderer in renderers) {
            b.Encapsulate(renderer.bounds); 
        }

        Vector3 target = b.center;

        float halfXY = Mathf.Max(b.extents.x, b.extents.y) * edgePadding;
        cam.orthographicSize = halfXY;
        float depth = b.extents.z * 2f + 1f;

        cam.transform.position = target - cam.transform.forward * depth;
        cam.nearClipPlane = 0.01f;
        cam.farClipPlane = depth + b.extents.magnitude * 4f + 2f;
        cam.transform.LookAt(target);
    }


}
