using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using TMPro;

public class OutOfBounds : MonoBehaviourPun
{
    public float allowedOutTime = 10f;
    private float _boundsTimer;
    private int _oobZoneCount = 0; // ? track how many OOB zones we're inside

    public GameObject outOfBoundsPanel;
    public TextMeshProUGUI boundsCountdown;

    void Start()
    {
        _boundsTimer = allowedOutTime;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("OutOfBounds")) return;

        _oobZoneCount++;

        if (_oobZoneCount == 1) // just entered first zone
        {
            outOfBoundsPanel.SetActive(true);
            Debug.Log("[OutOfBounds] Timer Started!");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("OutOfBounds")) return;

        _oobZoneCount = Mathf.Max(0, _oobZoneCount - 1); // safety clamp

        if (_oobZoneCount == 0) // fully back in bounds
        {
            _boundsTimer = allowedOutTime;
            outOfBoundsPanel.SetActive(false);
            Debug.Log("[OutOfBounds] Timer Stopped!");
        }
    }

    void Update()
    {
        if (!photonView.IsMine) return;
        if (_oobZoneCount == 0) return; // ? use count, not bool

        _boundsTimer -= Time.deltaTime;
        int secondsLeft = Mathf.CeilToInt(_boundsTimer);
        boundsCountdown.text = secondsLeft.ToString();

        if (_boundsTimer <= 0f)
        {
            _boundsTimer = allowedOutTime;
            _oobZoneCount = 0;
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
        if (!PhotonNetwork.IsMasterClient) return;
        DamageSystem.Instance.ApplyEnvironmentDamage(actorNumber, 9999);
    }
}