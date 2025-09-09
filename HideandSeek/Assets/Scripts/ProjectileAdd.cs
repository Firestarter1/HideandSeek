using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ProjectileAdd : MonoBehaviour
{

    public int damage;

    private Rigidbody rb;

    private bool targetHit;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        //make it only stick to the first object
        if (targetHit)
            return;
        else
            targetHit = true;    

        //make sure it sticls to the surface
        rb.isKinematic = true;

        //make it move with target
        transform.SetParent(collision.transform);
    }
}
