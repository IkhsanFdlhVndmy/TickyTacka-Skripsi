using Player;
using UnityEngine;

namespace TilemapLayer
{
    public class PreviewTilemap: TilemapLayer
    {
        [SerializeField] private SpriteRenderer _previewRenderer;
        public void ShowPreview(UnitModel item, Vector3 worldCoords, bool isValid)
        {
            Vector3Int coords = _tilemap.WorldToCell(worldCoords);
            Vector3 previewPosition = _tilemap.CellToWorld(coords) + new Vector3(1 / 2f, 1 / 2f);
            _previewRenderer.enabled = true;
            _previewRenderer.sprite = item.UnitData.UnitSprite;
            _previewRenderer.transform.position = previewPosition; 
            _previewRenderer.color = isValid ? new Color(0, 1, 0, 0.5f) : new Color(1, 0, 0, 0.5f);
        }

        public void ClearPreview()
        {
            _previewRenderer.enabled = false;
        }
    }
}