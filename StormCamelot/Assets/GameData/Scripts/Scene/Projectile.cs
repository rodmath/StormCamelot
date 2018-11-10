using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Chronos;



[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Timeline))]
public class Projectile : MonoBehaviour
{
    public Vector3 carriedAngle = Vector3.zero;
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
            //note we need to deal with null values
            if (_owner)
                IgnoreCollisionsWithOwner(false);

            _owner = value;
            if (value)
                IgnoreCollisionsWithOwner(true);
        }
    }



    void Start()
    {
        time = GetComponent<Timeline>();
        state = ProjectileState.inFlight;
    }

    private void IgnoreCollisionsWithOwner(bool ignore)
    {
        foreach (Collider thisCol in GetComponentsInChildren<Collider>())
            foreach (Collider otherCol in Owner.GetComponentsInChildren<Collider>())
                Physics.IgnoreCollision(thisCol, otherCol, ignore);
    }


    public void ChronosPickUp(GameObject newOwner, Transform holdPoint, Vector3 gripOffset)
    {
        time.Do(false,
                delegate () { return DoPickup(newOwner, holdPoint, gripOffset);},
                delegate (Transform oldParent) { UndoPickup(oldParent); }
               );
    }


    public Transform DoPickup(GameObject newOwner, Transform holdPoint, Vector3 gripOffset)
    {
        state = ProjectileState.carried;

        //if we are stuck to something, lets "unstick"
        if (Owner)
        {
            FixedJoint joint = Owner.GetComponent<FixedJoint>();
            if (joint) Destroy(joint);
        }

        time.rigidbody.isKinematic = true;
        transform.position = holdPoint.position + gripOffset;
        transform.forward = holdPoint.forward;
        transform.Rotate(carriedAngle, Space.Self);

        Transform oldParent = transform.parent;
        transform.SetParent(holdPoint);
        return oldParent;
    }

    public void UndoPickup(Transform oldParent)
    {
        state = ProjectileState.impaled;

        transform.SetParent(oldParent);
    }

    public void TimeSetParent(Transform newParent)
    {
        time.Do
        (
            true,
            delegate ()
            {
                Transform oldParent = transform.parent;
                transform.SetParent(newParent);
                return oldParent;
            },
            delegate (Transform oldParent)
            {
                transform.SetParent(oldParent);
            }
        );
    }


    public void ChronosLaunch(float force, GameObject pOwner)
    {
        time.Do(false,
                delegate () { return DoLaunch(force, pOwner); },
                delegate (Transform oldParent) { UndoLaunch(oldParent); }
               );
    }


    public Transform DoLaunch(float force, GameObject pOwner)
    {
        state = ProjectileState.inFlight;
        Owner = pOwner;

        lastPosition = transform.position;

        time.rigidbody.isKinematic = false;
        time.rigidbody.velocity = transform.forward * force;

        Transform oldParent = transform.parent;
        transform.SetParent(null);
        return oldParent;
    }

    public void UndoLaunch(Transform oldParent)
    {
        state = ProjectileState.carried;
        Owner = oldParent.gameObject;

        time.rigidbody.isKinematic = true;
        transform.SetParent(oldParent);
    }



    void FixedUpdate()
    {
        if (state == ProjectileState.inFlight)
        {
            Transform t = transform;
            if (transformToSpin)
                t = transformToSpin;

            if (spinInFlight.magnitude.RoughlyEquals(0f))
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
            TimeSetParent(deadObj.transform);
            Owner = deadObj;


            Timeline deadObjTime = deadObj.GetComponent<Timeline>();
            if (deadObjTime)
            {
                deadObj.AddComponent<FixedJoint>().connectedBody = time.rigidbody.component;
                //deadObjTime.rigidbody.AddForce(time.rigidbody.velocity * 10f);
            }


        }
        //if can't kill, just impale 
        else
        {
            time.Do(false,
                    delegate () { return DoImpale(other.transform.root.transform); },
                    delegate (Transform oldParent) { UndoImpale(oldParent); }
            );
        }
    }


    private Transform DoImpale(Transform newParent)
    {
        state = ProjectileState.impaled;

        time.rigidbody.isKinematic = true;
        Owner = newParent.root.gameObject;

        Transform oldParent = transform.parent;
        transform.SetParent(newParent.transform);

        return oldParent;
    }

    private void UndoImpale(Transform oldParent)
    {
        state = ProjectileState.inFlight;

        time.rigidbody.isKinematic = false;
        Owner = null;
        if (oldParent)
            Owner = oldParent.root.gameObject;

        transform.SetParent(oldParent);
    }



    private Transform DoKill(Life life)
    {
        GameObject deadObj = life.Kill();
        Transform oldParent = transform.parent;
        transform.SetParent(deadObj.transform);
        Owner = deadObj;

        Timeline deadObjTime = deadObj.GetComponent<Timeline>();
        if (deadObjTime)
        {
            deadObj.AddComponent<FixedJoint>().connectedBody = time.rigidbody.component;
        }

        return oldParent;
    }

}
