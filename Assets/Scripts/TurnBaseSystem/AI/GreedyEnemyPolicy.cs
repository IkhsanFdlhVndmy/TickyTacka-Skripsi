using System.Collections.Generic;
using System.Linq;
using Player;
using TilemapLayer;
using UnityEngine;

namespace Turnbase_System.AI
{
    /// <summary>
    /// Baseline AI lama (greedy) yang menjadi pembanding penelitian. Keterbatasannya SENGAJA dipertahankan:
    /// - hanya mengejar pasangan (enemy, player) dengan jarak terdekat, tanpa memikirkan balasan pemain;
    /// - hanya melangkah 1 petak per giliran (kecuali sirkulasi bawaan saat sudah berdampingan);
    /// - tidak memperhitungkan friendly fire;
    /// - tidak memanfaatkan barrel dan tidak pernah menunggu posisi yang lebih baik.
    /// Perbaikan yang hanya bersifat bug: tidak menginjak obstacle/barrel/unit (hanya cell legal),
    /// memutari penghalang saat melangkah, dan langsung meledak jika target sudah dalam pola ledakan.
    /// </summary>
    public class GreedyEnemyPolicy : IEnemyPolicy
    {
        private static readonly Vector3Int[] Dirs =
        {
            Vector3Int.right, Vector3Int.left, Vector3Int.up, Vector3Int.down
        };

        public EnemyAction ChooseAction(TurnBaseSystem tbs)
        {
            var enemies = tbs.GetAliveUnitsBySide(UnitSide.Enemy);
            var players = tbs.GetAliveUnitsBySide(UnitSide.Player);

            // Pilih enemy dan target terdekat
            UnitModel enemy = null, target = null;
            int bestDist = int.MaxValue;
            foreach (var e in enemies)
                foreach (var p in players)
                {
                    if (e.IsDead || p.IsDead) continue;
                    int d = Manhattan(e.Coordinates, p.Coordinates);
                    if (d < bestDist)
                    {
                        bestDist = d;
                        enemy = e;
                        target = p;
                    }
                }

            if (enemy == null || target == null) return EnemyAction.None;

            var offsets = UnitAttackCalculate.GetOffsets(
                enemy.UnitData.AttackPattern,
                enemy.UnitData.Range,
                enemy.UnitData.Direction
            );

            bool adjacent = Manhattan(enemy.Coordinates, target.Coordinates) == 1;
            bool canAttackNow = offsets.Any(off => enemy.Coordinates + off == target.Coordinates);

            // Target sudah dalam pola ledakan: langsung meledak (tanpa cek friendly fire)
            if (canAttackNow)
                return new EnemyAction(enemy, target, null, tryAttack: true, type: EnemyActionType.DirectAttack);

            var legalCells = tbs.GetLegalMoveCells(enemy);
            Vector3Int? moveCell = null;
            if (adjacent)
            {
                // Sirkulasi: pindah ke posisi berlawanan offset agar target masuk pola attack
                foreach (var off in offsets)
                {
                    var cirCell = target.Coordinates - off;
                    if (legalCells.Contains(cirCell))
                    {
                        moveCell = cirCell;
                        break;
                    }
                }
            }
            else
            {
                // Satu langkah mendekat; jika langkah lurus terhalang obstacle/unit, cari jalan memutar
                // Langkah lurus hanya dipakai jika benar-benar memperpendek jalur (tidak masuk kantong buntu).
                var step = GetStepTowards(enemy.Coordinates, target.Coordinates);
                if (legalCells.Contains(step) && IsImprovingStep(tbs, enemy.Coordinates, step, target.Coordinates))
                    moveCell = step;
                else
                    moveCell = GetDetourStep(tbs, enemy.Coordinates, target.Coordinates, legalCells);
            }

            // Setelah melangkah, meledak jika target masuk pola ledakan (dicek saat eksekusi)
            return new EnemyAction(enemy, target, moveCell, tryAttack: true);
        }

        private static Vector3Int GetStepTowards(Vector3Int from, Vector3Int to)
        {
            var dir = to - from;
            if (Mathf.Abs(dir.x) > Mathf.Abs(dir.y))
                return from + new Vector3Int((int)Mathf.Sign(dir.x), 0, 0);
            return from + new Vector3Int(0, (int)Mathf.Sign(dir.y), 0);
        }

        /// <summary>
        /// Langkah 1 cell yang legal dan memperpendek jarak jalan ke target.
        /// Jalan dihitung memutari obstacle DAN cell yang ditempati (barrel, abu robot, robot lain).
        /// Jika jalur seperti itu tidak ada (mis. tertutup robot), fallback ke jalur yang hanya memutari obstacle.
        /// Null jika tidak ada langkah yang lebih dekat, sehingga enemy diam seperti sebelumnya.
        /// </summary>
        private static Vector3Int? GetDetourStep(TurnBaseSystem tbs, Vector3Int from, Vector3Int to,
            HashSet<Vector3Int> legalCells)
        {
            var step = GetImprovingStep(from, legalCells, GetWalkDistances(tbs, to, from, avoidOccupied: true));
            if (step.HasValue) return step;
            return GetImprovingStep(from, legalCells, GetWalkDistances(tbs, to, from, avoidOccupied: false));
        }

        private static bool IsImprovingStep(TurnBaseSystem tbs, Vector3Int from, Vector3Int step, Vector3Int to)
        {
            var dist = GetWalkDistances(tbs, to, from, avoidOccupied: true);
            // Jika jalur bebas tidak ada sama sekali, pertahankan perilaku lama (langkah lurus diizinkan).
            if (!dist.TryGetValue(from, out var current)) return true;
            return dist.TryGetValue(step, out var next) && next < current;
        }

        private static Vector3Int? GetImprovingStep(Vector3Int from, HashSet<Vector3Int> legalCells,
            Dictionary<Vector3Int, int> distToTarget)
        {
            int current = distToTarget.TryGetValue(from, out var c) ? c : int.MaxValue;

            Vector3Int? best = null;
            int bestDist = current;
            foreach (var d in Dirs)
            {
                var next = from + d;
                if (!legalCells.Contains(next)) continue;
                if (distToTarget.TryGetValue(next, out var nd) && nd < bestDist)
                {
                    bestDist = nd;
                    best = next;
                }
            }
            return best;
        }

        /// <summary>
        /// Jarak jalan (BFS 4 arah) dari origin (cell target) ke semua cell walkable.
        /// avoidOccupied: cell berisi unit/barrel dianggap penghalang, kecuali cell target dan cell enemy sendiri.
        /// </summary>
        private static Dictionary<Vector3Int, int> GetWalkDistances(TurnBaseSystem tbs, Vector3Int origin,
            Vector3Int self, bool avoidOccupied)
        {
            var dist = new Dictionary<Vector3Int, int> { [origin] = 0 };
            var queue = new Queue<Vector3Int>();
            queue.Enqueue(origin);
            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                foreach (var d in Dirs)
                {
                    var next = current + d;
                    if (dist.ContainsKey(next) || !tbs.IsWalkableCell(next)) continue;
                    if (avoidOccupied && next != self && !tbs.IsEmptyCell(next)) continue;
                    dist[next] = dist[current] + 1;
                    queue.Enqueue(next);
                }
            }
            return dist;
        }

        private static int Manhattan(Vector3Int a, Vector3Int b)
            => Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
    }
}
