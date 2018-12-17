using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(Rigidbody))]
public class RigidbodyFreeze : MonoBehaviour
{
    private FreezeMaster master;

    private Vector3 cacheVelocity;
    private Vector3 cacheAngularVelocity;
    private Rigidbody body;
    private NavMeshAgent navAgent;

    private bool canBeNonKinetic;
    private bool frozen;

    public bool Frozen { get { return frozen; } }



    private void OnEnable()
    {
        if (master == null)
            master = Object.FindObjectOfType<FreezeMaster>();

        master.RegisterMe(this);
    }


    private void OnDisable()
    {
        master.UnregisterMe(this);
    }

    private void OnDestroy()
    {
        master.UnregisterMe(this);
    }



    private void Start()
    {
        body = GetComponent<Rigidbody>();
        navAgent = GetComponent<NavMeshAgent>();

        canBeNonKinetic = !body.isKinematic;
    }


    public bool Freeze
    {
        set
        {
            //only make a change if the setting is different to what we already have
            if (value == frozen)
                return;

            //freeze it
            if (value)
            {
                cacheVelocity = body.velocity;
                cacheAngularVelocity = body.angularVelocity;

                if (canBeNonKinetic)
                    body.isKinematic = true;

                if (navAgent)
                    navAgent.isStopped = true; 

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

                if (navAgent)
                    navAgent.isStopped = false;

                frozen = false;
            }
        }
    }
}
    

