using System.Collections.Generic;
using UnityEngine;

namespace Player
{
    public static class UnitAttackCalculate
    {
        private static readonly Dictionary<AttackDirection, Vector3Int> DirMap = new() {
            { AttackDirection.Up, Vector3Int.up },
            { AttackDirection.Down, Vector3Int.down },
            { AttackDirection.Left, Vector3Int.left },
            { AttackDirection.Right, Vector3Int.right }
        };

        public static List<Vector3Int> GetOffsets(AttackPattern pattern, int range, AttackDirection dir) {
            var list = new List<Vector3Int>();
            switch (pattern) {
                case AttackPattern.Single:
                    // hanya satu di arah dir
                    list.Add(DirMap[dir]);
                    break;

                case AttackPattern.Line:
                    // deret cell di arah dir
                    Vector3Int d = DirMap[dir];
                    for (int i = 1; i <= range; i++)
                        list.Add(d * i);
                    break;

                case AttackPattern.Cross:
                    // plus shape di semua 4 arah
                    foreach (var kv in DirMap) {
                        for (int i = 1; i <= range; i++)
                            list.Add(kv.Value * i);
                    }
                    break;

                case AttackPattern.Surround:
                    // ring di sekeliling
                    for (int x = -range; x <= range; x++) {
                        for (int y = -range; y <= range; y++) {
                            if (Mathf.Max(Mathf.Abs(x), Mathf.Abs(y)) == range)
                                list.Add(new Vector3Int(x, y, 0));
                        }
                    }
                    break;
            }
            return list;
        }
    }
}