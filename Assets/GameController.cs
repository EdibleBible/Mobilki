using Fusion;
using UnityEngine;

public class GameController : MonoBehaviour
{
    [SerializeField] private NetworkPrefabRef playerPrefab;
    private NetworkRunner networkRunner;

    private void Start()
    {
        networkRunner = FindObjectOfType<NetworkRunner>();
        if (networkRunner != null && networkRunner.IsRunning)
        {
            SpawnPlayer();
        }
    }

    private void SpawnPlayer()
    {
        if (playerPrefab == null)
        {
            Debug.LogError("Player prefab is not assigned!");
            return;
        }

        float randomX = Random.Range(-5f, 5f);
        float randomZ = Random.Range(-5f, 5f);
        Vector3 spawnPosition = new Vector3(randomX, 1.5f, randomZ);

        networkRunner.Spawn(playerPrefab, spawnPosition, Quaternion.identity);
        Debug.Log("Player spawned!");
    }
}
