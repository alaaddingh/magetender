/**
 * Purpose:
 * 1) Loads monster data
 * 2) Controls monster dialogue on "Take Order" screen
 * 3) Makes brew button visible when needed
 */
using TMPro;
using UnityEngine;

public class DialogueController : MonoBehaviour
{
    [Header("UI variables")]
    [SerializeField] private TMP_Text monsterName;
    [SerializeField] private TMP_Text monsterSpeech;

    [Header("Buttons")]
    [SerializeField] private GameObject brewButtonObject; 

    [Header("Data source")]
    [SerializeField] private string monstersJsonResourcePath = "Data/Monsters"; 


    [Header("Ui Panels (to toggle/hide)")] 
    public GameObject orderScreen;
    public GameObject selectingGlassScreen;

    private MonsterData currentMonster;
    private int dialogueIndex = 0;

    void Start()
    {
        if (brewButtonObject != null)
            brewButtonObject.SetActive(false);

        LoadMonster();
        Dialogue();
    }

    private void LoadMonster()
    {
        TextAsset json = Resources.Load<TextAsset>(monstersJsonResourcePath);

        MonstersFile file = JsonUtility.FromJson<MonstersFile>(json.text);
        currentMonster = file.monsters[0]; /* Count Drunkula */
        dialogueIndex = 0;
    }


    /* sets monster name, displays current dialogue index */
    private void Dialogue()
    {
            monsterName.text = currentMonster.name;
            dialogueIndex = Mathf.Clamp(dialogueIndex, 0, currentMonster.dialogue.Count - 1);
            monsterSpeech.text = currentMonster.dialogue[dialogueIndex];
    }

    /* handles Next UI Button clicks (iteration + brew button visibility) */
    public void OnNextPressed()
    {
        Debug.Log("PRESSED");
        if (currentMonster?.dialogue == null || currentMonster.dialogue.Count == 0) return;

        /* if at the last line, show brew button. */
        bool last = dialogueIndex >= currentMonster.dialogue.Count - 1;
        if (last)
        {
            brewButtonObject.SetActive(true);
            return;
        }
        dialogueIndex++;
        Dialogue();

        if (dialogueIndex == currentMonster.dialogue.Count - 1)
        {
            if (brewButtonObject != null)
                brewButtonObject.SetActive(true);
        }
    }

    public void BrewingPressed()
    {
        Debug.Log("'Begin Brewing' pressed");
        orderScreen.SetActive(false);
        selectingGlassScreen.SetActive(true);
    }
}
