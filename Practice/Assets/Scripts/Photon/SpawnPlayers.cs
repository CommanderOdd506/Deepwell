using Photon.Pun;
using UnityEngine;

public class SpawnPlayers : MonoBehaviourPun
{
    public GameObject playerPrefab;
    public Transform[] spawns;

    public Vector3 GetRandomSpawn()
    {
        int spawnIndex = Random.Range(0, spawns.Length);
        return spawns[spawnIndex].position;
    }

    [PunRPC]
    void SpawnPlayerRPC()
    {
        Debug.Log($"[SPAWN] Actor {PhotonNetwork.LocalPlayer.ActorNumber} spawning");

        GameObject.FindObjectOfType<RotateObject>().gameObject.SetActive(false);

        Vector3 spawnPos = GetRandomSpawn();
        PhotonNetwork.Instantiate(playerPrefab.name, spawnPos, Quaternion.identity);
    }

    public void SpawnPlayerLate()
    {
        Debug.Log($"[SPAWN] Actor {PhotonNetwork.LocalPlayer.ActorNumber} spawning");

        GameObject.FindObjectOfType<RotateObject>().gameObject.SetActive(false);

        Vector3 spawnPos = GetRandomSpawn();
        PhotonNetwork.Instantiate(playerPrefab.name, spawnPos, Quaternion.identity);
    }

    public void StartGame()
    {
        if (!PhotonNetwork.IsMasterClient) return;

        photonView.RPC("SpawnPlayerRPC", RpcTarget.All);
    }
}