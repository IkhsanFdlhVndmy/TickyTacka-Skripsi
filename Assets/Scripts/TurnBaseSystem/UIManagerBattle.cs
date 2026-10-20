using System;
using Audio;
using Cysharp.Threading.Tasks;
using Player;
using TilemapLayer;
using UnityEngine;
using UnityEngine.Serialization;

namespace Turnbase_System
{
    public class UIManagerBattle:MonoBehaviour
    {
        [SerializeField] private UnitPopUpController _unitPopUpController;
        [SerializeField] private GameObject _explosivePS;
        [SerializeField] private GameObject _tutorialPanel;
        [SerializeField] private GameAnnouncement _gameAnnouncement;
        [SerializeField] private GameObject _victoryPanel;
        [SerializeField] private GameObject _losePanel;
        [SerializeField] private AudioTrigger _audioTrigger;
        [SerializeField] private GameObject _pausePanel;
        // [SerializeField] private Canvas _mainCanvas;

        // public Canvas MainCanvas => _mainCanvas;
        private CardManager _cardManager;
        private AudioManager _audioManager;

        public UnitPopUpController UnitController => _unitPopUpController;

        private void Start()
        {
            _cardManager = CardManager.Instance;

            _audioManager = AudioManager.Instance;
        }
        
        public void ShowPlayerCards()
        {
            _cardManager.SpawnChooseCard();
        }
        public void HidePlayerCards()
        {
            _cardManager.DestroyChooseCard();
        }
        public void ShowRecruitCards()
        {
            _cardManager.SpawnRecruitCard();
        }
        public void HideRecruitCards()
        {
            _cardManager.DestroyRecruitCard();
        }

        public void ShowTutorial()
        {
            _tutorialPanel.SetActive(true);
        }
        public void HideTutorial()
        {
            _tutorialPanel.SetActive(false);
        }
        public void HideUnitAction()
        {
            _unitPopUpController.HidePopUp();
        }
        public void ShowUnitAction(UnitModel unitModel,Vector3 position)
        {
            _unitPopUpController.ShowPopUpAction(unitModel, position);
        }

        public void ShowConfirmMove()
        {
            _unitPopUpController.ShowConfirmMovePanel();
        }

        public void StartVFXExplosive(Vector3 position)
        {
            Instantiate(_explosivePS, position, Quaternion.identity);
            _audioTrigger.TriggerSound();   
        }

        public async UniTask ShowAnnouncement(string message, float stayDuration = 1.2f)
        {
            await _gameAnnouncement.ShowAnnouncement(message, stayDuration);
        }

        public void ShowVictoryPanel()
        {
            _audioManager.PlaySound(SoundType.SFX_Victory);
            _victoryPanel.SetActive(true);
        }
        public void HideVictoryPanel()
        {
            _victoryPanel.SetActive(false);
        }
        public void ShowLosePanel()
        {
            _audioManager.PlaySound(SoundType.SFX_Lose);
            _cardManager.ResetCard();
            _losePanel.SetActive(true);
        }
        public void HideLosePanel()
        {
            _losePanel.SetActive(false);
        }

        public void RemoveCard(CardSO cardSo)
        {
            _cardManager.RemoveCard(cardSo);
        }

        public void PauseGame()
        {
            _pausePanel.SetActive(true);
            Time.timeScale = 0f;
        }

        public void ResumeGame()
        {
            _pausePanel.SetActive(false);
            Time.timeScale = 1f;
        }
    }
}