using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Vectrosity;
using Chronos;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(Timeline))]
public class SoldierAgent : MonoBehaviour
{
    [Header("Inspector setup variables")]
    public SpriteRenderer actionRadius;
    public Transform actionPoint;

    [Header("Weaponary")]
    public Projectile projectile;

    [Header("Move and Aim stuff")]
    public Color aimColour = Color.blue;
    public Color moveColour = Color.green;
    public float aimWidth = 2f;
    public float acceleration = 0.1f;
    public float deceleration = 0.1f;
    public float agility = 0.1f;
    public float maxSpeed = 4;
    public float drag = 0.05f;

    [Header("Input distance limits - set by director")]
    private float aimMin = 1f;
    private float aimMax = 2f;
    private float moveTriggerRange = 3f;



    private VectorLine aimMinLine;
    private VectorLine aimMaxLine;
    private VectorLine moveTriggerLine;
    private VectorLine aimLine;
    private VectorLine moveLine, currentMoveLine;
    private float speed;
    private float speedChange;
    private Vector3 aimingPoint;



    private CharacterController cc;
    private Timeline time;

    public bool ShowSelected { set { actionRadius.enabled = value; } }

    void Start()
    {
        cc = GetComponent<CharacterController>();
        time = GetComponent<Timeline>();
        SetupLines();
        aimingPoint = actionPoint.position;
        actionRadius.enabled = false;
        //if (projectile) GripProjectile(projectile);
    }

    public void SetupInput(float min, float max, float move)
    {
        aimMin = min;
        aimMax = max;
        moveTriggerRange = move;
        actionRadius.transform.localScale = Vector3.one * move;
    }



    private void SetupLines()
    {
        aimMinLine = new VectorLine("Min aim limit line", new List<Vector3>(64), 1);
        aimMinLine.Draw3DAuto();

        aimMaxLine = new VectorLine("Max aim limit line", new List<Vector3>(64), 1);
        aimMaxLine.Draw3DAuto();

        moveTriggerLine = new VectorLine("Move trigger line", new List<Vector3>(64), 1);
        moveTriggerLine.Draw3DAuto();


        aimLine = new VectorLine("Aim line", new List<Vector3>(), aimWidth);
        aimLine.points3.Add(transform.position + Vector3.up);
        aimLine.points3.Add(transform.position + Vector3.up);
        aimLine.color = aimColour;
        aimLine.Draw3DAuto();

        moveLine = new VectorLine("Move line", new List<Vector3>(), aimWidth);
        moveLine.points3.Add(transform.position + Vector3.up);
        moveLine.points3.Add(transform.position + Vector3.up);
        moveLine.color = moveColour;
        moveLine.Draw3DAuto();

        currentMoveLine = new VectorLine("Current Move line", new List<Vector3>(), aimWidth);
        currentMoveLine.points3.Add(transform.position + Vector3.up);
        currentMoveLine.points3.Add(transform.position + Vector3.up);
        currentMoveLine.points3.Add(transform.position + Vector3.up);
        currentMoveLine.color = Color.yellow;
        currentMoveLine.lineType = LineType.Continuous;
        currentMoveLine.SetWidth(aimWidth * 3f, 0);
        currentMoveLine.SetWidth(aimWidth, 1);
        currentMoveLine.Draw3DAuto();


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
        float aimLengthFactor = dotNormalised.Clamp(1f, 0.3f);

        aimingPoint = actionPoint.position + (aimVector * aimLengthFactor * 4f);

    }



    public void MoveIn(Vector3 moveDirection)
    {
        Vector3 moveVector = moveDirection.normalized;
        Vector3 turnedVector = Vector3.Slerp(transform.forward, moveVector, agility);

        float dot = Vector3.Dot(moveVector, transform.forward);

        //if it's backwards, we slow down to a speed where we can turn
        if (dot < -0.1f)
        {
            speedChange = -deceleration;
            if (speed < 0.1f)
                transform.rotation = Quaternion.LookRotation(turnedVector);
        }

        //if it's not backwards we turn and if mainly forwards, accelerate
        else
        {
            speedChange = 0f;
            transform.rotation = Quaternion.LookRotation(turnedVector);
            if (dot > 0.5f)
                speedChange = acceleration;
        }


    }






    void Update()
    {
        //if we have some speed we move
        speed = (speed + (speedChange * time.deltaTime)).Clamp(0f, maxSpeed);
        if (speed.Abs() > 0.1f)
            cc.Move(transform.forward * time.deltaTime * speed);

        LinesUpdate();

        //assume slowing down for next frame
        speedChange = -deceleration;

    }

    private void LinesUpdate()
    {
        //Update all our drawing lines
        currentMoveLine.points3[0] = transform.position;
        currentMoveLine.points3[1] = transform.position + (transform.forward * speed);
        currentMoveLine.points3[2] = transform.position + (transform.forward * maxSpeed);

        aimLine.points3[0] = actionPoint.position;
        aimLine.points3[1] = aimingPoint;
        aimLine.color = Color.red;

        aimMinLine.MakeCircle(transform.position, transform.up, aimMin);
        aimMaxLine.MakeCircle(transform.position, transform.up, aimMax);
        moveTriggerLine.MakeCircle(transform.position, transform.up, moveTriggerRange);


        ////debug lines around aiming
        //if (showDebug)
        //{
        //    aimMinLine.color = Color.blue;
        //    aimMaxLine.color = Color.blue;
        //    moveTriggerLine.color = Color.blue;
        //}
        //else
        //{
        //    aimMinLine.color = Color.clear;
        //    aimMaxLine.color = Color.clear;
        //    moveTriggerLine.color = Color.clear;
        //}
    }



    public void GripProjectile(Projectile proj)
    {
        if (!projectile)
        {
            projectile = proj;
            projectile.held = true;
            projectile.transform.SetParent(transform);
            projectile.transform.position = actionPoint.position;
            projectile.transform.forward = transform.up;
        }
    }


    public void LaunchProjectile()
    {
        if (projectile)
        {
            projectile.held = false;
            projectile.transform.SetParent(null);
            projectile.transform.position = actionPoint.position;
            projectile.transform.forward = (aimingPoint - actionPoint.position);
            projectile.Launch(15f, gameObject.transform);

            projectile = null;
        }
    }


    private void OnTriggerEnter(Collider other)
    {
        Projectile proj = other.GetComponent<Projectile>();
        if (proj)
        {
            if (proj.held)
            {
                if (projectile)
                    Debug.Log("On Projectile, but can't pick up as is already holding one");
                else
                    GripProjectile(proj);
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }

    //        else
    //    {
    //        SoldierAgent soldierAgent = hitCol.GetComponentInParent<SoldierAgent>();

    //        if (soldierAgent && soldierAgent.projectile==null && owner==null)
    //        {

    //            soldierAgent.GripProjectile(this);
    //}

}
