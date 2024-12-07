using Fusion;
using UnityEngine;

public class PlayerSpawner : SimulationBehaviour, IPlayerJoined
{
    public GameObject PlayerPrefab;

    public void PlayerJoined(PlayerRef player)
    {
        float randomX = Random.Range(-5f, 5f);
        float randomZ = Random.Range(-5f, 5f);
        Vector3 spawnPosition = new Vector3(randomX, 1.5f, randomZ);

        // Sprawdzenie, czy to lokalny gracz
        if (player == Runner.LocalPlayer)
        {
            // Przypisanie InputAuthority dla lokalnego gracza
            Runner.Spawn(PlayerPrefab, spawnPosition, Quaternion.identity, player);
        }
    }
}
