using MelonLoader;
using SteamShelf;
using SteamShelf.Save;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

[assembly: MelonInfo(typeof(BoxroomMultiRoom.Core), "Boxroom MultiRoom", "1.2.0", "MidgetBrony")]
[assembly: MelonGame(null, "BOXROOM")]

namespace BoxroomMultiRoom
{
    /// <summary>
    /// MelonLoader entry point. It owns the mod lifecycle and performs save-slot
    /// transitions requested by linked doors.
    /// </summary>
    public class Core : MelonMod
    {
        internal static Core Instance { get; private set; }

        /// <summary>
        /// Loads persisted links once MelonLoader has initialized the mod.
        /// </summary>
        public override void OnInitializeMelon()
        {
            Instance = this;
            DoorLinkManager.Initialize();
            LoggerInstance.Msg($"MultiRoom initialized. Link file: {DoorLinkManager.LinkPath}");
        }

        /// <summary>
        /// Changes BOXROOM's active save slot and reloads its main scene.
        /// Pending cross-scene state must be stored outside scene objects because
        /// Unity destroys those objects during the reload.
        /// </summary>
        internal static void LoadSlot(int slot)
        {
            try
            {
                if (SaveManager.Instance == null)
                {
                    MelonLogger.Warning("[MultiRoom] SaveManager is not available.");
                    return;
                }

                if (SaveManager.Instance.CurrentSlot == slot)
                {
                    MelonLogger.Warning($"[MultiRoom] Already in Slot_{slot}.");
                    return;
                }

                MelonLogger.Msg($"[MultiRoom] Loading Slot_{slot}...");

                // SetSlot saves the room being left before changing slots.
                SaveManager.Instance.SetSlot(slot);
                SceneLoader.LoadMainScene();
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"[MultiRoom] Failed to load Slot_{slot}: {ex}");
                PendingTeleport.Clear();
            }
        }

        /// <summary>
        /// Waits for BOXROOM to finish constructing the new room before moving the
        /// player. Checking IsSpawningRoom avoids racing the game's own spawn logic.
        /// </summary>
        private static IEnumerator TeleportAfterRoomLoads()
        {
            const float timeoutSeconds = 30f;
            float started = Time.realtimeSinceStartup;

            while (Time.realtimeSinceStartup - started < timeoutSeconds)
            {
                if (SceneManager.GetActiveScene().name != "Main")
                {
                    yield return null;
                    continue;
                }

                RoomDataManager room = RoomDataManager.Instance;
                FirstPersonController player =
                    UnityEngine.Object.FindAnyObjectByType<FirstPersonController>();

                if (room != null &&
                    player != null &&
                    !room.IsSpawningRoom)
                {
                    DoorEndpoint destination = PendingTeleport.Destination;
                    Vector3 spawn = DoorRuntime.GetSpawnPosition(room, destination);

                    CharacterController controller =
                        player.GetComponent<CharacterController>();

                    // CharacterController rejects direct transform movement while enabled.
                    if (controller != null)
                        controller.enabled = false;

                    player.transform.position = spawn;
                    player.transform.rotation =
                        Quaternion.Euler(0f, DoorRuntime.GetArrivalYaw(destination.Facing), 0f);

                    if (controller != null)
                        controller.enabled = true;

                    MelonLogger.Msg(
                        $"[MultiRoom] Arrived at Slot_{destination.Slot}, " +
                        $"door ({destination.X},{destination.Y}) facing {destination.Facing}.");

                    PendingTeleport.Clear();
                    yield break;
                }

                yield return null;
            }

            MelonLogger.Warning("[MultiRoom] Timed out waiting to place the player.");
            PendingTeleport.Clear();
        }
    }
}
