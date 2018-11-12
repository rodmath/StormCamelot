using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Chronos;

public class Life : MonoBehaviour {

    public GameObject prefabOfDeadObject;

    private GameObject deadObject;

    private void Start()
    {

    }

    public void Dead(bool isDead)
    {
        if (isDead)
            Debug.Log(name + " was killed");
        else
            Debug.Log(name + " was resurrected");


        foreach (MonoBehaviour m in GetComponents<MonoBehaviour>())
        {
            if (m.GetType() == typeof(Transform))
                continue;
            //if (m.GetType() == typeof(Timeline))
                //continue;

            m.enabled = !isDead;
        }
    }


}
