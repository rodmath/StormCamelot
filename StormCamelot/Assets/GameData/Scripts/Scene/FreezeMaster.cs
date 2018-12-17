using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FreezeMaster : MonoBehaviour {

    public bool IsFrozen { get { return rigidBodiesFrozen; }}

    private bool rigidBodiesFrozen = false;
    private List<RigidbodyFreeze> freezers = new List<RigidbodyFreeze>();

    public void RegisterMe(RigidbodyFreeze freeze)
    {
        if (!freezers.Contains(freeze))
            freezers.Add(freeze);
    }

    public void UnregisterMe(RigidbodyFreeze freeze)
    {
        if (freezers.Contains(freeze))
            freezers.Remove(freeze);
    }





    public void FreezeRigidBodies(bool makeFrozen)
    {

        rigidBodiesFrozen = makeFrozen;
        foreach (RigidbodyFreeze freeze in freezers)
            freeze.Freeze = rigidBodiesFrozen;

    }

    public void ToggleFreeze()
    {
        rigidBodiesFrozen = !rigidBodiesFrozen;
        foreach (RigidbodyFreeze freeze in freezers)
            freeze.Freeze = rigidBodiesFrozen;

    }

}
