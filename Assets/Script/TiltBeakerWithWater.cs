//using UnityEngine;
//using ManoMotion;
//
//public class TiltBeakerWithWater : MonoBehaviour
//{
//    [SerializeField] private GameObject beakerModel;
//    [SerializeField] private Transform waterObject;
//    [SerializeField] private ParticleSystem spillEffect;
//
//    private float tiltSpeed = 30f;
//    private float maxTiltAngle = 30f;
//    private bool isSpilling = false;
//
//    void Start()
//    {
//        if (ManoMotionManager.Instance != null)
//        {
//            ManoMotionManager.Instance.ShouldCalculateGestures(true);
//        }
//
//        if (spillEffect != null)
//        {
//            spillEffect.Stop();
//        }
//    }
//
//    void Update()
//    {
//        if (ManoMotionManager.Instance == null || ManoMotionManager.Instance.HandInfos == null)
//            return;
//
//        foreach (var handInfo in ManoMotionManager.Instance.HandInfos)
//        {
//            if (handInfo.gestureInfo.manoClass == ManoClass.NO_HAND)
//                continue;
//
//            BoundingBox box = handInfo.trackingInfo.boundingBox;
//            float centerX = box.topLeft.x + box.width / 2f;
//            float centerY = box.topLeft.y - box.height / 2f;
//            float normalizedX = centerX - 0.5f;
//            float normalizedY = 0.5f - centerY;
//
//            switch (handInfo.gestureInfo.manoGestureContinuous)
//            {
//                case ManoGestureContinuous.OPEN_HAND_GESTURE:
//                    float tiltX = normalizedY * tiltSpeed * Time.deltaTime;
//                    float tiltZ = -normalizedX * tiltSpeed * Time.deltaTime;
//                    beakerModel.transform.Rotate(tiltX, 0, tiltZ);
//                    break;
//
//                case ManoGestureContinuous.CLOSED_HAND_GESTURE:
//                    beakerModel.transform.rotation = Quaternion.Lerp(beakerModel.transform.rotation, Quaternion.identity, Time.deltaTime * 2f);
//                    break;
//            }
//        }
//
//        CheckSpillCondition();
//    }
//
//    void CheckSpillCondition()
//    {
//        Vector3 localEuler = beakerModel.transform.localEulerAngles;
//        float xAngle = NormalizeAngle(localEuler.x);
//        float zAngle = NormalizeAngle(localEuler.z);
//
//        if (Mathf.Abs(xAngle) > maxTiltAngle || Mathf.Abs(zAngle) > maxTiltAngle)
//        {
//            if (!isSpilling)
//            {
//                SpillWater();
//            }
//        }
//        else
//        {
//            if (isSpilling)
//            {
//                StopSpilling();
//            }
//        }
//    }
//
//    float NormalizeAngle(float angle)
//    {
//        return (angle > 180) ? angle - 360 : angle;
//    }
//
//    void SpillWater()
//    {
//        isSpilling = true;
//
//        if (spillEffect != null && !spillEffect.isPlaying)
//        {
//            spillEffect.Play();
//        }
//
//        if (waterObject != null)
//        {
//            Vector3 currentScale = waterObject.localScale;
//            waterObject.localScale = new Vector3(currentScale.x, Mathf.Max(0, currentScale.y - 0.1f), currentScale.z);
//        }
//    }
//
//    void StopSpilling()
//    {
//        isSpilling = false;
//
//        if (spillEffect != null && spillEffect.isPlaying)
//        {
//            spillEffect.Stop();
//        }
//    }
//}
