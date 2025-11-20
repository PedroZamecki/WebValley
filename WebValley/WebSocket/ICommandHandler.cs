namespace WebValley.WebSocket
{
    /// <summary>Handles WebSocket commands to control the mod.</summary>
    internal interface ICommandHandler
    {
        /// <summary>
        /// Process a command message from a WebSocket client.
        /// </summary>
        /// <param name="message">The message to process.</param>
        /// <returns>A response message to send back to the client.</returns>
        string HandleCommand(string message);
    }
}
