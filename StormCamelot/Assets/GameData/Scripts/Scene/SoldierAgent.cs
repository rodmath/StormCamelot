using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Vectrosity;


public class SoldierAgent : MonoBehaviour
{
    private CharacterController cc;


    public bool takeInput = false;
    public bool showDebug = true;

    //[Header("Input stuff")]
    private VectorLine aimMinLine;
    private VectorLine aimMaxLine;
    private VectorLine moveTriggerLine;

    [Header("Aim stuff")]
    public Vector2 aimVector = new Vector2(1, 0);
    public Color32 aimColour = Color.black;
    public float aimWidth = 2f;


    [Header("Mouse distance limits")]
    public float aimMin = 2f;
    public float aimMax = 3f;
    public float moveTriggerRange = 4f;

    private VectorLine aimLine;

    void Start()
    {
        cc = GetComponent<CharacterController>();

        aimMinLine = new VectorLine("Min aim limit line", new List<Vector3>(64), 1);
        aimMinLine.color = Color.blue;
        aimMinLine.Draw3DAuto();

        aimMaxLine = new VectorLine("Max aim limit line", new List<Vector3>(64), 1);
        aimMaxLine.color = Color.blue;
        aimMaxLine.Draw3DAuto();


        moveTriggerLine = new VectorLine("Move trigger line", new List<Vector3>(64), 1);
        moveTriggerLine.color = Color.blue;
        moveTriggerLine.Draw3DAuto();


        aimLine = new VectorLine("Aim line", new List<Vector3>(), aimWidth);
        aimLine.points3.Add(transform.position);
        aimLine.points3.Add(transform.position);

        aimLine.color = aimColour;
        aimLine.Draw3DAuto();


    }


    void Update()
    {
        //if (takeInput)
        //    ProcessInput();
        //else
            //cc.Move(Useful.RandomVector2Direction() * Time.deltaTime);
    }


    private void ProcessInput()
    {
        if (showDebug)
        {
            aimMinLine.MakeCircle(transform.position, transform.up, aimMin);
            aimMaxLine.MakeCircle(transform.position, transform.up, aimMax);
            moveTriggerLine.MakeCircle(transform.position, transform.up, moveTriggerRange);
        }

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

                // figure out based on the input where we want to aim
                Vector3 aimP = transform.position - hitpoint;
                transform.rotation = Quaternion.LookRotation(aimP);

                //if less than aim distance, just look
                if (aimP.magnitude < aimMin)
                    aimLine.color = Color.clear;

                //if in aim area, proportionally put point
                else if (aimP.magnitude < aimMax)
                    aimLine.color = Color.white;

                //if in move max aim area, put max aim point
                else if (aimP.magnitude < moveTriggerRange)
                    aimP = aimP.normalized * aimMax;

                //if beyon move range; move
                else
                {
                    aimLine.color = Color.clear;
                    cc.Move(aimP * Time.deltaTime);
                }



                aimLine.points3[0] = transform.position;
                aimLine.points3[1] = transform.position + aimP;
            }
        }

    }
}
