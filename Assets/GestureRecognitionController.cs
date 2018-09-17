using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Leap;
using Leap.Unity;
using System;
using UnityEngine.Events;

namespace HandGesture
{

    public enum RemoteTouchState
    {
        Neutral,
        Hover,
        Touch
    }

    [System.Serializable]
    public struct RemoteTouchProperty
    {
        public RemoteTouchState state;
        public Vector3 touchStartPosition;
        public float currentDistance;
    }

    public class GestureRecognitionController : MonoBehaviour
    {
        [Header("List of Uni and Bi Manual Gestures ")]
        public List<SimpleUnimanualGestureTemplate> simpleUnimanualGestures = new List<SimpleUnimanualGestureTemplate>();
        public List<SimpleBimanualGestureTemplate> simpleBimanualGestures = new List<SimpleBimanualGestureTemplate>();

        [Header("Settings")]
        public bool overridePinchStatus;
        [Tooltip("If true, the pinch and grab pose will be separated")]
        public float maxPinchStrength = 0.8f;
        public float minPinchStrength = 0.7f;
        public UnimanualData leftUnimanualData;
        public UnimanualData rightUnimanualData;
        public BimanualData bimanualData;
        public SimpleGestureRecognition leftGestureRecognition;
        public SimpleGestureRecognition bimanualRecognition;
        public SimpleGestureRecognition rightGestureRecognition;
        [Tooltip("Default distance for touch state: distance >= avg -> touch, distance < avg - stddev -> hover, otherwise neutral")]
        public float averageTouchDistance = 0.4633f;
        public float stdDevTouchDistance = 0.1493f;

        [Tooltip("The max time (s) between start and end hand pose to form a gesture")]
        public float gestureTimeRate;

        [Header("Active hand pose")]
        public bool open;
        public bool paper;
        public bool point;
        public bool rock;
        public bool scissorOpen;
        public bool scissorClosed;
        public bool thumb;
        public bool promise;
        public bool metalNoThumb;
        public bool metal;
        public bool middleFinger;
        public bool pistol;

        public UnityEvent OnROpenBegin;
        public UnityEvent OnROpenStay;
        public UnityEvent OnROpenEnd;
        public UnityEvent OnRPaperBegin;
        public UnityEvent OnRPaperStay;
        public UnityEvent OnRPaperEnd;



        [Header("Public fields")]
        public bool debug;
        public Hand leftHand;
        public Hand rightHand;
        public HandPoseType leftHandPose;
        public HandPoseType rightHandPose;
        public UnimanualGestureType leftHandGesture;
        public UnimanualGestureType rightHandGesture;
        public BimanualGestureType bimanualGesture;
        public bool isLeftHandTracked;
        public bool isRightHandTracked;
        public bool isLeftHandPinching;
        public bool isRightHandPinching;

        public Vector3 rightPalmVelocity;
        public Vector3 leftPalmVelocity;

        public Vector3 rightPalmStabilizedVelocity;
        public Vector3 leftPalmStabilizedVelocity;
        private Vector3 rightPalmPreviousVelocity = Vector3.negativeInfinity;
        private Vector3 leftPalmPreviousVelocity = Vector3.negativeInfinity;
        private float alpha = 0.25f;

        public Vector3 leftHandPinchStart = Vector3.negativeInfinity;
        public Vector3 righHandPinchStart = Vector3.negativeInfinity;

        public RemoteTouchProperty rightHandRemoteTouch;
        public RemoteTouchProperty leftHandRemoteTouch;

        private Vector3 rightHandLastPosition = Vector3.negativeInfinity;
        private Vector3 leftHandPreviousPosition = Vector3.negativeInfinity;
        private LeapServiceProvider leapServiceProvider;
        private HandPoseRecognition handPoseRecognition;
        private string debugText;
        private Vector3 startPosition;
        private HandPoseType startPose = HandPoseType.Pistol;
        private HandPoseType stopPose = HandPoseType.Rock;
        private bool isGesturingDebug = false;
        private string text;
        private HandPoseType lastHandPose;

        void Start()
        {
            handPoseRecognition = new HandPoseRecognition();
            leapServiceProvider = FindObjectOfType<LeapServiceProvider>();
            leftGestureRecognition = new SimpleGestureRecognition(gestureTimeRate);
            rightGestureRecognition = new SimpleGestureRecognition(gestureTimeRate);
            bimanualRecognition = new SimpleGestureRecognition(gestureTimeRate);
            leftHandPose = HandPoseType.Unknown;
            rightHandPose = HandPoseType.Unknown;

            InitUnimanualGestures();
            InitBimanualGestures();
        }

        void DebugText(string txt, bool append)
        {
            if (append)
            {
                debugText += "\n" + txt;

            }
            else
            {
                debugText = txt;
            }
        }

        public float GetHandsDistance()
        {
            if(isLeftHandTracked && isRightHandTracked)
            {
                return Vector3.Distance(rightHand.PalmPosition.ToVector3(), leftHand.PalmPosition.ToVector3());
            }
            else
            {
                return float.NegativeInfinity;
            }
        }

        public bool IsHandPinching(Hand h)
        {
            return (h.IsLeft) ? isLeftHandPinching : isRightHandPinching;
        }

        public Vector3 GetHandPinchStartPosition(Hand h)
        {
            return (h.IsLeft) ? leftHandPinchStart : righHandPinchStart;
        }

        public Vector3 GetHandVelocity(Hand h)
        {
            return (h.IsLeft) ? leftPalmVelocity : rightPalmVelocity;
        }

        public Vector3 GetDominantVelocity(bool isLeftHanded)
        {
            return (isLeftHanded) ? leftPalmVelocity : rightPalmVelocity;
        }

        public Hand GetDominantHand(bool isLeftHanded)
        {
            if (!isLeftHanded)
            {
                return rightHand;
            }
            else
            {
                return leftHand;
            }
        }

        public UnimanualData GetDominantHandData(bool isLeftHanded)
        {
            if (!isLeftHanded)
            {
                return rightUnimanualData;
            }
            else
            {
                return leftUnimanualData;
            }
        }

        public UnimanualGestureType GetDominantGesture(bool isLeftHanded)
        {
            if (!isLeftHanded)
            {
                return rightHandGesture;
            }
            else
            {
                return leftHandGesture;
            }
        }

        public HandPoseType GetDominantHandPose(bool isLeftHanded)
        {
            if (!isLeftHanded)
            {
                return rightHandPose;
            }
            else
            {
                return leftHandPose;
            }
        }

        private void OnGUI()
        {
            GUI.Label(new Rect(0, 0, 1500, 1500), debugText);
        }

        private void Update()
        {
            VelocityUpdate();
        }

        private void FixedUpdate()
        {
           // RemoteTouchUpdate();
            PoseGestureUpdate();
            DebugUpdate();
        }

        internal Hand GetDominantHand(object isLeftHanded)
        {
            throw new NotImplementedException();
        }

        public bool IsMovingRight(Vector3 v)
        {
            Vector3 cam = transform.InverseTransformDirection(Camera.main.transform.forward);
            bool r = false;
            if (cam.x > 0 && cam.z < 0 && v.x < 0 && v.z < 0) r = true;
            if (cam.x > 0 && cam.z > 0 && v.x > 0 && v.z < 0) r = true;
            if (cam.x < 0 && cam.z > 0 && v.x > 0 && v.z > 0) r = true;
            if (cam.x < 0 && cam.z < 0 && v.x < 0 && v.z > 0) r = true;

            return r;
        }

        Vector3 LowPass(Vector3 x, Vector3 xPrev, float a)
        {
            return (xPrev * a) + (x * (1f - a));
        }

        void VelocityUpdate()
        {
            if (isLeftHandTracked)
            {
                leftHandPreviousPosition = (leftHandPreviousPosition.Equals(Vector3.negativeInfinity)) ? leftHand.PalmPosition.ToVector3() : leftHandPreviousPosition;
                leftPalmVelocity = (leftHand.PalmPosition.ToVector3() - leftHandPreviousPosition) / Time.deltaTime;
                leftPalmStabilizedVelocity = (leftPalmPreviousVelocity == Vector3.negativeInfinity) ? leftPalmVelocity : LowPass(leftPalmVelocity, leftPalmPreviousVelocity, alpha);
                leftHandPreviousPosition = leftHand.PalmPosition.ToVector3();
                leftPalmPreviousVelocity = leftPalmVelocity;
            }
            else
            {
                leftHandPreviousPosition = Vector3.negativeInfinity;
                leftPalmVelocity = Vector3.zero;
                leftPalmStabilizedVelocity = Vector3.zero;
            }

            if (isRightHandTracked)
            {
                rightHandLastPosition = (rightHandLastPosition.Equals(Vector3.negativeInfinity)) ? rightHand.PalmPosition.ToVector3() : rightHandLastPosition;
                rightPalmVelocity = (rightHand.PalmPosition.ToVector3() - rightHandLastPosition) / Time.deltaTime;
                rightPalmStabilizedVelocity = (rightPalmPreviousVelocity == Vector3.negativeInfinity) ? rightPalmVelocity : LowPass(rightPalmVelocity, rightPalmPreviousVelocity, alpha);
                rightHandLastPosition = rightHand.PalmPosition.ToVector3();
                rightPalmPreviousVelocity = rightPalmVelocity;
            }
            else
            {
                rightHandLastPosition = Vector3.negativeInfinity;
                rightPalmVelocity = Vector3.zero;
                rightPalmStabilizedVelocity = Vector3.zero;
            }


        }

        void DebugUpdate()
        {
            if (debug)
            {
                debugText = "";
                if (rightHand != null)
                {
                    debugText = HandPoseUtility.GetHandFingerStats(rightHand);
                    DebugText("R Pincing " + isRightHandPinching, true);
                    DebugText("Velocity : " + rightHand.PalmVelocity.ToVector3(), true);
                    DebugText("R Pinch Strength " + rightHand.PinchStrength, true);
                }

                if (leftHand != null)
                {
                    debugText += HandPoseUtility.GetHandFingerStats(leftHand);
                    DebugText("L Pincing " + isRightHandPinching, true);
                    DebugText("Velocity : " + leftHand.PalmVelocity.ToVector3(), true);
                    DebugText("L Pinch Strength " + leftHand.PinchStrength, true);

                }

                DebugText("-----------REMOTE TOUCH------------------", true);
                DebugText("L Remote Touch State: " + leftHandRemoteTouch.state, true);
                DebugText("L Remote Touch Distance: " + leftHandRemoteTouch.currentDistance, true);
                DebugText("L Remote Touch Start Pos: " + leftHandRemoteTouch.touchStartPosition, true);
                DebugText("R Remote Touch State: " + rightHandRemoteTouch.state, true);
                DebugText("R Remote Touch Distance: " + rightHandRemoteTouch.currentDistance, true);
                DebugText("R Remote Touch Start Pos: " + rightHandRemoteTouch.touchStartPosition, true);

                DebugText("-----------GESTURE & POSE------------------", true);
                DebugText("L Detected: " + isLeftHandTracked, true);
                DebugText("L Pose: " + leftHandPose, true);
                DebugText("L Gesture: " + leftHandGesture, true);
                DebugText("R Detected: " + isRightHandTracked, true);
                DebugText("R Pose: " + rightHandPose, true);
                DebugText("R Gesture: " + rightHandGesture, true);
                DebugText("Gesture time " + (rightGestureRecognition.gestureTimeRate - (Time.time - rightGestureRecognition.lastPoseTime)), true);
            }

        }

        void PoseGestureUpdate()
        {
            Frame frame = leapServiceProvider.CurrentFrame;
            List<Hand> hands = frame.Hands;
            if (hands.Count > 0)
            {
                switch (hands.Count)
                {
                    case 1:
                        if (hands[0].IsLeft)
                        {
                            leftHand = hands[0];
                            rightHand = null;
                        }
                        else
                        {
                            leftHand = null;
                            rightHand = hands[0];
                        }
                        break;
                    case 2:
                        if (hands[0].IsRight)
                        {
                            leftHand = hands[1];
                            rightHand = hands[0];
                        }
                        else
                        {
                            leftHand = hands[0];
                            rightHand = hands[1];
                        }
                        break;
                }

                if (leftHand != null && leftHand.Id != 0)
                {
                    if (leftHand.IsPinching())
                    {
                        if(!isLeftHandPinching) leftHandPinchStart = leftHand.PalmPosition.ToVector3();

                        if (overridePinchStatus)
                        {
                            isLeftHandPinching = (leftHand.PinchStrength <= maxPinchStrength && leftHand.PinchStrength >= minPinchStrength) ? true : false;
                        }
                        else
                        {
                            isLeftHandPinching = true;
                        }
                    }
                    else if (!leftHand.IsPinching())
                    {
                        isLeftHandPinching = false;
                        leftHandPinchStart = Vector3.negativeInfinity;
                    }
                    isLeftHandTracked = true;
                    leftHandPose = handPoseRecognition.GetHandPose(leftHand);
                    leftHandGesture = leftGestureRecognition.GetUnimanualGesture(leftHandPose);
                    
                    if (!leftHandGesture.Equals(UnimanualGestureType.Unknown.ToString()))
                    {
                        if (leftUnimanualData == null || leftUnimanualData.gesture != leftHandGesture)
                        {
                            Hand h = new Hand();
                            h = h.CopyFrom(leftHand);
                            leftUnimanualData = new UnimanualData(leftHandGesture, h);
                        }
                    }
                    else
                    {
                        leftUnimanualData = null;
                    }
                }
                else
                {
                    LeftUntracked();
                }

                if (rightHand != null && rightHand.Id != 0)
                {
                    if (rightHand.IsPinching())
                    {
                        if(!isRightHandPinching) righHandPinchStart = rightHand.PalmPosition.ToVector3();

                        if (overridePinchStatus)
                        {
                            isRightHandPinching = (rightHand.PinchStrength <= maxPinchStrength && rightHand.PinchStrength >= minPinchStrength) ? true : false;
                        }
                        else
                        {
                            isRightHandPinching = true;
                        }
                        
                    }else if (!rightHand.IsPinching())
                    {
                        isRightHandPinching = false;
                        righHandPinchStart = Vector3.negativeInfinity;
                    }

                    isRightHandTracked = true;
                    rightHandPose = handPoseRecognition.GetHandPose(rightHand);
                    rightHandGesture = rightGestureRecognition.GetUnimanualGesture(rightHandPose);
                    if (!rightHandGesture.Equals(UnimanualGestureType.Unknown.ToString()))
                    {
                        if (rightUnimanualData == null || rightUnimanualData.gesture != rightHandGesture)
                        {
                            Hand h = new Hand();
                            h = h.CopyFrom(rightHand);
                            rightUnimanualData = new UnimanualData(rightHandGesture, h);
                        }
                    }
                    else
                    {
                        rightUnimanualData = null;
                    }

                    //EventUpdate
                    UpdatePoseEvents();

                }
                else
                {
                    RightUntracked();
                }

                if (hands.Count == 2 && leftHand != null && rightHand != null)
                {

                    bimanualGesture = bimanualRecognition.GetBimanualGesture(leftHandGesture, rightHandGesture);
                    if (!bimanualGesture.Equals(BimanualGestureType.Unknown.ToString()))
                    {
                        if (bimanualData == null || bimanualGesture != bimanualData.gesture)
                        {
                            Hand l = new Hand();
                            l = l.CopyFrom(leftHand);
                            Hand r = new Hand();
                            r = r.CopyFrom(rightHand);
                            bimanualData = new BimanualData(l, r, leftHandGesture, rightHandGesture, bimanualGesture);
                        }
                    }
                    else
                    {
                        bimanualData = null;
                    }
                }
                else
                {
                    bimanualGesture = BimanualGestureType.Unknown;
                }
            }
            else
            {
                LeftUntracked();
                RightUntracked();
                BimanualUntracked();
            }
        }

        void UpdatePoseEvents()
        {
            if (rightHandPose != lastHandPose)
            {
                switch (rightHandPose)
                {
                    case HandPoseType.Open:
                        OnROpenBegin.Invoke();
                        break;
                    case HandPoseType.Paper:
                        OnRPaperBegin.Invoke();
                        break;
                }
                switch (lastHandPose)
                {
                    case HandPoseType.Open:
                        OnROpenEnd.Invoke();
                        break;
                    case HandPoseType.Paper:
                        OnRPaperEnd.Invoke();
                        break;
                }
            }
            else 
            {
                switch (rightHandPose)
                {
                    case HandPoseType.Open:
                        OnROpenStay.Invoke();
                        break;
                    case HandPoseType.Paper:
                        OnRPaperStay.Invoke();
                        break;
                }
            }

            lastHandPose = rightHandPose;
        }

        void RemoteTouchUpdate()
        {
            if (rightHand == null || !isRightHandTracked)
            {
                rightHandRemoteTouch.state = RemoteTouchState.Neutral;
            }
            else
            {
                rightHandRemoteTouch.currentDistance = Vector3.Distance(Camera.main.transform.position, rightHand.WristPosition.ToVector3());
                if (rightHandRemoteTouch.currentDistance >= averageTouchDistance)
                {
                    if (!rightHandRemoteTouch.state.Equals(RemoteTouchState.Touch))
                    {
                        rightHandRemoteTouch.touchStartPosition = rightHand.WristPosition.ToVector3();
                    }
                    rightHandRemoteTouch.state = RemoteTouchState.Touch;
                }
                else if (rightHandRemoteTouch.currentDistance >= averageTouchDistance - stdDevTouchDistance)
                {
                    rightHandRemoteTouch.state = RemoteTouchState.Hover;
                }
                else
                {
                    rightHandRemoteTouch.state = RemoteTouchState.Neutral;
                }
            }

            if (leftHand == null || !isLeftHandTracked)
            {
                leftHandRemoteTouch.state = RemoteTouchState.Neutral;
            }
            else
            {
                leftHandRemoteTouch.currentDistance = Vector3.Distance(Camera.main.transform.position, leftHand.WristPosition.ToVector3());
                if (leftHandRemoteTouch.currentDistance >= averageTouchDistance)
                {
                    if (!leftHandRemoteTouch.state.Equals(RemoteTouchState.Touch))
                    {
                        leftHandRemoteTouch.touchStartPosition = leftHand.WristPosition.ToVector3();
                    }
                    leftHandRemoteTouch.state = RemoteTouchState.Touch;
                }
                else if (leftHandRemoteTouch.currentDistance >= averageTouchDistance - stdDevTouchDistance)
                {
                    leftHandRemoteTouch.state = RemoteTouchState.Hover;
                }
                else
                {
                    leftHandRemoteTouch.state = RemoteTouchState.Neutral;
                }
            }
        }

        void BimanualUntracked()
        {
            bimanualGesture = BimanualGestureType.Unknown;
            bimanualData = null;
        }

        void LeftUntracked()
        {
            isLeftHandPinching = false;
            leftUnimanualData = null;
            leftHand = null;
            isLeftHandTracked = false;
            leftHandPose = HandPoseType.Unknown;
            leftHandGesture = UnimanualGestureType.Unknown;
        }

        void RightUntracked()
        {
            isRightHandPinching = false;
            rightUnimanualData = null;
            rightHand = null;
            isRightHandTracked = false;
            rightHandPose = HandPoseType.Unknown;
            rightHandGesture = UnimanualGestureType.Unknown;
        }

       public RemoteTouchProperty GetDominantTouch(bool isLeft)
        {
            if (isLeft)
            {
                return leftHandRemoteTouch;
            }
            else
            {
                return rightHandRemoteTouch;
            }
        }

        void InitUnimanualGestures()
        {

            List<HandPoseTemplate> handPoseList = new List<HandPoseTemplate>();
            if (open) handPoseList.Add(HandPoseTemplates.Open);
            if (paper) handPoseList.Add(HandPoseTemplates.Paper);
            if (point) handPoseList.Add(HandPoseTemplates.Point);
            if (rock) handPoseList.Add(HandPoseTemplates.Rock);
            if (scissorOpen) handPoseList.Add(HandPoseTemplates.ScissorOpen);
            if (scissorClosed) handPoseList.Add(HandPoseTemplates.ScissorClosed);
            if (thumb) handPoseList.Add(HandPoseTemplates.Thumb);
            if (promise) handPoseList.Add(HandPoseTemplates.Promise);
            if (metalNoThumb) handPoseList.Add(HandPoseTemplates.MetalNoThumb);
            if (metal) handPoseList.Add(HandPoseTemplates.Metal);
            if (middleFinger) handPoseList.Add(HandPoseTemplates.MiddleFinger);
            if (pistol) handPoseList.Add(HandPoseTemplates.Pistol);

            handPoseRecognition.SetHandPoses(handPoseList);

            leftGestureRecognition.SetUnimanualGestureTemplate(simpleUnimanualGestures);
            rightGestureRecognition.SetUnimanualGestureTemplate(simpleUnimanualGestures);
        }

        void InitBimanualGestures()
        {
            bimanualRecognition.SetBimanualGestureTemplate(simpleBimanualGestures);
        }

    }

}