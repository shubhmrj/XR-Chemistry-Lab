using System;
using UnityEngine;

namespace ManoMotion
{
	/// <summary>
	/// Camera frame information to be set and broadcast by InputManagers.
	/// </summary>
	[Serializable]
	public struct ManoMotionFrame
	{
		// Main image. Right image when using stereo input
		public Texture2D texture;

		// Right image when using stereo input
		public Texture2D textureSecond;

		// The orientation of the device. 
		public DeviceOrientation orientation;
	}
}