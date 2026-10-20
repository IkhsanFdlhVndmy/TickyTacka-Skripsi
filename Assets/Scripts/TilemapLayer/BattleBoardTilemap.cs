using System;
using System.Collections.Generic;
using Player;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace TilemapLayer
{
    public class BattleBoardTilemap:TilemapLayer
    {
        [SerializeField] private BattleAreaTilemap _battleAreaTilemap;
        [SerializeField] private ColliderTilemap _colliderTilemap;
        private Dictionary<Vector3Int,UnitModel> _units = new();

        private void Start()
        {
            SpawnItem();
        }

        public void HideTileView()
        {
            if (_tilemap != null)
            {
                _tilemap.color = new Color(1f, 1f, 1f, 0f);
            }
        }

        public UnitModel Build(Vector3 worldCoords, GameObject prefab, UnitData unitData, TileItemSpawn tileItemSpawn=null)
        {
            Vector3Int baseCoords = _tilemap.WorldToCell(worldCoords);
            if(!IsEmpty(baseCoords)) return null;
            _colliderTilemap.SetCollider(baseCoords);
            var position = _tilemap.CellToWorld(baseCoords) + new Vector3(1 / 2f, 1 / 2f);
            var player = Instantiate( prefab, position, Quaternion.identity);
            var unit = new UnitModel(_tilemap, unitData, player ,baseCoords, position, tileItemSpawn);
            if (unit.UnitData)
            {
                var unitController = player.GetComponent<UnitController>();
                unitController.InitializeUnit(unit);
            }
            _units.Add(baseCoords, unit);
            return unit;
        }
        public SortedDictionary<int, Vector3> GetSpawnLoc(UnitSide side)
        {
            var dict = new SortedDictionary<int, Vector3>();
            foreach (var pos in _tilemap.cellBounds.allPositionsWithin)
            {
                var tile = _tilemap.GetTile(pos) as TileSpawn;
                if (tile != null && tile.SpawnSide == side)
                {
                    var world = _tilemap.CellToWorld(pos) + new Vector3(.5f, .5f, 0);
                    dict[tile.SpawnIndex] = world;
                }
            }
            return dict;
        }

        public void SpawnItem()
        {
            foreach (var pos in _tilemap.cellBounds.allPositionsWithin)
            {
                var tile = _tilemap.GetTile(pos) as TileItemSpawn;
                if (tile != null)
                {
                    var world = _tilemap.CellToWorld(pos) + new Vector3(.5f, .5f, 0);
                    Build(world, tile.PrefabItem, null, tile);
                }
            }
        }
        public bool IsEmpty(Vector3 worldCoords)
        {
            Vector3Int baseCoords = _tilemap.WorldToCell(worldCoords);
            if (_units.ContainsKey(baseCoords))
            {
                return false;
            }
            return true;

        }

        public UnitModel GetUnit(Vector3 worldCoords)
        {
            Vector3Int baseCoords = _tilemap.WorldToCell(worldCoords);
            _units.TryGetValue(baseCoords, out var data);
            return data;

        }

        public List<UnitModel> GetUnits(UnitSide side, bool life)
        {
            var list = new List<UnitModel>();
            foreach (var unit in _units)
            {
                if(!unit.Value.UnitData) continue;
                if (unit.Value.UnitData.UnitSide == side )
                {
                    if(unit.Value.IsItem) continue;
                    if (life && !unit.Value.IsDead )
                    {
                        list.Add(unit.Value);
                    }
                    if(!life)
                    {
                        list.Add(unit.Value);
                    }
                }
            }
            return list;
        }
        public void RemoveUnit(UnitModel buildable)
        {
            _colliderTilemap.DestroyCollider(buildable.Coordinates);
            _units.Remove(buildable.Coordinates);
            if (buildable.GameObject != null)
                Destroy(buildable.GameObject);
        }

        public Vector3 CellToWorld(Vector3Int baseCoords)
        {
            return _tilemap.CellToWorld(baseCoords);
        }
        public Vector3Int WorldToCell(Vector3 worldCoords)
        {
            return _tilemap.WorldToCell(worldCoords);
        }
    }
}