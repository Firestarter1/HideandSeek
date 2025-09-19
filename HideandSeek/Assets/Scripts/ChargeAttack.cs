using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;
using System.Collections;

public class ChargeAttack : MonoBehaviour
{
    [Header("Attack")]
    [SerializeField] int damage = 1;
    [SerializeField] float triggerRange = 10.0f;
    [SerializeField] float range = 2.0f;
    [SerializeField] float cooldown = 10.0f;
    [Space(10)]
    [SerializeField] Vector2 knockbackForce = new Vector2(5f, 0f);
    [SerializeField] float crowdPushRadius = 0.9f;
    [SerializeField] float chargeSteerSpeed = 20f;
    [SerializeField] float wallCheckDistance = 0.75f;
    [SerializeField] float targetRehitCooldown = 0.5f;
    [SerializeField] LayerMask chargeStopLayerMask;
    [SerializeField] LayerMask crowdPushLayerMask;
    

    [Header("Targeting")]
    [SerializeField] Transform eyePosition;
    [SerializeField, Range(0f, 360f)] float fov = 90f;
    [SerializeField] LayerMask targetLayerMask;
    [SerializeField] LayerMask obstructionLayerMask;

    Animator animator;
    NavMeshAgent navigationAgent;
    float cooldownTimer = 0.0f;
    bool isCharging = false;
    float originalSteerSpeed;
    List<GameObject> hitObjects = new List<GameObject>();

    private void Start()
    {
        if (!TryGetComponent<NavMeshAgent>(out navigationAgent))
        {
            Destroy(this);
            return;
        }
        animator = GetComponent<Animator>();
        if (!eyePosition) eyePosition = transform;
        originalSteerSpeed = navigationAgent.angularSpeed;
        cooldownTimer = cooldown;
    }

    private void Update()
    {
        if (isCharging)
        {
            DoArea();
            if (CollidingWithWall(out RaycastHit hit))
            {
                FinishCharge();
                return;
            }
        }

        //if (navigationAgent.isStopped) return;
        cooldownTimer += Time.deltaTime;
        if (cooldownTimer < cooldown) return;

        if (!ReachedTarget()) return;

        GameObject target = GameManager.Instance.player;

        if (TargetInRange(target.transform, triggerRange))
        {
            animator?.SetTrigger("Attack");

            /*Vector3 look = target.transform.position - transform.position;
            look.y = 0;
            if (look.magnitude > 0.0001f)
            {
                transform.rotation = Quaternion.LookRotation(look);
            }*/
        }
    }

    public void StartCharge()
    {
        isCharging = true;
        hitObjects.Clear();
        animator?.ResetTrigger("Attack");
        navigationAgent.angularSpeed = chargeSteerSpeed;
    }

    void FinishCharge()
    {
        cooldownTimer = 0f;
        isCharging = false;
        animator?.ResetTrigger("Attack");

        animator?.SetTrigger("Charge Finish");
        navigationAgent.angularSpeed = originalSteerSpeed;
    }

    bool CollidingWithWall(out RaycastHit hit)
    {
        Vector3 origin = transform.position + Vector3.up * 0.2f;
        float radius = Mathf.Max(0.1f, navigationAgent.radius * 0.9f);
        Vector3 direction = transform.forward;
        direction.y = 0f;
        direction.Normalize();

        int mask = chargeStopLayerMask;

        return Physics.SphereCast(origin, radius, direction, out hit, wallCheckDistance, mask, QueryTriggerInteraction.Ignore);
    }

    void DoArea()
    {
        Vector3 center = transform.position + Vector3.up * 0.3f;

        Collider[] collisions = Physics.OverlapSphere(center, crowdPushRadius, crowdPushLayerMask, QueryTriggerInteraction.Ignore);

        foreach (Collider collider in collisions)
        {
            if (hitObjects.Contains(collider.gameObject) || collider.gameObject == gameObject) continue;
            Attack(collider.gameObject);
        }
        
    }

    void ApplyKinematicKnockback(GameObject target, Vector3 impulse)
    {
        Vector3 disp = impulse * Time.deltaTime;
        float dist = disp.magnitude;
        if (dist <= 0f) return;

        Vector3 dir = disp / Mathf.Max(dist, 1e-6f);

        Vector3 rayOrigin = target.transform.position + Vector3.up * 0.2f;
        if (Physics.Raycast(rayOrigin, dir, out var hit, dist, chargeStopLayerMask, QueryTriggerInteraction.Ignore))
            dist = Mathf.Max(0f, hit.distance - 0.02f);

        if (dist <= 0f) return;

        if (target.TryGetComponent<CharacterController>(out var cc))
        {
            cc.Move(dir * dist);
            return;
        }

        if (target.TryGetComponent<NavMeshAgent>(out var agent))
        {
            Vector3 desired = target.transform.position + dir * dist;

            if (agent.isOnNavMesh &&
                NavMesh.SamplePosition(desired, out var navHit, agent.radius * 1.5f, agent.areaMask))
            {
                agent.Warp(navHit.position);
            }
            else
            {
                target.transform.position = desired;
            }
            return;
        }

        target.transform.position += dir * dist;
    }

    public void Attack(GameObject target)
    {
       
        
        if (TargetInRange(target.transform, range))
        {
            
            Vector3 dir = target.transform.position - transform.position;
            dir.y = 0f;
            if (dir.sqrMagnitude < 0.001f) dir = transform.forward;
            dir.Normalize();
            Vector3 pushbackForce = dir * knockbackForce.x + Vector3.up * knockbackForce.y;
            //ApplyKinematicKnockback(target, pushbackForce);

            if (hitObjects.Contains(target)) { Debug.Log("Contains " + target.name); return; };

            if (target.TryGetComponent<IDamage>(out IDamage dmg) && target == GameManager.Instance.player)
            {
                dmg.takeDamage(damage);
            }
            hitObjects.Add(target);
            StartCoroutine(EnableRehit(target));
        }
    }

    IEnumerator EnableRehit(GameObject obj)
    {
        yield return new WaitForSeconds(targetRehitCooldown);
        hitObjects.Remove(obj);
    }

    bool ReachedTarget()
    {
        if (!navigationAgent.isOnNavMesh) return false;
        if (navigationAgent.pathPending) return false;

        float distance = Vector3.Distance(navigationAgent.transform.position, GameManager.Instance.player.GetComponent<Collider>().ClosestPoint(navigationAgent.transform.position));
        bool inStoppingDistance = distance <= navigationAgent.stoppingDistance + 0.1f;


        return inStoppingDistance;
    }

    bool TargetInRange(Transform target, float _range)
    {
        Vector3 eyePos = eyePosition.position;
        Vector3 checkPosition = target.position;
        if (target.TryGetComponent<Collider>(out Collider collider))
        {
            checkPosition = collider.ClosestPoint(eyePos);
        }

        if ((checkPosition - eyePos).sqrMagnitude > _range * _range) return false;

        Vector3 to = checkPosition - eyePos;

        float distance = to.magnitude;

        if (distance <= 0.0001f) return true;

        if (Vector3.Angle(new Vector3(eyePosition.forward.x, 0f, eyePosition.forward.z), new Vector3(to.x, 0f, to.z)) > fov * 0.5f) return false;

        to /= distance;

        int mask = targetLayerMask | obstructionLayerMask;

        RaycastHit hit;
        bool connected = Physics.Raycast(eyePos, to, out hit, distance, mask, QueryTriggerInteraction.Ignore);
        if (!connected) return true;

        return hit.transform.root == target.root;
    }

}
