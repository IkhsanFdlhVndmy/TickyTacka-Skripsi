using System;
using TilemapLayer;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Player
{
    public class UnitPopUpController:MonoBehaviour
    {
        [SerializeField] private UnitActionPopUp _popUpAction;
        [SerializeField] private UnitMovePopup _unitMovePopup;
        [SerializeField] private UnitAttackPopup _unitAttackPopup;

        public UnitModel CurrentItem { get; private set; }
        private Vector3 _worldPosition;
        public Vector3 ScreenPos{ get; private set; }
        private Camera _mainCamera;
        private TurnBaseSystem _turnBaseSystem;
        private bool _alreadyMove;
        private bool _onMoveUnit;
        public bool AlreadyMove => _alreadyMove;
        public bool OnMoveUnit => _onMoveUnit;
        public void SetAlreadyMove(bool value)
        {
            _alreadyMove = value;
        }

        public void SetOnMoveUnit(bool value)
        {
            _onMoveUnit = value;
        }
        private void Awake()
        {
            _mainCamera = Camera.main;
        }

        private void Start()
        {
        }
        public void ShowPopUp()
        {
            gameObject.SetActive(true);
        }

        public void HidePopUp()
        {
            gameObject.SetActive(false);
        }

        private void InitializePopUp(UnitModel item, Vector3 position)
        {
            HideAttackPanel();
            HideMovePanel();
            
            _turnBaseSystem = TurnBaseSystem.Instance;
            CurrentItem = item;
            _worldPosition = position;
            gameObject.SetActive(true);
            _turnBaseSystem.SetActiveUnit(item);
            
            Vector3 screenPos = _mainCamera.WorldToScreenPoint(position);
            screenPos.y += 100f; 
            screenPos.x += 120f;
            ScreenPos = screenPos;
            gameObject.transform.position = screenPos;
        }
        public void ShowPopUpAction(UnitModel item, Vector3 position)
        {
            InitializePopUp(item, position);
            _popUpAction.Show();
        }

        public void ShowActionPanel()
        {
            _popUpAction.Show();
        }
        public void HideActionPanel()
        {
            _popUpAction.HidePopUp();
        }

        public void ShowAttackPanel()
        {
            _unitAttackPopup.Show();
        }

        public void HideAttackPanel()
        {
            _unitAttackPopup.HidePanel();
        }
        public void ShowMovePanel()
        {
            _unitMovePopup.Show();
        }

        public void ShowConfirmMovePanel()
        {
            _unitMovePopup.ShowConfirmPanel();
        }

        public void HideMovePanel()
        {
            _unitMovePopup.HidePanel();
        }

    }
}