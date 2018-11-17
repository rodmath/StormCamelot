using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Life : MonoBehaviour {

    public bool alive = true;

    private GameObject deadObject;

    public void Dead()
    {
        Debug.Log(name + " was killed");

        //ragdollify
        Rigidbody body = GetComponent<Rigidbody>();
        if (body && body.isKinematic == true)
            body.isKinematic = false;

        alive = false;
    }


}
