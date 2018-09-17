using System.Collections.Generic;
using UnityEngine;
using Leap;
using Leap.Unity;



namespace HandGesture
{
    [System.Serializable]
    public enum UnimanualGestureType
    {
        ThumbPistol,
        ThumbOpen,
        Pointing,
        HandOpen,
        HandClose,
        RockPistol,
        Unknown
    }

    [System.Serializable]
    public enum BimanualGestureType
    {
        ThumbPistols,
        ThumbOpens,
        PointPoint,
        Unknown
    }

    public class BimanualData
    {
        public Hand leftHand;
        public Hand rightHand;
        public UnimanualGestureType leftHandGesture;
        public UnimanualGestureType rightHandGesture;
        public BimanualGestureType gesture;

        public BimanualData(Hand l, Hand r, UnimanualGestureType lg, UnimanualGestureType rg, BimanualGestureType g)
        {
            leftHand = l;
            rightHand = r;
            leftHandGesture = lg;
            rightHandGesture = rg;
            gesture = g;
        }

        public Vector3 GetCenter()
        {
            return (leftHand.PalmPosition.ToVector3() + rightHand.PalmPosition.ToVector3()) * 0.5f;
        }

        public float GetDistance()
        {
            return Vector3.Distance(leftHand.PalmPosition.ToVector3(), rightHand.PalmPosition.ToVector3());
        }
    }


    public class UnimanualData
    {
        public UnimanualGestureType gesture;
        public Hand hand;

        public UnimanualData(UnimanualGestureType g, Hand h)
        {
            gesture = g;
            hand = h;
        }
    }

    [System.Serializable]
    public enum HandPoseType
    {
        Open,
        Paper,
        Rock,
        ScissorOpen,
        ScissorClosed,
        Metal,
        Thumb,
        Promise,
        MetalNoThumb,
        Point,
        Pistol,
        MiddleFinger,
        Unknown
    }


    public class HandPoseTemplate
    {
        public float maxNonThumbFingerDistance;
        public float minNonThumbFingersDistance;
        public float minThumbIndexDistance;
        public float maxThumbIndexDistance;
        public bool thumbExtended;
        public bool indexExtended;
        public bool middleExtended;
        public bool ringExtended;
        public bool pinkyExtended;
        public HandPoseType poseType;

        public HandPoseTemplate(HandPoseType type, float minNon, float maxNon, float minTI, float maxTI, bool t, bool i, bool m, bool r, bool p)
        {
            minNonThumbFingersDistance = minNon;
            maxNonThumbFingerDistance = maxNon;
            minThumbIndexDistance = minTI;
            maxThumbIndexDistance = maxTI;
            thumbExtended = t;
            indexExtended = i;
            middleExtended = m;
            ringExtended = r;
            pinkyExtended = p;
            poseType = type;
        }


        public bool isMatch(float nonThumb, float ti, bool t, bool i, bool m, bool r, bool p)
        {
            nonThumb *= 1000f;
            ti *= 1000f;

            if (thumbExtended == t &&
                indexExtended == i &&
                middleExtended == m &&
                ringExtended == r &&
                pinkyExtended == p &&
                nonThumb >= minNonThumbFingersDistance &&
                nonThumb <= maxNonThumbFingerDistance &&
                ti >= minThumbIndexDistance &&
                ti <= maxThumbIndexDistance
                )
                return true;
            else
                return false;
        }
    }

    public static class HandPoseTemplates
    {
        public static float MAX_DISTANCE = 300f;

        public static HandPoseTemplate Open = new HandPoseTemplate(HandPoseType.Open, 0f, MAX_DISTANCE, 0f, MAX_DISTANCE, true, true, true, true, true);
        public static HandPoseTemplate Paper = new HandPoseTemplate(HandPoseType.Paper, 30, MAX_DISTANCE, 100f, MAX_DISTANCE, true, true, true, true, true);
        public static HandPoseTemplate Rock = new HandPoseTemplate(HandPoseType.Rock, 0f, MAX_DISTANCE, 0f, MAX_DISTANCE, false, false, false, false, false);
        public static HandPoseTemplate Metal = new HandPoseTemplate(HandPoseType.Metal, 30f, 100f, 70f, MAX_DISTANCE, true, true, false, false, true);
        public static HandPoseTemplate ScissorOpen = new HandPoseTemplate(HandPoseType.ScissorOpen, 43f, MAX_DISTANCE, 0f, MAX_DISTANCE, false, true, true, false, false);
        public static HandPoseTemplate ScissorClosed = new HandPoseTemplate(HandPoseType.ScissorClosed, 0f, 43f, 0f, MAX_DISTANCE, false, true, true, false, false);
        public static HandPoseTemplate Thumb = new HandPoseTemplate(HandPoseType.Thumb, 0f, MAX_DISTANCE, 0f, MAX_DISTANCE, true, false, false, false, false);
        public static HandPoseTemplate Promise = new HandPoseTemplate(HandPoseType.Promise, 0f, MAX_DISTANCE, 0f, MAX_DISTANCE, false, false, false, false, true);
        public static HandPoseTemplate MetalNoThumb = new HandPoseTemplate(HandPoseType.MetalNoThumb, 0f, MAX_DISTANCE, 0f, MAX_DISTANCE, false, true, false, false, true);
        public static HandPoseTemplate Point = new HandPoseTemplate(HandPoseType.Point, 0f, MAX_DISTANCE, 0f, MAX_DISTANCE, false, true, false, false, false);
        public static HandPoseTemplate MiddleFinger = new HandPoseTemplate(HandPoseType.MiddleFinger, 0f, MAX_DISTANCE, 0f, MAX_DISTANCE, false, false, true, false, false);
        public static HandPoseTemplate Pistol = new HandPoseTemplate(HandPoseType.Pistol, 0f, MAX_DISTANCE, 0f, MAX_DISTANCE, true, true, false, false, false);

        public static List<HandPoseTemplate> GetAllHandPoses()
        {
            List<HandPoseTemplate>  handPoses = new List<HandPoseTemplate>();
            handPoses.Add(Open);
            handPoses.Add(Paper);
            handPoses.Add(Rock);
            handPoses.Add(Metal);
            handPoses.Add(ScissorOpen);
            handPoses.Add(ScissorClosed);
            handPoses.Add(Thumb);
            handPoses.Add(Promise);
            handPoses.Add(MetalNoThumb);
            handPoses.Add(Point);
            handPoses.Add(MiddleFinger);
            handPoses.Add(Pistol);

            return handPoses;
        }
    }

    public class HandPoseRecognition
    {

        List<HandPoseTemplate> handPoses;

        public HandPoseRecognition()
        {
            handPoses = new List<HandPoseTemplate>();
        }

        public void SetHandPoses(List<HandPoseTemplate> list)
        {
            handPoses = list;
        }

        public HandPoseType GetHandPose(Hand hand)
        {
            HandPoseType type = HandPoseType.Unknown;
            float nonThumb = HandPoseUtility.NonThumbFingersGapAverage(hand);
            float ti = HandPoseUtility.FingerDistanceTI(hand);
            bool t = HandPoseUtility.GetFinger(hand, Finger.FingerType.TYPE_THUMB).IsExtended;
            bool i = HandPoseUtility.GetFinger(hand, Finger.FingerType.TYPE_INDEX).IsExtended;
            bool m = HandPoseUtility.GetFinger(hand, Finger.FingerType.TYPE_MIDDLE).IsExtended;
            bool r = HandPoseUtility.GetFinger(hand, Finger.FingerType.TYPE_RING).IsExtended;
            bool p = HandPoseUtility.GetFinger(hand, Finger.FingerType.TYPE_PINKY).IsExtended;

            foreach (HandPoseTemplate template in handPoses)
            {
                if (template.isMatch(nonThumb, ti, t, i, m, r, p))
                {
                    type = template.poseType;
                }
            }

            return type;
        }


    }

   
    [System.Serializable]
    public class SimpleUnimanualGestureTemplate
    {
        public HandPoseType startPose;
        public HandPoseType endPose;
        public UnimanualGestureType gestureType;
        //deleted public float time;

        public SimpleUnimanualGestureTemplate(HandPoseType s, HandPoseType e, UnimanualGestureType type)
        {
            startPose = s;
            endPose = e;
          //deleted  time = t;
            gestureType = type;
        }
    }

    [System.Serializable]
    public class SimpleBimanualGestureTemplate
    {
        public UnimanualGestureType leftHand;
        public UnimanualGestureType rigthHand;
        public BimanualGestureType gestureType;

        public SimpleBimanualGestureTemplate(UnimanualGestureType l, UnimanualGestureType r, BimanualGestureType t)
        {
            leftHand = l;
            rigthHand = r;
            gestureType = t;
        }

        public bool IsMatch(UnimanualGestureType l, UnimanualGestureType r)
        {
            if (l == leftHand && r == rigthHand)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

    }


    public class SimpleGestureRecognition
    {

        private HandPoseType lastPose;
        private UnimanualGestureType lastGesture;
        public float lastPoseTime;
        public float gestureTimeRate;


        public List<SimpleUnimanualGestureTemplate> simpleUnimanualGestures;
        public List<SimpleBimanualGestureTemplate> simpleBimanualGestures;

        public SimpleGestureRecognition(float t)
        {
            lastPose = HandPoseType.Unknown;
            lastGesture = UnimanualGestureType.Unknown;
            lastPoseTime = Time.time;
            gestureTimeRate = t;
        }

        public void SetUnimanualGestureTemplate(List<SimpleUnimanualGestureTemplate> list)
        {
            simpleUnimanualGestures = list;
        }

        public void SetBimanualGestureTemplate(List<SimpleBimanualGestureTemplate> list)
        {
            simpleBimanualGestures = list;
        }

        public BimanualGestureType GetBimanualGesture(UnimanualGestureType l, UnimanualGestureType r)
        {

            BimanualGestureType gesture = BimanualGestureType.Unknown;
            foreach (SimpleBimanualGestureTemplate template in simpleBimanualGestures)
            {
                if (template.IsMatch(l, r))
                {
                    gesture = template.gestureType;
                }
            }
            return gesture;
        }

        public UnimanualGestureType GetUnimanualGesture(HandPoseType pose)
        {
            if (pose == HandPoseType.Unknown || pose == lastPose) return lastGesture;

            float t = Mathf.Abs(Time.time - lastPoseTime);

            UnimanualGestureType gesture = UnimanualGestureType.Unknown;
            foreach (SimpleUnimanualGestureTemplate template in simpleUnimanualGestures)
            {
                if (IsMatch(template,lastPose, pose,t))
                {
                    gesture = template.gestureType;
                }
            }
            lastPose = pose;
            lastGesture = gesture;
            lastPoseTime = Time.time;

            return gesture;
        }

        public bool IsMatch(SimpleUnimanualGestureTemplate template, HandPoseType s, HandPoseType e, float t)
        {

            if (s == template.startPose && e == template.endPose && t <= gestureTimeRate)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }




    public class MappingFunction
    {
        public static Vector3 ExponentialMapXY(float distance, Vector3 input)
        {
            float x = input.x * ExponentialMap(distance);
            float y = input.y * ExponentialMap(distance);
            return new Vector3(x, y, input.z);
        }

        public static float ExponentialMap(float x)
        {
            return Mathf.Pow(1.3f, (-x));
        }
    }


    public class HandPoseUtility
    {
        public static Vector3 ToVector3(Vector v)
        {
            return new Vector3(v.x, v.y, v.z);
        }

        public static Hand GetDominantHand(List<Hand> hands, bool isLeftHanded)
        {
            Hand dominant = hands[0];
            if (dominant.IsRight && isLeftHanded) dominant = hands[1];
            if (dominant.IsLeft && !isLeftHanded) dominant = hands[1];
            return dominant;
        }

        public static bool IsHandDominantMatch(Hand hand, bool isLeftHanded)
        {
            if(isLeftHanded && hand.IsLeft)
            {
                return true;
            }
            else if(!isLeftHanded && hand.IsRight)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static Hand GetNonDominantHand(List<Hand> hands, bool isLeftHanded)
        {
            Hand dominant = hands[0];
            if (dominant.IsLeft && !isLeftHanded) dominant = hands[1];
            if (dominant.IsRight && isLeftHanded) dominant = hands[1];
            return dominant;
        }

        public static Vector3 ConvertPosition(Vector v)
        {

            Vector3 pos = Leap.Unity.UnityVectorExtension.ToVector3(v);

            pos = new Vector3(pos.x, pos.y, -pos.z);

            return pos;
        }

        public static string GetHandFingerStats(Hand hand)
        {
            string s = GetHandStats(hand);
            foreach (Finger f in hand.Fingers)
            {
                s += GetFingerStats(f);
            }
            return s;
        }

        public static string GetHandStats(Hand hand)
        {
            string s = "\n Hand \n Position: " + hand.PalmPosition;
            s += "\n Rotation: " + hand.Rotation;
            s += (hand.IsLeft) ? "\n Type: Left" : s += "\n Type: Right";
            return s;
        }

        public static string GetFingerStats(Finger finger)
        {
            string s = "\n Finger \n Finger Type: " + finger.Type;
            s += "\n Extended: " + finger.IsExtended;
            s += "\n Position: " + finger.TipPosition;
            return s;
        }

        public static bool IsAllFingerExtended(Hand hand)
        {
            bool status = true;
            foreach (Finger f in hand.Fingers)
            {
                if (!f.IsExtended)
                {
                    status = false;
                    break;
                }
            }
            return status;
        }

        public static Finger GetFinger(Hand hand, Finger.FingerType type)
        {
            foreach (Finger f in hand.Fingers)
            {
                if (f.Type == type)
                {
                    return f;
                }
            }
            return null;
        }

        public static string GetFingersGapDistance(Hand hand)
        {
            string s = "\n Fingers Gap Distance \n TI = " + FingerDistanceTI(hand);
            s += "\n IM = " + FingerDistanceIM(hand);
            s += "\n MR = " + FingerDistanceMR(hand);
            s += "\n RP = " + FingerDistanceRP(hand);

            return s;
        }

        public static float NonThumbFingersGapAverage(Hand hand)
        {
            float avg = 0f;
            avg = (FingerDistanceIM(hand) + FingerDistanceMR(hand) + FingerDistanceRP(hand)) / 4f;
            return avg;
        }

        public static float FingerDistanceTI(Hand hand)
        {
            Finger t = GetFinger(hand, Finger.FingerType.TYPE_THUMB);
            Finger i = GetFinger(hand, Finger.FingerType.TYPE_INDEX);
            return FingerDistance(t, i);
        }

        public static float FingerDistanceIM(Hand hand)
        {
            Finger m = GetFinger(hand, Finger.FingerType.TYPE_MIDDLE);
            Finger i = GetFinger(hand, Finger.FingerType.TYPE_INDEX);
            return FingerDistance(m, i);
        }

        public static float FingerDistanceMR(Hand hand)
        {
            Finger m = GetFinger(hand, Finger.FingerType.TYPE_MIDDLE);
            Finger r = GetFinger(hand, Finger.FingerType.TYPE_RING);
            return FingerDistance(m, r);
        }

        public static float FingerDistanceRP(Hand hand)
        {
            Finger r = GetFinger(hand, Finger.FingerType.TYPE_RING);
            Finger p = GetFinger(hand, Finger.FingerType.TYPE_PINKY);
            return FingerDistance(r, p);
        }

        public static float FingerDistance(Finger f1, Finger f2)
        {
            return f1.TipPosition.DistanceTo(f2.TipPosition);
        }

    }

}

