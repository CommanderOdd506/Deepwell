using UnityEngine;
using Photon.Pun;
using System.Collections;

public class CarSpawner : MonoBehaviourPunCallbacks
{
    [Header("Spawn Settings")]
    [SerializeField] private GameObject[] carPrefabs;
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private Vector3 carDirection = Vector3.forward;

    [Header("Timing")]
    [SerializeField] private float minSpawnInterval = 5f;
    [SerializeField] private float maxSpawnInterval = 15f;

    void Start()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            StartCoroutine(SpawnCarsRoutine());
        }
    }

    IEnumerator SpawnCarsRoutine()
    {
        while (true)
        {
            float waitTime = Random.Range(minSpawnInterval, maxSpawnInterval);
            yield return new WaitForSeconds(waitTime);

            SpawnCar();
        }
    }

    void SpawnCar()
    {
        if (!PhotonNetwork.IsMasterClient)
            return;

        if (carPrefabs.Length == 0 || spawnPoints.Length == 0)
        {
            Debug.LogWarning("[CarSpawner] No car prefabs or spawn points assigned!");
            return;
        }

        GameObject carPrefab = carPrefabs[Random.Range(0, carPrefabs.Length)];
        Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];

        GameObject car = PhotonNetwork.Instantiate(
            carPrefab.name,
            spawnPoint.position,
            spawnPoint.rotation
        );

        CarController controller = car.GetComponent<CarController>();
        if (controller != null)
        {
            photonView.RPC("RPC_SetCarDirection", RpcTarget.AllBuffered, car.GetComponent<PhotonView>().ViewID, carDirection);
        }

        Debug.Log($"[CarSpawner] Spawned car at {spawnPoint.name}");
    }

    [PunRPC]
    void RPC_SetCarDirection(int carViewID, Vector3 direction)
    {
        PhotonView carView = PhotonView.Find(carViewID);
        if (carView != null)
        {
            CarController controller = carView.GetComponent<CarController>();
            if (controller != null)
            {
                controller.GetType().GetField("direction", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(controller, direction);
            }
        }
    }

    public override void OnMasterClientSwitched(Photon.Realtime.Player newMasterClient)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            StopAllCoroutines();
            StartCoroutine(SpawnCarsRoutine());
        }
    }
}
