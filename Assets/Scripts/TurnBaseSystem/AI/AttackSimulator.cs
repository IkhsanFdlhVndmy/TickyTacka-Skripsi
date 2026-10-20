using System.Collections.Generic;
using Player;
using TilemapLayer;
using UnityEngine;

namespace Turnbase_System.AI
{
    /// <summary>Prediksi hasil satu attack tanpa mengubah papan.</summary>
    public readonly struct AttackOutcome
    {
        public int PlayersKilled { get; }
        public int EnemiesKilled { get; }      // tidak termasuk attacker (attacker selalu mati setelah attack)
        public int BarrelsTriggered { get; }
        public UnitModel FirstBarrel { get; }

        public AttackOutcome(int playersKilled, int enemiesKilled, int barrelsTriggered, UnitModel firstBarrel)
        {
            PlayersKilled = playersKilled;
            EnemiesKilled = enemiesKilled;
            BarrelsTriggered = barrelsTriggered;
            FirstBarrel = firstBarrel;
        }
    }

    /// <summary>
    /// Forward model attack: meniru aturan TurnBaseSystem.PerformAttack + TriggerItem
    /// (unit di pola attack mati, barrel yang terkena meledak dengan pola miliknya),
    /// tetapi hanya menghitung hasilnya. Berguna untuk policy heuristik maupun simulasi MCTS/AlphaZero.
    /// Jika aturan PerformAttack/TriggerItem diubah, simulator ini harus ikut disesuaikan.
    /// </summary>
    public static class AttackSimulator
    {
        public static AttackOutcome Predict(TurnBaseSystem tbs, UnitModel attacker, Vector3Int fromCell)
        {
            var killed = new HashSet<UnitModel>();
            var triggered = new HashSet<UnitModel>();
            UnitModel firstBarrel = null;

            var data = attacker.UnitData;
            var offsets = UnitAttackCalculate.GetOffsets(data.AttackPattern, data.Range, data.Direction);
            foreach (var off in offsets)
            {
                var target = GetUnitAt(tbs, fromCell + off);
                // attacker sendiri diabaikan (saat simulasi setelah move, cell lamanya sudah kosong)
                if (target == null || target == attacker) continue;

                AddIfKillable(killed, target, attacker);

                if (target.IsItem && triggered.Add(target))
                {
                    if (firstBarrel == null) firstBarrel = target;
                    var item = target.ItemData;
                    var blast = UnitAttackCalculate.GetOffsets(item.AttackPattern, item.AttacKRange, item.AttackDirection);
                    foreach (var b in blast)
                        AddIfKillable(killed, GetUnitAt(tbs, target.Coordinates + b), attacker);
                }
            }

            int players = 0, enemies = 0;
            foreach (var unit in killed)
            {
                if (unit.UnitData.UnitSide == UnitSide.Player) players++;
                else enemies++;
            }
            return new AttackOutcome(players, enemies, triggered.Count, firstBarrel);
        }

        private static void AddIfKillable(HashSet<UnitModel> killed, UnitModel unit, UnitModel attacker)
        {
            // hanya robot yang masih hidup (barrel tidak punya UnitData, abu sudah IsDead)
            if (unit == null || unit == attacker || !unit.UnitData || unit.IsDead) return;
            killed.Add(unit);
        }

        private static UnitModel GetUnitAt(TurnBaseSystem tbs, Vector3Int cell)
            => tbs.GetUnit(tbs.CellToWorld(cell) + new Vector3(0.5f, 0.5f));
    }
}
