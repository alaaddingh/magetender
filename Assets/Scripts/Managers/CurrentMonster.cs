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
    private bool dataLoaded;
    private int currentEncounterIndex;
    private int lastSeenDay;

    public MonsterData Data => GetCurrentMonsterData();
    public LevelEncounterData CurrentEncounter => GetCurrentEncounter();

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
        return encounter != null ? encounter.tolerances.anger_tolerance : 100f;
    }

    public float GetSatisfiedTolerance()
    {
        var encounter = CurrentEncounter;
        return encounter != null ? encounter.tolerances.satisfied_tolerance : 0f;
    }

    public Vector2 GetOrderSpritePosition()
    {
        return Data != null && Data.position != null
            ? new Vector2(Data.position.pos_x, Data.position.pos_y)
            : Vector2.zero;
    }

    public Vector2 GetServeSpritePosition()
    {
        return Data != null && Data.position != null
            ? new Vector2(Data.position.pos_x, Data.position.pos_y)
            : Vector2.zero;
    }

    public List<string> GetDialogue(string state)
    {
        MonsterData monster = Data;
        LevelEncounterData encounter = CurrentEncounter;

        DialogueEntry entry = null;
        foreach (var d in dialogueFile.dialogue)
        {
            if (d.id == monster.dialogueId)
            {
                entry = d;
                break;
            }
        }
        if (entry == null) return new List<string>();

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
        SyncEncounterIndexToDay();
        currentEncounterIndex = 0;
        var encounter = GetCurrentEncounter();
        if (encounter != null)
            SetCurrentMonsterName(GetMonsterById(encounter.monster_id).name);
    }

    public bool AdvanceToNextMonster()
    {
        SyncEncounterIndexToDay();
        var level = GetLevelForDay(GetCurrentDay());
        if (level == null || level.encounters == null || level.encounters.Count == 0) return false;
        if (currentEncounterIndex >= level.encounters.Count - 1) return false;

        currentEncounterIndex++;
        var encounter = GetCurrentEncounter();
        if (encounter == null) return false;

        SetCurrentMonsterName(GetMonsterById(encounter.monster_id).name);
        return true;
    }

    public bool HasNextMonsterInCurrentLevel()
    {
        SyncEncounterIndexToDay();
        var level = GetLevelForDay(GetCurrentDay());
        if (level == null || level.encounters == null || level.encounters.Count == 0) return false;
        return currentEncounterIndex < level.encounters.Count - 1;
    }

    private void LoadData()
    {
        monstersFile = JsonUtility.FromJson<MonstersFile>(Resources.Load<TextAsset>(monstersJsonResourcePath).text);
        levelsFile = JsonUtility.FromJson<LevelsFile>(Resources.Load<TextAsset>(levelsJsonResourcePath).text);
        dialogueFile = JsonUtility.FromJson<DialogueFile>(Resources.Load<TextAsset>(dialogueJsonResourcePath).text);
        dataLoaded = true;
        currentEncounterIndex = 0;
        lastSeenDay = GetCurrentDay();

        if (!string.IsNullOrWhiteSpace(name)) return;

        var encounter = GetCurrentEncounter();
        if (encounter != null)
            name = GetMonsterById(encounter.monster_id).name;
        else if (monstersFile != null && monstersFile.monsters != null && monstersFile.monsters.Count > 0)
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

    private MonsterData GetCurrentMonsterData()
    {
        EnsureDataLoaded();
        var encounter = GetCurrentEncounter();
        if (encounter != null && !string.IsNullOrWhiteSpace(encounter.monster_id))
        {
            var byId = GetMonsterById(encounter.monster_id);
            if (byId != null) return byId;
        }

        return GetMonsterByName(name);
    }

    private MonsterData GetMonsterByName(string monsterName)
    {
        EnsureDataLoaded();
        if (monstersFile == null || monstersFile.monsters == null || monstersFile.monsters.Count == 0)
            return null;

        foreach (MonsterData monster in monstersFile.monsters)
        {
            if (monster.name == monsterName)
                return monster;
        }

        return monstersFile.monsters[0];
    }

    private MonsterData GetMonsterById(string monsterId)
    {
        EnsureDataLoaded();
        if (monstersFile == null || monstersFile.monsters == null || monstersFile.monsters.Count == 0)
            return null;

        foreach (MonsterData monster in monstersFile.monsters)
        {
            if (monster.id == monsterId)
                return monster;
        }

        return null;
    }

    private LevelData GetLevelForDay(int day)
    {
        EnsureDataLoaded();
        if (levelsFile == null || levelsFile.levels == null) return null;

        int levelIndex = Mathf.Clamp(day - 1, 0, levelsFile.levels.Count - 1);
        return levelsFile.levels[levelIndex];
    }

    private LevelEncounterData GetEncounterForDayAndMonster(int day, MonsterData monster)
    {
        EnsureDataLoaded();
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

    private LevelEncounterData GetCurrentEncounter()
    {
        EnsureDataLoaded();
        SyncEncounterIndexToDay();
        var level = GetLevelForDay(GetCurrentDay());
        if (level == null || level.encounters == null || level.encounters.Count == 0) return null;

        currentEncounterIndex = Mathf.Clamp(currentEncounterIndex, 0, level.encounters.Count - 1);
        return level.encounters[currentEncounterIndex];
    }

    private void EnsureDataLoaded()
    {
        if (!dataLoaded)
            LoadData();
    }

    private void SyncEncounterIndexToDay()
    {
        int day = GetCurrentDay();
        if (day != lastSeenDay)
        {
            currentEncounterIndex = 0;
            lastSeenDay = day;
        }
    }
}
