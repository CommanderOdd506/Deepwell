using UnityEngine;
using Photon.Pun;

public class KillTrigger : MonoBehaviourPun
{
    private void OnTriggerEnter(Collider other)
    {
        PlayerHealth health = other.GetComponent<PlayerHealth>();
        if (health == null)
            return;

        int actorNumber = health.photonView.Owner.ActorNumber;

        // Ask Master to kill this player
        photonView.RPC(
            "RPC_RequestKill",
            RpcTarget.MasterClient,
            actorNumber
        );
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