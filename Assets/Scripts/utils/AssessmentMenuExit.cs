using UnityEngine;
using Magetender.Data;

public static class AssessmentMenuExit
{
	public static void TryCommitSaveBeforeReturningToTitle()
	{
		var assessCtrl = Object.FindObjectOfType<AssessController>();
		if (assessCtrl == null || !assessCtrl.IsAssessmentCommitEligible())
			return;

		var msm = assessCtrl.MonsterStateForAssessment;
		if (msm == null)
			return;

		string mood = msm.MonsterState;
		var cm = CurrentMonster.Instance;
		var gm = GameManager.Instance;
		if (cm == null || gm == null)
			return;

		if (mood == "angry")
		{
			gm.SetPendingBarFight(true, cm.currentEncounterIndex);
			gm.SetSkipDayPanelNextMixLoad(false);
			GameAnalytics.RecordAssessmentFight(cm, gm);
			SaveSystem.WriteData();
			return;
		}

		if (mood != "satisfied" && mood != "neutral")
			return;

		bool hasNext = cm.HasNextMonsterInCurrentLevel();

		if (!hasNext && LoseManager.WouldLoseIfEndOfDayAdvanceNow())
		{
			SaveSystem.WriteLoseState();
			return;
		}

		gm.SetPendingBarFight(false, 0);
		gm.SetSkipDayPanelNextMixLoad(hasNext);

		SaveData data = BuildSaveAfterNeutralAssessmentExit(gm, cm, hasNext);
		SaveSystem.SaveGame(data);
	}

	private static SaveData BuildSaveAfterNeutralAssessmentExit(GameManager gm, CurrentMonster cm, bool hasNext)
	{
		int coins = gm.Coins;
		int day = gm.Day;
		int encounterIndex = cm.currentEncounterIndex;
		bool tutorialCompleted = gm.TutorialCompleted;
		string[] unlocked = gm.GetUnlockedIngredientIds();

		if (hasNext)
		{
			return new SaveData
			{
				coins = coins,
				day = day,
				currentEncounterIndex = encounterIndex + 1,
				unlockedIngredientIds = unlocked,
				tutorialCompleted = tutorialCompleted,
				pendingBarFight = false,
				skipDayPanelNextMixLoad = true,
				pendingBarFightEncounterIndex = 0,
				resumeLoseScreenOnContinue = false,
				fightCheckpoint = new FightCheckpointState { hasData = false },
				playthroughId = gm.PlaythroughId
			};
		}

		string levelId = cm.GetCurrentLevelId();
		if (levelId == "tutorial")
		{
			gm.WriteTutorialExitMaintenancePlayerPrefs(coins);
			return new SaveData
			{
				coins = 0,
				day = day,
				currentEncounterIndex = 0,
				unlockedIngredientIds = unlocked,
				tutorialCompleted = true,
				pendingBarFight = false,
				skipDayPanelNextMixLoad = false,
				pendingBarFightEncounterIndex = 0,
				resumeLoseScreenOnContinue = false,
				fightCheckpoint = new FightCheckpointState { hasData = false },
				playthroughId = gm.PlaythroughId
			};
		}

		int maintenance = cm.GetMaintenanceCostForDay(day + 1);
		day++;
		if (day > 1 && maintenance > 0)
			coins -= maintenance;
		coins = Mathf.Max(0, coins);

		return new SaveData
		{
			coins = coins,
			day = day,
			currentEncounterIndex = 0,
			unlockedIngredientIds = unlocked,
			tutorialCompleted = tutorialCompleted,
			pendingBarFight = false,
			skipDayPanelNextMixLoad = false,
			pendingBarFightEncounterIndex = 0,
			resumeLoseScreenOnContinue = false,
			fightCheckpoint = new FightCheckpointState { hasData = false },
			playthroughId = gm.PlaythroughId
		};
	}
}
