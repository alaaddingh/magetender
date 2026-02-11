
using UnityEngine;

/* State machine for monster.
    monsters will have: neutral/satisfied/angry/start as options*/
public class MonsterStateManager : MonoBehaviour
{
    public string MonsterState = "start";  /* starting state */

    public void SetState(string State)
        {
           MonsterState = State;
            
        }
}