using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Chronos;

public class Life : MonoBehaviour {

    public GameObject prefabOfDeadObject;

    public GameObject Kill()
    {
        Agent agent = GetComponent<Agent>();
        if (agent)
            agent.enabled = false;

        GameObject deadObject = Instantiate(prefabOfDeadObject, transform.position, transform.rotation);

        //now kill this one
        gameObject.SetActive(false);

        return deadObject;
    }

    public GameObject Ressurect()
    {
        Agent agent = GetComponent<Agent>();
        if (agent)
            agent.enabled = false;

        GameObject deadObject = Instantiate(prefabOfDeadObject, transform.position, transform.rotation);

        //now kill this one
        gameObject.SetActive(false);

        return deadObject;
    }
}
