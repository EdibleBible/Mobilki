using Fusion;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class SessionLobbyElement : MonoBehaviour
{
    public TextMeshProUGUI roomName;
    public TextMeshProUGUI playerCount;
    public UnityEngine.UI.Button joinRoomBtn;
    public SceneAsset gameplayScene;


    public void JoinRoom()
    {
        NetworkController.runnerInstance.StartGame(new StartGameArgs()
        {
            Scene = SceneRef.FromIndex(NetworkController.GetSceneIndex(gameplayScene.name)),
            SessionName = roomName.text,
            GameMode = GameMode.Shared,
        });
    }
}
