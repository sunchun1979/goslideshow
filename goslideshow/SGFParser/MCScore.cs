using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Weiqi
{

    public class MCScore
    {
        private BoardStatus _baseStatus;
        private Mask _mask;

        private int _totalN = 0;
        private int _maxN = 500;
        private double _value = 0;
        private int _coCount = 0;

        private List<Tuple<int, int>> _candidateList;

        Random _random;

        public MCScore(BoardStatus baseStatus, Mask mask = null)
        {
            _baseStatus = new BoardStatus(baseStatus);
            _mask = mask;
            _random = new Random();
        }

        public double GetMCValue()
        {
            return _value;
        }

        public void GenerateMCScore(WColor firstMove)
        {
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
            _baseStatus.Turn = firstMove;
            _candidateList = _mask.GetOpenMaskList();

            MCPlayOut3();

            stopWatch.Stop();
            TimeSpan ts = stopWatch.Elapsed;
            string elapsedTime = String.Format("{0}.{1}", ts.Seconds, ts.Milliseconds);
            //Console.WriteLine("White = " + _white + " Black = " + _black + " Empty = " + _empty);
            Console.WriteLine("Value = " + _value);
            Console.WriteLine("Time: " + elapsedTime);
            Console.WriteLine("End");
        }

        private void MCPlayOut3()
        {
            _totalN = 0;
            _coCount = 0;
            while (_totalN < _maxN)
            {
                var status = new BoardStatus(_baseStatus);
                bool finished = CheckFinished(status, _mask);

                List<int> candidateIndex = new List<int>();

                int steps = 0;

                while (!finished)
                {
                    if (steps > 100)
                    {
                        _coCount++;
                        break;
                    }
                    candidateIndex.Clear();
                    for (int i = 0; i < _candidateList.Count; i++)
                    {
                        if (status.Board[_candidateList[i]] == WColor.EMPTY)
                        {
                            candidateIndex.Add(i);
                        }
                    }
                    int next = _random.Next(candidateIndex.Count + 1);
                    if (next > 0)
                    {
                        var coord = _candidateList[candidateIndex[next - 1]];
                        MoveUtils.Move(ref status, (byte)coord.Item1, (byte)coord.Item2);
                    }
                    else
                    {
                        status.FlipTurn();
                    }
                    finished = CheckFinished(status, _mask);
                    ++steps;
                }
                ScoreEst est = new ScoreEst(status.Board, _mask, 2);
                //Console.WriteLine("Finished");
                //BoardUtil.PrintToConsole(status, _mask);
                //Console.WriteLine(est.RigidCount() + " " + steps);
                //Console.ReadLine();
                Console.WriteLine(_totalN);
                ++_totalN;
                _value = _value * (_totalN - 1) / _totalN + (double)est.RigidCount() / _totalN;
            }
        }

        public bool CheckFinished(BoardStatus board, Mask mask)
        {
            foreach (var point in board.Board.LoopPoints(mask))
            {
                int i = point.Item1;
                int j = point.Item2;
                if (board.Board[i, j] == WColor.EMPTY)
                {
                    WColor color = board.Turn;
                    if ( (MoveUtils.CheckMove(board.Board, color, (byte)i, (byte)j)) 
                        && (MoveUtils.CheckMove(board.Board, ColorUtils.Flip(color), (byte)i, (byte)j)) )
                    {
                        return false;
                    }
                }
            }
            return true;
        }


    }
}
