using DG.Tweening;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour, IDamage
{
    [Header("Health Settings")]
    [SerializeField] public int health = 10;
    private int maxHealth;
    [Space(10)]
    //The amount of time after getting hit before the enemy will start regenerating health
    [SerializeField] private float healthRegenDelay = 10.0f;
    //How frequently the health will be regenerated once the initial delay has passed
    [SerializeField] private float healthRegenRate = 3.0f;
    //The amount of health to regenerate per regen tick
    [SerializeField] private int healthRegenAmount = 1;
    Coroutine regenCoroutine;
    public int cashToDrop = 10;

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
    float refreshTime;
    bool near;

    Animator animator;

    [SerializeField] SkinnedMeshRenderer meshRenderer;
    Color defaultMatColor;

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
        refreshTime = farPositionRefreshTime;
        defaultMatColor = meshRenderer.material.color;
    }

    private void Update()
    {
        Vector3 lookVec = navigationAgent.hasPath ? navigationAgent.steeringTarget - transform.position : navigationAgent.desiredVelocity;
        lookVec.y = 0f;
        if (lookVec.sqrMagnitude > 0.0001f && !CurrentState("Death"))
        {
            Quaternion lookRotation = Quaternion.LookRotation(lookVec);
            float maxTurn = navigationAgent.angularSpeed * Time.deltaTime;
            transform.rotation = Quaternion.RotateTowards(transform.rotation, lookRotation, maxTurn);
        }

        navigationAgent.nextPosition = transform.position;
        if (!navigationAgent.isOnNavMesh) return;
        positionUpdateTimer += Time.deltaTime;
        AgentSync();
        if (positionUpdateTimer < refreshTime) return;
        if (!near && farPositionRefreshVariability > 0) refreshTime = Random.Range(farPositionRefreshTime - farPositionRefreshVariability, farPositionRefreshTime + farPositionRefreshVariability);
        UpdateNearFar();
        navigationAgent.destination = player.transform.position;
        positionUpdateTimer = 0;
    }

    void AgentSync()
    {
        if (!navigationAgent.isOnNavMesh)
        {
            if (NavMesh.SamplePosition(transform.position, out NavMeshHit hit, 2f, navigationAgent.areaMask) || NavMesh.SamplePosition(transform.position, out hit, 6f, navigationAgent.areaMask))
            {
                navigationAgent.Warp(hit.position);
            }
            return;
        }
        float drift = Vector3.Distance(navigationAgent.nextPosition, transform.position);
        if (drift > 0.75f)
        {
            navigationAgent.Warp(transform.position);
        }
    }

    bool CurrentState(string stateName)
    {
        AnimatorStateInfo info = animator.GetCurrentAnimatorStateInfo(0);
        if (animator.IsInTransition(0))
        {
            AnimatorStateInfo nextInfo = animator.GetNextAnimatorStateInfo(0);
            return info.shortNameHash == Animator.StringToHash(stateName) || nextInfo.shortNameHash == Animator.StringToHash(stateName);
        }
        return info.shortNameHash == Animator.StringToHash(stateName);
    }

    void FootstepSound()
    {
        SoundManager.Instance.PlaySoundFXClip(SoundType.Footstep, transform.position, AudioGroup.SFX, 0.05f, 0.05f, 0.5f, 0.1f);
    }

    void OnAnimatorMove()
    {
        if (animator == null) return;
        Vector3 delta = animator.deltaPosition;
        Vector3 proposedDelta = transform.position + delta;

        if (navigationAgent.isOnNavMesh && NavMesh.SamplePosition(proposedDelta, out NavMeshHit hit, navigationAgent.radius * 2f, navigationAgent.areaMask))
        {
            transform.position = hit.position;
        } else
        {
            transform.position = proposedDelta;
        }

        if (Time.deltaTime > 0f)
        {
            navigationAgent.velocity = delta / Time.deltaTime;
        }

        navigationAgent.nextPosition = transform.position;
        AgentSync();
    }

    void UpdateNearFar()
    {
        if (near && Vector3.Distance(transform.position, player.transform.position) > farNearDistance)
        {
            refreshTime = farPositionRefreshTime;
            near = false;
        } else if (!near && Vector3.Distance(transform.position,player.transform.position) <= farNearDistance)
        {
            refreshTime = nearPositionRefreshTime;
            near = true;
        }
    }

    public void takeDamage(int amount)
    {
        health -= amount;
        health = Mathf.Clamp(health, 0, maxHealth);
        meshRenderer.material.DOColor(Color.red, 0.1f).OnComplete(() =>
        {
            meshRenderer.material.DOColor(defaultMatColor, 0.1f);
        });
        if (health == 0)
        {
            navigationAgent.isStopped = true;
            GetComponent<Collider>().enabled = false;
            animator.SetTrigger("Death");
            GameManager.Instance.playerScript.UpdateWallet(cashToDrop);
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
