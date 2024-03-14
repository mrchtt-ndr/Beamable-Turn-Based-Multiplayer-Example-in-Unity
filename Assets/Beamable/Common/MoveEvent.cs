using System;

namespace Beamable.Common.TicTacToe
{
    [Serializable]
    public struct CellPosition { public ushort Row; public ushort Col; }

    [Serializable]
    public struct MoveEvent
    {
        public long PlayerId;
        public CellPosition Position;

        public MoveEvent(long playerId, CellPosition position)
        {
            PlayerId = playerId;
            Position = position;
        }

        public override readonly string ToString()
        {
            return $"[MoveEvent({PlayerId},{Position})]";
        }
    }
}