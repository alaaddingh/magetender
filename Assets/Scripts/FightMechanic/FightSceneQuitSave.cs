using UnityEngine;
using UnityEngine.SceneManagement;
using Magetender.Data;

/// <summary>
/// Persists bar-fight progress when returning to the title from QTECombatScene (Pause → return to menu).
/// You do not need to add this component to the scene: <see cref="PauseGame"/> calls
/// <see cref="TryCommitMidFightSaveIfInFightScene"/> automatically. Attach this script only if you want
/// a visible marker in the Hierarchy or to call <see cref="TryCommitMidFightSaveIfInFightScene"/> from a custom UI.
/// </summary>
public class FightSceneQuitSave : MonoBehaviour
{
	public const string QteSceneName = "QTECombatScene";

	public static void TryCommitMidFightSaveIfInFightScene()
	{
		if (SceneManager.GetActiveScene().name != QteSceneName)
			return;

		var qte = Object.FindFirstObjectByType<QTECombatManager>();
		if (qte == null)
			return;

		if (qte.IsFightEnded())
			return;

		FightCheckpointState checkpoint = qte.BuildFightCheckpoint();
		SaveSystem.WriteFightQuitSave(checkpoint, pendingBarFight: true);
	}
}
