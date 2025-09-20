using UnityEngine;

public class HealthBarScript : MonoBehaviour
{
    [SerializeField] Material fillMaterial;

    void Start()
    {
        GameManager.Instance.playerScript.healthUpdated.AddListener(UpdateMaterialFloat);
    }

    void UpdateMaterialFloat(float percent)
    {
        fillMaterial?.SetFloat("_Fill", percent);
    }
}
