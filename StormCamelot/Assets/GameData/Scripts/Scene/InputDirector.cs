using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Chronos;

public class InputDirector : MonoBehaviour
{
    public float soldierSelectionRange = 3f;

    private List<SoldierAgent> soldiers;
    private SoldierAgent soldierSelected;
    private SmoothCamera3D smoothCamera;
    private Clock rootClock;

    private Vector3 clickPos;



    private void Start()
    {
        soldiers = Object.FindObjectsOfType<SoldierAgent>().ToList();
        smoothCamera = Object.FindObjectOfType<SmoothCamera3D>();
        rootClock = Timekeeper.instance.Clock("Root");
    }

    private void Update()
    {
        if (Input.GetMouseButton(0))
        {
            SetClickPoint();

            if (Input.GetMouseButtonDown(0))
                StartClick();
            else if (Input.GetMouseButtonUp(0))
                rootClock.localTimeScale = 1;
            else
            {
                //mouse is down, but not being moved up or down this frame
                if (soldierSelected)
                    UpdateSoldierControl();
                else
                    UpdateCameraPanControl();
            }
        }
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
        ////Find out where we have clicked
        Plane plane = new Plane(Vector3.up, transform.position);
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        float rayDist;
        if (plane.Raycast(ray, out rayDist))
        {
            clickPos = ray.GetPoint(rayDist);
        }
    }

    private void StartClick()
    {
        SoldierAgent soldierToSelect = null;
        float dist;
        float minDist = soldierSelectionRange;
        foreach (SoldierAgent soldier in soldiers)
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


    private void SelectSoldier(SoldierAgent soldierToSelect)
    {
        Debug.Log("Soldier selected: " + soldierToSelect.name);
        if (soldierSelected)
            soldierSelected.showDebug = false;

        soldierSelected = soldierToSelect;
        soldierSelected.showDebug = true;

        rootClock.localTimeScale = 0f;
        smoothCamera.target = soldierSelected.transform;
    }

    private void UpdateSoldierControl()
    {
        Vector3 dir = soldierSelected.transform.position - clickPos;
        dir.y = 0f;

        if (dir.magnitude > soldierSelectionRange)
        {
            rootClock.localTimeScale = 1f;
            soldierSelected.ClearAiming();
            soldierSelected.MoveIn(dir);

        }
        else
        {
            rootClock.localTimeScale = 0f;
            soldierSelected.AimIn(dir);
        }
    }

    private void UpdateCameraPanControl()
    {
    }
}
