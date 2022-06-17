using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace TetrisServer.Hubs {
    public class TetrisHub : Hub {
        /// <summary>
        /// Used to update the other client with the players' game situation.
        /// </summary>
        public async Task UpdateBoard(string board,
            string queue,
            int points,
            int lines) {
            await Clients.Others.SendAsync("Update", board, queue, points, lines);
        }

        /// <summary>
        /// Used to generate a seed for the multiplayer game and makes the call to the players.
        /// </summary>
        private async Task Start() {
            var seed = Guid.NewGuid().GetHashCode();
            await Clients.All.SendAsync("Start", seed);
        }

        /// <summary>
        /// Called when the player is readied up.
        /// </summary>
        /// <param name="allReady">Whether both the players are readied up and ready to play the game.</param>
        public async Task ReadyUp(bool allReady) {
            if (!allReady)
                await Clients.Others.SendAsync("ReadyUp");
            else
                await Start();
        }

        /// <summary>
        /// Called when one of the players' game has finished.
        /// </summary>
        public async Task GameOver() {
            await Clients.Others.SendAsync("GameOver");
        }
    }
}