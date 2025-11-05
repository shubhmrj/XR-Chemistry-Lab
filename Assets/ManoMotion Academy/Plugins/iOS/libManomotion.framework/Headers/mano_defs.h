#pragma once
//  Created by Thang Nguyen on 18/03/2019.
//  Copyright 2019 ManoMotion. All rights reserved.
//
#include <string>

#if defined(SAVE_SKELETON_DETAILS) || defined(SHOW_PROCESS)
enum IMAGE_TYPE {
	FULL_IMAGE = 0,
	CROPPED = 1
};
#endif
//FIXME, everytime we update a new version
static const float sdk_version = 2; //used for Unity
static const char* sdk_version_string = "P2"; // Required for the data collection


//used for keeping track which frame we are processing across the SDK in order to save this information and being able to relate the images in the different classes
extern int global_frame_number;

//USED IN ANDORID to get the different type of paths
enum ACCELERATOR_TYPE {
	CPU_ACC = 0,
	GPU_ACC = 1,
	XNNPACK_ACC = 2
};

//USED IN ANDORID to get the different type of paths
enum PATH_TYPE {
	INTERNAL = 0,
	EXTERNAL = 1
};
//For microseconds
#define TIME_TYPE std::chrono::microseconds
//#define TIME_UNIT "us"
//std::string string_version = to_string(llround(sdk_version));
//static string sdk_version_string = "L";// +string_version.c_str();
//static int CHAR_VERSION_MAX = 12;
//char * sdk_version = {'L','0','1','.','0','3','.','0','0','.','0','0'}; // aiming at L01.03.04.05, unity nees to fix the size 
//#define TIME_TYPE std::chrono::microseconds

enum SEGMENTATION_TYPE { ACCLIVIS_SEGMENTATION = 2, THANG_SEGMENTATION = 3, DL_SEGMENTATION = 1, NO_SEGMENTATION = 0 };

// Decide whether to use obfuscation or not. cmake from command line--> -D_OBFUSCATE_=1|0
#ifdef _OBFUSCATE_

#define MANO_FLATOBF __attribute((__annotate__(("flat")))) //flattening obfuscation
#define MANO_BCFOBF __attribute((__annotate__(("bogus")))) //bogus control flow obfuscation

#else
#define MANO_FLATOBF 
#define MANO_BCFOBF  
#endif


//Make sure this corresponds to the output of the model.
//If the model changes the output, it will be inverted.
//In the current situation we have model_v1 (IS_HAND = 0,IS_NO_HAND = 1)
// Model v3 (IS_HAND = 1,IS_NO_HAND = 0)
enum HAND_NO_HAND_OUTPUT {
	IS_HAND = 1,
	IS_NO_HAND = 0
};

enum GESTUREDB {
	NODB = 0,
	OPENHANDDB,
	CLOSEHANDDB,
	OPENPINCHDB,
	CLOSEPINCHDB,
	POINTDB
};

enum FingerType {
	FINGER_THUMB = 0,
	FINGER_INDEX = 1,
	FINGER_MIDDLE = 2,
	FINGER_RING = 3,
	FINGER_PINKY = 4
};

enum SWIPE_TYPE {
	LEFT = 1000,
	RIGHT = 1001,
	UP = 1002,
	DOWN = 1003,
	NO_SWIPE = 999
};
#define FINGERTIP_MAX 5

#define MAX_DISTORTION_COEFFS 6

#define EXTRINSIC_MATRIX 16

//#ifndef __IS_VARJO__
//#define DATA_COLLECTION
//#endif
#define FIGERTIP_CLOSE 0

#define FINGERTIP_POI 1 //for pinch and point

#define FINGERTIP_OPEN 5

#define SKELETON_JOINTS 21
#define CONTOUR_POINTS 200
//Mesh hand info
#define VERTICES_MAX 778 // got it from the file we read the model
#define INDICES_MAX 1538 // got it from the file we read the model


#define FINGERTIP_WIN 10.0

//Size of input images for Haar detectors
//#define TRAIN_IMG_SIZE 217.0
#define TRAIN_IMG_SIZE 130.0 //for window 24x24

#define IMGSIZE_FINGERTIP 240.0

#define FINGERTIP_TRACKING_SCALE 1.0

//define maximum number of Kalman filter to track fingertips
#define KL_TRACKING_STEP 3//careful, with 7 steps we got more than the allowed time(500ms) between corrections keeping it at initialization permanently
#define KF_FINGERTIP_MAX 10

//Old configuration
/*#define KL_STATE_SIZE 6
#define KL_MEASURE_SIZE 4*/

#define KL_STATE_SIZE 8
#define KL_MEASURE_SIZE 6
#define PROBABILITY_THRESHOLD .2f
#define MM_PI 3.141592f
//this was in utils before and it was making troubles

// removed the input type, we willl use macro instead to avoid compiling grayscale when it is not neded

enum INPUT_TYPE {
	RGB = 0,
	GRAYSCALE = 1
};
//used to configure the SDK based on init
enum VARJO_MODE_SEG {
	SEG_MODE_ONE = 1,
	SEG_MODE_TWO = 2


};
#define DCUTOFF 1.0

enum FlagWarnings {
	FLAG_NOTHING = 0,
	FLAG_WARNING_HAND_NOT_FOUND = 1,
	 FLAG_WARNING_APPROACHING_LOWER_EDGE =3,
	FLAG_WARNING_APPROACHING_UPPER_EDGE = 4,
	FLAG_WARNING_APPROACHING_LEFT_EDGE = 5,
	FLAG_WARNING_APPROACHING_RIGHT_EDGE = 6,
	// FLAG_WARNING_HAND_TOO_CLOSE =7
};


///this was in the embed data file
//-------------------------------------------------------------//
// "Malware related compile-time hacks with C++11" by LeFF   //
// You can use this code however you like, I just don't really //
// give a shit, but if you feel some respect for me, please //
// don't cut off this comment when copy-pasting... ;-)       //
//-------------------------------------------------------------//

////////////////////////////////////////////////////////////////////
template <int X> struct EnsureCompileTime {
	enum : int {
		Value = X
	};
};
////////////////////////////////////////////////////////////////////


////////////////////////////////////////////////////////////////////
//Use Compile-Time as seed
#define Seed ((__TIME__[7] - '0') * 1  + (__TIME__[6] - '0') * 10  + \
              (__TIME__[4] - '0') * 60   + (__TIME__[3] - '0') * 600 + \
              (__TIME__[1] - '0') * 3600 + (__TIME__[0] - '0') * 36000)
////////////////////////////////////////////////////////////////////


////////////////////////////////////////////////////////////////////
constexpr int LinearCongruentGenerator(int Rounds)
{
	return 1013904223 + 1664525 * ((Rounds > 0) ? LinearCongruentGenerator(Rounds - 1) : Seed & 0xFFFFFFFF);
}
#define _Random() EnsureCompileTime<LinearCongruentGenerator(10)>::Value //10 Rounds
#define RandomNumber(Min, Max) (Min + (_Random() % (Max - Min + 1)))
////////////////////////////////////////////////////////////////////


////////////////////////////////////////////////////////////////////
template <int... Pack> struct IndexList {};
////////////////////////////////////////////////////////////////////


////////////////////////////////////////////////////////////////////
template <typename IndexList, int Right> struct Append;
template <int... Left, int Right> struct Append<IndexList<Left...>, Right> {
	typedef IndexList<Left..., Right> Result;
};
////////////////////////////////////////////////////////////////////


////////////////////////////////////////////////////////////////////
template <int N> struct ConstructIndexList {
	typedef typename Append<typename ConstructIndexList<N - 1>::Result, N - 1>::Result Result;
};
template <> struct ConstructIndexList<0> {
	typedef IndexList<> Result;
};
////////////////////////////////////////////////////////////////////


////////////////////////////////////////////////////////////////////
const char XORKEY = static_cast<char>(RandomNumber(0, 0xFF));
constexpr char EncryptCharacter(const char Character, int Index)
{
	return Character ^ (XORKEY + Index);
}

template <typename IndexList> class CXorString;
template <int... Index> class CXorString<IndexList<Index...> > {
private:
	char Value[sizeof...(Index) + 1];
public:
	constexpr CXorString(const char* const String)
		: Value{ EncryptCharacter(String[Index], Index)... }
	{}

	char* decrypt()
	{
		for (int t = 0; t < sizeof...(Index); t++) {
			Value[t] = Value[t] ^ (XORKEY + t);
		}
		Value[sizeof...(Index)] = '\0';
		return Value;
	}

	char* get()
	{
		return Value;
	}
};

/*
#ifndef _DEBUG
#define XorS(X, String) CXorString<ConstructIndexList<sizeof(String)-1>::Result> X(String)
#define MANO_CRYPT(String) ([](){XorS(A, String); return std::string(A.decrypt());})()
#else
#define XorS(X, String) String
#define MANO_CRYPT(String) String
#endif
*/

/*#ifndef _DEBUG_ON_
#define XorS(X, String) CXorString<ConstructIndexList<sizeof(String)-1>::Result> X(String)
#define MANO_CRYPT(String) ([](){XorS(A, String); return std::string(A.decrypt());})()
#else
#ifdef __JETSON__
	#define XorS(X, String) String
	#define MANO_CRYPT(String) String
#endif
#endif*/
#define XorS(X, String) String
#define MANO_CRYPT(String) String

struct embedded_file {
	std::string name;
	const unsigned char* data;
	size_t size;
};
