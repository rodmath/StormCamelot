using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Chronos;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Timeline))]
public class Projectile : MonoBehaviour
{
    public Collider impalePoint;


    public bool held;
    public bool inFlight;

    private GameObject owner;
    private Timeline time;

    private Vector3 lastPosition;
    private float speed;
    private float rot;
    private float spin;
    private float impaleDepth;

    void Start()
    {
        time = GetComponent<Timeline>();
        owner = gameObject;
    }


    public void Launch(float force, GameObject pOwner)
    {
        ActivateTriggers(false);    //make physical

        lastPosition = transform.position;
        rot = Random.value * 360;
        spin = Random.Range(30, 80);

        transform.SetParent(null);

        time.rigidbody.isKinematic = false;
        //time.rigidbody.AddForce(transform.forward * force, ForceMode.Impulse);
        time.rigidbody.velocity = transform.forward * force;
        speed = force;

        inFlight = true;

        owner = pOwner;
        IgnoreCollisions(true);
        held = false;
    }

    public void CanCollide(bool collidersEnabled)
    {
        foreach(Collider col in GetComponents<Collider>())
        {
            col.enabled = collidersEnabled;
        }
    }


    private void IgnoreCollisions(bool ignore)
    {
        foreach (Collider thisCol in GetComponentsInChildren<Collider>())
            foreach (Collider otherCol in owner.GetComponentsInChildren<Collider>())
                Physics.IgnoreCollision(thisCol, otherCol, ignore);
    }

    private void ActivateTriggers(bool triggerTrue)
    {
        foreach (Collider thisCol in GetComponentsInChildren<Collider>())
            thisCol.isTrigger = triggerTrue;
    }

    void FixedUpdate()
    {
        if (!held)
        {
            transform.forward = Vector3.Slerp(transform.forward, time.rigidbody.velocity.normalized, time.fixedDeltaTime * 5f);

            //RaycastHit hit;
            //if (Physics.Linecast(lastPosition, transform.position, out hit))
            //{
            //    transform.position = hit.point + time.rigidbody.velocity.normalized * impaleDepth;
            //    time.rigidbody.isKinematic = true;
            //    transform.parent = hit.collider.transform.parent;
            //    held = true;
            //}
            //else
            //{
            //    if (!time.rigidbody.isKinematic)
            //    {
            //        transform.LookAt(transform.position + time.rigidbody.velocity);
            //        transform.Rotate(0, 0, rot);
            //        rot += spin * time.deltaTime;
            //    }
            //}
            //lastPosition = transform.position;
        }

    }


    private void OnCollisionEnter(Collision col)
    {
        Collider other = col.contacts[0].otherCollider;
        Agent agent = other.GetComponentInParent<Agent>();

        //if we are not held, we are hitting something
        if (!held)
        {
            GetComponent<Rigidbody>().MovePosition(col.contacts[0].point);
            IgnoreCollisions(false);


            Life life = other.GetComponentInParent<Life>();
            //have we hit something we can kill
            if (life)
            {
                //kill obj
                GameObject deadObj = life.Kill();
                transform.SetParent(deadObj.transform);

                //apply force in direction of our velocity to dead obj
                Vector3 finalBlowForce = time.rigidbody.velocity;
        
                Timeline deadObjTime = deadObj.GetComponent<Timeline>();
                if (deadObjTime)
                {
                    deadObj.AddComponent<FixedJoint>().connectedBody = time.rigidbody.component;
                    deadObjTime.rigidbody.AddForce(time.rigidbody.velocity);
                    ActivateTriggers(false);    //make physical
                }
               

            }
            //if can't kill, just impale and wait as triggers
            else
            {
                transform.SetParent(other.transform.parent);
                time.rigidbody.isKinematic = true;
                ActivateTriggers(true);    //make physical
            }

            time.rigidbody.velocity = Vector3.zero;
            time.rigidbody.angularVelocity = Vector3.zero; 

            held = true;
            owner = gameObject;
        }
    }

    //private void OnTriggerEnter(Collider other)
    //{
    //    Debug.Log(name + " had trigger entered by " + other.name);

    //    SoldierAgent agent = other.GetComponentInParent<SoldierAgent>();
    //    if (agent && agent.gameObject == owner)
    //    {
    //        return;
    //    }

    //    //if we are not held, we are hitting something
    //    if (!held)
    //    {
    //        IgnoreCollisions(false);

    //        Life life = other.GetComponentInParent<Life>();
    //        if (life)
    //        {
    //            transform.SetParent(life.Kill().transform);
    //        }
    //        else
    //        {
    //            transform.SetParent(other.transform);
    //        }

    //        time.rigidbody.velocity = Vector3.zero;
    //        time.rigidbody.isKinematic = true;
    //        held = true;
    //        owner = gameObject;
    //    }
    //    else if (agent)
    //    {
    //        agent.GripProjectile(this);
    //    }
    //}



    //private void OnCollisionEnter(Collision col)
    //{
    //    Collider other = col.contacts[0].otherCollider;
    //    SoldierAgent agent = other.GetComponentInParent<SoldierAgent>();

    //    //if we are not held, we are hitting something
    //    if (!held)
    //    {
    //        GetComponent<Rigidbody>().MovePosition(col.contacts[0].point);
    //        IgnoreCollisions(false);

    //        Life life = other.GetComponentInParent<Life>();
    //        if (life)
    //        {
    //            transform.SetParent(life.Kill().transform);
    //        }
    //        else
    //        {
    //            transform.SetParent(other.transform.parent);
    //        }

    //        time.rigidbody.velocity = Vector3.zero;
    //        time.rigidbody.angularVelocity = Vector3.zero; 
    //        time.rigidbody.isKinematic = true;
    //        held = true;
    //        owner = gameObject;
    //    }
    //    else if (agent)
    //    {
    //        agent.GripProjectile(this);
    //    }
    //}



}
