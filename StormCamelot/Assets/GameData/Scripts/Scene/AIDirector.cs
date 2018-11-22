﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AIDirector : MonoBehaviour {

    public int numGoodies = 10;
    public int numBadies = 10;

    public int targetActiveBaddies = 1;
    [SerializeField]
    private int activeBaddies;

    public GameObject actorPrefab;

    public List<Actor> goodies;
    public List<Actor> baddies;

    private List<Actor> actors;



    // Use this for initialization
    void Start () {

        //get existing actors, top up as necessary - assume built are goodies
        goodies = Object.FindObjectsOfType<Actor>().ToList();
        populateActors(goodies, numGoodies, -10, 2, "Goody");
        populateActors(baddies, numBadies, 25, 4, "Baddy");


        //now deactivate baddies until just have our target number
        activeBaddies = 0;
        foreach(Actor a in baddies)
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
        float x = -numToHave * (spacing/2);
        foreach (Actor a in listToPopulate)
        {
            a.transform.SetParent(actorHolder.transform);
            a.transform.position = new Vector3(x, 0f, zLocation);
            a.name = namePrefix + (" 0" + i.ToString()).Right(2);
            x += spacing;
            i++;
        }
    }



    private void ActivateBaddie()
    {
        foreach(Actor a in baddies)
        {
            if (!a.isActiveAndEnabled)
            {
                a.gameObject.SetActive(true);
                return;
            }
        }
    }


	// Update is called once per frame
	void Update () {

        activeBaddies = 0;
        foreach (Actor a in baddies)
        {
            if (a.isActiveAndEnabled)
                activeBaddies++;
        }

        if (activeBaddies < targetActiveBaddies)
        {
            ActivateBaddie();
        }
	}
}