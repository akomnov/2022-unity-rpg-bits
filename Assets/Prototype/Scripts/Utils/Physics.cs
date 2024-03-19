using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

namespace RPG.Utils
{
    public static class Physics
    {
        public static HashSet<(Tilemap tilemap, Vector3Int location)> GetColliderTiles(UnityEngine.Collision2D collision)
        {
            var _tilemap = collision.collider.GetComponent<Tilemap>();
            if (_tilemap == null) return null;
            var _result = new HashSet<(Tilemap tilemap, Vector3Int location)>();
            for (int _idx = collision.contactCount - 1; _idx >= 0; --_idx)
            {
                var _contact = collision.GetContact(_idx);
                _result.Add(
                    (
                        tilemap: _tilemap,
                        location: _tilemap.layoutGrid.WorldToCell(
                            _contact.point - _contact.normal * 0.01f
                        )
                    )
                );
            }
            return _result;
        }
    }
}