using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class SpriteRandomiser : MonoBehaviour
{
    public bool randomRotation = true;
    public bool randomColour = true;

    public float xPosVar = 0f;
    public float yPosVar = 0f;

    public float xScaleVar = 0f;
    public float yScaleVar = 0f;

    void Start()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();

        if (randomRotation)
            transform.rotation = Quaternion.Euler(0f, 0f, Random.Range(0f, 360f));

        if (randomColour)
            sr.color = Useful.RandomColor();

        Vector3 p = transform.position;
        p.x += Random.Range(-xPosVar, xPosVar);
        p.y += Random.Range(-yPosVar, yPosVar);

        transform.position = p;

        Vector3 scale = transform.localScale;
        scale.x *= Random.Range(1f, 1f+xScaleVar);
        scale.y *= Random.Range(1f, 1f+yScaleVar);
        transform.localScale = scale;

        /*
        //flips
        if (Useful.RandomBoolean())
            transform.localScale = Vector3.Scale(transform.localScale, new Vector3(-1f, 1f, 1f));
        if (Useful.RandomBoolean())
            transform.localScale = Vector3.Scale(transform.localScale, new Vector3(1f, -1f, 1f));
        */


    }


}
