using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Vectrosity;
using Cinemachine;
using UnityEngine.UI;
using UnityEngine.AI;

public class InputDirector : MonoBehaviour
{
    [Header("Inspector setup variables")]
    public GameObject playerSelectButton;

    [Header("Settings")]
    public float soldierSelectionRange = 3f;
    public float soldierNoActionRange = 1f;


    private List<Actor> soldiers;
    private Actor soldierSelected;
    private bool inFPSmode = false;

    private List<RigidbodyFreeze> freezers;
    private bool rigidBodiesFrozen = false;

    private Vector3 clickPos;
    private VectorLine horizon;


    private void Start()
    {
        freezers = Object.FindObjectsOfType<RigidbodyFreeze>().ToList();
        soldiers = Object.FindObjectsOfType<Actor>().ToList();

        foreach (Actor s in soldiers)
            s.SetupInput(soldierNoActionRange, soldierSelectionRange, soldierSelectionRange);


        horizon = new VectorLine("Line: Horizon", new List<Vector2>(), 1);
        horizon.points2.Add(new Vector2(0, Screen.height / 2f));
        horizon.points2.Add(new Vector2(Screen.width, Screen.height / 2f));
        horizon.color = Color.green;
        horizon.Draw();
    }

    private void Update()
    {
        if (Input.GetMouseButton(0))
        {
            SetClickPoint();

            if (Input.GetMouseButtonDown(0))
                StartClick();
            else
            {
                //mouse is down, but not being moved up or down this frame
                if (inFPSmode)
                    UpdateFPSControl();
                else if (soldierSelected)
                    UpdateSoldierControl();
                else
                    UpdateCameraPanControl();
            }
        }
        else if (Input.GetMouseButtonUp(0))
            EndClick();
        else
        {
            if (Input.GetKeyDown("1"))
                SelectSoldier(soldiers[0]);
            else if (Input.GetKeyDown("2") && soldiers.Count > 1)
                SelectSoldier(soldiers[1]);
            else if (Input.GetKeyDown("3") && soldiers.Count > 2)
                SelectSoldier(soldiers[2]);
            else if (Input.GetKeyDown("4") && soldiers.Count > 3)
                SelectSoldier(soldiers[3]);

            if (Input.GetKeyDown("p"))
                ToggleFreeze();

            if (Input.GetKeyDown("z"))
                SetDestinationsTo(soldiers[0].transform.position);
            else if (Input.GetKeyDown("x") && soldiers.Count > 1)
                SetDestinationsTo(soldiers[1].transform.position);
            else if (Input.GetKeyDown("c") && soldiers.Count > 2)
                SetDestinationsTo(soldiers[2].transform.position);
            else if (Input.GetKeyDown("v") && soldiers.Count > 3)
                SetDestinationsTo(soldiers[3].transform.position);


        }


    }

    private void SetClickPoint()
    {
        if (inFPSmode)
            clickPos = Camera.main.ScreenToViewportPoint(Input.mousePosition);
        else
        {
            Plane plane = new Plane(Vector3.up, transform.position);
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            float rayDist;
            if (plane.Raycast(ray, out rayDist))
            {
                clickPos = ray.GetPoint(rayDist);
            }
        }
    }

    private void StartClick()
    {
        if (inFPSmode && soldierSelected)
            return;


        Actor soldierToSelect = null;
        float dist;
        float minDist = soldierSelectionRange;
        foreach (Actor soldier in soldiers)
        {
            dist = (clickPos - soldier.transform.position).magnitude;
            if (dist < soldierSelectionRange)
            {
                if (dist < minDist)
                {
                    minDist = dist;
                    soldierToSelect = soldier;
                }
            }
        }


        SelectSoldier(soldierSelected);//re-select to revert all settings (i.e. look at it again)
    }


    private void EndClick()
    {
        //if we're in FPS mode we are launching something
        if (inFPSmode)
        {
            soldierSelected.vCamFPS.Priority = 10;

            if (soldierSelected.projectile)
            {
                // Capture the FPS press as an X rotation to determine the flight angle
                // viewport = 0 = -30
                // viewport 0.5 = level
                // viewport > 0  = 90

                float xAngle = 0f;
                if (clickPos.y < 0)
                    xAngle = (clickPos.y - 0.5f) * 60f;   //-0.5*60 = -30 to 0
                else
                    xAngle = (clickPos.y - 0.5f) * 120f; //0 to 0.5*180 = 60

                float yAngle = 0f;
                if (clickPos.x < 0)
                    yAngle = (clickPos.x - 0.5f) * 60f;   //-0.5*60 = -30 to 0
                else
                    yAngle = (clickPos.x - 0.5f) * 120f; //0 to 0.5*180 = 60


                StartCoroutine(MoveCameraThenLaunch(soldierSelected, xAngle, yAngle));
            }

            SelectSoldier(null);
            inFPSmode = false;
            return;
        }

        //if we're not in FPS mode then we're aiming, and might go into FPS mode
        else if (soldierSelected && soldierSelected.projectile)
        {
            //note uses last frame
            Vector3 dir = soldierSelected.transform.position - clickPos;
            dir.y = 0f;

            if (dir.magnitude < soldierSelectionRange && dir.magnitude > soldierNoActionRange)
            {
                soldierSelected.vCamFPS.Priority = 12;
                inFPSmode = true;
            }
            else
                soldierSelected.ClearAiming();
        }
    }

    IEnumerator MoveCameraThenLaunch(Actor launchingAgent, float xAngle, float yAngle)
    {
        //wait until the overhead camera has the shot, then launch
        while (!CinemachineCore.Instance.IsLive(launchingAgent.vCamOverhead))
        {
            yield return new WaitForEndOfFrame();
        }

        yield return new WaitForSeconds(1f);

        FreezeRigidBodies(false);

        Transform projectile = launchingAgent.LaunchProjectile(xAngle);

        launchingAgent.vCamOverhead.LookAt = projectile;
        launchingAgent.ClearAiming();  //must be after launch projectile

        //vClearShotCam.LookAt = projectile;
        //foreach (CinemachineVirtualCamera vCam in vClearShotCam.GetComponentsInChildren<CinemachineVirtualCamera>())
        //vCam.LookAt = projectile

    }


    private void SelectSoldier(Actor newSelectedSoldier)
    {
        if (soldierSelected)
        {
            playerSelectButton.SetActive(true);
            soldierSelected.vCamOverhead.Priority = 11;
            soldierSelected.vCamFPS.Priority = 10;
            soldierSelected.ShowSelected = false;
            soldierSelected = null;

            //unfreeze - in case no new soldier selected
            FreezeRigidBodies(false);
        }

        if (newSelectedSoldier)
        {
            playerSelectButton.SetActive(false);
            soldierSelected = newSelectedSoldier;
            soldierSelected.ShowSelected = true;


            soldierSelected.vCamOverhead.Priority = 12;
            soldierSelected.vCamOverhead.LookAt = soldierSelected.transform;
            inFPSmode = false;

            FreezeRigidBodies(true);
        }



    }

    private void UpdateSoldierControl()
    {
        Vector3 dir = soldierSelected.transform.position - clickPos;
        dir.y = 0f;

        if (dir.magnitude > soldierSelectionRange)
        {
            //we're moving, start time and move
            FreezeRigidBodies(false); 
            soldierSelected.ClearAiming();
            soldierSelected.MoveIn(dir);

        }
        else
        {
            //we're either aiming or doing nothing, either way ensure time frozen
            FreezeRigidBodies(true);
            if (dir.magnitude > soldierNoActionRange)
                soldierSelected.AimIn(dir);
            else
                soldierSelected.ClearAiming();
        }
    }


    private void SetDestinationsTo(Vector3 destination)
    {
        foreach (Actor a in soldiers)
        {
            if (a != soldierSelected)
            {
                NavMeshAgent navMeshAgent = a.GetComponent<NavMeshAgent>();
                if (navMeshAgent)
                {
                    navMeshAgent.destination = destination;
                }
            }
        }
    }


    private void FreezeRigidBodies(bool makeFrozen)
    {

        rigidBodiesFrozen = makeFrozen;
        foreach (RigidbodyFreeze freeze in freezers)
            freeze.Freeze = rigidBodiesFrozen;

    }

    private void ToggleFreeze()
    {
        rigidBodiesFrozen = !rigidBodiesFrozen;
        foreach (RigidbodyFreeze freeze in freezers)
            freeze.Freeze = rigidBodiesFrozen;

    }

    private void UpdateCameraPanControl()
    {
    }

    private void UpdateFPSControl()
    {
    }

    public void GotoPlayerButtonClick()
    {
        if (inFPSmode)
            return;


            SelectSoldier(soldiers[0]);

    }

}
