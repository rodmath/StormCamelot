using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

[RequireComponent(typeof(Rigidbody))]
public class DeathRoutine : MonoBehaviour {

	void Start () {

        Rigidbody rb = GetComponent<Rigidbody>();
        rb.WakeUp();
        rb.isKinematic = false;

        Vector3 deathForce = Random.onUnitSphere;
        deathForce.y = 0f;
        rb.AddForce(deathForce * 5f);
	}

}
