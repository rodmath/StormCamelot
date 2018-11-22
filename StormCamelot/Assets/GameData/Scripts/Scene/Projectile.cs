using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;


[RequireComponent(typeof(Rigidbody))]
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
        deflected,
        atRest
    }

    [SerializeField]
    private ProjectileState state = ProjectileState.inFlight;

    public bool CanBePickedUp { get { return state == ProjectileState.impaled || state == ProjectileState.atRest; } }

    private Rigidbody projectileBody;
    private Vector3 lastPosition;
    private Vector3 lastVel;
    private Vector3 lastAngVel;
    private float speed;
    private float restTimer = 0.5f;

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
    private TrailRenderer trail;

    void Start()
    {
        projectileBody = GetComponent<Rigidbody>();
        projectileBody.centerOfMass = new Vector3(0f, 0f, 0.1f);

        trail = GetComponentInChildren<TrailRenderer>();
        state = ProjectileState.inFlight;
        restTimer = 0.5f;
    }


    private void IgnoreCollisions(GameObject obj, bool ignore)
    {
        foreach (Collider thisCol in GetComponentsInChildren<Collider>())
            foreach (Collider otherCol in obj.GetComponentsInChildren<Collider>())
                Physics.IgnoreCollision(thisCol, otherCol, ignore);
    }


    private void IgnoreCollisionsWithOwner(bool ignore)
    {
        IgnoreCollisions(Owner, ignore);
    }


    private void SetCollidersAsTriggers(bool asTriggers)
    {
        foreach (Collider c in GetComponentsInChildren<Collider>())
            c.isTrigger = asTriggers;
    }


    public void PickUp(GameObject newOwner, Transform holdPoint, Vector3 gripOffset)
    {
        state = ProjectileState.carried;

        transform.position = holdPoint.position + gripOffset;
        transform.forward = holdPoint.forward;
        transform.Rotate(carriedAngle, Space.Self);
        Owner = newOwner;

        //stick ourselves to our new owner
        if (!stickTo)
            stickTo = gameObject.AddComponent<FixedJoint>();

        stickTo.connectedBody = newOwner.GetComponent<Rigidbody>();
    }


    public void Launch(float force)
    {
        state = ProjectileState.inFlight;
        restTimer = 0.5f;

        lastPosition = transform.position;

        Destroy(stickTo);

        projectileBody.velocity = transform.forward * force;
        trail.enabled = true;
    }



    void FixedUpdate()
    {
        if (state == ProjectileState.inFlight)
        {

            Transform t = transform;
            if (transformToSpin)
                t = transformToSpin;

            if (spinInFlight.magnitude.RoughlyEquals(0f))
                t.forward = Vector3.Slerp(transform.forward, projectileBody.velocity.normalized, Time.fixedDeltaTime * 5f);
            else
                t.Rotate(spinInFlight * Time.fixedDeltaTime, Space.Self);

        }

        if (state == ProjectileState.deflected)
        {
            if (projectileBody.velocity.magnitude.RoughlyEquals(0f))
                restTimer -= Time.fixedDeltaTime;

            if (restTimer <= 0f)
                state = ProjectileState.atRest;
        }

        if (state == ProjectileState.impaled)
        {
            if (!stickTo.gameObject.activeSelf)
                Destroy(stickTo);
        }


        lastPosition = transform.position;
        lastVel = projectileBody.velocity;
        lastAngVel = projectileBody.angularVelocity;
    }

    /* working
    private void OnCollisionEnter(Collision col)
    {
        if (state != ProjectileState.inFlight)
            return;

        state = ProjectileState.impaled;
        trail.enabled = false;

        //Fix in position
        //projectileBody.MovePosition(col.contacts[0].point);
        //projectileBody.velocity = Vector3.zero;
        //projectileBody.angularVelocity = Vector3.zero;

        //Find out what we hit and fix to it or it's dead version
        Collider other = col.contacts[0].otherCollider;
        Life life = other.GetComponentInParent<Life>();
        Rigidbody otherBody = other.GetComponentInParent<Rigidbody>();

        //kill
        if (life)
            life.Dead();

        //apply hit force - do we need?
        if (otherBody)
        {
            stickTo = gameObject.AddComponent<FixedJoint>();
            stickTo.connectedBody = otherBody;
            otherBody.AddForceAtPosition(transform.forward * 10f, col.contacts[0].point, ForceMode.Impulse);

        }

        Owner = other.transform.root.gameObject;
    }
*/


    private void OnCollisionEnter(Collision collision)
    {
        //Can only penetrate something if we are in flight - deflections can't, they are just normal collisions
        if (state != ProjectileState.inFlight)
            return;

        //after we've hit something, we are no longer owned by anyone/thing
        Owner = null;

        Impenetrable impenetrable = collision.collider.GetComponent<Impenetrable>();
        if (!impenetrable)
        {
            state = ProjectileState.impaled;
            IgnoreCollisions(collision.collider.gameObject, true);

            Rigidbody otherBody = collision.collider.GetComponentInParent<Rigidbody>();
            if (otherBody)
            {
                stickTo = gameObject.AddComponent<FixedJoint>();
                stickTo.connectedBody = otherBody;
            }
        }
        else
            state = ProjectileState.deflected;

        trail.enabled = false;
    }

    /*
    private void OnTriggerStay(Collider other)
    {
        Substance substance = other.GetComponent<Substance>();
        Vector3 vel = projectileBody.velocity;

        if (substance)
            projectileBody.velocity *= (1 - substance.density);

        Debug.Log(vel.magnitude + " reduced to " + projectileBody.velocity.magnitude);

        if (projectileBody.velocity.magnitude < 0.1f)
        {
            state = ProjectileState.impaled;
            Rigidbody otherBody = other.GetComponentInParent<Rigidbody>();
            if (otherBody)
            {
                stickTo = gameObject.AddComponent<FixedJoint>();
                stickTo.connectedBody = otherBody;
            }
        }
    }
*/

}