using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.IO;

namespace Weiqi
{
    public enum WColor 
    {
        EMPTY = 0,
        BLACK = 1,
        WHITE = 2,
        MARK1 = 1,
    };

    public enum Mark
    {
        MARK1 = 1,
        MARK2 = 2,
        MARK3 = 4,
        MARK4 = 8,
    };

    public static class AlgoConstants
    {
        public const int StoneRangeMax = 4;
    };
    
    public static class ColorUtils
    {
        public static WColor Flip(WColor c)
        {
            if (c == WColor.BLACK)
            {
                return WColor.WHITE;
            }
            else if (c == WColor.WHITE)
            {
                return WColor.BLACK;
            }
            return WColor.EMPTY;
        }
    }

    public static class BoardUtil
    {
        public static void PrintToConsole(PartialBoard board)
        {
            PrintToConsole(board.boardStatus, board.mask);
        }

        public static void PrintToConsole(BoardStatus boardStatus, Mask mask = null)
        {
            byte size = boardStatus.Board.Size;

            Console.WriteLine();
            ConsoleColor saveColor = Console.ForegroundColor;

            Console.Write("   ");
            Console.ForegroundColor = ConsoleColor.Gray;
            for (byte i = 0; i < size; i++)
            {
                Console.Write(' ');
                if (i<8)
                {
                    Console.Write((char)(i + 'A'));
                }
                else
                {
                    Console.Write((char)(i + 'B'));
                }
            }
            Console.WriteLine();
            for (byte i = 0; i < size; i++)
            {
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.Write(" {0,2}", (int)(19 - i));
                for (byte j = 0; j < size; j++)
                {
                    WColor stoneColor = boardStatus.Board.Get(j, i);
                    Mark markColor = boardStatus.Board.GetMark(j, i);
                    ConsoleColor fc = ConsoleColor.Gray;
                    string mark = "";
                    if (stoneColor == WColor.EMPTY)
                    {
                        fc = ConsoleColor.Gray;
                        mark = " .";
                    }
                    else if (stoneColor == WColor.WHITE)
                    {
                        fc = ConsoleColor.Green;
                        mark = " X";
                    }
                    else if (stoneColor == WColor.BLACK)
                    {
                        fc = ConsoleColor.Yellow;
                        mark = " O";
                    }

                    if (markColor == Mark.MARK1)
                    {
                        fc = ConsoleColor.Red;
                    }
                    if (mask.CheckMaskClose(j, i))
                    {
                        fc = ConsoleColor.Gray;
                        mark = " #";
                    }
                    Console.ForegroundColor = fc;
                    Console.Write(mark);
                }
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.Write(" {0}", (int)(19 - i));
                Console.WriteLine();
            }
            Console.Write("   ");
            Console.ForegroundColor = ConsoleColor.Gray;
            for (byte i = 0; i < size; i++)
            {
                Console.Write(' ');
                if (i < 8)
                {
                    Console.Write((char)(i + 'A'));
                }
                else
                {
                    Console.Write((char)(i + 'B'));
                }
            }
            Console.WriteLine();
            Console.WriteLine();
            Console.ForegroundColor = saveColor;
        }

        public static void ScoreEstToConsole(BoardStatus boardStatus, ScoreEst estimator)
        {
            byte size = boardStatus.Board.Size;
            Console.WriteLine();
            ConsoleColor saveColor = Console.ForegroundColor;
            Console.Write("   ");
            Console.ForegroundColor = ConsoleColor.Gray;
            for (byte i = 0; i < size; i++)
            {
                Console.Write(' ');
                if (i < 8)
                {
                    Console.Write((char)(i + 'A'));
                }
                else
                {
                    Console.Write((char)(i + 'B'));
                }
            }
            Console.Write("            ");
            for (byte i = 0; i < size; i++)
            {
                Console.Write(' ');
                if (i < 8)
                {
                    Console.Write((char)(i + 'A'));
                }
                else
                {
                    Console.Write((char)(i + 'B'));
                }
            }
            Console.WriteLine();
            for (byte i = 0; i < size; i++)
            {
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.Write(" {0,2}", (int)(19 - i));
                for (byte j = 0; j < size; j++)
                {
                    WColor stoneColor = estimator.GetOwnership(j, i);
                    ConsoleColor fc = ConsoleColor.Gray;
                    string mark = "";
                    if (stoneColor == WColor.EMPTY)
                    {
                        fc = ConsoleColor.Gray;
                        mark = " .";
                    }
                    else if (stoneColor == WColor.WHITE)
                    {
                        fc = ConsoleColor.Green;
                        mark = " #";
                    }
                    else if (stoneColor == WColor.BLACK)
                    {
                        fc = ConsoleColor.Yellow;
                        mark = " #";
                    }

                    Console.ForegroundColor = fc;
                    Console.Write(mark);
                }
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.Write(" {0,2}", (int)(19 - i));

                Console.Write("      ");

                Console.Write(" {0,2}", (int)(19 - i));
                for (byte j = 0; j < size; j++)
                {
                    Mark markColor = boardStatus.Board.GetMark(j, i);
                    ConsoleColor fc = ConsoleColor.Gray;
                    int score = (int)estimator.GetScore(j, i, 9);
                    string mark = " " + Math.Abs(score).ToString();
                    if (score == 0)
                    {
                        fc = ConsoleColor.Gray;
                    }
                    else if (score < 0)
                    {
                        fc = ConsoleColor.Green;
                    }
                    else if (score > 0)
                    {
                        fc = ConsoleColor.Yellow;
                    }

                    if (markColor == Mark.MARK1)
                    {
                        fc = ConsoleColor.Red;
                    }
                    Console.ForegroundColor = fc;
                    Console.Write(mark);
                }
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.Write(" {0,2}", (int)(19 - i));
                Console.WriteLine();
            }
            Console.Write("   ");
            Console.ForegroundColor = ConsoleColor.Gray;
            for (byte i = 0; i < size; i++)
            {
                Console.Write(' ');
                if (i < 8)
                {
                    Console.Write((char)(i + 'A'));
                }
                else
                {
                    Console.Write((char)(i + 'B'));
                }
            }
            Console.Write("            ");
            for (byte i = 0; i < size; i++)
            {
                Console.Write(' ');
                if (i < 8)
                {
                    Console.Write((char)(i + 'A'));
                }
                else
                {
                    Console.Write((char)(i + 'B'));
                }
            }
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine();
            Console.ForegroundColor = saveColor;

        }

        public static bool CheckBounds(int Size, int row, int col)
        {
            return CheckBounds(Size, (byte)row, (byte)col);
        }

        public static bool CheckBounds(int Size, Tuple<int,int> stone)
        {
            return CheckBounds(Size, (byte)stone.Item1, (byte)stone.Item2);
        }

        public static bool CheckBounds(int Size, byte row, byte col)
        {
            if ((row < 0) || (row >= Size) || (col < 0) || (col >= Size))
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        public static Tuple<byte, byte> GetCoord(string strcoord)
        {
            char srow = strcoord[0].ToString().ToUpper()[0];
            byte row = (byte)((srow - 'A'));
            if (row > 8)
                row--;
            byte col = (byte)(19 - Int32.Parse(strcoord.Substring(1)));
            return new Tuple<byte, byte>(row, col);
        }

        public static string GetCoord(int row, int col)
        {
            StringBuilder buf = new StringBuilder();
            if (row <= 7)
            {
                buf.Append((char)(row + 'A'));
            }
            else
            {
                buf.Append((char)(row + 'B'));
            }
            buf.Append((int)(19 - col));
            return buf.ToString();
        }

        public static IEnumerable<Tuple<int, int>> GetNeighbors(byte row, byte col, byte size, int degree)
        {
            for (int i = 0; i <= degree; i++)
            {
                for (int j = 0; j < degree - i; j++)
                {
                    if ( (i==0) && (j==0) )
                    {
                        continue;
                    }
                    if (CheckBounds(size, row + i, col + j))
                    {
                        yield return new Tuple<int, int>(row + i, col + j);
                    }
                    if (CheckBounds(size, row - i, col + j))
                    {
                        yield return new Tuple<int, int>(row - i, col + j);
                    }
                    if (CheckBounds(size, row - i, col - j))
                    {
                        yield return new Tuple<int, int>(row - i, col - j);
                    }
                    if (CheckBounds(size, row + i, col - j))
                    {
                        yield return new Tuple<int, int>(row + i, col - j);
                    }
                }
            }
        }

    }
}
