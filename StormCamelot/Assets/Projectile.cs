using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Projectile : MonoBehaviour {

    public float force = 5;

    private Rigidbody rb;


	void Start () {
        rb = GetComponent<Rigidbody>();

        rb.AddForce(transform.forward * force, ForceMode.Impulse);
	}
	
	// Update is called once per frame
	void Update () {
        transform.forward = rb.velocity.normalized;
	}
}
