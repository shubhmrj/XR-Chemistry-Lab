using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ManoMotion.Demos
{
    /// <summary>
    /// Settings for "scenes" when using SceneSelection.
    /// </summary>
    [RequireComponent(typeof(RectTransform), typeof(Image))]
    public partial class SceneOption : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] SceneSelection sceneSelection;
        [SerializeField] GameObject sceneParent;
        [SerializeField] bool frontFacingCamera = false, showSkeleton = true, lockToLandscape = false, handOcclusion = false;
        [SerializeField] ProcessingType processingType = ProcessingType.Async;
        [SerializeField] Color selectedColor;
        [SerializeField] UnityEvent OnSelected, OnDeselected;

        Image image;
        Color baseColor;

        public float Width { get; private set; }
        public float Height { get; private set; }
        private Image Image
        {
            get
            {
                if (image == null)
                {
                    image = GetComponent<Image>();
                    baseColor = image.color;
                }
                return image;
            }
        }

        void Awake()
        {
            if (!image)
            {
                image = GetComponent<Image>();
                baseColor = image.color;
            }

            Width = GetComponent<RectTransform>().rect.width;
            Height = GetComponent<RectTransform>().rect.height;
        }

        public virtual void Select()
        {
            Image.color = selectedColor;

            if (gameObject.activeInHierarchy)
            {
                sceneParent.SetActive(true);
                UpdateSessionFeatures();
                OnSelected?.Invoke();
            }
        }

        public virtual void Deselect()
        {
            Image.color = baseColor;

            if (gameObject.activeInHierarchy)
            {
                sceneParent.SetActive(false);
                OnDeselected?.Invoke();
            }
        }

        private void UpdateSessionFeatures()
        {
            ManoMotionManager manager = ManoMotionManager.Instance;
            manager.InputManager.SetFrontFacing(frontFacingCamera);  
            manager.SetProcessingType(processingType);
            manager.ShouldRunContour(handOcclusion);
            SkeletonManager.Instance.ShouldShowSkeleton = showSkeleton;

            ManoUtils.Instance.updateOrientation = !lockToLandscape;
            if (lockToLandscape)
            {
                Screen.orientation = ScreenOrientation.LandscapeLeft;
                ManoUtils.Instance.SetOrientation(SupportedOrientation.LANDSCAPE_LEFT);
            }
            else
            {
                Screen.orientation = ScreenOrientation.AutoRotation;
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            sceneSelection.Snap(transform.GetSiblingIndex());
        }
    }
}