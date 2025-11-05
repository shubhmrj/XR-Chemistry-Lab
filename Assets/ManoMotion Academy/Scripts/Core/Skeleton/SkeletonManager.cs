using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace ManoMotion
{
    /// <summary>
    /// Enum to list the different skeleton models
    /// </summary>
    public enum SkeletonModel { SKEL_2D = 0, SKEL_3D = 1 }

    /// <summary>
    /// Handles the visualization of the skeleton joints.
    /// </summary>
    public class SkeletonManager : MonoBehaviour
    {
        [SerializeField] bool shouldShowSkeleton = true;
        [SerializeField] bool useSkeletonSmoothing = true;
        [SerializeField] GameObject skeletonPrefab2D, skeletonPrefab3D;

        ///The list of joints used for visualization
        List<GameObject> joints = new List<GameObject>(new GameObject[21]);
        List<GameObject> jointsSecond = new List<GameObject>(new GameObject[21]);

        ///Skeleton confidence
        private float skeletonConfidenceThreshold = 0.0001f;
        private GameObject skeletonParent;
        private GameObject skeletonModel, skeletonModelSecond;
        private Renderer[] renderers, renderersSecond;

        private OneEuroFilterSetting positionFilterSetting = new OneEuroFilterSetting(120, 0.0001f, 500f, 1f);
        private OneEuroFilterSetting depthFilterSetting = new OneEuroFilterSetting(120, 0.0001f, 500f, 1f);
        private List<OneEuroFilter<Vector3>> positionFilter = new List<OneEuroFilter<Vector3>>();
        private List<OneEuroFilter<Vector3>> positionFilterSecond = new List<OneEuroFilter<Vector3>>();
        private OneEuroFilter<Vector2>[] handDepthFilters = new OneEuroFilter<Vector2>[2];
        private Quaternion[] handRotations = new Quaternion[2];
 
        const float FadeTime = 0.01f; // The time it takes to fade in/out the skeleton.
        const int JointsLength = 21; // The number of Joints the skeleton is made of.

        static SkeletonManager instance;

        public static SkeletonManager Instance => instance;

        public bool ShouldShowSkeleton
        {
            get { return shouldShowSkeleton; }
            set { shouldShowSkeleton = value; }
        }

        public static UnityEvent OnSkeletonChanged = new UnityEvent();

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
            }
            else
            {
                gameObject.SetActive(false);
                Debug.LogWarning("More than 1 SkeletonManager in scene");
            }

            CreateSkeletonParent();
            ChangeSkeletonModel(SkeletonModel.SKEL_3D);
            CreateOneEuroFilters();
        }

        private void OnEnable()
        {
            ManoMotionManager.OnSkeletonActivated += ChangeSkeletonModel;
        }

        private void OnDisable()
        {
            ManoMotionManager.OnSkeletonActivated -= ChangeSkeletonModel;
        }

        private void CreateSkeletonParent()
        {
            skeletonParent = new GameObject();
            skeletonParent.name = "SkeletonParent";
        }

        /// <summary>
        /// Creates a OneEuroFilter for each of the joints
        /// </summary>
        private void CreateOneEuroFilters()
        {
            for (int i = 0; i < handDepthFilters.Length; i++)
                handDepthFilters[i] = new OneEuroFilter<Vector2>(depthFilterSetting);

            for (int i = 0; i < JointsLength; i++)
            {
                positionFilter.Add(new OneEuroFilter<Vector3>(positionFilterSetting));
                positionFilterSecond.Add(new OneEuroFilter<Vector3>(positionFilterSetting));
            }
        }

        /// <summary>
        /// Changes the current skeleton model for a new one
        /// </summary>
        /// <param name="skeleton">Type of skeleton (2D or 3D)</param>
        private void ChangeSkeletonModel(SkeletonModel skeleton)
        {
            GameObject modelToLoad = skeleton == SkeletonModel.SKEL_3D ? skeletonPrefab3D : skeletonPrefab2D;

            joints.Clear();
            jointsSecond.Clear();
            Destroy(skeletonModel);
            Destroy(skeletonModelSecond);

            skeletonModel = Instantiate(modelToLoad, skeletonParent.transform);
            skeletonModelSecond = Instantiate(modelToLoad, skeletonParent.transform);

            for (int i = 0; i < skeletonModel.transform.childCount; i++)
            {
                joints.Add(skeletonModel.transform.GetChild(i).gameObject);
                jointsSecond.Add(skeletonModelSecond.transform.GetChild(i).gameObject);
            }

            renderers = joints[0].transform.parent.gameObject.GetComponentsInChildren<Renderer>();
            renderersSecond = jointsSecond[0].transform.parent.gameObject.GetComponentsInChildren<Renderer>();

            OnSkeletonChanged?.Invoke();
        }

        void Update()
        {
            HandInfo[] handInfos = ManoMotionManager.Instance.HandInfos;

            for (int i = 0; i < handInfos.Length; i++)
            {
                var jointRenderers = i == 0 ? renderers : renderersSecond;
                FadeSkeletonJoints(jointRenderers, handInfos[i]);
                List<GameObject> joints = GetJoints(i);
                List<OneEuroFilter<Vector3>> posFilter = i == 0 ? positionFilter : positionFilterSecond;
                UpdateJointsPosition(ref joints, handInfos[i], posFilter, handDepthFilters[i]);
                UpdateJointsOrientation(ref joints, handInfos[i], i);
            }
        }

        private void UpdateJointsPosition(ref List<GameObject> skeletonJoints, HandInfo handInfo, List<OneEuroFilter<Vector3>> oneEuroFiltersToApply, OneEuroFilter<Vector2> depthFilter)
        {
            SkeletonInfo skeletonInfo = handInfo.trackingInfo.skeleton;
            WorldSkeletonInfo worldSkeletonInfo = handInfo.trackingInfo.worldSkeleton;
            float depthEstimation = handInfo.trackingInfo.depthEstimation;

            float handDepth = Mathf.Clamp(depthEstimation, 0.1f, 1);
            if (useSkeletonSmoothing)
                handDepth = depthFilter.Filter(new Vector2(handDepth, 0)).x;

            if (skeletonInfo.jointPositions != null)
            {
                for (int i = 0; i < skeletonInfo.jointPositions.Length; i++)
                {
                    Vector3 jointDepth = skeletonInfo.jointPositions[i];
                    jointDepth.z = worldSkeletonInfo.jointPositions[i].z * 0.3f;

                    Vector3 newPosition3D = ManoUtils.Instance.CalculateNewPositionWithDepth(jointDepth, handDepth);
                    if (useSkeletonSmoothing)
                        newPosition3D = oneEuroFiltersToApply[i].Filter(newPosition3D);       
                    skeletonJoints[i].transform.position = newPosition3D;
                }
            }
        }

        /// <summary>
        /// Updates the orientation of the joints according to the orientation given by the SDK.
        /// </summary>
        private void UpdateJointsOrientation(ref List<GameObject> listToModify, HandInfo handInfo, int handIndex)
        {
            if (handInfo.trackingInfo.skeleton.confidence == 0)
                return;

            Quaternion handRotation = RotationUtility.GetHandRotation(handInfo);
            handRotations[handIndex] = handRotation;

            for (int i = 0; i < listToModify.Count; i++)
            {
                if (listToModify[i].TryGetComponent(out LookAtJoint joint))
                {
                    Quaternion rotation = handRotation;
                    if (i > 0)
                        rotation *= RotationUtility.GetFingerJointRotation(handInfo, i);
                    joint.UpdateRotation(rotation, handInfo.gestureInfo.handSide);
                }
            }
        }

        /// <summary>
        /// Fade the skeleton materials when no hand is detected.
        /// </summary>
        /// <param name="renderers">The renderers of the skeleton model</param>
        /// <param name="handInfo">the hand info using the skeleton model</param>
        private void FadeSkeletonJoints(Renderer[] renderers, HandInfo handInfo)
        {
            SkeletonInfo skeletonInfo = handInfo.trackingInfo.skeleton;
            bool hasConfidence = skeletonInfo.confidence > skeletonConfidenceThreshold;
            float alphaChange = (1f / FadeTime) * Time.deltaTime;

            if (!hasConfidence || !shouldShowSkeleton)
                alphaChange *= -1f;

            for (int i = 0; i < renderers.Length; i++)
            {
                Color tempColor = renderers[i].material.color;
                float alpha = Mathf.Clamp01(tempColor.a + alphaChange);
                tempColor.a = alpha;
                renderers[i].material.color = tempColor;
            }
        }

        /// <summary>
        /// Returns pure hand rotation value.
        /// </summary>
        public Quaternion GetHandRotation(int handIndex)
        {
            return handRotations[handIndex];
        }

        /// <summary>
        /// Returns a specific skeleton joint.
        /// </summary>
        public GameObject GetJoint(int handIndex, int jointIndex)
        {
            return handIndex switch
            {
                0 => joints[jointIndex],
                1 => jointsSecond[jointIndex],
                _ => null
            };
        }

        /// <summary>
        /// Returns all joints of the specified hand (0-1).
        /// </summary>
        /// <param name="handIndex">Specific hand, corresponds to same index in ManoMotionManager handInfos.</param>
        public List<GameObject> GetJoints(int handIndex)
        {
            return handIndex switch
            {
                0 => joints,
                1 => jointsSecond,
                _ => null
            };
        }
    }
}