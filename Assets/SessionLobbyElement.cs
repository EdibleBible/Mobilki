using Fusion;
using TMPro;
using UnityEditor;
using UnityEngine;

public class SessionLobbyElement : MonoBehaviour
{
    public TextMeshProUGUI roomName;
    public TextMeshProUGUI playerCount;
    public UnityEngine.UI.Button joinRoomBtn;
    public string gameplayScene = "Game";


    public void JoinRoom()
    {
        NetworkController.runnerInstance.StartGame(new StartGameArgs()
        {
            Scene = SceneRef.FromIndex(NetworkController.GetSceneIndex(gameplayScene)),
            SessionName = roomName.text,
            GameMode = GameMode.Shared,
        });
    }
}
