using TilemapLayer;
using UnityEngine;
using UnityEngine.UI;

namespace Player
{
    public class UnitAttackPopup:MonoBehaviour
    {
        [SerializeField] private Button _confirmYes;
        [SerializeField] private Button _confirmNo;
        private UnitPopUpController _unitPopUpController;
        private TurnBaseSystem _turnBaseSystem;

        public void Show()
        {
            _turnBaseSystem = TurnBaseSystem.Instance;
            _unitPopUpController = _turnBaseSystem.UIManagerBattle.UnitController;
            gameObject.SetActive(true);

            _confirmYes.onClick.RemoveAllListeners();
            _confirmNo.onClick.RemoveAllListeners();

            _confirmYes.onClick.AddListener(() => OnConfirmYes());
            _confirmNo.onClick.AddListener(() => OnConfirmNo());
        }
        public void HidePanel()
        {
            gameObject.SetActive(false);
        }
        private void OnConfirmYes()
        {
            _turnBaseSystem.OnAttackButton();
            _turnBaseSystem.UIManagerBattle.UnitController.SetAlreadyMove(false);
        }

        private void OnConfirmNo()
        {
            HidePanel();
            _turnBaseSystem.OnAttackCanceled();
            _unitPopUpController.ShowActionPanel();
        }
    }
}