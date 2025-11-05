using UnityEngine;
using UnityEngine.Events;

namespace ManoMotion.Instructions.New
{
    /// <summary>
    /// Manager for new style of instructions.
    /// </summary>
    public class InstructionManager : MonoBehaviour
    {
        [SerializeField] GameObject instructionCanvas, closeButton, nextTimer, restartButton;
        [SerializeField] bool useTimer = true;
        [SerializeField] InstructionStep[] steps;
        [SerializeField] UnityEvent OnInstructionsStart, OnInstructionsOver;
        
        int currentStep = 0;
        bool isTracking = true;

        private void OnEnable()
        {
            steps[currentStep].StartInstructions();
            OnInstructionsStart?.Invoke();
        }

        public void Next()
        {
            nextTimer.SetActive(false);
            steps[currentStep].gameObject.SetActive(false);
            
            if (currentStep < steps.Length - 1)
            {
                currentStep++;
                steps[currentStep].StartInstructions();
            }
            else
            {
                SetActive(false);
                OnInstructionsOver?.Invoke();
            }
        }

        private void Update()
        {
            if (!isTracking || currentStep >= steps.Length)
                return;

            if (steps[currentStep].InstructionsCompleted && useTimer)
            {
                nextTimer.SetActive(true);
            }
        }

        public void Restart()
        {
            for (int i = 0; i < steps.Length; i++)
            {
                steps[i].ResetInstructions();
            }
            SetActive(true);
            steps[currentStep].gameObject.SetActive(false);
            currentStep = 0;
            steps[currentStep].StartInstructions();
            OnInstructionsStart?.Invoke();
        }

        public void Close()
        {
            for (int i = 0; i < steps.Length; i++)
            {
                steps[i].ResetInstructions();
            }
            SetActive(false);
            nextTimer.SetActive(false);
            OnInstructionsOver?.Invoke();
        }

        private void SetActive(bool activate)
        {
            isTracking = activate;
            instructionCanvas.SetActive(activate);
            restartButton.SetActive(!activate);
        }
    }
}