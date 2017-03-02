using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FiveInARow;

namespace Test
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            SearchingChessboard chessboard = new SearchingChessboard();
            chessboard.PlaceChessPiece(7, 7);
            chessboard.PlaceChessPiece(7, 8);
            chessboard.PlaceChessPiece(5, 8);
            chessboard.PlaceChessPiece(7, 9);
            chessboard.PlaceChessPiece(5, 9);
            chessboard.PlaceChessPiece(8, 8);
            chessboard.PlaceChessPiece(6, 9);
            chessboard.PlaceChessPiece(9, 7);
            chessboard.PlaceChessPiece(4, 10);
            //chessboard.PlaceChessPiece(10, 6);
            Node node = new Node(new Position(10, 6), ChessPieceKind.BlackChessPiece);
            int playTime = 100000;
            int blackWinTime = 0;
            int writeWinTime = 0;
            for (int i = 0; i < playTime; i++)
            {
                var result = node.PlayOut(chessboard.MakeCopy());
                switch (result)
                {
                    case ChessPieceKind.WriteChessPiece:
                        writeWinTime++;
                        break;

                    case ChessPieceKind.BlackChessPiece:
                        blackWinTime++;
                        break;
                }
            }

            Console.WriteLine($"black: {blackWinTime}");
            Console.WriteLine($"write: {writeWinTime}");

            Console.ReadLine();
        }
    }
}