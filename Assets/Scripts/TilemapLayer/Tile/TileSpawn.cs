using Player;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace TilemapLayer
{
    [CreateAssetMenu(fileName = "TileSpawn", menuName = "Tile/Spawn")]
    public class TileSpawn: TileBase
    {
        [SerializeField] private Color _validAreaColor = new Color(0, 1, 0, 0.3f);
        [SerializeField] private Sprite _areaSprite;
        [SerializeField] private UnitSide _spawnSide;
        [SerializeField] private int _spawnIndex;
        public UnitSide SpawnSide => _spawnSide;
        public int SpawnIndex => _spawnIndex;
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