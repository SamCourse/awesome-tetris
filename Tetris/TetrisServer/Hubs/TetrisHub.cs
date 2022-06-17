using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace TetrisServer.Hubs {
    public class TetrisHub : Hub {
        public async Task UpdateBoard(string board,
            string queue,
            int points,
            int lines) {
            await Clients.Others.SendAsync("Update", board, queue, points, lines);
        }

        private async Task Start() {
            var seed = Guid.NewGuid().GetHashCode();
            await Clients.All.SendAsync("Start", seed);
        }

        public async Task ReadyUp(bool allReady) {
            if (!allReady)
                await Clients.Others.SendAsync("ReadyUp");
            else
                await Start();
        }

        public async Task GameOver() {
            await Clients.Others.SendAsync("GameOver");
        }
    }
}