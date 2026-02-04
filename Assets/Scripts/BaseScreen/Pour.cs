using UnityEngine;
using UnityEngine.UI;

/* Simple script for liquid drops that fall downward at constant speed */
public class Pour : MonoBehaviour
{
    [Header("Drop Settings")]
    public float fallSpeed = 400f;
    public float destroyY = -600f;

    private RectTransform rectTransform;
    private Image dropImage;
    private BaseController baseController;
    private string baseKey = "";

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        dropImage = GetComponent<Image>();
        baseController = FindFirstObjectByType<BaseController>();
    }

    public void SetBaseInfo(string key)
    {
        baseKey = key;
    }

    private void Update()
    {
        if (rectTransform == null) return;

        if (baseController != null && baseController.CheckDropCollision(rectTransform))
        {
            baseController.CatchDrop(baseKey, dropImage.color);
            Destroy(gameObject);
            return;
        }

        Vector2 currentPos = rectTransform.anchoredPosition;
        currentPos.y -= fallSpeed * Time.deltaTime;
        rectTransform.anchoredPosition = currentPos;

        if (currentPos.y < destroyY)
        {
            Destroy(gameObject);
        }
    }
}
