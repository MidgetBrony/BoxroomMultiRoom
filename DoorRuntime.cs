using SteamShelf.Rooms;
using SteamShelf.Save;
using System;
using UnityEngine;

namespace BoxroomMultiRoom
{
    /// <summary>
    /// Adapts BOXROOM's live room objects to MultiRoom's serializable door model and
    /// converts saved endpoints back into safe player transforms.
    /// </summary>
    internal static class DoorRuntime
    {
        /// <summary>
        /// Finds the Door record that owns an interacted component. BOXROOM stores
        /// doors as edge objects on room blocks, so component identity must be mapped
        /// back to a slot, tile coordinate, and facing before it can be persisted.
        /// </summary>
        public static bool TryResolve(
            Interactable_door interactable,
            out DoorEndpoint endpoint)
        {
            endpoint = null;

            if (interactable == null ||
                SaveManager.Instance == null ||
                RoomDataManager.Instance == null)
            {
                return false;
            }

            RoomDataManager room = RoomDataManager.Instance;

            foreach (Door door in room.Doors)
            {
                if (door == null)
                    continue;

                if (!room.TryGetBlock(door.Position, out RoomBlock block))
                    continue;

                // Facing is the index into BOXROOM's four edge-object slots.
                GameObject edgeObject = block.EdgeObjects[(int)door.Facing];
                if (edgeObject == null)
                    continue;

                Interactable_door edgeInteractable =
                    edgeObject.GetComponentInChildren<Interactable_door>(true);

                if (edgeInteractable == interactable)
                {
                    endpoint = new DoorEndpoint(
                        SaveManager.Instance.CurrentSlot,
                        door.Position.x,
                        door.Position.y,
                        door.Facing);

                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Computes a point just inside the destination door. Stale endpoints fall
        /// back to BOXROOM's own valid reset position instead of stranding the player.
        /// </summary>
        public static Vector3 GetSpawnPosition(
            RoomDataManager room,
            DoorEndpoint endpoint)
        {
            if (room == null || endpoint == null)
                return new Vector3(16f, 1.1f, 15f);

            Vector2Int tile = new Vector2Int(endpoint.X, endpoint.Y);

            // Spawn inside the owning tile, slightly away from the door edge.
            Vector2Int facingDirection =
                GridUtility.FacingToDirection(endpoint.Facing);

            Vector3 candidate = new Vector3(tile.x, 1.1f, tile.y) -
                                new Vector3(facingDirection.x, 0f, facingDirection.y) * 0.65f;

            // A moved or deleted door may leave a stale endpoint.
            if (!room.InBounds(tile) || room.GetRoomID(tile) == 0)
                return room.FindValidPlayerResetPosition();

            return candidate;
        }

        /// <summary>Returns a yaw that faces away from the wall and into the room.</summary>
        public static float GetArrivalYaw(Facing facing)
        {
            Vector2Int direction = GridUtility.FacingToDirection(facing);

            // Face into the destination room.
            Vector3 forward = new Vector3(-direction.x, 0f, -direction.y);
            if (forward.sqrMagnitude < 0.01f)
                return 0f;

            return Quaternion.LookRotation(forward, Vector3.up).eulerAngles.y;
        }
    }
}
