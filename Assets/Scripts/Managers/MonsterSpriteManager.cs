using UnityEngine;
using UnityEngine.UI;

public class MonsterSpriteManager : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private CurrentMonster currentMonsterManager;
    [SerializeField] private MonsterStateManager monsterStateManager;

    [Header("UI image targets")]
    [SerializeField] private Image orderSprite;
    [SerializeField] private Image serveSprite;
    [SerializeField] private string orderSpriteObjectName = "";
    [SerializeField] private string serveSpriteObjectName = "";

    [Header("Idle animation")]
    [SerializeField] private float bobAmplitude = 8f;
    [SerializeField] private float bobFrequency = 0.25f;
    [SerializeField] private float jitterAmplitude = 4f;
    [SerializeField] private float jitterFrequency = 16f;

    private bool hadTargetsLastFrame;
    private Image lastOrderSpriteRef;
    private Image lastServeSpriteRef;
    private Sprite lastOrderSpriteValue;
    private Sprite lastServeSpriteValue;
    private Vector2 baseOrderPosition;
    private Vector2 baseServePosition;

    private void Awake()
    {
        if (currentMonsterManager == null)
            currentMonsterManager = CurrentMonster.Instance;
        TryResolveMissingTargets();
        lastOrderSpriteRef = orderSprite;
        lastServeSpriteRef = serveSprite;
        lastOrderSpriteValue = orderSprite != null ? orderSprite.sprite : null;
        lastServeSpriteValue = serveSprite != null ? serveSprite.sprite : null;
    }

    private void OnEnable()
    {
        if (currentMonsterManager != null)
            currentMonsterManager.OnMonsterChanged += HandleMonsterChanged;
        if (monsterStateManager != null)
            monsterStateManager.OnStateChanged += HandleStateChanged;

        TryResolveMissingTargets();
        RefreshSprite();
    }

    private void OnDisable()
    {
        if (currentMonsterManager != null)
            currentMonsterManager.OnMonsterChanged -= HandleMonsterChanged;
        if (monsterStateManager != null)
            monsterStateManager.OnStateChanged -= HandleStateChanged;
    }

    private void HandleMonsterChanged(string _)
    {
        RefreshSprite();
    }

    private void HandleStateChanged(string _)
    {
        RefreshSprite();
    }

    private void Update()
    {
        bool orderBecameNone = lastOrderSpriteRef != null && orderSprite == null;
        bool serveBecameNone = lastServeSpriteRef != null && serveSprite == null;

        TryResolveMissingTargets();

        bool orderTargetChanged = orderSprite != lastOrderSpriteRef;
        bool serveTargetChanged = serveSprite != lastServeSpriteRef;

        bool orderSpriteCleared = orderSprite != null && lastOrderSpriteValue != null && orderSprite.sprite == null;
        bool serveSpriteCleared = serveSprite != null && lastServeSpriteValue != null && serveSprite.sprite == null;
        bool newTargetHasNoSprite = (orderTargetChanged && orderSprite != null && orderSprite.sprite == null) ||
                                    (serveTargetChanged && serveSprite != null && serveSprite.sprite == null);

        bool hasTargets = orderSprite != null || serveSprite != null;
        if (orderBecameNone || serveBecameNone || orderSpriteCleared || serveSpriteCleared || newTargetHasNoSprite || (hasTargets && !hadTargetsLastFrame))
            RefreshSprite();

        ApplyStateAnimation();

        hadTargetsLastFrame = hasTargets;
        lastOrderSpriteRef = orderSprite;
        lastServeSpriteRef = serveSprite;
        lastOrderSpriteValue = orderSprite != null ? orderSprite.sprite : null;
        lastServeSpriteValue = serveSprite != null ? serveSprite.sprite : null;
    }

    private void RefreshSprite()
    {
        if (orderSprite == null && serveSprite == null) return;
        if (currentMonsterManager == null) return;
        MonsterData monster = currentMonsterManager.Data;
        if (monster == null || monster.sprites == null) return;

        baseOrderPosition = currentMonsterManager.GetOrderSpritePosition();
        baseServePosition = currentMonsterManager.GetServeSpritePosition();

        string state = monsterStateManager != null ? monsterStateManager.MonsterState : "start";
        string path = ResolveSpritePathForState(monster.sprites, state);
        if (string.IsNullOrEmpty(path)) return;

        ApplySprite(path);

        ApplyStateAnimation();
    }

    private string ResolveSpritePathForState(MonsterSprites sprites, string state)
    {
        if (state == "angry")
            return sprites.angry;
        if (state == "satisfied" || state == "happy")
            return sprites.happy;

        return sprites.neutral; // start + neutral both map here
    }

    private void TryResolveMissingTargets()
    {
        if (orderSprite == null && !string.IsNullOrWhiteSpace(orderSpriteObjectName))
        {
            var go = GameObject.Find(orderSpriteObjectName);
            if (go != null)
                orderSprite = go.GetComponent<Image>();
        }

        if (serveSprite == null && !string.IsNullOrWhiteSpace(serveSpriteObjectName))
        {
            var go = GameObject.Find(serveSpriteObjectName);
            if (go != null)
                serveSprite = go.GetComponent<Image>();
        }
    }

    private void ApplyStateAnimation()
    {
        if (orderSprite == null && serveSprite == null) return;

        string state = monsterStateManager != null ? monsterStateManager.MonsterState : "start";
        bool isAngry = state == "angry";

        Vector2 offset = isAngry
            ? AnimationHelper.GetJitterOffset(Time.time, jitterAmplitude, jitterFrequency)
            : AnimationHelper.GetBobOffset(Time.time, bobAmplitude, bobFrequency);

        if (orderSprite != null)
            orderSprite.rectTransform.anchoredPosition = baseOrderPosition + offset;
        if (serveSprite != null)
            serveSprite.rectTransform.anchoredPosition = baseServePosition + offset;
    }

    private void ApplySprite(string path)
    {
        Sprite sprite = Resources.Load<Sprite>(path);
        if (sprite == null)
        {
            Debug.LogWarning($"MonsterSpriteManager: Could not load sprite at Resources path '{path}'.");
            return;
        }

        if (orderSprite != null)
            orderSprite.sprite = sprite;
        if (serveSprite != null)
            serveSprite.sprite = sprite;
    }
}
