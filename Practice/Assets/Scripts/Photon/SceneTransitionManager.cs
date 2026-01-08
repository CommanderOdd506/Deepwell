using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;

public class SceneTransitionManager : MonoBehaviourPunCallbacks
{
    private static SceneTransitionManager instance;

    static bool leaving;
    static bool quitting;

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public static void ReturnToLobby()
    {
        if (instance == null)
        {
            Debug.LogError("[SceneTransition] No instance found!");
            return;
        }

        leaving = true;
        instance.StartLeavingRoom();
    }

    public static void QuitGame()
    {
        if (instance == null)
        {
            Debug.LogError("[SceneTransition] No instance found!");
            return;
        }

        quitting = true;
        instance.StartLeavingRoom();
    }

    private void StartLeavingRoom()
    {
        Debug.Log("[SceneTransition] Leaving room...");

        if (PhotonNetwork.InRoom)
        {
            PhotonNetwork.LeaveRoom();
        }
        else
        {
            LoadLobby();
        }
    }

    public override void OnLeftRoom()
    {
        Debug.Log("[SceneTransition] Left room successfully, loading Lobby...");
        if (leaving)
        {
            LoadLobby();
        }
        if (quitting)
        {
            Application.Quit();
        }
    }

    private void LoadLobby()
    {
        SceneManager.LoadScene("Loading");
    }
}
