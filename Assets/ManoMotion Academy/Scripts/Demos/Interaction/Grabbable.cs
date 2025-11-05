using System;
using UnityEngine;

namespace ManoMotion.Demos
{
    /// <summary>
    /// Component to interact with.
    /// </summary>
    public class Grabbable : MonoBehaviour
    {
        [SerializeField] protected Material[] materials;
        [SerializeField] protected float hoveredAlpha = 0.5f;

        protected MeshRenderer meshRenderer;
        protected int currentMaterialIndex = 0;

        protected Grabber grabber;

        public virtual bool CanBeGrabbed => grabber == null;

        public static Action<Grabbable> OnGrabbed, OnReleased, OnClicked, OnDragged, OnSpawned;

        private void Awake()
        {
            meshRenderer = GetComponent<MeshRenderer>();
        }

        public virtual void HoverStart(Grabber grabber)
        {
            // Make a copy of the material with different alpha value
            Material material = meshRenderer.material;
            Color color = material.color;
            color.a = hoveredAlpha;
            material.color = color;
            meshRenderer.material = material;
        }

        public virtual void HoverStop(Grabber grabber)
        {
            meshRenderer.material = materials[currentMaterialIndex];
        }

        public virtual void Grab(Grabber grabber)
        {
            this.grabber = grabber;
            OnGrabbed?.Invoke(this);
        }

        public virtual void Release(Grabber grabber)
        {
            this.grabber = null;
            OnReleased?.Invoke(this);
        }

        public virtual void Click(Grabber grabber)
        {
            currentMaterialIndex = (currentMaterialIndex + 1) % materials.Length;
            meshRenderer.material = materials[currentMaterialIndex];
            OnClicked?.Invoke(this);
        }

        public virtual void DragStart(Grabber grabber)
        {
            grabber.Grab(Duplicate(), ManoGestureTrigger.PICK);
            OnDragged?.Invoke(this);
        }

        public virtual void Move(Vector3 position)
        {
            transform.position = position;
        }

        public virtual void Rotate(Quaternion rotation)
        {
            transform.rotation = rotation;
        }

        private Grabbable Duplicate()
        {
            Grabbable duplicate = Instantiate(this, transform.parent);
            duplicate.currentMaterialIndex = (currentMaterialIndex + 1) % this.materials.Length;
            Material[] materials = new Material[1];
            materials[0] = this.materials[duplicate.currentMaterialIndex];
            duplicate.meshRenderer.materials = materials;
            OnSpawned?.Invoke(duplicate);
            return duplicate;
        }
    }
}