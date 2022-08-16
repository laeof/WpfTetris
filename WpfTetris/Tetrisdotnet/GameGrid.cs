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

        private void ClearRow(int r)
        {
            for (int c = 0; c < Columns; c++)
            {
                grid[r, c] = 0;
            }
        }

        private void MoveRowDown(int r, int numRows)
        {

            for (int c = 0; c < Columns; c++)
            {
                grid[r + numRows, c] = grid[r, c];
                grid[r, c] = 0;
            }

        }

        public int ClearFullRows()
        {
            int cleared = 0;

            for (int r = Rows-1; r >= 0; r--)
            {
                if (IsRowFull(r))
                {
                    ClearRow(r);
                    cleared++;
                }
                else if (cleared > 0)
                {
                    MoveRowDown(r, cleared);
                }
            }

            return cleared;
        }
        /*/// <summary>
        /// убираем заполненную линию
        /// </summary>
        /// <param name="row"></param>
        /// <returns></returns>
        private bool IsRowFilled(int row)
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
        private Task DownRows(int row)
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
        public async void ClearOneRow()
        {
            for (int i = 0; i < Rows; i++)
            {
                if (IsRowFilled(i))
                {
                    await DownRows(i);
                }
            }
        }*/
    }
}
