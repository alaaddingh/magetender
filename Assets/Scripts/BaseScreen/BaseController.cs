using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;


/* purpose of file: 
    1) gets chosen glass from mixmanager then displays
    2) when user clicks on base, applies a tint to the glass
    3) write selected base to mixmanager
    4) sets "next" button to visible when neccesary
    */
public class BaseController : MonoBehaviour
{
    [Header("Target bottle shown on Base screen")]
    public Image BaseBottle;

    [Header("Source bottle UI images (existing in scene)")]
    public Image smallBottleUI;
    public Image mediumBottleUI;
    public Image largeBottleUI;

    [Header("Base colors")]
    public Color BloodColor = new Color(0.7f, 0.1f, 0.1f, 1f); // red
    public Color HolyWaterColor = new Color(0.7f, 0.9f, 1f, 1f); // blue

    [Header("Manager")]
    [SerializeField] private MixManager mixManager;

    public GameObject nextButton;
    public GameObject CurrentScreen;
    public GameObject NextScreen;

    private void Awake()
    {
        if (mixManager == null)
            mixManager = FindFirstObjectByType<MixManager>();
    }

    private void OnEnable()
    {
        ApplySelectedBottleToBaseBottle();
        ApplySavedBaseTint();

        nextButton.SetActive(false);
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Image hoveredBase = GetHoveredBase();
            if (hoveredBase != null)
            {
                string key = hoveredBase.gameObject.name.ToLower();

                if (key.Contains("blood"))
                {
                    SetBase("blood", BloodColor);
                }
                else if (key.Contains("holy"))
                {
                    SetBase("holywater", HolyWaterColor);
                }
            }
        }
    }

    private void ApplySelectedBottleToBaseBottle()
    {
        if (mixManager == null || BaseBottle == null) return;

        switch (mixManager.SelectedBottle)
        {
            case "small":
                UIImgUtil.CopyAppearance(smallBottleUI, BaseBottle);
                break;
            case "medium":
                UIImgUtil.CopyAppearance(mediumBottleUI, BaseBottle);
                break;
            case "large":
                UIImgUtil.CopyAppearance(largeBottleUI, BaseBottle);
                break;
            default:
                break;
        }
    }

    private void SetBase(string baseKey, Color tint)
    {
        mixManager.SetBase(baseKey);
        BaseBottle.color = tint;

        nextButton.SetActive(true);

    }

    private void ApplySavedBaseTint()
    {
        if (mixManager == null || BaseBottle == null) return;

        switch (mixManager.SelectedBase)
        {
            case "blood":
                BaseBottle.color = BloodColor;
                break;
            case "holywater":
                BaseBottle.color = HolyWaterColor;
                break;
        }
    }

    public void NextPressed()
    {

        if (CurrentScreen != null) CurrentScreen.SetActive(false);
        if (NextScreen != null) NextScreen.SetActive(true);
    }

    private Image GetHoveredBase()
    {
        if (EventSystem.current == null) return null;

        var pointer = new PointerEventData(EventSystem.current)
        {
            position = Input.mousePosition
        };

        var results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointer, results);

        foreach (var r in results)
        {
            if (!r.gameObject.CompareTag("Base")) continue;

            var img = r.gameObject.GetComponent<Image>();
            if (img != null) return img;
        }

        return null;
    }
}
