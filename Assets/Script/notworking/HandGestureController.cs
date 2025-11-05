using UnityEngine;
using ManoMotion;

public class HandGestureController : MonoBehaviour
{
    [SerializeField] private GameObject targetModel;
    private HandInfo handInfo;
    private GestureInfo gestureInfo;
    private float rotationSpeed = 50f;
    private float moveSpeed = 5f;

    void Start()
    {
        ManoMotionManager.Instance.ShouldCalculateGestures(true);
    }

    void Update()
    {
        handInfo = (HandInfo)ManoMotionManager.Instance.HandInfos[0];

        if (ManoMotionManager.Instance.HandInfos != null && ManoMotionManager.Instance.HandInfos.Length > 0 && handInfo.gestureInfo.manoClass != ManoClass.NO_HAND)
        {
            Vector3 handPosition = new Vector3(
                handInfo.trackingInfo.boundingBox.topLeft.x + handInfo.trackingInfo.boundingBox.width / 2,
                handInfo.trackingInfo.boundingBox.height / 2,
                0
            );

            switch (gestureInfo.manoGestureContinuous)
            {
                case ManoGestureContinuous.CLOSED_HAND_GESTURE:
                    targetModel.transform.Rotate(Vector3.up * rotationSpeed * Time.deltaTime);
                    break;

                case ManoGestureContinuous.OPEN_HAND_GESTURE:
                    Vector3 targetPosition = Camera.main.ViewportToWorldPoint(new Vector3(
                        handPosition.x,
                        handPosition.y,
                        10
                    ));
                    targetModel.transform.position = Vector3.Lerp(
                        targetModel.transform.position,
                        targetPosition,
                        moveSpeed * Time.deltaTime
                    );
                    break;

                case ManoGestureContinuous.HOLD_GESTURE:
                    float scale = 1 + handPosition.y;
                    targetModel.transform.localScale = Vector3.one * Mathf.Clamp(scale, 0.5f, 2f);
                    break;
            }
        }
    }
}