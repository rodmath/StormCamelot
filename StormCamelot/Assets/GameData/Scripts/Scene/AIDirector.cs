using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;


public class AIDirector : MonoBehaviour
{

    public int numGoodies = 10;
    public int numBadies = 10;

    public int targetActiveBaddies = 1;
    [SerializeField]
    private int activeBaddies;

    public GameObject actorPrefab;

    public List<Actor> goodies;
    public List<Actor> baddies;

    public List<SpawnLocation> spawns;

    private List<Actor> actors;
    private List<Item> items;
    private List<SpawnLocation> usedSpawns;

    private FreezeMaster freezeMaster;



    void Start()
    {
        freezeMaster = Object.FindObjectOfType<FreezeMaster>();

        spawns = Object.FindObjectsOfType<SpawnLocation>().ToList();
        items = Object.FindObjectsOfType<Item>().ToList();

        //get existing actors, top up as necessary - assume built are goodies
        goodies = Object.FindObjectsOfType<Actor>().ToList();
        //populateActors(goodies, numGoodies, -10, 2, "Goody");
        //populateActors(baddies, numBadies, 25, 4, "Baddy");

        populateActors(goodies, numGoodies, "Goody");
        populateActors(baddies, numBadies, "Baddy");

        //now deactivate baddies until just have our target number
        activeBaddies = 0;
        foreach (Actor a in baddies)
        {
            if (activeBaddies >= targetActiveBaddies)
                a.gameObject.SetActive(false);
            else
                activeBaddies++;
        }

    }


    private void populateActors(List<Actor> listToPopulate, int numToHave, float zLocation, float spacing, string namePrefix)
    {
        while (listToPopulate.Count < numToHave)
            listToPopulate.Add(Instantiate(actorPrefab).GetComponent<Actor>());

        GameObject actorHolder = new GameObject(namePrefix);
        int i = 1;
        float x = -numToHave * (spacing / 2);
        foreach (Actor a in listToPopulate)
        {
            a.transform.SetParent(actorHolder.transform);
            a.transform.position = new Vector3(x, 0f, zLocation);
            a.name = namePrefix + (" 0" + i.ToString()).Right(2);
            x += spacing;
            i++;
        }
    }


    private void populateActors(List<Actor> listToPopulate, int numToHave, string namePrefix)
    {
        while (listToPopulate.Count < numToHave)
            listToPopulate.Add(Instantiate(actorPrefab).GetComponent<Actor>());

        GameObject actorHolder = new GameObject(namePrefix);
        int i = 1;
        foreach (Actor a in listToPopulate)
        {
            a.transform.SetParent(actorHolder.transform);
            a.name = namePrefix + ((" 0" + i.ToString())).Right(2);

            if (spawns.Count > 0)
            {

                SpawnLocation sp = Useful.PickRandom<SpawnLocation>(spawns);
                {
                    a.transform.position = sp.transform.position;
                    spawns.Remove(sp);
                }
            }
            i++;
        }
    }


    //activates one more baddie - if any not active
    private void ActivateBaddie()
    {
        foreach (Actor a in baddies)
        {
            if (!a.isActiveAndEnabled)
            {
                a.gameObject.SetActive(true);
                return;
            }
        }
    }


    // Update is called once per frame
    void Update()
    {
        //do nothing while frozen
        if (freezeMaster.IsFrozen)
            return;

        activeBaddies = 0;
        foreach (Actor a in baddies)
        {
            if (a.isActiveAndEnabled)
            {
                activeBaddies++;
                BasicAI(a);
            }
        }

        if (activeBaddies < targetActiveBaddies)
        {
            ActivateBaddie();
        }

    }



    private void BasicAI(Actor a)
    {
        //needs navmesh agent
        NavMeshAgent agent = a.GetComponent<NavMeshAgent>();
        if (!agent)
            return;


        //If we can attack them, do that
        if (TryAndAttackFoe(a, agent))
            return;
        else
            a.attackTimer = 1f;


        //if we are on a path somewhere, just keep going
        bool randomReplan = (Random.Range(0, 100) == 1);
        bool endOfPath = (agent.pathStatus == NavMeshPathStatus.PathComplete || agent.remainingDistance <= 0.2f); //Arrived.

        if (!randomReplan && !endOfPath)
            return;

        //we're not on a path, and couldn't attack anyone, so now:
        //do we have a weapon, do we have an enemy

        if (!a.IsArmed)
        {
            a.target = GetClosestWeapon(a, agent);
            agent.destination = a.target.position;
            return;
        }

        //ensure we have a foe
        if (!a.foeTarget)
            a.foeTarget = Useful.PickRandom<Actor>(goodies).transform;

        agent.destination = a.foeTarget.position;
    }


    private Transform GetClosestWeapon(Actor actor, NavMeshAgent agent)
    {
        float minDist = float.PositiveInfinity;
        Item closestItem = null;
        foreach (Item i in items)
        {
            if (i.canBePickedUp)
            {
                float dist = (actor.transform.position - i.transform.position).magnitude;
                if (dist < minDist)
                {
                    closestItem = i;
                    minDist = dist;
                }
            }
        }

        return closestItem.transform;
    }


    private bool TryAndAttackFoe(Actor actor, NavMeshAgent agent)
    {
        //are we armed
        if (!actor.IsArmed)
            return false;

        //do we have a target
        if (!actor.foeTarget)
            return false;

        GameObject us = actor.gameObject;
        GameObject them = actor.foeTarget.gameObject;

        //are they too far away
        float minDist = 10;
        Vector3 targetVect = them.transform.position - us.transform.position;
        actor.AimIn(targetVect);

        if (targetVect.magnitude > minDist)
            return false;

        //can we actually see them
        if (!Useful.hasLOS(us, them))
            return false;


        agent.isStopped = true;
        agent.ResetPath();

        actor.attackTimer -= Time.deltaTime;
        if (actor.attackTimer <=0)
        {
            actor.LaunchProjectile(10f);
            actor.attackTimer = 1f;
        }

        return true;
    }

}
