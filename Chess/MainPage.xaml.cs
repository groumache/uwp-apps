using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;


namespace Chess
{
    public sealed partial class MainPage : Page
    {
        // Create a backend chess board
        ChessBoard chessBoard = new ChessBoard();

        // UI components
        private Button[,] ChessBoardUIComponents = new Button[8, 8];
        private TextBlock[,] Indexes = new TextBlock[8, 4];

        private bool isPieceSelected = false;
        private Position selectedPiecePosition;


        public MainPage()
        {
            this.InitializeComponent();
            InitializeChessBoardUI();
        }


        private void InitializeChessBoardUI()
        {
            Piece[,] Board = chessBoard.GetBoard();

            // create the chessboard
            for (int row = 0; row < 8; row++)
            {
                for (int col = 0; col < 8; col++)
                {
                    ChessBoardUIComponents[row, col] = new Button
                    {
                        Content = PieceToString(Board[row, col]),
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center,
                        FontSize = 30,
                        Width = 50,
                        Height = 50,
                        Name = row.ToString() + col.ToString(),
                    };
                    ChessBoardUIComponents[row, col].Click += BoardClick;
                    ChessBoardUI.Children.Add(ChessBoardUIComponents[row, col]);
                    Grid.SetRow(ChessBoardUIComponents[row, col], row);
                    Grid.SetColumn(ChessBoardUIComponents[row, col], col);
                }
            }

            // color the tiles
            CleanBackground();

            // write the letters and numbers next to the board
            List<char> letters = new List<char>() { 'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h' };
            for (int i = 0; i < 8; i++)
            {
                List<Grid> NumGrids = new List<Grid>() { NumbersLeft, NumbersRight };
                List<Grid> LettersGrids = new List<Grid>() { LettersAbove, LettersBelow };

                for (int j = 0; j < 2; j++)
                {
                    // add numbers
                    Indexes[i, j] = new TextBlock
                    {
                        Text = (8 - i).ToString(),
                        FontSize = 30,
                        Width = 50,
                        Height = 50,
                        TextAlignment = TextAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center,
                    };
                    NumGrids[j].Children.Add(Indexes[i, j]);
                    Grid.SetRow(Indexes[i, j], i);
                    Grid.SetColumn(Indexes[i, j], 0);

                    // add letters
                    Indexes[i, 2 + j] = new TextBlock
                    {
                        Text = letters[i].ToString(),
                        FontSize = 30,
                        Width = 50,
                        Height = 50,
                        TextAlignment = TextAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center,
                    };
                    LettersGrids[j].Children.Add(Indexes[i, 2 + j]);
                    Grid.SetRow(Indexes[i, 2 + j], 0);
                    Grid.SetColumn(Indexes[i, 2 + j], i);
                }
            }
        }
        private string PieceToString(Piece piece)
        {
            char c = ' ';

            if (piece.isWhite)
            {
                c = WhitePieceToChar(piece, c);
            }
            else
            {
                c = BlackPieceToChar(piece, c);
            }

            return c.ToString();
        }
        private static char WhitePieceToChar(Piece piece, char c)
        {
            switch (piece.rank)
            {
                case PieceRank.King: // White King
                    c = '\u2654';
                    break;
                case PieceRank.Queen:
                    c = '\u2655';
                    break;
                case PieceRank.Rook:
                    c = '\u2656';
                    break;
                case PieceRank.Bishop:
                    c = '\u2657';
                    break;
                case PieceRank.Knight:
                    c = '\u2658';
                    break;
                case PieceRank.Pawn:
                    c = '\u2659';
                    break;
                default:
                    break;
            }

            return c;
        }
        private static char BlackPieceToChar(Piece piece, char c)
        {
            switch (piece.rank)
            {
                case PieceRank.King: // Black King
                    c = '\u265A';
                    break;
                case PieceRank.Queen:
                    c = '\u265B';
                    break;
                case PieceRank.Rook:
                    c = '\u265C';
                    break;
                case PieceRank.Bishop:
                    c = '\u265D';
                    break;
                case PieceRank.Knight:
                    c = '\u265E';
                    break;
                case PieceRank.Pawn:
                    c = '\u265F';
                    break;
                default:
                    break;
            }

            return c;
        }


        // handles user input
        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            chessBoard = new ChessBoard();
            isPieceSelected = false;
            InitializeChessBoardUI();
        }
        private void BoardClick(object sender, RoutedEventArgs e)
        {
            // NOTE: b.Name = "row" + "col"
            Button b = sender as Button;
            string pos = b.Name.ToString();
            int row = System.Globalization.CharUnicodeInfo.GetDecimalDigitValue(pos[0]);
            int col = System.Globalization.CharUnicodeInfo.GetDecimalDigitValue(pos[1]);
            Position position = new Position(row, col);

            if (isPieceSelected && chessBoard.LegalMoves(selectedPiecePosition).Contains(position))
            {
                // move the piece to the position, clean the background and unselect the piece
                var pieceToMove = ChessBoardUIComponents[selectedPiecePosition.row, selectedPiecePosition.col].Content;

                GameStatus previousStatus = chessBoard.GetStatus();
                (GameStatus currentStatus, _) = chessBoard.Play(selectedPiecePosition, position);

                if (previousStatus != currentStatus) // ex: WhiteTurn -> BlackTurn
                {
                    ChessBoardUIComponents[position.row, position.col].Content = pieceToMove;
                    ChessBoardUIComponents[selectedPiecePosition.row, selectedPiecePosition.col].Content = "";
                }
                
                CleanBackground();
                isPieceSelected = false;
            }
            else if (isPieceSelected && chessBoard.LegalMoves(selectedPiecePosition).Contains(position))
            {
                // clean the background and unselect the piece
                CleanBackground();
                isPieceSelected = false;
            }
            else if (!isPieceSelected && chessBoard.IsOccupiedPosition(position))
            {
                // check if the color of the piece and the player's turn are the same
                if ((chessBoard.GetStatus() == GameStatus.WhiteTurn && !chessBoard.GetPieceFromPosition(position).isWhite)
                    || (chessBoard.GetStatus() == GameStatus.BlackTurn && chessBoard.GetPieceFromPosition(position).isWhite))
                {
                    return;
                }

                // check if the game is ongoing
                if (!chessBoard.IsGameOnGoing())
                {
                    return;
                }

                // select the piece and show the legal moves (only if the piece can move)
                List<Position> legalMoves = chessBoard.LegalMoves(position);
                if (legalMoves.Count() != 0)
                {
                    foreach (Position legalPos in legalMoves)
                    {
                        ChessBoardUIComponents[legalPos.row, legalPos.col].Background = new SolidColorBrush(Windows.UI.Colors.LightBlue);
                    }
                    isPieceSelected = true;
                    selectedPiecePosition = position;
                }
            }
            else if (isPieceSelected)
            {
                // unselect the piece and clean the background
                isPieceSelected = false;
                CleanBackground();
            }
        }


        // The colors of the squares are restored
        private void CleanBackground()
        {
            for (int row = 0; row < 8; row++)
            {
                for (int col = 0; col < 8; col++)
                {
                    if ((row % 2 == 0 && col % 2 == 0) || (row % 2 == 1 && col % 2 == 1))
                    {
                        ChessBoardUIComponents[row, col].Background = new SolidColorBrush(Windows.UI.Colors.DarkGray);
                    }
                    else if ((row % 2 == 0 && col % 2 == 1) || (row % 2 == 1 && col % 2 == 0))
                    {
                        ChessBoardUIComponents[row, col].Background = new SolidColorBrush(Windows.UI.Colors.White);
                    }
                }
            }
        }
    }
}
