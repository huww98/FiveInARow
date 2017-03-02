using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace FiveInARow
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private const double chessboardSize = 315.0;
        private const double chessboardSizePerLine = chessboardSize / Chessboard.LineNum;
        private const double chessPieceSize = 17.0;
        private const double chessPieceOffset = (chessboardSizePerLine - chessPieceSize) / 2;

        private readonly SolidColorBrush blackChessBrush = new SolidColorBrush(Color.FromArgb(255, 0, 0, 0));
        private readonly SolidColorBrush writeChessBrush = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255));

        private Chessboard chessboard;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void initializeGame()
        {
            foreach (var item in ais.Values)
            {
                item?.Dispose();
            }

            chessboard = new Chessboard();
            chessboard.ChessPiecePlaced += Chessboard_ChessPiecePlaced;

            chessboardCanvas.Children.Clear();
            previousMoveIndicator.Visibility = Visibility.Hidden;
            Player player1 = new Player { Name = "玩家1", Kind = PlayerKind.Human };
            Player player2 = new Player { Name = "玩家2", Kind = PlayerKind.Human };
            chessboard.BlackPlayer = player1;
            chessboard.WritePlayer = player2;

            registerPlayerInput(chessboard.BlackPlayer);
            registerPlayerInput(chessboard.WritePlayer);

            this.DataContext = chessboard;
        }

        private void Chessboard_ChessPiecePlaced(object sender, ChessPiecePlacedEventArgs e)
        {
            this.Dispatcher.Invoke(() =>
            {
                setChessPiece(e.ChessPiece);
            });

            if (e.IsWin)
            {
                MessageBox.Show("Win!");
            }
        }

        private void registerPlayerInput(Player player)
        {
            player.PlayStarted += Player_PlayStarted;
            if (player.IsPlaying)
            {
                Player_PlayStarted(player, new EventArgs());
            }
        }

        private void unregisterPlayerInput(Player player)
        {
            player.PlayStarted -= Player_PlayStarted;
        }

        private void Player_PlayStarted(object sender, EventArgs e)
        {
            Player p = (Player)sender;
            previewChessPiece = createChessPieceShape(p.UsingChessPiece);
            chessboardBackground.MouseMove += chessboard_MouseMove;
            chessboardBackground.MouseLeftButtonUp += chessboard_MouseLeftButtonUp;
        }

        private Ellipse createChessPieceShape(ChessPieceKind kind)
        {
            var newShape = new Ellipse
            {
                Height = chessPieceSize,
                Width = chessPieceSize,
                Stroke = new SolidColorBrush(Color.FromArgb(255, 0, 0, 0)),
                Fill = kind == ChessPieceKind.BlackChessPiece ? blackChessBrush : writeChessBrush,
                Visibility = Visibility.Hidden
            };
            chessboardCanvas.Children.Add(newShape);

            return newShape;
        }

        private Ellipse previewChessPiece;
        private int previewX, previewY;

        private void chessboard_MouseMove(object sender, MouseEventArgs e)
        {
            setChessPiecePreview(e.GetPosition(chessboardCanvas));
        }

        private void setChessPiecePreview(Point mousePosition)
        {
            previewX = (int)(mousePosition.X / chessboardSizePerLine);
            previewY = (int)(mousePosition.Y / chessboardSizePerLine);
            if (!chessboard.CanPlaceChessPiece(previewX, previewY))
            {
                previewChessPiece.Visibility = Visibility.Hidden;
                return;
            }

            Canvas.SetLeft(previewChessPiece, previewX * chessboardSizePerLine + chessPieceOffset);
            Canvas.SetTop(previewChessPiece, previewY * chessboardSizePerLine + chessPieceOffset);
            previewChessPiece.Visibility = Visibility.Visible;
        }

        private void setChessPiece(ChessPieceInfo chessPieceInfo)
        {
            var chessPiece = createChessPieceShape(chessPieceInfo.Kind);
            chessPiece.Visibility = Visibility.Visible;
            previousMoveIndicator.Visibility = Visibility.Visible;
            double left = chessPieceInfo.X * chessboardSizePerLine + chessPieceOffset;
            var top = chessPieceInfo.Y * chessboardSizePerLine + chessPieceOffset;
            Canvas.SetLeft(chessPiece, left);
            Canvas.SetTop(chessPiece, top);
            Canvas.SetLeft(previousMoveIndicator, left);
            Canvas.SetTop(previousMoveIndicator, top);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            initializeGame();
            chessboard.StartGame();
        }

        private Dictionary<ChessPieceKind, AI> ais = new Dictionary<ChessPieceKind, AI>
        {
            { ChessPieceKind.BlackChessPiece, null },
            { ChessPieceKind.WriteChessPiece, null }
        };

        private void switchPlayerKind(Player player)
        {
            if (chessboard == null)
            {
                return;
            }
            if (player.Kind == PlayerKind.Human)
            {
                unregisterPlayerInput(player);
                if (player.IsPlaying)
                {
                    removePreview();
                }
                ais[player.UsingChessPiece] = new AI { Player = player };
                player.Kind = PlayerKind.AI;
            }
            else
            {
                ais[player.UsingChessPiece].Dispose();
                registerPlayerInput(player);
                player.Kind = PlayerKind.Human;
            }
        }

        private void blackPlayerSwitch_Click(object sender, RoutedEventArgs e)
        {
            if (chessboard != null)
            {
                switchPlayerKind(chessboard.BlackPlayer);
            }
        }

        private void writePlayerSwitch_Click(object sender, RoutedEventArgs e)
        {
            if (chessboard != null)
            {
                switchPlayerKind(chessboard.WritePlayer);
            }
        }

        private void chessboard_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            setChessPiecePreview(e.GetPosition(chessboardCanvas));
            if (previewChessPiece.Visibility == Visibility.Visible)
            {
                removePreview();
                chessboardBackground.MouseMove -= chessboard_MouseMove;
                chessboardBackground.MouseLeftButtonUp -= chessboard_MouseLeftButtonUp;

                chessboard.PlayingPlayer.PlaceChessPiece(previewX, previewY);
            }
        }

        private void removePreview()
        {
            chessboardCanvas.Children.Remove(previewChessPiece);
        }
    }

    internal class ChessPiece
    {
        public ChessPieceInfo Info { get; set; }
        public Ellipse Shape { get; set; }
    }
}