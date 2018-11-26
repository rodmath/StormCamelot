using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Vectrosity;
using Cinemachine;

public class Actor : MonoBehaviour
{
    [Header("Inspector setup variables")]
    public SpriteRenderer actionRadius;
    public Transform actionPoint;
    public Transform head;
    public Transform body;
    public Transform shoulders;
    public Collider pickupCollider;
    public CinemachineVirtualCamera vCamFPS;
    public CinemachineVirtualCamera vCamOverhead;


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
    public float speed;
    public float speedChange;
    private Vector3 aimingVector;
    public Vector3 AimingVector { get { return aimingVector; } }


    private Rigidbody agentBody;
    private Life life;
    private Color actionRadiusBaseColour;

    public bool ShowSelected { set { actionRadius.enabled = value; } }

    void Start()
    {
        agentBody = GetComponent<Rigidbody>();
        life = GetComponent<Life>();

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

        currentMoveLine = new VectorLine("Line: Current Move", new List<Vector3>(), aimWidth * 3f);
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




    public Transform LaunchProjectile(float angle)
    {
        if (projectile)
        {
            projectile.transform.position = actionPoint.position;
            projectile.transform.forward = aimingVector;
            projectile.transform.Rotate(-angle, 0f, 0f, Space.Self);
            projectile.Launch(throwForce);

            Transform proj = projectile.transform;
            projectile = null;
            actionRadius.color = actionRadiusBaseColour;

            return proj;
        }
        else
            return null;
    }


    private void Update()
    {
        //input updates happen in regular update
        LinesUpdate();

        //if we have some speed we move
        speed = (speed + (speedChange * Time.deltaTime)).Clamp(0f, maxSpeed);

        //assume slowing down for next frame
        speedChange = -deceleration;
    }

    void FixedUpdate()
    {
        if (speed.Abs() > 0.1f)
        {
            agentBody.MovePosition(transform.position + (transform.forward * Time.fixedDeltaTime * speed));
            pickupCollider.enabled = false;
        }
        else
            pickupCollider.enabled = true;

    }

    private void LinesUpdate()
    {
        //Update all our drawing lines
        currentMoveLine.points3[0] = Vector3.up + transform.position;
        currentMoveLine.points3[1] = Vector3.up + transform.position + (transform.forward * speed);
        currentMoveLine.points3[2] = Vector3.up + transform.position + (transform.forward * maxSpeed);

        //float dot = Vector3.Dot(head.forward, transform.forward);
        //float dotNormalised = (dot + 1f) / 2f;  //should b 1 = same direction, 0 = opposite direction
        //float aimLengthFactor = dotNormalised.Clamp(1f, 0.5f);
        float aimLengthFactor = 1f;

        aimLine.points3[0] = actionPoint.position;
        aimLine.points3[1] = actionPoint.position + (aimingVector * aimLengthFactor * 4f);

        aimMinLine.MakeCircle(transform.position, Vector3.up, aimMin);
        aimMaxLine.MakeCircle(transform.position, Vector3.up, aimMax);
        moveTriggerLine.MakeCircle(transform.position, Vector3.up, moveTriggerRange);


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
        //can only pick up when this is enabled and time is running
        if (!enabled)
            return;
        if (Time.timeScale <= 0)
            return;
            
        Projectile otherProj = other.GetComponentInParent<Projectile>();

        if (otherProj && !projectile && otherProj.canBePickedUp)
        {
            Color c = new Color(1f, 0f, 0f, actionRadiusBaseColour.a);
            actionRadius.color = c;

            StartCoroutine(PickupDelay(otherProj, 1f));

            Vector3 gripOffset = (actionPoint.right * 0.75f) + (actionPoint.up * 0.25f);
            otherProj.Grab(gameObject, actionPoint.transform, gripOffset);
        }
            
    }

    IEnumerator PickupDelay(Projectile pickupProj, float secs)
    {
        yield return new WaitForSeconds(secs);
        projectile = pickupProj;
    }




}



