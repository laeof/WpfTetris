using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Threading;
using System;

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

        public GameGrid(int rows, int columns)
        {
            Rows = rows;
            Columns = columns;
            grid = new int[rows, columns];
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
        //for waitasync
        TimeSpan timeout = new TimeSpan(0, 0, 0, 0, 200);
        //score
        public int Score { get; private set; }
        public async void ClearFullRows()
        {
            int cleared = 0;
            bool AddScore = true;
            for (int r = Rows - 1; r >= 0; r--)
            {
                if (IsRowFull(r))
                {
                    await ClearRow(r, 8).WaitAsync(timeout);
                    await Task.Delay(100);
                    await ClearRow(r, 0).WaitAsync(timeout);
                    cleared++;
                }
                else if (cleared > 0)
                {
                    await MoveRowDown(r, cleared).WaitAsync(timeout);
                    if (AddScore)
                    {
                        Score += cleared;
                        AddScore = false;
                    }
                }
            }
        }
        /// <summary>
        /// убираем заполненную линию
        /// </summary>
        /// <param name="row"></param>
        /// <returns></returns>
        /*private bool IsRowFilled(int row)
        {
            int count = 0;
            for (int i = 0; i < Columns; i++)
            {
                if (grid[row, i] != 0)
                {
                    count++;
                }
                else count--;
            }
            if (count == Columns)
            {
                return true;
            }
            else return false;
        }
        /// <summary>
        /// убираем заполненную линию
        /// </summary>
        /// <param name="row"></param>
        private void DownRows(int row)
        {

            for (int j = 0; j < Columns; j++)
            {
                grid[row, j] = 0;
                grid[row, j] = grid[row - 1, j];
            }
        }

        /// <summary>
        /// убираем заполненную линию
        /// </summary>
        public  void ClearOneRow()
        {
            for (int i = 0; i < Rows; i++)
            {
                if (IsRowFilled(i))
                {

                    /*for (int j = 0; j < Columns; j++)
                    {
                        grid[i, j] = 8;
                    } 
                    //await Task.Delay(500);
                    
                    DownRows(i);
                }
            }
        }*/
    }
}
