using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Highlight : MonoBehaviour {

    public SpriteRenderer highlight;
	
	void Update () {
        //put on ground (plus a tiny bit)
        Vector3 pos = transform.position;
        pos.y = 0.01f;

        highlight.transform.position = pos;

        highlight.transform.forward = Vector3.up;
	}
}
