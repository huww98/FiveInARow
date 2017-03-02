using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FiveInARow
{
    internal class AI : IDisposable
    {
        private const int firstPosition = BasicChessboard.LineNum / 2;
        private Node rootNode;
        private bool shouldPauseAfterTimes = false;
        private CancellationTokenSource cts;
        private List<Task> tasks;
        private SearchingChessboard chessboard = new SearchingChessboard();
        private Player _player;

        public Player Player
        {
            get { return _player; }
            set
            {
                if (_player != null)
                {
                    _player.PlayStarted -= _player_PlayStarted;
                    _player.Chessboard.ChessPiecePlaced += Chessboard_ChessPiecePlaced;
                }
                _player = value;
                _player.PlayStarted += _player_PlayStarted;
                _player.Chessboard.ChessPiecePlaced += Chessboard_ChessPiecePlaced;
                chessboard = _player.Chessboard.MakeCopy();
                if (_player.IsPlaying)
                {
                    if (_player.Chessboard.previousMove != null)
                    {
                        chessboard[_player.Chessboard.previousMove.X, _player.Chessboard.previousMove.Y] = ChessPieceKind.Empty;
                        chessboard.NextMoveKind =
                            chessboard.NextMoveKind == ChessPieceKind.BlackChessPiece ?
                            ChessPieceKind.WriteChessPiece : ChessPieceKind.BlackChessPiece;
                    }

                    _player_PlayStarted(_player, new EventArgs());
                }
            }
        }

        private async void Chessboard_ChessPiecePlaced(object sender, ChessPiecePlacedEventArgs e)
        {
            if (e.IsWin)
            {
                await StopThinking();
            }
        }

        private const int playedTimes = 50000;

        private void startThinking()
        {
            cts = new CancellationTokenSource();
            tasks = new List<Task>();
            for (int i = 0; i < 1; i++)
            {
                CancellationToken ct = cts.Token;
                tasks.Add(Task.Run(() =>
                {
                    while (!shouldPauseAfterTimes || rootNode.PlayedTimes < playedTimes)
                    {
                        if (ct.IsCancellationRequested)
                        {
                            break;
                        }
                        rootNode.Search(chessboard.MakeCopy(), rootNode.PlayedTimes);
                    }
                }));
            }
        }

        private async void _player_PlayStarted(object sender, EventArgs e)
        {
            var previousMove = Player.Chessboard.previousMove;
            if (previousMove == null)
            {
                rootNode = new Node(new Position(firstPosition, firstPosition), ChessPieceKind.BlackChessPiece);
            }
            else
            {
                Position previousPosition = new Position(previousMove.X, previousMove.Y);
                shouldPauseAfterTimes = true;

                if (rootNode == null)
                {
                    rootNode = new Node(previousPosition, previousMove.Kind);
                    startThinking();
                    await Task.WhenAll(tasks);
                }
                else
                {
                    await StopThinking();
                    writeRootNodeToChessboard();
                    rootNode = rootNode.NextNodes.SingleOrDefault(n => n.PreviousPlacedPosition.Equals(previousPosition));
                    if (rootNode == null)
                    {
                        rootNode = new Node(previousPosition, previousMove.Kind);
                    }
                    if (rootNode.PlayedTimes < playedTimes)
                    {
                        startThinking();
                        await Task.WhenAll(tasks);
                    }
                }
                writeRootNodeToChessboard();
                rootNode = rootNode.NextNodes.Aggregate((n1, n2) => { return n1.PlayedTimes > n2.PlayedTimes ? n1 : n2; });
            }
            if (!disposedValue)
            {
                bool isWin = Player.PlaceChessPiece(rootNode.PreviousPlacedPosition.X, rootNode.PreviousPlacedPosition.Y);
                if (!isWin)
                {
                    shouldPauseAfterTimes = false;
                    startThinking();
                }
            }
        }

        private void writeRootNodeToChessboard()
        {
            chessboard.PlaceChessPiece(rootNode.PreviousPlacedPosition.X, rootNode.PreviousPlacedPosition.Y);
        }

        public async Task StopThinking()
        {
            cts.Cancel();
            await Task.WhenAll(tasks);
        }

        #region IDisposable Support

        private bool disposedValue = false; // 要检测冗余调用

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                disposedValue = true;

                if (disposing)
                {
                    var t = StopThinking();
                    if (_player != null)
                    {
                        _player.PlayStarted -= _player_PlayStarted;
                        _player.Chessboard.ChessPiecePlaced -= Chessboard_ChessPiecePlaced;
                    }
                }
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        #endregion IDisposable Support
    }
}