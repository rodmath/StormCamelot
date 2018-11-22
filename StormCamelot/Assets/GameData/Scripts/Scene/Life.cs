using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Life : MonoBehaviour
{

    private enum LifeState
    {
        alive,
        stunned,
        frozen,
        takingLastBreaths,
        dead
    }

    public bool alive = true;
    public float deathThrowDuration = 4f;

    [SerializeField]
    private LifeState state = LifeState.alive;
    private bool cacheIsKinetic;
    private Vector3 cachePosition;
    private Quaternion cacheRotation;

    public void Kill()
    {
        Debug.Log(name + " was killed");
        alive = false;

    }

    private void Update()
    {
        if (!alive && state == LifeState.alive)
            StartCoroutine(TakeLastBreaths());

        //manual resurrection
        if (alive && state == LifeState.dead)
            StartCoroutine(Resurrect(true));

        //returning to wait queue
        if (!alive && transform.position.y < -10f)
            StartCoroutine(Resurrect(false));
    }

    private void MakeComponents(bool active)
    {
        NavMeshAgent nav = GetComponent<NavMeshAgent>();
        if (nav)
            nav.enabled = active;

        Actor agent = GetComponent<Actor>();
        if (agent)
            agent.enabled = active;

    }

    private IEnumerator TakeLastBreaths()
    {
        state = LifeState.takingLastBreaths;

        MakeComponents(false);

        Rigidbody body = GetComponent<Rigidbody>();
        if (body)
        {
            //ragdollify
            body.WakeUp();
            cacheIsKinetic = body.isKinematic;
            cachePosition = body.position;
            cacheRotation = body.rotation;

            if (body.isKinematic)
                body.isKinematic = false;

            //twitch
            int twitches = Random.Range(2, 5);
            for (int i = 0; i < twitches; i++)
            {
                body.AddForce(Random.onUnitSphere * body.mass / 5f, ForceMode.Impulse);
                body.AddTorque(Random.onUnitSphere * body.mass / 5f, ForceMode.Impulse);

                yield return new WaitForSeconds(deathThrowDuration / (float)twitches);
            }
        }

        state = LifeState.dead;
    }


    private IEnumerator Resurrect(bool andEnable)
    {
        state = LifeState.alive;

        Rigidbody body = GetComponent<Rigidbody>();
        if (body)
        {
            body.isKinematic = cacheIsKinetic;
            body.MovePosition(cachePosition);
            body.MoveRotation(cacheRotation);

        }

        //reactivate colliders
        foreach (Collider c in GetComponentsInChildren<Collider>())
            c.enabled = true;

        //wait a few frames for our position to update before re-enabling navmesh agent (otherwise it's not created)
        yield return new WaitForFixedUpdate();

        MakeComponents(true);

        gameObject.SetActive(andEnable);
    }


    private IEnumerator Stunned(float stunDuration)
    {
        state = LifeState.stunned;

        MakeComponents(false);

        Rigidbody body = GetComponent<Rigidbody>();
        if (body)
        {
            //ragdollify
            body.WakeUp();
            cacheIsKinetic = body.isKinematic;
            cachePosition = body.position;
            cacheRotation = body.rotation;

            if (body.isKinematic)
                body.isKinematic = false;

            //twitch
            body.AddForce(Random.onUnitSphere * body.mass, ForceMode.Impulse);
            body.AddTorque(Random.onUnitSphere * body.mass, ForceMode.Impulse);

            yield return new WaitForSeconds(stunDuration);
        }

        yield return new WaitForSeconds(stunDuration);

        MakeComponents(true);

        state = LifeState.alive;
    }







    private void OnCollisionEnter(Collision collision)
    {
        //Debug.Log(gameObject.name + " was hit by " + collision.collider.gameObject.name + " with rel vel: " + collision.relativeVelocity.magnitude); 

        Damage damage = collision.collider.GetComponent<Damage>();
        if (!damage)
            return;

        float hitForce = collision.relativeVelocity.magnitude;

        //what damage type is it, and what damage is caused
        if (damage.type == DamageType.piercing  || damage.type == DamageType.slashing)
        {
            if (collision.relativeVelocity.magnitude > 20f)
            {
                if (alive)
                    alive = false;
            }
        }


        if (damage.type == DamageType.bludgeoning)
        {
            if (collision.relativeVelocity.magnitude > 20f)
            {
                StartCoroutine(Stunned(1f));
            }

        }
    }

}
