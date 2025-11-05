using TMPro;
using UnityEngine;

namespace ManoMotion.Demos
{
    public class GestureExamples : MonoBehaviour
    {
        void Update()
        {
            GestureInfo gestureInfo = ManoMotionManager.Instance.HandInfos[0].gestureInfo;
            ContinuousGestureExample(gestureInfo);
            GestureTriggerExample(gestureInfo);
            HandSideExample(gestureInfo);
        }

        [SerializeField] TMP_Text continuousGestureText;

        void ContinuousGestureExample(GestureInfo gestureInfo)
        {
            ManoGestureContinuous continuousGesture = gestureInfo.manoGestureContinuous;
            continuousGestureText.text = continuousGesture.ToString();
        }

        [SerializeField] GameObject toggleObject;

        void GestureTriggerExample(GestureInfo gestureInfo)
        {
            ManoGestureTrigger gestureTrigger = gestureInfo.manoGestureTrigger;

            if (gestureTrigger == ManoGestureTrigger.CLICK)
            {
                bool active = toggleObject.activeSelf;
                toggleObject.SetActive(!active);
            }
        }

        [SerializeField] MeshRenderer handSideRenderer;
        [SerializeField] Material palmMaterial, backMaterial;

        void HandSideExample(GestureInfo gestureInfo)
        {
            HandSide handSide = gestureInfo.handSide;

            if (handSide == HandSide.Palmside)
            {
                handSideRenderer.material = palmMaterial;
            }
            else
            {
                handSideRenderer.material = backMaterial;
            }
        }
    }
}