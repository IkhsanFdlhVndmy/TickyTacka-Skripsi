using System;
using Input;
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
        private CanvasGroup _canvasGroup;
        private RectTransform _rectTransform;
        [SerializeField] private float _hoverAlphaWhileChoosingTile = 0.25f;
        public bool AlreadyMove => _alreadyMove;
        public bool IsChoosingMoveTile => _unitMovePopup.IsChoosingTile;
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
            _rectTransform = GetComponent<RectTransform>();
            _canvasGroup = GetComponent<CanvasGroup>();
            if (_canvasGroup == null) _canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        private void Update()
        {
            // Saat memilih petak tujuan move, popup tidak boleh menghalangi klik ke grid,
            // dan dibuat transparan ketika kursor berada di atasnya agar petak di bawahnya terlihat.
            bool choosingTile = _unitMovePopup.IsChoosingTile;
            _canvasGroup.blocksRaycasts = !choosingTile;

            bool hovered = false;
            if (choosingTile)
            {
                Vector2 mousePos = InputManager.Instance.PlayerInput.MousePos.Get();
                hovered = RectTransformUtility.RectangleContainsScreenPoint(_rectTransform, mousePos, null);
            }
            _canvasGroup.alpha = hovered ? _hoverAlphaWhileChoosingTile : 1f;
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

        /// <summary>
        /// Klik di luar UI saat popup terbuka.
        /// Sebelum move: aksi dibatalkan dan popup ditutup.
        /// Setelah move (AlreadyMove): tidak bisa dibatalkan, harus pilih Attack/Stay;
        /// konfirmasi attack yang terbuka hanya kembali ke menu Attack/Stay (sama seperti tombol "No").
        /// </summary>
        public void CancelByOutsideClick()
        {
            var tbs = TurnBaseSystem.Instance;
            if (_alreadyMove)
            {
                if (_unitAttackPopup.gameObject.activeSelf) _unitAttackPopup.Cancel();
                return;
            }

            tbs.OnMoveCanceled();
            tbs.OnAttackCanceled();
            _onMoveUnit = false;
            HideAttackPanel();
            HideMovePanel();
            HideActionPanel();
            HidePopUp();
            tbs.SetActiveUnit(null);
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