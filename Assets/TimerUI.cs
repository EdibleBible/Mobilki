using ExitGames.Client.Photon;
using Photon.Pun;
using TMPro;
using UnityEngine;

public class TimerUI : MonoBehaviourPunCallbacks
{
    [SerializeField] private TextMeshProUGUI timerText;
    private double startTime;
    private float gameDuration;
    PhotonView photonView;

    void Start()
    {
        photonView = GetComponent<PhotonView>();

        if (PhotonNetwork.IsMasterClient)
        {
            // Host ustawia czas początkowy, tylko raz
            startTime = PhotonNetwork.Time;
            Hashtable roomProperties = new Hashtable { { "StartTime", startTime } };
            PhotonNetwork.CurrentRoom.SetCustomProperties(roomProperties);
        }
        else
        {
            // Czekamy, aż Host przekaże nam czas
            if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue("StartTime", out object startTimeObj))
            {
                startTime = (double)startTimeObj;
            }
            else
            {
                photonView.RPC("SyncStartTime", RpcTarget.AllBuffered, startTime);
            }
        }

        gameDuration = FindObjectOfType<GameTimer>().GameDuration;
    }
    void Update()
    {
        if (startTime == 0) // Jeśli startTime nie zostało ustawione (brak synchronizacji), czekamy
        {
            if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue("StartTime", out object startTimeObj))
            {
                startTime = (double)startTimeObj;
            }
            return;
        }

        double elapsedTime = PhotonNetwork.Time - startTime;
        double remainingTime = Mathf.Max(0, (float)(gameDuration - elapsedTime));

        int minutes = Mathf.FloorToInt((float)remainingTime / 60);
        int seconds = Mathf.FloorToInt((float)remainingTime % 60);

        timerText.text = $"{minutes:00}:{seconds:00}";
    }

    [PunRPC]
    private void SyncStartTime(double syncStartTime)
    {
        startTime = syncStartTime; // Synchronizujemy czas u wszystkich graczy
    }

    [PunRPC]
    public void ResetTimeUI()
    {
        photonView = GetComponent<PhotonView>();

        if (PhotonNetwork.IsMasterClient)
        {
            // Host ustawia czas początkowy, tylko raz
            startTime = PhotonNetwork.Time;
            Hashtable roomProperties = new Hashtable { { "StartTime", startTime } };
            PhotonNetwork.CurrentRoom.SetCustomProperties(roomProperties);
        }
        else
        {
            // Czekamy, aż Host przekaże nam czas
            if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue("StartTime", out object startTimeObj))
            {
                startTime = (double)startTimeObj;
            }
            else
            {
                photonView.RPC("SyncStartTime", RpcTarget.AllBuffered, startTime);
            }
        }

        gameDuration = FindObjectOfType<GameTimer>().GameDuration;
    }
}