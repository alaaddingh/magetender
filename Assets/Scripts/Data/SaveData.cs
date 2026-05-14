using System;

namespace Magetender.Data
{
	[Serializable]
	public class FightCheckpointState
	{
		public bool hasData;
		public int encounterIndexWhenSaved;
		public int playerHealth;
		public int customerHealth;
		public float playerHealthFloat;
		public bool defendBankActive;
		public int defendProgress;
		public int attackProgress;
		public float defendTimer;
		public float attackTimer;
		public bool combatStarted;
		public bool tutorialMode;
		public bool combatPaused;
		public bool defendBankVisible;
		public bool attackBankVisible;
		public int[] defendSequenceKeyCodes;
		public int[] attackSequenceKeyCodes;
	}

    [Serializable]
    public class SaveData
    {
        public int coins;
        public int day;
        public int currentEncounterIndex;
		public string[] unlockedIngredientIds;
        public bool tutorialCompleted;
		public bool pendingBarFight;
		public bool skipDayPanelNextMixLoad;
		public int pendingBarFightEncounterIndex;
		public bool resumeLoseScreenOnContinue;
		public FightCheckpointState fightCheckpoint;
		public string playthroughId;
    }
}

