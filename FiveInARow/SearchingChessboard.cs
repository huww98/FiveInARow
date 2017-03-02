using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FiveInARow
{
    public class SearchingChessboard : BasicChessboard
    {
        public HashSet<Position> AvailablePositions { get; private set; } = new HashSet<Position>();
        private const int searchRadius = 1;
        private List<ChessPieceInfo> history = new List<ChessPieceInfo>();

        protected override void OnChessPlaced(ChessPieceInfo chessPiece, bool isWin)
        {
            base.OnChessPlaced(chessPiece, isWin);
            history.Add(chessPiece);
            if (!isWin)
            {
                updateAvailablePositions(chessPiece);
            }
            else
            {
                //using (StreamWriter writer = new StreamWriter("PlayOutLog.txt", true))
                //{
                //    writer.WriteLine(string.Join(" ",history.Select(c => $"{c.Kind} {c.X},{c.Y};")));
                //}
            }
        }

        private void updateAvailablePositions(ChessPieceInfo chessPiece)
        {
            AvailablePositions.Remove(new Position(chessPiece.X, chessPiece.Y));
            int minX = chessPiece.X <= searchRadius ? 0 : chessPiece.X - searchRadius;
            int minY = chessPiece.Y <= searchRadius ? 0 : chessPiece.Y - searchRadius;
            int maxX = chessPiece.X >= LineNum - searchRadius - 1 ? LineNum - 1 : chessPiece.X + searchRadius;
            int maxY = chessPiece.Y >= LineNum - searchRadius - 1 ? LineNum - 1 : chessPiece.Y + searchRadius;
            for (int i = minX; i <= maxX; i++)
            {
                for (int j = minY; j <= maxY; j++)
                {
                    if (this[i, j] == ChessPieceKind.Empty)
                    {
                        Position p = new Position(i, j);
                        AvailablePositions.Add(p);
                    }
                }
            }
        }

        public SearchingChessboard MakeCopy()
        {
            SearchingChessboard chessboard = new SearchingChessboard();
            base.CopyTo(chessboard);
            chessboard.AvailablePositions = new HashSet<Position>(AvailablePositions);
            return chessboard;
        }

        internal void GenerateAvailablePositions()
        {
            for (int i = 0; i < LineNum; i++)
            {
                for (int j = 0; j < LineNum; j++)
                {
                    var chessPieceKind = this[i, j];
                    if (chessPieceKind != ChessPieceKind.Empty)
                    {
                        updateAvailablePositions(new ChessPieceInfo { Kind = chessPieceKind, X = i, Y = j });
                    }
                }
            }
        }
    }

    internal static class SearchingChessboardExtentions
    {
        public static SearchingChessboard MakeCopy(this BasicChessboard chessboard)
        {
            SearchingChessboard newChessboard = new SearchingChessboard();
            chessboard.CopyTo(newChessboard);
            newChessboard.GenerateAvailablePositions();
            return newChessboard;
        }
    }
}