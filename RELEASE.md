# BoxroomMultiRoom v1.1.0

Turn separate BOXROOM save slots into one connected space by linking their doors together.

BoxroomMultiRoom lets you Shift-click a door, choose another saved room, and Shift-click the destination door to create a permanent connection. Using that door normally will load the linked room and place you beside the destination door.

## Highlights

- Connect doors across separate BOXROOM save slots.
- Create two-way links for travel in either direction.
- Create one-way links for entrances, exits, and secret routes.
- Replace a door's existing destination by linking it again.
- Keep normal BOXROOM behavior on every unlinked door.
- Save all door connections automatically between sessions.
- Use a room picker styled to match BOXROOM's own interface.

## Installation

1. Install [MelonLoader](https://github.com/LavaGang/MelonLoader) for BOXROOM.
2. Download `BoxroomMultiRoom.dll` from the assets below.
3. Copy the DLL into the game's `Mods` folder:

   ```text
   BOXROOM/Mods/BoxroomMultiRoom.dll
   ```

4. Start BOXROOM.

## Linking rooms

1. Enter the room containing the first door.
2. Hold **Shift** and click that door.
3. Choose whether the link should be **Two-Way**.
4. Select the destination room from the menu.
5. After the room loads, hold **Shift** and click its destination door.

The connection is saved immediately.

To cancel while choosing a room, press **Escape** or select **Cancel**.

## Using linked doors

Click a linked door normally to travel through it. BOXROOM loads the destination save slot and places you inside the connected doorway.

Unlinked doors continue to work normally.

## Link data

Connections are stored separately from the room files in:

```text
<BOXROOM persistent data>/Saves/rooms_link.json
```

Back up `rooms_link.json` with your BOXROOM saves if you want to preserve your room connections.

Doors are identified by save slot, grid position, and facing direction. If you move, rotate, replace, or delete a linked door, create the link again from its new position.

## Current limitations

- Connected rooms load one at a time; they do not exist in the same Unity scene.
- Creating links currently requires a mouse and keyboard using **Shift-click**.
- Existing links are replaced by relinking the source door; there is not yet a separate link-management screen.

## For modders

The repository includes fully commented source intended to serve as reference material for:

- MelonLoader lifecycle hooks
- Harmony interaction patches
- Reading BOXROOM room and save-slot data
- Persisting mod data with JSON
- Carrying state across Unity scene loads
- Drawing an in-game IMGUI overlay

## Suggested GitHub release fields

- **Tag:** `v1.1.0`
- **Release title:** `BoxroomMultiRoom v1.1.0`
- **Binary asset:** `BoxroomMultiRoom.dll`

> This is a community-made mod and is not affiliated with the BOXROOM developers.
