using System.Diagnostics;
using System.Windows;

namespace TetrisClient {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        public MainWindow() {
            InitializeComponent();
        }
        
        /// <summary>
        /// Button handler for when singleplayer is chosen.
        /// </summary>
        private void Singleplayer_Click(object sender, RoutedEventArgs e) {
            var gamePage = new GamePage();
            Content = gamePage;
            gamePage.Initialize();
        }

        /// <summary>
        /// Button handler for when multiplayer is chosen.
        /// </summary>
        private void Multiplayer_Click(object sender, RoutedEventArgs e) {
            Content = new MultiplayerPage();
            Width = 1440;
            Height = 650;
        }


        /// <summary>
        /// Opens a WikiHow page on how to play Tetris
        /// </summary>
        private void HowToButton_Click(object sender, RoutedEventArgs e) {
            Process.Start(new ProcessStartInfo {
                FileName = "https://www.wikihow.com/Play-Tetris",
                UseShellExecute = true
            });
        }
    }
}