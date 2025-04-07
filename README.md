# More Players! v0.9.2-Alpha

<p align="center">
  <img alt="Placeholder Image" src="https://raw.githubusercontent.com/DeveloperBlue/ATTMorePlayers/refs/heads/main/previews/3.jpg" width="32%">
&nbsp;
  <img alt="Placeholder Image" src="https://raw.githubusercontent.com/DeveloperBlue/ATTMorePlayers/refs/heads/main/previews/3.jpg" width="32%">
  &nbsp;
  <img alt="Placeholder Image" src="https://raw.githubusercontent.com/DeveloperBlue/ATTMorePlayers/refs/heads/main/previews/3.jpg" width="32%">
</p>

----

<h1 color="red">🔥 Hotpatch for the 1.4.2 / Inn Update 🔥</h1>

I have pushed out a hotpatch for the 1.4.2 / Inn Update! These patches should fix picking up items, interacting with the inventory, duplicated items, errors with the player list, and fixes game saving!

Thank you all for your patience, and especially thank you all for the **bug reports** and **support**!

If you continue to run into issues, please [report your issues here](https://github.com/DeveloperBlue/ATTMorePlayers/issues).

- 4/7/2025

----

This is an **extremely experimental** mod that patches in support for up to **8** players.

<h3>⭐ All players <b>MUST</b> have this mod installed.</h3>

Clients that try to join without the mod installed may not see assets and can cause issues for the host.

Please wait for the host to fully load in before attempting to join. 

Tested with 6 players, playing for about 4 hours, up to the Herbalist quest on version <b>1.0.29</b>.
<br><br>
<p align="left">
    <a href="https://buymeacoffee.com/michaelrooplall" target="_blank"><img src="https://www.buymeacoffee.com/assets/img/custom_images/orange_img.png" alt="Buy Me A Coffee" style="height: 41px !important;width: 174px !important;box-shadow: 0px 3px 2px 0px rgba(190, 190, 190, 0.5) !important;-webkit-box-shadow: 0px 3px 2px 0px rgba(190, 190, 190, 0.5) !important;" ></a>
</p>

## Installation

The only requirement for this mod is [BepInEx](https://thunderstore.io/c/ale-and-tale-tavern/p/BepInEx/BepInExPack/).

### Mod Manager (Recommended)
I personally recommend using the modmanager so you can get any patches and fixes as they come out.

- Install this mod directly in r2modman or the Thunderstore Launcher by clicking the big blue "Install with Mod Manager" button above.

### Manual Installation

- To manually install this mod, download and unzip the file. Place the folder in your {game}/BepInEx/plugins folder.

## Notes

By default, the game includes some scaling. More creatures will spawn outside the tavern, and raids can scale larger with more players. Things like the boss health and hp recovery also scale.

Saves created using this mod are most likely NOT backwards compatible if you remove the mod. If you wish to uninstall the mod and load up the world in vanilla, you need to edit the save file to remove the inventories for players 5 through 8. The vanilla game crashes trying to load them in to slots that do not exist.

This mod was created for fun in my spare time. It's also my first Unity mod.

## Patches

⚠️ Patches, also known as "Places the mod can break" ⚠️

- Inventory (Tested, working well)
  - Creates inventories (and wall backpacks) for players 5-8.
  - Inventories can be accessed when the players are offline, just like vanilla.
  - Inventories are "remembered", so players always rejoin with the right inventory.
- PlayerManager (Tested with 6 players, working well)
  - Adds more "slots" for players
  - Modifies the SteamNetManager to accept up to 8 players in the lobby.
  - Reuses players 1-4 spawn points for players 5-8.
    - You may be able to do a funny with everyone joining at the same time and getting squished into the same spawn locations.
- UI
  - ~~Adds more player slots in the Player List Menu for players 5 through 8~~. (REMOVED: The vanilla game now creates 8 player list items on its own)
    - Voice chat still works. Volumes are adjustable between all players.
    - Host can still kick/ban additional players
  - Changes the New Game menu to say "BiggerTavern" instead of just "Tavern", as a way to tell if the mod is loaded or not.
- RecipeManager
  - Patches the EXP multiplier. (originally: more players = less exp, changes: caps exp drop to be the same as 4 players).
- Quests
  - Modifies code for the Herbalist Quest. (Tested once, seems fine)
    - The max amount of bears that can spawn is numberPlayers/2. Adds more spawn slots, so up to 4 bears can spawn.
  - Modifies code for the Defense Quest. **(Untested)**
    - Adjusts how quickly the waves spawn. (originally: more players = faster, changes: caps at the same max speed as 4 players)
  - Cave Quest **(COMPLETELY UNTESTED)**
    - Tries to only spawn as many creatures as the game has spawn points.
    - Tries to only spawn as much loot as the game has spawn points.
    - Tries to only spawn as many chests as the game has spawn points.
      - e.g. if there are only 8 spots for a chest to spawn, and we try to spawn a 9th chest, the game would crash.
  - 1.4.2 / Inn Update **(Fixes bugs [#2](https://github.com/DeveloperBlue/ATTMorePlayers/issues/2) [#3](https://github.com/DeveloperBlue/ATTMorePlayers/issues/3), not tested with multiple players)**
    - This version and versions after have only been lightly tested for bug fixes.
    - Items can now be picked up
    - Interacting with an inventory item no longer duplicates it
    - Fixes saving
    - No longer creates Player List Items for players 5-8 as the vanilla game now handles that internally

## To Do
- Play the full game and test all quests up tho the Cave Quest
- Server mod "enforcement", to ensure players without the mod can't join the lobby (and break the mod).
- ✔️ Scroll bar in the Player List menu (Thank you game developers)

## Issues
[Please report any issues you encounter.](https://github.com/DeveloperBlue/ATTMorePlayers/issues)
If you are able to include logs, crash reports, or information on what you were doing that caused the issue, please provide it!

----

| Links    |  |
| -------- | ------- |
| GitHub  | https://github.com/DeveloperBlue/ATTMorePlayers/ |
| Thunderstore | https://thunderstore.io/c/ale-and-tale-tavern/p/DeveloperBlue/MorePlayers/ |
