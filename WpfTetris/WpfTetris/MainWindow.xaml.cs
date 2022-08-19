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
    using System.IO;
    using System.Media;
    using System.IO.Compression;
    using System.Windows.Media.Animation;
    using System.Text;
    using System.Collections.Generic;

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

        private bool isPaused = false;
        private bool isplayed = false;
        private MediaPlayer playmove = new MediaPlayer();
        private MediaPlayer playclick = new MediaPlayer();

        private static double Volume = 0.5;


        private readonly Image[,] imageControls;

        private GameState? gameState = new GameState(gameLoop, Volume);
        
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
        
        #region draw
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
        #endregion

        private void GameLoop(object sender, EventArgs e)
        {
            if (gameState != null)
            {
                if (!gameState.GameOver)
                {
                    if (!isPaused)
                    {
                        gameState.MoveBlockDown();
                        if (gameLoop.Interval > TimeSpan.FromMilliseconds(250))
                            gameLoop.Interval = TimeSpan.FromMilliseconds((double)((0 - (gameState.Score * gameState.Score)) / 10) + 700);
                    }
                }
                else
                {
                    GameOverMenu.Visibility = Visibility.Visible;
                    FinalScoreText.Text = $"Score: {gameState.Score}";
                }
            }
        }

        
        
        bool IsGameStarted = false;
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (IsGameStarted)
            {
                if (isPaused)
                {

                    gameLoop.Stop();
                    if (e.Key != Key.Escape)
                        e.Handled = true;
                    else Continue_Click(ContinueBut, e);
                }
                else if (gameState.GameOver)
                {
                    return;
                }
                else if (!isPaused)
                {
                    if(e.Key == keys[0])
                        gameState.MoveBlockLeft();
                    if (e.Key == keys[1])
                        gameState.MoveBlockRight();
                    if (e.Key == keys[2])
                        gameState.MoveBlockDown();
                    if (e.Key == keys[3])
                        gameState.DropBlock();
                    if (e.Key == keys[4])
                        gameState.RotateBlockCCW();
                    if (e.Key == keys[5])
                        gameState.HoldBlock();
                    if (e.Key == keys[6])
                        gameState.RotateBlockCW();
                    if (e.Key == Key.Escape)
                        if (!isPaused)
                        {
                            playclick.Play();
                            isPaused = true;
                            GamePause.Visibility = Visibility.Visible;
                            CurrentScoreText.Text = $"Score: {gameState.Score}";
                        }
                }
            }
        }

        private void PlayAgain_Click(object sender, RoutedEventArgs e)
        {
            gameState = new GameState(gameLoop, Volume);
            gameLoop.Interval = TimeSpan.FromMilliseconds(700);
            GameOverMenu.Visibility = Visibility.Hidden;
            GamePause.Visibility = Visibility.Hidden;
            MainMenu.Visibility = Visibility.Hidden;
            isPaused = false;
            PlayClick();
            t.Start();
            gameLoop.Start();
            IsGameStarted = true;

        }
        private void Continue_Click(object sender, RoutedEventArgs e)
        {
            GamePause.Visibility = Visibility.Hidden;
            isPaused = false;
            PlayClick();
            gameLoop.Start();
        }
        private void ApplyConfig()
        {
            LeftPosition.Content = keys[0];
            RightPosition.Content = keys[1];
            DownPosition.Content = keys[2];
            Drop.Content = keys[3];
            CW.Content = keys[6];
            CCW.Content = keys[4];
            Hold.Content = keys[5];
            SliderVolume.Value = Volume;
            Volume = SliderVolume.Value * 0.2;
        }

        private void ApplyDefaultConfig()
        {
            SliderVolume.Value = 2.5;
            Volume *= 0.2;
            keys.Add(Key.A);
            keys.Add(Key.D);
            keys.Add(Key.S);
            keys.Add(Key.Space);
            keys.Add(Key.Q);
            keys.Add(Key.W);
            keys.Add(Key.E);
            LeftPosition.Content = Key.A;
            RightPosition.Content = Key.D;
            DownPosition.Content = Key.S;
            Drop.Content = Key.Space;
            CW.Content = Key.E;
            CCW.Content = Key.Q;
            Hold.Content = Key.W;
        }

        List<Key> keys;
        Configuration configuration = new Configuration();
        DispatcherTimer t;
        static DispatcherTimer gameLoop;

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            
            keys = new List<Key>();
            if (configuration.LoadConfig(ref keys, ref Volume))
            {
                ApplyConfig();
            }
            else 
            {
                ApplyDefaultConfig();
            }

            t = new DispatcherTimer() { Interval = TimeSpan.FromMilliseconds(20) };
            gameLoop = new DispatcherTimer() { Interval = TimeSpan.FromMilliseconds(700) };
            gameLoop.Tick += new EventHandler(GameLoop);
            t.Tick += new EventHandler(timertick);
            username.Text = System.Environment.UserName;
        }
        private void timertick(object sender, EventArgs e)
        {
            Draw(gameState);
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            PlayClick();
            Close();

        }
        private void LeaveOptions_Click(object sender, RoutedEventArgs e)
        {
            if (!ZeroButtonsArePressed)
            {
                PlayClick();
                Options.Visibility = Visibility.Hidden;
                MainMenu.Visibility = Visibility.Visible;

                configuration.SaveConfig(keys, SliderVolume.Value);
                keys = new List<Key>();
                configuration.LoadConfig(ref keys, ref Volume);
                ApplyConfig();
            }
        }
        private void MainMenu_Click(object sender, RoutedEventArgs e)
        {
            PlayClick();
            gameLoop.Stop();
            t.Stop();
            gameState = null;
            isPaused = true;
            IsGameStarted = false;
            //stop the task
            GC.Collect();
            GC.WaitForPendingFinalizers();
            MainMenu.Visibility = Visibility.Visible;
        }
        private void StartGame_MouseMove(object sender, MouseEventArgs e)
        {
            if (!isplayed)
            {
                playmove.Open(new Uri("choocelement.wav", UriKind.Relative));
                playmove.Volume = Volume / 2.5;
                isplayed = true;
                playmove.Play();
                PlayMove().WaitAsync(TimeSpan.FromMilliseconds(150));
            }
        }
        private void StartGame_MouseLeave(object sender, MouseEventArgs e)
        {
            isplayed = false;
        }
        private void Options_Click(object sender, RoutedEventArgs e)
        {
            PlayClick();
            MainMenu.Visibility = Visibility.Hidden;
            Options.Visibility = Visibility.Visible;
        }
        private void Leaders_Click(object sender, RoutedEventArgs e)
        {
            PlayClick();
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


        private async Task PlayMove()
        {
            await Task.Delay(100);            

            playclick.Close();
        }
        private void PlayClick()
        {
            playclick.Close();
            playclick.Open(new Uri("enterelement.wav", UriKind.Relative));
            playclick.Volume = Volume;
            playclick.Play();
        }

        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Volume = SliderVolume.Value * 0.2;
            playclick.Close();
            playclick.Open(new Uri("enterelement.wav", UriKind.Relative));
            playclick.Volume = Volume;
            playclick.Play();
        }
        bool ZeroButtonsArePressed = false;
        private void OptionChange_Click(object sender, RoutedEventArgs e)
        {   
            if (!ZeroButtonsArePressed)
            {
                ((Button)sender).Content = " ";
                ZeroButtonsArePressed = true;
                PlayClick();
            }
        }

        private void Options_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (LeftPosition.Content.Equals(" "))
            {
                keys[0] = e.Key;
                LeftPosition.Content = e.Key.ToString();
                ZeroButtonsArePressed = false;
            }
            else if (RightPosition.Content.ToString() == " ")
            {
                keys[1] = e.Key;
                RightPosition.Content = e.Key.ToString();
                ZeroButtonsArePressed = false;
            }
            else if (DownPosition.Content.ToString() == " ")
            {
                keys[2] = e.Key;
                DownPosition.Content = e.Key.ToString();
                ZeroButtonsArePressed = false;
            }
            else if (Drop.Content.ToString() == " ")
            {
                keys[3] = e.Key;
                Drop.Content = e.Key.ToString();
                ZeroButtonsArePressed = false;
            }
            else if (CCW.Content.ToString() == " ")
            {
                keys[4] = e.Key;
                CCW.Content = e.Key.ToString();
                ZeroButtonsArePressed = false;
            }
            else if (Hold.Content.ToString() == " ")
            {
                keys[5] = e.Key;
                Hold.Content = e.Key.ToString();
                ZeroButtonsArePressed = false;
            }
            else if (CW.Content.ToString() == " ")
            {
                keys[6] = e.Key;
                CW.Content = e.Key.ToString();
                ZeroButtonsArePressed = false;
            }
        }
    }
}
