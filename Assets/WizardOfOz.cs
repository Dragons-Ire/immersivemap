﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using sl;

public class WizardOfOz : MonoBehaviour {
	public Mapbox.Unity.Map.AbstractMap attachment;
	public float yPosition;
	public float zoom;
    GameObject mainCamera;
	public Material selectedMaterial;
	public List<GameObject> annotations;
    public Camera zed;
    int count;
    float timer;
    //InitParameters init_params;
    // Use this for initialization
    void Start () {
        count = -1;
        mainCamera = GameObject.FindWithTag("MainCamera");
        //init_params.resolution = RESOLUTION_HD720;
        //init_params.coordinateSystem = COORDINATE_SYSTEM_RIGHT_HANDED_Y_UP;
        //init_params.coordinateUnit = UNIT_METER;
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
            timer = 3;
            count += 1;
            Component light = annotations[0].GetComponent("Light");
            light.GetType().GetProperty("enabled").SetValue(light, true, null);
            Component halo = annotations[0].GetComponent("Halo");
            halo.GetType().GetProperty("enabled").SetValue(halo, true, null);
            annotations[0].SetActive(true);
			//Vector3 annotationPosition = new Vector3(Random.Range(gameObject.transform.position.x - 2.5f, gameObject.transform.position.x + 2.5f), yPosition + 0.1f, Random.Range(gameObject.transform.position.z - 2.5f, gameObject.transform.position.z + 2.5f));
			//annotations.Add((Transform)Instantiate(annotationPrefab, annotationPosition, Quaternion.identity));
		}

		if (Input.GetKeyDown (KeyCode.M))
		{
            //foreach(GameObject annotation in annotations)
            //{
            //    Component light = annotation.GetComponent("Light");
            //    light.GetType().GetProperty("enabled").SetValue(light, false, null);
            //    Component halo = annotation.GetComponent("Halo");
            //    halo.GetType().GetProperty("enabled").SetValue(halo, false, null);
            //    annotation.SetActive(false);
            //}
            //count = -1;
			//annotations [Random.Range (0, annotations.Count)].GetComponent<MeshRenderer>().material = selectedMaterial;
		}

        timer -= Time.deltaTime;
        if(timer <= 0)
        {
            Component light = annotations[0].GetComponent("Light");
            light.GetType().GetProperty("enabled").SetValue(light, false, null);
            Component halo = annotations[0].GetComponent("Halo");
            halo.GetType().GetProperty("enabled").SetValue(halo, false, null);
        }

		zoom += zoomChange;

		Mapbox.Utils.Vector2d mapboxMovement = new Mapbox.Utils.Vector2d (movement.z, movement.x);
		attachment.UpdateMap (attachment.CenterLatitudeLongitude - (mapboxMovement), zoom);

		yPosition += height;

		Vector3 temp = new Vector3 (gameObject.transform.position.x, yPosition, gameObject.transform.position.z);
		gameObject.transform.position = temp;

		float currentAngle = attachment.transform.eulerAngles.y;
        Vector3 zedMiniPosition = new Vector3(zed.gameObject.transform.position.x, attachment.transform.position.y, zed.gameObject.transform.position.z);
        Vector3 targetLocation = (Quaternion.Euler (0, angle, 0) * (attachment.transform.position - zedMiniPosition))  + zedMiniPosition;
		attachment.transform.SetPositionAndRotation(targetLocation, Quaternion.Euler(0, (currentAngle + angle), 0));
	}
}
