using System;
using System.Collections.Generic;

/* utility to structure JSON monster data (called in DialogueController.cs) */
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
    public List<string> dialogue;

    /* for laterrr */
    public List<string> liked_ingredients;
}
