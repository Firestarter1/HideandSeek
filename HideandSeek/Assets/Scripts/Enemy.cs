using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour, IDamage
{
    [Header("Health Settings")]
    [SerializeField] private int health = 10;
    private int maxHealth;
    [Space(10)]
    //The amount of time after getting hit before the enemy will start regenerating health
    [SerializeField] private float healthRegenDelay = 10.0f;
    //How frequently the health will be regenerated once the initial delay has passed
    [SerializeField] private float healthRegenRate = 3.0f;
    //The amount of health to regenerate per regen tick
    [SerializeField] private int healthRegenAmount = 1;
    Coroutine regenCoroutine;

    [Header("Navigation Settings")]
    NavMeshAgent navigationAgent;
    [SerializeField] private float farNearDistance;
    [SerializeField] private float farPositionRefreshTime;
    [SerializeField] private float farPositionRefreshVariability = 2.0f;
    [SerializeField] private float nearPositionRefreshTime;
    [Space(10)]
    [SerializeField] private Color nearFarGizmoColor;

    GameObject player;
    float positionUpdateTimer = 0.0f;
    bool near = false;

    Animator animator;

    private void Start()
    {
        if (!TryGetComponent<NavMeshAgent>(out navigationAgent))
        {
            Destroy(this); return;
        }
        player = GameManager.Instance.player;
        maxHealth = health;
        animator = GetComponent<Animator>();
        navigationAgent.updatePosition = false;
        navigationAgent.updateRotation = false;
        animator.applyRootMotion = true;
    }

    private void Update()
    {
        Vector3 desired = navigationAgent.desiredVelocity;
        float desiredSpeed = desired.magnitude;

        if (desiredSpeed > 0.01f)
        {
            Vector3 dir = desired.normalized;
            dir.y = 0;
            Quaternion targetRotation = Quaternion.LookRotation(dir, Vector3.up);

            if (IsWalking())
            {
                transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, navigationAgent.angularSpeed);

            }
            
        }

        navigationAgent.nextPosition = transform.position;

        positionUpdateTimer += Time.deltaTime;
        if (positionUpdateTimer < (near == true ? nearPositionRefreshTime : farPositionRefreshTime)) return;

        UpdateNearFar();
        navigationAgent.destination = player.transform.position;
        
    }

    bool IsWalking()
    {
        AnimatorStateInfo info = animator.GetCurrentAnimatorStateInfo(0);
        if (animator.IsInTransition(0))
        {
            AnimatorStateInfo nextInfo = animator.GetNextAnimatorStateInfo(0);
            return info.shortNameHash == Animator.StringToHash("Walking") || nextInfo.shortNameHash == Animator.StringToHash("Walking");
        }
        return info.shortNameHash == Animator.StringToHash("Walking");
    }

    void FootstepSound()
    {
        SoundManager.Instance.PlaySoundFXClip(SoundType.Footstep, transform.position, AudioGroup.SFX, 0.2f, 0.05f, 0.5f, 0.1f);
    }

    void OnAnimatorMove()
    {
        Vector3 delta = animator.deltaPosition;

        transform.position += delta;

        if (Time.deltaTime > 0f)
        {
            navigationAgent.velocity = delta / Time.deltaTime;
        }

        navigationAgent.nextPosition = transform.position;
    }

    void UpdateNearFar()
    {
        if (near && Vector3.Distance(transform.position, player.transform.position) > farNearDistance)
        {
            near = false;
        } else if (!near && Vector3.Distance(transform.position,player.transform.position) <= farNearDistance)
        {
            near = true;
        }
    }

    public void takeDamage(int amount)
    {
        health -= amount;
        health = Mathf.Clamp(health, 0, maxHealth);
        if (health == 0)
        {
            navigationAgent.isStopped = true;
            GetComponent<Collider>().enabled = false;
            animator.SetTrigger("Death");
            //Emit health depleted signal
            return;
        }
        if (regenCoroutine != null)
        {
            StopCoroutine(regenCoroutine);
        }
        regenCoroutine = StartCoroutine(HealthRegenDelay());
    }

    IEnumerator HealthRegenDelay()
    {
        yield return new WaitForSeconds(healthRegenDelay);
        regenCoroutine = StartCoroutine(HealthRegenCoroutine());
    }

    IEnumerator HealthRegenCoroutine()
    {
        yield return new WaitForSeconds(healthRegenRate);
        health += healthRegenAmount;
        health = Mathf.Clamp(health, 0, maxHealth);
        if (health < maxHealth)
        {
            regenCoroutine = StartCoroutine(HealthRegenCoroutine());
        }
    }

#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        Gizmos.color = nearFarGizmoColor;
        Gizmos.DrawWireSphere(transform.position, farNearDistance);
    }
#endif
}
