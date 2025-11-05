using UnityEngine;

namespace ManoMotion.Instructions.New
{
    /// <summary>
    /// Manager of an instruction panel that can have multiple requirements. 
    /// Example is the panel with both Grab and Release instructions.
    /// </summary>
    public class InstructionStep : MonoBehaviour
    {
        [SerializeField] Instruction[] instructions;

        int current = 0;

        public bool InstructionsCompleted => current >= instructions.Length;

        public void StartInstructions()
        {
            current = 0;
            gameObject.SetActive(true);
            if (current < instructions.Length)
                instructions[current].StartInstruction();
        }

        private void Update()
        {
            // Out of range
            if (current >= instructions.Length)
                return;

            // Instruction not completed
            if (instructions[current].IsCompleted == false)
                return;

            // Start next instruction
            instructions[current].StopInstruction();
            current++;
            if (current < instructions.Length)
            {
                instructions[current].StartInstruction();
            }
        }

        public void ResetInstructions()
        {
            current = 0;
            gameObject.SetActive(false);
            for (int i = 0; i < instructions.Length; i++)
            {
                instructions[i].ResetInstruction();
            }
        }
    }
}