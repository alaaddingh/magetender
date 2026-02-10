/* This file:
1) Assesses the monster's mood upon serving (by communicating w/ ScoreManager)
2) Sets the monster's state (neutral/satisfied/angry) in MonsterStateManager
3) makes fight button visible if you rly messed up
*/

using UnityEngine;

public class AssessController : MonoBehaviour
{
    [Header("refs")]
    [SerializeField] private ScoreManager scoreManager;
    [SerializeField] private MonsterStateManager MonsterStateManager;

    [Header("monster data (MVP)")]
    [SerializeField] private string monstersJsonResourcePath = "Data/Monsters";
    private MonstersFile monstersFile;
    private MonsterData currentMonster;   /* monster[0] AGAIN TEMPORARY SOLUTION SRY*/

    [Header("graph bounds")]
    [SerializeField] private float graphmin = -1f;
    [SerializeField] private float graphmax = 1f;

 

    public float MixAccuracy; /* just so we can see the accuracy as a perctenage */

    public GameObject FightButton;

    private void Awake()
    {
        LoadMonsters();
        currentMonster = monstersFile.monsters[0]; /* temp */

        MixAccuracy = AssessAccuracy();
        AssessState(MixAccuracy);

    }

    private void LoadMonsters()
    {
        TextAsset json = Resources.Load<TextAsset>(monstersJsonResourcePath);
        monstersFile = JsonUtility.FromJson<MonstersFile>(json.text);
    }

    /* assesses percentage difference of the final mix's X and Y,
     returns accuracy in form of a whole number*/
   private float AssessAccuracy()
        {
            /* live values (no stale data) */
            float finalX = scoreManager.CurrMoodBoardX;
            float finalY = scoreManager.CurrMoodBoardY;
            float targetX = currentMonster.goal_score.x;
            float targetY = currentMonster.goal_score.y;

            float diffx = finalX - targetX;
            float diffy = finalY - targetY;
            float distance = Mathf.Sqrt(diffx * diffx + diffy * diffy);


            float graphsize = 2f;   /* since graph goes from [-1, 1] => 1 -(-)1 = 2 */
            float maxdistance = Mathf.Sqrt(2f * graphsize * graphsize);

            /* convert to accuracy (as whole number percentage) */
            float accuracy = 1f - (distance / maxdistance);
            float accuracypercent = accuracy * 100f;

            return Mathf.Clamp(accuracypercent, 0f, 100f);
        }



    private void AssessState(float Accuracy)
        {
            float AngerTolerance = currentMonster.anger_tolerance;
            float SatisfiedTolerance = currentMonster.satisfied_tolerance;

            float error = 100f - Accuracy;

            if (error >= AngerTolerance)
            {
                /* accuracy is too low for monster */
                FightButton.SetActive(true);
                MonsterStateManager.SetState("angry");
            }
            else if (error <= SatisfiedTolerance)
            {
                /* accuracy is exceptional */
                MonsterStateManager.SetState("satisfied");
            }
            else
            {
                /* not bad or good */
                MonsterStateManager.SetState("neutral");
            }
        }
}

