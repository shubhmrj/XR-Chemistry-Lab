using UnityEngine;

namespace ManoMotion.CameraSystem
{
    /// <summary>
    /// Contains a single method to switch the camera between front- and back facing. 
    /// Add to Button and connect with OnClick event.
    /// </summary>
    public class CameraSwitch : MonoBehaviour
    {
        public void SwitchCamera()
        {
            ManoMotionManager.Instance.InputManager.SetFrontFacing(!ManoMotionManager.Instance.InputManager.IsFrontFacing);
        }
    }
}