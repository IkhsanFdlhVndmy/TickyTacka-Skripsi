using System.Collections.Generic;
using Player;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace TilemapLayer
{
    public class UnitModel
    {
        public Tilemap ParentTilemap { get; private set; }
        public UnitData UnitData { get; private set; }
        public GameObject GameObject { get; private set; }
        public Vector3Int Coordinates { get; private set; }
        public UnitController UnitController { get; private set; }
        public Vector3 WorldCoords { get; private set; }
        public bool IsDead { get; private set; }
        public bool IsItem { get; private set; }
        public TileItemSpawn ItemData { get; private set; }
        public UnitModel(Tilemap parentTilemap, UnitData unitData, GameObject unitObject, Vector3Int coords, Vector3 worldCoords,TileItemSpawn itemData = null)
        {
            ParentTilemap = parentTilemap;
            UnitData = unitData;
            GameObject = unitObject;
            Coordinates = coords;
            WorldCoords = worldCoords;
            UnitController = unitObject.GetComponent<UnitController>();
            IsDead = false;
            IsItem = false;
            if (itemData)
            {
                IsItem = true;
                ItemData = itemData;
            }
        }

        public void ChangeStatus(bool value)
        {
            IsDead = value;
        }

    }
}