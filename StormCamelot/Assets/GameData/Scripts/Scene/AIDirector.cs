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


    // Use this for initialization
    void Start()
    {

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


        //have we got to our destination, so need a new one
        if (agent.pathStatus == NavMeshPathStatus.PathComplete && agent.remainingDistance <= 0) //Arrived.
        {
            //if we haven't got a weapon head to the closest one.
            if (!a.IsArmed)
            {
                float minDist = float.PositiveInfinity;
                Item closestItem = null;
                foreach (Item i in items)
                {
                    if (i.canBePickedUp)
                    {
                        float dist = (a.transform.position - i.transform.position).magnitude;
                        if (dist < minDist)
                        {
                            closestItem = i;
                            minDist = dist;
                        }
                    }
                }

                agent.destination = closestItem.transform.position;
                return;
            }

            else
            {
                //we are armed - head towatds a player
                agent.destination = Useful.PickRandom<Actor>(goodies).transform.position;
            }
        }


    }


}
