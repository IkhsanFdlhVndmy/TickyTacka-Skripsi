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
            InitializedManager();
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