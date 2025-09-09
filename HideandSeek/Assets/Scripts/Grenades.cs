using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class Grenades : MonoBehaviour
{
    [Header("Stats")]
    public int damage;
    public bool destoryOnHit;

    [Header("Effects")]
    public GameObject hitEffect;

    [Header("Explosion")]
    public bool isExplosive;
    public float explosionRadius;
    public float explosionForce;
    public int explosionDamage;
    public GameObject explosionEffect;

    private Rigidbody rb;
    private bool hitTarget;

    private void Start()
    {
        //Get rigidbody component
        rb = GetComponent<Rigidbody>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (hitTarget)
            return;
        else
            hitTarget = true;

        //enemy hit
        if(collision.gameObject.GetComponent<EnemeyAI>() != null)
        {
            EnemeyAI enemy = collision.gameObject.GetComponent<EnemeyAI>();

            //deal damage
            enemy.takeDamage(damage);

            //spawn hit effect is used
            if (hitEffect != null)
                Instantiate(hitEffect, transform.position, Quaternion.identity);

            //destroy projectile
            if (!isExplosive && destoryOnHit)
                Invoke(nameof(DestroyProjectile), 0.1f);
        }

        //explode projectile if explosive
        if (isExplosive)
        {
            Explode();
            return;
        }

        //make sure it sticks to the surface
        rb.isKinematic = true;

        //make sure it moves with target
        transform.SetParent(collision.transform);
    }

    private void Explode()
    {
        //spawn effect if used
        if (explosionEffect != null)
            Instantiate(explosionEffect, transform.position, Quaternion.identity);

        //find all objects in explosion range
        Collider[] objectsInRange = Physics.OverlapSphere(transform.position, explosionRadius);

        //loop through and apply damage and force
        for (int i = 0; i < objectsInRange.Length; i++)
        {
            if (objectsInRange[i].gameObject == gameObject)
            {

            }
            else
            {
                //check if enemy if so deal damage
                if (objectsInRange[i].GetComponent<EnemeyAI>() != null)
                    objectsInRange[i].GetComponent<EnemeyAI>().takeDamage(explosionDamage);

                //check if object has rigidbody
                if (objectsInRange[i].GetComponent<Rigidbody>() != null)
                {
                    //custom force
                    Vector3 objectPos = objectsInRange[i].transform.position;

                    //calculate force direction
                    Vector3 forceDirection = (objectPos - transform.position).normalized;

                    //apply force to object in range
                    objectsInRange[i].GetComponent<Rigidbody>().AddForceAtPosition(forceDirection * explosionForce + Vector3.up * explosionForce, transform.position + new Vector3(0, -0.5f, 0), ForceMode.Impulse);

                    Debug.Log("Kaboom " + objectsInRange[i].name);
                }
            }
        }

        //destory projectile with 0,1 dely
        Invoke(nameof(DestroyProjectile), 0.1f);
    }

    private void DestroyProjectile()
    {
        Destroy(gameObject);
    }

    //just graphic stuff
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}
