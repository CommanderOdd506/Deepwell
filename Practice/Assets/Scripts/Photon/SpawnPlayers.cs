using Photon.Pun;
using UnityEngine;
using System.Collections;

public class SpawnPlayers : MonoBehaviour
{
    public GameObject playerPrefab;
    public Transform spawn;

    void Start()
    {
        StartCoroutine(WaitForRoomAndSpawn());
    }

    IEnumerator WaitForRoomAndSpawn()
    {
        while (!PhotonNetwork.InRoom)
        {
            yield return null; // wait until actually inside room
        }

        Debug.Log($"[SPAWN] Actor {PhotonNetwork.LocalPlayer.ActorNumber} spawning");

        PhotonNetwork.Instantiate(playerPrefab.name, spawn.position, Quaternion.identity);
    }
}