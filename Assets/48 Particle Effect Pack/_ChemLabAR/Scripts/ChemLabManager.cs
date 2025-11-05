using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ChemLabManager : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject startButton;   // Reference to the Start Button
    public Text infoText;            // Reference to the Info Text (inside InfoPanel)

    [Header("Optional Chemistry Table")]
    public GameObject chemistryTable; // If you want to spawn table after Start

    public void StartLab()
    {
        if (startButton != null)
            startButton.SetActive(false);      // Hide Start Button

        if (infoText != null)
            infoText.text = "Pick up a bottle and pour it into the beaker!";  // Update Info Text

        if (chemistryTable != null)
            chemistryTable.SetActive(true);    // Activate Chemistry Table if it's disabled
    }

    public void ResetLab()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);    // Reload current scene
    }
}
