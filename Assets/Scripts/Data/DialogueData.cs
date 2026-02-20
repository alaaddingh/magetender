using System;
using System.Collections.Generic;

[Serializable]
public class DialogueFile
{
    public List<DialogueEntry> dialogue;
}

[Serializable]
public class DialogueEntry
{
    public string id;
    public List<DialogueLevelEntry> levels;
}

[Serializable]
public class DialogueLevelEntry
{
    public string level;
    public List<string> starting;
    public List<string> neutral;
    public List<string> satisfied;
    public List<string> angry;
}
