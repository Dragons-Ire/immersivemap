using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Leap;
using HandGesture;
using Leap.Unity;

public class AttachOnFinger : MonoBehaviour {
    public GameObject indicator;
    public Finger.FingerType fingerType;
    public bool showOnPalm;
    public bool onNonDominantHand = false;
    public Vector3 offset;

    private GestureRecognitionController gestureRecognitionController;
	// Use this for initialization
	void Start () {
        gestureRecognitionController = FindObjectOfType<GestureRecognitionController>();
	}
	
	// Update is called once per frame
	void Update () {
        Hand hand = gestureRecognitionController.GetDominantHand(onNonDominantHand);
        if (hand == null) {
            indicator.SetActive(false);
        }
        else
        {
            indicator.SetActive(false);
            if (showOnPalm)
            {
                indicator.transform.position = hand.PalmPosition.ToVector3() + offset;
            }
            else
            {
                Finger finger = HandPoseUtility.GetFinger(hand, fingerType);
                indicator.transform.position = finger.TipPosition.ToVector3() + offset;
            }

        } 
    }
}
