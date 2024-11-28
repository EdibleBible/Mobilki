using Photon.Pun;
using TMPro;
using UnityEngine;

public class CreateAndJoinRooms : MonoBehaviourPunCallbacks
{
    [SerializeField] private TMP_InputField createInput; // Pole do wpisania nazwy pokoju do stworzenia
    [SerializeField] private TMP_InputField joinInput;   // Pole do wpisania nazwy pokoju do dołączenia

    private void Start()
    {
        if (!PhotonNetwork.IsConnected)
        {
            PhotonNetwork.ConnectUsingSettings(); // Łączymy się z Photonem
        }
    }

    public void CreateRoom()
    {
        if (string.IsNullOrWhiteSpace(createInput.text))
        {
            Debug.LogError("Nazwa pokoju nie może być pusta!");
            return;
        }

        Debug.Log($"Tworzenie pokoju: {createInput.text}");
        PhotonNetwork.CreateRoom(createInput.text, new Photon.Realtime.RoomOptions { MaxPlayers = 4 });
    }

    public void JoinRoom()
    {
        if (string.IsNullOrWhiteSpace(joinInput.text))
        {
            Debug.LogError("Nazwa pokoju nie może być pusta!");
            return;
        }

        Debug.Log($"Dołączanie do pokoju: {joinInput.text}");
        PhotonNetwork.JoinRoom(joinInput.text);
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("Dołączono do pokoju. Ładowanie sceny 'Game'.");
        PhotonNetwork.LoadLevel("Game");
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        Debug.LogError($"Tworzenie pokoju nie powiodło się ({returnCode}): {message}");
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        Debug.LogError($"Dołączanie do pokoju nie powiodło się ({returnCode}): {message}");
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("Połączono z serwerem Master. Gotowy do tworzenia lub dołączania do pokoi.");
    }
}
