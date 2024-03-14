using Beamable.Common.TicTacToe;
using UnityEngine;
using TMPro;

namespace TicTacToe
{
    public class Cell : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _signText;

        public enum Sign { O, X }

        private CellPosition _position;
        public CellPosition Position => _position;

        private Sign? _value;
        public Sign? Value
        {
            get => _value.HasValue? _value : null;
            set
            {
                _value = value;
                if (_value != null)
                    _signText.text = _value.ToString();
                else
                    _signText.text = string.Empty;
            }
        }

        public void SetPosition(ushort row, ushort col)
        {
            _position = new CellPosition { Row = row, Col = col };
        }

        public void SendMove ()
        {
            GameManager.Instance.SendPlayerMove(_position);
        }

        public void Write(bool isPlayer)
        {
            if (_value == null)
            {
                var playerSign = GameManager.Instance.CurrentPlayerSign;
                Value = isPlayer ? playerSign : playerSign == Sign.O ? Sign.X : Sign.O;
            }
            else
                Debug.LogWarning("Trying to write a cell that is already filled.");
        }
    }
}
