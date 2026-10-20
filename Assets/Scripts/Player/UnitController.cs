using TilemapLayer;
using UnityEngine;

namespace Player
{
    public class UnitController:MonoBehaviour
    {
        private UnitModel _unitModel;
        private TurnBaseSystem _turnBaseSystem;
        private Animator _animator;
        private void Start()
        {
            _turnBaseSystem = TurnBaseSystem.Instance;
            _animator = GetComponent<Animator>();
        }

        public void InitializeUnit(UnitModel unitModel)
        {
            _unitModel = unitModel;
        }
        public void PlayAttackAnim()
        {
            _animator.SetTrigger("Attack");
        }

        public void PlayDeadAnim()
        {
            _animator.SetTrigger("Dead");
        }
    }
}