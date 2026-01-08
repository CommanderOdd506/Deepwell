using Photon.Pun;
using UnityEngine;
using System.Collections;

public class SpawnPlayers : MonoBehaviour
{
    public GameObject playerPrefab;
    public Transform[] spawns;

    void Start()
    {
        StartCoroutine(WaitForRoomAndSpawn());
    }

    public Vector3 GetRandomSpawn()
    {
        int spawnIndex = Random.Range(0, spawns.Length - 1);
        return spawns[spawnIndex].position;
    }

    IEnumerator WaitForRoomAndSpawn()
    {
        while (!PhotonNetwork.InRoom)
        {
            yield return null; // wait until actually inside room
        }

        Debug.Log($"[SPAWN] Actor {PhotonNetwork.LocalPlayer.ActorNumber} spawning");

        int spawnIndex = Random.Range(0, spawns.Length - 1);
        PhotonNetwork.Instantiate(playerPrefab.name, spawns[spawnIndex].position, Quaternion.identity);
    }
}