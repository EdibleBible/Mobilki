using Fusion;
using Fusion.Sockets;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NetworkController : MonoBehaviour, INetworkRunnerCallbacks
{
    public static NetworkRunner runnerInstance;
    [SerializeField] private string lobbyName = "Default Name";

    [SerializeField] private Transform sessionListContentParent;
    [SerializeField] private GameObject SessionListEnktryPrefab;
    public Dictionary<string, GameObject> SessionDictionaryElement = new Dictionary<string, GameObject>();

    [SerializeField] private string gameplayScene; // Zmienione na string
    [SerializeField] private string lobbyScene; // Zmienione na string

    [SerializeField] private GameObject playerPrefab;

    [SerializeField] private GameEvent Event_OnPlayerSpawn;
    [SerializeField] private GameEvent Event_OnPlayerDespawn;

    private void Awake()
    {
        runnerInstance = gameObject.AddComponent<NetworkRunner>();

        if (runnerInstance == null)
        {
            runnerInstance = gameObject.AddComponent<NetworkRunner>();
        }
    }

    private void Start()
    {
        runnerInstance.JoinSessionLobby(SessionLobby.Shared, lobbyName);
    }

    public static void ReturnToLobby()
    {
        NetworkController.runnerInstance.Despawn(runnerInstance.GetPlayerObject(runnerInstance.LocalPlayer));
        NetworkController.runnerInstance.Shutdown(true, ShutdownReason.Ok);
    }

    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
    {
        SceneManager.LoadScene(lobbyScene); // Zmienione na string
    }

    public void CreateRandomSession()
    {
        int randomInt = UnityEngine.Random.Range(1000, 9999);
        string randomName = $"Room nr:{randomInt}";

        try
        {
            runnerInstance.StartGame(new StartGameArgs()
            {
                Scene = SceneRef.FromIndex(GetSceneIndex(gameplayScene)), // Zmienione na string
                SessionName = randomName,
                GameMode = GameMode.Shared,
            });
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error starting game: {ex.Message}");
        }
    }

    public static int GetSceneIndex(string sceneName)
    {
        for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
        {
            string scenePath = SceneUtility.GetScenePathByBuildIndex(i);
            string name = System.IO.Path.GetFileName(scenePath);
            if (name == sceneName + ".unity")
                return i;
        }
        return -1;
    }

    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
    {
        DeleteOldSessionFromUi(sessionList);
        CompareList(sessionList);
    }

    private void CompareList(List<SessionInfo> sessionList)
    {
        foreach (SessionInfo session in sessionList)
        {
            if (SessionDictionaryElement.ContainsKey(session.Name))
            {
                UpdateEntryUI(session);
            }
            else
            {
                CreateEntryUI(session);
            }
        }
    }

    private void CreateEntryUI(SessionInfo info)
    {
        Debug.Log($"[CreateEntryUI] Start creating UI element for session: {info.Name}");

        // Tworzenie nowego elementu UI
        GameObject newElement = Instantiate(SessionListEnktryPrefab, sessionListContentParent);
        Debug.Log($"[CreateEntryUI] Instantiated new UI element for session: {info.Name}");

        // Pobranie komponentu i zaktualizowanie wartości
        SessionLobbyElement lobbyElement = newElement.GetComponent<SessionLobbyElement>();
        if (lobbyElement == null)
        {
            Debug.LogError($"[CreateEntryUI] Missing SessionLobbyElement component in instantiated prefab for session: {info.Name}");
            return;
        }

        SessionDictionaryElement.Add(info.Name, newElement);
        Debug.Log($"[CreateEntryUI] Added session {info.Name} to the dictionary.");

        lobbyElement.roomName.text = info.Name;
        Debug.Log($"[CreateEntryUI] Set room name to: {info.Name}");

        lobbyElement.playerCount.text = $"{info.PlayerCount}/{info.MaxPlayers}";
        Debug.Log($"[CreateEntryUI] Set player count to: {info.PlayerCount}/{info.MaxPlayers}");

        lobbyElement.joinRoomBtn.interactable = info.IsOpen;
        Debug.Log($"[CreateEntryUI] Set join button interactable to: {info.IsOpen}");

        newElement.SetActive(info.IsVisible);
        Debug.Log($"[CreateEntryUI] Set element visibility to: {info.IsVisible}");

        Debug.Log($"[CreateEntryUI] Finished creating UI element for session: {info.Name}");
    }

    private void UpdateEntryUI(SessionInfo info)
    {
        SessionDictionaryElement.TryGetValue(info.Name, out GameObject newElement);
        SessionLobbyElement lobbyElement = newElement.GetComponent<SessionLobbyElement>();

        lobbyElement.roomName.text = info.Name;
        lobbyElement.playerCount.text = $"{info.PlayerCount}/{info.MaxPlayers}";
        lobbyElement.joinRoomBtn.interactable = info.IsOpen;

        newElement.SetActive(info.IsVisible);
    }

    private void DeleteOldSessionFromUi(List<SessionInfo> sessionList)
    {
        bool isContained = false;
        GameObject uiToDelete = null;

        foreach (var kvp in SessionDictionaryElement)
        {
            string sessionKey = kvp.Key;
            foreach (var sessionInfo in sessionList)
            {
                if (sessionInfo.Name == sessionKey)
                {
                    isContained = true;
                    break;
                }
            }

            if (!isContained)
            {
                uiToDelete = kvp.Value;
                SessionDictionaryElement.Remove(sessionKey);
            }
        }
    }

    private void UnloadLobbyScene()
    {
        // Pobierz aktywną scenę (scena lobby)
        Scene activeScene = SceneManager.GetActiveScene();

        // Sprawdź, czy aktywna scena to scena lobby
        if (activeScene.name == lobbyScene) // Zmienione na string
        {
            // Usuń scenę lobby
            SceneManager.UnloadSceneAsync(lobbyScene); // Zmienione na string
        }
    }

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        UnloadLobbyScene();

        // Spawnowanie lokalnego gracza
        if (player == runnerInstance.LocalPlayer)
        {
            var netObj = runnerInstance.Spawn(playerPrefab,
                                      new Vector3(0, 1, 0),
                                      Quaternion.identity,
                                      player); // PRZEKAZUJEMY PlayerRef!
            runnerInstance.SetPlayerObject(player, netObj);

            Debug.Log($"Spawnowano lokalnego gracza: {netObj.name}, przypisanego do PlayerRef {player.PlayerId}");

            if (netObj != null)
            {
                Debug.Log($"Znaleziono obiekt NetworkObject dla gracza {player.PlayerId}: {netObj.name}");
            }
            else
            {
                Debug.LogWarning($"Nie znaleziono obiektu NetworkObject dla gracza {player.PlayerId}");
            }
            Event_OnPlayerSpawn.Raise(this, (netObj.transform, true));
        }
        else
        {
            // Innych graczy nie spawnujemy lokalnie – ich obiekty zostaną zreplikowane automatycznie
            Debug.Log($"Inny gracz dołączył: {player.PlayerId}");

            var netObj = runnerInstance.GetPlayerObject(player);
            if (netObj != null)
            {
                Debug.Log($"Znaleziono obiekt NetworkObject dla gracza {player.PlayerId}: {netObj.name}");
                Event_OnPlayerSpawn.Raise(this, (netObj.transform, false));
            }
            else
            {
                Debug.LogWarning($"Nie znaleziono obiektu NetworkObject dla gracza {player.PlayerId}");
                StartCoroutine(CheckPlayerObjectDelayed(player));
            }
        }
    }

    private IEnumerator CheckPlayerObjectDelayed(PlayerRef player)
    {
        yield return new WaitForSeconds(0.5f); // Poczekaj chwilę na synchronizację

        var netObj = runnerInstance.GetPlayerObject(player);
        if (netObj != null)
        {
            Debug.Log($"Znaleziono obiekt NetworkObject dla gracza {player.PlayerId}: {netObj.name}");
            Event_OnPlayerSpawn.Raise(this, (netObj.transform, false));
        }
        else
        {
            Debug.LogWarning($"Nie znaleziono obiektu NetworkObject dla gracza {player.PlayerId}, nawet po opóźnieniu.");
            StartCoroutine(CheckPlayerObjectDelayed(player));
        }
    }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        Debug.Log("Left");
    }

    public void OnConnectedToServer(NetworkRunner runner)
    {
        Debug.Log("OnConnectedToServer");
    }

    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason)
    {
        Debug.Log("OnConnectFailed");
    }

    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token)
    {
    }

    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data)
    {
    }

    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason)
    {
    }

    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken)
    {
    }

    public void OnInput(NetworkRunner runner, NetworkInput input)
    {
    }

    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input)
    {
    }

    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
    {
    }

    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
    {
    }

    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress)
    {
    }

    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data)
    {
    }

    public void OnSceneLoadDone(NetworkRunner runner)
    {
    }

    public void OnSceneLoadStart(NetworkRunner runner)
    {
    }

    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message)
    {
    }
}
