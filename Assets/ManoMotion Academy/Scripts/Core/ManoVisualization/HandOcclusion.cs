using UnityEngine;

namespace ManoMotion.Visualization
{
    /// <summary>
    /// Displays a hand cutout on a mesh in world space.
    /// </summary>
    public class HandOcclusion : MonoBehaviour
    {
        [SerializeField] MeshRenderer manomotionGenericTransparentLayer;
        [SerializeField] Texture2D noHandTexture;

        MeshRenderer[] occlusionRenderers = new MeshRenderer[2];
        Transform[] wristJoints = new Transform[2];
        Texture2D[] occlusionRGBTextures = new Texture2D[2];

        bool showHandOcclusion = true;

        private void Awake()
        {
            for (int i = 0; i < occlusionRenderers.Length; i++)
            {
                occlusionRenderers[i] = Instantiate(manomotionGenericTransparentLayer);
                occlusionRenderers[i].transform.name = $"Hand Occlusion {i}";
                occlusionRenderers[i].transform.SetParent(Camera.main.transform);
            }
        }

        private void Start()
        {
            // Cache references
            SetWristJoints();
            occlusionRGBTextures[0] = ManoMotionManager.Instance.VisualizationInfo.occlusionRGB;
            occlusionRGBTextures[1] = ManoMotionManager.Instance.VisualizationInfo.occlusionRGBsecond;
        }

        private void Update()
        {
            for (int i = 0; i < occlusionRenderers.Length; i++)
            {
                bool hand = ManoMotionManager.Instance.HandInfos[i].gestureInfo.manoClass != ManoClass.NO_HAND;
                occlusionRenderers[i].enabled = showHandOcclusion && hand;

                if (hand)
                {
                    // Depth
                    float depth = Vector3.Distance(Camera.main.transform.position, wristJoints[i].position);
                    occlusionRenderers[i].transform.localPosition = new Vector3(0, 0, depth);
                    occlusionRenderers[i].material.mainTexture = occlusionRGBTextures[i];

                    // Towards camera
                    occlusionRenderers[i].transform.rotation = Camera.main.transform.rotation;

                    // Fit to screen
                    ManoUtils.Instance.OrientMeshRenderer(occlusionRenderers[i]);
                    ManoUtils.Instance.AdjustBorders(occlusionRenderers[i], ManoMotionManager.Instance.ManomotionSession);
                }
                else
                {
                    occlusionRenderers[i].material.mainTexture = noHandTexture;
                }
            }
        }

        private void OnEnable()
        {
            ManoMotionManager.OnContourToggle += ContourToggle;
            SkeletonManager.OnSkeletonChanged.AddListener(SetWristJoints);
        }

        private void OnDisable()
        {
            ManoMotionManager.OnContourToggle -= ContourToggle;
            SkeletonManager.OnSkeletonChanged.RemoveListener(SetWristJoints);
        }

        private void ContourToggle(bool active)
        {
            showHandOcclusion = active;
        }

        private void SetWristJoints()
        {
            wristJoints[0] = SkeletonManager.Instance.GetJoint(0, 0).transform;
            wristJoints[1] = SkeletonManager.Instance.GetJoint(1, 0).transform;
        }
    }
}