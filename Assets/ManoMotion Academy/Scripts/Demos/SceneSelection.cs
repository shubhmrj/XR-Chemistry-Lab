using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ManoMotion.Demos
{
    /// <summary>
    /// Manages the scene carousel.
    /// </summary>
    public class SceneSelection : MonoBehaviour, IBeginDragHandler, IEndDragHandler
    {
        [SerializeField] bool isHorizontal = true, snapOnStart = true;
        [SerializeField] ScrollRect scroll;
        [SerializeField] RectTransform content;
        [SerializeField] HorizontalOrVerticalLayoutGroup layoutGroup;
        [SerializeField] float snapForce = 200f;
        [SerializeField] int currentIndex = 0;
        [SerializeField] SceneOption[] contentItems;
        [SerializeField] UnityEvent<int> OnIndexSelected;

        bool isSnapped = false;

        float contentItemWidth, contentItemHeight;

        private float CalculatedIndex => isHorizontal ? -content.localPosition.x / (contentItemWidth + layoutGroup.spacing) :
                                                         content.localPosition.y / (contentItemHeight + layoutGroup.spacing);

        void Start()
        {
            contentItemWidth = contentItems[0].Width;
            contentItemHeight = contentItems[0].Height;
            if (snapOnStart)
                Snap(currentIndex);
        }

        private void Update()
        {
            if (isSnapped)
            {
                scroll.velocity = Vector3.zero;

                Vector3 targetPosition = content.localPosition;
                if (isHorizontal)
                    targetPosition.x = -currentIndex * (contentItemWidth + layoutGroup.spacing);
                else
                    targetPosition.y = currentIndex * (contentItemHeight + layoutGroup.spacing);

                content.localPosition = Vector3.MoveTowards(content.localPosition, targetPosition, snapForce * Time.deltaTime);
            }
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            isSnapped = false;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            SnapToClosest();
        }

        public void Snap(int index)
        {
            contentItems[currentIndex].Deselect();
            currentIndex = Mathf.Clamp(index, 0, contentItems.Length - 1);
            contentItems[currentIndex].Select();
            isSnapped = true;
            OnIndexSelected?.Invoke(currentIndex);
        }

        private void SnapToClosest()
        {
            int closestIndex = Mathf.RoundToInt(CalculatedIndex);
            Snap(closestIndex);
        }

        public void SetPosition(int index)
        {
            contentItems[currentIndex].Deselect();
            currentIndex = index;
            contentItems[currentIndex].Select();

            Vector3 targetPosition = content.localPosition;
            if (isHorizontal)
                targetPosition.x = -currentIndex * (contentItemWidth + layoutGroup.spacing);
            else
                targetPosition.y = currentIndex * (contentItemHeight + layoutGroup.spacing);

            content.localPosition = targetPosition;
            isSnapped = true;
        }
    }
}