using System;
using System.Collections.Generic;
using Player;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace TilemapLayer
{
    public class BattleAreaTilemap:TilemapLayer
    {
        [Header("Move Highlight")]
        [SerializeField] private GameObject _highlightContainer;
        [SerializeField] private GameObject _highlight;
        [Header("Attack Highlight")]
        [SerializeField] private GameObject _attackHighlightContainer;
        [SerializeField] private GameObject _attackHighlightPrefab;
        private HashSet<Vector3Int> _validMoveCells = new();
        private HashSet<Vector3Int> _validAttackCells = new();

        private void Start()
        {
            if (_tilemap != null)
            {
                _tilemap.color = new Color(1f, 1f, 1f, 0f);
            }
        }

        public bool IsValidBattleArea(Vector3 worldPosition)
        {
            Vector3Int baseCoords= _tilemap.WorldToCell(worldPosition);
            TileBase tile = _tilemap.GetTile(baseCoords);
            return tile is TileBattleArea;
        }

        public void ShowMoveTile(UnitModel unitModel)
        {
            _validMoveCells.Clear();
            foreach (Transform c in _highlightContainer.transform)
                Destroy(c.gameObject);

            Vector3Int start = unitModel.Coordinates;
            int moveRange = unitModel.UnitData.MoveRange;

            var visited = new HashSet<Vector3Int>();
            var queue   = new Queue<(Vector3Int cell, int dist)>();

            // mulai dari origin
            visited.Add(start);
            queue.Enqueue((start, 0));

            // empat arah
            var dirs = new[]
            {
                new Vector3Int(1, 0, 0),
                new Vector3Int(-1, 0, 0),
                new Vector3Int(0, 1, 0),
                new Vector3Int(0, -1, 0),
            };

            while (queue.Count > 0)
            {
                var (current, dist) = queue.Dequeue();

                // highlight kecuali tile asal
                if (current != start)
                {
                    Vector3 worldPos = _tilemap.GetCellCenterWorld(current);
                    Instantiate(_highlight, worldPos, Quaternion.identity, _highlightContainer.transform);
                }

                // kalau sudah sampai jangkauan maksimum, jangan lanjut
                if (dist == moveRange)
                    continue;

                foreach (var d in dirs)
                {
                    var next = current + d;
                    // sudah dikunjungi?
                    if (visited.Contains(next))
                        continue;

                    // hanya tile battle area yang valid (wall = bukan BattleArea)
                    TileBase tile = _tilemap.GetTile(next);
                    if (tile is TileBattleArea)
                    {
                        visited.Add(next);
                        queue.Enqueue((next, dist + 1));
                    }
                }
            }

            // simpan semua cell yang bisa dipijak
            _validMoveCells = visited;

        }
        public bool IsValidMoveCell(Vector3 worldPosition)
        {
            Vector3Int cell = _tilemap.WorldToCell(worldPosition);
            return _validMoveCells.Contains(cell);
        }
        public void HideMoveTile()
        {
            _validMoveCells.Clear();
            foreach (Transform child in _highlightContainer.transform)
                Destroy(child.gameObject);
        }

        public void ShowAttackTile(UnitModel unitModel)
        {
            _validAttackCells.Clear();
            // bersihkan dulu instance lama
            foreach (Transform child in _attackHighlightContainer.transform)
                Destroy(child.gameObject);

            Vector3Int origin = unitModel.Coordinates;
            int range = unitModel.UnitData.Range;
            var pattern = unitModel.UnitData.AttackPattern;
            var direction =  unitModel.UnitData.Direction;
            var offsets = UnitAttackCalculate.GetOffsets(
                pattern, range, direction
            );

            foreach (var off in offsets)
            {
                Vector3Int cell = origin + off;
                TileBase tile = _tilemap.GetTile(cell);
                // hanya highlight jika memang di dalam battle area
                if (tile is TileBattleArea)
                {
                    _validAttackCells.Add(cell);
                    Vector3 worldPos = _tilemap.GetCellCenterWorld(cell);
                    Instantiate(
                        _attackHighlightPrefab,
                        worldPos,
                        Quaternion.identity,
                        _attackHighlightContainer.transform
                    );
                }
            }
        }
        public bool IsValidAttackCell(Vector3 worldPosition)
        {
            Vector3Int cell = _tilemap.WorldToCell(worldPosition);
            return _validAttackCells.Contains(cell);
        }
        public void HideAttackTile()
        {
            _validAttackCells.Clear();
            foreach (Transform child in _attackHighlightContainer.transform)
                Destroy(child.gameObject);
        }
        public List<Vector3Int> GetValidMoveCells()
        {
            return new List<Vector3Int>(_validMoveCells);
        }

        public List<Vector3> GetValidMoveWorldPositions()
        {
            var worlds = new List<Vector3>();
            foreach (var cell in _validMoveCells)
            {
                // GetCellCenterWorld agar diposisikan di tengah tile
                worlds.Add(_tilemap.GetCellCenterWorld(cell));
            }
            return worlds;
        }
    }
}