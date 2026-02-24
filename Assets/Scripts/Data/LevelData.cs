using System;
using System.Collections.Generic;

[Serializable]
public class LevelsFile
{
    public List<LevelData> levels;
}

[Serializable]
public class LevelData
{
    public string id;
    public string name;
    public List<LevelEncounterData> encounters;
}

[Serializable]
public class LevelEncounterData
{
    public string monster_id;
    public EncounterTolerances tolerances;
    public ScorePair starting_score;
    public ScorePair goal_score;
    public string dialogue_key;
}

[Serializable]
public class EncounterTolerances
{
    public float anger_tolerance;
    public float satisfied_tolerance;
}

[Serializable]
public class ScorePair
{
    public float x;
    public float y;
}
