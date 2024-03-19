using System.Collections.Generic;

namespace RPG.Core.Shared.Campaign
{
    public class AreaState
    {
        public struct TilePointer
        {
            public string tilemap;
            public (int, int, int) location;
        }

        private Dictionary<string, Dictionary<(int, int, int), AreaStateData.ITileState>> tilemapStates = new Dictionary<string, Dictionary<(int, int, int), AreaStateData.ITileState>>();

        public AreaStateData.ITileState RegisterTileState(TilePointer ptr, AreaStateData.ITileState state)
        {
            if (! tilemapStates.ContainsKey(ptr.tilemap))
                tilemapStates[ptr.tilemap] = new Dictionary<(int, int, int), AreaStateData.ITileState>();
            var _mapState = tilemapStates[ptr.tilemap];
            if (! _mapState.ContainsKey(ptr.location))
                _mapState[ptr.location] = state;
            return _mapState[ptr.location];
        }

        public AreaStateData.ITileState GetTileState(TilePointer ptr)
        {
            if (! tilemapStates.ContainsKey(ptr.tilemap))
                return null;
            var _mapState = tilemapStates[ptr.tilemap];
            if (! _mapState.ContainsKey(ptr.location))
                return null;
            return _mapState[ptr.location];
        }

        public void SetTileState(TilePointer ptr, AreaStateData.ITileState state)
        {
            if (!tilemapStates.ContainsKey(ptr.tilemap))
                tilemapStates[ptr.tilemap] = new Dictionary<(int, int, int), AreaStateData.ITileState>();
            tilemapStates[ptr.tilemap][ptr.location] = state;
        }
    }
}