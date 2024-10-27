using System;
using System.ComponentModel;
using System.Drawing;
using System.Reflection.Metadata.Ecma335;
using System.Threading;

namespace System
{
    internal class Program
    {
        static void Main(string[] args)
        {
            ChessGame game = new ChessGame();
            game.startGame();
        }
    }
    class ChessGame
    {
        bool isWhiteTurn = true;
        bool endGame = false;
        Location lastMoveEarlyLocation = new Location(1, 1), lastMoveFinalLocation = new Location(1, 1);
        Location BlackKingLocation = new Location(1, 5), WhiteKingLocation = new Location(8, 5);
        int fiftyMovesRuleCountTo100moves;
        string boardString = "";
        Location location, toLocation;
        public void startGame()
        {
            string input;
            ChessPiece[,] board = createBoard();
            printBoard(board);
            do
            {
                if (endGame == true)
                    input = "ENOUGH";
                else
                {
                    Console.WriteLine("its {0} turn. please enter the curr LETTER of row and curr NUMBER of col and, and the new LETTER of row and new NUMBER of col." +
                    "\nYou can also ask the opponent agree to a DRAW", isWhiteTurn ? "WHITE" : "BLACK");
                    input = Console.ReadLine();
                    input = input.Trim();
                    input = input.ToUpper();
                }
                if (input != "DRAW")
                {
                    if (inputChek(input))
                    {
                        location = new Location(numberToCurrectNumber(input, 1), letterToNumber(input, 0));
                        toLocation = new Location(numberToCurrectNumber(input, 3), letterToNumber(input, 2));
                    }
                    else
                    {
                        continue;
                    }

                    if ((board[location.getNumberLocation(), location.getLetterLocation()] != null) && isMoveLegal(location, toLocation, board, lastMoveEarlyLocation, lastMoveFinalLocation, true))
                    {
                        if (isItCastling(location, toLocation, board) &&
                            isCastlingValid(location, toLocation, board, lastMoveEarlyLocation, lastMoveFinalLocation))
                        {
                            doTheCastling(location, toLocation, board);
                        }
                        else if (isItEnPassant(location, toLocation, board))
                        {
                            doTheEnPassant(location, toLocation, board);
                        }
                        else
                        {
                            update50MovesRuleCount(board, location, toLocation);
                            doTheMove(location, toLocation, board);
                        }
                        printBoard(board);

                        endFirstTurn(toLocation, board);
                    }
                    else
                    {
                        Console.WriteLine("this is not valid movment");
                        continue;
                    }

                    afterTurnCheksAndEffects(location, toLocation, board, lastMoveEarlyLocation, lastMoveFinalLocation);
                }

                else if (input == "DRAW")
                {
                    String YorN;
                    bool toEndLoop = false;
                    do
                    {
                        Console.WriteLine("{0} do you agree to a DRAW? \npress Y to AGREE or N to DECLINE.", isWhiteTurn ? "BLACK" : "WHITE");
                        YorN = Console.ReadLine();
                        YorN = YorN.Trim().ToUpper();
                        switch (YorN)
                        {
                            case "Y":
                                {
                                    Console.WriteLine("its a draw!");
                                    toEndLoop = true;
                                    input = "ENOUGH";
                                    break;
                                }
                            case "N":
                                {
                                    Console.WriteLine("the game continues!");
                                    toEndLoop = true;
                                    break;
                                }
                            default:
                                Console.WriteLine("invalid input Y or N only");

                                break;
                        }

                    } while (!toEndLoop);

                }
            } while (input != "ENOUGH");

        }
        #region board related funcs
        public ChessPiece[,] createBoard() //create strting board
        {
            ChessPiece[,] board = { { null,null, null, null, null, null, null, null, null },
                                    { null, new Rook(false,true), new Knight(false), new Bishop(false), new Queen(false),
                                            new King(false,true), new Bishop(false), new Knight(false), new Rook(false, true) },
                                    { null, new Pawn(false), new Pawn(false), new Pawn(false), new Pawn(false),new Pawn(false), new Pawn(false), new Pawn(false), new Pawn(false) },
                                    { null, null,            null,            null,            null,           null,            null,            null,            null },
                                    { null, null,            null,            null,            null,           null,            null,            null,            null },
                                    { null, null,            null,            null,            null,           null,            null,            null,            null },
                                    { null, null,            null,            null,            null,           null,            null,            null,            null },
                                    { null, new Pawn(true),  new Pawn(true),  new Pawn(true),  new Pawn(true), new Pawn(true),  new Pawn(true),  new Pawn(true),  new Pawn(true) },
                                    { null, new Rook(true, true), new Knight(true), new Bishop(true), new Queen(true),
                                            new King(true, true), new Bishop(true), new Knight(true), new Rook(true, true) } };

            return board;
        }
        public void printBoard(ChessPiece[,] board) //printBoard board
        {
            int BoardNumbers = 8, BoardLetterUniCode = 65;
            for (int i = 0; i < board.GetLength(0); i++)
            {
                for (int j = 0; j < board.GetLength(1); j++)
                    if (board[i, j] is ChessPiece)
                        Console.Write(board[i, j]);
                    else
                    {
                        if (i == 0)
                        {
                            if (j == 0)
                                Console.Write("-- ");
                            if (j != 0)
                                Console.Write((char)(BoardLetterUniCode++) + "  ");
                        }
                        else
                        {
                            if (j == 0)
                                Console.Write(BoardNumbers-- + "  ");
                            else
                                Console.Write("-- ");
                        }
                    }
                Console.WriteLine();
            }
            Console.WriteLine();
        }
        public string makeBoardString(ChessPiece[,] playBoard, Location lastMoveEarlyLocation, Location lastMoveFinalLocation)
        {
            string result = "";

            for (int i = 1; i < playBoard.GetLength(0); i++)
            {
                for (int j = 1; j < playBoard.GetLength(1); j++)
                {
                    if (playBoard[i, j] is ChessPiece)
                    {
                        char pieceName = playBoard[i, j].ToString()[2];
                        switch (pieceName)
                        {
                            case 'K':
                                result += playBoard[i, j].ToString();
                                result += (playBoard[i, j] as King).getIsFirstTurn() ? "t," : "f,";
                                break;
                            case 'R':
                                result += playBoard[i, j].ToString();
                                result += (playBoard[i, j] as Rook).getIsFirstTurn() ? "t," : "f,";
                                break;
                            case 'P':
                                result += playBoard[i, j].ToString();
                                if ((i == (playBoard[i, j].getIsWhite() ? 4 : 5)) &&  //eater side //En Passant Position
                                    (playBoard[lastMoveFinalLocation.getNumberLocation(), lastMoveFinalLocation.getLetterLocation()] is Pawn) && //eaten side
                                    (lastMoveLengthOfVerticalVector() == 2) &&
                                    lastMoveFinalLocation.getNumberLocation() == i &&
                                    Math.Abs(lastMoveFinalLocation.getLetterLocation() - j) == 1)
                                {
                                    result += "t,";
                                }
                                else
                                    result += "f,";

                                break;
                            default:
                                result += playBoard[i, j].ToString() + ",";
                                break;
                        }
                    }

                    else
                        result += "nl,";
                }
            }
            result += getIsWhiteTurn() ? "BlackTurn?" : "WhiteTurn?";
            return result;
        }
        public string getBoardString()
        { return boardString; }
        public bool setBoardString(string str)
        { boardString = str; return true; }
        #endregion board related funcs
        #region check and draw
        public bool isDraw(string boardString, ChessPiece[,] playBoard, Location lastMoveEarlyLocation, Location lastMoveFinalLocationn)
        {
            if (isStalemateDraw(playBoard, lastMoveEarlyLocation, lastMoveFinalLocation))
            {
                return true;
            }
            if (isDeadPositionDraw(playBoard))
            {
                return true;
            }
            if (is50MoveRuleDraw())
            {
                return true;
            }
            if (isThreeFoldRepetitionDraw(boardString, playBoard, lastMoveEarlyLocation, lastMoveFinalLocation) == true)
            {
                return true;
            }

            return false;
        }
        public bool isThreeFoldRepetitionDraw(string boardString, ChessPiece[,] playBoard, Location lastMoveEarlyLocation, Location lastMoveFinalLocation)
        {
            if (isThreeFoldRepetitionOption(boardString, playBoard, lastMoveEarlyLocation, lastMoveFinalLocation) == true)
            {
                Console.WriteLine("you can claim a draw due Three Fold Repetition rule");
                String YorN;
                bool toEndLoop = false;

                do
                {
                    Console.WriteLine("do you want to claim a DRAW? \npress Y to AGREE or N to DECLINE.");
                    YorN = Console.ReadLine();
                    YorN = YorN.Trim().ToUpper();
                    switch (YorN)
                    {
                        case "Y":
                            {
                                toEndLoop = true;
                                return true;
                            }
                        case "N":
                            {
                                toEndLoop = true;
                                return false;
                            }
                        default:
                            Console.WriteLine("invalid input Y or N only");

                            break;
                    }

                } while (!toEndLoop);

            }
            return false;
        }
        public bool isThreeFoldRepetitionOption(string boardString, ChessPiece[,] playBoard, Location lastMoveEarlyLocation, Location lastMoveFinalLocation)
        {
            string newBordString = "";
            newBordString = makeBoardString(playBoard, lastMoveEarlyLocation, lastMoveFinalLocation);
            setBoardString(getBoardString() + newBordString);
            int RepetitionCount = 0;
            string[] strings = getBoardString().Split('?');
            for (int i = 0; i < strings.Length; i++)
            {
                RepetitionCount = 0;
                for (int j = i; j < strings.Length; j++)
                {
                    if (strings[i] == strings[j])
                        RepetitionCount++;
                    if (RepetitionCount >= 3)
                        return true;
                }
            }
            return false;
        }
        public bool is50MoveRuleDraw()
        {
            if (isFiftyMoveRulePossible() == true)
            {
                Console.WriteLine("you can claim a draw dou 50 moves rule");
                String YorN;
                bool toEndLoop = false;

                do
                {
                    Console.WriteLine("do you want to claim a DRAW? \npress Y to AGREE or N to DECLINE.");
                    YorN = Console.ReadLine();
                    YorN = YorN.Trim().ToUpper();
                    switch (YorN)
                    {
                        case "Y":
                            {
                                toEndLoop = true;
                                return true;
                            }
                        case "N":
                            {
                                toEndLoop = true;
                                return false;
                            }
                        default:
                            Console.WriteLine("invalid input Y or N only");

                            break;
                    }

                } while (!toEndLoop);

            }
            return false;
        }
        #region 50 moves related funcs
        public bool isFiftyMoveRulePossible()
        {
            if (fiftyMovesRuleCountTo100moves >= 100)
                return true;
            return false;
        }
        public bool update50MovesRuleCount(ChessPiece[,] playBoard, Location location, Location toLocation)
        {
            addOneToFiftyMovesRuleCount();
            setToZeroFiftyMovesRuleCount(playBoard, location, toLocation);
            return true;
        }
        public void addOneToFiftyMovesRuleCount()
        { fiftyMovesRuleCountTo100moves++; }
        public void setToZeroFiftyMovesRuleCount(ChessPiece[,] playBoard, Location location, Location toLocation)
        {
            if ((playBoard[location.getNumberLocation(), location.getLetterLocation()] is Pawn) || playBoard[toLocation.getNumberLocation(), toLocation.getLetterLocation()] != null)
                fiftyMovesRuleCountTo100moves = 0;
        }
        #endregion 50 moves related funcs
        public bool isDeadPositionDraw(ChessPiece[,] playBoard)
        {
            int piecesCount = 0, kingCount = 0, knightCount = 0, blacBishopCount = 0, WhiteBishopCount = 0, BishopSquerColor = -1;
            for (int i = 1; i < playBoard.GetLength(0); i++)
            {
                for (int j = 1; j < playBoard.GetLength(1); j++)
                {
                    if (playBoard[i, j] != null)
                    {
                        piecesCount++;
                        if (piecesCount > 4)
                            return false;
                        char pieceName = ' ';
                        pieceName = playBoard[i, j].getName();
                        switch (pieceName)
                        {
                            case 'K':
                                kingCount++;
                                break;
                            case 'N':
                                knightCount++;
                                if (knightCount > 1)
                                    return false;
                                break;
                            case 'B':
                                if (playBoard[i, j].getIsWhite())
                                {
                                    WhiteBishopCount++;
                                    if (WhiteBishopCount > 1)
                                        return false;
                                    if (BishopSquerColor != -1 && BishopSquerColor != (i + j) % 2)
                                        return false;
                                    BishopSquerColor = (i + j) % 2;
                                    break;
                                }
                                blacBishopCount++;
                                if (blacBishopCount > 1)
                                    return false;
                                if (BishopSquerColor != -1 && BishopSquerColor != (i + j) % 2)
                                    return false;
                                BishopSquerColor = (i + j) % 2;
                                break;

                            default:
                                return false;
                        }
                    }
                }
            }
            return true;
        }
        public bool isStalemateDraw(ChessPiece[,] playBoard, Location lastMoveEarlyLocation, Location lastMoveFinalLocation)
        {
            bool condition1, temp = false;
            condition1 = (isKingThreatened(playBoard, lastMoveEarlyLocation, lastMoveFinalLocation) == true);
            if (condition1 == true)
                return false;
            for (int i = 1; i < playBoard.GetLength(0); i++)
            {
                for (int j = 1; j < playBoard.GetLength(1); j++)
                {
                    Location testSquare = new Location(i, j);
                    temp = isSomeOneIsAbleToMove(testSquare, playBoard, lastMoveEarlyLocation, lastMoveFinalLocation);
                    if (temp == true)
                    {
                        if ((temp == true) && (condition1 == false))
                            return false;
                    }
                }
            }
            return true;
        }
        public bool isChekmate(ChessPiece[,] playBoard, Location lastMoveEarlyLocation, Location lastMoveFinalLocation)
        {
            bool condition1;
            condition1 = (isKingThreatened(playBoard, lastMoveEarlyLocation, lastMoveFinalLocation) == true);
            if (condition1 == false)
                return false;
            for (int i = 1; i < playBoard.GetLength(0); i++) //change back to 1
            {
                for (int j = 1; j < playBoard.GetLength(1); j++) //change back to 1
                {
                    Location testSquare = new Location(i, j);
                    if (isSomeOneIsAbleToMove(testSquare, playBoard, lastMoveEarlyLocation, lastMoveFinalLocation) == true)
                    {
                        return false;
                    }
                }
            }
            return true;
        }
        public bool isSomeOneIsAbleToMove(Location location, ChessPiece[,] playBoard, Location lastMoveEarlyLocation, Location lastMoveFinalLocation)
        {
            if (playBoard[location.getNumberLocation(), location.getLetterLocation()] != null)
            {
                for (int i = 1; i < playBoard.GetLength(0); i++)
                {
                    for (int j = 1; j < playBoard.GetLength(1); j++)
                    {
                        Location testSquare = new Location(i, j);
                        if (isMoveLegal(location, testSquare, playBoard, lastMoveEarlyLocation, lastMoveFinalLocation, true) == true)
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }
        #endregion check and draw
        #region move ligalitty
        #region calculation of diraction by the difrence beteween the location and destenation
        #region left&right
        public int newLetterLocationMinusCurrentLetterLocation()
        {
            return toLocation.getLetterLocation() - location.getLetterLocation();
        }
        public bool isHorizontalVectorToTheRight()
        {
            return newLetterLocationMinusCurrentLetterLocation() > 0;
        }
        public int lengthOfHorizontalVector()
        {
            return Math.Abs(newLetterLocationMinusCurrentLetterLocation());
        }
        #endregion left&right
        #region up&down
        public int newNumberLocationMinusCurrentNumberLocation()
        {
            return toLocation.getNumberLocation() - location.getNumberLocation();
        }
        public bool isVerticalVectorPointingDown()
        {
            return newNumberLocationMinusCurrentNumberLocation() > 0;
        }
        public int lengthOfVerticalVector()
        {
            return Math.Abs(newNumberLocationMinusCurrentNumberLocation());
        }

        #endregion up&down
        #region memo
        public int LastMoveFinalNumberLocationMinuslastMoveEarlyNumberLocation()
        {
            return lastMoveFinalLocation.getNumberLocation() - lastMoveEarlyLocation.getNumberLocation();
        }
        public int lastMoveLengthOfVerticalVector()
        {
            return Math.Abs(LastMoveFinalNumberLocationMinuslastMoveEarlyNumberLocation());
        }
        #endregion memo
        #endregion calculation of diraction by the difrence beteween the location and destenation
        public bool isMoveLegal(Location location, Location toLocation, ChessPiece[,] playBoard, Location lastMoveEarlyLocation, Location lastMoveFinalLocation, bool isNeededToChekIfKingThreatenedAfterMovement) // will have more conditions
        {

            if ((playBoard[location.getNumberLocation(), location.getLetterLocation()] != null) &&
                playBoard[location.getNumberLocation(), location.getLetterLocation()].getIsWhite() == getIsWhiteTurn() &&
                location != toLocation &&
                playBoard[location.getNumberLocation(), location.getLetterLocation()].ChekMovmentValid(location, toLocation, playBoard, lastMoveEarlyLocation, lastMoveFinalLocation) &&
                ChekMovementEatingRightColor(toLocation, playBoard) &&
                ChekIsClearPath(location, toLocation, playBoard) &&
                (isNeededToChekIfKingThreatenedAfterMovement ? ((isKingThreatenedAfterMovement(location, toLocation, playBoard, lastMoveEarlyLocation, lastMoveFinalLocation)) == false) : true))
            {
                if (isItCastling(location, toLocation, playBoard))
                {
                    if (isCastlingValid(location, toLocation, playBoard, lastMoveEarlyLocation, lastMoveFinalLocation))
                        return true;
                    else
                        return false;
                }
                return true;
            }
            return false;
        }
        public bool isItEnPassant(Location location, Location toLocation, ChessPiece[,] playBoard)
        {
            if ((playBoard[location.getNumberLocation(), location.getLetterLocation()] is Pawn) && (playBoard[toLocation.getNumberLocation(), toLocation.getLetterLocation()] == null) && ((lengthOfHorizontalVector()) == 1))
                return true;
            return false;
        }
        public void doTheEnPassant(Location location, Location toLocation, ChessPiece[,] playBoard)
        {
            update50MovesRuleCount(playBoard, location, toLocation);
            doTheMove(location, toLocation, playBoard);
            playBoard[location.getNumberLocation(), toLocation.getLetterLocation()] = null;
        }
        public bool isItCastling(Location location, Location toLocation, ChessPiece[,] playBoard)
        {
            if (playBoard[location.getNumberLocation(), location.getLetterLocation()] is King && (lengthOfHorizontalVector()) == 2)
            {
                return true;
            }
            return false;
        }
        public bool isCastlingValid(Location location, Location toLocation, ChessPiece[,] playBoard, Location lastMoveEarlyLocation, Location lastMoveFinalLocation)
        {
            Location castlingKingMidMovementSquare = new Location(toLocation.getNumberLocation(), toLocation.getLetterLocation() + (isHorizontalVectorToTheRight() ? 1 : -1));
            if ((isKingThreatened(playBoard, lastMoveEarlyLocation, lastMoveFinalLocation) == false) &&
                (isThreatenedPlace(toLocation, playBoard, lastMoveEarlyLocation, lastMoveFinalLocation) == false) &&
                (isThreatenedPlace(castlingKingMidMovementSquare, playBoard, lastMoveEarlyLocation, lastMoveFinalLocation) == false))
            {
                if ((playBoard[location.getNumberLocation(), isHorizontalVectorToTheRight() ? 8 : 1] is Rook) &&
                    ((playBoard[location.getNumberLocation(), isHorizontalVectorToTheRight() ? 8 : 1] as Rook).getIsFirstTurn()))
                {
                    return true;
                }
                else
                    return false;
            }
            else return false;
        }
        public void doTheCastling(Location location, Location toLocation, ChessPiece[,] playBoard)
        {
            Location CastlingRookLocation = new Location(location.getNumberLocation(), isHorizontalVectorToTheRight() ? 8 : 1);
            Location CastlingRookDestination = new Location(location.getNumberLocation(), isHorizontalVectorToTheRight() ? 6 : 4);
            update50MovesRuleCount(playBoard, location, toLocation);
            doTheMove(location, toLocation, playBoard);
            doTheMove(CastlingRookLocation, CastlingRookDestination, playBoard);
            (playBoard[location.getNumberLocation(), isHorizontalVectorToTheRight() ? 6 : 4] as Rook).setIsFirstTurn(false);
        }
        public bool ChekIsClearPath(Location location, Location toLocation, ChessPiece[,] playBoard)
        {
            if (lengthOfHorizontalVector() > 1)
            {
                if ((isVerticalVectorPointingDown() == false) && lengthOfHorizontalVector() == 0) //up up
                {
                    for (int i = (location.getNumberLocation() - 1); i > toLocation.getNumberLocation(); i--)
                    {
                        if (playBoard[i, location.getLetterLocation()] != null)
                            return false;
                    }
                }
                else if (isVerticalVectorPointingDown() && lengthOfHorizontalVector() == 0) //down down
                {
                    for (int i = (location.getNumberLocation() + 1); i < toLocation.getNumberLocation(); i++)
                    {
                        if (playBoard[i, location.getLetterLocation()] != null)
                            return false;
                    }
                }
                else if (lengthOfVerticalVector() == 0 && isHorizontalVectorToTheRight()) //right right
                {
                    for (int i = (location.getLetterLocation() + 1); i < toLocation.getLetterLocation(); i++)
                    {
                        if (playBoard[location.getNumberLocation(), i] != null)
                            return false;
                    }
                }
                else if (lengthOfVerticalVector() == 0 && (isHorizontalVectorToTheRight() == false)) //left left
                {
                    for (int i = (location.getLetterLocation() - 1); i > toLocation.getLetterLocation(); i--)
                    {
                        if (playBoard[location.getNumberLocation(), i] != null)
                            return false;
                    }
                }
                else if ((isVerticalVectorPointingDown() == false) && (isHorizontalVectorToTheRight() == false))//up left
                {
                    for (int i = (location.getNumberLocation() - 1), j = (location.getLetterLocation() - 1); (i > toLocation.getNumberLocation()) && (j > toLocation.getLetterLocation()); i--, j--)
                    {
                        if (playBoard[i, j] != null)
                            return false;
                    }
                }
                else if ((isVerticalVectorPointingDown() == false) && isHorizontalVectorToTheRight())//up right
                {
                    for (int i = (location.getNumberLocation() - 1), j = (location.getLetterLocation() + 1); (i > toLocation.getNumberLocation()) && (j < toLocation.getLetterLocation()); i--, j++)
                    {
                        if (playBoard[i, j] != null)
                            return false;
                    }

                }
                else if (isVerticalVectorPointingDown() && (isHorizontalVectorToTheRight() == false))//down left
                {
                    for (int i = (location.getNumberLocation() + 1), j = (location.getLetterLocation() - 1); (i < toLocation.getNumberLocation()) && (j > toLocation.getLetterLocation()); i++, j--)
                    {
                        if (playBoard[i, j] != null)
                            return false;
                    }
                }
                else if (isVerticalVectorPointingDown() && isHorizontalVectorToTheRight())//down right
                {
                    for (int i = (location.getNumberLocation() + 1), j = (location.getLetterLocation() + 1); (i < toLocation.getNumberLocation()) && (j < toLocation.getLetterLocation()); i++, j++)
                    {
                        if (playBoard[i, j] != null)
                            return false;
                    }
                }
            }
            return true;
        }
        public bool ChekMovementEatingRightColor(Location toLocation, ChessPiece[,] playBoard) //no knibalism
        {
            bool result = true;
            if (playBoard[toLocation.getNumberLocation(), toLocation.getLetterLocation()] != null)
                result = (playBoard[toLocation.getNumberLocation(), toLocation.getLetterLocation()].getIsWhite() != playBoard[toLocation.getNumberLocation(), toLocation.getLetterLocation()].getIsWhite() ? true : false);

            return result;
        }
        public void doTheMove(Location location, Location toLocation, ChessPiece[,] playBoard)
        {
            playBoard[toLocation.getNumberLocation(), toLocation.getLetterLocation()] = playBoard[location.getNumberLocation(), location.getLetterLocation()];
            playBoard[location.getNumberLocation(), location.getLetterLocation()] = null;

        }
        #endregion move ligalitty
        #region threat related funcs
        public bool isKingThreatenedAfterMovement(Location location, Location toLocation, ChessPiece[,] playBoard, Location lastMoveEarlyLocation, Location lastMoveFinalLocation)
        {
            bool result = false;
            ChessPiece movingPieceHolder = playBoard[location.getNumberLocation(), location.getLetterLocation()]; //move the piece and chek the king
            {
                playBoard[location.getNumberLocation(), location.getLetterLocation()] = null;//last change
                if (playBoard[toLocation.getNumberLocation(), toLocation.getLetterLocation()] == null)
                    playBoard[toLocation.getNumberLocation(), toLocation.getLetterLocation()] = new ChessPiece(getIsWhiteTurn());
                else
                    playBoard[toLocation.getNumberLocation(), toLocation.getLetterLocation()].setIsWhite(getIsWhiteTurn());
            }
            result = isKingThreatened(playBoard, lastMoveEarlyLocation, lastMoveFinalLocation);
            {
                playBoard[location.getNumberLocation(), location.getLetterLocation()] = movingPieceHolder;
                if (playBoard[toLocation.getNumberLocation(), toLocation.getLetterLocation()].getName() == 'T')
                    playBoard[toLocation.getNumberLocation(), toLocation.getLetterLocation()] = null;
                else
                    playBoard[toLocation.getNumberLocation(), toLocation.getLetterLocation()].setIsWhite(!(getIsWhiteTurn()));
            }
            return result;
        }
        public bool isKingThreatened(ChessPiece[,] playBoard, Location lastMoveEarlyLocation, Location lastMoveFinalLocation)
        {
            bool result;
            ChessPiece KingObgectHolder;
            Location KingLocation = thisTurnColorKingLocation();
            KingObgectHolder = playBoard[KingLocation.getNumberLocation(), KingLocation.getLetterLocation()];
            playBoard[KingLocation.getNumberLocation(), KingLocation.getLetterLocation()] = null;
            result = isThreatenedPlace(KingLocation, playBoard, lastMoveEarlyLocation, lastMoveFinalLocation);
            playBoard[KingLocation.getNumberLocation(), KingLocation.getLetterLocation()] = KingObgectHolder;

            return result;
        }
        public bool isThreatenedPlace(Location placeLocation, ChessPiece[,] playBoard, Location lastMoveEarlyLocation, Location lastMoveFinalLocation)
        {
            for (int i = 1; i < playBoard.GetLength(0); i++)
            {
                for (int j = 1; j < playBoard.GetLength(1); j++)
                {
                    if ((playBoard[i, j] != null) && (playBoard[i, j].getIsWhite() != getIsWhiteTurn()))
                    {
                        Location potentialTrheatingPieceLocation = new Location(i, j);
                        ChessPiece chessPieceHolder = playBoard[placeLocation.getNumberLocation(), placeLocation.getLetterLocation()];
                        if (playBoard[placeLocation.getNumberLocation(), placeLocation.getLetterLocation()] == null)
                        {
                            playBoard[placeLocation.getNumberLocation(), placeLocation.getLetterLocation()] = new ChessPiece(getIsWhiteTurn());
                        }
                        else
                        {
                            playBoard[placeLocation.getNumberLocation(), placeLocation.getLetterLocation()].setIsWhite(getIsWhiteTurn());
                        }
                        changeTurn();
                        if (isMoveLegal(potentialTrheatingPieceLocation, placeLocation, playBoard, lastMoveEarlyLocation, lastMoveFinalLocation, false) == true)
                        {//if threatend
                            return true;
                        }
                        playBoard[placeLocation.getNumberLocation(), placeLocation.getLetterLocation()] = chessPieceHolder;
                        changeTurn();
                    }
                }
            }

            return false;
        }
        public Location thisTurnColorKingLocation()
        {
            switch (getIsWhiteTurn())
            {
                case true:
                    {
                        return WhiteKingLocation;
                    }
                case false:
                    {
                        return BlackKingLocation;
                    }
            }
        }
        #endregion threat related funcs
        public void afterTurnCheksAndEffects(Location location, Location toLocation, ChessPiece[,] playBoard, Location lastMoveEarlyLocation, Location lastMoveFinalLocation)
        {
            if (playBoard[toLocation.getNumberLocation(), toLocation.getLetterLocation()] is Pawn)
            {
                (playBoard[toLocation.getNumberLocation(), toLocation.getLetterLocation()] as Pawn).isPromotionOption(toLocation, playBoard);
                printBoard(playBoard);
            }
            remmemberLastMove(location, toLocation);
            kingLocationTraceUpdate(toLocation, playBoard);
            changeTurn();
            if (isKingThreatened(playBoard, lastMoveEarlyLocation, lastMoveFinalLocation) == true)
                Console.WriteLine("You are under check!");
            if (isChekmate(playBoard, lastMoveEarlyLocation, lastMoveFinalLocation) == true)
            {
                Console.WriteLine(" checkmate!");
                endGame = true;
            }
            if (isDraw(boardString, playBoard, lastMoveEarlyLocation, lastMoveFinalLocation) == true)
            {
                Console.WriteLine(" its draw!");
                endGame = true;
            }
        }
        public void kingLocationTraceUpdate(Location toLocation, ChessPiece[,] playBoard)
        {
            if (playBoard[toLocation.getNumberLocation(), toLocation.getLetterLocation()] is King)
            {
                switch (playBoard[toLocation.getNumberLocation(), toLocation.getLetterLocation()].getIsWhite())
                {
                    case true:
                        {
                            WhiteKingLocation = toLocation;
                            break;
                        }
                    case false:
                        {
                            BlackKingLocation = toLocation;
                            break;
                        }
                }
            }
        }
        public bool changeTurn()
        { return isWhiteTurn = !isWhiteTurn; }
        public void remmemberLastMove(Location location, Location toLocation)
        {
            lastMoveEarlyLocation.setNumberLocation(location.getNumberLocation());
            lastMoveEarlyLocation.setLetterLocation(location.getLetterLocation());
            lastMoveFinalLocation.setNumberLocation(toLocation.getNumberLocation());
            lastMoveFinalLocation.setLetterLocation(toLocation.getLetterLocation());
        }
        #region input validation related funcs
        public bool inputChek(string input)
        {

            if ((input.Length == 4) && (isValidLetter(input, 0))
                && (isValidLetter(input, 2)) && isValidNumber(input, 1) && isValidNumber(input, 3))
            {
                return true;
            }
            else
                if (input != "ENOUGH")
                Console.WriteLine("invalid input");
            return false;
        }
        public bool isValidLetter(string input, int index)
        {
            switch (input[index])
            {
                case 'A':
                case 'a':
                case 'B':
                case 'b':
                case 'C':
                case 'c':
                case 'D':
                case 'd':
                case 'E':
                case 'e':
                case 'F':
                case 'f':
                case 'G':
                case 'g':
                case 'H':
                case 'h':
                    {
                        return true;
                    }
                default:
                    return false;
            }
        }
        public bool isValidNumber(string input, int index)
        {
            switch (input[index])
            {
                case '1':
                case '2':
                case '3':
                case '4':
                case '5':
                case '6':
                case '7':
                case '8':
                    return true;
                default:
                    return false;

            }
        }
        public int letterToNumber(string input, int index)
        {
            switch (input[index])
            {
                case 'A':
                case 'a':
                    return 1;
                case 'B':
                case 'b':
                    return 2;
                case 'C':
                case 'c':
                    return 3;
                case 'D':
                case 'd':
                    return 4;
                case 'E':
                case 'e':
                    return 5;
                case 'F':
                case 'f':
                    return 6;
                case 'G':
                case 'g':
                    return 7;
                case 'H':
                case 'h':
                    return 8;
                default:
                    return -1;
            }
        }
        public int numberToCurrectNumber(string input, int index)
        {
            switch (input[index])
            {
                case '1':
                    return 8;
                case '2':
                    return 7;
                case '3':
                    return 6;
                case '4':
                    return 5;
                case '5':
                    return 4;
                case '6':
                    return 3;
                case '7':
                    return 2;
                case '8':
                    return 1;
                default:
                    return -1;

            }
        }
        #endregion input validation related funcs
        public bool getIsWhiteTurn()
        {
            return isWhiteTurn;
        }
        public void endFirstTurn(Location toLocation, ChessPiece[,] playBoard)
        {
            switch (playBoard[toLocation.getNumberLocation(), toLocation.getLetterLocation()].getName())
            {
                case 'K':
                    (playBoard[toLocation.getNumberLocation(), toLocation.getLetterLocation()] as King).setIsFirstTurn(false);
                    break;
                case 'R':
                    (playBoard[toLocation.getNumberLocation(), toLocation.getLetterLocation()] as Rook).setIsFirstTurn(false);
                    break;
                case 'P':
                    (playBoard[toLocation.getNumberLocation(), toLocation.getLetterLocation()] as Pawn).setIsFirstTurn(false);
                    break;
            }
        }
    }
    class Location
    {
        int NumberLocation;
        int LetterLocation;
        public Location(int numberLocation, int letterLocation)
        {
            NumberLocation = numberLocation;
            LetterLocation = letterLocation;
        }
        public int getNumberLocation()
        { return NumberLocation; }
        public int getLetterLocation()
        { return LetterLocation; }
        public bool setNumberLocation(int NumberLocation)
        { this.NumberLocation = NumberLocation; return true; }
        public bool setLetterLocation(int LetterLocation)
        { this.LetterLocation = LetterLocation; return true; }
    }
    class ChessPiece
    {
        ChessGame MathmaticFuncsRunner = null;
        bool isWhite;
        char name = 'T';
        public ChessPiece(bool isWhite)
        {
            setIsWhite(isWhite);
        }
        public virtual bool ChekMovmentValid(Location location, Location toLocation, ChessPiece[,] playBoard, Location lastMoveEarlyLocation, Location lastMoveFinalLocation)
        {
            Console.WriteLine("this is not valid piece");
            return false;
        }
        public override string ToString()
        {
            return (isWhite ? "W" : "B") + this.name + " ";
        }
        #region chess piece realated
        public bool getIsWhite()
        { return isWhite; }
        public bool setIsWhite(bool isWhite)
        {
            this.isWhite = isWhite;
            return true;
        }
        public char getName()
        { return name; }
        public bool setName(char name)
        { this.name = name; return true; }
        #endregion chess piece realated
        public ChessGame getMathmaticFuncsRunner()
        { return MathmaticFuncsRunner; }
    }
    class King : ChessPiece
    {
        bool isFirstTurn = true;

        public King(bool isWhite, bool isFirstTurn) : base(isWhite)
        { setIsFirstTurn(isFirstTurn); setName('K'); }
        public bool getIsFirstTurn()
        { return isFirstTurn; }
        public bool setIsFirstTurn(bool isFirstTurn)
        { this.isFirstTurn = isFirstTurn; return true; }
        public override bool ChekMovmentValid(Location location, Location toLocation, ChessPiece[,] playBoard, Location lastMoveEarlyLocation, Location lastMoveFinalLocation)
        {
            if ((getMathmaticFuncsRunner().lengthOfVerticalVector() <= 1) &&  //king movment
                ((getMathmaticFuncsRunner().lengthOfHorizontalVector() <= 1) || ((isFirstTurn) && (getMathmaticFuncsRunner().lengthOfHorizontalVector() == 2) && (getMathmaticFuncsRunner().lengthOfVerticalVector() == 0))))
            {
                return true;
            }
            return false;
        }
    }
    class Queen : ChessPiece
    {

        public Queen(bool isWhite) : base(isWhite)
        { setName('Q'); }
        public override bool ChekMovmentValid(Location location, Location toLocation, ChessPiece[,] playBoard, Location lastMoveEarlyLocation, Location lastMoveFinalLocation)
        {
            if ((location.getLetterLocation() == toLocation.getLetterLocation()) || (location.getNumberLocation() == toLocation.getNumberLocation()) || ((getMathmaticFuncsRunner().lengthOfVerticalVector()) == (getMathmaticFuncsRunner().lengthOfHorizontalVector()))) //queen movment
            {
                return true;
            }
            return false;
        }
    }
    class Rook : ChessPiece
    {
        bool isFirstTurn = true;
        public Rook(bool isWhite, bool isFirstTurn) : base(isWhite)
        { setIsFirstTurn(isFirstTurn); setName('R'); }
        public bool getIsFirstTurn()
        { return isFirstTurn; }
        public bool setIsFirstTurn(bool isFirstTurn)
        { this.isFirstTurn = isFirstTurn; return true; }
        public override bool ChekMovmentValid(Location location, Location toLocation, ChessPiece[,] playBoard, Location lastMoveEarlyLocation, Location lastMoveFinalLocation) //rook movment
        {
            if (((location.getLetterLocation() == toLocation.getLetterLocation()) || (location.getNumberLocation() == toLocation.getNumberLocation())))
            {
                return true;
            }
            return false;
        }
    }
    class Bishop : ChessPiece
    {
        public Bishop(bool isWhite) : base(isWhite)
        { setName('B'); }
        public override bool ChekMovmentValid(Location location, Location toLocation, ChessPiece[,] playBoard, Location lastMoveEarlyLocation, Location lastMoveFinalLocation)
        {

            if (getMathmaticFuncsRunner().lengthOfVerticalVector() == getMathmaticFuncsRunner().lengthOfHorizontalVector()) //bishop movment
            {
                return true;
            }
            return false;
        }
    }
    class Knight : ChessPiece
    {
        public Knight(bool isWhite) : base(isWhite)
        { setName('N'); }
        public override bool ChekMovmentValid(Location location, Location toLocation, ChessPiece[,] playBoard, Location lastMoveEarlyLocation, Location lastMoveFinalLocation)  //knight movment
        {
            if ((getMathmaticFuncsRunner().lengthOfVerticalVector() == 2 && getMathmaticFuncsRunner().lengthOfHorizontalVector() == 1) ||
                (getMathmaticFuncsRunner().lengthOfVerticalVector() == 1 && getMathmaticFuncsRunner().lengthOfHorizontalVector() == 2))
            {
                return true;
            }
            return false;
        }
    }
    class Pawn : ChessPiece
    {
        bool isFirstTurn = true;
        public Pawn(bool isWhite) : base(isWhite)
        { setName('P'); }
        public override bool ChekMovmentValid(Location location, Location toLocation, ChessPiece[,] playBoard, Location lastMoveEarlyLocation, Location lastMoveFinalLocation) //pawn movment
        {
            if (playBoard[toLocation.getNumberLocation(), toLocation.getLetterLocation()] == null) //normal mooveing + en passant
            {
                if ((getMathmaticFuncsRunner().lengthOfVerticalVector() <= (getIsFirstTurn() ? 2 : 1)) && isValidVerticalDiraction() && getMathmaticFuncsRunner().lengthOfHorizontalVector() == 0)
                {
                    return true;
                }
                else if ((location.getNumberLocation() == (getIsWhite() ? 4 : 5)) && (toLocation.getNumberLocation() == (getIsWhite() ? 3 : 6)) && ((getMathmaticFuncsRunner().lengthOfHorizontalVector()) == 1) && //eater side
                        (playBoard[lastMoveFinalLocation.getNumberLocation(), lastMoveFinalLocation.getLetterLocation()] is Pawn) && //eaten side
                        (playBoard[lastMoveFinalLocation.getNumberLocation(), lastMoveFinalLocation.getLetterLocation()] == playBoard[location.getNumberLocation(), toLocation.getLetterLocation()]) &&
                        (getMathmaticFuncsRunner().lastMoveLengthOfVerticalVector() == 2))
                {
                    return true;
                }
            }
            else if (playBoard[toLocation.getNumberLocation(), toLocation.getLetterLocation()] != null) // normal eating
            {
                if (isValidVerticalDiraction() && getMathmaticFuncsRunner().lengthOfVerticalVector() == 1 && getMathmaticFuncsRunner().lengthOfHorizontalVector() == 1)
                {
                    return true;
                }
            }
            return false;
        }
        #region promotion related funcs
        public void isPromotionOption(Location toLocation, ChessPiece[,] playBoard)
        {
            if ((this is Pawn) && (toLocation.getNumberLocation() == 8 || toLocation.getNumberLocation() == 1))
            {
                promotion(toLocation, playBoard);
            }
        }
        public void promotion(Location toLocation, ChessPiece[,] playBoard)
        {
            bool PromotedPawnColor = this.getIsWhite();
            Console.WriteLine("your pawn crossed the board" +
                "\npress Q to promote it to a Queen" +
                "\npress R to promote it to a Rook" +
                "\npress B to promote it to a Bishop" +
                "\npress N to promote it to a knight");
            string input = Console.ReadLine();
            input = input.Trim();
            input = input.ToUpper();
            if (inputChek(input))
            {
                switch (input[0])
                {
                    case 'Q':
                        playBoard[toLocation.getNumberLocation(), toLocation.getLetterLocation()] = new Queen(PromotedPawnColor);
                        break;
                    case 'R':
                        playBoard[toLocation.getNumberLocation(), toLocation.getLetterLocation()] = new Rook(PromotedPawnColor, false);
                        break;
                    case 'B':
                        playBoard[toLocation.getNumberLocation(), toLocation.getLetterLocation()] = new Bishop(PromotedPawnColor);
                        break;
                    case 'N':
                        playBoard[toLocation.getNumberLocation(), toLocation.getLetterLocation()] = new Knight(PromotedPawnColor);
                        break;
                }
            }
            else
            { this.promotion(toLocation, playBoard); }

        }
        public bool inputChek(string input)
        {
            if ((input.Length == 1) && (isValidLetter(input, 0)))
            {
                return true;
            }
            else
                Console.WriteLine("invalid input");
            return false;
        }
        public bool isValidLetter(string input, int index)
        {
            switch (input[index])
            {
                case 'Q':
                case 'R':
                case 'B':
                case 'N':
                    {
                        return true;
                    }
                default:
                    return false;
            }
        }
        #endregion promotion related funcs
        public bool getIsFirstTurn()
        { return isFirstTurn; }
        public bool setIsFirstTurn(bool isFirstTurn)
        { this.isFirstTurn = isFirstTurn; return true; }
        public bool isValidVerticalDiraction()
        {
            return getMathmaticFuncsRunner().isVerticalVectorPointingDown() == (getIsWhite() ? false : true);
        }
    }
}