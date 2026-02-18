using UnityEngine;
using Photon.Pun;
using TMPro;

public class MainMenu : MonoBehaviourPun
{
    public GameObject createJoin;
    public GameObject nicknamePanel;
    public GameObject mainScreen;

    public TMP_InputField nameInput;

    private const int MIN_NAME_LENGTH = 3;
    private const int MAX_NAME_LENGTH = 16;

    void Start()
    {
        if (PlayerPrefs.HasKey("PlayerNickname"))
        {
            PhotonNetwork.NickName = PlayerPrefs.GetString("PlayerNickname");
        }
    }

    public void SetNickname()
    {
        string nickname = nameInput.text.Trim();

        if (!IsValidNickname(nickname))
        {
            Debug.Log("Invalid nickname.");
            return;
        }

        PhotonNetwork.NickName = nickname;

        PlayerPrefs.SetString("PlayerNickname", nickname);
        PlayerPrefs.Save();

        nicknamePanel.SetActive(false);
        createJoin.SetActive(true);
    }

    private bool IsValidNickname(string nickname)
    {
        if (string.IsNullOrEmpty(nickname))
            return false;

        if (nickname.Length < MIN_NAME_LENGTH)
            return false;

        if (nickname.Length > MAX_NAME_LENGTH)
            return false;

        if (nickname.Replace(" ", "").Length == 0)
            return false;

        foreach (char c in nickname)
        {
            if (!char.IsLetterOrDigit(c) && c != '_')
                return false;
        }

        return true;
    }

    public void PlayButton()
    {
        if (PlayerPrefs.HasKey("PlayerNickname"))
        {
            PhotonNetwork.NickName = PlayerPrefs.GetString("PlayerNickname");

            mainScreen.SetActive(false);
            nicknamePanel.SetActive(false);
            createJoin.SetActive(true);
        }
        else
        {
            mainScreen.SetActive(false);
            nicknamePanel.SetActive(true);
            createJoin.SetActive(false);
        }
    }
}
