using Cysharp.Threading.Tasks;
using DG.Tweening;
using TMPro;
using UnityEngine;

namespace Turnbase_System
{
    public class GameAnnouncement:MonoBehaviour
    {
        [SerializeField] private RectTransform panelTransform;
        [SerializeField] private TextMeshProUGUI announcementText;

        [Header("Animation Settings")]
        [SerializeField] private float slideInDuration = 0.4f;
        [SerializeField] private float slideOutDuration = 0.4f;

        private Vector2 _offscreenRight;
        private Vector2 _center;
        private Vector2 _offscreenLeft;

        private void Awake()
        {
            // Hitung posisi berdasarkan lebar layar
            float canvasWidth = ((RectTransform)panelTransform.parent).rect.width;

            _offscreenRight = new Vector2(canvasWidth, 0);
            _center = Vector2.zero;
            _offscreenLeft = new Vector2(-canvasWidth, 0);

            panelTransform.anchoredPosition = _offscreenRight;
        }

        public async UniTask ShowAnnouncement(string message, float stayDuration = 1.2f)
        {
            announcementText.text = message;

            // Masuk dari kanan ke tengah
            Tween slideIn = panelTransform.DOAnchorPos(_center, slideInDuration).SetEase(Ease.OutQuart);
            await slideIn.AsyncWaitForCompletion();

            // Diam sejenak
            await UniTask.Delay((int)(stayDuration * 1000));

            // Keluar ke kiri
            Tween slideOut = panelTransform.DOAnchorPos(_offscreenLeft, slideOutDuration).SetEase(Ease.InQuart);
            await slideOut.AsyncWaitForCompletion();

            // Reset ke kanan lagi (biar siap dipakai ulang)
            panelTransform.anchoredPosition = _offscreenRight;
        }
    }
}