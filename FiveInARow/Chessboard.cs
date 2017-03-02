using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FiveInARow
{
    internal class Chessboard : BasicChessboard
    {
        private Player _blackPlayer;
        private Player _writePlayer;

        public Player BlackPlayer
        {
            get { return _blackPlayer; }
            set
            {
                _blackPlayer = value;
                _blackPlayer.Chessboard = this;
                _blackPlayer.UsingChessPiece = ChessPieceKind.BlackChessPiece;
            }
        }

        public Player WritePlayer
        {
            get { return _writePlayer; }
            set
            {
                _writePlayer = value;
                _writePlayer.Chessboard = this;
                _writePlayer.UsingChessPiece = ChessPieceKind.WriteChessPiece;
            }
        }

        public Player PlayingPlayer
        {
            get
            {
                return NextMoveKind == ChessPieceKind.BlackChessPiece ? BlackPlayer : WritePlayer;
            }
        }

        public void StartGame()
        {
            PlayingPlayer.StartPlay();
        }

        protected override void OnChessPlaced(ChessPieceInfo chessPiece, bool isWin)
        {
            base.OnChessPlaced(chessPiece, isWin);
            ChessPiecePlaced?.Invoke(this, new ChessPiecePlacedEventArgs { ChessPiece = chessPiece, IsWin = isWin });
            if (!isWin)
            {
                PlayingPlayer.StartPlay();
            }
        }

        public event EventHandler<ChessPiecePlacedEventArgs> ChessPiecePlaced;
    }

    internal class ChessPiecePlacedEventArgs
    {
        public ChessPieceInfo ChessPiece { get; set; }

        public bool IsWin { get; set; }
    }

    public class ChessPieceInfo
    {
        public int X { get; set; }
        public int Y { get; set; }
        public ChessPieceKind Kind { get; set; }
    }
}