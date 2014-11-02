using UnityEngine;
using System.Collections;

public class CameraSmoothFollow : MonoBehaviour {

	[SerializeField] private float dampTime = 0.15f;
	private Vector3 velocity = Vector3.zero;
	[SerializeField] private Transform target;
    [SerializeField]
    private float xMin = 0.0f, xMax = 0.0f, yMin = 0.0f, yMax = 0.0f;

	void Update () 
	{
		if (target)
		{
			Vector3 point = camera.WorldToViewportPoint(target.position);
			Vector3 delta = target.position - camera.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, point.z)); //(new Vector3(0.5, 0.5, point.z));
			Vector3 destination = transform.position + delta;
            destination.x = Mathf.Clamp(destination.x,xMin, xMax);
            destination.y = Mathf.Clamp(destination.y ,yMin, yMax);
			transform.position = Vector3.SmoothDamp(transform.position, destination, ref velocity, dampTime);          
		}
	}
}
