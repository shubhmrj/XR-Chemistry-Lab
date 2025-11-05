using UnityEngine;
using UnityEngine.UI;

namespace ManoMotion.Visualization
{
    /// <summary>
    /// Displays a hand cutout on a canvas
    /// </summary>
    public class HandOcclusionARFoundation : MonoBehaviour
    {
        [SerializeField] Canvas[] cutoutCanvases = new Canvas[2];
        [SerializeField] RawImage[] handCutouts = new RawImage[2];
        [SerializeField] Texture2D noHandTexture;
        bool showHandOcclusion = true;

        Texture2D[] occlusionRGBTextures = new Texture2D[2];

        private void Start()
        {
            showHandOcclusion = ManoMotionManager.Instance.ManomotionSession.enabledFeatures.contour == 1;
            occlusionRGBTextures[0] = ManoMotionManager.Instance.VisualizationInfo.occlusionRGB;
            occlusionRGBTextures[1] = ManoMotionManager.Instance.VisualizationInfo.occlusionRGBsecond;
        }

        private void LateUpdate()
        {
            for (int i = 0; i < cutoutCanvases.Length; i++)
            {
                Canvas cutoutCanvas = cutoutCanvases[i];
                cutoutCanvas.gameObject.SetActive(showHandOcclusion);

                if (!showHandOcclusion)
                    continue;

                HandInfo handInfo = ManoMotionManager.Instance.HandInfos[i];
                Texture2D texture = occlusionRGBTextures[i];
                SetCutoutTexture(handInfo, cutoutCanvas, handCutouts[i], texture, i);
            }
        }

        private void SetCutoutTexture(HandInfo handInfo, Canvas canvas, RawImage image, Texture2D cutoutTexture, int index)
        {
            if (handInfo.gestureInfo.manoClass != ManoClass.NO_HAND)
            {
                image.texture = cutoutTexture;
                GameObject joint = SkeletonManager.Instance.GetJoint(index, 0);
                float depth = Vector3.Distance(Camera.main.transform.position, joint.transform.position);
                canvas.planeDistance = depth;
            }
            else
            {
                image.texture = noHandTexture;
            }
        }

        private void OnEnable()
        {
            ManoMotionManager.OnContourToggle += ContourToggle;
        }

        private void OnDisable()
        {
            ManoMotionManager.OnContourToggle -= ContourToggle;
        }

        private void ContourToggle(bool active)
        {
            showHandOcclusion = active;
        }
    }
}