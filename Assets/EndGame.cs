using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class EndGame : MonoBehaviour
{
    [Header("Настройки")]
    public LevelDatabase levelDatabase;
    public int currentLevelIndex = 0;

    private int grade = 0;
    private float levelStartTime;
    private bool levelCompeted = false;
    private LevelDatabase.LevelInfo level;

    [Header("Тех настройка")]
    [Tooltip("Нужно ли загружать следующий уровень")]
    public bool loadNextScene = false;
    [Tooltip("Название следующего уровня")]
    public string nextLevelName;

    [Header("Menu")]
    [SerializeField] private GameObject winScreen;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button menuButton;
    [SerializeField] private Button nextButton;
    [SerializeField] private string nextLevel = "Level_1";
    [SerializeField] private string menuScene = "MainMenu";
    [SerializeField] private GameObject[] stars;
    [SerializeField] private TMP_Text[] timeTexts;
    [SerializeField] private TMP_Text gameTimeTexts;
    [SerializeField] private string timeFormat = "mm':'ss'.'ff";

    [SerializeField] private Animator winScreenAnimator;
    [SerializeField] private Animator starsAnimator;
    [SerializeField] private string winScreenTrigger = "Show";
    [SerializeField] private string starsTrigger = "Show";

    [Header("Цвета времени")]
    [SerializeField] private Color completedTimeColor = Color.yellow; 
    [SerializeField] private Color failedTimeColor = new Color(0.7f, 0.7f, 0.7f); 
    [SerializeField] private Color defaultTimeColor = Color.white; 

    private const string GRADE = "_grade";

    private void Start()
    {
        winScreenAnimator.gameObject.SetActive(false);
        winScreen.SetActive(false);
        
        if (restartButton != null) restartButton.onClick.AddListener(RestartGame);
            
        if (menuButton != null) menuButton.onClick.AddListener(GoToMenu);

        if (nextButton != null) nextButton.onClick.AddListener(GoToLevel);

        if (levelDatabase == null || currentLevelIndex >= levelDatabase.levels.Length) return;
        
        level = levelDatabase.levels[currentLevelIndex];

        grade = PlayerPrefs.GetInt($"Level_{level.LevelName + GRADE}", 0);
        levelStartTime = Time.time;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            CompleteLevel();
        }
    }

    private void CompleteLevel()
    {
        winScreen.SetActive(true); 
        winScreenAnimator.gameObject.SetActive(true);
        winScreenAnimator.SetTrigger(winScreenTrigger);

        float completeTime = Time.time - levelStartTime;
        int nowGrade = CalcGrade(completeTime);
        if (nowGrade > grade) grade = nowGrade;

        // Отображаем время под звездами
        DisplayTimes(completeTime);

        for (int i=0; i<grade; i++)
        {
            stars[i].SetActive(true);
        }

        Invoke("ShowStars", 0.5f);
        
        if (grade != 0) 
        {
            PlayerPrefs.SetInt($"Level_{level.LevelName}_Completed", 1);
            PlayerPrefs.SetInt($"Level_{level.LevelName + GRADE}", grade);
            PlayerPrefs.Save();
        }

        if(loadNextScene && !string.IsNullOrEmpty(nextLevelName))
        {
            SceneManager.LoadScene(nextLevelName);
        }
    }

    private int CalcGrade(float time)
    {
        
        if (time <= level.timeGrade3) return 3;
        if (time <= level.timeGrade2) return 2;
        if (time <= level.timeGrade1) return 1;
        return 0;
    }

    private void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    
    private void GoToMenu()
    {
        SceneManager.LoadScene(menuScene); 
    }

    private void GoToLevel()
    {
        SceneManager.LoadScene(nextLevel);
    }

    private void ShowStars()
    {
        starsAnimator.SetTrigger(starsTrigger);
    }
    
    private void DisplayTimes(float playerTime)
    {
        // Получаем пороговые времена для звезд из текущего уровня
        float[] starTimeThresholds = new float[] 
        {
            level.timeGrade3,
            level.timeGrade2,
            level.timeGrade1
        };

        // Отображаем время под каждой звездой
        for (int i = 0; i < timeTexts.Length && i < starTimeThresholds.Length; i++)
        {
            if (timeTexts[i] == null) continue;
            
            // Устанавливаем базовый цвет
            timeTexts[i].color = defaultTimeColor;
            
            // Устанавливаем текст с пороговым временем для звезды
            timeTexts[i].text = FormatTime(starTimeThresholds[i]);
            
            // Меняем цвет, если игрок уложился в это время
            if (playerTime <= starTimeThresholds[i])
            {
                timeTexts[i].color = completedTimeColor;
            }
            else if (i == 0) // Для самого строгого времени можно особый цвет
            {
                timeTexts[i].color = failedTimeColor;
            }
            
            // Добавляем анимацию для текста
            if (timeTexts[i].TryGetComponent<Animator>(out var animator))
            {
                animator.SetTrigger("Show");
            }
        }
    }

    private string FormatTime(float time)
    {
        // Создаем TimeSpan из секунд
        System.TimeSpan timeSpan = System.TimeSpan.FromSeconds(time);

        // Форматируем время в минуты:секунды.сотые
        // Используем стандартные форматеры без кавычек
        return string.Format("{0:00}:{1:00}.{2:00}",
            timeSpan.Minutes + timeSpan.Hours * 60,
            timeSpan.Seconds,
            timeSpan.Milliseconds / 10);
    }
}
