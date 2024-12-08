/*using UnityEngine;
using Fusion;
using Fusion.Sockets;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System;

public class ConnectToServerFusion : MonoBehaviour, INetworkRunnerCallbacks
{
    [SerializeField] private NetworkRunner _networkRunner;

    private async void Awake()
    {
        _networkRunner.ProvideInput = true; // Jeśli potrzebujesz obsługi wejścia

        // Uruchom klienta Fusion i połącz z serwerem
        var result = await _networkRunner.StartGame(new StartGameArgs
        {
            GameMode = GameMode.Client,
            SessionName = "LobbySession", // Nazwa lobby lub sesji
            SceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>() // Automatyczne zarządzanie scenami
        });

        if (result.Ok)
        {
            Debug.Log("Połączono z serwerem!");
        }
        else
        {
            Debug.LogError($"Błąd połączenia z serwerem: {result.ShutdownReason}");
        }
    }

    public void OnConnectedToServer(NetworkRunner runner)
    {
        Debug.Log("Połączono z serwerem Fusion.");
    }

    public void OnJoinedSession(NetworkRunner runner)
    {
        Debug.Log("Dołączono do lobby.");
        SceneManager.LoadScene("Lobby");
    }

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        Debug.Log($"Gracz {player.PlayerId} dołączył do gry.");
    }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        Debug.Log($"Gracz {player.PlayerId} opuścił grę.");
    }

    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
    {
        Debug.Log($"Serwer zamknięty: {shutdownReason}");
    }

    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason)
    {
        Debug.LogError($"Połączenie nieudane: {reason}");
    }

    // Puste implementacje metod, których nie używasz. Można je usunąć, jeśli nie są potrzebne.
    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    public void OnInput(NetworkRunner runner, NetworkInput input) { }
    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason) { }
    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }
    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data) { }
    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) { }
    public void OnSceneLoadDone(NetworkRunner runner) { }
    public void OnSceneLoadStart(NetworkRunner runner) { }
}
*/