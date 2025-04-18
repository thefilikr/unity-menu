using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "LevelDatabase", menuName = "Game/Level Database")]
public class LevelDatabase : ScriptableObject
{
    [System.Serializable]
    public class LevelInfo
    {
        public string LevelName;
        public float timeGrade3 = 60f;
        public float timeGrade2 = 90f;
        public float timeGrade1 = 120f;
    }

    public LevelInfo[] levels;
}
