# Introduction
This mod for the game **From the Depths** adds several improvements and new features and a new gamemode to the Adventure, including:

- Allowing enemies to spawn much closer or further away
- Enabling fortress spawns (designed for use with the Adventure Randomizer)  
- Scaling resource zones based on difficulty  
- Customizing the adventure bell’s cooldown and allowing it to ignore altitude
- Allowing the adventureraft to die (still not recommended to deconstruct it since it can still break runs, unsure why.)
- Allowing to freeze the vehicle using caps lock unless enemies are nearby
- Allowing Rambot to live and shoot without a Heartstone
- Allowing Godmode for Rambot  
- Sandboxing options, made for debugging and testing purposes, which include:  
Sending you through a Blue/Red/Green Portal  
Spawning an enemy and changing the difficulty of spawns or despawning all enemies
Blocking naturally occuring enemy spawns(Bell and Forced spawns are not blocked intentionally.)

# Quick saves
You can create a quicksave of the current adventure within the quicksaves tab in options. The quicksave can then be restored into a free adventure save slot and will be entirely seperate from the original run, allowing it to take a different course.

# A Variety of challenges:
## Spawn enemies on a timer
After a grace period of up to 30 minutes, enemies will be spawned every X (configurable: 5-600) seconds.
## Dynamic difficulty Scaling
The difficulty of spawned enemies is determined by the player teams' combined cost instead of the Warp Difficulty.
## Auto Increase Difficulty (thanks for the suggestion Ninjin)
After a grace period of up to 30 minutes, the difficulty will increase every X (configurable: 180-1800) seconds.

# Wave Defense mode
An entirely new gamemode within adventure, featuring rapid spawns in waves. The wave duration and "Danger Level"(difficulty was already used, sorry) can be changed. The enemies spawned will scale according to the cost of the players vehicles and the time between spawns is increased for every enemy present. Both the spawndelay and enemy cost are scaled according to the Danger Level. If the wave is won by surviving long enough (without stopping it manually), the player is rewarded in commodities and all player craft are repaired if you can afford it. There are two keybindings, one for starting(enter) and one for stopping waves(f8), which are rebindable in the options. On wave start, a quick save is created to allow rolling back to before the wave started. The gamemode should work on all planets/adventures.
Danger Levels below 5 are intended to be easy, 5 average, and above should get **really** hard. The wavecount is only for decoration at the moment until i figure out a proper way to scale it.

The values and balance are subject to change and i hope to receive feedback/suggestions.

# General Changes
-  Settings are saved into the run and restored when loading the save. Exiting back to the menu restores the previous settings.
-  In multiplayer, the allied craft are not repaired on wave end in wavemode
-  In multiplayer, the difficulty increases in the challenge mode have to respawn all enemies and resource zones are converted into commodities.
-  In wavemode, enemies are spawned according to their cost relative to the player team cost instead of using the wave difficulty system.
-  Bonus starting material can be assigned in wavemode to allow a steeper takeoff (and doesn't affect balance since enemies are scaled relative to player cost)
-  Early game in wavemode(below ~150k) most rammers, nukes and fast fliers are excluded from spawning
-  Early game in wavemode(until ~250k) two or even three enemies may spawn at a time. Also looking for feedback on expanding, removing or tuning that.

You can customize the behavior of each feature or disable them entirely through the options menu.

Changes should apply immediately.

Multiplayer is supported as long as the host has the mod installed. For clients many settings are missing since most of them only work as a host.

Feedback, bug reports and feature suggestions are always appreciated!

Thanks to Melezhor and Cookie for extensive helping with tests and Rinathunderpanzer for helping me figure out how to move the settings into their own ftd options tab.

