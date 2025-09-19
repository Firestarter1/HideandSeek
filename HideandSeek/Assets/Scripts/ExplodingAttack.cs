using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class ExplodingAttack : MonoBehaviour
{
    [Header("Attack")]
    [SerializeField] int damage = 1;
    [SerializeField] float range = 2.5f;
    [SerializeField] float radius = 5.0f;
    [SerializeField] float cooldown = 1.0f;
    [SerializeField] ParticleSystem explodeParticles;
    [SerializeField] MeshRenderer radiusPlane;
    [SerializeField] float radiusPlaneY;

    [Header("Targeting")]
    [SerializeField] Transform eyePosition;
    [SerializeField, Range(0f, 360f)] float fov = 90f;
    [SerializeField] LayerMask targetLayerMask;
    [SerializeField] LayerMask obstructionLayerMask;

   

    Animator animator;
    NavMeshAgent navigationAgent;

    bool detonated = false;
    private void Start()
    {
        if (!TryGetComponent<NavMeshAgent>(out navigationAgent))
        {
            Destroy(this);
            return;
        }
        animator = GetComponent<Animator>();
        if (!eyePosition) eyePosition = transform;
        radiusPlaneY = radiusPlane.transform.position.y;
    }

    private void Update()
    {
        if (radiusPlane.gameObject.activeInHierarchy)
        {
            radiusPlane.transform.position = new Vector3(radiusPlane.transform.position.x, radiusPlaneY, radiusPlane.transform.position.z);
            radiusPlane.material.SetColor("_TintColor", Color.Lerp(radiusPlane.material.GetColor("_TintColor"), new Color(radiusPlane.material.GetColor("_TintColor").r, radiusPlane.material.GetColor("_TintColor").g, radiusPlane.material.GetColor("_TintColor").b, 1f), 0.25f * Time.deltaTime));
        }

        //if (navigationAgent.isStopped) return;
        if (!ReachedTarget()) return;

        GameObject target = GameManager.Instance.player;

        if (TargetInRange(target.transform) && !detonated)
        {
            animator?.SetTrigger("Attack");
            detonated = true;
            EnableRadiusPlane();
            /*Vector3 look = target.transform.position - transform.position;
            look.y = 0;
            if (look.magnitude > 0.0001f)
            {
                transform.rotation = Quaternion.LookRotation(look);
            }*/
        }

        
    }

    void EnableRadiusPlane()
    {
        radiusPlane.gameObject.transform.localScale = new Vector3(radius / 5f, 0.1f, radius / 5f);
        radiusPlane.gameObject.SetActive(true);
    }

    void Attack()
    {
        Vector3 origin = transform.position;
        Vector3 particlePosition = origin + Vector3.up * 0.33f;
        ParticleSystem p = Instantiate(explodeParticles, particlePosition, explodeParticles.transform.rotation);
        p.Play();

        radiusPlane.gameObject.SetActive(false);

        Vector3 closePoint = GameManager.Instance.player.GetComponent<Collider>().ClosestPoint(transform.position);
        float distance = Vector3.Distance(closePoint, transform.position);
        
        if (distance <= radius)
        {
            if (HasAnyLineOfSight(origin, GameManager.Instance.player.GetComponent<Collider>()))
            {
                float proximity = Mathf.Clamp01(distance / radius);
                int dealt = Mathf.CeilToInt(Mathf.Lerp(damage, 1f, proximity));
                GameManager.Instance.playerScript.takeDamage(dealt);
            }
        }
        Destroy(gameObject);
    }

    bool HasAnyLineOfSight(Vector3 origin, Collider targetCol)
    {
        Vector3 center = targetCol.bounds.center;
        Vector3 top = center + Vector3.up * targetCol.bounds.extents.y;
        Vector3 low = center - Vector3.up * (targetCol.bounds.extents.y * 0.5f);

        return HasLOS(origin, targetCol.transform, targetCol.ClosestPoint(origin)) || HasLOS(origin, targetCol.transform, center) || HasLOS(origin, targetCol.transform, top) || HasLOS(origin, targetCol.transform, low);
    }

    bool HasLOS(Vector3 origin, Transform target, Vector3 samplePoint)
    {
        Vector3 dir = samplePoint - origin;
        float distance = dir.magnitude;
        if (distance <= 0.0001f) return true; 

        dir /= distance;

        int mask = targetLayerMask | obstructionLayerMask;

        Vector3 start = origin + dir * 0.05f;

        if (Physics.Raycast(start, dir, out RaycastHit hit, distance, mask, QueryTriggerInteraction.Ignore))
        {
            return hit.transform.root == target.root;
        }

        return true;
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


#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        Gizmos.color = Color.orange;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
#endif
}
