using UnityEngine;

public class ShopScreenNavigation : MonoBehaviour
{
	[Header("Panels")]
	[SerializeField] private GameObject shopScreen;
	[SerializeField] private GameObject returnToScreen;

	[Header("Optional UI to hide while shopping")]
	[SerializeField] private GameObject moodPanel;

	[Header("Coin UI")]
	[SerializeField] private GameObject coinCanvas;

	private bool coinCanvasCaptured;
	private bool coinCanvasPrevActive;

	public void OpenShop()
	{
		if (AudioManager.Instance != null)
			AudioManager.Instance.PlayButtonClick();

		if (returnToScreen != null)
			returnToScreen.SetActive(false);
		if (moodPanel != null)
			moodPanel.SetActive(false);
		if (coinCanvas != null)
			coinCanvas.SetActive(true);
		if (shopScreen != null)
			shopScreen.SetActive(true);

		if (shopScreen != null)
		{
			var shopController = shopScreen.GetComponentInChildren<ShopPanelController>(includeInactive: true);
			if (shopController != null)
				shopController.ForceRefreshUI();
		}
	}

	public void CloseShop()
	{
		if (AudioManager.Instance != null)
			AudioManager.Instance.PlayButtonClick();

		if (shopScreen != null)
			shopScreen.SetActive(false);

		if (coinCanvas != null)
			coinCanvas.SetActive(false);

		if (moodPanel != null)
			moodPanel.SetActive(true);
		if (returnToScreen != null)
			returnToScreen.SetActive(true);
	}
}

