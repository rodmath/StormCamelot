using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Vectrosity;


public class SoldierAgent : MonoBehaviour {

    public bool takeInput = false;
    public bool showDebug = true;

    [Header("Input stuff")]
    private VectorLine inputLimitLine;

    [Header ("Aim stuff")]
    public Vector2 aimVector = new Vector2(1, 0);
    public Color32 aimColour = Color.black;
    public float aimMin = 2f;
    public float aimMax = 4f;
    public float aimWidth = 2f;

    private VectorLine aimLine;

	void Start () {
        inputLimitLine = new VectorLine("Input limit line", new List<Vector3>(64), 1);
        inputLimitLine.color = Color.red;
        inputLimitLine.Draw3DAuto();


        aimLine = new VectorLine("Aim line", new List<Vector3>(), aimWidth);
        aimLine.points3.Add(transform.position);
        aimLine.points3.Add(transform.position);

        aimLine.color = aimColour;
        aimLine.Draw3DAuto();
    }
	
	
	void Update () {
        if (takeInput)
            ProcessInput();
    }


    private void ProcessInput()
    {
        if (showDebug)
            inputLimitLine.MakeCircle(transform.position, aimMax);

        if (Input.GetMouseButton(0))
        {
            //get the distance from the mouse click to this transform
                // this creates a horizontal plane passing through this object's center
                Plane plane = new Plane(Vector3.up, transform.position);
            // create a ray from the mousePosition
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            // plane.Raycast returns the distance from the ray start to the hit point
            float distance;
            if (plane.Raycast(ray, out distance))
            {
                // some point of the plane was hit - get its coordinates
                Vector3 hitpoint = ray.GetPoint(distance);

                Vector3 a = transform.position - hitpoint;
                if (a.magnitude < aimMin)
                {
                    a = a.normalized * aimMin;
                }
                else if (a.magnitude > aimMax)
                {
                    a = a.normalized * aimMax;
                    aimLine.color = Color.red;
                }
                else
                    aimLine.color = aimColour;

                aimLine.points3[1] = transform.position + a;
            }
        }

    }

    }
