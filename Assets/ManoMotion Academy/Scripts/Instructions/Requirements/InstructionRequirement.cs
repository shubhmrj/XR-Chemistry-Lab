using UnityEngine;

namespace ManoMotion.Instructions
{
    /// <summary>
    /// Base class for instruction requirement ScriptableObjects
    /// </summary>
    public abstract class InstructionRequirement : ScriptableObject
    {
        /// <summary>
        /// Returns true if the requirement has been fulfilled, can make Update checks in here too.
        /// </summary>
        public virtual bool IsFulfilled() { return true; }

        /// <summary>
        /// Initializes the requirement to start tracking the process.
        /// </summary>
        public virtual void Start() { }

        /// <summary>
        /// Stops the requirement from tracking the process
        /// </summary>
        public virtual void Stop() { }
    }
}