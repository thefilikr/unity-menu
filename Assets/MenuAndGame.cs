using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class WinScreenManager : MonoBehaviour
{
    [SerializeField] private GameObject winScreen;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button menuButton;
    [SerializeField] private Button nextButton;
    [SerializeField] private string nextLevel = "Level_1";
    [SerializeField] private string menuScene = "MainMenu";
    
    void Start()
    {
        winScreen.SetActive(false);
        
        if (restartButton != null) restartButton.onClick.AddListener(RestartGame);
            
        if (menuButton != null) menuButton.onClick.AddListener(GoToMenu);

        if (nextButton != null) nextButton.onClick.AddListener(GoToLevel);
    }
    
    public void ShowWinScreen()
    {
        winScreen.SetActive(true);
        Time.timeScale = 0f; 
    }
    
    private void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    
    private void GoToMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(menuScene); 
    }

    private void GoToLevel()
    {
        SceneManager.LoadScene(nextLevel);
    }
}