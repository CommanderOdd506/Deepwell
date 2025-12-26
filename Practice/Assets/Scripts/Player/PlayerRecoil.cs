using UnityEngine;
using Photon.Pun;

public class PlayerRecoil : MonoBehaviourPun
{
    Vector3 originalPos;
    Vector3 currentRecoil;
    Vector3 targetRecoil;

    void Start()
    {
        if (!photonView.IsMine) return;
        originalPos = transform.localPosition;
    }

    void Update()
    {
        if (!photonView.IsMine) return;
        UpdateRecoil();
    }

    public void ApplyRecoil(RecoilData data)
    {
        targetRecoil += new Vector3(
            Random.Range(-data.kick.x, data.kick.x),
            data.kick.y,
            -data.kickBack
        );
    }

    void UpdateRecoil()
    {
        targetRecoil = Vector3.Lerp(
            targetRecoil,
            Vector3.zero,
            Time.deltaTime * 10f
        );

        currentRecoil = Vector3.Lerp(
            currentRecoil,
            targetRecoil,
            Time.deltaTime * 20f
        );

        transform.localPosition = originalPos + currentRecoil;
    }
}
