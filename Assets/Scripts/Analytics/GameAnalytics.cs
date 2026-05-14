using System;
using System.Collections;
using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Analytics;

public static class GameAnalytics
{
	public const string DialogueFightSurfaceTakeOrder = "take_order_screen";
	public const string DialogueFightSurfacePostServeAssessment = "post_serve_assessment_screen";

	private static bool s_initStarted;
	private static bool s_canRecord;
	private static string s_pendingNewGameRunId;

	public static void InitializeIfNeeded()
	{
		if (s_initStarted)
			return;

		s_initStarted = true;
		var go = new GameObject("MagetenderAnalyticsInit");
		UnityEngine.Object.DontDestroyOnLoad(go);
		go.hideFlags = HideFlags.HideAndDontSave;
		go.AddComponent<MagetenderAnalyticsInitRunner>();
	}

	public static void RecordPlaythroughStarted()
	{
		var gm = GameManager.Instance;
		if (gm == null)
			return;

		if (s_canRecord)
		{
			var e = new CustomEvent("new_game_run_started");
			e.Add("game_run_id", gm.PlaythroughId);
			TryRecord(e);
			return;
		}

		s_pendingNewGameRunId = gm.PlaythroughId;
	}

	public static void RecordServeOutcome(CurrentMonster cm, string outcome, float mixAccuracyPercent)
	{
		if (cm == null || cm.Data == null)
			return;

		var gm = GameManager.Instance;
		if (gm == null)
			return;

		var e = new CustomEvent("customer_serve_mood_result");
		e.Add("game_run_id", gm.PlaythroughId);
		e.Add("game_calendar_day", gm.Day);
		e.Add("content_level_id", cm.GetCurrentLevelId() ?? string.Empty);
		e.Add("customer_monster_id", cm.Data.id ?? string.Empty);
		e.Add("customer_visit_index", cm.currentEncounterIndex);
		e.Add("customer_mood_outcome", outcome ?? string.Empty);
		e.Add("drink_mix_accuracy_percent", Mathf.Round(mixAccuracyPercent * 10f) / 10f);
		TryRecord(e);
	}

	public static void RecordShopOpened()
	{
		var gm = GameManager.Instance;
		if (gm == null)
			return;

		var e = new CustomEvent("shop_screen_opened");
		e.Add("game_run_id", gm.PlaythroughId);
		e.Add("game_calendar_day", gm.Day);
		TryRecord(e);
	}

	public static void RecordShopPurchase(string ingredientId)
	{
		var gm = GameManager.Instance;
		if (gm == null)
			return;

		var e = new CustomEvent("shop_ingredient_unlock_purchase");
		e.Add("game_run_id", gm.PlaythroughId);
		e.Add("game_calendar_day", gm.Day);
		e.Add("ingredient_unlock_id", ingredientId ?? string.Empty);
		TryRecord(e);
	}

	public static void RecordFightCompleted(string monsterId, int day, bool playerWon, bool tutorialMode)
	{
		var gm = GameManager.Instance;
		bool customerIsAlien = !string.IsNullOrEmpty(monsterId) &&
			string.Equals(monsterId, "alien", StringComparison.OrdinalIgnoreCase);
		var e = new CustomEvent("qte_fight_outcome");
		e.Add("game_run_id", gm != null ? gm.PlaythroughId : string.Empty);
		e.Add("game_calendar_day", day);
		e.Add("customer_monster_id", monsterId ?? string.Empty);
		e.Add("player_won_qte_fight", playerWon);
		e.Add("qte_was_tutorial_mode", tutorialMode);
		e.Add("customer_is_alien", customerIsAlien);
		TryRecord(e);
	}

	public static void RecordAssessmentFight(CurrentMonster cm, GameManager gm)
	{
		if (cm == null || cm.Data == null || gm == null)
			return;

		var e = new CustomEvent("angry_serve_fight_queued_from_menu");
		e.Add("game_run_id", gm.PlaythroughId);
		e.Add("game_calendar_day", gm.Day);
		e.Add("customer_monster_id", cm.Data.id ?? string.Empty);
		e.Add("customer_visit_index", cm.currentEncounterIndex);
		e.Add("customer_is_alien", string.Equals(cm.Data.id, "alien", StringComparison.OrdinalIgnoreCase));
		TryRecord(e);
	}

	public static void RecordDialogueFightButtonBecameAvailable(string dialogueFightButtonLocation)
	{
		var gm = GameManager.Instance;
		var cm = CurrentMonster.Instance;
		if (gm == null || cm == null || cm.Data == null)
			return;

		if (string.IsNullOrEmpty(dialogueFightButtonLocation))
			dialogueFightButtonLocation = "unknown";

		bool customerIsAlien = string.Equals(cm.Data.id, "alien", StringComparison.OrdinalIgnoreCase);
		var e = new CustomEvent("dialogue_fight_button_became_available");
		e.Add("game_run_id", gm.PlaythroughId);
		e.Add("game_calendar_day", gm.Day);
		e.Add("customer_monster_id", cm.Data.id ?? string.Empty);
		e.Add("customer_visit_index", cm.currentEncounterIndex);
		e.Add("customer_is_alien", customerIsAlien);
		e.Add("dialogue_fight_button_location", dialogueFightButtonLocation);
		TryRecord(e);
	}

	public static void TryRecordAlienIdRefusedFromInkChoice(string normalizedChoiceText)
	{
		if (string.IsNullOrEmpty(normalizedChoiceText))
			return;

		var cm = CurrentMonster.Instance;
		if (cm == null || cm.Data == null)
			return;

		if (!string.Equals(cm.Data.id, "alien", StringComparison.OrdinalIgnoreCase))
			return;

		string refusalBranchId = null;
		if (normalizedChoiceText.Contains("look legit") &&
			(normalizedChoiceText.Contains("doesnt") || normalizedChoiceText.Contains("dont")))
			refusalBranchId = "alien_license_rejected_not_legit_day1";
		else if (normalizedChoiceText.Contains("nice fake"))
			refusalBranchId = "alien_license_rejected_nice_fake_day2";

		if (refusalBranchId == null)
			return;

		var gm = GameManager.Instance;
		if (gm == null)
			return;

		var e = new CustomEvent("alien_age_license_refused_in_dialogue");
		e.Add("game_run_id", gm.PlaythroughId);
		e.Add("game_calendar_day", gm.Day);
		e.Add("alien_refusal_dialogue_branch_id", refusalBranchId);
		TryRecord(e);
	}

	private static void TryRecord(CustomEvent customEvent)
	{
		if (!s_canRecord || customEvent == null)
			return;

		try
		{
			AnalyticsService.Instance.RecordEvent(customEvent);
		}
		catch (Exception ex)
		{
			Debug.LogWarning($"[GameAnalytics] RecordEvent failed: {ex.Message}");
		}
	}

	private sealed class MagetenderAnalyticsInitRunner : MonoBehaviour
	{
		private void Start()
		{
			StartCoroutine(CoInit());
		}

		private IEnumerator CoInit()
		{
			var initTask = UnityServices.InitializeAsync();
			while (!initTask.IsCompleted)
				yield return null;

			if (initTask.Exception != null)
			{
				Debug.LogWarning($"[GameAnalytics] UnityServices init failed: {initTask.Exception.InnerException?.Message ?? initTask.Exception.Message}");
				yield break;
			}

			try
			{
				AnalyticsService.Instance.StartDataCollection();
				s_canRecord = true;
				FlushPendingNewGameRunStarted();
			}
			catch (Exception ex)
			{
				Debug.LogWarning($"[GameAnalytics] StartDataCollection failed: {ex.Message}");
			}
		}
	}

	private static void FlushPendingNewGameRunStarted()
	{
		if (string.IsNullOrEmpty(s_pendingNewGameRunId))
			return;

		var e = new CustomEvent("new_game_run_started");
		e.Add("game_run_id", s_pendingNewGameRunId);
		s_pendingNewGameRunId = null;
		try
		{
			AnalyticsService.Instance.RecordEvent(e);
		}
		catch (Exception ex)
		{
			Debug.LogWarning($"[GameAnalytics] RecordEvent failed: {ex.Message}");
		}
	}
}
