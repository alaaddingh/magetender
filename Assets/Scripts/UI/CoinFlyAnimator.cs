using UnityEngine;

public class CoinFlyAnimator : MonoBehaviour
{
	public static CoinFlyAnimator Instance { get; private set; }

	[Header("Target (can be inactive at start)")]
	[SerializeField] private GameObject coinFlyRoot;
	[SerializeField] private Animator animator;
	[SerializeField] private string coinAnimStateName = "CoinAnim_UI";
	[SerializeField] private string idleStateName = "Idle";

	private void Awake()
	{
		if (Instance != null && Instance != this)
		{
			Destroy(gameObject);
			return;
		}

		Instance = this;

		if (coinFlyRoot == null)
			coinFlyRoot = gameObject;

		EnsureAnimator();
	}

	private void OnDestroy()
	{
		if (Instance == this)
			Instance = null;
	}

	public static void NotifyCoinsAdded(int amount)
	{
		if (amount <= 0 || Instance == null)
			return;

		Instance.Play();
	}

	private void Play()
	{
		if (coinFlyRoot == null)
			coinFlyRoot = gameObject;

		if (!coinFlyRoot.activeInHierarchy)
			coinFlyRoot.SetActive(true);

		EnsureAnimator();

		if (animator == null)
			return;

		// Play the coin animation directly (no trigger/transition reliance).
		// Deactivation is handled by a StateMachineBehaviour on the Idle state.
		if (!string.IsNullOrEmpty(coinAnimStateName))
			animator.Play(coinAnimStateName, 0, 0f);
	}

	private void EnsureAnimator()
	{
		if (animator != null)
			return;
		if (coinFlyRoot == null)
			return;
		animator = coinFlyRoot.GetComponent<Animator>();
	}
}
