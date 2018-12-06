using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ItemType
{
    weaopon, ammunition
}

public class Item : MonoBehaviour
{

    public bool canBePickedUp;


    [SerializeField]
    private GameObject _owner;
    public GameObject Owner
    {
        get { return _owner; }
        set
        {
            //note we need to deal with null values
            if (_owner)
                IgnoreCollisions(Owner, false);

            _owner = value;
            if (value)
                IgnoreCollisions(Owner, true);
        }
    }

    private FixedJoint stickTo;



    public void IgnoreCollisions(GameObject obj, bool ignore)
    {
        foreach (Collider thisCol in GetComponentsInChildren<Collider>())
            foreach (Collider otherCol in obj.GetComponentsInChildren<Collider>())
                Physics.IgnoreCollision(thisCol, otherCol, ignore);
    }



    public void Grabbed(GameObject newOwner, Transform holdPoint = null, Vector3 gripOffset = new Vector3())
    {
        if (holdPoint)
        {
            transform.position = holdPoint.position + gripOffset;
            transform.forward = holdPoint.forward;
        }

        Owner = newOwner;

        //stick ourselves to our new owner
        if (!stickTo)
            stickTo = gameObject.AddComponent<FixedJoint>();

        Rigidbody stickToRb = newOwner.GetComponentInParent<Rigidbody>();
        if (stickToRb)
            stickTo.connectedBody = stickToRb;
            
    }



    public void Release()
    {
        if (stickTo)
            Destroy(stickTo);

        StartCoroutine(CanBePickedUpIn(1));
    }

    private IEnumerator CanBePickedUpIn(float secs)
    {
        canBePickedUp = false;
        yield return new WaitForSeconds(secs);
        canBePickedUp = true;
    }


}
