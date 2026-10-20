using Input;
using UnityEngine;

namespace Turnbase_System
{
    public class RecruitCardsState:BattleState
    {
        private Vector2 _mousePos;
        public RecruitCardsState(TurnBaseSystem turnBaseSystem) : base(turnBaseSystem)
        {
            
        }
        public override void OnEnter()
        {
            Debug.Log("RECRUIT PLAYER");
            
        }

        public override void OnUpdate()
        {

        }

        public override void OnExit()
        {
        }
    }
}