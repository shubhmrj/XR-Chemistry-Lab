using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ManoMotion.Gizmos
{
    /// <summary>
    /// To display specific trigger gestures in skeleton scene.
    /// </summary>
    public class TriggerGestureVisualizer : MonoBehaviour
    {
        [SerializeField] LeftOrRightHand handLeftRight;
        [SerializeField] TriggerGizmo triggerTextPrefab;
        [SerializeField] Transform middleFingerRoot, thumbTip;
        [SerializeField] Transform canvas;
        [SerializeField] Vector3 localTriggerSize;

        List<TriggerGizmo> triggerObjectPool = new List<TriggerGizmo>();
        const int poolSize = 20;

        HandSide lastSide;
        float sideTime = 0;

        const float SideTimeBeforeEnablingTriggers = 0.5f;

        private void Awake()
        {
            for (int i = 0; i < poolSize; i++)
            {
                TriggerGizmo newTriggerObject = Instantiate(triggerTextPrefab, middleFingerRoot);
                newTriggerObject.gameObject.SetActive(false);
                newTriggerObject.transform.localScale = localTriggerSize;
                newTriggerObject.SetScale(localTriggerSize);
                triggerObjectPool.Add(newTriggerObject);
            }
        }

        void Update()
        {
            // Get the trigger gesture of the hand and display it
            if (ManoMotionManager.Instance.TryGetHandInfo(handLeftRight, out HandInfo handInfo))
            {
                HandSide side = handInfo.gestureInfo.handSide;
                if (lastSide == side)
                {
                    sideTime += Time.deltaTime;
                    if (sideTime > SideTimeBeforeEnablingTriggers)
                    {
                        GestureInfo gestureInfo = handInfo.gestureInfo;
                        StartCoroutine(DisplayTriggerGesture(gestureInfo.manoGestureTrigger));
                    }
                }
                else
                {
                    sideTime = 0;
                    lastSide = side;
                }
            }
        }

        /// <summary>
        /// Display Visual information of the detected trigger gesture and trigger swipes.
        /// In the case where a click is intended (Open pinch, Closed Pinch, Open Pinch) we are clearing out the visual information that are generated from the pick/drop
        /// </summary>
        /// <param name="triggerGesture">Requires an input from ManoGestureTrigger.</param>
        /// <param name="trackingInfo">Requires an input of tracking info.</param>
        IEnumerator DisplayTriggerGesture(ManoGestureTrigger triggerGesture)
        {
            for (int i = 0; i < 3; i++)
            {
                yield return null;
            }

            if (ManoMotionManager.Instance.TryGetHandInfo(handLeftRight, out HandInfo handInfo))
            {
                HandSide side = handInfo.gestureInfo.handSide;
                if (lastSide == side)
                {
                    switch (triggerGesture)
                    {
                        case ManoGestureTrigger.CLICK:
                            TriggerDisplay(triggerGesture, thumbTip);
                            break;
                        case ManoGestureTrigger.SWIPE_LEFT:
                        case ManoGestureTrigger.SWIPE_RIGHT:
                        case ManoGestureTrigger.SWIPE_DOWN:
                        case ManoGestureTrigger.SWIPE_UP:
                            TriggerDisplay(triggerGesture, middleFingerRoot);
                            break;
                    }
                }
            }
        }

        /// <summary>
        /// Displays the visual information of the performed trigger gesture.
        /// </summary>
        /// <param name="bounding_box">Bounding box.</param>
        /// <param name="triggerGesture">Trigger gesture.</param>
        void TriggerDisplay(ManoGestureTrigger triggerGesture, Transform parent)
        {
            TriggerGizmo triggerGizmo = GetCurrentPooledTrigger();

            if (triggerGizmo)
            {
                triggerGizmo.transform.SetParent(parent);
                triggerGizmo.transform.localScale = localTriggerSize;
                triggerGizmo.SetScale(localTriggerSize);
                triggerGizmo.transform.localPosition = Vector3.zero;
                triggerGizmo.transform.SetParent(canvas);
                triggerGizmo.transform.rotation = Camera.main.transform.rotation;

                triggerGizmo.gameObject.SetActive(true);
                triggerGizmo.name = triggerGesture.ToString();
                triggerGizmo.InitializeTriggerGizmo(triggerGesture);
            }
        }

        /// <summary>
        /// Gets the current pooled trigger object.
        /// </summary>
        /// <returns>The current pooled trigger.</returns>
        private TriggerGizmo GetCurrentPooledTrigger()
        {
            for (int i = 0; i < triggerObjectPool.Count; i++)
            {
                if (!triggerObjectPool[i].gameObject.activeInHierarchy)
                {
                    return triggerObjectPool[i];
                }
            }
            return null;
        }
    }
}