using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Chronos;
using Vectrosity;

public class InputDirector : MonoBehaviour
{
    public float soldierSelectionRange = 3f;
    public float soldierNoActionRange = 1f;

    private Camera overheadCam;
    private Camera fpsCam;
    private List<Agent> soldiers;
    private Agent soldierSelected;
    private SmoothCamera3D smoothCamera;
    private Clock rootClock;
    private bool inFPSmode = false;

    private Vector3 clickPos;



    private void Start()
    {
        overheadCam = Camera.main;
        soldiers = Object.FindObjectsOfType<Agent>().ToList();
        smoothCamera = Object.FindObjectOfType<SmoothCamera3D>();
        rootClock = Timekeeper.instance.Clock("Root");

        foreach (Agent s in soldiers)
        {
            s.SetupInput(soldierNoActionRange, soldierSelectionRange, soldierSelectionRange);
        }

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
            else if (Input.GetKeyDown("2"))
                SelectSoldier(soldiers[1]);
            else if (Input.GetKeyDown("3"))
                SelectSoldier(soldiers[2]);
            else if (Input.GetKeyDown("4"))
                SelectSoldier(soldiers[3]);
        }
    }

    private void SetClickPoint()
    {
        if (inFPSmode)
            clickPos = fpsCam.ScreenToViewportPoint(Input.mousePosition);
        else
        {
            Plane plane = new Plane(Vector3.up, transform.position);
            Ray ray = overheadCam.ScreenPointToRay(Input.mousePosition);
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


        Agent soldierToSelect = null;
        float dist;
        float minDist = soldierSelectionRange;
        foreach (Agent soldier in soldiers)
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

        if (soldierToSelect)
            SelectSoldier(soldierToSelect);
        else
            Debug.Log("Nothing selected");
    }


    private void EndClick()
    {
        if (inFPSmode)
        {
            rootClock.localTimeScale = 1f;
            Transform projectile = soldierSelected.LaunchProjectile();
            smoothCamera.target = projectile;
            soldierSelected.ClearAiming();
            fpsCam.enabled = false;
            overheadCam.enabled = true;
            inFPSmode = false;

            SelectSoldier(null);
            return;
        }


        if (soldierSelected)
        {
            //note uses last frame
            Vector3 dir = soldierSelected.transform.position - clickPos;
            dir.y = 0f;

            if (dir.magnitude < soldierSelectionRange && dir.magnitude > soldierNoActionRange)
            {
                fpsCam.enabled = true;
                overheadCam.enabled = false;
                inFPSmode = true;
            }
            else
                soldierSelected.ClearAiming();
        }
    }


    private void SelectSoldier(Agent newSelectedSoldier)
    {
        if (soldierSelected)
            soldierSelected.ShowSelected = false;

        if (newSelectedSoldier)
        {
            soldierSelected = newSelectedSoldier;
            soldierSelected.ShowSelected = true;
            fpsCam = soldierSelected.GetComponentInChildren<Camera>();
            rootClock.localTimeScale = 0.001f;
            smoothCamera.target = soldierSelected.transform;
        }
        else
            rootClock.localTimeScale = 1f;



    }

    private void UpdateSoldierControl()
    {
        Vector3 dir = soldierSelected.transform.position - clickPos;
        dir.y = 0f;

        if (dir.magnitude > soldierSelectionRange)
        {
            //we're moving, start time and move
            rootClock.localTimeScale = 1f;
            soldierSelected.ClearAiming();
            soldierSelected.MoveIn(dir);

        }
        else
        {
            //we're not moving, stop time 
            rootClock.localTimeScale = 0.001f;

            if (dir.magnitude > soldierNoActionRange)
                soldierSelected.AimIn(dir);
            else
                soldierSelected.ClearAiming();
        }
    }

    private void UpdateCameraPanControl()
    {
    }

    private void UpdateFPSControl()
    {
    }
}
