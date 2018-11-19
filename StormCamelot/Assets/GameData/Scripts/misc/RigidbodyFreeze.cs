using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class RigidbodyFreeze : MonoBehaviour
{

    private Vector3 cacheVelocity;
    private Vector3 cacheAngularVelocity;
    private Rigidbody body;

    private bool canBeNonKinetic;
    private bool frozen;

    public bool Frozen { get { return frozen; } }

    private void Start()
    {
        body = GetComponent<Rigidbody>();
        canBeNonKinetic = !body.isKinematic;
    }


    public bool Freeze
    {
        set
        {
            //freeze it
            if (value)
            {
                cacheVelocity = body.velocity;
                cacheAngularVelocity = body.angularVelocity;

                if (canBeNonKinetic)
                    body.isKinematic = true;

                frozen = true;
            }
            //unfreeze it
            else
            {
                if (canBeNonKinetic)
                    body.isKinematic = false;

                body.velocity = cacheVelocity;
                body.angularVelocity = cacheAngularVelocity;
                body.WakeUp();
                frozen = false;
            }
        }
    }
}
    

