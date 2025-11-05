using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace ManoMotion.Instructions.Old
{
    /// <summary>
    /// Manager of old style instructions.
    /// </summary>
    public class SceneInstruction : MonoBehaviour
    {
        [SerializeField] GameObject overviewPanel, instructionPanel, replayButton, skipButton;
        [SerializeField] TextMeshProUGUI titleText, overviewText, instructionText;
        [SerializeField] Animator gestureAnimator;

        [Space(), Header("Progress")]
        [SerializeField] GameObject performedRequirementImage;
        [SerializeField] GameObject progressParent;
        [SerializeField] Image[] progressImages;
        [SerializeField] Color unfinishedColor, finishedColor;

        [SerializeField] InstructionStep[] steps;
        [SerializeField] UnityEvent OnStartInstructions, OnStopInstructions;

        int currentStep = 0;
        bool checkProgression = false;
        bool seenInstructions = false;

        private void OnEnable()
        {
            if (!seenInstructions)
            {
                PlayInstructions();
            }
        }

        private void Update()
        {
            if (checkProgression)
            {
                bool finished = steps[currentStep].IsFulfilled();
                UpdateProgress();

                if (finished)
                {
                    checkProgression = false;
                    performedRequirementImage.SetActive(true);
                }
            }
        }

        private void UpdateProgress()
        {
            if (steps[currentStep].ShowProgress)
            {
                for (int i = 0; i < progressImages.Length; i++)
                {
                    progressImages[i].gameObject.SetActive(i < steps[currentStep].Steps);
                    progressImages[i].color = i < steps[currentStep].CurrentStep ? finishedColor : unfinishedColor;
                }
            }
        }

        /// <summary>
        /// Proceeds to the next instruction if possible.
        /// If there are no more instructions it stops the instructions instead.
        /// </summary>
        public void NextStep()
        {
            if (currentStep < steps.Length - 1)
            {
                StartInstructionStep(currentStep + 1);
            }
            else
            {
                StopInstructions();
            }
        }

        /// <summary>
        /// Starts the instruction, showing the title and overview.
        /// </summary>
        /// <param name="index"></param>
        private void StartInstructionStep(int index)
        {
            // Stop the old instruction
            steps[currentStep].Stop();

            // Start the new instruction
            currentStep = index;
            steps[currentStep].Start();

            if (steps[currentStep].ShowOverview)
            {
                ShowOverview();
            }
            else
            {
                ShowInstruction();
            }

            checkProgression = true;
        }

        private void ShowOverview()
        {
            overviewPanel.SetActive(true);
            instructionPanel.SetActive(false);
            progressParent.SetActive(false);
            gestureAnimator.gameObject.SetActive(false);
            performedRequirementImage.SetActive(false);
            titleText.text = steps[currentStep].Title;
            overviewText.text = steps[currentStep].Overview;
        }

        /// <summary>
        /// Hides the overview and shows the instructions in a smaller panel.
        /// </summary>
        public void ShowInstruction()
        {
            if (steps[currentStep].ShowInstruction)
            {
                overviewPanel.SetActive(false);
                instructionPanel.SetActive(true);
                instructionText.text = steps[currentStep].Instruction;
                progressParent.SetActive(steps[currentStep].ShowProgress);

                GestureAnimation animation = steps[currentStep].Animation;
                if (animation.Equals(GestureAnimation.None))
                {
                    gestureAnimator.gameObject.SetActive(false);
                }
                else
                {
                    gestureAnimator.gameObject.SetActive(true);
                    gestureAnimator.Play(animation.ToString());
                }
            }
            else
            {
                StopInstructions();
            }
        }

        public void PlayInstructions()
        {
            overviewPanel.SetActive(true);
            instructionPanel.SetActive(false);
            progressParent.SetActive(false);
            gestureAnimator.gameObject.SetActive(false);
            performedRequirementImage.SetActive(false);
            replayButton.SetActive(false);
            skipButton.SetActive(true);

            StartInstructionStep(0);
            checkProgression = true;
            OnStartInstructions?.Invoke();
        }

        /// <summary>
        /// Stops the instructions, disabling the instruction UI.
        /// </summary>
        public void StopInstructions()
        {
            seenInstructions = true;
            steps[currentStep].Stop();

            overviewPanel.SetActive(false);
            instructionPanel.SetActive(false);
            progressParent.SetActive(false);
            gestureAnimator.gameObject.SetActive(false);
            performedRequirementImage.SetActive(false);
            replayButton.SetActive(true);
            skipButton.SetActive(false);

            checkProgression = false;
            OnStopInstructions?.Invoke();
        }

        /// <summary>
        /// Resets each step and starts over from the beginning
        /// </summary>
        public void RestartInstructions()
        {
            currentStep = 0;
            for (int i = 0; i < steps.Length; i++)
            {
                steps[i].Stop();
            }
            PlayInstructions();
        }
    }
}