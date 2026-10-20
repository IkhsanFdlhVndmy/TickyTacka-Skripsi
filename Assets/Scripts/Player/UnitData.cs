using UnityEngine;

namespace Player
{
    [CreateAssetMenu(fileName = "UnitData", menuName = "Create Unit Data")]
    public class UnitData : ScriptableObject
    {
        [SerializeField] private string _name;
        [SerializeField] private UnitSide _unitSide;
        [SerializeField] private GameObject _prefab;
        [SerializeField] private int _moveRange;
        [SerializeField] private AttackPattern _attackPattern;
        [SerializeField] private int _attackRange;
        [SerializeField] private AttackDirection _attackDirection;
        public string Name => _name;
        public UnitSide UnitSide => _unitSide;
        public GameObject UnitPrefab => _prefab;
        public AttackPattern AttackPattern => _attackPattern;
        public AttackDirection Direction => _attackDirection;
        public int Range => _attackRange;
        public int MoveRange => _moveRange;
        public Sprite UnitSprite => UnitPrefab.GetComponent<SpriteRenderer>().sprite;
    }
    
    public enum UnitSide
    {
        Enemy,
        Player
    }
    public enum AttackPattern {
        Single,    // Type A
        Line,      // Type B
        Cross,     // Type C
        Surround   // Type D
    }
    
    public enum AttackDirection {
        Up,
        Down,
        Left,
        Right
    }
}