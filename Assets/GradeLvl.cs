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
        if (levelButtons == null || levelButtons.Length == 0)
        {
            Debug.LogError("LevelButtons array is not set in inspector!");
            return;
        }
        
        // Первый уровень всегда доступен
        if (levelButtons[0] != null)
        {
            UnlockLevel(levelButtons[0].levelName);
            UpdateLevelButtons();
        }
    }

    private void UpdateLevelButtons()
    {
        // Проверяем, что массив кнопок инициализирован
        if (levelButtons == null)
        {
            Debug.LogError("LevelButtons array is not set in inspector!");
            return;
        }

        // Проходим по всем кнопкам уровней
        for (int i = 0; i < levelButtons.Length; i++)
        {
            // Пропускаем null-элементы
            if (levelButtons[i] == null)
            {
                Debug.LogWarning($"LevelButton at index {i} is null!");
                continue;
            }

            // Проверяем наличие кнопки
            if (levelButtons[i].button == null)
            {
                Debug.LogWarning($"Button for level {i} is not set!");
                continue;
            }

            // Получаем статус и количество звезд для уровня
            bool isUnlocked = IsLevelUnlocked(i);
            int starsEarned = GetStarsForLevel(levelButtons[i].levelName);

            // Настраиваем кнопку
            levelButtons[i].button.interactable = isUnlocked;
            
            // Настраиваем цвет для заблокированных уровней
            var buttonColors = levelButtons[i].button.colors;
            buttonColors.disabledColor = new Color(0.3f, 0.3f, 0.3f); // Темно-серый цвет
            levelButtons[i].button.colors = buttonColors;

            // Настраиваем звезды (если массив существует)
            if (levelButtons[i].stars != null)
            {
                for (int j = 0; j < levelButtons[i].stars.Length; j++)
                {
                    // Пропускаем null-звезды
                    if (levelButtons[i].stars[j] == null) continue;
                    
                    // Устанавливаем спрайт звезды
                    levelButtons[i].stars[j].sprite = (j < starsEarned) ? filledStar : emptyStar;
                    // Показываем/скрываем звезду в зависимости от доступности уровня
                    levelButtons[i].stars[j].gameObject.SetActive(isUnlocked);
                }
            }
            else
            {
                Debug.LogWarning($"Stars array for level {i} is not set!");
            }

            // Добавляем обработчик клика с безопасным захватом индекса
            int levelIndex = i;
            levelButtons[i].button.onClick.RemoveAllListeners();
            levelButtons[i].button.onClick.AddListener(() => 
            {
                if (IsLevelUnlocked(levelIndex))
                {
                    LoadLevel(levelIndex);
                }
                else
                {
                    Debug.Log($"Level {levelIndex} is locked!");
                    // Можно добавить звук или анимацию блокировки
                }
            });
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
        if (buttonIndex < 0 || buttonIndex >= levelButtons.Length)
        {
            Debug.LogError($"Invalid button index: {buttonIndex}");
            return;
        }

        if (levelButtons[buttonIndex] == null)
        {
            Debug.LogError($"LevelButton at index {buttonIndex} is null!");
            return;
        }

        if (!IsLevelUnlocked(buttonIndex))
        {
            Debug.LogWarning($"Level {buttonIndex} is locked!");
            return;
        }

        string levelName = levelButtons[buttonIndex].levelName;
        if (string.IsNullOrEmpty(levelName))
        {
            Debug.LogError($"Level name for index {buttonIndex} is not set!");
            return;
        }

        Debug.Log($"Загрузка уровня: {levelName}");
        SceneManager.LoadScene(levelName);
    }

    private void UnlockLevel(string levelName)
    {
        PlayerPrefs.SetInt(levelName + COMPLETED_SUFFIX, 1);
        PlayerPrefs.Save();
    }
 
    [ContextMenu("Reset All Progress")]
    public void ResetAllProgress()
    {
        foreach (var lb in levelButtons)
        {
            if (lb != null)
            {
                PlayerPrefs.DeleteKey(lb.levelName + COMPLETED_SUFFIX);
                PlayerPrefs.DeleteKey(lb.levelName + GRADE_SUFFIX);
            }
        }
        PlayerPrefs.Save();
        UpdateLevelButtons();
    }
}