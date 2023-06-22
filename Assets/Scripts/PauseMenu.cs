using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    public GameObject pauseMenuUI;
    private GameActions _actions;

    private void Awake()
    {
        _actions = new GameActions();
        _actions.pause.openPauseMenu.performed += OnPauseGame;
    }

    private void OnEnable()
    {
        _actions.pause.Enable();
    }

    private void OnDisable()
    {
        _actions.pause.Disable();
    }

    private void OnPauseGame(InputAction.CallbackContext context)
    {
        Time.timeScale = 1f - Time.timeScale;
        pauseMenuUI.SetActive(Time.timeScale == 0);
    }

    public void LoadMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}