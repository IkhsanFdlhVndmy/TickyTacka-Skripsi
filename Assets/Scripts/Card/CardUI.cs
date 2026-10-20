using System;
using Audio;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CardUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    private Vector2 _originalPosition;
    private bool _isClicked;
    private bool _isMoving;

    private ChooseCard _chooseCard;
    private RecruitCard _recruitCard;

    private bool _isChoose = false;
    private bool _isRecruit = false;

    [SerializeField]
    private float _hoverHeight = 20f;

    [SerializeField]
    private float _duration = 0.2f;

    public CardSO cardSO;
    private RectTransform _rectTransform;

    private Action<string> onClick;
    public string id;

    void Start()
    {
        // _originalPosition = transform.localPosition;
        _rectTransform = GetComponent<RectTransform>();
        GetComponent<Image>().sprite = cardSO.cardSprite;
        GetComponentInChildren<TextMeshProUGUI>().text = cardSO.cardName;
    }

    public void SetChooseCard()
    {
        _chooseCard = GetComponentInParent<ChooseCard>();
        _isChoose = true;
    }

    public void SetRecruitCard()
    {
        _recruitCard = GetComponentInParent<RecruitCard>();
        _isRecruit = true;
    }

    public void SetOriginalPosition()
    {
        _originalPosition = _rectTransform.anchoredPosition;
    }

    public void SetClicked(bool value)
    {
        _isClicked = value;
    }

    public void SetId(string id)
    {
        this.id = id;
    }

    public void OnClick()
    {
        onClick?.Invoke(id);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!_isClicked && !_isMoving && !_isRecruit)
        {
            _rectTransform.DOAnchorPosY(_originalPosition.y + _hoverHeight, _duration);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!_isClicked && !_isMoving && !_isRecruit) 
        {
            _rectTransform.DOAnchorPosY(_originalPosition.y, _duration);
        }
    }

    private void moveBackChooseCard()
    {
        Sequence seq = DOTween.Sequence();
        _chooseCard.RemoveCard(_rectTransform.anchoredPosition, cardSO);
        transform.SetParent(_chooseCard.GetBottomPanel());
        seq.Join(_rectTransform.DOScale(2.5f, _duration));
        seq.Join(_rectTransform.DOAnchorPos(_originalPosition, _duration).SetEase(Ease.InOutQuad));

        seq.OnComplete(() =>
        {
            _isClicked = false;
            _rectTransform.DOAnchorPosY(_originalPosition.y, _duration);
        });
    }

    private void moveBackRecruitCard()
    {
        Sequence seq = DOTween.Sequence();
        _recruitCard.RemoveCard(_rectTransform.anchoredPosition, cardSO);
        transform.SetParent(_recruitCard.GetCenterPanel());
        seq.Join(_rectTransform.DOScale(3f, _duration));
        seq.Join(_rectTransform.DOAnchorPos(_originalPosition, _duration).SetEase(Ease.InOutQuad));

        seq.OnComplete(() =>
        {
            _isClicked = false;
            _rectTransform.DOAnchorPosY(_originalPosition.y, _duration);
        });
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        AudioManager.Instance.PlaySound(SoundType.SFX_CardPick);
        if (_isClicked && !_isMoving )
        {
            if (_isChoose)
            {
                moveBackChooseCard();
            }
            else if (_isRecruit)
            {
                moveBackRecruitCard();
            }
        }
        else
        {
            // _isClicked = true;
            // transform.DOLocalMoveY(_originalPosition.y + _hoverHeight, _duration);

            if (_isChoose)
            {
                if (!_chooseCard.IsFull())
                {
                    _isMoving = true;
                    MoveChooseCard();
                }
            }
            else if (_isRecruit)
            {
                if (!_recruitCard.IsFull())
                {
                    _isMoving = true;
                    MoveRecruitCard();
                }
            }
        }
    }

    private void MoveChooseCard()
    {
        Vector2 targetPos = _chooseCard.GetAvailablePosition();
        if (transform.localPosition.y != _originalPosition.y + _hoverHeight)
        {
            _rectTransform
                .DOAnchorPosY(_originalPosition.y + _hoverHeight, _duration)
                .OnComplete(() =>
                {
                    Sequence seq = DOTween.Sequence();

                    transform.SetParent(_chooseCard.GetCenterPanel(), true);
                    seq.Join(transform.DOScale(3f, _duration));
                    seq.Join(
                        _rectTransform.DOAnchorPos(targetPos, _duration).SetEase(Ease.InOutQuad)
                    );
                    seq.OnComplete(() =>
                    {
                        _isMoving = false;
                        _isClicked = true;
                    });
                });
        }
        else
        {
            Sequence seq = DOTween.Sequence();
            transform.SetParent(_chooseCard.GetCenterPanel(), true);
            seq.Join(transform.DOScale(3f, _duration));
            seq.Join(_rectTransform.DOAnchorPos(targetPos, _duration).SetEase(Ease.InOutQuad));
            seq.OnComplete(() =>
            {
                _isMoving = false;
                _isClicked = true;
            });
        }
        _chooseCard.AddCard(targetPos, cardSO);
    }

    private void MoveRecruitCard()
    {
        Vector2 targetPos = _recruitCard.GetAvailablePosition();
        if (transform.localPosition.y != _originalPosition.y + _hoverHeight)
        {
            _rectTransform
                .DOAnchorPosY(_originalPosition.y + _hoverHeight, _duration)
                .OnComplete(() =>
                {
                    Sequence seq = DOTween.Sequence();

                    transform.SetParent(_recruitCard.GetBottomPanel(), true);
                    seq.Join(transform.DOScale(2.5f, _duration));
                    seq.Join(
                        _rectTransform.DOAnchorPos(targetPos, _duration).SetEase(Ease.InOutQuad)
                    );
                    seq.OnComplete(() =>
                    {
                        _isMoving = false;
                        _isClicked = true;
                    });
                });
        }
        else
        {
            Sequence seq = DOTween.Sequence();
            transform.SetParent(_recruitCard.GetBottomPanel(), true);
            seq.Join(transform.DOScale(2f, _duration));
            seq.Join(_rectTransform.DOAnchorPos(targetPos, _duration).SetEase(Ease.InOutQuad));
            seq.OnComplete(() =>
            {
                _isMoving = false;
                _isClicked = true;
            });
        }
        _recruitCard.AddCard(targetPos, cardSO);
    }
}
