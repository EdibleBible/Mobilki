using Photon.Pun;
using UnityEngine;

public class NetworkedSpawner : MonoBehaviour
{
    [Header("Spawner Settings")]
    public GameObject prefabToSpawn; // Prefab do spawnowania (musi być zarejestrowany w Photon)
    public LayerMask pickableLayer;  // Warstwa dla obiektów Pickable
    public int maxPickableObjects = 10; // Maksymalna liczba obiektów Pickable
    public float spawnCooldown = 5f;    // Czas pomiędzy próbami spawnowania
    public Transform SpawnLocation;

    private float spawnTimer; // Licznik czasu dla cooldownu

    void Update()
    {
        // Odliczanie czasu
        spawnTimer += Time.deltaTime;

        // Sprawdź, czy czas do spawnowania minął
        if (spawnTimer >= spawnCooldown)
        {
            AttemptSpawn();
            spawnTimer = 0f; // Zresetuj licznik
        }
    }

    private void AttemptSpawn()
    {
        // Sprawdź, ile obiektów pickable jest obecnie na scenie
        int currentPickableCount = FindObjectsOfType<ThrowableItem>().Length;

        if (currentPickableCount >= maxPickableObjects)
        {
            Debug.Log("Limit obiektów Pickable został osiągnięty!");
            return; // Nie spawnuj więcej obiektów
        }

        // Sprawdź, czy w obszarze spawnera znajduje się obiekt na warstwie Pickable
        Collider[] colliders = Physics.OverlapSphere(SpawnLocation.position, 1f, pickableLayer);

        if (colliders.Length > 0)
        {
            Debug.Log("Obiekt Pickable znajduje się w obszarze spawnera. Nie można spawnować.");
            return; // Nie spawnuj, jeśli coś już jest w obszarze
        }

        // Jeśli warunki są spełnione, zainicjuj spawnowanie
        SpawnObject();
    }

    private void SpawnObject()
    {
        // PhotonNetwork.Instantiate synchronizuje prefab w całej sieci
        PhotonNetwork.Instantiate(prefabToSpawn.name, SpawnLocation.position, Quaternion.identity);
        Debug.Log("Obiekt został zespawnowany w sieci!");
    }

    private void OnDrawGizmos()
    {
        // Rysowanie obszaru wykrywania w edytorze
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(SpawnLocation.position, 1f);
    }
}
