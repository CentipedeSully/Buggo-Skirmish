using UnityEngine;
using UnityEngine.SceneManagement;


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

}
