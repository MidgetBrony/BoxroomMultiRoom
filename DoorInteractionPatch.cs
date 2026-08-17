using HarmonyLib;
using MelonLoader;
using SteamShelf.Save;
using UnityEngine;

namespace BoxroomMultiRoom
{
    /// <summary>
    /// Harmony prefix for BOXROOM's door interaction. Returning true lets the
    /// original game method run; returning false replaces that interaction.
    /// </summary>
    [HarmonyPatch(typeof(Interactable_door), nameof(Interactable_door.OnInteract))]
    internal static class DoorInteractionPatch
    {
        /// <summary>
        /// Routes Shift-clicks into link creation and ordinary clicks into linked
        /// travel. Doors that MultiRoom cannot resolve remain entirely game-owned.
        /// </summary>
        private static bool Prefix(Interactable_door __instance)
        {
            // Harmony injects the Interactable_door instance through __instance.
            if (!DoorRuntime.TryResolve(__instance, out DoorEndpoint currentDoor))
            {
                return true;
            }

            bool shiftHeld =
                Input.GetKey(KeyCode.LeftShift) ||
                Input.GetKey(KeyCode.RightShift);

            if (shiftHeld)
            {
                HandleShiftClick(currentDoor);
                return false;
            }

            // Returning true is important: unlinked doors retain vanilla behavior.
            if (!DoorLinkManager.TryGetTarget(currentDoor, out DoorEndpoint target))
            {
                return true;
            }

            MelonLogger.Msg(
                $"[MultiRoom] Door link triggered: {currentDoor.Key} -> {target.Key}");

            PendingTeleport.Set(target);
            Core.LoadSlot(target.Slot);

            // Do not animate a door that is about to be unloaded.
            return false;
        }

        /// <summary>
        /// Implements a two-step interaction across scene loads: first remember the
        /// source door, then accept the destination door after its room is loaded.
        /// </summary>
        private static void HandleShiftClick(DoorEndpoint clickedDoor)
        {
            if (!PendingDoorLink.HasSource)
            {
                LinkMenu.OpenForSource(clickedDoor);
                return;
            }

            DoorEndpoint source = PendingDoorLink.Source;

            if (source.Key == clickedDoor.Key)
            {
                LinkMenu.ShowMessage("That is the same door. Select another door.");
                return;
            }

            DoorLinkManager.AddOrReplace(
                source,
                clickedDoor,
                LinkMenu.CreateTwoWayLink);

            PendingDoorLink.Clear();

            LinkMenu.ShowMessage(
                $"Linked Slot_{source.Slot} door ({source.X},{source.Y}) " +
                $"to Slot_{clickedDoor.Slot} door ({clickedDoor.X},{clickedDoor.Y}).");
        }
    }
}
