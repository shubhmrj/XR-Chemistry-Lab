using UnityEngine;
using ManoMotion;

public class ChemistryExperimentManager : MonoBehaviour
{
    public GameObject beakerA, beakerB;
    public ParticleSystem reactionEffect;

    private bool isChemicalPicked = false;
    private Transform handAnchor; // Hand world position

    void Start()
    {
        handAnchor = new GameObject("HandAnchor").transform;
    }

    void Update()
    {
        var gestureInfo = ManoMotionManager.Instance.HandInfos[0].gestureInfo;

        // // Check if hand is detected
        // // 
        // var handPosition = ManoMotionManager.Instance.HandInfos[0].trackingInfo.palm_center;

        // // Convert hand screen point to world
        // Vector3 worldHandPos = Camera.main.ViewportToWorldPoint(new Vector3(handPosition.x, handPosition.y, 0.5f));
        // handAnchor.position = worldHandPos;

        HandInfo[] handInfos = ManoMotionManager.Instance.HandInfos;
        for (int i = 0; i < handInfos.Length; i++)
        {
            HandInfo handInfo = handInfos[i];

            // If no found is hand continue to the next handInfo.
            if (handInfos[i].gestureInfo.manoClass == ManoClass.NO_HAND)
                continue;

            // Get screen position of hand bounding box.
            Vector3 WorldHandPos =Camera.main.ViewportToWorldPoint( new Vector3(
               handInfo.trackingInfo.boundingBox.topLeft.x + handInfo.trackingInfo.boundingBox.width / 2,
               handInfo.trackingInfo.boundingBox.topLeft.y - handInfo.trackingInfo.boundingBox.height / 2,
               0
           ));

            // Set the hand anchor position to the world position of the hand
            handAnchor.position = WorldHandPos;
        }

        if (gestureInfo.manoGestureContinuous == ManoGestureContinuous.CLOSED_HAND_GESTURE)
        {
            PickChemical();
        }
        else if (gestureInfo.manoGestureContinuous == ManoGestureContinuous.OPEN_HAND_GESTURE)
        {
            PourChemical();
        }
        else if (gestureInfo.manoGestureTrigger == ManoGestureTrigger.CLICK)
        {
            TriggerReaction();
        }
    }

    private void PickChemical()
    {
        if (!isChemicalPicked)
        {
            if (Vector3.Distance(handAnchor.position, beakerA.transform.position) < 0.2f)
            {
                isChemicalPicked = true;
                beakerA.transform.SetParent(handAnchor);
                Debug.Log("Chemical A Picked");
            }
        }
    }

    private void PourChemical()
    {
        if (isChemicalPicked)
        {
            if (Vector3.Distance(handAnchor.position, beakerB.transform.position) < 0.3f)
            {
                isChemicalPicked = false;
                beakerA.transform.SetParent(null);
                beakerA.transform.position = new Vector3(beakerA.transform.position.x, beakerB.transform.position.y + 0.2f, beakerA.transform.position.z);
                Debug.Log("Chemical A Poured into B");
            }
        }
    }

    private void TriggerReaction()
    {
        if (!isChemicalPicked)
        {
            Debug.Log("Reaction Started!");
            beakerB.GetComponent<Renderer>().material.color = Color.green;
            if (!reactionEffect.isPlaying)
                reactionEffect.Play();
        }
    }
}
