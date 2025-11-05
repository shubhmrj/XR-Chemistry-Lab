using System.Collections;
using UnityEngine;

namespace ManoMotion.Gizmos
{
    /// <summary>
    /// Displays a line between start/stop gesture triggers. Like grab and release.
    /// </summary>
    public class TriggerLineVisualizer : MonoBehaviour
    {
        [SerializeField] LeftOrRightHand handLeftRight;
        [SerializeField] ManoGestureTrigger startGesture, stopGesture;
        [SerializeField] TriggerMarker startMarker, stopMarker;
        [SerializeField] LineRenderer lineRenderer;
        [SerializeField] float timeBeforeRemoving = 1f;

        [SerializeField] Transform canvas;
        [SerializeField] TriggerGizmo startGizmo, stopGizmo;
        [SerializeField] Vector3 localTriggerSize;

        bool started;
        ManoClass manoClass;

        private void Awake()
        {
            lineRenderer.positionCount = 2;
        }

        private void Update()
        {
            // Get the trigger gesture of the hand and display it
            if (ManoMotionManager.Instance.TryGetHandInfo(handLeftRight, out HandInfo handInfo))
            {
                if (!started)
                {
                    TryStart(handInfo);
                }
                else
                {
                    TryStop(handInfo);
                }

                if (started)
                {
                    stopMarker.Activate(transform.position);
                    lineRenderer.SetPosition(1, transform.position);

                    if (handInfo.gestureInfo.manoClass != manoClass)
                    {
                        Stop();
                    }
                }
            }
            else
            {
                if (started)
                    SetTextPosition(stopGizmo);
                Stop();
            }
        }

        private bool TryStart(HandInfo handInfo)
        {
            GestureInfo gestureInfo = handInfo.gestureInfo;

            if (gestureInfo.manoGestureTrigger.Equals(startGesture))
            {
                StopAllCoroutines();
                started = true;
                manoClass = gestureInfo.manoClass;
                startMarker.Activate(transform.position);
                stopMarker.Activate(transform.position);
                lineRenderer.SetPosition(0, transform.position);
                lineRenderer.SetPosition(1, transform.position);
                lineRenderer.enabled = true;
                SetTextPosition(startGizmo);
                stopGizmo.gameObject.SetActive(false);
                return true;
            }
            return false;
        }

        private bool TryStop(HandInfo handInfo)
        {
            GestureInfo gestureInfo = handInfo.gestureInfo;

            if (gestureInfo.manoGestureTrigger.Equals(stopGesture))
            {
                Stop();
                SetTextPosition(stopGizmo);
                return true;
            }
            return false;
        }

        private void Stop()
        {
            started = false;
            StartCoroutine(DisableMarkers());

            IEnumerator DisableMarkers()
            {
                yield return new WaitForSeconds(timeBeforeRemoving);
                startMarker.Deactivate();
                stopMarker.Deactivate();
                lineRenderer.enabled = false;

                startGizmo.StartFading();
                stopGizmo.StartFading();
                startGizmo.enabled = true;
                stopGizmo.enabled = true;
            }
        }

        private void SetTextPosition(TriggerGizmo triggerGizmo)
        {
            triggerGizmo.transform.SetParent(transform);
            triggerGizmo.transform.localScale = localTriggerSize;
            triggerGizmo.SetScale(localTriggerSize);
            triggerGizmo.transform.localPosition = Vector3.zero;
            triggerGizmo.transform.SetParent(canvas);
            triggerGizmo.transform.rotation = Camera.main.transform.rotation;

            triggerGizmo.gameObject.SetActive(true);
            triggerGizmo.enabled = false;
        }

        private void OnDisable()
        {
            started = false;
            if (startMarker)
                startMarker.gameObject.SetActive(false);
            if (stopMarker)
                stopMarker.gameObject.SetActive(false);
            lineRenderer.enabled = false;

            startGizmo.gameObject.SetActive(false);
            stopGizmo.gameObject.SetActive(false);
            startGizmo.enabled = false;
            stopGizmo.enabled = false;
        }
    }
}