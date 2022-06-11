﻿using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using TetrisEngine;
using Shape = TetrisEngine.Shape;

namespace TetrisClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
    public partial class MainWindow : Window {

        private TetrisGame _game;

        public MainWindow()
        {
            InitializeComponent();

            Matrix matrix = new Matrix(new int[,]
                {
                    { 0, 0, 1 },
                    { 1, 1, 1 },
                    { 0, 0, 0 },
                }
            );
            matrix = matrix.Rotate90();
            
            int offsetY = 0;
            int offsetX = 0;

            int[,] values = matrix.Value;
            for (int i = 0; i < values.GetLength(0); i++)
            {
                for (int j = 0; j < values.GetLength(1); j++)
                {
                    // Als de waarde niet gelijk is aan 1,
                    // dan hoeft die niet getekent te worden:
                    if (values[i, j] != 1) continue;

                    Rectangle rectangle = new Rectangle()
                    {
                        Width = 25, // Breedte van een 'cell' in de Grid
                        Height = 25, // Hoogte van een 'cell' in de Grid
                        Stroke = Brushes.White, // De rand
                        StrokeThickness = 1, // Dikte van de rand
                        Fill = Brushes.Red, // Achtergrondkleur
                    };

                    TetrisGrid.Children.Add(rectangle); // Voeg de rectangle toe aan de Grid
                    Grid.SetRow(rectangle, i + offsetY); // Zet de rij
                    Grid.SetColumn(rectangle, j + offsetX); // Zet de kolom
                }
        private void KeyPressed(object sender, KeyEventArgs keyPress) {
            switch (keyPress.Key) {
                default:
                    return;
                
                case Key.A:
                case Key.Left:
                    _game.MoveLeft();
                    break;
                
                case Key.S:
                case Key.Down:
                    _game.MoveDown();
                    break;
                    
                case Key.D:
                case Key.Right:
                    _game.MoveRight();
                    break;
                
                case Key.W:
                case Key.Up:
                    _game.Rotate();
                    break;
            }
            keyPress.Handled = true;
            
            
        }
        
        private void RegisterKeyEventListener() {
            KeyDown += KeyPressed;
        }

        private void UnregisterKeyEventListener() {
            KeyDown -= KeyPressed;
        }
    }
}
