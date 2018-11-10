using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Chronos;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Time))]
public class DeathRoutine : MonoBehaviour {

    public float force = 5f;

	void Start () {

        Timeline time = GetComponent<Timeline>();
        //time.rigidbody.WakeUp();
        time.rigidbody.isKinematic = false;

        Vector3 deathForce = Random.onUnitSphere * force;
        deathForce.y = 0f;
        time.rigidbody.AddForce(deathForce * force);
	}

}
