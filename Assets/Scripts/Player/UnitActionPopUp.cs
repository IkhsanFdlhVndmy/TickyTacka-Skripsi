using System;
using TilemapLayer;
using UnityEngine;
using UnityEngine.UI;

namespace Player
{
    public class UnitActionPopUp:MonoBehaviour
    {
        [SerializeField] private Button _buttonMove;
        [SerializeField] private Button _buttonStay;
        [SerializeField] private Button _buttonAttack;
        
        private UnitPopUpController _unitPopUpController;
        private TurnBaseSystem _turnBaseSystem;
        private UnitModel _unitModel;
        
        private void Start()
        {
            _turnBaseSystem = TurnBaseSystem.Instance;
        }

        private void InitializeButton(UnitModel unitModel)
        {
            _buttonMove.gameObject.SetActive(true);
            _buttonStay.gameObject.SetActive(true);
            _buttonAttack.gameObject.SetActive(true);
            if(_unitPopUpController.AlreadyMove) _buttonMove.gameObject.SetActive(false);
        }
        public void Show()
        {
            _turnBaseSystem = TurnBaseSystem.Instance;
            _unitPopUpController = _turnBaseSystem.UIManagerBattle.UnitController;
            gameObject.SetActive(true);
            _unitModel = _unitPopUpController.CurrentItem;
            InitializeButton(_unitModel);

            _buttonMove.onClick.RemoveAllListeners();
            _buttonStay.onClick.RemoveAllListeners();
            _buttonAttack.onClick.RemoveAllListeners();

            _buttonMove.onClick.AddListener(() => OnMoveAction());
            _buttonStay.onClick.AddListener(() => OnStayAction());
            _buttonAttack.onClick.AddListener(() => OnAttackAction());
        }
        public void HidePopUp()
        {
            gameObject.SetActive(false);
        }
        private void OnMoveAction()
        {
            // Debug.Log("Move Action " + _unitModel.UnitData.Name);
            HidePopUp();
            _turnBaseSystem.ShowPlayerMove(_unitModel);
            _unitPopUpController.ShowMovePanel();
        }

        private void OnStayAction()
        {
            // Debug.Log("Stay Action " + _unitModel.UnitData.Name);
            HidePopUp();
            _unitPopUpController.HidePopUp();
            _turnBaseSystem.OnStayPerformed();
        }

        private void OnAttackAction()
        {
            // Debug.Log("Attack Action " + _unitModel.UnitData.Name);
            HidePopUp();
            _turnBaseSystem.ShowPlayerAttack(_unitModel);
            _unitPopUpController.ShowAttackPanel();
        }
    }
}