using System;
using Cysharp.Threading.Tasks;
using Player;
using Turnbase_System.AI;
using UnityEngine;

namespace Turnbase_System
{
    public class EnemyTurnState:BattleState
    {
        private TurnBaseSystem tbs;
        private IEnemyPolicy _policy = new GreedyEnemyPolicy();
        public EnemyTurnState(TurnBaseSystem tbs) : base(tbs) { }

        /// <summary>Ganti pengambil keputusan enemy (mis. agen AlphaZero) tanpa mengubah alur giliran.</summary>
        public void SetPolicy(IEnemyPolicy policy)
        {
            _policy = policy ?? new GreedyEnemyPolicy();
        }

        public override void OnEnter()
        {

            tbs = TurnBaseSystem;
            StartEnemyTurn().Forget();
        }

        private async UniTask StartEnemyTurn()
        {
            await UniTask.Delay(TimeSpan.FromSeconds(1), ignoreTimeScale: false);
            await UIManagerBattle.ShowAnnouncement("Enemy Turn",0.8f);
            await ExecuteSingleEnemyAction();
        }

        public override void OnUpdate() { }

        public override void OnExit()
        {
            TurnBaseSystem.HidePlayerMove();
            TurnBaseSystem.HidePlayerAttack();

        }

        private async UniTask ExecuteSingleEnemyAction()
        {
            Debug.Log("ENEMIES " + tbs.GetAliveUnitsBySide(UnitSide.Enemy).Count + "PLAYER "+ tbs.GetAliveUnitsBySide(UnitSide.Player).Count);

            // 1. Keputusan (murni data, hanya dari aksi legal)
            var action = _policy.ChooseAction(tbs);
            if (action.IsNone)
            {
                tbs.BattleState.ChangeState(tbs.PlayerTurnState);
                return;
            }

            // 2. Eksekusi aksi
            var enemy = action.Unit;
            tbs.SetActiveUnit(enemy);

            if (action.TryAttack)
            {
                tbs.ShowPlayerMove(enemy);
                await UniTask.Delay(500);
                if (action.HasMove)
                {
                    var worldPos = tbs.CellToWorld(action.MoveCell) + new Vector3(0.5f, 0.5f);
                    tbs.MoveEnemy(enemy, worldPos);
                    await UniTask.Delay(300);
                    enemy = tbs.ActiveUnit; // update referensi setelah move
                }
                tbs.HidePlayerMove();

                // Lakukan attack sekali jika target sudah dalam pola attack
                if (tbs.IsInAttackRange(enemy, action.Target))
                {
                    tbs.ShowPlayerAttack(enemy);
                    await UniTask.Delay(400);
                    tbs.PerformAttack();
                    await UniTask.Delay(500);
                    tbs.HidePlayerAttack();
                }
            }

            // 3. Cek hasil akhir giliran
            Debug.Log("TEST "+ tbs.GetAliveUnitsBySide(UnitSide.Player).Count);
            if(tbs.GetAliveUnitsBySide(UnitSide.Player).Count == 0)
            {
                tbs.SetBattleResult(BattleResult.EnemyWin);
                tbs.BattleState.ChangeState(tbs.GameEndState);
            }else if (tbs.GetAliveUnitsBySide(UnitSide.Enemy).Count == 0)
            {
                tbs.SetBattleResult(BattleResult.PlayerWin);
                tbs.BattleState.ChangeState(tbs.GameEndState);
            }
            else if (tbs.GetAliveUnitsBySide(UnitSide.Player).Count > 0)
            {
                tbs.BattleState.ChangeState(tbs.PlayerTurnState);
            }
        }
    }
}
