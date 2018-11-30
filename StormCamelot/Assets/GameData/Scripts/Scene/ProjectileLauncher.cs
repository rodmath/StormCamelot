using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileLauncher : MonoBehaviour {
    [Header("Config")]
    public Transform launchPostion;
    public GameObject projectilePrefab;

    [Header("Attributes")]
    public int projectilesHeld;
    public float power;


    public Transform launch(GameObject owner, Vector3 aimingVector, float vAngle)
    {
        GameObject newProjObj = Instantiate(projectilePrefab);
        newProjObj.transform.position = launchPostion.position;
        newProjObj.transform.forward = aimingVector;
        //newProjObj.transform.Rotate(-vAngle, 0f, 0f, Space.Self);


        Projectile newProj =  newProjObj.GetComponent<Projectile>();
        newProj.Grabbed(owner);
        newProj.Launch(power);

        return newProjObj.transform;
    }


}
