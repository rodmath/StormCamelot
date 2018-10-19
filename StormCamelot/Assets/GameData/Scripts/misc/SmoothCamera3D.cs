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
    private float baseY;

	private void Start()
	{
        baseY = transform.position.y;
	}

	void Update()
    {

        if (target)
        {
            Vector3 point = Camera.main.WorldToViewportPoint(target.position);
            float dist2Cam = (Camera.main.transform.position - target.position).magnitude;
            Vector3 delta = target.position + followOffset - Camera.main.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, point.z));
            Vector3 destination = transform.position + delta;

            if (type == SmoothFollowType.OnlyX)
                destination.y = transform.position.y;
            if (type == SmoothFollowType.OnlyY)
                destination.x = transform.position.x;

            Vector3 move2Pos = Vector3.SmoothDamp(transform.position, destination, ref velocity, dampTime);
            transform.position = move2Pos;

            if (useVelocityScaling) VelocityScaling();
        }
    }

    private void VelocityScaling()
    {
        float speed = velocity.magnitude;
        float normalisedSpeed =  velocity.magnitude / speedForMinZoom;
        float newY = baseY - (zoomRange * normalisedSpeed.Clamp(0f, 1f));


        Vector3 destination = new Vector3(transform.position.x, newY, transform.position.y);
        Vector3 move2Pos = Vector3.Slerp(transform.position, destination, 0.1f);
        transform.position = move2Pos;
    }
}