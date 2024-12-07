using Fusion;
using Fusion.Sockets;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem.iOS;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using static Autodesk.Fbx.FbxTime;

public class NetworkController : MonoBehaviour, INetworkRunnerCallbacks
{

    public static NetworkRunner runnerInstance;
    [SerializeField] private string lobbyName = "Default Name";

    [SerializeField] private Transform sessionListContentParent;
    [SerializeField] private GameObject SessionListEnktryPrefab;
    public Dictionary<string, GameObject> SessionDictionaryElement = new Dictionary<string, GameObject>();

    public SceneAsset gameplayScene;

    public GameObject PlayerPrefab;

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

    public void CreateRandomSession()
    {
        int randomInt = UnityEngine.Random.Range(1000, 9999);
        string randomName = $"Room nr:{randomInt}";

        runnerInstance.StartGame(new StartGameArgs()
        {
            Scene = SceneRef.FromIndex(GetSceneIndex(gameplayScene.name)),
            SessionName = randomName,
            GameMode = GameMode.Shared
        });
    }

    public int GetSceneIndex(string sceneName)
    {
        for(int i = 0; i< SceneManager.sceneCountInBuildSettings; i++)
        {
            string scenePath = SceneUtility.GetScenePathByBuildIndex(i);
            string name = System.IO.Path.GetFileName(scenePath);
            if(name == sceneName)
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
        GameObject newElement = Instantiate(SessionListEnktryPrefab, sessionListContentParent);
        SessionLobbyElement lobbyElement = newElement.GetComponent<SessionLobbyElement>();
        SessionDictionaryElement.Add(info.Name, newElement);

        lobbyElement.roomName.text = info.Name;
        lobbyElement.playerCount.text = $"{info.PlayerCount}/{info.MaxPlayers}";
        lobbyElement.joinRoomBtn.interactable = info.IsOpen;

        newElement.SetActive(info.IsVisible);
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
    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        if(player == runnerInstance.LocalPlayer)
        {
            SceneManager.LoadScene(gameplayScene.name);
            NetworkObject playerNetworkObject = runner.Spawn(PlayerPrefab, Vector3.one);
            runner.SetPlayerObject(player, playerNetworkObject); 
        }
    }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
    }
    public void OnConnectedToServer(NetworkRunner runner)
    {
        throw new NotImplementedException();
    }

    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason)
    {
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
    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
    {
    }

    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message)
    {
    }
}
