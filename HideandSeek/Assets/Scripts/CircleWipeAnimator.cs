using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
[ExecuteAlways]
public class CircleWipeAnimator : MonoBehaviour
{
    Material mat;
    Graphic g;
    [Range(0.0f,1.0f)]
    public float fillPercent = 0.0f;
    void OnEnable()
    {
        g = GetComponent<Graphic>();    
        mat = g.material;
    }

    void OnValidate()
    {
        if (!Application.isPlaying)
        {
            mat?.SetFloat("_Fill", fillPercent);

            g.SetMaterialDirty();

        }
    }
    private void LateUpdate()
    {
        mat?.SetFloat("_Fill", fillPercent);
        
        g.SetMaterialDirty();

        #if UNITY_EDITOR
        UnityEditor.EditorApplication.QueuePlayerLoopUpdate();
        UnityEditor.SceneView.RepaintAll();
        #endif
    }
}
