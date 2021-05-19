using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chess
{
    public enum PieceRank
    {
        King,
        Queen,
        Rook,
        Bishop,
        Knight,
        Pawn,
        None, // for empty squares
    }
    public class Piece
    {
        public bool isWhite;
        public PieceRank rank;

        public Piece(PieceRank rank, bool isWhite)
        {
            this.isWhite = isWhite;
            this.rank = rank;
        }

        public Piece()
        {
            this.rank = PieceRank.None;
            this.isWhite = true;
        }
    }
    public struct Position
    {
        public int col, row;

        public Position(int row, int col)
        {
            this.col = col;
            this.row = row;
        }
    }
    public struct PieceMove
    {
        public Piece piece;
        public bool isDead;
        public Position previous;
        public Position current;
    }
    public enum GameStatus
    {
        WhiteTurn,
        BlackTurn,
        Draw,
        WhiteWin,
        BlackWin,
    }


    class ChessBoard
    {
        private GameStatus Status = GameStatus.WhiteTurn;
        private Piece[,] Board = new Piece[8, 8];


        public Piece[,] GetBoard() => Board;
        public GameStatus GetStatus() => Status;
        public bool IsGameOnGoing() => Status == GameStatus.WhiteTurn || Status == GameStatus.BlackTurn;


        public ChessBoard()
        {
            Status = GameStatus.WhiteTurn;

            // fill the board
            for (int row = 0; row < 8; row++)
            {
                for (int col = 0; col < 8; col++)
                {
                    Board[row, col] = new Piece();
                }
            }

            // i = 0 for white, 1 for black
            for (int i = 0; i < 2; i++)
            {
                Board[i * 7, 0] = new Piece(PieceRank.Rook, Convert.ToBoolean(i));
                Board[i * 7, 1] = new Piece(PieceRank.Knight, Convert.ToBoolean(i));
                Board[i * 7, 2] = new Piece(PieceRank.Bishop, Convert.ToBoolean(i));
                Board[i * 7, 3] = new Piece(PieceRank.Queen, Convert.ToBoolean(i));
                Board[i * 7, 4] = new Piece(PieceRank.King, Convert.ToBoolean(i));
                Board[i * 7, 5] = new Piece(PieceRank.Bishop, Convert.ToBoolean(i));
                Board[i * 7, 6] = new Piece(PieceRank.Knight, Convert.ToBoolean(i));
                Board[i * 7, 7] = new Piece(PieceRank.Rook, Convert.ToBoolean(i));

                // add pawns
                for (int j = 0; j < 8; j++)
                {
                    Board[1 + i * 5, j] = new Piece(PieceRank.Pawn, Convert.ToBoolean(i));
                }
            }
        }


        /// <summary>
        /// If the movement is correct according to the rules of chess, the game-state will change and
        /// return a list of the pieces that were moved. If the movement is illegal, nothing will change
        /// and the list will be empty. The status of the game is also returned.
        /// </summary>
        /// <param name="current">Position of the piece to move.</param>
        /// <param name="next">Where the piece is to be moved.</param>
        public (GameStatus, List<PieceMove>) Play(Position current, Position next)
        {
            List<PieceMove> movedPieces = new List<PieceMove>();

            if (GameFinished(Status) || !IsLegalPosition(current) || !IsLegalPosition(next))
            {
                return (Status, movedPieces);
            }

            Piece piece = GetPieceFromPosition(current);
            if (piece.rank == PieceRank.None || (piece.isWhite && Status != GameStatus.WhiteTurn) || (!piece.isWhite && Status == GameStatus.WhiteTurn)
                || !LegalMoves(current).Contains(next))
            {
                return (Status, movedPieces);
            }

            Piece nextPosPiece = GetPieceFromPosition(next);
            if (nextPosPiece.rank != PieceRank.None)
            {
                PieceMove killedPiece = new PieceMove { isDead = true, piece = nextPosPiece, previous = next, };
                movedPieces.Add(killedPiece);

                if (nextPosPiece.rank == PieceRank.King && nextPosPiece.isWhite)
                {
                    Status = GameStatus.BlackWin;
                }
                else if (nextPosPiece.rank == PieceRank.King && !nextPosPiece.isWhite)
                {
                    Status = GameStatus.WhiteWin;
                }
            }

            PieceMove move = new PieceMove { isDead = false, piece = piece, previous = current, current = next, };
            movedPieces.Add(move);
            Board[next.row, next.col] = piece;
            Board[current.row, current.col] = new Piece();

            Status = ReverseStatus(Status);
            return (Status, movedPieces);
        }


        /// <summary>
        /// If there is piece in the given position, 'LegalMoves' will return every move that this piece
        /// is allowed to make. If there isn't any piece, the list will be empty.
        /// </summary>
        /// <param name="pos"></param>
        /// <returns></returns>
        public List<Position> LegalMoves(Position pos)
        {
            List<Position> legalPos = new List<Position>();
            Piece piece = GetPieceFromPosition(pos);

            // check if the color of the piece corresponds to the player's turn
            if ((piece.isWhite && Status != GameStatus.WhiteTurn) || (!piece.isWhite && Status == GameStatus.WhiteTurn))
            {
                return legalPos;
            }
            
            switch (piece.rank)
            {
                case PieceRank.Pawn:
                    legalPos = PawnMovements(pos, piece);
                    break;
                case PieceRank.Bishop:
                    legalPos = BishopMovements(pos);
                    break;
                case PieceRank.King:
                    legalPos = KingMovements(pos);
                    break;
                case PieceRank.Queen:
                    legalPos = QueenMovements(pos);
                    break;
                case PieceRank.Knight:
                    legalPos = KnightMovements(pos);
                    break;
                case PieceRank.Rook:
                    legalPos = RookMovements(pos);
                    break;
            }

            return legalPos;
        }


        public Piece GetPieceFromPosition(Position pos) => Board[pos.row, pos.col];
        public bool IsOccupiedPosition(Position pos) => GetPieceFromPosition(pos).rank != PieceRank.None;


        private List<Position> PawnMovements(Position pos, Piece piece)
        {
            List<Position> legalMoves = new List<Position>();
            if (piece.isWhite)
            {
                // check forward position
                Position forwardPos = new Position(pos.row - 1, pos.col);
                if (IsLegalPosition(forwardPos) && !IsOccupiedPosition(forwardPos))
                {
                    legalMoves.Add(forwardPos);
                }

                // check 2 position forward (on first move)
                Position forwardTwoPos = new Position(4, pos.col);
                if (pos.row == 6 && !IsOccupiedPosition(forwardPos) && !IsOccupiedPosition(forwardTwoPos))
                {
                    legalMoves.Add(forwardTwoPos);
                }

                // check if it's possible to capture a piece
                Position southWest = new Position(pos.row - 1, pos.col - 1);
                Position southEast = new Position(pos.row - 1, pos.col + 1);
                if (PiecesOfDifferentColors(pos, southWest))
                {
                    legalMoves.Add(southWest);
                }
                if (PiecesOfDifferentColors(pos, southEast))
                {
                    legalMoves.Add(southEast);
                }
            }
            else // black
            {
                // check forward position
                Position forwardPos = new Position(pos.row + 1, pos.col);
                if (IsLegalPosition(forwardPos) && !IsOccupiedPosition(forwardPos))
                {
                    legalMoves.Add(forwardPos);
                }

                // check 2 position forward (on first move)
                Position forwardTwoPos = new Position(3, pos.col);
                if (pos.row == 1 && !IsOccupiedPosition(forwardPos) && !IsOccupiedPosition(forwardTwoPos))
                {
                    legalMoves.Add(forwardTwoPos);
                }

                // check if it's possible to capture a piece
                Position northWest = new Position(pos.row + 1, pos.col - 1);
                Position northEast = new Position(pos.row + 1, pos.col + 1);
                if (PiecesOfDifferentColors(pos, northWest))
                {
                    legalMoves.Add(northWest);
                }
                if (PiecesOfDifferentColors(pos, northEast))
                {
                    legalMoves.Add(northEast);
                }
            }

            return legalMoves;
        }
        private List<Position> BishopMovements(Position pos)
        {
            List<Position> legalMoves = new List<Position>();

            legalMoves.AddRange(NorthWestDiagonal(pos));
            legalMoves.AddRange(NorthEastDiagonal(pos));
            legalMoves.AddRange(SouthWestDiagonal(pos));
            legalMoves.AddRange(SouthEastDiagonal(pos));

            return legalMoves;
        }
        private List<Position> KingMovements(Position pos)
        {
            Position northWest = new Position(pos.row + 1, pos.col - 1);
            Position north = new Position(pos.row + 1, pos.col);
            Position northEast = new Position(pos.row + 1, pos.col + 1);
            Position west = new Position(pos.row, pos.col - 1);
            Position east = new Position(pos.row, pos.col + 1);
            Position southWest = new Position(pos.row - 1, pos.col - 1);
            Position south = new Position(pos.row - 1, pos.col);
            Position southEast = new Position(pos.row - 1, pos.col + 1);

            List<Position> nearPositions = new List<Position>(){ northWest, north, northEast, west, east, southWest, south, southEast };

            var legalMoves =
                from move in nearPositions
                where IsLegalPosition(move)
                where (!IsOccupiedPosition(move) || PiecesOfDifferentColors(pos, move))
                select move;

            return legalMoves.ToList();
        }
        private List<Position> QueenMovements(Position pos)
        {
            List<Position> legalMoves = new List<Position>();

            // Diagonals
            legalMoves.AddRange(NorthWestDiagonal(pos));
            legalMoves.AddRange(NorthEastDiagonal(pos));
            legalMoves.AddRange(SouthWestDiagonal(pos));
            legalMoves.AddRange(SouthEastDiagonal(pos));

            // Horizontal/Vertical
            legalMoves.AddRange(WestHorizontal(pos));
            legalMoves.AddRange(EastHorizontal(pos));
            legalMoves.AddRange(SouthVertical(pos));
            legalMoves.AddRange(NorthVertical(pos));

            return legalMoves;
        }
        private List<Position> RookMovements(Position pos)
        {
            List<Position> legalMoves = new List<Position>();

            legalMoves.AddRange(WestHorizontal(pos));
            legalMoves.AddRange(EastHorizontal(pos));
            legalMoves.AddRange(SouthVertical(pos));
            legalMoves.AddRange(NorthVertical(pos));

            return legalMoves;
        }
        private List<Position> KnightMovements(Position pos)
        {
            Position northLeft = new Position(pos.row + 2, pos.col - 1);
            Position northRight = new Position(pos.row + 2, pos.col + 1);
            Position southLeft = new Position(pos.row - 2, pos.col - 1);
            Position southRight = new Position(pos.row - 2, pos.col + 1);
            Position westUp = new Position(pos.row + 1, pos.col - 2);
            Position westDown = new Position(pos.row - 1, pos.col - 2);
            Position eastUp = new Position(pos.row + 1, pos.col + 2);
            Position eastDown = new Position(pos.row - 1, pos.col + 2);

            List<Position> moves = new List<Position>(){ northLeft, northRight, southLeft, southRight, westUp, westDown, eastUp, eastDown };

            var legalMoves =
                from move in moves
                where IsLegalPosition(move)
                where (!IsOccupiedPosition(move) || PiecesOfDifferentColors(pos, move))
                select move;

            return legalMoves.ToList();
        }

        private List<Position> NorthWestDiagonal(Position pos)
        {
            List<Position> legalMoves = new List<Position>();
            Position newPos = new Position(pos.row + 1, pos.col - 1);

            while (IsLegalPosition(newPos))
            {
                if (!IsOccupiedPosition(newPos) || PiecesOfDifferentColors(pos, newPos))
                {
                    legalMoves.Add(newPos);
                }

                if (IsOccupiedPosition(newPos))
                {
                    break;
                }

                newPos = new Position(newPos.row + 1, newPos.col - 1);
            }

            return legalMoves;
        }
        private List<Position> NorthEastDiagonal(Position pos)
        {
            List<Position> legalMoves = new List<Position>();
            Position newPos = new Position(pos.row + 1, pos.col + 1);

            while (IsLegalPosition(newPos))
            {
                if (!IsOccupiedPosition(newPos) || PiecesOfDifferentColors(pos, newPos))
                {
                    legalMoves.Add(newPos);
                }

                if (IsOccupiedPosition(newPos))
                {
                    break;
                }

                newPos = new Position(newPos.row + 1, newPos.col + 1);
            }

            return legalMoves;
        }
        private List<Position> SouthWestDiagonal(Position pos)
        {
            List<Position> legalMoves = new List<Position>();
            Position newPos = new Position(pos.row - 1, pos.col - 1);

            while (IsLegalPosition(newPos))
            {
                if (!IsOccupiedPosition(newPos) || PiecesOfDifferentColors(pos, newPos))
                {
                    legalMoves.Add(newPos);
                }

                if (IsOccupiedPosition(newPos))
                {
                    break;
                }

                newPos = new Position(newPos.row - 1, newPos.col - 1);
            }

            return legalMoves;
        }
        private List<Position> SouthEastDiagonal(Position pos)
        {
            List<Position> legalMoves = new List<Position>();
            Position newPos = new Position(pos.row - 1, pos.col + 1);

            while (IsLegalPosition(newPos))
            {
                if (!IsOccupiedPosition(newPos) || PiecesOfDifferentColors(pos, newPos))
                {
                    legalMoves.Add(newPos);
                }

                if (IsOccupiedPosition(newPos))
                {
                    break;
                }

                newPos = new Position(newPos.row - 1, newPos.col + 1);
            }

            return legalMoves;
        }
        private List<Position> WestHorizontal(Position pos)
        {
            List<Position> legalMoves = new List<Position>();
            Position newPos = new Position(pos.row, pos.col - 1);

            while (IsLegalPosition(newPos))
            {
                if (!IsOccupiedPosition(newPos) || PiecesOfDifferentColors(pos, newPos))
                {
                    legalMoves.Add(newPos);
                }

                if (IsOccupiedPosition(newPos))
                {
                    break;
                }

                newPos = new Position(newPos.row, newPos.col - 1);
            }

            return legalMoves;
        }
        private List<Position> EastHorizontal(Position pos)
        {
            List<Position> legalMoves = new List<Position>();
            Position newPos = new Position(pos.row, pos.col + 1);

            while (IsLegalPosition(newPos))
            {
                if (!IsOccupiedPosition(newPos) || PiecesOfDifferentColors(pos, newPos))
                {
                    legalMoves.Add(newPos);
                }

                if (IsOccupiedPosition(newPos))
                {
                    break;
                }

                newPos = new Position(newPos.row, newPos.col + 1);
            }

            return legalMoves;
        }
        private List<Position> NorthVertical(Position pos)
        {
            List<Position> legalMoves = new List<Position>();
            Position newPos = new Position(pos.row + 1, pos.col);

            while (IsLegalPosition(newPos))
            {
                if (!IsOccupiedPosition(newPos) || PiecesOfDifferentColors(pos, newPos))
                {
                    legalMoves.Add(newPos);
                }

                if (IsOccupiedPosition(newPos))
                {
                    break;
                }

                newPos = new Position(newPos.row + 1, newPos.col);
            }

            return legalMoves;
        }
        private List<Position> SouthVertical(Position pos)
        {
            List<Position> legalMoves = new List<Position>();
            Position newPos = new Position(pos.row - 1, pos.col);

            while (IsLegalPosition(newPos))
            {
                if (!IsOccupiedPosition(newPos) || PiecesOfDifferentColors(pos, newPos))
                {
                    legalMoves.Add(newPos);
                }

                if (IsOccupiedPosition(newPos))
                {
                    break;
                }

                newPos = new Position(newPos.row - 1, newPos.col);
            }

            return legalMoves;
        }

        private bool IsLegalPosition(int y, int x) => y >= 0 && x >= 0 && y < 8 && x < 8;
        private bool IsLegalPosition(Position pos) => IsLegalPosition(pos.row, pos.col);
        private bool PiecesOfDifferentColors(Position pos1, Position pos2)
        {
            return (IsOccupiedPosition(pos1) && IsOccupiedPosition(pos2)
                    && (GetPieceFromPosition(pos1).isWhite && !GetPieceFromPosition(pos2).isWhite
                    || !GetPieceFromPosition(pos1).isWhite && GetPieceFromPosition(pos2).isWhite));
        }

        private GameStatus ReverseStatus(GameStatus status)
        {
            if (status == GameStatus.WhiteTurn)
            {
                status = GameStatus.BlackTurn;
            }
            else if (status == GameStatus.BlackTurn)
            {
                status = GameStatus.WhiteTurn;
            }

            return status;
        }
        private bool GameFinished(GameStatus status)
        {
            return status != GameStatus.BlackTurn && status != GameStatus.WhiteTurn;
        }
    }
}
