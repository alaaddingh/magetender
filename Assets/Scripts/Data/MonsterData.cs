using System;
using System.Collections.Generic;

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
    public string glassPreference;
    public MonsterSprites sprites;
    public MonsterPosition position;
    public string dialogueId;
}

[Serializable]
public class MonsterSprites
{
    public string neutral;
    public string happy;
    public string angry;
}

[Serializable]
public class MonsterPosition
{
    public float pos_x;
    public float pos_y;
}
