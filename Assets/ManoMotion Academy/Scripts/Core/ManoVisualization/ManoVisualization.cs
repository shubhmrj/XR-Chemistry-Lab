using UnityEngine;

namespace ManoMotion.Visualization
{
    /// <summary>
    /// Shows the camera as a background on a mesh in world space.
    /// </summary>
    public class ManoVisualization : MonoBehaviour
    {
        [SerializeField] MeshRenderer manomotionGenericLayerPrefab;
        [SerializeField] bool showBackground;
        [SerializeField] float backgroundDepth;
        [SerializeField] Texture2D noHandTexture;

        MeshRenderer backgroundMeshRenderer;

        public bool ShowBackground
        {
            get { return showBackground; }
            set { showBackground = value; }
        }

        private void Awake()
        {
            backgroundMeshRenderer = Instantiate(manomotionGenericLayerPrefab);
            backgroundMeshRenderer.transform.name = "Background";
            backgroundMeshRenderer.transform.SetParent(Camera.main.transform);
            backgroundMeshRenderer.transform.localPosition = new Vector3(0, 0, backgroundDepth);
        }

        private void LateUpdate()
        {
            backgroundMeshRenderer.enabled = showBackground;
            if (showBackground)
            {
                // Make the background mesh fill the screen
                backgroundMeshRenderer.material.mainTexture = ManoMotionManager.Instance.VisualizationInfo.rgbImage;
                ManoUtils.Instance.OrientMeshRenderer(backgroundMeshRenderer);
                ManoUtils.Instance.AdjustBorders(backgroundMeshRenderer, ManoMotionManager.Instance.ManomotionSession);
            }
        }
    }
}