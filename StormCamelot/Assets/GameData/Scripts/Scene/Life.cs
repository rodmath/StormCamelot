using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Life : MonoBehaviour {

    public GameObject prefabOfDeadObject;

    public GameObject Kill()
    {
        GameObject deadObject = Instantiate(prefabOfDeadObject, transform.position, transform.rotation);

        //now kill this one
        gameObject.SetActive(false);
        //gameObject.transform.position = new Vector3(100000f, 0f, 10000f);

        return deadObject;
    }
}
