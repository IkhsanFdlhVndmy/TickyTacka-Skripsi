using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Turnbase_System
{
    public class GameEndState:BattleState
    {
        public GameEndState(TurnBaseSystem turnBaseSystem) : base(turnBaseSystem)
        {
            
        }
        public override void OnEnter()
        {
            GameEnd().Forget();
        }

        private async UniTask GameEnd()
        {
            Debug.Log("GAME END"+ TurnBaseSystem.BattleResult);
            await UniTask.Delay(TimeSpan.FromSeconds(1), ignoreTimeScale: false);
            if (TurnBaseSystem.BattleResult == BattleResult.PlayerWin)
            {
                UIManagerBattle.ShowVictoryPanel();
            }
            else
            {
                UIManagerBattle.ShowLosePanel();
            }
        }

        public override void OnUpdate()
        {
        }

        public override void OnExit()
        {
            UIManagerBattle.HideLosePanel();
            UIManagerBattle.HideVictoryPanel();
        }
    }
}