namespace WpfTetris
{
    using System;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
    using Tetrisdotnet;
    using System.Runtime.InteropServices;
    using System.Threading;
    using System.Windows.Threading;

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly ImageSource[] tileImages = new ImageSource[]
                {
            new BitmapImage(new Uri("Source/TileEmpty.png", UriKind.Relative)),
            new BitmapImage(new Uri("Source/TileCyan.png", UriKind.Relative)),
            new BitmapImage(new Uri("Source/TileBlue.png", UriKind.Relative)),
            new BitmapImage(new Uri("Source/TileOrange.png", UriKind.Relative)),
            new BitmapImage(new Uri("Source/TileYellow.png", UriKind.Relative)),
            new BitmapImage(new Uri("Source/TileGreen.png", UriKind.Relative)),
            new BitmapImage(new Uri("Source/TilePurple.png", UriKind.Relative)),
            new BitmapImage(new Uri("Source/TileRed.png", UriKind.Relative)),
            new BitmapImage(new Uri("Source/TileWhite.png", UriKind.Relative))
                };

        private readonly ImageSource[] blockImages = new ImageSource[]
        {
            new BitmapImage(new Uri("Source/Block-Empty.png", UriKind.Relative)),
            new BitmapImage(new Uri("Source/Block-I.png", UriKind.Relative)),
            new BitmapImage(new Uri("Source/Block-J.png", UriKind.Relative)),
            new BitmapImage(new Uri("Source/Block-L.png", UriKind.Relative)),
            new BitmapImage(new Uri("Source/Block-O.png", UriKind.Relative)),
            new BitmapImage(new Uri("Source/Block-S.png", UriKind.Relative)),
            new BitmapImage(new Uri("Source/Block-T.png", UriKind.Relative)),
            new BitmapImage(new Uri("Source/Block-Z.png", UriKind.Relative))
        };

        private readonly Image[,] imageControls;
        private readonly int maxDelay = 1000;
        private readonly int minDelay = 75;
        private readonly int delayDecrease = 25;

        private GameState gameState = new GameState();

        public MainWindow()
        {
            InitializeComponent();
            imageControls = SetupGameCanvas(gameState.GameGrid);
        }

        private Image[,] SetupGameCanvas(GameGrid grid)
        {
            Image[,] imageControls = new Image[grid.Rows, grid.Columns];
            int cellSize = 25;

            for (int r = 0; r < grid.Rows; r++)
            {
                for (int c = 0; c < grid.Columns; c++)
                {
                    Image imageControl = new Image
                    {
                        Width = cellSize,
                        Height = cellSize
                    };

                    Canvas.SetTop(imageControl, (r - 2) * cellSize + 10);
                    Canvas.SetLeft(imageControl, c * cellSize);
                    GameCanvas.Children.Add(imageControl);
                    imageControls[r, c] = imageControl;
                }
            }

            return imageControls;
        }

        private void DrawGrid(GameGrid grid)
        {
            for (int r = 0; r < grid.Rows; r++)
            {
                for (int c = 0; c < grid.Columns; c++)
                {
                    int id = grid[r, c];
                    imageControls[r, c].Opacity = 1;
                    imageControls[r, c].Source = tileImages[id];
                }
            }
        }

        private void DrawBlock(Block block)
        {
            foreach (Position p in block.TilePositions())
            {
                imageControls[p.Row, p.Column].Opacity = 1;
                imageControls[p.Row, p.Column].Source = tileImages[block.Id];
            }
        }

        private void DrawNextBlock(BlockQueue blockQueue)
        {
            Block next = blockQueue.NextBlock;
            NextImage.Source = blockImages[next.Id];
        }

        private void DrawHeldBlock(Block heldBlock)
        {
            if (heldBlock == null)
            {
                HoldImage.Source = blockImages[0];
            }
            else
            {
                HoldImage.Source = blockImages[heldBlock.Id];
            }
        }

        private void DrawGhostBlock(Block block)
        {
            int dropDistance = gameState.BlockDropDistance();

            foreach (Position p in block.TilePositions())
            {
                imageControls[p.Row + dropDistance, p.Column].Opacity = 0.25;
                imageControls[p.Row + dropDistance, p.Column].Source = tileImages[block.Id];
            }
        }

        private void Draw(GameState gameState)
        {
            DrawGrid(gameState.GameGrid);
            DrawGhostBlock(gameState.CurrentBlock);
            DrawBlock(gameState.CurrentBlock);
            DrawNextBlock(gameState.BlockQueue);
            DrawHeldBlock(gameState.HeldBlock);
            ScoreText.Text = $"Score: {gameState.Score}";
        }
       
        
        private async Task GameLoop()
        {
            Draw(gameState);

            while (!gameState.GameOver)
            {
                if (isPaused)
                    break;
                int delay = Math.Max(minDelay, maxDelay - (gameState.Score * delayDecrease));
                await Task.Delay(delay);
                gameState.MoveBlockDown();
                t.Start();
            }

            if (gameState.GameOver)
            {
                GameOverMenu.Visibility = Visibility.Visible;
                FinalScoreText.Text = $"Score: {gameState.Score}";
            }
        }

        bool isPaused = false;
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (gameState.GameOver)
            {
                return;
            }
            if (isPaused)
            {
                t.Stop();
                if (e.Key != Key.Escape)
                    e.Handled = true;
                else Continue_Click(ContinueBut, e);
            }
            else
                switch (e.Key)
                {
                    case Key.Left:
                        gameState.MoveBlockLeft();
                        break;
                    case Key.Right:
                        gameState.MoveBlockRight();
                        break;
                    case Key.Down:
                        gameState.MoveBlockDown();
                        break;
                    case Key.Up:
                        gameState.RotateBlockCW();
                        break;
                    case Key.Z:
                        gameState.RotateBlockCCW();
                        break;
                    case Key.C:
                        gameState.HoldBlock();
                        break;
                    case Key.Space:
                        gameState.DropBlock();
                        break;
                    case Key.Escape:
                        if (!isPaused)
                        {
                            isPaused = true;
                            GamePause.Visibility = Visibility.Visible;
                            CurrentScoreText.Text = $"Score: {gameState.Score}";
                        }
                        break;
                    default:
                        return;
                }
        }

        /*private async void GameCanvas_Loaded(object sender, RoutedEventArgs e)
        {

            //await GameLoop();
        }*/

        private async void PlayAgain_Click(object sender, RoutedEventArgs e)
        {
            gameState = new GameState();
            GameOverMenu.Visibility = Visibility.Hidden;
            GamePause.Visibility = Visibility.Hidden;
            MainMenu.Visibility = Visibility.Hidden;
            isPaused = false;
            await GameLoop();
        }

        #region UI_Program

        /// <summary>
        /// restore window
        /// </summary>
        private bool mRestoreIfMove = false;

        private void Close_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Close();
        }
        private void Min_MouseDown(object sender, MouseButtonEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }
        /// <summary>
        /// maximize
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Max_MouseDown(object sender, MouseButtonEventArgs e)
        {
            SwitchWindowState();
            
        }
        /// <summary>
        /// max/min
        /// </summary>
        private void SwitchWindowState()
        {
            switch (WindowState)
            {
                case WindowState.Normal:
                    {
                        WindowState = WindowState.Maximized;
                        break;
                    }
                case WindowState.Maximized:
                    {
                        WindowState = WindowState.Normal;
                        mRestoreIfMove = false;
                        break;
                    }
            }
        }
        /// <summary>
        /// restore
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_PreviewMouseMove(object sender, MouseEventArgs e)
        {
           
            if (mRestoreIfMove)
            {
                mRestoreIfMove = false;
                //cursor
                double percentHorizontal = e.GetPosition(this).X / ActualWidth;
                double targetHorizontal = RestoreBounds.Width * percentHorizontal;

                double percentVertical = e.GetPosition(this).Y / ActualHeight;
                double targetVertical = RestoreBounds.Height * percentVertical;

                WindowState = WindowState.Normal;

                POINT lMousePosition;
                GetCursorPos(out lMousePosition);

                //form under cursor
                Left = lMousePosition.X - targetHorizontal;
                Top = lMousePosition.Y - targetVertical;

                if (e.LeftButton == MouseButtonState.Pressed)
                    DragMove();
            }
        }

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool GetCursorPos(out POINT lpPoint);

        //cords
        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public int X;
            public int Y;

            public POINT(int x, int y)
            {
                this.X = x;
                this.Y = y;
            }
        }

        /// <summary>
        /// 2 clicks restore/max
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Border_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                
                if (e.ClickCount == 2)
                {
                    SwitchWindowState();

                    return;
                }
                if (WindowState == WindowState.Maximized)
                {
                    mRestoreIfMove = true;
                    return;
                }
                DragMove();
            }
        }

        #endregion



        private async void Continue_Click(object sender, RoutedEventArgs e)
        {
            GamePause.Visibility = Visibility.Hidden;
            isPaused = false;
            t.Stop();
            await GameLoop();
        }
        DispatcherTimer t;
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            t = new DispatcherTimer() { Interval = TimeSpan.FromMilliseconds(20) };
            t.Tick += new EventHandler(timertick);
        }
        private void timertick(object sender, EventArgs e)
        {
            Draw(gameState);
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
        private void MainMenu_Click(object sender, RoutedEventArgs e)
        {
            MainMenu.Visibility = Visibility.Visible;
        }
    }

}
