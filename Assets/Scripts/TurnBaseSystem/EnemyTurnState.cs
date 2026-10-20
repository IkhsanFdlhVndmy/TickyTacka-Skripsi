using System;
using System.Linq;
using Cysharp.Threading.Tasks;
using Player;
using TilemapLayer;
using UnityEngine;

namespace Turnbase_System
{
    public class EnemyTurnState:BattleState
    {
        private TurnBaseSystem tbs;
        public EnemyTurnState(TurnBaseSystem tbs) : base(tbs) { }

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

        private async UniTaskVoid ExecuteSingleEnemyAction3()
        {
            var enemies = tbs.GetAliveUnitsBySide(UnitSide.Enemy);
            var players = tbs.GetAliveUnitsBySide(UnitSide.Player);

            if (enemies.Count == 0 || players.Count == 0)
            {
                tbs.BattleState.ChangeState(tbs.PlayerTurnState);
                return;
            }

            // Pilih enemy terdekat ke salah satu player
            UnitModel enemy = null, target = null;
            int bestDist = int.MaxValue;
            foreach (var e in enemies)
                foreach (var p in players)
                {
                    if (e.IsDead || p.IsDead) continue;
                    int d = Mathf.Abs(p.Coordinates.x - e.Coordinates.x)
                          + Mathf.Abs(p.Coordinates.y - e.Coordinates.y);
                    if (d < bestDist)
                    {
                        bestDist = d;
                        enemy = e;
                        target = p;
                    }
                }

            if (enemy == null || target == null)
            {
                tbs.BattleState.ChangeState(tbs.PlayerTurnState);
                return;
            }

            tbs.SetActiveUnit(enemy);
            bool inRange = tbs.IsInAttackRange(enemy, target);

            if (!inRange)
            {
                tbs.ShowPlayerMove(enemy);
                await UniTask.Delay(500);

                // Hitung sel langkah
                var stepCell = GetStepTowards(enemy.Coordinates, target.Coordinates);
                var worldPos = tbs.CellToWorld(stepCell) + new Vector3(0.5f, 0.5f);

                // Cek apakah tile kosong (tidak ada unit)
                if (tbs.GetUnit(worldPos) == null)
                {
                    tbs.MoveEnemy(enemy, worldPos);
                    await UniTask.Delay(600);
                    // update reference setelah move
                    enemy = tbs.ActiveUnit;
                }
                // hilangkan highlight move
                tbs.HidePlayerMove();
            }

            // cek lagi apakah dalam range setelah kemungkinan move
            if (tbs.IsInAttackRange(enemy, target))
            {
                tbs.ShowPlayerAttack(enemy);
                await UniTask.Delay(300);
                tbs.PerformAttack();
                await UniTask.Delay(400);
                tbs.HidePlayerAttack();
            }

            tbs.BattleState.ChangeState(tbs.PlayerTurnState);

        }
        private async UniTaskVoid ExecuteSingleEnemyAction2()
        {
            var tbs     = TurnBaseSystem.Instance;
            var enemies = tbs.GetAliveUnitsBySide(UnitSide.Enemy);
            var players = tbs.GetAliveUnitsBySide(UnitSide.Player);

            UnitModel enemy = null, target = null;
            int bestDist = int.MaxValue;
            foreach (var e in enemies)
            foreach (var p in players)
            {
                if (e.IsDead || p.IsDead) continue;
                int d = Mathf.Abs(p.Coordinates.x - e.Coordinates.x)
                        + Mathf.Abs(p.Coordinates.y - e.Coordinates.y);
                if (d < bestDist)
                {
                    bestDist = d;
                    enemy = e;
                    target = p;
                }
            }


            tbs.SetActiveUnit(enemy);

            // Hitung offsets attack
            var offsets = UnitAttackCalculate.GetOffsets(
                enemy.UnitData.AttackPattern,
                enemy.UnitData.Range,
                enemy.UnitData.Direction
            );

            // Cek apakah bisa langsung menyerang
            bool canAttack = offsets.Any(off => enemy.Coordinates + off == target.Coordinates);

            // Jika belum bisa attack, pilih tile terdekat DI ANTARA semua move‐tiles
            if (!canAttack)
            {
                // Tampilkan highlight (bukan wajib, tapi berguna untuk debug)
                tbs.ShowPlayerMove(enemy);
                await UniTask.Delay(100);

                // Ambil semua posisi world yang boleh dipijak
                var moves = tbs.GetAvailableMoveWorldPositions(enemy);

                // Hide kembali highlight
                tbs.HidePlayerMove();

                // Pilih yang paling dekat ke salah satu cell offset (agar attack pattern tercapai)
                Vector3Int desiredAttackCell = offsets
                    .Select(off => enemy.Coordinates + off)
                    .OrderBy(cell => Mathf.Abs(cell.x - target.Coordinates.x) + Mathf.Abs(cell.y - target.Coordinates.y))
                    .First();
                
                // Dari moves, pilih satu yang meminimalkan jarak ke desiredAttackCell
                Vector3 bestWorld = moves
                    .OrderBy(w => {
                        var c = tbs.WorldToCell(w);
                        return Mathf.Abs(c.x - desiredAttackCell.x) + Mathf.Abs(c.y - desiredAttackCell.y);
                    })
                    .FirstOrDefault();

                // Pindah jika ada
                if (bestWorld != default)
                {
                    tbs.ShowPlayerMove(enemy);
                    await UniTask.Delay(100);
                    tbs.MoveEnemy(enemy, bestWorld);
                    await UniTask.Delay(200);
                    tbs.HidePlayerMove();

                    enemy = tbs.ActiveUnit; // update referensi setelah move
                }
            }

            // Setelah move (atau jika sudah bisa attack), lakukan attack satu kali
            foreach (var off in offsets)
            {
                if (enemy.Coordinates + off == target.Coordinates)
                {
                    tbs.ShowPlayerAttack(enemy);
                    await UniTask.Delay(150);
                    tbs.PerformAttack();
                    await UniTask.Delay(200);
                    tbs.HidePlayerAttack();
                    break;
                }
            }

            // Giliran selesai
            tbs.BattleState.ChangeState(tbs.PlayerTurnState);
        }
        private async UniTask ExecuteSingleEnemyAction()
        {
            var enemies = tbs.GetAliveUnitsBySide(UnitSide.Enemy);
            var players = tbs.GetAliveUnitsBySide(UnitSide.Player);
            Debug.Log("ENEMIES " + enemies.Count + "PLAYER "+ players.Count);
            // Pilih enemy dan target terdekat
            UnitModel enemy = null, target = null;
            int bestDist = int.MaxValue;
            foreach (var e in enemies)
                foreach (var p in players)
                {
                    if (e.IsDead || p.IsDead) continue;
                    int d = Mathf.Abs(p.Coordinates.x - e.Coordinates.x)
                          + Mathf.Abs(p.Coordinates.y - e.Coordinates.y);
                    if (d < bestDist)
                    {
                        bestDist = d;
                        enemy = e;
                        target = p;
                    }
                }

            if (enemy == null || target == null)
            {
                tbs.BattleState.ChangeState(tbs.PlayerTurnState);
                return;
            }

            tbs.SetActiveUnit(enemy);

            // Ambil semua attack offsets
            var offsets = UnitAttackCalculate.GetOffsets(
                enemy.UnitData.AttackPattern,
                enemy.UnitData.Range,
                enemy.UnitData.Direction
            );

            // Cek jarak dan kemampuan attack saat ini
            int manh = Mathf.Abs(enemy.Coordinates.x - target.Coordinates.x)
                      + Mathf.Abs(enemy.Coordinates.y - target.Coordinates.y);
            bool adjacent    = (manh == 1);
            bool canAttackNow = offsets.Any(off => enemy.Coordinates + off == target.Coordinates);

            // Jika berdampingan tapi belum attackable, lakukan sirkulasi 1 langkah
            if (adjacent && !canAttackNow)
            {
                tbs.ShowPlayerMove(enemy);
                await UniTask.Delay(700);
                // coba tiap offset: pindah ke posisi berlawanan offset
                foreach (var off in offsets)
                {
                    var cirCell = target.Coordinates - off;
                    var worldCir = tbs.CellToWorld(cirCell) + new Vector3(0.5f, 0.5f);
                    if (tbs.GetUnit(worldCir) == null)
                    {
                        tbs.MoveEnemy(enemy, worldCir);
                        await UniTask.Delay(400);
                        enemy = tbs.ActiveUnit;
                        break;
                    }
                }
                tbs.HidePlayerMove();
            }
            else if (!canAttackNow)
            {
                // satu langkah mendekat biasa
                tbs.ShowPlayerMove(enemy);
                await UniTask.Delay(500);
                var step      = GetStepTowards(enemy.Coordinates, target.Coordinates);
                var worldStep = tbs.CellToWorld(step) + new Vector3(0.5f, 0.5f);
                if (tbs.GetUnit(worldStep) == null)
                {
                    tbs.MoveEnemy(enemy, worldStep);
                    await UniTask.Delay(300);
                }
                tbs.HidePlayerMove();
                enemy = tbs.ActiveUnit;
            }

            // Lakukan attack sekali jika sudah dalam offsets
            if (!canAttackNow) // kalau sudah bisa di awal, skip ke bawah
            {
                foreach (var off in offsets)
                {
                    if (enemy.Coordinates + off == target.Coordinates)
                    {
                        tbs.ShowPlayerAttack(enemy);
                        await UniTask.Delay(400);
                        tbs.PerformAttack();
                        await UniTask.Delay(500);
                        tbs.HidePlayerAttack();
                        break;
                    }
                }
            }
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


        private Vector3Int GetStepTowards(Vector3Int from, Vector3Int to)
        {
            var dir = to - from;
            if (Mathf.Abs(dir.x) > Mathf.Abs(dir.y))
                return from + new Vector3Int((int)Mathf.Sign(dir.x), 0, 0);
            return from + new Vector3Int(0, (int)Mathf.Sign(dir.y), 0);
        }
    }
}