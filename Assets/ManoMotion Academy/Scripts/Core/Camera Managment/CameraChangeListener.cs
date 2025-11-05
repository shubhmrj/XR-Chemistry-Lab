using UnityEngine;
using UnityEngine.Events;

namespace ManoMotion.CameraSystem
{
    /// <summary>
    /// Invokes UnityEvents when camera is changed to front- or back facing.
    /// </summary>
    public class CameraChangeListener : MonoBehaviour
    {
        [SerializeField] InputManagerBase inputManager;
        [SerializeField] UnityEvent OnBackfacing, OnFrontfacing;

        private void OnEnable()
        {
            InputManagerBase.OnChangeCamera += OnCameraChange;
        }

        private void OnDisable()
        {
            InputManagerBase.OnChangeCamera -= OnCameraChange;
        }

        private void OnCameraChange()
        {
            if (inputManager.IsFrontFacing)
                OnFrontfacing?.Invoke();
            else
                OnBackfacing?.Invoke();
        }
    }
}