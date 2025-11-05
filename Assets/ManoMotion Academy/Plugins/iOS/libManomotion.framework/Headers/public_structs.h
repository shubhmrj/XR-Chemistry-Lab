//
//  public_structs.normalized_bounding_box_h
//  ManoMotion-iOS
//
//  Created by Julio chaná on 20/09/2018.
//  Copyright © 2018 ManoMotion. All rights reserved.
//
#pragma once

#include <opencv2/opencv.hpp>
#include "mano_defs.h"

/*
 * This public structs are meant to be shared with the user such as Unity
 @TODO provide default values the primitive types to avoid compiler warnings
 */


enum CAMERA_SELECTOR {
	RGB_DEFAULT = 0,
	RGB_EXTERNAL = 1,
};
enum INPUT_FEED_TYPE {
	CAMERA = 0,
	IMAGE = 1,
	VIDEO = 2, 
	REAL_SENSE = 3
};

 /*
  * Added new to return the license
  */
enum LicenseAnswer {
	LICENSE_OK = 30,
	LICENSE_KEY_NOT_FOUND = 31,
	LICENSE_EXPIRED = 32,
	LICENSE_INVALID_PLAN = 33,
	LICENSE_KEY_BLOCKED = 34,
	LICENSE_INVALID_ACCESS_TOKEN = 35,
	LICENSE_ACCESS_DENIED = 36,
	LICENSE_MAX_NUM_DEVICES = 37,
	LICENSE_UNKNOWN_SERVER_REPLY = 38,
	LICENSE_PRODUCT_NOT_FOUND = 39,
	LICENSE_INCORRECT_INPUT_PARAMETER = 40,
	LICENSE_INTERNET_REQUIRED = 41,
	LICENSE_INCORRECT_BUNDLE_ID = 42
};
struct LicenseStatus {
	LicenseAnswer license_answer = LICENSE_UNKNOWN_SERVER_REPLY;
	int machines_left = 0;
	int days_left = 0;
	float version;
};

struct AssetStatus {
	LicenseAnswer license_answer = LICENSE_UNKNOWN_SERVER_REPLY;
	float version;
};

// ---------------------------------
struct BoundingBox {
	cv::Point3f top_left;
	float width;
	float height;
};

struct WristInfo {
	cv::Point3f left_point;
	cv::Point3f right_point;
	int flag;
};

struct FingerInfo {
	cv::Point3f left_point;
	cv::Point3f right_point;
	int flag;
};

#ifndef WIN32 
// todo The struct 'hidden' below is defined in common/core/TRBaseInference.hpp 
// but that file is only included in #define WIN32. I (BERNARD) experienced some errors in Android
// so i decided to copy it here for other non-win32 platform. 
namespace manomotion {
	//#define GPU
	struct TRInputDim {
		int batch; //Number of batch, it is usually equal to 1 for inference
		int row; //Number of rows
		int col; //Number of columns
		int channel; //Number of channels, it is = 3 for RGB and =1 for grayscale
	};
}
#endif // __IS_ANDROID__


struct Quaternion {
	float qx, qy, qz, qw;
};

// For getting skeleton on images
//This is the output to Unity since Unity requires Point3f
struct MM_SkeletonOutput {
	float detection_confidence;//Test with one hand first
	cv::Point3f joints[SKELETON_JOINTS]; //21 joints for one hand
	//cv::Point3f orientation3d_output[SKELETON_JOINTS];
	//Quaternion orientation3d_output_quat[SKELETON_JOINTS];

};


struct World_SkeletonOutput {
	cv::Point3f joints[SKELETON_JOINTS]; //21 joints for one hand
	Quaternion orientation3d_output_quat[SKELETON_JOINTS];
};

enum CAMERA_MODEL {
	DEFAULT,
	FISHEYE,
	OMNIDIRECTIONAL
};



struct CameraInfo {
	//DEFAULT:
	// https://docs.opencv.org/master/d4/d94/tutorial_camera_calibration.html
	// distortion coefficients is a vector with 5 values: k1,k2,p1,p2,k3

	//FISHEYE:
	//https://docs.opencv.org/3.4/db/d58/group__calib3d__fisheye.html
	// distortion coefficients is a vector with 4 values: k1,k2,k3,k4


	//OMNIDIRECTIONAL:
	// https://docs.opencv.org/master/dd/d12/tutorial_omnidir_calib_main.html
	//distortion coefficients is a vector with 6 values

	CAMERA_MODEL model;

	//The size of image over normalized_bounding_box_x and normalized_bounding_box_y directions
	int image_x, image_y;

	float focus_x, focus_y, principal_point_x, principal_point_y;

	float distortion_coeffs[MAX_DISTORTION_COEFFS];

	//Camera extrinsics
	float extrinsic_params[EXTRINSIC_MATRIX];

};


struct MeshInfo {
	//Mesh: vertices, normals, and triangle indices
	//changed by Yeray to fit in Unity
	float vertices[VERTICES_MAX *3];
	float normals[VERTICES_MAX*3];
	unsigned int indices[INDICES_MAX*3];
	int number_of_vectices = 0;
	int num_face_indices = 0;
};

//Used to pass the meshinfo on c++. When we send it to Unity we have to copy it into an array with a defined size
struct MeshInfoInternal {
	//Mesh: vertices, normals, and triangle indices
	//Will be a dynamic array
	float* vertices = 0;
	float* normals = 0;
	unsigned int* indices = 0;
	int number_of_vectices = 0;
	int num_face_indices = 0;
};

struct MultiCameraInfo {
	CameraInfo* camera;
	int number_of_camera;
};

struct TrackingInfo {
	BoundingBox bounding_box;
	//cv::Point3f poi;
	//cv::Point3f palm_center;
	WristInfo wrist;
	float depth_estimation;
	//float rotation;
	int amount_of_contour_points = 0;
	//cv::Point3f finger_tips[5];
	cv::Point3f contour_points[CONTOUR_POINTS];
	MM_SkeletonOutput skeleton;  // For getting skeleton info on images

	//FIXME we should remove EULER angles
	//added since merge with arjo segmentation. 
	World_SkeletonOutput world_skeleton;

	FingerInfo ring_info;  // For getting skeleton info
#ifdef PROCESS_MESH
	MeshInfo mesh;
#endif
};

struct GestureInfo {
	int mano_class;
	int mano_gesture_continuous;
	int mano_gesture_trigger;
	int state;
	int hand_side;
	int is_right;
};

struct HandInfo {
	TrackingInfo tracking_info;
	GestureInfo gesture_info;
	int warning;
};

struct UserInputForGestures {
	HandInfo* hand_info;
	float gesture_smoother;
};

/*
1.-pinch_poi
2.-skeleton_in_3d
3.-enabled_gestures
4.-prediction
5.-wrist
6.-ring
7.-segmentation
8.- two hands
// if alpha mode, this
9. - test type (1=LIVE, 2=VIDEO_FEED , 3=PIXEL_READER), default=2
*/
struct Features {
	//int pinch_poi;
	int skeleton_in_3d;
	int enabled_gestures;
	int prediction;
	int wrist_info;
	int finger_info;
	int segmentation;
	int two_hands;
};
struct Session {
	int flag;
	int orientation;
	int add_on;
	float tracking_smoother;
	float gesture_smoother;
	Features features;
};

struct ManoSettings {
	int platform;
	int image_format;
	char* serial_key;

	//had to add this to allow the segmentation mode for varjo
	int seg_mode;

};


enum AddOn {
	ADD_ON_DEFAULT = 0,
	ADD_ON_ARKIT = 1,
	ADD_ON_CORE = 2,
	ADD_ON_VUFORIA = 3,
	ADD_ON_FOUNDATION = 4,
	ADD_ON_DIMENCO = 5,
	ADD_ON_XTAL = 6,
	ADD_ON_PICO=7,
	ADD_ON_VARJO = 8,
	ADD_ON_MAGIC_LEAP = 9,
	ADD_ON_TOBII_UNITY = 10, 
	ADD_ON_VOLVO_PENTA = 11,
	ADD_ON_CANON = 12,
	ADD_ON_KMAX_UNITY = 13,
	ADD_ON_VARJO_UNITY = 14,
	ADD_ON_PICO_4 = 15
};
/*
enum Orientations{
	LANDSCAPE_FRONT_FACING_LEFT = 8,
	LANDSCAPE_FRONT_FACING_RIGHT= 7,
	PORTRAIT_FRONT_FACING_INVERTED = 6,
	PORTRAIT_FRONT_FACING = 5,
	LANDSCAPE_RIGHT = 4 ,
	LANDSCAPE_LEFT = 3 ,
	PORTRAIT = 2 ,
	PORTRAIT_INVERTED = 1
};*/

enum Orientations
{
	UNKNOWN = 0,
	PORTRAIT_INVERTED = 1,
	PORTRAIT = 2,
	LANDSCAPE_LEFT = 3,
	LANDSCAPE_RIGHT = 4,
	FACE_UP = 5,
	FACE_DOWN = 6,
	PORTRAIT_FRONT_FACING = 7,
	PORTRAIT_UPSIDE_DOWN_FRONT_FACING = 8,
	LANDSCAPE_LEFT_FRONT_FACING = 9,
	LANDSCAPE_RIGHT_FRONT_FACING = 10
};
enum IMAGE_FORMAT {
	GRAYSCALE_UNITY_FORMAT = 9,
	NV12_FORMAT = 8,
	YUV_GRAY_FORMAT = 7, // Android Monochrome
	YUV_FORMAT = 6,
	GRAYSCALE_FORMAT = 5,
	BGRA_FORMAT = 4,
	RGBA_FORMAT = 3,
	RGB_FORMAT = 2,
	BGR_FORMAT = 1
};

enum POSE_DETECTION_MODE {
	POSE_DETECTION_SEARCH = 0,
	POSE_DETECTION_TRACK = 1
};


struct BodyPoseHandOut {
	std::vector<cv::Point3f> landmarks;
	std::vector<float> presence;
	std::vector<float> visibility;
	std::vector<HandInfo>hands_info;
};

struct BodyPoseOut {
	std::vector<cv::Point3f> landmarks;
	std::vector<float> presence;
	int pose_id;
	POSE_DETECTION_MODE detect_mode;
};

struct BodyPoseOutWithConf {
	std::vector<cv::Point3f> landmarks;
	std::vector<float> presence;
	int pose_id;
	float confidence = 1.0;
};

struct ManoMotionInfo {
	std::vector<BodyPoseHandOut>persons_info;
};

struct CameraConfigOpenCV {
	int camera_index = -1;
	int width = -1;
	int height=-1;
	int auto_white_balance=-1;
	int auto_exposure=-1;
};
struct ManoMotionStruct {
#ifdef _RETURN_BODY_TRACKING_
	ManoMotionInfo mm_info;
#else
	HandInfo hand_0; // output
	HandInfo hand_1; // output
#endif

	cv::Mat left_input_img; // output
	cv::Mat right_input_img; // output
	std::string feed_file_path; // input for image and video and video
	std::string feed_file_name; // input for image and video and video
	CameraConfigOpenCV cam_config;
	bool split_image_in_two = false;
	cv::Mat right_mask; // used for returning the hand masks
	cv::Mat left_mask;
	bool stereo_input = false;
};

/* public_structs_h */
