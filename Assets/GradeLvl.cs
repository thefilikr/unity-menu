using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class LevelSelection : MonoBehaviour
{
    [System.Serializable]
    public class LevelButton
    {
        public Button button;
        public Image[] stars; // Массив изображений звезд (3 элемента)
        public int levelIndex;
        public string levelName; // Добавляем имя уровня
    }

    [Header("Настройки")]
    public Sprite emptyStar;
    public Sprite filledStar;
    public LevelButton[] levelButtons;

    private const string GRADE_SUFFIX = "_grade";
    private const string COMPLETED_SUFFIX = "_completed";

    private void Start()
    {
        // Первый уровень всегда доступен
        UnlockLevel(levelButtons[0].levelName);
        UpdateLevelButtons();
    }

    private void UpdateLevelButtons()
    {
        for (int i = 0; i < levelButtons.Length; i++)
        {
            bool isUnlocked = IsLevelUnlocked(i);
            int starsEarned = GetStarsForLevel(levelButtons[i].levelName);

            // Настраиваем кнопку
            levelButtons[i].button.interactable = isUnlocked;
            
            // Настраиваем звезды
            for (int j = 0; j < levelButtons[i].stars.Length; j++)
            {
                levelButtons[i].stars[j].sprite = (j < starsEarned) ? filledStar : emptyStar;
                levelButtons[i].stars[j].gameObject.SetActive(isUnlocked);
            }

            // Добавляем обработчик клика
            int levelIndex = i;
            levelButtons[i].button.onClick.RemoveAllListeners();
            levelButtons[i].button.onClick.AddListener(() => LoadLevel(levelIndex));
        }
    }

    private bool IsLevelUnlocked(int buttonIndex)
    {
        // Первый уровень всегда разблокирован
        if (buttonIndex == 0) return true;
        
        // Проверяем, пройден ли предыдущий уровень
        string prevLevelName = levelButtons[buttonIndex-1].levelName;
        return PlayerPrefs.GetInt(prevLevelName + COMPLETED_SUFFIX, 0) == 1;
    }

    private int GetStarsForLevel(string levelName)
    {
        int grade = PlayerPrefs.GetInt(levelName + GRADE_SUFFIX, 0);
        
        return Mathf.Clamp(grade, 0, 3);
    }

    private void LoadLevel(int buttonIndex)
    {
        if (buttonIndex >= 0 && buttonIndex < levelButtons.Length && 
            IsLevelUnlocked(buttonIndex))
        {
            string levelName = levelButtons[buttonIndex].levelName;
            Debug.Log($"Загрузка уровня: {levelName}");
            SceneManager.LoadScene(levelName);
        }
    }

    private void UnlockLevel(string levelName)
    {
        PlayerPrefs.SetInt(levelName + COMPLETED_SUFFIX, 1);
        PlayerPrefs.Save();
    }
}