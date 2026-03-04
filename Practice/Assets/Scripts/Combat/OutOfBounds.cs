using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using TMPro;

public class OutOfBounds : MonoBehaviourPun
{

    public float allowedOutTime = 10f;
    private bool _timerRunning;
    private float _boundsTimer;
    public GameObject outOfBoundsPanel;
    public TextMeshProUGUI boundsCountdown;

    void Start()
    {
        _boundsTimer = allowedOutTime;
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "OutOfBounds" && !_timerRunning)
        {
            _timerRunning = true;
            outOfBoundsPanel.SetActive(true);
            Debug.Log("[OutOfBounds} Timer Started ! ");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("OutOfBounds"))
        {
            _timerRunning = false;
            _boundsTimer = allowedOutTime;// reset timer
            outOfBoundsPanel.SetActive(false);
            Debug.Log("[OutOfBounds} Timer Stopped ! ");
        }
    }
    // Update is called once per frame
    void Update()
    {
        if (!photonView.IsMine) return;
        if (!_timerRunning) return;

        _boundsTimer -= Time.deltaTime;

        int secondsLeft = Mathf.CeilToInt(_boundsTimer);
        boundsCountdown.text = secondsLeft.ToString();
        if (_boundsTimer <= 0f)
        {
            _timerRunning = false;
            _boundsTimer = allowedOutTime;
            outOfBoundsPanel.SetActive(false);

            photonView.RPC(
                "RPC_RequestKill",
                RpcTarget.MasterClient,
                photonView.Owner.ActorNumber
            );
        }
    }

    [PunRPC]
    private void RPC_RequestKill(int actorNumber)
    {
        if (!PhotonNetwork.IsMasterClient)
            return;

        // Apply lethal damage
        DamageSystem.Instance.ApplyEnvironmentDamage(actorNumber, 9999);
    }
}
