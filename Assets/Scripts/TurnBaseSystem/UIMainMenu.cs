using System;
using Manager;
using UnityEngine;

namespace Turnbase_System
{
    public class UIMainMenu:MonoBehaviour
    {
        [SerializeField] private GameObject _settingPanel;
        private SceneController _sceneController;

        private void Start()
        {
            _sceneController = SceneController.Instance;
        }

        public void OnStartButton()
        {
            _sceneController.ChangeScene("Stage1");
        }

        public void ShowSettingPanel()
        {
            _settingPanel.SetActive(true);
        }
        public void HideSettingPanel()
        {
            _settingPanel.SetActive(false);
        }

        public void QuitGame()
        {
            _sceneController.QuitGame();
        }
    }
}