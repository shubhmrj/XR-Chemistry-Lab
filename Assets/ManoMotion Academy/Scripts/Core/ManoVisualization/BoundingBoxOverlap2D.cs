using UnityEngine;
using UnityEngine.Events;
using ManoMotion.Visualization;
using UnityEngine.UI;

namespace ManoMotion
{
    /// <summary>
    /// Component that can be used to check overlaps between the bounding box and 2D colliders.
    /// </summary>
    public class BoundingBoxOverlap2D : MonoBehaviour
    {
        [SerializeField] BoundingBoxUI boundingBoxUI;
        [SerializeField] RectTransform canvas;
        [SerializeField] Image boundingBoxVisualization;
        [SerializeField] Color intersectColor, defaultColor;
        [SerializeField] UnityEvent<Collider2D[]> OnIntersection;
        [SerializeField] UnityEvent OnIntersectionStop;

        Bounds bounds;

        private void LateUpdate()
        {
            boundingBoxVisualization.gameObject.SetActive(boundingBoxUI.Activated);
            if (boundingBoxUI.Activated)
            {
                BoundingBox bb = boundingBoxUI.BoundingBox;

                CalculateBoundsValues(bb, out Vector3 center, out Vector3 size);
                bounds = new Bounds(center, size);

                size.x = Mathf.InverseLerp(0, Screen.width, size.x) * canvas.sizeDelta.x;
                size.y = Mathf.InverseLerp(0, Screen.height, size.y) * canvas.sizeDelta.y;

                boundingBoxVisualization.rectTransform.position = bounds.center;
                boundingBoxVisualization.rectTransform.sizeDelta = size;
                boundingBoxVisualization.gameObject.SetActive(boundingBoxUI.Activated);

                CheckIntersection();
            }
            else
            {
                OnIntersectionStop?.Invoke();
            }
        }

        private void CalculateBoundsValues(BoundingBox bb, out Vector3 center, out Vector3 size)
        {
            Vector3 topLeft = bb.topLeft;
            Vector3 botRight = bb.topLeft + Vector3.right * bb.width + Vector3.down * bb.height;
            topLeft = ManoUtils.Instance.CalculateScreenPosition(topLeft);
            botRight = ManoUtils.Instance.CalculateScreenPosition(botRight);
            center = (topLeft + botRight) / 2;
            size = CalculateSize(topLeft, botRight);
        }

        private Vector3 CalculateSize(Vector3 topLeft, Vector3 botRight)
        {
            float width = botRight.x - topLeft.x;
            float height = topLeft.y - botRight.y;
            Vector3 size = new Vector3(width, height);
            return size;
        }

        private void CheckIntersection()
        {
            Collider2D[] colliders = Physics2D.OverlapBoxAll(bounds.center, bounds.size, 0);
            bool intersects = colliders.Length > 0;
            boundingBoxVisualization.color = intersects ? intersectColor : defaultColor;

            if (intersects)
            {
                OnIntersection?.Invoke(colliders);
            }
            else
            {
                OnIntersectionStop?.Invoke();
            }
        }
    }
}