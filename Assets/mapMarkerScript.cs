using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class mapMarkerScript : MonoBehaviour {

    public bool selected;
    public Material green;
    public Material red;
    public GameObject camera;

	// Use this for initialization
	void Start () {

	}
	
	// Update is called once per frame
	void Update () {
        if (selected)
        {
            gameObject.GetComponent<MeshRenderer>().material = green;
        }
        else
        {
            gameObject.GetComponent<MeshRenderer>().material = red;
        }
        //gameObject.transform.LookAt(camera.transform);
        //gameObject.transform.rotation.Set(0.0f, gameObject.transform.rotation.y, 0.0f, 0.0f);
	}
}
