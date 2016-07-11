using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Weiqi
{
    public static class MoveUtils
    {

        public static bool Move(ref BoardStatus status, byte row, byte col)
        {
            if (status.Board[row, col] != WColor.EMPTY)
            {
                return false;
            }
            status.Board[row, col] = status.Turn;

            if (CalculateCaptureStones(row, col, status.Turn, ref status) == false)
            {
                status.Board[row, col] = WColor.EMPTY;
                return false;
            }
            else
            {
                status.Turn = ColorUtils.Flip(status.Turn);
                return true;
            }
        }

        public static bool CheckMove(BoardStatus status, byte row, byte col)
        {
            BoardStatus savedStatus = new BoardStatus(status);
            bool ret = Move(ref status, row, col);
            status = savedStatus;
            return ret;
        }

        public static bool CheckMove(BoardBase status, WColor turn, byte row, byte col)
        {
            BoardStatus toCheck = new BoardStatus(status.Size);
            toCheck.Board = new BoardBase(status);
            toCheck.Turn = turn;
            bool ret = Move(ref toCheck, row, col);
            return ret;
        }

        private static bool CalculateCaptureStones(byte row, byte col, WColor color, ref BoardStatus newStatus)
        {
            List<Tuple<byte, byte>> capturedStones;
            bool captured = false;

            var flipColor = ColorUtils.Flip(color);
            if ((capturedStones = Capture(newStatus, (byte)(row + 1), col, flipColor)).Count > 0)
            {
                RemoveStones(capturedStones, newStatus);
                captured = true;
            }
            if ((capturedStones = Capture(newStatus, (byte)(row - 1), col, flipColor)).Count > 0)
            {
                RemoveStones(capturedStones, newStatus);
                captured = true;
            }
            if ((capturedStones = Capture(newStatus, row, (byte)(col + 1), flipColor)).Count > 0)
            {
                RemoveStones(capturedStones, newStatus);
                captured = true;
            }
            if ((capturedStones = Capture(newStatus, row, (byte)(col - 1), flipColor)).Count > 0)
            {
                RemoveStones(capturedStones, newStatus);
                captured = true;
            }

            if ( (!captured) && ((capturedStones = Capture(newStatus, row, col, color)).Count > 0) )
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        public static void RemoveStones(List<Tuple<byte, byte>> capturedStones, BoardStatus newStatus)
        {
            foreach (var stonePosition in capturedStones)
            {
                newStatus.Board[stonePosition.Item1, stonePosition.Item2] = WColor.EMPTY;
                newStatus.Board.ClearMark(stonePosition.Item1, stonePosition.Item2);
            }
        }

        private static List<Tuple<byte, byte>> Capture(BoardStatus newStatus, byte row, byte col, WColor color)
        {
            var capturedStones = new List<Tuple<byte, byte>>();
            BoardBase bbTemp = new BoardBase(newStatus.Board);
            if ( !captureKernel(bbTemp, capturedStones, row, col, color) )
            {
                capturedStones.Clear();
            }
            return capturedStones;
        }

        private static bool captureKernel(BoardBase board, List<Tuple<byte,byte>> captureList, byte row, byte col, WColor color)
        {
            WColor flipColor = ColorUtils.Flip(color);
            if (!BoardUtil.CheckBounds(board.Size, row, col))
            {
                return true;
            }
            else if (board[row, col] == flipColor)
            {
                return true;
            }
            else if (board.GetMark(row, col) == Mark.MARK1)
            {
                return true;
            }
            else if (board[row, col] == WColor.EMPTY)
            {
                return false;
            }
            else
            {
                captureList.Add(new Tuple<byte, byte>(row, col));
                board.Mark(row, col, Mark.MARK1);
                return (
                    captureKernel(board, captureList, (byte)(row + 1), col, color) 
                    && captureKernel(board, captureList, (byte)(row - 1), col, color)
                    && captureKernel(board, captureList, row, (byte)(col + 1), color)
                    && captureKernel(board, captureList, row, (byte)(col - 1), color)
                    );
            }
        }
    }
}
