using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PauseMenu : MonoBehaviour
{
    public GameObject pausePanel;
    private bool paused;

    private PlayerInput playerInput;
    private PlayerMovement playerMovement;
    private MouseLook mouseLook;
    private PlayerCombatController playerCombatController;
    private HeadBobController headBobController;

    void Start()
    {
        playerInput = GetComponent<PlayerInput>();
        playerMovement = GetComponent<PlayerMovement>();
        mouseLook = GetComponent<MouseLook>();
        playerCombatController = GetComponent<PlayerCombatController>();
        headBobController = GetComponent<HeadBobController>();
    }

    void Update()
    {
        if (playerInput.pausePressed)
        {
            if (!paused)
                Pause();
            else
                Resume();
        }
    }

    private void Pause()
    {
        if (paused)
            return;

        paused = true;
        pausePanel.SetActive(true);
        mouseLook.ToggleMovement(false);
        playerMovement.ToggleMovement(false);
        playerCombatController.ToggleControl(false);
        headBobController.ToggleMotion(false);
    }

    public void LeaveRoom()
    {
        Debug.Log("[PauseMenu] Requesting leave room...");
        SceneTransitionManager.ReturnToLobby();
    }

    public void Quit()
    {
        Debug.Log("[PauseMenu] Requesting Quit...");
        SceneTransitionManager.QuitGame();
    }

    private void Resume()
    {
        if (!paused)
            return;

        paused = false;
        pausePanel.SetActive(false);
        mouseLook.ToggleMovement(true);
        playerMovement.ToggleMovement(true);
        playerCombatController.ToggleControl(true);
        headBobController.ToggleMotion(true);
    }

    public bool IsPaused()
    {
        return paused;
    }
}
