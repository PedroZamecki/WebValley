# WebSocket Commands Documentation

## Overview
This mod provides a simple WebSocket server for controlling Stardew Valley through WebSocket messages. The server runs on `http://localhost:8080/` by default and processes text-based commands.

## Message Format
Commands are sent as plain text strings with two main categories:

### Info Queries (Read-only)
```
info:{info_name}
```

### Game Commands (Write operations - implementation pending)
```
command:{command_name}:{arguments separated with ;}
```

## Available Commands

### `ping`
Test connectivity with the server.
- **Request:** `ping`
- **Response:** `pong`

## Info Commands

Info commands are read-only queries about the game state. Format: `info:{info_name}`

### `info:time`
Get the current in-game time.
- **Request:** `info:time`
- **Response:** `time:1200` (where 1200 is the time of day in 24-hour format)
- **Error:** `error:world_not_ready` if no save is loaded

### `info:money`
Get the player's current money.
- **Request:** `info:money`
- **Response:** `money:5000`
- **Error:** `error:world_not_ready` if no save is loaded

### `info:name`
Get the player's name.
- **Request:** `info:name`
- **Response:** `name:Farmer`
- **Error:** `error:world_not_ready` if no save is loaded

### `info:health`
Get the player's current and max health.
- **Request:** `info:health`
- **Response:** `health:20/20`
- **Error:** `error:world_not_ready` if no save is loaded

### `info:energy`
Get the player's current and max stamina/energy.
- **Request:** `info:energy`
- **Response:** `energy:100/270`
- **Error:** `error:world_not_ready` if no save is loaded

### `info:level` or `info:level:skill`
Get the player's skill level(s).
- **Request (all skills):** `info:level`
- **Response:** `level:farming:5,fishing:3,foraging:2,mining:4,combat:1`
- **Request (single skill):** `info:level:farming`
- **Response:** `level:farming:5`
- **Valid skills:** farming, fishing, foraging, mining, combat
- **Error:** `error:unknown_skill` if an invalid skill is requested

### `help`
Get a list of available commands.
- **Request:** `help`
- **Response:** `available_commands:ping,info:{time|money|name|level|level:skill|health|energy},command:{name}:{args},help`

## Game Commands

Game commands are for executing actions in the game. Format: `command:{command_name}:{arguments separated with ;}`

**Status:** Currently not implemented. Reserved for future use.

**Example format (when implemented):**
```
command:increase_speed:amount=10;duration=10000
```

## Error Responses
All errors follow the format: `error:error_reason`
- `error:empty_message` - Empty message received
- `error:unknown_command:COMMAND` - Unknown command
- `error:missing_info_type` - Info command without specifying info type
- `error:missing_command_name` - Game command without command name
- `error:unknown_info_type:TYPE` - Unknown info type requested
- `error:world_not_ready` - Game world is not ready (no save loaded)
- `error:unknown_skill` - Invalid skill name for level info
- `error:game_commands_not_implemented` - Game commands are not yet implemented

## Example Usage (JavaScript)

```javascript
const ws = new WebSocket('ws://localhost:8080/');

ws.onopen = () => {
    // Test connection
    ws.send('ping');
};

ws.onmessage = (event) => {
    console.log('Response:', event.data);

    // Parse info responses
    if (event.data.startsWith('money:')) {
        const money = event.data.split(':')[1];
        console.log('Player has:', money, 'gold');
    }

    if (event.data.startsWith('time:')) {
        const time = event.data.split(':')[1];
        console.log('Current time:', time);
    }

    if (event.data.startsWith('level:')) {
        const levels = event.data.split(':')[1];
        console.log('Levels:', levels);
    }
};

ws.onerror = (error) => {
    console.error('WebSocket error:', error);
};

// Example: Get player info
function getPlayerInfo() {
    ws.send('info:name');      // Get player name
    ws.send('info:money');     // Get player money
    ws.send('info:health');    // Get player health
    ws.send('info:level');     // Get all skill levels
    ws.send('info:level:farming'); // Get specific skill level
}
```

## Server Lifecycle
- The WebSocket server **starts** when you load a save file
- The WebSocket server **stops** when you return to the title screen
- The server will **restart** when you load a new save file



