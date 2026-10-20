namespace Turnbase_System
{
    public abstract class BattleState: IState
    {
        protected TurnBaseSystem TurnBaseSystem;
        protected UIManagerBattle UIManagerBattle;
        protected BattleState(TurnBaseSystem turnBaseSystem)
        {
            TurnBaseSystem = turnBaseSystem;
            UIManagerBattle = turnBaseSystem.UIManagerBattle;
        }
        public abstract void OnEnter();
        public abstract void OnUpdate();
        public abstract void OnExit();
    }
}