using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using ManoMotion;
using ManoMotion.UI;

// This script is attached to the PlayerObject GameObject in the Unity scene.
// It is responsible for managing the player's interactions and movements in the game.
// The script uses the ManoMotion SDK to detect hand gestures and update the player's position accordingly.

[RequireComponent(typeof(BoxCollider),typeof(Rigidbody))]
public class gpt2 : MonoBehaviour
{   
    [SerializeField] private Material[] materials = new Material[2];
    private Renderer objectRenderer;
    private string handTag = "Player";
    private bool isGrabbing;
    private float skeletonConfidence=0.0001f;

    void Start() 
    {
        objectRenderer = GetComponent<Renderer>();  
    } 

    void Update() 
    { 
        ManoMotionManager. Instance. ShouldCalculateGestures (true); 

        var currentGesture = ManoMotionManager.Instance.HandInfos[0].gestureInfo.manoGestureTrigger;

        if (currentGesture == ManoGestureTrigger.GRAB_GESTURE)      
        { 
            isGrabbing = true; 
        } 
        else if (currentGesture == ManoGestureTrigger.RELEASE_GESTURE) 
        { 
            isGrabbing = false; 
            transform.parent = null;
        }

        bool hasConfidence = ManoMotionManager.Instance.HandInfos[0].trackingInfo.skeleton.confidence > skeletonConfidence;        
        
        if (!hasConfidence) 
        { 
        objectRenderer.sharedMaterial = materials[0]; 
        } 

    }

    private void OnTriggerEnter(Collider other) 
    { 
        if (other.gameObject.CompareTag(handTag)) 
        { 
            objectRenderer.sharedMaterial = materials[1]; 
            Handheld.Vibrate(); 
        }
        else if (isGrabbing) 
        {
            transform.parent = other.gameObject.transform; 
        }
    }

    private void OnTriggerStay(Collider other) 
    { 
        if (other.gameObject.CompareTag(handTag) && isGrabbing) 
        { 
            transform.parent = other.gameObject.transform;       
        } 
       
    }

    private void OnTriggerExit(Collider other) 
    { 
        transform.parent = null;
        objectRenderer.sharedMaterial = materials[0];
        
    }

}

