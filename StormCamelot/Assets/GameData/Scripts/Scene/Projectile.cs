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

    [SerializeField]
    private GameObject _owner;
    public GameObject Owner
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

    private FixedJoint stickTo;


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
                delegate () { return DoPickup(newOwner, holdPoint, gripOffset); },
                delegate (Rigidbody oldImpaledRb) { UndoPickup(oldImpaledRb); }
               );
    }


    public Rigidbody DoPickup(GameObject newOwner, Transform holdPoint, Vector3 gripOffset)
    {
        state = ProjectileState.carried;

        transform.position = holdPoint.position + gripOffset;
        transform.forward = holdPoint.forward;
        transform.Rotate(carriedAngle, Space.Self);
        Owner = newOwner;

        //if we are stuck to something, lets "unstick"
        if (stickTo)
        {
            Rigidbody oldImpaledRb = stickTo.connectedBody;
            stickTo.connectedBody = newOwner.GetComponent<Rigidbody>();
            return oldImpaledRb;
        }
        return null;
    }

    public void UndoPickup(Rigidbody oldImpaledRb)
    {
        state = ProjectileState.impaled;

        stickTo.connectedBody = oldImpaledRb;

        Owner = oldImpaledRb.transform.root.gameObject;
    }



    public void ChronosLaunch(float force)
    {
        time.Do(false,
                delegate () { DoLaunch(force); },
                delegate () { UndoLaunch(); }
               );
    }


    public void DoLaunch(float force)
    {
        state = ProjectileState.inFlight;

        lastPosition = transform.position;

        Destroy(stickTo);

        time.rigidbody.velocity = transform.forward * force;
    }

    public void UndoLaunch()
    {
        state = ProjectileState.carried;

        stickTo = gameObject.AddComponent<FixedJoint>();
        stickTo.connectedBody =  Owner.GetComponent<Rigidbody>();

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
        Timeline otherTime = other.GetComponentInParent<Timeline>();

        if (life)
        {
            time.Do(false,
                    delegate () { life.Dead(true); },
                    delegate () { life.Dead(false); }
            );
        }
        if (otherTime)
        {
            //otherTime.rigidbody.AddForce(transform.forward, ForceMode.Impulse);
            //otherTime.rigidbody.AddForceAtPosition(transform.forward, col.contacts[0].point, ForceMode.Impulse);
        }

        time.Do(false,
                delegate () { return DoImpale2(other.gameObject); },
                delegate (GameObject oldOwner) { UndoImpale2(oldOwner); }
        );


    }

    private GameObject DoImpale2(GameObject impaledObj)
    {
        state = ProjectileState.impaled;

        //impaledObj.AddComponent<FixedJoint>().connectedBody = time.rigidbody.component;
        stickTo = gameObject.AddComponent<FixedJoint>();

        Rigidbody rb = impaledObj.GetComponentInParent<Rigidbody>();
        if (rb)
            stickTo.connectedBody = rb;

        GameObject oldOwner = Owner;
        Owner = impaledObj.transform.root.gameObject;

        return oldOwner;
    }

    private void UndoImpale2(GameObject oldOwner)
    {
        state = ProjectileState.inFlight;

        if (Owner)
            if (stickTo) Destroy(stickTo);

        if (oldOwner)
            Owner = oldOwner;
    }
}
