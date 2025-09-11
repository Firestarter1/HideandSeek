using UnityEngine;
using System.Collections;

public class Grenades : MonoBehaviour
{
    [SerializeField] Rigidbody rb;

    [SerializeField] int speed;
    [SerializeField] int upwardSpeed;
    [SerializeField] int destroyTime;


    [SerializeField] GameObject explosion;

    void Start()
    {
        rb.linearVelocity = (transform.forward * speed) + (transform.up * upwardSpeed);
        StartCoroutine(explode());
    }

    IEnumerator explode()
    {
        yield return new WaitForSeconds(destroyTime);
        Instantiate(explosion, transform.position, Quaternion.identity);
        Destroy(gameObject);
    }

}