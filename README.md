# BoxroomMultiRoom

Connect doors between separate BOXROOM save slots and turn multiple rooms into one larger, explorable space.

BoxroomMultiRoom is a MelonLoader mod for **BOXROOM**. A linked door loads its destination room, places you beside the matching door, and turns you to face into the room. The linking menu uses BOXROOM's visual style so it feels at home beside the game's existing interface.

## Features

- Link a door in one save slot to a door in another save slot.
- Create two-way links for travel in both directions.
- Create one-way links when a return route is not wanted.
- Replace an existing link by linking its source door again.
- Preserve ordinary door behavior for doors that have not been linked.
- Save links automatically between game sessions.
- Use a room-selection interface styled after BOXROOM's own menus.

## Requirements

- BOXROOM for Windows
- MelonLoader installed for BOXROOM
- `BoxroomMultiRoom.dll`

## Installation

1. Install MelonLoader for BOXROOM if it is not already installed.
2. Close the game.
3. Copy `BoxroomMultiRoom.dll` into BOXROOM's `Mods` folder.

   A typical installation looks like:

   ```text
   BOXROOM/
   ├── Mods/
   │   └── BoxroomMultiRoom.dll
   └── BOXROOM.exe
   ```

4. Start BOXROOM.

The MelonLoader console should report that **Boxroom MultiRoom** initialized successfully.

## Linking two doors

1. Enter the room containing the first door.
2. Hold **Shift** and click the door.
3. In the MultiRoom menu, choose whether **Two-Way** is enabled.
4. Select the destination room.
5. Wait for that room to load.
6. Hold **Shift** and click the destination door.

The link is saved immediately. If **Two-Way** was enabled, either door can now be used to travel between the rooms.

Press **Escape** or choose **Cancel** to stop linking without making a change.

## Using a linked door

Click a linked door normally. BOXROOM will load the connected save slot and place you just inside the destination door.

Doors without a MultiRoom link continue to open normally.

## One-way and two-way links

### Two-way

With **Two-Way** enabled, the source and destination doors form a returnable connection:

```text
Room A door  ⇄  Room B door
```

### One-way

With **Two-Way** disabled, only the source door performs the room transition:

```text
Room A door  →  Room B door
```

The destination door keeps its existing behavior unless it has another link of its own.

## Replacing a link

To point a linked source door somewhere else, repeat the normal linking process from that door and choose a new destination. The old source link is replaced automatically.

When creating a two-way link, an existing source link on the destination door is also replaced so the new pair remains unambiguous.

## Link data and backups

Door links are stored separately from BOXROOM's room files in:

```text
<BOXROOM persistent data>/Saves/rooms_link.json
```

The mod creates this file automatically. Back up `rooms_link.json` along with your BOXROOM saves if you want to preserve the connections between rooms.

Each door is identified by its save-slot number, grid position, and facing direction. Moving, rotating, deleting, or replacing a linked door may make the saved connection refer to the old door location. Relink the door after changing the room layout.

## Troubleshooting

### Shift-click does nothing

- Confirm that `BoxroomMultiRoom.dll` is in the game's `Mods` folder.
- Check the MelonLoader console for a **MultiRoom initialized** message.
- Make sure you are Shift-clicking an actual placed door.

### The destination room is not listed

Only existing save slots other than the current room are shown. Create or save another room, then open the linking menu again.

### A linked door opens normally

The door may have been moved or rotated since it was linked. Create the link again from its current position.

### Travel loads the room but places the player somewhere else

If the saved destination tile no longer exists, MultiRoom uses BOXROOM's safe player-reset position instead. Restore the destination door or relink it at its new location.

### A room takes too long to load

MultiRoom waits up to 30 seconds for BOXROOM's main room scene and player controller. Check the MelonLoader console for loading or timeout errors.

## Building from source

The project targets `.NET Standard 2.1` and references the managed assemblies from a local BOXROOM installation.

1. Clone or download the source.
2. Open `Directory.Build.props` and set `GamePath` to your BOXROOM installation directory.
3. Build `BoxroomMultiRoom.csproj`.

   ```powershell
   dotnet build BoxroomMultiRoom.csproj -c Debug
   ```

The project currently copies the compiled DLL into `<GamePath>/Mods` after a successful build.

## Current limitations

- Room travel requires a scene reload; connected rooms are not loaded simultaneously.
- Linking is currently performed with mouse and keyboard using **Shift-click**.
- There is no in-game link-management list yet. Existing source links are changed by linking the door again.

## How it works

BoxroomMultiRoom records the source and destination save slot, door grid position, and facing direction. When a linked door is used, the mod switches BOXROOM to the target save slot, loads the main scene, waits for the room and player to become available, and then places the player beside the destination door.

Unlinked interactions are passed back to BOXROOM unchanged.
