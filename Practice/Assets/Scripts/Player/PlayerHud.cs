using UnityEngine;
using TMPro;
using Photon.Pun;
using System.Collections;

public class PlayerHUD : MonoBehaviour
{
    public TextMeshProUGUI healthText;

    void Start()
    {
        StartCoroutine(AssignUI());
    }

    IEnumerator AssignUI()
    {
        // Wait until player exists
        while (PhotonNetwork.LocalPlayer == null)
            yield return null;

        PlayerHealth localPlayer = null;

        while (localPlayer == null)
        {
            foreach (PlayerHealth ph in FindObjectsOfType<PlayerHealth>())
            {
                if (ph.photonView.IsMine)
                {
                    localPlayer = ph;
                    break;
                }
            }
            yield return null;
        }

        localPlayer.SetHealthUI(healthText);
        Debug.Log("[HUD] Health UI assigned");
    }
}
