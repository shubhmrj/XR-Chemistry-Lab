using UnityEngine;
using ManoMotion;

public class HandController : MonoBehaviour
{
    private GameObject heldObject;
    private Camera mainCamera;

    void Start()
    {
        mainCamera = Camera.main;
    }

    void Update()
    {
        GestureInfo gesture = ManoMotionManager.Instance.HandInfos[0].gestureInfo;
        if (gesture.manoGestureTrigger == ManoGestureTrigger.GRAB_GESTURE)
        {
            TryGrab();
        }
        else if (gesture.manoGestureTrigger == ManoGestureTrigger.RELEASE_GESTURE)
        {
            Release();
        }
    }

    void TryGrab()
    {
        if (heldObject != null) return;

        BoundingBox boundingBox = ManoMotionManager.Instance.HandInfos[0].trackingInfo.boundingBox;
        Vector2 center = new Vector2(boundingBox.topLeft.x + boundingBox.width / 2, boundingBox.topLeft.y - boundingBox.height / 2);
        Ray ray = mainCamera.ScreenPointToRay(center);
        if (Physics.Raycast(ray, out RaycastHit hit, 1.5f))
        {
            IInteractable interactable = hit.collider.GetComponent<IInteractable>();
            if (interactable != null)
            {
                heldObject = hit.collider.gameObject;
                interactable.OnGrab();
                heldObject.transform.SetParent(transform);
            }
        }
    }

    void Release()
    {
        if (heldObject != null)
        {
            IInteractable interactable = heldObject.GetComponent<IInteractable>();
            interactable?.OnRelease();
            heldObject.transform.SetParent(null);
            heldObject = null;
        }
    }
}
