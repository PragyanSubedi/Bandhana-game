using System.Collections;
using UnityEngine;

namespace Bandhana.Battle
{
    public enum BattleState
    {
        Start, ActionSelect, MoveSelect, ResolveTurn, Animate, CheckWithdraw, NextTurn, End
    }

    // TODO M3: full state machine driving turn flow
    public class BattleStateMachine : MonoBehaviour
    {
        public BattleState state { get; private set; } = BattleState.Start;

        public BattleUnit playerUnit;
        public BattleUnit enemyUnit;

        IEnumerator Start()
        {
            state = BattleState.Start;
            yield return null;
            state = BattleState.ActionSelect;
        }

        // TODO M3: ActionSelect -> MoveSelect -> ResolveTurn -> Animate -> CheckWithdraw -> NextTurn -> End
    }
}
