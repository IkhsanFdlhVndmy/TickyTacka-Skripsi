using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Manager
{
    public class SceneController:PersistentSingleton<SceneController>
    {
        private bool _isPlaying;

        public void ChangeScene(string scene)
        {
            SceneManager.LoadScene(scene);
        }

        public void ChangeSceneWithSound(string scene)
        {
            if (!_isPlaying)
            {
                StartCoroutine(PlayAudioAndChangeScene(scene));
            }
        }
        private IEnumerator PlayAudioAndChangeScene(string scene)
        {
            _isPlaying = true;
            var audioSource = gameObject.GetComponent<AudioSource>();
            audioSource.Play();
            yield return new WaitForSeconds(audioSource.clip.length);

            UnityEngine.SceneManagement.SceneManager.LoadScene(scene);
        }
        public void QuitGame()
        {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
        }
        public void ResumeGame()
        {
            Time.timeScale = 1f; 
        }
        public void PauseGame()
        {
            Time.timeScale = 0f; 
        }

        public void NextStage(string currentStage)
        {
            switch (currentStage)
            {
                case "Stage1": ChangeScene("Stage2");
                    break;
                case "Stage2": ChangeScene("Stage3");
                    break;
                case "Stage3": ChangeScene("Stage4");
                    break;
                case "Stage4": ChangeScene("Stage5");
                    break;
                case "Stage5": ChangeScene("Credit");
                    break;
            }
        }
    }
}