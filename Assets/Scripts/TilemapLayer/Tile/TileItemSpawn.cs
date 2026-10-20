using Player;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace TilemapLayer
{
    [CreateAssetMenu(fileName = "TileItemSpawn", menuName = "Tile/Item Tile")]
    public class TileItemSpawn: TileBase
    {
        [SerializeField] private GameObject _prefabItem;
        [SerializeField] private Color _validAreaColor = new Color(0, 1, 0, 0.3f);
        [SerializeField] private Sprite _areaSprite;
        [SerializeField] private AttackPattern _attackPattern;
        [SerializeField] private int _attackRange;
        [SerializeField] private AttackDirection _attackDirection;
        public AttackDirection AttackDirection => _attackDirection;
        public AttackPattern AttackPattern => _attackPattern;
        public int AttacKRange => _attackRange;
        public GameObject PrefabItem => _prefabItem;
        public override void RefreshTile(Vector3Int position, ITilemap tilemap)
        {
            tilemap.RefreshTile(position);
        }

        public override void GetTileData(Vector3Int position, ITilemap tilemap, ref TileData tileData)
        {
            tileData.sprite = _areaSprite;
            tileData.color = _validAreaColor ;
            tileData.flags = TileFlags.LockColor;
            tileData.colliderType = Tile.ColliderType.None; 
        }
    }
}