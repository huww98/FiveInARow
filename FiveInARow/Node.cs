using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FiveInARow
{
    public class Node
    {
        public Node(Position previousPlacedPosition, ChessPieceKind previousChessPieceKind)
        {
            PreviousPlacedPosition = previousPlacedPosition;
            PreviousChessPieceKind = previousChessPieceKind;
        }

        public Position PreviousPlacedPosition { get; set; }
        public ChessPieceKind PreviousChessPieceKind { get; set; }
        public int Score { get; set; } = 0;
        public int PlayedTimes { get; set; } = 0;
        private ReaderWriterLockSlim timesLock = new ReaderWriterLockSlim();

        public double GetUCB(int totalPlayedTimes)
        {
            timesLock.EnterReadLock();
            double ucb = (double)Score / PlayedTimes + Math.Sqrt(2 * Math.Log(totalPlayedTimes) / PlayedTimes);
            timesLock.ExitReadLock();
            return ucb;
        }

        public List<Node> NextNodes { get; private set; } = new List<Node>();
        public Queue<Position> PositionsAvailable { get; private set; }
        private ReaderWriterLockSlim nextNodeLock = new ReaderWriterLockSlim();

        public ChessPieceKind Search(SearchingChessboard previousChessboard, int totalPlayedTimes)
        {
            bool isWin = previousChessboard.PlaceChessPiece(PreviousPlacedPosition.X, PreviousPlacedPosition.Y);
            var currentChessboard = previousChessboard;
            ChessPieceKind result;
            if (isWin)
            {
                result = PreviousChessPieceKind;
            }
            else if (currentChessboard.AvailablePositions.Count == 0)
            {
                result = ChessPieceKind.Empty;
            }
            else if (PlayedTimes < 1)
            {
                result = PlayOut(currentChessboard);
            }
            else
            {
                findAvailablePositions(currentChessboard);
                var nodeToPlay = getNodeToPlay(currentChessboard, totalPlayedTimes);
                result = nodeToPlay.Search(currentChessboard, this.PlayedTimes);
            }

            timesLock.EnterWriteLock();
            if (result == PreviousChessPieceKind)
            {
                Score++;
            }
            else if (result != ChessPieceKind.Empty)
            {
                Score--;
            }
            PlayedTimes++;
            timesLock.ExitWriteLock();

            return result;
        }

        private Node getNodeToPlay(BasicChessboard chessboard, int totalPlayedTimes)
        {
            Node nodeToPlay = null;

            if (PositionsAvailable.Count > 0)
            {
                nextNodeLock.EnterWriteLock();
                if (PositionsAvailable.Count > 0)
                {
                    nodeToPlay = new Node(PositionsAvailable.Dequeue(), chessboard.NextMoveKind);
                    NextNodes.Add(nodeToPlay);
                }
                nextNodeLock.ExitWriteLock();
            }
            if (nodeToPlay == null)
            {
                nextNodeLock.EnterReadLock();
                nodeToPlay = NextNodes.Aggregate((n1, n2) =>
                {
                    return n1.GetUCB(totalPlayedTimes) > n2.GetUCB(totalPlayedTimes) ? n1 : n2;
                });
                nextNodeLock.ExitReadLock();
            }

            return nodeToPlay;
        }

        private void findAvailablePositions(SearchingChessboard chessboard)
        {
            if (PositionsAvailable == null)
            {
                nextNodeLock.EnterWriteLock();
                if (PositionsAvailable == null)
                {
                    PositionsAvailable = new Queue<Position>(chessboard.AvailablePositions);
                }
                nextNodeLock.ExitWriteLock();
            }
        }

        private static Random rand = new Random();

        public ChessPieceKind PlayOut(SearchingChessboard chessboard)
        {
            while (true)
            {
                int randInt;
                lock (rand)
                {
                    randInt = rand.Next(chessboard.AvailablePositions.Count);
                }
                var nextPosition = chessboard.AvailablePositions.Skip(randInt).First();
                bool isWin = chessboard.PlaceChessPiece(nextPosition.X, nextPosition.Y);

                if (isWin)
                {
                    return chessboard.Result;
                }
                if (chessboard.AvailablePositions.Count == 0)
                {
                    return ChessPieceKind.Empty;
                }
            }
        }

        public override string ToString()
        {
            return $"Position:{PreviousPlacedPosition},PlayedTimes:{PlayedTimes,6},Score:{Score,5}";
        }
    }

    public class Position
    {
        public Position(int x, int y)
        {
            this.X = x;
            this.Y = y;
        }

        public int X { get; set; }
        public int Y { get; set; }

        public override int GetHashCode()
        {
            const int multiplier = int.MaxValue / Chessboard.LineNum / Chessboard.LineNum * 2;
            const int subtractor = Chessboard.LineNum * Chessboard.LineNum / 2;
            return (X * BasicChessboard.LineNum + Y - subtractor) * multiplier;
        }

        public override bool Equals(object obj)
        {
            Position p = obj as Position;
            return p != null && p.X == X && p.Y == Y;
        }

        public override string ToString()
        {
            return $"({X,2},{Y,2})";
        }
    }
}