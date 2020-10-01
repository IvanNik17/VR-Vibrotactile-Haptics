using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PenPop : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Rigidbody rBody = gameObject.GetComponent(typeof(Rigidbody)) as Rigidbody;
        rBody.maxDepenetrationVelocity = 1;

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnCollisionEnter(Collision collision)
    {
        foreach (ContactPoint contact in collision.contacts)
        {
            Debug.DrawRay(contact.point, contact.normal, Color.white);
        }

        //Debug.DrawRay(collision.contacts[0].point, collision.contacts[0].normal, Color.white,5);
        //Debug.Log("asdogh");
    }
}
