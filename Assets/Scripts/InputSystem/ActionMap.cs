
using UnityEngine;

namespace Input
{
    public abstract class ActionMap: IState
    {
        protected InputActions InputActions;
        public abstract bool HasPollable { get; }
        public ActionMap(InputActions action)
        {
            InputActions = action;
        }
        public abstract void OnEnter();

        public abstract void OnExit();

        public virtual void OnUpdate()
        {
        }
    }

    
    public class PlayerActionMap : ActionMap
    {
        private InputValue<Vector2> _movement;
        private InputValue<Vector2> _mousePos;
        private InputButton _performed;
        private InputButton _pause;
        public InputButton Performed => _performed;
        public InputValue<Vector2> Movement => _movement;
        public InputValue<Vector2> MousePos => _mousePos;
        public InputButton Pause => _pause;

        public override bool HasPollable => true;

        public PlayerActionMap(InputActions action) : base(action)
        {
            _movement = new InputValue<Vector2>(action.Player.Move);
            _performed = new InputButton(action.Player.Performed);
            _pause = new InputButton(action.Player.Pause);
            _mousePos = new InputValue<Vector2>(action.Player.Mouse);

        }


        public override void OnEnter()
        {
            InputActions.Player.Enable();
        }

        public override void OnExit()
        {
            InputActions.Player.Disable();
        }
        public override void OnUpdate()
        {
            _movement.ForcePoll();
            _mousePos.ForcePoll();
        }
    }
    
}