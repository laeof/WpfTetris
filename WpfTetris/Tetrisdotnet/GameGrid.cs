using System.Threading.Tasks;
using System.Windows.Threading;
using System;
using System.Windows.Media;
using System.Windows;

namespace Tetrisdotnet
{
    public class GameGrid
    {
        private readonly int[,] grid;
        public int Rows { get; }
        public int Columns { get; }

        public int this[int r, int c]
        {
            get => grid[r, c];
            set => grid[r, c] = value;
        }
        DispatcherTimer gameLoop;

        MediaPlayer pl = new MediaPlayer();

        public double Volume;

        public GameGrid(int rows, int columns, DispatcherTimer dt, double volume)
        {
            Rows = rows;
            Columns = columns;
            grid = new int[rows, columns];
            gameLoop = dt;
            Volume = volume;
        }

        public bool IsInside(int r, int c)
        {
            return r >= 0 && r < Rows && c >= 0 && c < Columns;
        }

        public bool IsEmpty(int r, int c)
        {
            return IsInside(r, c) && grid[r, c] == 0;
        }

        public bool IsRowFull(int r)
        {
            for (int c = 0; c < Columns; c++)
            {
                if (grid[r, c] == 0)
                {
                    return false;
                }
            }

            return true;
        }

        public bool IsRowEmpty(int r)
        {
            for (int c = 0; c < Columns; c++)
            {
                if (grid[r, c] != 0)
                {
                    return false;
                }
            }

            return true;
        }

        private async Task ClearRow(int r, int num)
        {
            for (int c = 0; c < Columns; c++)
            {
                await Task.Delay(5);
                grid[r, c] = num;
            }
        }

        private async Task MoveRowDown(int r, int numRows)
        {

            for (int c = 0; c < Columns; c++)
            {
                await Task.Delay(5);
                grid[r + numRows, c] = grid[r, c];
                grid[r, c] = 0;
            }

        }
        private void PlaySound()
        {
            pl.Close();
            pl.Open(new Uri("WhiteClear.wav", UriKind.Relative));
            pl.Volume = Volume;
            pl.Play();
        }
        //score
        public bool RowIsFull = false;
        public int Score { get; private set; }
        public async void ClearFullRows()
        {
            int cleared = 0;
            bool AddScore = true;
            //for waitasync
            TimeSpan timeout = new TimeSpan(0, 0, 0, 0, 250);
            for (int r = Rows - 1; r >= 0; r--)
            {
                if (IsRowFull(r))
                {
                    RowIsFull = true;
                    await ClearRow(r, 8).WaitAsync(timeout);
                    PlaySound();
                    await Task.Delay(5);
                    await ClearRow(r, 0).WaitAsync(timeout);
                    
                    cleared++;
                }
                else if (cleared > 0)
                {
                    gameLoop.Stop();
                    try
                    {
                        await MoveRowDown(r, cleared).WaitAsync(timeout);
                    }
                    catch (Exception e)
                    { 
                        MessageBox.Show(e.Message);
                    }
                    gameLoop.Start();
                    if (AddScore)
                    {
                        Score += cleared;
                        AddScore = false;
                    }
                }
                RowIsFull = false;
            }
        }
    }
}
