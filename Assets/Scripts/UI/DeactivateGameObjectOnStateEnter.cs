using UnityEngine;

public class DeactivateGameObjectOnStateEnter : StateMachineBehaviour
{
	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		if (animator == null)
			return;

		// Disable the animated object when we return to Idle.
		animator.gameObject.SetActive(false);
		CoinFlyAnimator.RestoreCoinStaticImageAfterFly();
	}
}

