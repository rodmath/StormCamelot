using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Life : MonoBehaviour
{

    private enum LifeState
    {
        alive,
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
                body.AddForce(Random.onUnitSphere * body.mass, ForceMode.Impulse);
                body.AddTorque(Random.onUnitSphere * body.mass, ForceMode.Impulse);

                yield return new WaitForSeconds(deathThrowDuration / (float)twitches);
            }

            //fall through floor
            foreach (Collider c in GetComponentsInChildren<Collider>())
                c.enabled = false;
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

}
