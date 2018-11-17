using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FixedFacing : MonoBehaviour {

    public Vector3 worldDirectionToLook;
	
	// Update is called once per frame
	void Update () {
        transform.forward = worldDirectionToLook;
	}
}
