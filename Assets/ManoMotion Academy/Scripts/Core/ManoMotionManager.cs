using UnityEngine;
using System;
using ManoMotion.CameraSystem;

namespace ManoMotion
{
    /// <summary>
    /// The ManomotionManager handles the communication with the SDK.
    /// </summary>
    [RequireComponent(typeof(InputManagerBase))]
    public class ManoMotionManager : ManoMotionManagerBase
    {
        IntPtr renderEventFunc;

        protected static ManoMotionManager instance;

        public static ManoMotionManager Instance
        {
            get
            {
                if (instance == null)
                    instance = FindObjectOfType<ManoMotionManager>();
                return instance;
            }        
        }

        protected override void Awake()
        {
            inputManager = GetComponent<InputManagerBase>();

            if (instance && instance != this)
            {
                gameObject.SetActive(false);
                Debug.LogWarning("More than 1 ManoMotionManager in scene");
                return;
            }

            base.Awake();
            instance = this;

            Init();

            // Setup multithreading processing 
            renderEventFunc = GetRenderEventFunc();
        }

        void Start()
        {
            // Disable and enable at start to fix issue with delayed processing.
            gameObject.SetActive(false);
            gameObject.SetActive(true);
        }

        void Update()
        {
            CalculateFPSAndProcessingTime();

            if (processingType.Equals(ProcessingType.Async))
                ProcessFrameAsync();
        }

        protected virtual void OnEnable()
        {
            ManoUtils.OnOrientationChanged += HandleOrientationChanged;
            InputManagerBase.OnChangeCamera += HandleOrientationChanged;
            InputManagerBase.OnCameraFacingChanged += CameraFacingChanged;
            InputManagerBase.OnAddonSet += HandleAddOnSet;
            InputManagerBase.OnFrameInitialized += HandleManomotionFrameUpdated;
            InputManagerBase.OnFrameResized += HandleManomotionFrameUpdated;
            InputManagerBase.OnFrameUpdated += HandleNewFrame;
            InputManagerBase.OnFrameInitializedPointer += HandleManoMotionFrameInitializedPointer;      
        }

        protected virtual void OnDisable()
        {
            ManoUtils.OnOrientationChanged -= HandleOrientationChanged;
            InputManagerBase.OnChangeCamera -= HandleOrientationChanged;
            InputManagerBase.OnCameraFacingChanged -= CameraFacingChanged;
            InputManagerBase.OnAddonSet -= HandleAddOnSet;
            InputManagerBase.OnFrameInitialized -= HandleManomotionFrameUpdated;
            InputManagerBase.OnFrameResized -= HandleManomotionFrameUpdated;
            InputManagerBase.OnFrameUpdated -= HandleNewFrame;
            InputManagerBase.OnFrameInitializedPointer -= HandleManoMotionFrameInitializedPointer;
        }

        /// <summary>
        /// Handles changes to Manomotion frame
        /// </summary>
        protected void HandleManomotionFrameUpdated(ManoMotionFrame newFrame)
        {
            if (newFrame.texture == null)
                return;
            currentManomotionFrame = newFrame;
            SetResolutionValues(newFrame.texture.width, newFrame.texture.height);
        }

        /// <summary>
        /// Set camera frame pointer for the SDK to process with multithreading
        /// </summary>
        protected void HandleManoMotionFrameInitializedPointer(Texture2D image, int splittingFactor)
        {
            SetTextureFromUnity(image.GetNativeTexturePtr(), image.width, image.height, splittingFactor);
        }

        /// Stops the SDK from processing.
        public void StopProcessing()
        {
            stop();
        }    

        /// <summary>
        /// Respond to the event of a ManoMotionFrame being updated.
        /// Processes the image if runMultithreading is false
        /// </summary>
        /// <param name="newFrame">The new camera frame</param>
        protected virtual void HandleNewFrame(ManoMotionFrame newFrame)
        {
            // Update camera rgb texture
            visualizationInfo.rgbImage = newFrame.texture;
            visualizationInfo.rgbImage.Apply();

            if (processingType.Equals(ProcessingType.Sync))
            {
                ProcessFrameSync(newFrame);
            }

            // Update hand occlusion textures
            visualizationInfo.occlusionRGB.SetPixels32(occlusionPixelsBuffer);
            visualizationInfo.occlusionRGB.Apply();
            visualizationInfo.occlusionRGBsecond.SetPixels32(occlusionPixelsBuffer1);
            visualizationInfo.occlusionRGBsecond.Apply();

            OnManoMotionFrameProcessed?.Invoke();
        }

        /// <summary>
        /// Processes the current frame and updates the handInfos
        /// </summary>
        protected void ProcessFrameSync(ManoMotionFrame frame)
        {
            setLeftFrameArray(frame.texture.GetPixels32());
            long start = DateTime.UtcNow.Millisecond + DateTime.UtcNow.Second * 1000 + DateTime.UtcNow.Minute * 60000;
            processFrameTwoHands(ref handInfos[0], ref handInfos[1], ref manomotionSession);
            long end = DateTime.UtcNow.Millisecond + DateTime.UtcNow.Second * 1000 + DateTime.UtcNow.Minute * 60000;
            if (start < end)
                processingTimeList.Add((int)(end - start));
        }

        /// <summary>
        /// Process frame and update handinfos without stopping Unitys main thread.
        /// </summary>
        protected void ProcessFrameAsync()
        {
            if (inputManager.IsFrameUpdated())
                GL.IssuePluginEvent(renderEventFunc, 1);
            int frameNumber = copyHandInfo(ref handInfos[0], ref handInfos[1], ref manomotionSession);
        }

        protected void HandleAddOnSet(AddOn addon)
        {
            manomotionSession.addOn = addon;
        }

        /// <summary>
        /// Updates the orientation information as captured from the device to the Session
        /// </summary>
        protected void HandleOrientationChanged()
        {
            manomotionSession.orientation = ManoUtils.Instance.Orientation;

            if (inputManager.IsFrontFacing)
            {
                manomotionSession.orientation = manomotionSession.orientation switch
                {
                    SupportedOrientation.PORTRAIT => SupportedOrientation.PORTRAIT_FRONT_FACING,
                    SupportedOrientation.PORTRAIT_UPSIDE_DOWN => SupportedOrientation.PORTRAIT_UPSIDE_DOWN_FRONT_FACING,
                    SupportedOrientation.LANDSCAPE_LEFT => SupportedOrientation.LANDSCAPE_LEFT_FRONT_FACING,
                    SupportedOrientation.LANDSCAPE_RIGHT => SupportedOrientation.LANDSCAPE_RIGHT_FRONT_FACING,
                    _ => SupportedOrientation.UNKNOWN
                };
            }

            HandleManomotionFrameUpdated(currentManomotionFrame);
        }

        protected void CameraFacingChanged()
        {
            manomotionSession.orientation = ManoUtils.Instance.Orientation;

            if (inputManager.IsFrontFacing)
            {
                manomotionSession.orientation = manomotionSession.orientation switch
                {
                    SupportedOrientation.PORTRAIT => SupportedOrientation.PORTRAIT_FRONT_FACING,
                    SupportedOrientation.PORTRAIT_UPSIDE_DOWN => SupportedOrientation.PORTRAIT_UPSIDE_DOWN_FRONT_FACING,
                    SupportedOrientation.LANDSCAPE_LEFT => SupportedOrientation.LANDSCAPE_LEFT_FRONT_FACING,
                    SupportedOrientation.LANDSCAPE_RIGHT => SupportedOrientation.LANDSCAPE_RIGHT_FRONT_FACING,
                    _ => SupportedOrientation.UNKNOWN
                };
            }
        }
    }
}