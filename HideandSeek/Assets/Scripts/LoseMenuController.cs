using UnityEngine;
using UnityEngine.UI;
public class LoseMenuController : MonoBehaviour
{
    Animator animator;
    Material mat;
    void Start()
    {
        animator = GetComponent<Animator>();
        mat = GetComponent<Image>().material;
    }

    public void TriggerLoseMenu()
    {
        animator.SetTrigger("Lose In");
    }

    private void LateUpdate()
    {
        mat.SetFloat("_UnscaledTime", Time.unscaledTime);
    }
}
