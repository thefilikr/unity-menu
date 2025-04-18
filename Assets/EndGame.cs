using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

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

    private const string GRADE = "_grade";

    private void Start()
    {
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
        float completeTime = Time.time - levelStartTime;
        int nowGrade = CalcGrade(completeTime);
        if (nowGrade > grade) grade = nowGrade;
        
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
}
