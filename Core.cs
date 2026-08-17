using MelonLoader;
using SteamShelf;
using SteamShelf.Save;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

[assembly: MelonInfo(typeof(BoxroomMultiRoom.Core), "Boxroom MultiRoom", "1.1.0", "MidgetBrony")]
[assembly: MelonGame(null, "BOXROOM")]

namespace BoxroomMultiRoom
{
    public class Core : MelonMod
    {
        internal static Core Instance { get; private set; }

        public override void OnInitializeMelon()
        {

            Instance = this;
            DoorLinkManager.Initialize();
            LoggerInstance.Msg($"MultiRoom initialized. Link file: {DoorLinkManager.LinkPath}");
        }


        public override void OnGUI()
        {
            LinkMenu.Draw();
        }

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

                // SetSlot already saves the previous active slot when Main is active.
                SaveManager.Instance.SetSlot(slot);
                SceneLoader.LoadMainScene();
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"[MultiRoom] Failed to load Slot_{slot}: {ex}");
                PendingTeleport.Clear();
            }
        }

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