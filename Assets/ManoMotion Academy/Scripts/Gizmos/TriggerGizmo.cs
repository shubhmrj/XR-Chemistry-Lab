using TMPro;
using UnityEngine;

namespace ManoMotion.Gizmos
{
    /// <summary>
    /// Handles the visualization of the trigger gizmos.
    /// </summary>
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class TriggerGizmo : MonoBehaviour
    {
        [SerializeField] AnimationCurve fadeAnimation;
        [SerializeField] float fadeTime = 2f;
        [SerializeField] Color clickColor, pickColor, dropColor, grabColor, releaseColor, swipeColor;

        private float currentAlphaValue = 1f;
        float currentFadeTime = 0;
        bool isFading;
        TextMeshProUGUI triggerLabelText;
        Vector3 originalScale;

        bool stayOnScreen = false;

        void Awake()
        {
            triggerLabelText = GetComponent<TextMeshProUGUI>();
            originalScale = transform.localScale;
        }

        private void OnEnable()
        {
            currentAlphaValue = 1f;
            Color color = triggerLabelText.color;
            color.a = currentAlphaValue;
            triggerLabelText.color = color;
        }

        public void SetScale(Vector3 scale)
        {
            originalScale = scale;
        }

        void FixedUpdate()
        {
            if (!stayOnScreen)
            {
                FadeAndExpand();
            }
        }

        private void FadeAndExpand()
        {
            if (isFading)
            {
                currentFadeTime += Time.deltaTime;
                currentAlphaValue = 1 - fadeAnimation.Evaluate(currentFadeTime / fadeTime);
                Color color = triggerLabelText.color;
                color.a = currentAlphaValue;
                triggerLabelText.color = color;

                if (currentFadeTime > fadeTime)
                {
                    isFading = false;
                }
            }
            else
            {
                currentFadeTime = 0;
                currentAlphaValue = 1;
                Color color = triggerLabelText.color;
                color.a = 1;
                triggerLabelText.color = color;
                gameObject.SetActive(false);
            }
        }

        public virtual void InitializeTriggerGizmo(ManoGestureTrigger triggerGesture)
        {
            transform.localScale = originalScale;
            isFading = true;
            if (!triggerLabelText)
            {
                triggerLabelText = GetComponent<TextMeshProUGUI>();
            }

            switch (triggerGesture)
            {
                case ManoGestureTrigger.CLICK:
                    triggerLabelText.text = "Click";
                    triggerLabelText.color = clickColor;
                    break;
                case ManoGestureTrigger.DROP:
                    triggerLabelText.text = "Drop";
                    triggerLabelText.color = dropColor;
                    break;
                case ManoGestureTrigger.PICK:
                    triggerLabelText.text = "Pick";
                    triggerLabelText.color = pickColor;
                    break;
                case ManoGestureTrigger.GRAB_GESTURE:
                    triggerLabelText.text = "Grab";
                    triggerLabelText.color = grabColor;
                    break;
                case ManoGestureTrigger.RELEASE_GESTURE:
                    triggerLabelText.text = "Release";
                    triggerLabelText.color = releaseColor;
                    break;
                case ManoGestureTrigger.SWIPE_LEFT:
                    triggerLabelText.text = "Left";
                    triggerLabelText.color = swipeColor;
                    break;
                case ManoGestureTrigger.SWIPE_RIGHT:
                    triggerLabelText.text = "Right";
                    triggerLabelText.color = swipeColor;
                    break;
                case ManoGestureTrigger.SWIPE_UP:
                    triggerLabelText.text = "Up";
                    triggerLabelText.color = swipeColor;
                    break;
                case ManoGestureTrigger.SWIPE_DOWN:
                    triggerLabelText.text = "Down";
                    triggerLabelText.color = swipeColor;
                    break;
            }
        }

        public void StartFading()
        {
            isFading = true;
        }
    }
}