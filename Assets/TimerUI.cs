using ExitGames.Client.Photon;
using Photon.Pun;
using TMPro;
using UnityEngine;

public class TimerUI : MonoBehaviourPunCallbacks
{
    [SerializeField] private TextMeshProUGUI timerText;
    private double startTime;
    private float gameDuration;
    private PhotonView photonView;

    private const string IsGameRunningKey = "IsGameRunning";

    public void StartGameTimer()
    {
        photonView = GetComponent<PhotonView>();
        startTime = PhotonNetwork.Time;
        gameDuration = FindObjectOfType<GameTimer>().GameDuration;
    }

    void Update()
    {
        if (!IsGameRunning()) // Timer działa tylko, gdy gra jest włączona
        {
            timerText.text = "--:--"; // Wyświetlamy placeholder, jeśli gra nie jest uruchomiona
            return;
        }

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

    private bool IsGameRunning()
    {
        if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue(IsGameRunningKey, out object isRunningObj))
        {
            return (bool)isRunningObj;
        }
        return false; // Domyślnie gra nie jest włączona
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
            Hashtable roomProperties = new Hashtable
            {
                { "StartTime", startTime },
                { IsGameRunningKey, true } // Ustawiamy grę jako uruchomioną
            };
            PhotonNetwork.CurrentRoom.SetCustomProperties(roomProperties);
        }
        else
        {
            // Czekamy, aż Host przekaże nam czas
            if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue("StartTime", out object startTimeObj))
            {
                startTime = (double)startTimeObj;
            }
        }

        gameDuration = FindObjectOfType<GameTimer>().GameDuration;
    }
}
