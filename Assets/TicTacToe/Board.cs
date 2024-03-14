using UnityEngine;
using Beamable.Common.TicTacToe;
using UnityEngine.UI;


namespace TicTacToe
{
    public class Board : MonoBehaviour
    {
        private readonly Cell[,] _cells = new Cell[3, 3];

        private bool _isActive;
        public bool IsActive
        {
            get { return _isActive; }
            set
            {
                _isActive = value;

                foreach (Selectable selectable in transform.GetComponentsInChildren<Selectable>(true))
                    selectable.interactable = _isActive;
            }
        }

        private void Awake()
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                ushort row = (ushort)(i % 3);
                ushort col = (ushort)(i / 3);

                _cells[row, col] = transform.GetChild(i).GetComponent<Cell>();
                _cells[row, col].SetPosition(row, col);
            }
        }

        /// <summary>
        /// Writes the specific cell for the specific player, then checks the board and returns the resulting game state.
        /// </summary>
        public GameState WriteCell (CellPosition position, bool isPlayer)
        {
            _cells[position.Row, position.Col].Write(isPlayer);

            if (CheckRow(position.Row) || CheckColumn(position.Col) || CheckDiagonals(position.Row, position.Col))
                return isPlayer ? GameState.Won : GameState.Lost;
            else if (CheckFull())
                return GameState.Draw;
            else if (isPlayer)
                return GameState.OpponentTurn;
            else
                return GameState.PlayerTurn;
        }

        private bool CheckFull()
        {
            for (int i = 0; i < 3; i++)
                for (int j = 0; j < 3; j++)
                    if (_cells[i, j].Value == null)
                        return false;
            return true;
        }

        private bool CheckRow(ushort row)
        {
            return _cells[row, 0].Value == _cells[row, 1].Value &&
                   _cells[row, 1].Value == _cells[row, 2].Value;
        }

        private bool CheckColumn(ushort col)
        {
            return _cells[0, col].Value == _cells[1, col].Value &&
                   _cells[1, col].Value == _cells[2, col].Value;
        }

        private bool CheckDiagonals(ushort row, ushort col)
        {
            // Check main diagonal
            if (row == col)
                return _cells[0, 0].Value == _cells[1, 1].Value &&
                       _cells[1, 1].Value == _cells[2, 2].Value;

            // Check secondary diagonal
            else if (row + col == 2)
                return _cells[0, 2].Value == _cells[1, 1].Value &&
                       _cells[1,1].Value == _cells[2, 0].Value;

            return false;
        }

        public void Reset()
        {
            _isActive = false;

            for (int i = 0; i < 3; i++)
                for (int j = 0; j < 3; j++)
                    _cells[i, j].Value = null;
        }
    }
}

