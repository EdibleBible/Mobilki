using Fusion;
using Unity.Mathematics;
using UnityEngine;

public class GameLogic : NetworkBehaviour, IPlayerJoined, IPlayerLeft
{
    [SerializeField] private NetworkPrefabRef playerPrefab;
    [Networked, Capacity(12)] private NetworkDictionary<PlayerRef, Player> Players => default;
    public void PlayerJoined(PlayerRef player)
    {
        if(HasStateAuthority)
        {
            NetworkObject playerObject = NetworkController.runnerInstance.Spawn(playerPrefab, Vector3.up, quaternion.identity, player);
            Players.Add(player, playerObject.GetComponent<Player>());
        }
    }

    public void PlayerLeft(PlayerRef player)
    {
        if (!HasStateAuthority)
            return;

        if (Players.TryGet(player,out Player playerBehaviour))
        {
            Players.Remove(player);
            NetworkController.runnerInstance.Despawn(playerBehaviour.Object);
        }

    }
}
