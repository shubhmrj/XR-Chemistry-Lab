using UnityEngine;

namespace ManoMotion.Visualization
{
    /// <summary>
    /// Displays and modifies bounding box from hand tracking.
    /// </summary>
    public class BoundingBoxUI : MonoBehaviour
    {
        [SerializeField] LeftOrRightHand leftRightHand;
        [SerializeField] GameObject boundingBoxParent;
        [SerializeField, Min(0)] Vector2 paddingMultiplier;
        [SerializeField] LineRenderer boundingLineRenderer;

        [Header("Display information")]
        [SerializeField] bool displayBoundingBoxInformation;
        [SerializeField] TextMesh topLeftText, widthText, heightText;
        [SerializeField] float backgroundDepth = 8, textDepthModifier = 4, textAdjustment = 0.01f;

        BoundingBox boundingBox;
        bool activated = false;

        public bool Activated => activated;
        public BoundingBox BoundingBox => boundingBox;

        private void Start()
        {
            boundingLineRenderer.positionCount = 4;
        }

        private void Update()
        {
            activated = ManoMotionManager.Instance.TryGetHandInfo(leftRightHand, out HandInfo handInfo);
            boundingBoxParent.SetActive(activated);
            if (activated)
            {
                boundingBox = CalculateBoundingBox(handInfo.trackingInfo.skeleton);
                UpdateInfo(boundingBox);
            }
        }

        /// <summary>
        /// Calculates the bounding box based on the joint positions of the skeleton.
        /// </summary>
        private BoundingBox CalculateBoundingBox(SkeletonInfo skeleton)
        {
            // Find the lowest and highest value of each dimension
            Vector3[] positions = skeleton.jointPositions;
            float minX, maxX, minY, maxY;
            minX = maxX = positions[0].x;
            minY = maxY = positions[0].y;

            for (int i = 1; i < positions.Length; i++)
            {
                Vector3 position = positions[i];
                minX = Mathf.Min(minX, position.x);
                maxX = Mathf.Max(maxX, position.x);
                minY = Mathf.Min(minY, position.y);
                maxY = Mathf.Max(maxY, position.y);
            }

            // Add padding if we want to make the box slightly bigger (or smaller)
            float extraWidth = (maxX - minY) * paddingMultiplier.x;
            float extraHeight = (maxY - minY) * paddingMultiplier.y;
            minX = Mathf.Clamp01(minX - extraWidth);
            maxX = Mathf.Clamp01(maxX + extraWidth);
            minY = Mathf.Clamp01(minY - extraHeight);
            maxY = Mathf.Clamp01(maxY + extraHeight);

            // Use topLeft as starting position
            Vector3 topLeft = new Vector3(minX, maxY);
            float width = maxX - minX;
            float height = maxY - minY;
            BoundingBox bb = new BoundingBox() { topLeft = topLeft, width = width, height = height };
            return bb;
        }

        private void UpdateInfo(BoundingBox boundingBox)
        {
            float width = boundingBox.width;
            float height = boundingBox.height;

            // Calculate all corners
            Vector3 normalizedTopLeft = boundingBox.topLeft;
            Vector3 normalizedTopRight = normalizedTopLeft + Vector3.right * width;
            Vector3 normalizedBotLeft = normalizedTopLeft + Vector3.down * height;
            Vector3 normalizedBotRight = normalizedBotLeft + Vector3.right * width;

            // Draw lines for the borders
            boundingLineRenderer.SetPosition(0, ManoUtils.Instance.CalculateNewPositionWithDepth(normalizedTopLeft, backgroundDepth));
            boundingLineRenderer.SetPosition(1, ManoUtils.Instance.CalculateNewPositionWithDepth(normalizedTopRight, backgroundDepth));
            boundingLineRenderer.SetPosition(2, ManoUtils.Instance.CalculateNewPositionWithDepth(normalizedBotRight, backgroundDepth));
            boundingLineRenderer.SetPosition(3, ManoUtils.Instance.CalculateNewPositionWithDepth(normalizedBotLeft, backgroundDepth));

            topLeftText.gameObject.SetActive(displayBoundingBoxInformation);
            heightText.gameObject.SetActive(displayBoundingBoxInformation);
            widthText.gameObject.SetActive(displayBoundingBoxInformation);

            Quaternion cameraRotation = Camera.main.transform.rotation;

            // Display top left coordinates
            normalizedTopLeft.y += textAdjustment * 3;
            topLeftText.gameObject.transform.position = ManoUtils.Instance.CalculateNewPositionWithDepth(normalizedTopLeft, backgroundDepth / textDepthModifier);
            topLeftText.text = "Top Left: " + "X: " + normalizedTopLeft.x.ToString("F2") + " Y: " + normalizedTopLeft.y.ToString("F2");
            topLeftText.transform.rotation = cameraRotation;

            // Display height
            Vector3 normalizedTextHeightPosition = new Vector3(normalizedTopLeft.x + width + textAdjustment, normalizedTopLeft.y - height / 2f);
            heightText.transform.position = ManoUtils.Instance.CalculateNewPositionWithDepth(normalizedTextHeightPosition, backgroundDepth / textDepthModifier);
            heightText.text = "Height: " + height.ToString("F2");
            heightText.transform.rotation = cameraRotation;

            // Display width
            Vector3 normalizedTextWidth = new Vector3(normalizedTopLeft.x + width / 2, normalizedTopLeft.y - height - textAdjustment);
            widthText.transform.position = ManoUtils.Instance.CalculateNewPositionWithDepth(normalizedTextWidth, backgroundDepth / textDepthModifier);
            widthText.text = "Width: " + width.ToString("F2");
            widthText.transform.rotation = cameraRotation;
        }
    }
}