using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class TutorialScene : MonoBehaviour
{
    [SerializeField]
    private RectTransform _pageContainer;
    [SerializeField]
    private Button _leftButton;

    [SerializeField]
    private Button _rightbutton;

    [SerializeField]
    private float _pageWidth;
    private int _realPageCount = 5;
    private List<RectTransform> _pages = new();
    private int pageIndex = 0;
    private bool _isAnimating;

    void Start()
    {
        _leftButton.onClick.AddListener(ScrollLeft);
        _rightbutton.onClick.AddListener(ScrollRight);
        foreach (RectTransform item in _pageContainer)
        {
            _pages.Add(item);
        }
    }

    void ScrollLeft()
    {
        if (_isAnimating)
            return;
        pageIndex--;
        AnimateToPage(
            pageIndex,
            () =>
            {
                if (pageIndex == -1) 
                {
                    pageIndex = _realPageCount - 1;
                    SnapToPage(pageIndex);
                }
            }
        );
    }

    void ScrollRight()
    {
        if (_isAnimating)
            return;
        pageIndex++;
        AnimateToPage(
            pageIndex,
            () =>
            {
                if (pageIndex == _realPageCount) 
                {
                    pageIndex = 0;
                    SnapToPage(pageIndex);
                }
            }
        );
    }

    void AnimateToPage(int index, System.Action onComplete)
    {
        foreach (Transform item in _pages)
        {
            item.GetComponent<ScrollRect>().verticalNormalizedPosition = 1f;
        }

        _isAnimating = true;
        float targetX = -index * _pageWidth;
        _pageContainer
            .DOAnchorPosX(targetX, 0.5f)
            .SetEase(Ease.OutCubic)
            .OnComplete(() =>
            {
                _isAnimating = false;
                onComplete?.Invoke();
            });
    }

    void SnapToPage(int index)
    {
        float targetX = -index * _pageWidth;
        _pageContainer.anchoredPosition = new Vector2(targetX, _pageContainer.anchoredPosition.y);
    }
}
