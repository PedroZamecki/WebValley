using StardewModdingAPI;
using StardewValley;

namespace WebValley.WebSocket
{
    /// <summary>A simple command handler for testing basic Stardew Valley interactions.</summary>
    internal sealed class GameCommandHandler : ICommandHandler
    {
        private readonly IMonitor _monitor;

        /// <summary>
        /// Initializes a new instance of the <see cref="GameCommandHandler"/> class.
        /// </summary>
        /// <param name="monitor">The SMAPI monitor for logging.</param>
        public GameCommandHandler(IMonitor monitor)
        {
            _monitor = monitor;
        }

        /// <summary>
        /// Handle a command message.
        /// Message formats:
        /// - "ping" - Test connection
        /// - "info:{info_name}" - Query game info
        /// - "command:{command_name}:{args separated with ;}" - Execute game command.
        /// </summary>
        /// <param name="message">The message to process.</param>
        /// <returns>A response message to send back to the client.</returns>
        public string HandleCommand(string message)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(message))
                {
                    return "error:empty_message";
                }

                var parts = message.Split(':', 2);
                var command = parts[0].ToUpperInvariant().Trim();

                return command switch
                {
                    "PING" => "pong",
                    "INFO" => HandleInfoCommand(parts.Length > 1 ? parts[1] : string.Empty),
                    "COMMAND" => HandleGameCommand(parts.Length > 1 ? parts[1] : string.Empty),
                    "HELP" => GetHelpMessage(),
                    _ => $"error:unknown_command:{command}"
                };
            }
            catch (InvalidOperationException ex)
            {
                _monitor.Log($"Error handling command: {ex.Message}", LogLevel.Error);
                return $"error:{ex.Message}";
            }
        }

        /// <summary>Handle info queries like "info:time", "info:money", etc.</summary>
        private static string HandleInfoCommand(string infoRequest)
        {
            if (string.IsNullOrWhiteSpace(infoRequest))
            {
                return "error:missing_info_type";
            }

            var infoType = infoRequest.ToUpperInvariant().Trim();
            return infoType switch
            {
                "TIME" => GetCurrentTime(),
                "MONEY" => GetPlayerMoney(),
                "NAME" => GetPlayerName(),
                "LEVEL" => GetPlayerLevel(string.Empty),
                "LEVEL:FARMING" => GetPlayerLevel("FARMING"),
                "LEVEL:FISHING" => GetPlayerLevel("FISHING"),
                "LEVEL:FORAGING" => GetPlayerLevel("FORAGING"),
                "LEVEL:MINING" => GetPlayerLevel("MINING"),
                "LEVEL:COMBAT" => GetPlayerLevel("COMBAT"),
                "HEALTH" => GetPlayerHealth(),
                "ENERGY" => GetPlayerEnergy(),
                _ => $"error:unknown_info_type:{infoType}"
            };
        }

        /// <summary>Handle game commands. Currently not implemented.</summary>
        private static string HandleGameCommand(string commandRequest)
        {
            if (string.IsNullOrWhiteSpace(commandRequest))
            {
                return "error:missing_command_name";
            }

            return "error:game_commands_not_implemented";
        }

        private static string GetCurrentTime()
        {
            if (!Context.IsWorldReady)
            {
                return "error:world_not_ready";
            }

            return $"time:{Game1.timeOfDay}";
        }

        private static string GetPlayerMoney()
        {
            if (!Context.IsWorldReady)
            {
                return "error:world_not_ready";
            }

            return $"money:{Game1.player.Money}";
        }

        private static string GetPlayerName()
        {
            if (!Context.IsWorldReady)
            {
                return "error:world_not_ready";
            }

            return $"name:{Game1.player.Name}";
        }

        private static string GetPlayerLevel(string skill)
        {
            if (!Context.IsWorldReady)
            {
                return "error:world_not_ready";
            }

            if (skill.Length == 0)
            {
                return $"level:farming:{Game1.player.FarmingLevel},fishing:{Game1.player.FishingLevel},foraging:{Game1.player.ForagingLevel},mining:{Game1.player.MiningLevel},combat:{Game1.player.CombatLevel}";
            }

            return skill switch
            {
                "FARMING" => $"level:farming:{Game1.player.FarmingLevel}",
                "FISHING" => $"level:fishing:{Game1.player.FishingLevel}",
                "FORAGING" => $"level:foraging:{Game1.player.ForagingLevel}",
                "MINING" => $"level:mining:{Game1.player.MiningLevel}",
                "COMBAT" => $"level:combat:{Game1.player.CombatLevel}",
                _ => "error:unknown_skill"
            };
        }

        private static string GetPlayerHealth()
        {
            if (!Context.IsWorldReady)
            {
                return "error:world_not_ready";
            }

            return $"health:{Game1.player.health}/{Game1.player.maxHealth}";
        }

        private static string GetPlayerEnergy()
        {
            if (!Context.IsWorldReady)
            {
                return "error:world_not_ready";
            }

            return $"energy:{Game1.player.stamina}/{Game1.player.maxStamina}";
        }

        private static string GetHelpMessage()
        {
            return "available_commands:ping,info:{time|money|name|level|level:skill|health|energy},command:{name}:{args},help";
        }
    }
}
