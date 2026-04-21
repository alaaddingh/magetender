using System;

namespace Magetender.Data
{
    [Serializable]
    public class SaveData
    {
        public int coins;
        public int day;
        public int currentEncounterIndex;
		public string[] unlockedIngredientIds;
        public bool tutorialCompleted;
    }
}

