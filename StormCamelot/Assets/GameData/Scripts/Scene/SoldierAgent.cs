using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Vectrosity;


public class SoldierAgent : MonoBehaviour
{
    private CharacterController cc;

    public Vector3 gotoPoint;
    public Vector3 aimingPoint;

    public bool takeInput = false;
    public bool showDebug = true;

    //[Header("Input stuff")]
    private VectorLine aimMinLine;
    private VectorLine aimMaxLine;
    private VectorLine moveTriggerLine;

    [Header("Move and Aim stuff")]
    public Color aimColour = Color.blue;
    public Color moveColour = Color.green;
    public float aimWidth = 2f;
    public float acceleration = 0.1f;
    public float deceleration = 0.1f;
    public float agility = 0.1f;
    public float maxSpeed = 4;
    public float drag = 0.05f;

    [Header("Mouse distance limits")]
    public float aimMin = 1f;
    public float aimMax = 2f;
    public float moveTriggerRange = 3f;

    private VectorLine aimLine;
    private VectorLine moveLine, currentMoveLine;
    [SerializeField]
    private float speed;


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

        moveLine = new VectorLine("Move line", new List<Vector3>(), aimWidth);
        moveLine.points3.Add(transform.position);
        moveLine.points3.Add(transform.position);
        moveLine.color = moveColour;
        moveLine.Draw3DAuto();

        currentMoveLine = new VectorLine("Current Move line", new List<Vector3>(), aimWidth);
        currentMoveLine.points3.Add(transform.position);
        currentMoveLine.points3.Add(transform.position);
        currentMoveLine.points3.Add(transform.position);
        currentMoveLine.color = Color.yellow;
        currentMoveLine.lineType = LineType.Continuous;
        currentMoveLine.SetWidth(aimWidth * 10f, 0);
        currentMoveLine.SetWidth(aimWidth, 1); 
        currentMoveLine.Draw3DAuto();

        gotoPoint = transform.position;
    }


    public void ClearAiming()
    {
        aimingPoint = transform.position;
        aimLine.points3[0] = transform.position;
        aimLine.points3[1] = transform.position;
        aimLine.color = Color.clear;
    }


    public void AimIn(Vector3 aimDirection)
    {
        Vector3 aimVector = aimDirection.normalized;

        float dot = Vector3.Dot(aimVector, transform.forward);
        float dotNormalised = (dot + 1f) / 2f;  //should b 1 = same direction, 0 = opposite direction
        float aimLengthFactor = dotNormalised.Clamp(1f, 0.25f);

        aimingPoint = transform.position + (aimVector * aimLengthFactor * 4f);

        //
        aimLine.points3[0] = transform.position;
        aimLine.points3[1] = aimingPoint;
        aimLine.color = Color.red;
    }



    public void MoveIn(Vector3 moveDirection)
    {
        Vector3 moveVector = moveDirection.normalized;
        Vector3 turnedVector = Vector3.Slerp(transform.forward, moveVector, agility);

        float dot = Vector3.Dot(moveVector, transform.forward);

        //if it's backwards, we slow down to a speed where we can turn
        if (dot < -0.1f)
        {
            speed = (speed - deceleration).Clamp(0f, maxSpeed);
            if (speed < 0.1f)
                transform.rotation = Quaternion.LookRotation(turnedVector);
        }

        //if it's not backwards we turn and if quite forwards, accelerate
        else
        {
            transform.rotation = Quaternion.LookRotation(turnedVector);
            if (dot > 0.5f)
                speed = (speed + acceleration).Clamp(0f, maxSpeed);
        }

        //moveLine.points3[0] = transform.position;
        //moveLine.points3[1] = transform.position + (moveVector * maxSpeed);

    }






    void Update()
    {
        //if we have some speed we move
        if (speed.Abs() > 0.1f)
            cc.Move(transform.forward * Time.deltaTime * speed);

        //Draw our movement lines to be helpful
        currentMoveLine.points3[0] = transform.position;
        currentMoveLine.points3[1] = transform.position + (transform.forward * speed);
        currentMoveLine.points3[2] = transform.position + (transform.forward * maxSpeed);


        //apply drag
        speed = (speed * (1-drag)).Clamp(0f, maxSpeed);

        if (showDebug)
        {
            aimMinLine.MakeCircle(transform.position, transform.up, aimMin);
            aimMaxLine.MakeCircle(transform.position, transform.up, aimMax);
            moveTriggerLine.MakeCircle(transform.position, transform.up, moveTriggerRange);
        }


        ////Follow code
        //Vector3 toGoto = (gotoPoint - transform.position);
        //if (toGoto.magnitude > 0.2f)
        //{
        //    cc.Move(toGoto.normalized * Time.deltaTime);
        //}



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
