using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using UnityEngine.UI;
using System;
using TMPro;
using UnityEngine.InputSystem;


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
    [SerializeField] private PlayerBehavior _player;
    [SerializeField] private PlayerNestBehavior _playerNest;


    [Header("UI")]
    [SerializeField] private GameObject _controlsDescription;
    [SerializeField] private GameObject _objectiveDescription;
    [SerializeField] private GameObject _pausePopup;
    [SerializeField] private GameObject _winPopup;
    [SerializeField] private GameObject _losePopup;
    [SerializeField] private TextMeshProUGUI _loseMessage;

    



    //monobehavior
    private void Start()
    {
        _gameState = GameState.Playing;

        foreach (AiNestBehavior nest in _nests)
            nest.SetGameManager(this);

        _playerNest.SetGameManager(this);
        _player.SetGameManager(this);
    }



    //Internals
    private void SetPause(bool newState)
    {
        if (newState == false)
        {
            _pausePopup.SetActive(false);
            _isPaused = false;
            Time.timeScale = 1;
        }
            
        else
        {
            _pausePopup.SetActive(true);
            _isPaused = true;
            Time.timeScale = 0;
        }
    }

    private void RestartApplication()
    {
        if (Time.timeScale != 1)
            Time.timeScale = 1;

        //reload scene
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

    private void LoseGame(string loseMessage = "")
    {
        _gameState = GameState.Lose;
        _losePopup.SetActive(true);
        _loseMessage.text = loseMessage;
    }

    private void WinGame()
    {
        _gameState = GameState.Win;
        _winPopup.SetActive(true);
    }




    //Externals
    public void RemoveNestFromActiveNests(AiNestBehavior defeatedNest)
    {
        if (_nests.Contains(defeatedNest) && _gameState == GameState.Playing)
        {
            _nests.Remove(defeatedNest);

            if (_nests.Count == 0)
                WinGame();
        }
            

    }

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
        if (_gameState == GameState.Playing)
            WinGame();
    }

    public void TriggerGameLose( string loseMessage = "")
    {
        if (_gameState == GameState.Playing)
            LoseGame(loseMessage);
    }


}
