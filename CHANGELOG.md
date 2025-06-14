


# 0.4.6-alpha


**Fixes:**

- Visiting a custom map then visiting a vanilla map would prevent the user from leaving the ship regularly by teleporting them back inside. This has now been resolved.
- Check if user is in-game before registering keybinds



# 0.4.5-alpha


**Fixes:**

- Player should no longer be able to die while in a meeting



# 0.4.4-alpha

**Features:**

- Replaced keybinds with InputUtil dependency


**Fixes:**

- Sheriffs should be able to spawn their weapon if they for some reason didn't get one


# 0.4.3-alpha

**Fixes:**

- Removed reliance on in-game kick button icon (which is used for the voting)


# 0.4.2-alpha

**Fixes:**

- Support for LethalLevelLoader (LLL)




# 0.4.1-alpha

**Fixes:**

- Removed Liquidation from the list of vanilla map moons a user can travel to




# 0.4.0-alpha

**Features:**

- Added editable host settings
- If chat is disabled, players can only speak before a round and during a meeting
- Made Office the default map


**Fixes:**

- Destroy map game objects when ship leaves


# 0.3.0-alpha

**Features:**

- You can travel to vanilla moons now
- Display imposter names in chat at the end of every game.
- Completely revamped voting UI
- Having the voting UI open will prevent you from scrolling through your inventory & Looking around
- Players that have voted will be highlighted a different color
- Credits are automatically set to 0 instead of default 60
- Removed uid from being appended to usernames


**Fixes:**

- Players should no longer die in a meeting if the player count gets too high
- Reversed forced name change



# 0.2.0-alpha

**Features:**

- Players can't share the same suit (except the default)
- Carry weight has been reduced for items
- Append uid to the end of usernames

**Fixes:**

- Fixed Monsters dying if sheriff shot them while in a meeting
- Fixed VotingUI bug that would constantly spawn VotingUI gameobjects if a user was using a custom username
- Fixed kill cooldown
- Fixed two handed items preventing users from going to the ship
- Stopped player from getting stuck on certain ship objects


# 0.1.1-alpha

- Fixed game stil allowing bodies and meeting to be called when a game is in the process of ending
- Fixed individual meeting numbers not reseting when a game ends or if a user quits the game