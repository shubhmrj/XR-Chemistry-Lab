using System;
using UnityEngine;

namespace ManoMotion
{
    /// <summary>
    /// This tells the SDK if you are using any add ons like ARFoundations.
    /// </summary>
    [Serializable]
    public enum AddOn
    {
        DEFAULT = 0,
        ARFoundation = 4,
        FrontFacing = 5
    }

    /// <summary>
    /// Provides information regarding the different orientation types supported by the SDK.
    /// </summary>
    [Serializable]
    public enum SupportedOrientation
    {
        UNKNOWN = 0,
        PORTRAIT = 1,
        PORTRAIT_UPSIDE_DOWN = 2,
        LANDSCAPE_LEFT = 3,
        LANDSCAPE_RIGHT = 4,
        FACE_UP = 5,
        FACE_DOWN = 6,
        PORTRAIT_FRONT_FACING = 7,
        PORTRAIT_UPSIDE_DOWN_FRONT_FACING = 8,
        LANDSCAPE_LEFT_FRONT_FACING = 9,
        LANDSCAPE_RIGHT_FRONT_FACING = 10,
    }

    /// <summary>
    /// Provides additional information regarding the lincenses taking place in this application.
    /// </summary>
    [Serializable]
    public enum Flags
    {
        FLAG_IMAGE_SIZE_IS_ZERO = 1000,
        FLAG_IMAGE_IS_TOO_SMALL = 1001,
        ANDROID_SAVE_BATTERY_ON = 2000
    }

    /// <summary>
    /// Information reagarding the sessions sent to the SDK every frame.
    /// </summary>
    [Serializable]
    public struct Session
    {
        /// Information about imgae size.  
        public Flags flag;

        /// The current orientation of the device
        public SupportedOrientation orientation;

        /// Inforamtion if the SDK is used together with add on such as AR Foundation.
        public AddOn addOn;

        /// The current Tracking smoothing value (0-1) for the finger & wrist information.
        [Range(0, 1)] public float smoothingController;

        /// The current Tracking smoothing value (0-1) for the gestures.
        [Range(0, 1)] public float gestureSmoothingController;

        /// Information about which features to run.
        public Features enabledFeatures;
    }

    /// <summary>
    /// 1 for using it and 0 for not using it, for skeleton it´s either 3d if 1 or 2d if 0. 
    /// </summary>
    [Serializable]
    public struct Features
    {
        /// 1 = 3D skeleton, 0 = 2D skeleton.
        [Range(0, 1)] public int skeleton3D;

        /// 1 = gesturs on, 0 = gestures off.
        [Range(0, 1)] public int gestures;

        /// 1 = fast mode on, 0 = fast mode off.
        [Range(0, 1)] public int fastMode;

        /// 1 = wrist info on, 0 = wrist info off.
        [Range(0, 1)] public int wristInfo;

        /// 0 = finger info off.
        /// 1 = Thumb.
        /// 2 = Index Finger.
        /// 3 = Middle Finger.
        /// 4 = Ring Finger.
        /// 5 = Pinky.
        [Range(0, 5)] public int fingerInfo;

        /// 1 = contour on, 0 = contour off.
        [Range(0, 1)] public int contour;

        /// 1 = two hands on, 0 = two hands off.
        /// Should always be set to 1 now.
        [Range(1, 1)] public int twoHands;
    }
}