using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WizardOfOz : MonoBehaviour {
	public Mapbox.Unity.Map.AbstractMap attachment;
	public Transform annotationPrefab;
	public float yPosition;
	public float zoom;
	GameObject mainCamera = GameObject.FindWithTag ("MainCamera");
	public Material selectedMaterial;
	List<Transform> annotations;
	// Use this for initialization
	void Start () {
		annotations = new List<Transform>();
	}
	
	// Update is called once per frame
	void Update () {
		//Debug.Log ("hello");
		Vector3 movement = new Vector3();
		if (Input.GetKey(KeyCode.W))
		{
			//North
			movement.z = -0.0001f;
		}
		if (Input.GetKey(KeyCode.S))
		{
			//South
			movement.z = 0.0001f;
		}
		if (Input.GetKey(KeyCode.A))
		{
			//West
			movement.x = 0.0001f;
		}
		if (Input.GetKey(KeyCode.D))
		{
			//East
			movement.x = -0.0001f;
		}

		float angle = 0.0f;
		if (Input.GetKey (KeyCode.Q))
		{
			//Anti-Clockwise
			angle = 1.0f;
		}
		if (Input.GetKey (KeyCode.E))
		{
			//Clockwise
			angle = -1.0f;
		}

		float height = 0.0f;
		if (Input.GetKey (KeyCode.UpArrow))
		{
			//Up
			height = 0.01f;
		}
		if (Input.GetKey (KeyCode.DownArrow))
		{
			//Down
			height = -0.01f;
		}

		float zoomChange = 0.0f;
		if (Input.GetKey (KeyCode.LeftArrow))
		{
			//Out
			zoomChange = -0.01f;
		}
		if (Input.GetKey (KeyCode.RightArrow))
		{
			//In
			zoomChange = 0.01f;
		}

		if (Input.GetKeyDown (KeyCode.Space))
		{
			Vector3 annotationPosition = new Vector3(Random.Range(gameObject.transform.position.x - 2.5f, gameObject.transform.position.x + 2.5f), yPosition + 0.1f, Random.Range(gameObject.transform.position.z - 2.5f, gameObject.transform.position.z + 2.5f));
			annotations.Add((Transform)Instantiate(annotationPrefab, annotationPosition, Quaternion.identity));
		}

		if (Input.GetKeyDown (KeyCode.M))
		{
			annotations [Random.Range (0, annotations.Count)].GetComponent<MeshRenderer>().material = selectedMaterial;
		}

		zoom += zoomChange;

		Mapbox.Utils.Vector2d mapboxMovement = new Mapbox.Utils.Vector2d (movement.z, movement.x);
		attachment.UpdateMap (attachment.CenterLatitudeLongitude - (mapboxMovement), zoom);

		yPosition += height;

		Vector3 temp = new Vector3 (gameObject.transform.position.x, yPosition, gameObject.transform.position.z);
		gameObject.transform.position = temp;

		float currentAngle = attachment.transform.eulerAngles.y;
		//Vector3 targetLocation = (Quaternion.Euler (0, startAngle + angle - currentAngle, 0) * (attachment.transform.position - mainCamera.transform.position))  + mainCamera.transform.position;
		Vector3 targetLocation = (Quaternion.Euler (0, currentAngle + angle, 0) * (attachment.transform.position));
		attachment.transform.SetPositionAndRotation(targetLocation , Quaternion.Euler(0, (currentAngle + angle), 0));

		foreach (Transform annotation in annotations)
		{
			annotation.transform.position = new Vector3(annotation.transform.position.x + (movement.x*100), yPosition + 0.1f, annotation.transform.position.z + (movement.z*100));
		}
	}
}
