using UnityEngine;
using Photon.Pun;
using TMPro;

public class MatchCountdownUI : MonoBehaviour
{
    public GameObject countdownPanel;
    public TextMeshProUGUI countdownText;

    double startTime;

    void Update()
    {
        if (PhotonNetwork.CurrentRoom == null)
            return;

        // Check match state
        bool gameStarted = false;

        if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue("GameStarted", out object started))
        {
            gameStarted = (bool)started;
        }

        // If match started ? hide panel
        if (gameStarted)
        {
            countdownPanel.SetActive(false);
            return;
        }

        // Match not started ? show panel
        countdownPanel.SetActive(true);

        // Get start time
        if (!PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue("MatchStartTime", out object value))
            return;

        startTime = (double)value;

        double timeLeft = startTime - PhotonNetwork.Time;

        if (timeLeft > 0)
        {
            countdownText.text = Mathf.CeilToInt((float)timeLeft).ToString();
            countdownText.gameObject.SetActive(true);
        }
        else
        {
            countdownText.text = "GO";
            countdownText.gameObject.SetActive(false);
        }
    }
}