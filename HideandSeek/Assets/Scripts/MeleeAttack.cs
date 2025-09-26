using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.AI;

public class MeleeAttack : MonoBehaviour
{
    [Header("Attack")]
    [SerializeField] int damage = 1;
    [SerializeField] float range = 2.0f;
    [SerializeField] float cooldown = 1.0f;
    [SerializeField] ScreenShakeSettings hitScreenShake;
    [Header("Targeting")]
    [SerializeField] Transform eyePosition;
    [SerializeField, Range(0f, 360f)] float fov = 90f;
    [SerializeField] LayerMask targetLayerMask;
    [SerializeField] LayerMask obstructionLayerMask;

    Animator animator;
    NavMeshAgent navigationAgent;
    float cooldownTimer = 0.0f;

    private void Start()
    {
        if (!TryGetComponent<NavMeshAgent>(out navigationAgent))
        {
            Destroy(this);
            return;
        }
        animator = GetComponent<Animator>();
        if (!eyePosition) eyePosition = transform;
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
            animator?.SetTrigger("Attack");

            /*Vector3 look = target.transform.position - transform.position;
            look.y = 0;
            if (look.magnitude > 0.0001f)
            {
                transform.rotation = Quaternion.LookRotation(look);
            }*/
        }
    }

    public void Attack()
    {
        GameObject target = GameManager.Instance.player;
        if (TargetInRange(target.transform))
        {
            cooldownTimer = 0f;

            if (target.TryGetComponent<IDamage>(out IDamage dmg))
            {
                dmg.takeDamage(damage);
                CameraShakeManager.Instance.ScreenShakeFromSettings(hitScreenShake, GetComponent<CinemachineImpulseSource>());
            }

            
        }
    }
     
    bool ReachedTarget()
    {
        if (!navigationAgent.isOnNavMesh) return false;
        if (navigationAgent.pathPending) return false;
        
        float distance = Vector3.Distance(navigationAgent.transform.position, GameManager.Instance.player.GetComponent<Collider>().ClosestPoint(navigationAgent.transform.position));
        bool inStoppingDistance = distance <= navigationAgent.stoppingDistance + 0.1f;


        return inStoppingDistance;
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

}
