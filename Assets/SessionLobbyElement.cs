using Fusion;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class SessionLobbyElement : MonoBehaviour
{
    public TextMeshProUGUI roomName;
    public TextMeshProUGUI playerCount;
    public UnityEngine.UI.Button joinRoomBtn;
    
    public void JoinRoom()
    {
        NetworkController.runnerInstance.StartGame(new StartGameArgs()
        {
            SessionName = roomName.text,
        });
    }
}
