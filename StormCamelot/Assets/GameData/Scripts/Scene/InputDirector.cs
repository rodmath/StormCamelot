using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class InputDirector : MonoBehaviour
{
    public float soldierSelectionRange = 4f;

    private List<SoldierAgent> soldiers;
    private SoldierAgent soldierSelected;
    private SmoothCamera2D smoothCamera;

    private void Start()
    {
        soldiers = Object.FindObjectsOfType<SoldierAgent>().ToList();
        smoothCamera = Object.FindObjectOfType<SmoothCamera2D>();
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            StartClick();
        }
        else if (Input.GetMouseButtonDown(0))
        {
            if (soldierSelected)
                UpdateSoldierControl();
            else
                UpdateCameraPanControl();
        }
    }

    private void StartClick()
    {
        ////Find out where we have clicked, then test to see the nearest Soldier
        Plane plane = new Plane(Vector3.up, transform.position);
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        // plane.Raycast returns the distance from the ray start to the hit point
        float rayDist;
        float dist;
        float minDist = float.PositiveInfinity;
        if (plane.Raycast(ray, out rayDist))
        {
            Vector3 clickPos = ray.GetPoint(rayDist);
            foreach (SoldierAgent soldier in soldiers)
            {
                dist = (clickPos - soldier.transform.position).magnitude;
                if (dist < soldierSelectionRange)
                {
                    if (dist < minDist)
                    {
                        minDist = dist;
                        soldierSelected = soldier;
                        Debug.Log("Soldier selected: " + soldier.name);
                        smoothCamera.target = soldier.transform;
                    }
                }
            }
        }

    }

    private void UpdateSoldierControl()
    {

    }

    private void UpdateCameraPanControl()
    {
    }
}
