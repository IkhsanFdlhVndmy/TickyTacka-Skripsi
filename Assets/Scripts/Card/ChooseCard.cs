using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class ChooseCard : MonoBehaviour
{
    [SerializeField]
    private List<Vector2> _selectedCardPosition;

    [SerializeField]
    private Vector2 _bottomPanelTargetPos;

    private Dictionary<Vector2, bool> _selectedCardPositionValue = new Dictionary<Vector2, bool>();

    private List<CardSO> _selectedCard = new List<CardSO>();

    [SerializeField]
    private RectTransform _centerPanel;

    [SerializeField]
    private RectTransform _bottomPanel;

    [SerializeField]
    private Button _confirmButton;

    private TurnBaseSystem _turnBaseSystem;

    void Start()
    {
        _turnBaseSystem = TurnBaseSystem.Instance;
        for (var i = 0; i < _selectedCardPosition.Count; i++)
        {
            _selectedCardPositionValue.Add(_selectedCardPosition[i], false);
        }

        CardManager.Instance.SpawnCard(_bottomPanel);

        foreach (Transform item in _bottomPanel)
        {
            item.localScale = new Vector3(2.5f, 2.5f, 2.5f);
        }
        _bottomPanel
            .DOAnchorPosX(_bottomPanelTargetPos.x, 0.4f)
            .SetEase(Ease.InOutQuad)
            .OnComplete(() =>
            {
                foreach (RectTransform item in _bottomPanel)
                {
                    CardUI card = item.GetComponent<CardUI>();
                    card.SetOriginalPosition();
                    card.SetChooseCard();
                    HorizontalLayoutGroup layoutGroup =
                        _bottomPanel.GetComponent<HorizontalLayoutGroup>();
                    layoutGroup.enabled = false;
                }
            });

        SetConfirmButton();
    }

    private void SetConfirmButton()
    {
        _confirmButton.onClick.AddListener(() =>
        {
            CardManager.Instance.DestroyChooseCard();
        });
    }

    public Transform GetCenterPanel()
    {
        return _centerPanel;
    }

    public Transform GetBottomPanel()
    {
        return _bottomPanel;
    }

    public Vector2 GetAvailablePosition()
    {
        Vector2 position = new Vector2();
        foreach (var item in _selectedCardPositionValue)
        {
            if (!item.Value)
                position = item.Key;
        }
        return position;
    }

    public bool IsFull()
    {
        return _selectedCard.Count == 4;
    }

    public void AddCard(Vector2 targetPos, CardSO card)
    {
        _selectedCardPositionValue[targetPos] = true;
        _selectedCard.Add(card);
    }

    public void RemoveCard(Vector2 targetPos, CardSO card)
    {
        _selectedCardPositionValue[targetPos] = false;
        _selectedCard.Remove(card);
    }

    public void FinishChooseCard(Action onComplete)
    {
        if(_selectedCard.Count<1) return;
        _bottomPanel
            .DOAnchorPosX(1200, 0.3f)
            .OnComplete(() =>
            {
                _centerPanel
                    .DOAnchorPosX(-2200, 0.3f)
                    .OnComplete(() =>
                    {
                        foreach (var item in _selectedCard)
                        {
                           _turnBaseSystem.SetPlayer(item);
                           
                        }
                        _turnBaseSystem.OnDoneSelectPlayer();
                        onComplete?.Invoke();
                    });
            });
    }
}
