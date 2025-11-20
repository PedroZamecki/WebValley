using StardewModdingAPI;
using StardewModdingAPI.Events;
using WebValley.WebSocket;

namespace WebValley
{
    /// <summary>The mod entry point.</summary>
    internal sealed class ModEntry : Mod, IDisposable
    {
        /*********
         ** Fields
         *********/
        private WebSocketServer? _webSocketServer;

        /*********
         ** Public methods
         *********/

        /// <inheritdoc/>
        public new void Dispose()
        {
            _webSocketServer?.Dispose();
        }

        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            helper.Events.GameLoop.SaveLoaded += this.OnSaveLoaded;
            helper.Events.GameLoop.ReturnedToTitle += this.OnReturnedToTitle;
        }

        /*********
         ** Private methods
         *********/

        /// <summary>Raised after the game returns to the title screen (including when the player quits, or the save is unloaded).</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event data.</param>
        private void OnReturnedToTitle(object? sender, ReturnedToTitleEventArgs e)
        {
            _webSocketServer?.Stop();
            _webSocketServer?.Dispose();
            _webSocketServer = null;
            this.Monitor.Log("WebSocket server stopped (returned to title).", LogLevel.Info);
        }

        /// <summary>Raised after the player loads a save file (including in multiplayer).</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event data.</param>
        private void OnSaveLoaded(object? sender, SaveLoadedEventArgs e)
        {
            _webSocketServer?.Dispose();
            _webSocketServer = new WebSocketServer(this.Monitor, new GameCommandHandler(this.Monitor));
            _webSocketServer.Start();
        }
    }
}
