using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FiveInARow
{
    public class BasicChessboard
    {
        public const int LineNum = 15;
        private ChessPieceKind[,] positions = new ChessPieceKind[LineNum, LineNum];
        public ChessPieceKind NextMoveKind { get; set; } = ChessPieceKind.BlackChessPiece;
        public ChessPieceInfo previousMove { get; set; }

        public ChessPieceKind this[int x, int y]
        {
            get { return positions[x, y]; }
            set { positions[x, y] = value; }
        }

        public bool CanPlaceChessPiece(int x, int y)
        {
            if (x < 0 || x >= LineNum || y < 0 || y >= LineNum)
            {
                return false;
            }
            return positions[x, y] == ChessPieceKind.Empty;
        }

        public ChessPieceKind Result { get; private set; } = ChessPieceKind.Empty;

        public bool PlaceChessPiece(int x, int y)
        {
            if (!CanPlaceChessPiece(x, y))
            {
                throw new InvalidOperationException("非法落子");
            }
            positions[x, y] = NextMoveKind;
            ChessPieceInfo chessPiece = new ChessPieceInfo { X = x, Y = y, Kind = NextMoveKind };
            previousMove = chessPiece;
            bool isWin = calcResult(chessPiece);
            if (isWin)
            {
                Result = previousMove.Kind;
            }
            NextMoveKind = NextMoveKind == ChessPieceKind.BlackChessPiece ? ChessPieceKind.WriteChessPiece : ChessPieceKind.BlackChessPiece;
            OnChessPlaced(chessPiece, isWin);
            return isWin;
        }

        protected virtual void OnChessPlaced(ChessPieceInfo chessPiece, bool isWin)
        {
        }

        private bool calcResult(ChessPieceInfo chess)
        {
            return calcXResult(chess) || calcYResult(chess) || calcXY1Result(chess) || calcXY2Result(chess);
        }

        private bool calcXResult(ChessPieceInfo chess)
        {
            int chessInARowCount = 1;
            int currentX = chess.X + 1, currentY = chess.Y;
            while (currentX < LineNum && positions[currentX, currentY] == chess.Kind)
            {
                chessInARowCount++;
                currentX++;
            }

            currentX = chess.X - 1;
            while (currentX >= 0 && positions[currentX, currentY] == chess.Kind)
            {
                chessInARowCount++;
                currentX--;
            }

            return chessInARowCount >= 5;
        }

        private bool calcYResult(ChessPieceInfo chess)
        {
            int chessInARowCount = 1;
            int currentX = chess.X, currentY = chess.Y + 1;
            while (currentY < LineNum && positions[currentX, currentY] == chess.Kind)
            {
                chessInARowCount++;
                currentY++;
            }

            currentY = chess.Y - 1;
            while (currentY >= 0 && positions[currentX, currentY] == chess.Kind)
            {
                chessInARowCount++;
                currentY--;
            }

            return chessInARowCount >= 5;
        }

        private bool calcXY1Result(ChessPieceInfo chess)
        {
            int chessInARowCount = 1;
            int currentX = chess.X + 1, currentY = chess.Y + 1;
            while (currentX < LineNum && currentY < LineNum && positions[currentX, currentY] == chess.Kind)
            {
                chessInARowCount++;
                currentX++;
                currentY++;
            }

            currentX = chess.X - 1; currentY = chess.Y - 1;
            while (currentX >= 0 && currentY >= 0 && positions[currentX, currentY] == chess.Kind)
            {
                chessInARowCount++;
                currentX--;
                currentY--;
            }

            return chessInARowCount >= 5;
        }

        private bool calcXY2Result(ChessPieceInfo chess)
        {
            int chessInARowCount = 1;
            int currentX = chess.X + 1, currentY = chess.Y - 1;
            while (currentX < LineNum && currentY >= 0 && positions[currentX, currentY] == chess.Kind)
            {
                chessInARowCount++;
                currentX++;
                currentY--;
            }

            currentX = chess.X - 1; currentY = chess.Y + 1;
            while (currentX >= 0 && currentY < LineNum && positions[currentX, currentY] == chess.Kind)
            {
                chessInARowCount++;
                currentX--;
                currentY++;
            }

            return chessInARowCount >= 5;
        }

        public void CopyTo(BasicChessboard chessboard)
        {
            for (int i = 0; i < LineNum; i++)
            {
                for (int j = 0; j < LineNum; j++)
                {
                    chessboard.positions[i, j] = positions[i, j];
                }
            }
            chessboard.NextMoveKind = NextMoveKind;
            chessboard.Result = Result;
        }
    }

    public enum ChessPieceKind : byte
    {
        Empty,
        WriteChessPiece,
        BlackChessPiece,
    }
}