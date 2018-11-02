using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Chronos;



[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Timeline))]
public class Projectile : MonoBehaviour
{
    public Vector3 carriedAngle=Vector3.zero;
    public Vector3 spinInFlight = Vector3.zero;
    public Transform transformToSpin;

    private enum ProjectileState
    {
        inFlight,
        carried,
        impaled,
    }

    //if we are impaled into something or held by someone the following is true
    private ProjectileState state = ProjectileState.inFlight;
    public bool CanBePickedUp { get { return state == ProjectileState.impaled; } }

    //public bool Impaled { get { return state == ProjectileState.impaled; } }
    //public bool Held { get { return state == ProjectileState.held; } }



    private Timeline time;

    private Vector3 lastPosition;
    private float speed;


    private GameObject _owner;
    private GameObject Owner
    {
        get { return _owner; }
        set
        {
            if (_owner)
                IgnoreCollisionsWithOwner(false);

            _owner = value;
            IgnoreCollisionsWithOwner(true);
        }
    }



    void Start()
    {
        time = GetComponent<Timeline>();
    }

    private void IgnoreCollisionsWithOwner(bool ignore)
    {
        foreach (Collider thisCol in GetComponentsInChildren<Collider>())
            foreach (Collider otherCol in Owner.GetComponentsInChildren<Collider>())
                Physics.IgnoreCollision(thisCol, otherCol, ignore);
    }





    public void Pickedup(GameObject newOwner, Transform holdPoint, Vector3 gripOffset)
    {
        state = ProjectileState.carried;

        //if we are stuck to something, lets "unstick"
        FixedJoint joint = Owner.GetComponent<FixedJoint>();
        if (joint) Destroy(joint);

        time.rigidbody.isKinematic = true;
        transform.position = holdPoint.position + gripOffset;
        transform.forward = holdPoint.forward;
        transform.Rotate(carriedAngle, Space.Self);

        transform.SetParent(holdPoint);
    }



    public void Launch(float force, GameObject pOwner)
    {
        state = ProjectileState.inFlight;
        Owner = pOwner;

        lastPosition = transform.position;

        transform.SetParent(null);

        time.rigidbody.isKinematic = false;
        time.rigidbody.velocity = transform.forward * force;
    }






    void FixedUpdate()
    {
        if (state == ProjectileState.inFlight)
        {
            Transform t = transform;
            if (transformToSpin)
                t = transformToSpin;

            if (spinInFlight.magnitude == 0f)
                t.forward = Vector3.Slerp(transform.forward, time.rigidbody.velocity.normalized, time.fixedDeltaTime * 5f);
            else
                t.Rotate(spinInFlight * time.fixedDeltaTime, Space.Self);

            lastPosition = transform.position;

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
        if (state != ProjectileState.inFlight)
            return;

        state = ProjectileState.impaled;

        //Fix in position
        Rigidbody body = GetComponent<Rigidbody>();
        body.MovePosition(col.contacts[0].point);
        time.rigidbody.velocity = Vector3.zero;
        time.rigidbody.angularVelocity = Vector3.zero;

        //Find out what we hit and fix to it or it's dead version
        Collider other = col.contacts[0].otherCollider;
        Life life = other.GetComponentInParent<Life>();
        if (life)
        {
            GameObject deadObj = life.Kill();
            transform.SetParent(deadObj.transform);
            Owner = deadObj;


            Timeline deadObjTime = deadObj.GetComponent<Timeline>();
            if (deadObjTime)
            {
                deadObj.AddComponent<FixedJoint>().connectedBody = time.rigidbody.component;
                deadObjTime.rigidbody.AddForce(time.rigidbody.velocity * 10f);
            }


        }
        //if can't kill, just impale 
        else
        {
            transform.SetParent(other.transform.parent);
            time.rigidbody.isKinematic = true;  //stick it in position
            Owner = other.transform.root.gameObject;
        }
    }
}
