using UnityEngine;
using Photon.Pun;

public class CarControl : MonoBehaviourPun
{
    [Header("Movement")]
    [SerializeField] private float speed = 10f;
    [SerializeField] private Vector3 direction = Vector3.forward;

    [Header("Lifetime")]
    [SerializeField] private float maxDistance = 100f;

    private Vector3 startPosition;
    private float traveledDistance;

    void Start()
    {
        startPosition = transform.position;
    }

    void Update()
    {
        if (!photonView.IsMine)
            return;

        transform.position += direction.normalized * speed * Time.deltaTime;

        traveledDistance = Vector3.Distance(startPosition, transform.position);

        if (traveledDistance >= maxDistance)
        {
            PhotonNetwork.Destroy(gameObject);
        }
    }
}
