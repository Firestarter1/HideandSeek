using UnityEngine;
using  System.Collections;
using System.Collections.Generic;

public class AimScript : MonoBehaviour
{

    public GameObject Gun;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButton(1))
        {
            Gun.GetComponent<Animator>().Play("Aim");
        }
        if (Input.GetMouseButtonUp(1))
        {
            Gun.GetComponent<Animator>().Play("New State");
        }
    }
}
