using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class SpriteRandomiser : MonoBehaviour
{
    public float timeBetweenRandomisations = 0.5f;

    [Header("Colour Variance")]
    public bool randomColour = true;
    [Range(0, 1)]
    public float alfa = 1;


    [Header("Transform Variance")]
    public bool randomRotation = true; 
    public float xPosVar = 0f;
    public float yPosVar = 0f;
    public float xScaleVar = 0f;
    public float yScaleVar = 0f;


    private SpriteRenderer sr;

    private void OnEnable()
    {
        sr = GetComponent<SpriteRenderer>();

        Randomise();

        if (timeBetweenRandomisations > 0)
            StartCoroutine(RepeatRandomise());
    }



    IEnumerator RepeatRandomise()
    {
        while (enabled)
        {
            yield return new WaitForSecondsRealtime(timeBetweenRandomisations);
            Randomise();
        }
    }



    private void Randomise()
    {
        if (randomRotation)
            transform.rotation = Quaternion.Euler(0f, 0f, Random.Range(0f, 360f));

        if (randomColour)
            sr.color = Useful.RandomColor(alfa);

        if (xPosVar>0 || yPosVar>0)
        {
            Vector3 p = transform.position;
            p.x += Random.Range(-xPosVar, xPosVar);
            p.y += Random.Range(-yPosVar, yPosVar);

            transform.position = p;
        }

        if (xPosVar > 0 || yPosVar > 0)
        {
            Vector3 scale = transform.localScale;
            scale.x *= Random.Range(1f, 1f + xScaleVar);
            scale.y *= Random.Range(1f, 1f + yScaleVar);
            transform.localScale = scale;
        }

        /*
        //flips
        if (Useful.RandomBoolean())
            transform.localScale = Vector3.Scale(transform.localScale, new Vector3(-1f, 1f, 1f));
        if (Useful.RandomBoolean())
            transform.localScale = Vector3.Scale(transform.localScale, new Vector3(1f, -1f, 1f));
        */

    }




}
