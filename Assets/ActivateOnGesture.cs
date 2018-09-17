using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HandGesture;
using Leap;
using Leap.Unity;

public class ActivateOnGesture : MonoBehaviour {
    public HandPoseType handPoseType;
    public Mapbox.Unity.Map.AbstractMap attachment;
    public bool isLeftHanded = false;
    private GestureRecognitionController gestureRecognitionController;
	private float startAngle = 0;
	private bool action = false;
	public float actionType = 0;
	//private gestureStartPos = 
    void Start () {
        gestureRecognitionController = FindObjectOfType<GestureRecognitionController>();
    }

    // Update is called once per frame
    void Update()
    {
		action = false;
        if(!gestureRecognitionController.isLeftHandTracked && !gestureRecognitionController.isRightHandTracked)
        {
            return;
        }

        HandPoseType pose = gestureRecognitionController.GetDominantHandPose(isLeftHanded);
        if (pose.Equals(handPoseType))
        {
			action = true;
            GrabAction();
        }
		if (!action)
		{
			startAngle = attachment.transform.eulerAngles.y;
		}
    }

	void GrabAction()
    {
		//Debug.Log ("grabbing");
		//attachment.transform.position += new Vector3(gestureRecognitionController.GetDominantVelocity (isLeftHanded).x, 0.0f, gestureRecognitionController.GetDominantVelocity(isLeftHanded).z);



		//Debug.Log (gestureRecognitionController.GetDominantHand (false).PalmPosition.ToVector3 ());
		//if (!(gestureRecognitionController.GetHandPinchStartPosition (gestureRecognitionController.GetDominantHand (false)) == Vector3.negativeInfinity))

		GameObject mainCamera = GameObject.FindWithTag ("MainCamera");
		Vector3 startPinch = gestureRecognitionController.GetHandPinchStartPosition (gestureRecognitionController.GetDominantHand (false)) - mainCamera.transform.position;
		Vector3 currentHandPosition = gestureRecognitionController.GetDominantHand (false).PalmPosition.ToVector3 () - mainCamera.transform.position;
		if(!float.IsNegativeInfinity(startPinch.x))
		{
			if (actionType == 0)
			{
				//Vector3 velocity = Quaternion.AngleAxis((mainCamera.transform.position), Vector3.up) * gestureRecognitionController.GetDominantVelocity(isLeftHanded);
				//float angle = Quaternion.Angle (mainCamera.transform.rotation, attachment.transform.rotation);
				//if (mainCamera.transform.rotation > attachment.transform.rotation)
				//{
				//	angle = -angle;
				//}

				float angle = mainCamera.transform.localEulerAngles.y - attachment.transform.localEulerAngles.y;
				Debug.Log ("" + mainCamera.transform.localEulerAngles.y + " : " + attachment.transform.localEulerAngles.y + " : " + angle);

				Vector3 movement = Quaternion.AngleAxis (angle, Vector3.up) * gestureRecognitionController.GetDominantVelocity (isLeftHanded);
				Mapbox.Utils.Vector2d mapboxMovement = new Mapbox.Utils.Vector2d(movement.z, movement.x);
				attachment.UpdateMap (attachment.CenterLatitudeLongitude - (mapboxMovement * 0.001f), attachment.Zoom);
			}
			else if (actionType == 1)
			{
				
			}
			else if (actionType == 2)
			{
				float direction = startPinch.x - currentHandPosition.x;
				float angle = Vector2.SignedAngle(new Vector2(currentHandPosition.x, currentHandPosition.z), new Vector2(startPinch.x, startPinch.z));
				float currentAngle = attachment.transform.eulerAngles.y;
				Vector3 targetLocation = (Quaternion.Euler (0, startAngle + angle - currentAngle, 0) * (attachment.transform.position - mainCamera.transform.position))  + mainCamera.transform.position;
				attachment.transform.SetPositionAndRotation(targetLocation , Quaternion.Euler(0, (startAngle + angle), 0));
			}

			//Debug.Log (direction);

			//if (attachment.transform.eulerAngles.y / 180 < 1) {
			//	currentAngle = attachment.transform.eulerAngles.y % 180;
			//} else {
			//	currentAngle = -attachment.transform.eulerAngles.y % 180;
			//}
			//Vector3 targetLocation = attachment.transform.position;

			//attachment.transform.rotation = Quaternion.Euler(0, (startAngle + angle), 0);

			//Debug.Log ((startAngle + angle - currentAngle));
			//Debug.Log ("" + startAngle + " + " + angle + " - " + currentAngle);

			/*/if (direction <= 0)
			{
				attachment.transform.rotation = Quaternion.Euler(0, (startAngle + angle - currentAngle) % 180, 0);
				//Vector3 targetLocation = (Quaternion.Euler (0, startAngle - angle - currentAngle, 0) * (attachment.transform.position - cameraPosition))  + cameraPosition - attachment.transform.position;
				//attachment.transform.SetPositionAndRotation(targetLocation , Quaternion.Euler(0, (startAngle + angle - currentAngle) % 180, 0));

				Debug.Log ((startAngle + angle - currentAngle) % 180);
				//attachment.transform.RotateAround (cameraPosition, Vector3.up, startAngle - angle - currentAngle);
				Debug.Log ("" + startAngle + " + " + angle + " - " + currentAngle);
			}
			else
			{
				attachment.transform.rotation = Quaternion.Euler(0, (startAngle - angle - currentAngle) % 180, 0);
				//Vector3 targetLocation = (Quaternion.Euler (0, startAngle + angle - currentAngle, 0) * (attachment.transform.position - cameraPosition)) + cameraPosition - attachment.transform.position;
				//attachment.transform.SetPositionAndRotation(targetLocation , Quaternion.Euler(0, (startAngle - angle - currentAngle) % 180, 0));
				Debug.Log ((startAngle - angle - currentAngle) % 180);
				//attachment.transform.RotateAround (cameraPosition, Vector3.up, startAngle + angle - currentAngle);
				Debug.Log ("" + startAngle + " - " + angle + " - " + currentAngle);
			}/*/
		}

		//Vector2 startPinch = (Vector2)gestureRecognitionController.GetHandPinchStartPosition (gestureRecognitionController.GetDominantHand (false));
		//if (!float.IsNegativeInfinity (startPinch.x))
		//{

		//}
    }
}