using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Projectile : MonoBehaviour {

    public bool held;
    public Transform owner;

    private Rigidbody rb;


	void Start () {
        rb = GetComponent<Rigidbody>();
        owner = transform;
	}
	

    public void Launch(float force, Transform pOwner)
    {
        rb.isKinematic = false;
        rb.AddForce(transform.forward * force, ForceMode.Impulse);

        owner = pOwner;
        IgnoreCollisions(true);

    }

    private void IgnoreCollisions(bool ignore)
    {
        Collider thisCol = GetComponent<Collider>();

        foreach (Collider otherCol in owner.GetComponentsInChildren<Collider>())
            Physics.IgnoreCollision(thisCol, otherCol, ignore);
    }

	
	void LateUpdate () {
        if (!held)
            transform.forward = Vector3.Slerp(transform.forward, rb.velocity.normalized, 0.1f);
        //transform.forward = rb.velocity.normalized;

	}


    private void OnTriggerEnter(Collider other)
    {
        if (!held)
        {
            transform.SetParent(other.transform);
            rb.velocity = Vector3.zero;
            rb.isKinematic = true;
            held = true;

            IgnoreCollisions(false);
            owner = transform;
        }
    }



}
