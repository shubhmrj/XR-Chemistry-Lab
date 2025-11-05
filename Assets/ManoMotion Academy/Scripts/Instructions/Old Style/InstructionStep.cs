using System;
using UnityEngine;

namespace ManoMotion.Instructions.Old
{
    public enum GestureAnimation
    {
        None,
        Grab,
        Release,
        Click,
        Pick,
        Drop
    }

    /// <summary>
    /// Instruction that can require multiple requirements to be fulfilled.
    /// </summary>
    [Serializable]
    public class InstructionStep
    {
        [SerializeField] InstructionRequirement[] requirements;
        [SerializeField] GestureAnimation animation;
        [SerializeField] bool showOverview = true, showInstruction = true, showProgress;
        [SerializeField] string instructionTitle;
        [SerializeField, TextArea(2, 3)] string instructionOverview;
        [SerializeField, TextArea(2, 3)] string instructionText;

        // OVERVIEW
        public bool ShowOverview => showOverview;
        public string Title => instructionTitle;
        public string Overview => instructionOverview;

        // INSTRUCTION
        public bool ShowInstruction => showInstruction;
        public string Instruction => instructionText;
        public GestureAnimation Animation => animation;

        // PROGRESSION
        public bool ShowProgress => showProgress;
        public float ProgressionPercentage => (float)CurrentStep / Steps;
        public int CurrentStep => currentRequirement;
        public int Steps => requirements.Length;

        int currentRequirement = 0;

        /// <summary>
        /// If all requirements are fulfilled
        /// </summary>
        public bool IsFulfilled()
        {
            if (requirements.Length == 0)
            {
                return true;
            }

            if (requirements[currentRequirement].IsFulfilled())
            {
                // Stop finished requirement
                requirements[currentRequirement].Stop();
                currentRequirement++;

                // Start next requirement if possible
                if (currentRequirement < requirements.Length)
                {
                    requirements[currentRequirement].Start();
                }
            }
            return currentRequirement >= requirements.Length;
        }

        /// <summary>
        /// Start tracking requirements progress
        /// </summary>
        public void Start()
        {
            currentRequirement = 0;
            if (requirements.Length > 0)
            {
                requirements[currentRequirement].Start();
            }
        }

        /// <summary>
        /// Stop tracking progress
        /// </summary>
        public void Stop()
        {
            if (currentRequirement < requirements.Length)
            {
                requirements[currentRequirement].Stop();
            }
        }
    }
}