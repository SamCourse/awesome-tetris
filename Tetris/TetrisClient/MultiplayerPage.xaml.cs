using System.Threading.Tasks;
using System.Windows;
using Microsoft.AspNetCore.SignalR.Client;

namespace TetrisClient {
    public partial class MultiplayerPage {
        // The connection URL to the Tetris server
        private const string SERVER_URL = "http://127.0.0.1:5000/TetrisHub";

        private HubConnection _connection;
        private GamePage _p1GamePage;
        private bool p1Ready;
        private bool p2Ready;
        
        public MultiplayerPage() {
            InitializeComponent();
            Setup();
        }

        private void Setup() {
            // Create a new game control for player1
            _p1GamePage = new GamePage();
            Player1Frame.Content = _p1GamePage;

            // The connection builder for the Tetris server
            _connection = new HubConnectionBuilder()
                .WithUrl(SERVER_URL)
                .WithAutomaticReconnect()
                .Build();

            // Mount the send and receive handlers for the Tetris server
            MountHandlers();

            // Start the connection after all the handlers have been mounted
            Task.Run(async () => await _connection.StartAsync());
        }

        private void StartGame(int seed) {
            _p1GamePage.Initialize(seed);
            
            LobbyScreen.Visibility = Visibility.Hidden;
        }

        private void MountHandlers() {
            _connection.On("ReadyUp", () => {
                p2Ready = !p2Ready;
                Dispatcher.Invoke(() =>
                    Player2ReadyImage.Visibility = p2Ready ? Visibility.Visible : Visibility.Hidden);
            });

            _connection.On<int>("Start", 
                seed => Dispatcher.Invoke(() => StartGame(seed)));// Run from main thread because this thread does not own the element

            // ReceiveUpdate
            _connection.On("Update", (
                int[,] board,
                int[,] queue,
                int points,
                int lines) => {
                GameGridTwo.UpdateBoard(board);
                QueueGridTwo.UpdateBoard(queue);
                PointsLabelTwo.Content = points;
                LinesLabelTwo.Content = lines;
            });
            // ReceiveGameEnd
        }

        private async void ReadyUp_OnClick(object sender, RoutedEventArgs e) {
            // If the connection isn't initialized, nothing can be sent to it.
            if (_connection.State != HubConnectionState.Connected)
                return;

            p1Ready = !p1Ready;

            if (p1Ready) {
                Player1ReadyImage.Visibility = Visibility.Visible;
                ReadyUpButton.Content = "Cancel";
            }
            else {
                Player1ReadyImage.Visibility = Visibility.Hidden;
                ReadyUpButton.Content = "Ready up";
            }

            await _connection.InvokeAsync("ReadyUp", p1Ready && p2Ready);
        }
    }
}