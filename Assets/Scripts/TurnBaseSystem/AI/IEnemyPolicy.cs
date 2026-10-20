namespace Turnbase_System.AI
{
    /// <summary>
    /// Pengambil keputusan enemy. Implementasi lain (mis. agen AlphaZero/MCTS) cukup mengimplementasikan
    /// interface ini dan memilih salah satu aksi legal dari TurnBaseSystem.GetLegalMoveCells,
    /// lalu dipasang lewat EnemyTurnState.SetPolicy.
    /// </summary>
    public interface IEnemyPolicy
    {
        EnemyAction ChooseAction(TurnBaseSystem tbs);
    }
}
