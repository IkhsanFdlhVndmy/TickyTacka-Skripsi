using System;
using UnityEngine;

namespace Input
{
    public class InputManager:PersistentSingleton<InputManager>
    {
        private InputActions _inputActions;
        private FiniteStateMachine<ActionMap> _actionMapStates;
        private PlayerActionMap _player;
        public PlayerActionMap PlayerInput => _player;
        protected override void Awake()
        {
            base.Awake();
            // Duplikat (dihancurkan oleh base.Awake) tidak perlu membuat InputActions sendiri
            if (Instance != this) return;
            InitializedManager();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            // Matikan & lepas InputActions agar tidak bocor (assert "Player.Disable() has not been called")
            if (_inputActions == null) return;
            _inputActions.Player.Disable();
            _inputActions.Dispose();
            _inputActions = null;
        }

        private void InitializedManager()
        {
            _inputActions = new InputActions();
            _player = new PlayerActionMap(_inputActions);
            _actionMapStates = new FiniteStateMachine<ActionMap>(_player);
        }

        public void PlayerMode()
        {
            _actionMapStates.ChangeState(_player);
        }
    }
}