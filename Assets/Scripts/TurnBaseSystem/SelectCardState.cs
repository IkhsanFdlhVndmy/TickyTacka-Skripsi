using Cysharp.Threading.Tasks;

namespace Turnbase_System
{
    public class SelectCardState:BattleState
    {
        public SelectCardState(TurnBaseSystem turnBaseSystem) : base(turnBaseSystem)
        {
            
        }
        public override void OnEnter()
        {
            UIManagerBattle.ShowPlayerCards();
        }

        public override void OnUpdate()
        {
            throw new System.NotImplementedException();
        }

        public override void OnExit()
        {
            UIManagerBattle.HidePlayerCards();
        }
    }
}