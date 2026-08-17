using HarmonyLib;
using MelonLoader;
using SteamShelf.Save;
using UnityEngine;

namespace BoxroomMultiRoom
{
    [HarmonyPatch(typeof(Interactable_door), nameof(Interactable_door.OnInteract))]
    internal static class DoorInteractionPatch
    {
        private static bool Prefix(Interactable_door __instance)
        {
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

            if (!DoorLinkManager.TryGetTarget(currentDoor, out DoorEndpoint target))
            {
                return true;
            }

            MelonLogger.Msg(
                $"[MultiRoom] Door link triggered: {currentDoor.Key} -> {target.Key}");

            PendingTeleport.Set(target);
            Core.LoadSlot(target.Slot);

            // Suppress the ordinary swing because the scene is transitioning.
            return false;
        }

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
