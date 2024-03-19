using UnityEngine;
using UnityEngine.Tilemaps;

namespace RPG.ScriptableObjects.Tiles
{
    [CreateAssetMenu(fileName = "New Door", menuName = "Tile/Door")]
    public class Door : Tile
    {
        #region interop
        [System.NonSerialized]
        private Managers.PersistentManagers.IClientSequenceManager sequenceManager = null;

        private Managers.PersistentManagers.ClientSequences.CampaignSequence CurrentCampaignSequence {
            get {
#if UNITY_EDITOR
                if (!Application.isPlaying) return null;
#endif
                if (sequenceManager == null)
                    sequenceManager = Managers.PersistentManagers.ClientSequenceManager.Instance;
                return sequenceManager.CampaignSequence;
            }
        }

        public void OnEnable()
        {
#if UNITY_EDITOR
            if (! Application.isPlaying) return;
#endif
            sequenceManager = Managers.PersistentManagers.ClientSequenceManager.Instance;
        }

        public void OnDisable()
        {
            sequenceManager = null;
        }

        #endregion

        private class DoorState : Core.Shared.Campaign.AreaStateData.ITileState
        {
            public bool open = false;

            public void Open()
            {
                open = true;
            }
        }

        public Sprite spriteOpen;

        public bool startsOpen = false;

        public override void GetTileData(
            Vector3Int location,
            ITilemap tileMap,
            ref TileData tileData
        )
        {
            base.GetTileData(location, tileMap, ref tileData);
            tileData.sprite = (
                (
                    (DoorState)(CurrentCampaignSequence?.GetTileState(tileMap, location))
                )?.open ?? startsOpen
            ) ? spriteOpen : sprite;
        }

        public override void RefreshTile(Vector3Int position, ITilemap tilemap)
        {
            tilemap.RefreshTile(position);
        }

        public override bool StartUp(Vector3Int location, ITilemap tilemap, GameObject go)
        {
            CurrentCampaignSequence?.RegisterTileState(
                tilemap,
                location,
                new DoorState { open = startsOpen }
            );
            return true;
        }

        public void Open(Vector3Int position, Tilemap tilemap)
        {
            ((DoorState)(CurrentCampaignSequence?.GetTileState(tilemap, position)))?.Open();
            tilemap.RefreshTile(position);
        }
    }
}