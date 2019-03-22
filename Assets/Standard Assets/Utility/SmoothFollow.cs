using UnityEngine;

namespace UnityStandardAssets.Utility
{
	public class SmoothFollow : MonoBehaviour
	{

		// The circuitTarget we are following
		[SerializeField]
		private Transform circuitTarget;
		// The distance in the x-z plane to the circuitTarget
		[SerializeField]
		private float distance = 10.0f;
		// the height we want the camera to be above the circuitTarget
		[SerializeField]
		private float height = 5.0f;

		[SerializeField]
		private float rotationDamping;
		[SerializeField]
		private float heightDamping;

		// Use this for initialization
		void Start() { }

		// Update is called once per frame
		void LateUpdate()
		{
			// Early out if we don't have a circuitTarget
			if (!circuitTarget)
				return;

			// Calculate the current rotation angles
			var wantedRotationAngle = circuitTarget.eulerAngles.y;
			var wantedHeight = circuitTarget.position.y + height;

			var currentRotationAngle = transform.eulerAngles.y;
			var currentHeight = transform.position.y;

			// Damp the rotation around the y-axis
			currentRotationAngle = Mathf.LerpAngle(currentRotationAngle, wantedRotationAngle, rotationDamping * Time.deltaTime);

			// Damp the height
			currentHeight = Mathf.Lerp(currentHeight, wantedHeight, heightDamping * Time.deltaTime);

			// Convert the angle into a rotation
			var currentRotation = Quaternion.Euler(0, currentRotationAngle, 0);

			// Set the position of the camera on the x-z plane to:
			// distance meters behind the circuitTarget
			transform.position = circuitTarget.position;
			transform.position -= currentRotation * Vector3.forward * distance;

			// Set the height of the camera
			transform.position = new Vector3(transform.position.x ,currentHeight , transform.position.z);

			// Always look at the circuitTarget
			transform.LookAt(circuitTarget);
		}
	}
}