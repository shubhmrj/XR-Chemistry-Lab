using UnityEngine;
using ManoMotion.Demos;

namespace ManoMotion.Instructions
{
    /// <summary>
    /// ScriptableObject instruction requirement for Grabber/Grabbable interactions.
    /// </summary>
    [CreateAssetMenu(fileName = "New InteractionRequirement", menuName = "ManoMotion/Instructions/InteractionRequirement")]
    public class InteractionRequirement : InstructionRequirement
    {
        public enum InteractType { Grab, Release, Click, Drag }

        [SerializeField] InteractType interactType;

        bool performedInteraction = false;

        public override bool IsFulfilled()
        {
            return performedInteraction;
        }

        public override void Start()
        {
            performedInteraction = false;
            Subscribe();
        }

        public override void Stop()
        {
            performedInteraction = false;
            Unsubscribe();
        }

        private void OnInteraction(Grabbable grabbable)
        {
            performedInteraction = true;
            Unsubscribe();
        }

        private void Subscribe()
        {
            switch (interactType)
            {
                case InteractType.Grab:
                    Grabbable.OnGrabbed += OnInteraction;
                    break;
                case InteractType.Release:
                    Grabbable.OnReleased += OnInteraction;
                    break;
                case InteractType.Click:
                    Grabbable.OnClicked += OnInteraction;
                    break;
                case InteractType.Drag:
                    Grabbable.OnDragged += OnInteraction;
                    break;
            }
        }

        private void Unsubscribe()
        {
            switch (interactType)
            {
                case InteractType.Grab:
                    Grabbable.OnGrabbed -= OnInteraction;
                    break;
                case InteractType.Release:
                    Grabbable.OnReleased -= OnInteraction;
                    break;
                case InteractType.Click:
                    Grabbable.OnClicked -= OnInteraction;
                    break;
                case InteractType.Drag:
                    Grabbable.OnDragged -= OnInteraction;
                    break;
            }
        }
    }
}