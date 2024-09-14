using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using UnityEngine.UI;


public enum GameState
{
    Standby,
    Playing,
    Lose,
    Win
}


public class GameManager : MonoBehaviour
{
    //Delcarations
    [SerializeField] private GameState _gameState = GameState.Standby;
    private bool _isPaused = false;
    [SerializeField] private List<AiNestBehavior> _nests;
    [Header("UI")]
    [SerializeField] private GameObject _controlsDescription;
    [SerializeField] private GameObject _objectiveDescription;


    //Internals
    private void SetPause(bool newState)
    {
        if (newState == false)
        {
            _isPaused = false;
            Time.timeScale = 1;
        }
            
        else
        {
            _isPaused = true;
            Time.timeScale = 0;
        }
    }

    private void RestartApplication()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void ExitApplication()
    {
        Application.Quit();
    }

    private void ActivateNests()
    {
        _gameState = GameState.Playing;

        foreach (AiNestBehavior nest in _nests)
            nest.ActivateNest();
    }

    private void LoseGame()
    {
        _gameState = GameState.Lose;
    }

    private void WinGame()
    {
        _gameState = GameState.Win;
    }

    private void ShowControls()
    {
        _controlsDescription.SetActive(true);
    }

    private void HideControls() 
    {
        _controlsDescription.SetActive(false);
    }

    private void ShowObjective()
    {
        _objectiveDescription.SetActive(true);
    }

    private void HideObjective()
    {
        _objectiveDescription.SetActive(false);
    }


    //Externals
    public void Pause()
    {
        SetPause(true);
    }

    public void Resume()
    {
        SetPause(false);
    }

    public void TogglePause()
    {
        if (_isPaused)
            SetPause(false);
        else SetPause(true);
    }

    public void RestartGame()
    {
        RestartApplication();
    }

    public void QuitGame()
    {
        ExitApplication();
    }

    public void StartSpawns()
    {
        ActivateNests();
    }

    public void TriggerGameWin()
    {
        WinGame();
    }

    public void TriggerGameLose()
    {
        LoseGame();
    }

    public void ToggleControlsUI()
    {
        //hide controls if they're showing
       if (_controlsDescription.activeSelf)
            HideControls();

       //otherwise show the controls (and hide the objective text if it's showing)
       else
        {
            ShowControls();
            HideObjective();
        }
    }

    public void ToggleObjectiveUI()
    {
        //hide the objective if it's showing
        if (_objectiveDescription.activeSelf)
            HideObjective();

        //otherwise show the Objective (and hide the controls if they're showing)
        else
        {
            ShowObjective();
            HideControls();
        }
    }


}
