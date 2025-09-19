using NUnit.Framework.Constraints;
using UnityEngine;
using UnityEngine.AI;

public class RangedAttack : MonoBehaviour
{
    [Header("Attack")]
    [SerializeField] GameObject projectile;
    [SerializeField] Transform shootPosition;
    [SerializeField] float projectileSpeed = 22f;
    [SerializeField] float range = 25.0f;
    [SerializeField] float cooldown = 1.0f;
    [SerializeField] bool highArc = false;

    [Header("Targeting")]
    [SerializeField] Transform eyePosition;
    [SerializeField, Range(0f, 360f)] float fov = 90f;
    [SerializeField] LayerMask targetLayerMask;
    [SerializeField] LayerMask obstructionLayerMask;
    [SerializeField] int arcSamples = 12;

    Animator animator;
    NavMeshAgent navigationAgent;
    float cooldownTimer = 0.0f;

    Vector3 storedHighVec;
    Vector3 storedLowVec;
    Vector3 storedAimPointt;
    bool hasStoredArc = false;

    private void Start()
    {
        if (!TryGetComponent<NavMeshAgent>(out navigationAgent))
        {
            Destroy(this);
            return;
        }
        animator = GetComponent<Animator>();
        if (!eyePosition) eyePosition = transform;
        if (!shootPosition) shootPosition = eyePosition;
    }
    private void Update()
    {
        //if (navigationAgent.isStopped) return;
        cooldownTimer += Time.deltaTime;
        if (cooldownTimer < cooldown) return;

        if (!ReachedTarget()) return;

        GameObject target = GameManager.Instance.player;

        if (TargetInRange(target.transform))
        {
            Vector3 aimPoint = target.TryGetComponent<Collider>(out Collider collider) ? collider.bounds.center : target.transform.position;

            if (!CalcBallisticArc(shootPosition.position, aimPoint, projectileSpeed, out Vector3 vLow, out Vector3 vHigh)) { Debug.Log("Ballistic arc failed"); return; }
            storedHighVec = vHigh;
            storedLowVec = vLow;
            storedAimPointt = aimPoint;
            hasStoredArc = true;
            //if (!ArcLOS(shootPosition.position, launchVelocity, aimPoint, projectileRadius, arcSamples, mask, target.transform.root)) { Debug.Log("Arc LOS failed"); return; }

            animator?.SetTrigger("Attack");

            /*Vector3 look = target.transform.position - transform.position;
            look.y = 0;
            if (look.magnitude > 0.0001f)
            {
                transform.rotation = Quaternion.LookRotation(look);
            }*/
        }
    }

    Vector3 AimPoint(Transform target)
    {
        Vector3 origin = shootPosition ? shootPosition.position : transform.position;

        if (target.TryGetComponent<Collider>(out var col))
        {
            Vector3 dir = (target.position - origin);
            if (dir.sqrMagnitude < 1e-6f) dir = transform.forward;
            return col.ClosestPoint(origin + dir.normalized * 0.02f);
        }
        return target.position;
    }

    public void Attack()
    {
        cooldownTimer = 0f;
        Vector3 launchVelocity = highArc ? storedHighVec : storedLowVec;
        
        GameObject projectileInstance = Instantiate(projectile, shootPosition.position, Quaternion.LookRotation(launchVelocity));
        projectileInstance.GetComponent<Rigidbody>().linearVelocity = launchVelocity;
        Vector3 aimPoint = GameManager.Instance.player.TryGetComponent<Collider>(out Collider collider) ? collider.bounds.center : GameManager.Instance.player.transform.position;
        Vector3 face = aimPoint - transform.position;
        face.y = 0f;
        transform.rotation = Quaternion.LookRotation(face);
    }

    bool ReachedTarget()
    {
        if (!navigationAgent.isOnNavMesh) return false;
        if (navigationAgent.pathPending) return false;

        float distance = Vector3.Distance(navigationAgent.transform.position, GameManager.Instance.player.GetComponent<Collider>().ClosestPoint(navigationAgent.transform.position));
        bool inStoppingDistance = distance <= navigationAgent.stoppingDistance + 0.1f;

        bool nearby = navigationAgent.hasPath && navigationAgent.remainingDistance != Mathf.Infinity && navigationAgent.remainingDistance <= navigationAgent.stoppingDistance + 0.1f;

        return inStoppingDistance || nearby;
    }




    bool TargetInRange(Transform target)
    {
        Vector3 eyePos = eyePosition.position;
        Vector3 checkPosition = target.position;
        if (target.TryGetComponent<Collider>(out Collider collider))
        {
            checkPosition = collider.ClosestPoint(eyePos);
        }

        if ((checkPosition - eyePos).sqrMagnitude > range * range) return false;

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



    bool CalcBallisticArc(Vector3 origin, Vector3 target, float velocity, out Vector3 vLow, out Vector3 vHigh)
    {
        vLow = vHigh = Vector3.zero;
        Vector3 direction = target - origin;
        Vector3 flatDirection = new Vector3(direction.x, 0f, direction.z);

        float x = flatDirection.magnitude;
        float y = direction.y;
        float g = Mathf.Abs(Physics.gravity.y);

        float velocity2 = velocity * velocity;
        float velocity4 = velocity2 * velocity2;

        float discriminant = velocity4 - g * (g * x * x + 2f * y * velocity2);
        if (discriminant < 0f) return false;

        float root = Mathf.Sqrt(discriminant);

        float tLow = (velocity2 - root) / (g * x);
        float tHigh = (velocity2 + root) / (g * x);

        float thetaLow = Mathf.Atan(tLow);
        float thetaHigh = Mathf.Atan(tHigh);

        Vector3 dirXZ = (x > 0.0001f) ? (flatDirection / x) : transform.forward;

        vLow = dirXZ * (velocity * Mathf.Cos(thetaLow)) + Vector3.up * (velocity * Mathf.Sin(thetaLow));
        vHigh = dirXZ * (velocity * Mathf.Cos(thetaHigh)) + Vector3.up * (velocity * Mathf.Sin(thetaHigh));

        return true;
    }

    bool ArcLOS(Vector3 origin, Vector3 target, Vector3 velocity, float radius, int samples, int mask, Transform targetRoot)
    {
        Vector3 g = Physics.gravity;

        Vector3 flatVelocity = new Vector3(velocity.x, 0f, velocity.z);
        Vector3 flatDirection = new Vector3((target  - origin).x, 0f, (target - origin).z);
        float flatSpeed = flatVelocity.magnitude;
        float flatDistance = flatDirection.magnitude;
        if (flatSpeed < 0.001f) return false;

        float flightTime = flatDistance / flatSpeed;

        Vector3 previousPoint = origin;

        for (int i = 1; i <= Mathf.Max(2,samples); i++)
        {
            float t = flightTime * (i / (float)samples);
            Vector3 point = origin + velocity * t + 0.5f * g * (t * t);
            Vector3 segment = point - previousPoint;
            float length = segment.magnitude;
            if (length > 0.0001f)
            {
                if (Physics.SphereCast(previousPoint, radius, segment / length, out RaycastHit hit, length, mask, QueryTriggerInteraction.Ignore)) {
                    if (hit.transform.root != targetRoot) return false;
                }
            }
            previousPoint = point;
        }
        return true;
    }
}
