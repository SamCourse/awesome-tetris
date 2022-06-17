using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Microsoft.AspNetCore.SignalR.Client;
using Newtonsoft.Json;
using TetrisEngine;

namespace TetrisClient {
    /// <summary>
    /// The multiplayer page which displays multiplayer Tetris.
    /// </summary>
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

        /// <summary>
        /// Sets the page up ready to play Tetris multiplayer.
        /// </summary>
        private void Setup() {
            // Create a new GamePage for this player
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

        /// <summary>
        /// Start the game for this player.
        /// </summary>
        /// <param name="seed">The seed used for generating the random Tetromino queue.</param>
        private void StartGame(int seed) {
            // Initialize the game page
            _p1GamePage.Initialize(seed);
            // Add listener to the GameTimer interval callback
            _p1GamePage.Game.AddTimerListener((_, _) => DispatchUpdate());

            // Send an update to the other player with the new changes after starting the game
            DispatchUpdate();
            
            // Hide the lobby screen
            LobbyScreen.Visibility = Visibility.Hidden;
        }

        /// <summary>
        /// Mount the connection handlers that handle incoming connections from the other player (through the hub).
        /// </summary>
        private void MountHandlers() {
            // Handler for when the other player presses the ready-up button.
            _connection.On("ReadyUp", () => {
                p2Ready = !p2Ready;
                
                Dispatcher.Invoke(() =>
                    Player2ReadyImage.Visibility = p2Ready ? Visibility.Visible : Visibility.Hidden);
            });

            // Handler for when the hub has decided both players are ready and the game can start.
            _connection.On<int>("Start",
                seed => Dispatcher.Invoke(() => { // Run from main thread because this thread does not own the element
                    StartGame(seed);

                    // Mount key listener for updating the other player with new changes on the go.
                    _p1GamePage.RegisterKeyListener(KeyPressed);
                }));

            // Handler for receiving the updates on the game state of the other player
            _connection.On("Update", (
                string board,
                string queue,
                int points,
                int lines) => {
                Dispatcher.Invoke(() => {
                    // Deserialize the other players' board from string to int[,] and update the grid with it.
                    GameGridTwo.UpdateBoard(JsonConvert.DeserializeObject<int[,]>(board));
                    // Deserialize the other players' queue
                    QueueGridTwo.UpdateBoard(JsonConvert.DeserializeObject<int[,]>(queue));
                    PointsLabelTwo.Content = points;
                    LinesLabelTwo.Content = lines;
                });
            });

            // Handler for when the other player has ended his game
            _connection.On("GameOver",
                () => { Dispatcher.Invoke(() => { GameOverScreen.Visibility = Visibility.Visible; }); });
        }

        /// <summary>
        /// The handler for when the readyUp button is clicked
        /// If the connection with the hub isn't established, doesn't do anything
        /// Calls the hub with the ReadyUp method and updates the UI
        /// </summary>
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

        /// <summary>
        /// Dispatch an update of this players' board, queue and score.
        /// If this players' game is over, it sends that instead. Also removes the key listener.
        /// </summary>
        private async void DispatchUpdate() {
            if (_p1GamePage.Game.GameState == GameState.OVER) {
                await _connection.InvokeAsync("GameOver");
                _p1GamePage.RemoveKeyListener(KeyPressed);
            }
            else {
                await _connection.InvokeAsync("UpdateBoard",
                    JsonConvert.SerializeObject(_p1GamePage.Game.Board), // Serialize the int[,] board to a string.
                    JsonConvert.SerializeObject(_p1GamePage.Game.Queue), // serialize the queue to string
                    _p1GamePage.Game.Points,
                    _p1GamePage.Game.Lines);
            }
        }

        /// <summary>
        /// The event handler that handles key presses. Only space, up-down-left-right and ASDW are handled here.
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