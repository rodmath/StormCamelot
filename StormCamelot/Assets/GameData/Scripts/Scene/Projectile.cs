using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;


[RequireComponent(typeof(Rigidbody))]
public class Projectile : Item
{
    public bool hasFeathers = true;
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

    private Item item;
    private TrailRenderer trail;

    void Start()
    {
        item = GetComponent<Item>();
        projectileBody = GetComponent<Rigidbody>();

        trail = GetComponentInChildren<TrailRenderer>();
        restTimer = 0.5f;
    }



    public void Launch(float force)
    {
        state = ProjectileState.inFlight;
        restTimer = 0.5f;

        lastPosition = transform.position;

        item.Release();

        projectileBody.velocity = transform.forward * force;
        if (trail) 
            trail.enabled = true;
    }



    void FixedUpdate()
    {
        if (state == ProjectileState.inFlight)
        {

            Transform t = transform;
            if (transformToSpin)
                t = transformToSpin;

            if (hasFeathers && spinInFlight.magnitude.RoughlyEquals(0f))
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
        if (!enabled || state != ProjectileState.inFlight)
            return;

        //we are going to need a new owner
        Owner = null;

        Impenetrable impenetrable = collision.collider.GetComponent<Impenetrable>();
        Damage d = collision.contacts[0].thisCollider.GetComponent<Damage>();


        if (!impenetrable && d && d.type==DamageType.piercing)
        {
            state = ProjectileState.impaled;

            item.Grab(collision.collider.gameObject);
        }
        else
            state = ProjectileState.deflected;

        if (trail)
            trail.enabled = false;
    }


}