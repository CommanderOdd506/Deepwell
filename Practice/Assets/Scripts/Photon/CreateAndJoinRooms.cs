using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using TMPro;

public class CreateAndJoinRooms : MonoBehaviourPunCallbacks
{
    public TMP_InputField createInput;
    public TMP_InputField joinInput;

    public void CreateRoom()
    {
        Debug.Log($"[CREATE] Starting CreateRoom. InLobby: {PhotonNetwork.InLobby}");

        if (!PhotonNetwork.InLobby)
        {
            Debug.LogError("Cannot create room: Not in lobby yet.");
            return;
        }

        string roomName = createInput.text.Trim();

        if (string.IsNullOrEmpty(roomName))
        {
            Debug.LogError("Create failed: Room name is empty.");
            return;
        }

        Debug.Log($"[CREATE] Attempting to create room: '{roomName}'");

        RoomOptions roomOptions = new RoomOptions();
        roomOptions.MaxPlayers = 4;
        roomOptions.IsVisible = true;
        roomOptions.IsOpen = true;

        PhotonNetwork.CreateRoom(roomName, roomOptions);
    }

    public void JoinRoom()
    {
        Debug.Log($"[JOIN] Starting JoinRoom. InLobby: {PhotonNetwork.InLobby}");

        if (!PhotonNetwork.InLobby)
        {
            Debug.LogError("Cannot join room: Not in lobby yet.");
            return;
        }

        string roomName = joinInput.text.Trim();

        if (string.IsNullOrEmpty(roomName))
        {
            Debug.LogError("Join failed: Room name is empty.");
            return;
        }

        Debug.Log($"[JOIN] Attempting to join room: '{roomName}'");
        PhotonNetwork.JoinRoom(roomName);
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        Debug.LogError($"[JOIN FAILED] Code: {returnCode} | Message: {message}");
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        Debug.LogError($"[CREATE FAILED] Code: {returnCode} | Message: {message}");
    }

    public override void OnJoinedRoom()
    {
        Debug.Log($"[SUCCESS] Joined room: {PhotonNetwork.CurrentRoom.Name}");
        Debug.Log($"[SUCCESS] Players in room: {PhotonNetwork.CurrentRoom.PlayerCount}");
        Debug.Log($"[SUCCESS] Is Master Client: {PhotonNetwork.IsMasterClient}");

        if (PhotonNetwork.IsMasterClient)
        {
            Debug.Log("[SUCCESS] Master client loading Game scene...");
            PhotonNetwork.LoadLevel("Game");
        }
        else
        {
            Debug.Log("[SUCCESS] Waiting for master client to load scene...");
        }
    }
}
