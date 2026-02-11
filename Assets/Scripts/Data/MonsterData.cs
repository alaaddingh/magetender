using System;
using System.Collections.Generic;

/* utility to structure JSON monster data */
[Serializable]
public class MonstersFile
{
    public List<MonsterData> monsters;
}

[Serializable]
public class MonsterData
{
    public string id;
    public string name;
    public string description;

    public string glassPreference;

    public List<string> starting_dialogue;
    public List<string> satisfied_dialogue;
    public List<string> angry_dialogue;
    public List<string> neutral_dialogue;

    [Serializable]
    public class ScorePair
    {
        public float x;
        public float y;
    }

    public ScorePair starting_score;
    public ScorePair goal_score;

    public float anger_tolerance;
    public float satisfied_tolerance;
}
