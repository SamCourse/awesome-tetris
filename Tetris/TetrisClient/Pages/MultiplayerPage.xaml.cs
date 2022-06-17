using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Microsoft.AspNetCore.SignalR.Client;
using Newtonsoft.Json;
using TetrisEngine;

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
            _p1GamePage.Game.AddTimerListener((_, _) => DispatchUpdate());

            DispatchUpdate();
            LobbyScreen.Visibility = Visibility.Hidden;
        }

        private void MountHandlers() {
            // Mount connection handlers
            _connection.On("ReadyUp", () => {
                p2Ready = !p2Ready;
                Dispatcher.Invoke(() =>
                    Player2ReadyImage.Visibility = p2Ready ? Visibility.Visible : Visibility.Hidden);
            });

            _connection.On<int>("Start",
                seed => Dispatcher.Invoke(() => { // Run from main thread because this thread does not own the element
                    StartGame(seed);

                    // Mount key listener
                    _p1GamePage.RegisterKeyListener(KeyPressed);
                }));

            // ReceiveUpdate
            _connection.On("Update", (
                string board,
                string queue,
                int points,
                int lines) => {
                Dispatcher.Invoke(() => {
                    GameGridTwo.UpdateBoard(JsonConvert.DeserializeObject<int[,]>(board));
                    QueueGridTwo.UpdateBoard(JsonConvert.DeserializeObject<int[,]>(queue));
                    PointsLabelTwo.Content = points;
                    LinesLabelTwo.Content = lines;
                });
            });

            _connection.On("GameOver", () => {
                Dispatcher.Invoke(() => {
                    GameOverScreen.Visibility = Visibility.Visible;
                });
            });
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

        private async void DispatchUpdate() {
            if (_p1GamePage.Game.GameState == GameState.OVER)
                await _connection.InvokeAsync("GameOver");
            else {
                await _connection.InvokeAsync("UpdateBoard",
                    JsonConvert.SerializeObject(_p1GamePage.Game.Board),
                    JsonConvert.SerializeObject(_p1GamePage.Game.Queue),
                    _p1GamePage.Game.Points,
                    _p1GamePage.Game.Lines);
            }
        }

        /// <summary>
        /// The event handler that handles key presses. Only up-down-left-right and ASDW are handled here.
        /// </summary>
        private void KeyPressed(object sender, KeyEventArgs keyPress) {
            // Handle key presses
            switch (keyPress.Key) {
                case Key.A:
                case Key.Left:
                case Key.S:
                case Key.Down:
                case Key.D:
                case Key.Right:
                case Key.W:
                case Key.Up:
                case Key.Space:
                    DispatchUpdate();
                    break;
            }
        }
    }
}