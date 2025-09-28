using UnityEngine;

public class MainMenuAnimator : MonoBehaviour
{
    Animator animator;

    bool transitioning = false;

    private void Start()
    {
        animator = GetComponent<Animator>();
    }

    public void CreditsWindowIn()
    {
        if (transitioning ) return;
        animator.ResetTrigger("Credits Off");
        animator.SetTrigger("Credits On");
        transitioning = true;
    }

    public void CreditsWindowOut()
    {
        if (transitioning) return;
        animator.ResetTrigger("Credits On");
        animator.SetTrigger("Credits Off");
        transitioning = true;
    }

    public void AudioSettingsIn()
    {
        if (transitioning) return;
        animator.ResetTrigger("Settings Off");
        animator.SetTrigger("Settings On");
        transitioning = true;
    }

    public void AudioSettingsOut() {
        if (transitioning) return;
        animator.ResetTrigger("Settings On");
        animator.SetTrigger("Settings Off");
        transitioning = true;
    }

    public void FinishTransition()
    {
        transitioning = false;
    }
    
}
