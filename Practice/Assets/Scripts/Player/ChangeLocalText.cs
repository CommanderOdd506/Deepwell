using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Photon.Pun;
public class ChangeLocalText : MonoBehaviour
{
    public TextMeshProUGUI startButtonText;
    public GameObject startButtonObject;
    
    // Start is called before the first frame update
    void Start()
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            startButtonObject.SetActive(false);
            if (GameModeManager.Instance.InGame())
            {
                startButtonObject.SetActive(true);
                startButtonText.text = "Join";
            }
            
        }
    }

}
