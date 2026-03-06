using Photon.Pun;
using UnityEngine;

public class SpawnPlayers : MonoBehaviourPun
{
    public GameObject playerPrefab;
    public Transform[] spawns;

    bool hasSpawned = false;

    public Vector3 GetRandomSpawn()
    {
        int spawnIndex = Random.Range(0, spawns.Length);
        return spawns[spawnIndex].position;
    }

    private void SpawnLocalPlayer()
    {
        if (hasSpawned) return;

        hasSpawned = true;

        Debug.Log($"[SPAWN] Actor {PhotonNetwork.LocalPlayer.ActorNumber} spawning");

        RotateObject rotator = FindObjectOfType<RotateObject>();
        if (rotator != null)
            rotator.gameObject.SetActive(false);

        // Destroy old player safely
        if (PhotonNetwork.LocalPlayer.TagObject != null)
        {
            int actor = PhotonNetwork.LocalPlayer.ActorNumber;

            if (DamageSystem.Instance != null)
            {
                DamageSystem.Instance.CleanupActor(actor);
            }

            PhotonNetwork.Destroy((GameObject)PhotonNetwork.LocalPlayer.TagObject);
            PhotonNetwork.LocalPlayer.TagObject = null;
        }

        Vector3 spawnPos = GetRandomSpawn();

        GameObject newPlayer =
            PhotonNetwork.Instantiate(playerPrefab.name, spawnPos, Quaternion.identity);

        PhotonNetwork.LocalPlayer.TagObject = newPlayer;
    }

    [PunRPC]
    void SpawnPlayerRPC()
    {
        SpawnLocalPlayer();
    }


    public void SpawnPlayerLate()
    {
        Debug.Log($"[SPAWN] Actor {PhotonNetwork.LocalPlayer.ActorNumber} spawning");

        SpawnLocalPlayer();
    }

    public void StartGame()
    {
        if (!PhotonNetwork.IsMasterClient) return;

        photonView.RPC("SpawnPlayerRPC", RpcTarget.All);
    }
}