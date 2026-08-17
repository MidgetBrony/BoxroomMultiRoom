using SteamShelf.Rooms;
using SteamShelf.Save;
using System;
using UnityEngine;

namespace BoxroomMultiRoom
{
    internal static class DoorRuntime
    {
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

        public static Vector3 GetSpawnPosition(
            RoomDataManager room,
            DoorEndpoint endpoint)
        {
            if (room == null || endpoint == null)
                return new Vector3(16f, 1.1f, 15f);

            Vector2Int tile = new Vector2Int(endpoint.X, endpoint.Y);

            // The saved Door.Position points at the tile owning the real door prefab.
            // Start from the tile center and move slightly away from the edge.
            Vector2Int facingDirection =
                GridUtility.FacingToDirection(endpoint.Facing);

            Vector3 candidate = new Vector3(tile.x, 1.1f, tile.y) -
                                new Vector3(facingDirection.x, 0f, facingDirection.y) * 0.65f;

            // If that tile disappeared, use the game's own safe-reset fallback.
            if (!room.InBounds(tile) || room.GetRoomID(tile) == 0)
                return room.FindValidPlayerResetPosition();

            return candidate;
        }

        public static float GetArrivalYaw(Facing facing)
        {
            Vector2Int direction = GridUtility.FacingToDirection(facing);

            // Look away from the door after arriving.
            Vector3 forward = new Vector3(-direction.x, 0f, -direction.y);
            if (forward.sqrMagnitude < 0.01f)
                return 0f;

            return Quaternion.LookRotation(forward, Vector3.up).eulerAngles.y;
        }
    }
}
