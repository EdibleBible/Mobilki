using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using Fusion;
using System.Collections;

public class PlayerSpawner : SimulationBehaviour, IPlayerJoined, IPlayerLeft
{
    public GameObject PlayerPrefab;
    [SerializeField] private CinemachineTargetGroup targetGroup;

    // Przechowujemy graczy i ich NetworkObjects w słowniku
    private Dictionary<PlayerRef, NetworkObject> playerNetworkObjects = new Dictionary<PlayerRef, NetworkObject>();

    public void PlayerJoined(PlayerRef player)
    {
        // Spawnowanie lokalnego gracza
        if (player == Runner.LocalPlayer)
        {
            var netObj = Runner.Spawn(PlayerPrefab,
                                      new Vector3(0, 1, 0),
                                      Quaternion.identity,
                                      player); // PRZEKAZUJEMY PlayerRef!
            Runner.SetPlayerObject(player, netObj);

            Debug.Log($"Spawnowano lokalnego gracza: {netObj.name}, przypisanego do PlayerRef {player.PlayerId}");

            if (netObj != null)
            {
                Debug.Log($"Znaleziono obiekt NetworkObject dla gracza {player.PlayerId}: {netObj.name}");
            }
            else
            {
                Debug.LogWarning($"Nie znaleziono obiektu NetworkObject dla gracza {player.PlayerId}");
            }

            AddToTargetGroupAsMain(netObj.transform);
        }
        else
        {
            // Innych graczy nie spawnujemy lokalnie – ich obiekty zostaną zreplikowane automatycznie
            Debug.Log($"Inny gracz dołączył: {player.PlayerId}");

            var netObj = Runner.GetPlayerObject(player);
            if (netObj != null)
            {
                Debug.Log($"Znaleziono obiekt NetworkObject dla gracza {player.PlayerId}: {netObj.name}");
                AddToTargetGroupAsSecond(netObj.transform);
            }
            else
            {
                Debug.LogWarning($"Nie znaleziono obiektu NetworkObject dla gracza {player.PlayerId}");
            }

            StartCoroutine(CheckPlayerObjectDelayed(player));
        }
    }

    private IEnumerator CheckPlayerObjectDelayed(PlayerRef player)
    {
        yield return new WaitForSeconds(0.5f); // Poczekaj chwilę na synchronizację

        var netObj = Runner.GetPlayerObject(player);
        if (netObj != null)
        {
            Debug.Log($"Znaleziono obiekt NetworkObject dla gracza {player.PlayerId}: {netObj.name}");
            AddToTargetGroupAsSecond(netObj.transform);
        }
        else
        {
            Debug.LogWarning($"Nie znaleziono obiektu NetworkObject dla gracza {player.PlayerId}, nawet po opóźnieniu.");
        }

    }

    public void PlayerLeft(PlayerRef player)
    {
        Debug.Log($"{player.PlayerId} Left the session");
        if (playerNetworkObjects.TryGetValue(player, out NetworkObject netObj))
        {
            RemoveFromTargetGroup(netObj.transform);
            playerNetworkObjects.Remove(player);
        }
    }

    private void AddToTargetGroupAsMain(Transform objectTransform)
    {
        targetGroup.AddMember(objectTransform, 10f, 5f);
    }

    private void AddToTargetGroupAsSecond(Transform objectTransform)
    {
        targetGroup.AddMember(objectTransform, 5f, 2f);
    }

    private void RemoveFromTargetGroup(Transform objectTransform)
    {
        targetGroup.RemoveMember(objectTransform);
    }
}
