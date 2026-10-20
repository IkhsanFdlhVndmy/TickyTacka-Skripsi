using UnityEngine;
using UnityEngine.Tilemaps;

namespace TilemapLayer
{
    public class TilemapLayer:MonoBehaviour
    {
        protected Tilemap _tilemap { get; private set; }
        private void Awake()
        {
            _tilemap = GetComponent<Tilemap>();
        }
    }
}