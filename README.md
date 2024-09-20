# More Players!

<p align="center">
  <img alt="shot from the second floor looking down" src="https://raw.githubusercontent.com/DeveloperBlue/ATTMorePlayers/refs/heads/main/previews/3.jpg" width="32%">
&nbsp;
  <img alt="shot from the base of the stairs looking up" src="https://raw.githubusercontent.com/DeveloperBlue/ATTMorePlayers/refs/heads/main/previews/3.jpg" width="32%">
  &nbsp;
  <img alt="shot from the base of the stairs looking up" src="https://raw.githubusercontent.com/DeveloperBlue/ATTMorePlayers/refs/heads/main/previews/3.jpg" width="32%">
</p>

----

This is an **extremely experimental** mod that patches in support for up to **8** players. 

⭐ All players **MUST** have this mod installed. ⭐

Any clients that try to join without the mod installed may not see assets and can cause issues for the host. Please wait for the host to fully load in before attempting to join. 

This mod was tested with 6 players, playing for 4+ hours, up to the Herbalist quest.

<p align="left">
    <a href="https://buymeacoffee.com/michaelrooplall" target="_blank"><img src="https://www.buymeacoffee.com/assets/img/custom_images/orange_img.png" alt="Buy Me A Coffee" style="height: 41px !important;width: 174px !important;box-shadow: 0px 3px 2px 0px rgba(190, 190, 190, 0.5) !important;-webkit-box-shadow: 0px 3px 2px 0px rgba(190, 190, 190, 0.5) !important;" ></a>
</p>

## Installation

The only requirement for this mod is [BepInEx](https://thunderstore.io/c/ale-and-tale-tavern/p/BepInEx/BepInExPack/).

### Mod Manager (Recommended)

- Install this mod directly in r2modman or the Thunderstore Launcher by clicking the big blue "Install with Mod Manager" button above.
  
### Manual Installation

- To manually install this mod, download and unzip the file. Place the folder in your {game}/BepInEx/plugins folder.

## Notes

By default, the game includes some scaling. More creatures will spawn outside the tavern, and raids can scale larger with more players. Things like the boss health and hp recovery also scale.

Taverns created using this mod are most likely NOT backwards compatible when loaded without the mod. If you wish to revert your eight player world back to a vanilla world, you need to edit the save file to remove the inventories for players 5 through 8. The vanilla game crashes trying to load them in to slots that do not exist.

This mod was created for fun in my spare time.

## Patches

Patches, also known as "Places the mod can break" ⚠️

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
  - Adds more player slots in the Player List Menu for players 5 through 8. (Scroll bar coming soon)
    - Voice chat still works. Volumes are adjustable between all players.
    - Host can still kick/ban additional players
  - Changes the New Game menu to say "BiggerTavern" instead of just "Tavern", as a way to tell if the mod is loaded or not.
- RecipeManager
  - Patches the EXP multiplier. (originally: more players = less exp, changes: caps exp drop to be the same as 4 players).
- Quests
  - Modifies code for the Herbalist Quest. (Tested once, seems fine)
    - The max amount of bears that can spawn is numberPlayers/2. Adds more spawn slots, so up to 4 bears can spawn.
  - Modifies code for the Defense Quest. (Untested)
    - Adjusts how quickly the waves spawn. (originally: more players = faster, changes: caps at the same max speed as 4 players)
  - Cave Quest (COMPLETELY UNTESTED)
    - Tries to only spawn as many creatures as the game has spawn points.
    - Tries to only spawn as much loot as the game has spawn points.
    - Tries to only spawn as many chests as the game has spawn points.
      - e.g. if there are only 8 spots for a chest to spawn, and we try to spawn a 9th chest, the game would crash.


## Known Issues
- No major issues found so far. But I'm sure there's plenty. No one's code should work on the first try like this. It's unholy.

----

| Links    |  |
| -------- | ------- |
| GitHub  | https://github.com/DeveloperBlue/ATTMorePlayers/ |
| Thunderstore | https://thunderstore.io/c/ale-and-tale-tavern/p/DeveloperBlue/MorePlayers/ |
