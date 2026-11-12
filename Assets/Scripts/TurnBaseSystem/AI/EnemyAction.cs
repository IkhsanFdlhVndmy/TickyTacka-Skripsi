using TilemapLayer;
using UnityEngine;

namespace Turnbase_System.AI
{
    public enum EnemyActionType
    {
        Approach,      // mendekat 1 langkah ke player terdekat, lalu meledak jika target masuk pola
        DirectAttack   // target sudah dalam pola ledakan sejak awal giliran: langsung meledak
    }

    /// <summary>
    /// Satu keputusan enemy dalam satu giliran: unit mana yang bergerak, (opsional) pindah ke cell mana,
    /// lalu apakah mencoba attack setelah pindah. Policy hanya menghasilkan data ini;
    /// eksekusi (animasi, delay, PerformAttack) dilakukan oleh EnemyTurnState.
    /// </summary>
    public readonly struct EnemyAction
    {
        public UnitModel Unit { get; }
        public UnitModel Target { get; }
        public bool HasMove { get; }
        public Vector3Int MoveCell { get; }
        public bool TryAttack { get; }
        public EnemyActionType Type { get; }

        public bool IsNone => Unit == null;
        public static EnemyAction None => default;

        public EnemyAction(UnitModel unit, UnitModel target, Vector3Int? moveCell, bool tryAttack,
            EnemyActionType type = EnemyActionType.Approach)
        {
            Type = type;
            Unit = unit;
            Target = target;
            HasMove = moveCell.HasValue;
            MoveCell = moveCell ?? default;
            TryAttack = tryAttack;
        }
    }
}
