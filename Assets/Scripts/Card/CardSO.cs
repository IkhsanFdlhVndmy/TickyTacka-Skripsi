using System;
using Player;
using UnityEngine;

[CreateAssetMenu(fileName = "CardSO", menuName = "Scriptable Objects/CardSO")]
public class CardSO : ScriptableObject
{
    public Sprite cardSprite;
    public string cardName;
    public CardType cardType;
    public UnitData UnitData;
}
