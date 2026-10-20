using Cysharp.Threading.Tasks;
using Input;
using UnityEngine;

namespace Turnbase_System
{
    public class PlayerTurnState:BattleState
    {
        private Vector2 _mousePos;
        public PlayerTurnState(TurnBaseSystem turnBaseSystem) : base(turnBaseSystem)
        {
            
        }
        public override void OnEnter()
        {
            UIManagerBattle.ShowAnnouncement("PLAYER TURN", 0.8f).Forget();
        }

        public override void OnUpdate()
        {

        }

        public override void OnExit()
        {
        }
    }
}