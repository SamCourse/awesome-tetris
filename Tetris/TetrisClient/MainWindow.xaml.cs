using System.Diagnostics;
using System.Windows;

namespace TetrisClient {
    /// <summary>
    /// The main menu window which is the first page the user sees.
    /// </summary>
    public partial class MainWindow {
        public MainWindow() {
            InitializeComponent();
        }

        /// <summary>
        /// Button handler for when singleplayer is chosen.
        /// </summary>
        private void Singleplayer_Click(object sender, RoutedEventArgs e) {
            // Create a new GamePage
            var gamePage = new GamePage();
            Content = gamePage;
            
            // Initializes and starts the game.
            gamePage.Initialize();
        }

        /// <summary>
        /// Button handler for when multiplayer is chosen.
        /// Opens the multiplayer screen and sets the window dimensions.
        /// </summary>
        private void Multiplayer_Click(object sender, RoutedEventArgs e) {
            Content = new MultiplayerPage();
            Width = 1440;
            Height = 650;
        }


        /// <summary>
        /// Opens a WikiHow page on how to play Tetris, very useful indeed.
        /// </summary>
        private void HowToButton_Click(object sender, RoutedEventArgs e) {
            Process.Start(new ProcessStartInfo {
                FileName = "https://www.wikihow.com/Play-Tetris",
                UseShellExecute = true
            });
        }
    }
}