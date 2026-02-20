using System.Collections.Generic;
using UnityEngine;
using System;

public class CurrentMonster : MonoBehaviour
{
    public static CurrentMonster Instance { get; private set; }
    public event Action<string> OnMonsterChanged;

    [Header("Single source of truth")]
    public new string name = "";

    [Header("Data sources")]
    [SerializeField] private string monstersJsonResourcePath = "Data/Monsters";
    [SerializeField] private string levelsJsonResourcePath = "Data/Levels";
    [SerializeField] private string dialogueJsonResourcePath = "Data/Dialogue";

    private MonstersFile monstersFile;
    private LevelsFile levelsFile;
    private DialogueFile dialogueFile;
    private string lastMonsterName;

    public MonsterData Data => GetMonsterByName(name);
    public LevelEncounterData CurrentEncounter => GetEncounterForDayAndMonster(GetCurrentDay(), Data);

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        LoadData();
        lastMonsterName = name;
    }

    private void Update()
    {
        if (name != lastMonsterName)
        {
            lastMonsterName = name;
            OnMonsterChanged?.Invoke(name);
        }
    }

    public ScorePair GetStartingScore()
    {
        var encounter = CurrentEncounter;
        return encounter != null ? encounter.starting_score : null;
    }

    public ScorePair GetGoalScore()
    {
        var encounter = CurrentEncounter;
        return encounter != null ? encounter.goal_score : null;
    }

    public float GetAngerTolerance()
    {
        var encounter = CurrentEncounter;
        return encounter != null && encounter.tolerances != null ? encounter.tolerances.anger_tolerance : 100f;
    }

    public float GetSatisfiedTolerance()
    {
        var encounter = CurrentEncounter;
        return encounter != null && encounter.tolerances != null ? encounter.tolerances.satisfied_tolerance : 0f;
    }

    public List<string> GetDialogue(string state)
    {
        MonsterData monster = Data;
        LevelEncounterData encounter = CurrentEncounter;
        if (monster == null || encounter == null || dialogueFile == null || dialogueFile.dialogue == null)
            return new List<string>();

        DialogueEntry entry = null;
        foreach (var d in dialogueFile.dialogue)
        {
            if (d.id == monster.dialogueId)
            {
                entry = d;
                break;
            }
        }
        if (entry == null || entry.levels == null) return new List<string>();

        DialogueLevelEntry levelEntry = null;
        foreach (var l in entry.levels)
        {
            if (l.level == encounter.dialogue_key)
            {
                levelEntry = l;
                break;
            }
        }
        if (levelEntry == null) return new List<string>();

        if (state == "satisfied") return levelEntry.satisfied ?? new List<string>();
        if (state == "angry") return levelEntry.angry ?? new List<string>();
        if (state == "neutral") return levelEntry.neutral ?? new List<string>();
        return levelEntry.starting ?? new List<string>();
    }

    public void ResetToFirstMonster()
    {
        if (monstersFile == null || monstersFile.monsters == null || monstersFile.monsters.Count == 0) return;
        SetCurrentMonsterName(monstersFile.monsters[0].name);
    }

    public bool AdvanceToNextMonster()
    {
        if (monstersFile == null || monstersFile.monsters == null || monstersFile.monsters.Count == 0) return false;

        for (int i = 0; i < monstersFile.monsters.Count - 1; i++)
        {
            if (monstersFile.monsters[i].name == name)
            {
                SetCurrentMonsterName(monstersFile.monsters[i + 1].name);
                return true;
            }
        }

        return false;
    }

    private void LoadData()
    {
        monstersFile = JsonUtility.FromJson<MonstersFile>(Resources.Load<TextAsset>(monstersJsonResourcePath).text);
        levelsFile = JsonUtility.FromJson<LevelsFile>(Resources.Load<TextAsset>(levelsJsonResourcePath).text);
        dialogueFile = JsonUtility.FromJson<DialogueFile>(Resources.Load<TextAsset>(dialogueJsonResourcePath).text);

        if (string.IsNullOrWhiteSpace(name) && monstersFile != null && monstersFile.monsters != null && monstersFile.monsters.Count > 0)
            name = monstersFile.monsters[0].name;
    }

    public void SetCurrentMonsterName(string monsterName)
    {
        name = monsterName;
        if (name != lastMonsterName)
        {
            lastMonsterName = name;
            OnMonsterChanged?.Invoke(name);
        }
    }

    private int GetCurrentDay()
    {
        return GameManager.Instance != null ? GameManager.Instance.Day : 1;
    }

    private MonsterData GetMonsterByName(string monsterName)
    {
        if (monstersFile == null || monstersFile.monsters == null) return null;

        foreach (MonsterData monster in monstersFile.monsters)
        {
            if (monster.name == monsterName)
                return monster;
        }

        return monstersFile.monsters.Count > 0 ? monstersFile.monsters[0] : null;
    }

    private LevelEncounterData GetEncounterForDayAndMonster(int day, MonsterData monster)
    {
        if (levelsFile == null || levelsFile.levels == null || levelsFile.levels.Count == 0 || monster == null)
            return null;

        int levelIndex = Mathf.Clamp(day - 1, 0, levelsFile.levels.Count - 1);
        LevelData level = levelsFile.levels[levelIndex];
        if (level == null || level.encounters == null || level.encounters.Count == 0)
            return null;

        foreach (var encounter in level.encounters)
        {
            if (encounter.monster_id == monster.id)
                return encounter;
        }

        return level.encounters[0];
    }
}
