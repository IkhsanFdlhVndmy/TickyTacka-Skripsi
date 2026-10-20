using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class CardScroll : MonoBehaviour
{
    public RectTransform pageContainer;
    public RectTransform pageTemplate; // Assign disabled prefab page
    public CardUI itemPrefab;

    public Button btnLeft,
        btnRight;

    public float pageWidth = 600f;
    public int realPageCount = 2;

    [SerializeField]
    private List<CardSO> itemList = new();
    private List<RectTransform> pages = new();

    private int virtualPageIndex = 1; // Start at real page 0 (after the fake one)
    private bool isAnimating = false;
    private string selectedItemId = null;

    void Start()
    {
        btnLeft.onClick.AddListener(ScrollLeft);
        btnRight.onClick.AddListener(ScrollRight);

        BuildPages();
        SnapToPage(virtualPageIndex); // Start on first real page
    }

    void BuildPages()
    {
        // Clear old pages
        foreach (Transform child in pageContainer)
            Destroy(child.gameObject);
        pages.Clear();

        // Fake Last Page (copy of last real page)
        pages.Add(CreatePage(GetPageItems(realPageCount - 1)));

        // Real Pages
        for (int i = 0; i < realPageCount; i++)
        {
            pages.Add(CreatePage(GetPageItems(i)));
        }

        // Fake First Page (copy of first real page)
        pages.Add(CreatePage(GetPageItems(0)));

        // Position pages
        for (int i = 0; i < pages.Count; i++)
        {
            pages[i].anchoredPosition = new Vector2(i * pageWidth, 0);
            HorizontalLayoutGroup layoutGroup = pages[i].GetComponent<HorizontalLayoutGroup>();
            layoutGroup.enabled = false;
            foreach (Transform item in pages[i])
            {
                CardUI comp = item.GetComponent<CardUI>();
                comp.SetOriginalPosition();
            }
        }

        // Resize container
        pageContainer.sizeDelta = new Vector2(pages.Count * pageWidth, pageContainer.sizeDelta.y);
    }

    List<CardSO> GetPageItems(int page)
    {
        List<CardSO> items = new();
        int start = page * 4;
        for (int i = 0; i < 4; i++)
        {
            int idx = start + i;
            if (idx < itemList.Count)
                items.Add(itemList[idx]);
        }
        return items;
    }

    RectTransform CreatePage(List<CardSO> data)
    {
        RectTransform page = Instantiate(pageTemplate, pageContainer);
        page.gameObject.SetActive(true);



        foreach (Transform child in page)
            Destroy(child.gameObject);

        foreach (var item in data)
        {
            CardUI go = Instantiate(itemPrefab, page);
            go.cardSO = item;
            go.SetClicked(go.id == selectedItemId);
        }

        return page;
    }

    void ScrollLeft()
    {
        if (isAnimating)
            return;
        virtualPageIndex--;
        AnimateToPage(
            virtualPageIndex,
            () =>
            {
                if (virtualPageIndex == 0) // Fake first page (copy of last)
                {
                    virtualPageIndex = realPageCount;
                    SnapToPage(virtualPageIndex);
                }
            }
        );
    }

    void ScrollRight()
    {
        if (isAnimating)
            return;
        virtualPageIndex++;
        AnimateToPage(
            virtualPageIndex,
            () =>
            {
                if (virtualPageIndex == realPageCount + 1) // Fake last page (copy of first)
                {
                    virtualPageIndex = 1;
                    SnapToPage(virtualPageIndex);
                }
            }
        );
    }

    void AnimateToPage(int index, System.Action onComplete)
    {
        isAnimating = true;
        float targetX = -index * pageWidth;
        pageContainer
            .DOAnchorPosX(targetX, 0.5f)
            .SetEase(Ease.OutCubic)
            .OnComplete(() =>
            {
                isAnimating = false;
                onComplete?.Invoke();
            });
    }

    void SnapToPage(int index)
    {
        float targetX = -index * pageWidth;
        pageContainer.anchoredPosition = new Vector2(targetX, pageContainer.anchoredPosition.y);
    }
}
