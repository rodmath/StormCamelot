using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Vectrosity;
using Chronos;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(Timeline))]
public class Agent : MonoBehaviour
{
    [Header("Inspector setup variables")]
    public SpriteRenderer actionRadius;
    public Transform actionPoint;
    public Transform head;
    public Transform body;
    public Transform shoulders;
    public Collider pickupCollider;

    [Header("Weaponary")]
    public Projectile projectile;
    public float throwForce = 30f;
    public float throwAngle = 15f;

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
    private Vector3 aimingVector;



    private CharacterController cc;
    private Timeline time;
    private Color actionRadiusBaseColour;

    public bool ShowSelected { set { actionRadius.enabled = value; } }

    void Start()
    {
        cc = GetComponent<CharacterController>();
        time = GetComponent<Timeline>();
        SetupLines();
        aimingVector = Vector3.zero;
        actionRadius.enabled = false;
        actionRadiusBaseColour = actionRadius.color;
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
        aimMinLine = new VectorLine("Line: Min aim limit", new List<Vector3>(64), 1);
        aimMinLine.Draw3DAuto();

        aimMaxLine = new VectorLine("Line: Max aim limit", new List<Vector3>(64), 1);
        aimMaxLine.Draw3DAuto();

        moveTriggerLine = new VectorLine("Line: Move trigger", new List<Vector3>(64), 1);
        moveTriggerLine.Draw3DAuto();


        aimLine = new VectorLine("Line: Aim", new List<Vector3>(), aimWidth);
        aimLine.points3.Add(transform.position + Vector3.up);
        aimLine.points3.Add(transform.position + Vector3.up);
        aimLine.color = aimColour;
        aimLine.Draw3DAuto();

        moveLine = new VectorLine("Line: Move", new List<Vector3>(), aimWidth);
        moveLine.points3.Add(transform.position + Vector3.up);
        moveLine.points3.Add(transform.position + Vector3.up);
        moveLine.color = moveColour;
        moveLine.Draw3DAuto();

        currentMoveLine = new VectorLine("Line: Current Move", new List<Vector3>(), aimWidth);
        currentMoveLine.points3.Add(transform.position + Vector3.up);
        currentMoveLine.points3.Add(transform.position + Vector3.up);
        currentMoveLine.points3.Add(transform.position + Vector3.up);
        currentMoveLine.color = Color.yellow;
        currentMoveLine.lineType = LineType.Continuous;
        currentMoveLine.SetWidth(aimWidth * 3f, 0);
        currentMoveLine.SetWidth(aimWidth, 1);
        currentMoveLine.Draw3DAuto();


    }


    public void AimIn(Vector3 aimDirection)
    {
        aimingVector = aimDirection.normalized;
        aimLine.color = Color.red;

        shoulders.transform.forward = aimingVector;

    }

    public void ClearAiming()
    {
        aimingVector = Vector3.zero;
        aimLine.color = Color.clear;
    }




    public void MoveIn(Vector3 moveDirection)
    {
        Vector3 desiredMoveVector = moveDirection.normalized;
        Vector3 currentMoveVector = Vector3.Slerp(transform.forward, desiredMoveVector, agility);

        float dot = Vector3.Dot(desiredMoveVector, transform.forward);

        //if it's backwards, we slow down to a speed where we can turn
        if (dot < -0.1f)
        {
            speedChange = -deceleration;
            if (speed < 0.1f)
                transform.rotation = Quaternion.LookRotation(currentMoveVector);
        }

        //if it's not backwards we turn and if mainly forwards, accelerate
        else
        {
            speedChange = 0f;
            transform.rotation = Quaternion.LookRotation(currentMoveVector);
            shoulders.rotation = Quaternion.LookRotation(currentMoveVector);
            if (dot > 0.5f)
                speedChange = acceleration;
        }
    }


    public void GripProjectile(Projectile proj)
    {
        if (!projectile)
        {


            Color c = new Color(1f, 0f, 0f, actionRadiusBaseColour.a);
            time.Do(false, delegate () { return DoColourChange(c); }, delegate (Color oldColour) { UndoColourChange(oldColour); });
            time.Do(false, delegate () { return DoProjectileChange(proj); }, delegate (Projectile oldProj) { UndoProjectileChange(oldProj); });


            Vector3 gripOffset = (actionPoint.right * 0.75f) + (actionPoint.up * 0.25f);
            projectile.ChronosPickUp(gameObject, actionPoint.transform, gripOffset);
        }
    }


    public Transform LaunchProjectile(float angle)
    {
        if (projectile)
        {
            projectile.transform.position = actionPoint.position;
            projectile.transform.forward = aimingVector;
            projectile.transform.Rotate(-angle, 0f, 0f, Space.Self);
            projectile.ChronosLaunch(throwForce, gameObject);

            Transform proj = projectile.transform;

            time.Do(false, delegate () { return DoProjectileChange(null); }, delegate (Projectile oldProj) { UndoProjectileChange(oldProj); });
            time.Do(false, delegate () { return DoColourChange(actionRadiusBaseColour); }, delegate (Color oldColour) { UndoColourChange(oldColour); });

            return proj;
        }
        else
            return null;
    }

    private Color DoColourChange(Color color)
    {
        Color oldColour = actionRadius.color;
        actionRadius.color = color;
        return oldColour;
    }

    private void UndoColourChange(Color c)
    {
        actionRadius.color = c;
    }

    private Projectile DoProjectileChange(Projectile proj)
    {
        Projectile oldProj = projectile;
        projectile = proj;
        return oldProj;
    }

    private void UndoProjectileChange(Projectile oldProj)
    {
        projectile = oldProj;
    }

    void Update()
    {
        if (time.clock.localTimeScale < 0f)
            return;

            //aiming our head and body
            //if (aimingVector.magnitude > 0)
            //{
            //    head.forward = Vector3.Slerp(head.forward, aimingVector, 0.2f);
            //}

            //if we have some speed we move
            speed = (speed + (speedChange * time.deltaTime)).Clamp(0f, maxSpeed);

        if (speed.Abs() > 0.1f)
        {
            cc.Move(transform.forward * time.deltaTime * speed);
            pickupCollider.enabled = false;
        }
        else
        {
            pickupCollider.enabled = true;

        }
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

        //float dot = Vector3.Dot(head.forward, transform.forward);
        //float dotNormalised = (dot + 1f) / 2f;  //should b 1 = same direction, 0 = opposite direction
        //float aimLengthFactor = dotNormalised.Clamp(1f, 0.5f);
        float aimLengthFactor = 1f;

        aimLine.points3[0] = actionPoint.position;
        aimLine.points3[1] = actionPoint.position + (aimingVector * aimLengthFactor * 4f);

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





    private void OnTriggerStay(Collider other)
    {
        if (enabled && time.timeScale>=0 && !projectile)
        {
            Projectile proj = other.GetComponentInParent<Projectile>();
            //we can only pick it up if 
            if (proj && proj.CanBePickedUp && proj.Owner != gameObject)
                GripProjectile(proj);
        }
    }

    //private void OnTriggerEnter(Collider other)
    //{
    //    //drop out if we're not picking up
    //    if (!pickupCollider.enabled)
    //        return;

    //    Projectile proj = other.GetComponentInParent<Projectile>();
    //    if (proj)
    //    {
    //        if (proj.held)
    //        {
    //            if (projectile)
    //                Debug.Log("On Projectile, but can't pick up as is already holding one");
    //            else
    //                GripProjectile(proj);
    //        }
    //    }
    //}

    //        else
    //    {
    //        SoldierAgent soldierAgent = hitCol.GetComponentInParent<SoldierAgent>();

    //        if (soldierAgent && soldierAgent.projectile==null && owner==null)
    //        {

    //            soldierAgent.GripProjectile(this);
    //}

}
