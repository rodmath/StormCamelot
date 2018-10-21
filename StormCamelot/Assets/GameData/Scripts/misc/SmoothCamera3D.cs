using UnityEngine;
using System.Collections;




public class SmoothCamera3D : MonoBehaviour
{
    public Transform target;
    public SmoothFollowType type = SmoothFollowType.AllDirections;
    public Vector3 followOffset;
    public float dampTime = 0.15f;


    [Header("Use velocity to scale zoom")]
    public bool useVelocityScaling;
    public float zoomRange;
    public float speedForMinZoom;

    private Vector3 velocity = Vector3.zero;
    private float followHeight;

	private void Start()
	{
        //Find out where Camera is pointing on X-Z plaun
        Plane plane = new Plane(Vector3.up, transform.position);
        Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        float rayDist;

        //if camera is pointing at plane (so ray hits it) use that to get follow offset, otherwise just use it's offset from origin
        if (plane.Raycast(ray, out rayDist))
            followOffset = Camera.main.transform.position - ray.GetPoint(rayDist);
        else
            followOffset = Camera.main.transform.position;

	}

	void Update()
    {

        if (target)
        {
            Vector3 destination = target.position + followOffset;

            //Vector3 point = Camera.main.WorldToViewportPoint(target.position);
            //float dist2Cam = (Camera.main.transform.position - target.position).magnitude;
            //Vector3 delta = target.position + followOffset - Camera.main.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, point.z));
            //Vector3 destination = transform.position + delta;

            //if (type == SmoothFollowType.OnlyX)
            //    destination.y = transform.position.y;
            //if (type == SmoothFollowType.OnlyY)
            //destination.x = transform.position.x;

            Vector3 move2Pos = Vector3.SmoothDamp(transform.position, destination, ref velocity, dampTime);
            transform.position = move2Pos;

        }
    }
}