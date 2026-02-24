using System;
using UnityEngine;

/* State machine for monster.
    monsters will have: neutral/satisfied/angry/start as options*/
public class MonsterStateManager : MonoBehaviour
{
    public string MonsterState = "start";  /* starting state */
    public event Action<string> OnStateChanged;

    private string lastState;

    private void Awake()
    {
        lastState = MonsterState;
    }

    private void Update()
    {
        if (MonsterState != lastState)
        {
            lastState = MonsterState;
            OnStateChanged?.Invoke(MonsterState);
        }
    }

    public void SetState(string State)
    {
        MonsterState = State;
        if (MonsterState != lastState)
        {
            lastState = MonsterState;
            OnStateChanged?.Invoke(MonsterState);
        }
    }
}
