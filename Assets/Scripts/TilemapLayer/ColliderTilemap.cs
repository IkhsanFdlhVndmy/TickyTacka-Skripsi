using System.Collections.Generic;
using Player;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace TilemapLayer
{
    public class ColliderTilemap:TilemapLayer
    {
        [SerializeField] private TileBase _colliderTile;

        public void SetCollider(Vector3Int baseCoords)
        {
            _tilemap.SetTile(baseCoords, _colliderTile);
        }
        public void DestroyCollider(Vector3Int baseCoords)
        {
            _tilemap.SetTile(baseCoords, null);
        }
    }
}