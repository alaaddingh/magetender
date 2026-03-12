using UnityEngine;

public class UIButtonClickSfx : MonoBehaviour
{
	public void PlayClick()
	{
		if (AudioManager.Instance != null)
			AudioManager.Instance.PlayButtonClick();
	}
}
