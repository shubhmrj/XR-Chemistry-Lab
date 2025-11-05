using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class ARTableManager : MonoBehaviour
{
    public GameObject chemistryTablePrefab;
    private GameObject spawnedTable;

    void Start()
    {
        SpawnTable();
    }

    private void SpawnTable()
    {
        Vector3 spawnPosition = new Vector3(0, 0, 1.5f); // 1.5 meters in front
        spawnedTable = Instantiate(chemistryTablePrefab, spawnPosition, Quaternion.identity);
    }
}
// Compare this snippet from Assets/_ChemLabAR/Scripts/ARTableManager.cs: