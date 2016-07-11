using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Weiqi
{
    public class ScoreEst
    {
        protected BoardBase _boardBase;
        protected BoardBase _boardAux;
        protected Mask _mask;
        protected BoardGeneric<WColor> _boardOwnership;
        protected BoardGeneric<double> _boardScore;
        protected BoardGeneric<double> _boardUncertainty;

        private double _minScore = 0;
        private double _maxScore = 0;
        private double _scoreScaleFactor;

        private double _minUncertainty = 0;
        private double _maxUncertainty = 0;
        private double _uncertaintyScaleFactor;
        private double _averageUncertainty = 0;
        private double _averageVariance = 0;
        private bool _groupInfluence;
        private int _influenceRange = AlgoConstants.StoneRangeMax;

        private Grouping _group;

        private ScoreEst() {}

        public ScoreEst(BoardBase boardStatus, Mask mask, uint complexity)
        {
            _boardBase = boardStatus;
            _boardScore = new BoardGeneric<double>(_boardBase.Size);
            _boardUncertainty = new BoardGeneric<double>(_boardBase.Size);
            _boardOwnership = new BoardGeneric<WColor>(_boardBase.Size);

            _mask = mask;
            switch (complexity)
            {
                case 2:
                    {
                        _influenceRange = 2;
                        _groupInfluence = false;
                        break;
                    }
                case 3:
                    {
                        _influenceRange = 3;
                        _groupInfluence = false;
                        break;
                    }
                default:
                    {
                        _influenceRange = AlgoConstants.StoneRangeMax;
                        _groupInfluence = true;
                        break;
                    }
            }
            GenerateAux();
            GenerateScore();
        }

        public int RigidCount()
        {
            int black = 0;
            int white = 0;
            foreach (var point in _boardBase.LoopPoints(_mask))
            {
                int i = point.Item1;
                int j = point.Item2;
                if (_boardOwnership[i, j] == WColor.BLACK)
                {
                    black++;
                }
                else if (_boardOwnership[i, j] == WColor.WHITE)
                {
                    white++;
                }
                else if (_boardAux[i, j] == WColor.BLACK)
                {
                    black++;
                }
                else if (_boardAux[i, j] == WColor.WHITE)
                {
                    white++;
                }
            }
            return black - white;
        }

        public WColor GetOwnership(int row, int col)
        {
            return _boardOwnership[row, col];
        }

        public double GetScore(int row, int col, double scale = 1.0)
        {
            if (_boardAux[row, col] != WColor.EMPTY)
            {
                return 0;
            }
            double dScore = ((_boardScore[row, col] - _minScore) * _scoreScaleFactor * 2 * scale) - scale;
            return dScore;
        }

        public double GetUncertainty(int row, int col, double scale = 1.0)
        {
            // scale from 0 to +1 ( -scale to +scale )
            if (_boardAux[row, col] != WColor.EMPTY)
            {
                return 0;
            }
            return _boardUncertainty[row, col] * scale;
        }

        public double GetUncertaintyThreashold()
        {
            return _averageUncertainty - _averageVariance * 0.2;
        }

        private void GenerateAux()
        {
            if (_mask == null)
            {
                _boardAux = new BoardBase(_boardBase);
            }
            else
            {
                _boardAux = new BoardBase(_boardBase.Size);
                foreach (var stone in _mask.GetOpenMask())
                {
                    _boardAux[stone] = _boardBase[stone];
                }
            }
        }

        protected void GenerateScore()
        {
            CheckOneLibertyStones();
            GenerateScoreInfluence();
            if (_groupInfluence)
            {
                GenerateScoreGroups();
            }
            GenerateUncertainty();
        }

        private void CheckOneLibertyStones()
        {
            _group = new Grouping(_boardAux);
            for (int i = 0; i < _group.GetNumOfGroups(); i++)
            {
                var groupLiberty = _group.GetGroupLiberty(i);
                if (groupLiberty.Count == 1)
                {
                    var groupColor = _group.GetGroupColor(i);
                    byte row = (byte)groupLiberty[0].Item1;
                    byte col = (byte)groupLiberty[0].Item2;
                    if (!MoveUtils.CheckMove(_boardAux, groupColor, row, col))
                    {
                        foreach (var stone in _group.GetGroup(i))
                        {
                            _boardOwnership[stone] = ColorUtils.Flip(groupColor);
                        }
                    }
                }
            }
        }

        private void GenerateUncertainty()
        {
            foreach (var item in _boardBase.LoopPoints(null))
            {
                _boardUncertainty[item] = 0;
            }
            if (_mask != null)
            {
                CalculateUncertaintyWithMask();
            }
            else
            {
                // Throw Exception
                //for (int i = 0; i < _boardScore.Size; i++)
                //{
                //    for (int j = 0; j < _boardScore.Size; j++)
                //    {
                //        if (_boardScore[i, j] != 0)
                //        {
                //            _boardUncertainty[i, j] = -Math.Log(Math.Abs(_boardScore[i, j]));
                //        }
                //    }
                //}
            }
            _minUncertainty = double.MaxValue;
            _maxUncertainty = double.MinValue;
            foreach (var item in _boardBase.LoopPoints(_mask))
            {
                if (_boardBase[item] != WColor.EMPTY)
                {
                    continue;
                }
                if (_boardUncertainty[item] < _minUncertainty)
                {
                    _minUncertainty = _boardUncertainty[item];
                }
                if (_boardUncertainty[item] > _maxUncertainty)
                {
                    _maxUncertainty = _boardUncertainty[item];
                }
            }
            _uncertaintyScaleFactor = 1.0 / (_maxUncertainty - _minUncertainty);

            double tot = 0;
            double tot2 = 0;
            int num = 0;
            foreach (var item in _boardBase.LoopPoints(_mask))
            {
                if (_boardBase[item] != WColor.EMPTY)
                {
                    continue;
                }
                _boardUncertainty[item] = (_boardUncertainty[item] - _minUncertainty) * _uncertaintyScaleFactor;
                tot += _boardUncertainty[item];
                tot2 += _boardUncertainty[item] * _boardUncertainty[item];
                num++;
            }
            _averageUncertainty = tot / num;
            _averageVariance = Math.Sqrt(tot2 / num - _averageUncertainty * _averageUncertainty);
        }

        private void CalculateUncertaintyWithMask()
        {
            foreach (var openPosition in _boardBase.LoopPoints(_mask))
            {
                if (_boardBase[openPosition] != WColor.EMPTY)
                {
                    continue;
                }
                int x0 = openPosition.Item1;
                int y0 = openPosition.Item2;
                double score1 = 0;
                foreach (var borderPosition in _mask.Border)
                {
                    int x1 = borderPosition.Item1;
                    int y1 = borderPosition.Item2;
                    WColor mark = WColor.EMPTY;
                    foreach (var midPt in GetLinePoint(x0, y0, x1, y1))
                    {
                        if (_boardBase[midPt] != WColor.EMPTY)
                        {
                            if (mark == WColor.EMPTY)
                            {
                                score1 += 1;
                                mark = _boardBase[midPt];
                            }
                            else if (mark == _boardBase[midPt])
                            {
                                continue;
                            }
                            else
                            {
                                score1 += 2;
                                mark = _boardBase[midPt];
                            }
                        }
                    }
                }

                int black = 0;
                int white = 0;
                double score2 = 0;
                int NN = 2;
                foreach (var neighbor in BoardUtil.GetNeighbors((byte)x0, (byte)y0, _boardBase.Size, NN))
                {
                    if (_boardBase[neighbor] == WColor.BLACK)
                    {
                        black++;
                    }
                    else if (_boardBase[neighbor] == WColor.WHITE)
                    {
                        white++;
                    }
                }
                if ((black > 0) && (white > 0))
                {
                    score2 = (black + white) * 2;
                }
                else if (black > 0)
                {
                    score2 = black;
                }
                if (white > 0)
                {
                    score2 = white;
                
                }

                _boardUncertainty[openPosition] = score1 + score2 / ( (NN+1)*(NN+2) ) * _mask.Border.Count;
            }
        }

        private IEnumerable<Tuple<int, int>> GetLinePoint(int x0, int y0, int x1, int y1)
        {
            if (x1 - x0 == 0)
            {
                for (int y = Math.Min(y0, y1); y <= Math.Max(y0, y1); y++)
                {
                    yield return new Tuple<int, int>(x0, y);
                }
            }
            else if ( y1 - y0 == 0)
            {
                for (int x = Math.Min(x0, x1); x <= Math.Max(x0, x1); x++)
                {
                    yield return new Tuple<int, int>(x, y0);
                }
            }
            else
            {
                int dx = x1 - x0;
                int dy = y1 - y0;

                if (Math.Abs(dx) > Math.Abs(dy))
                {
                    double k = (double)dy / dx;
                    for (int x = Math.Min(x0, x1); x <= Math.Max(x0, x1); x++)
                    {
                        int y = (int)((x - x0) * k + y0);
                        yield return new Tuple<int, int>(x, y);
                    }
                }
                else
                {
                    double k = (double)dx / dy;
                    for (int y = Math.Min(y0, y1); y <= Math.Max(y0, y1); y++)
                    {
                        int x = (int)((y - y0) * k + x0);
                        yield return new Tuple<int, int>(x, y);
                    }
                }
            }
        }

        private void GenerateScoreGroups()
        {
            for (int i = 0; i < _group.GetNumOfGroups(); i++)
            {
                var groupLiberty = _group.GetGroupLiberty(i);
                bool ownership = false;
                if (groupLiberty.Count == 1)
                {
                    ownership = false;
                }

                WColor groupColor = _group.GetGroupColor(i);
                foreach (var stone in groupLiberty)
                {
                    if( (GetOwnership(stone.Item1, stone.Item2) != ColorUtils.Flip(groupColor)) 
                        && (_boardAux[stone] != groupColor) )
                    {
                        ownership = true;
                        break;
                    }
                }
                if (ownership == false)
                {
                    var group = _group.GetGroup(i);
                    if (groupColor == WColor.WHITE)
                    {
                        foreach (var stone in group)
                        {
                            _boardOwnership[stone] = WColor.BLACK;
                        }
                    }
                    else if (groupColor == WColor.BLACK)
                    {
                        foreach (var stone in group)
                        {
                            _boardOwnership[stone] = WColor.WHITE;
                        }
                    }
                }
            }
        }

        protected void GenerateScoreInfluence()
        {
            int size = _boardScore.Size;
            var auxBoard = new BoardBase((byte)(size + 2));
            for (int i0 = 0; i0 < size; i0++)
            {
                for (int j0 = 0; j0 < size; j0++)
                {
                    auxBoard[i0 + 1, j0 + 1] = _boardAux[i0, j0];
                }
            }
            for (int i0 = 0; i0 < size; i0++)
            {
                auxBoard[0, i0 + 1] = EdgeInfluenceScanIn(0, i0, 1, 0);
                auxBoard[size + 1, i0 + 1] = EdgeInfluenceScanIn(size - 1, i0, -1, 0);
                auxBoard[i0 + 1, 0] = EdgeInfluenceScanIn(i0, 0, 0, 1);
                auxBoard[i0 + 1, size + 1] = EdgeInfluenceScanIn(i0, size - 1, 0, -1);
            }
            auxBoard[0, 0] = EdgeInfluenceScanIn(0, 0, 1, 1);
            auxBoard[size + 1, size + 1] = EdgeInfluenceScanIn(size - 1, size - 1, -1, -1);
            auxBoard[0, size + 1] = EdgeInfluenceScanIn(0, size - 1, 1, -1);
            auxBoard[size + 1, 0] = EdgeInfluenceScanIn(size - 1, 0, -1, 1);

            for (int i0 = 0; i0 < size + 2; i0++)
            {
                for (int j0 = 0; j0 < size + 2; j0++)
                {
                    if (auxBoard[i0, j0] != WColor.EMPTY)
                    {
                        if (BoardUtil.CheckBounds(size, i0 - 1, j0 - 1))
                        {
                            if (_boardOwnership[i0 - 1, j0 - 1] == ColorUtils.Flip(auxBoard[i0, j0])) 
                            {
                                break;
                            }
                        }
                        for (int di = -_influenceRange; di < _influenceRange; di++)
                        {
                            for (int dj = -_influenceRange; dj < _influenceRange; dj++)
                            {
                                var score = CalcScore(di, dj, i0, j0, auxBoard);
                                if (BoardUtil.CheckBounds(size, score.Item1 - 1, score.Item2 - 1))
                                {
                                    _boardScore[score.Item1 - 1, score.Item2 - 1] += score.Item3;
                                }
                            }
                        }
                    }
                }
            }
            for (int i0 = 0; i0 < _boardScore.Size; i0++)
            {
                for (int j0 = 0; j0 < _boardScore.Size; j0++)
                {
                    if (_boardScore[i0, j0] < _minScore)
                    {
                        _minScore = _boardScore[i0, j0];
                    }
                    if (_boardScore[i0, j0] > _maxScore)
                    {
                        _maxScore = _boardScore[i0, j0];
                    }
                    if (_boardScore[i0, j0] < 0)
                    {
                        _boardOwnership[i0, j0] = WColor.WHITE;
                    }
                    else if (_boardScore[i0, j0] > 0)
                    {
                        _boardOwnership[i0, j0] = WColor.BLACK;
                    }
                }
            }
            _scoreScaleFactor = 1.0 / (_maxScore - _minScore);
        }

        private WColor EdgeInfluenceScanIn(int i0, int j0, int di, int dj)
        {
            WColor closestColor = _boardAux[i0, j0];
            int i = i0;
            int j = j0;
            int k0 = 0;
            while ((closestColor == WColor.EMPTY) && (k0 < _influenceRange))
            {
                i += di;
                j += dj;
                closestColor = _boardAux[i, j];
                k0++;
            }
            return closestColor;
        }

        protected static Tuple<int,int,double> CalcScore(int di, int dj, int i0, int j0, BoardBase board)
        {
            int i = i0 + di;
            int j = j0 + dj;
            var empty = new Tuple<int, int, double>(i0, j0, 0.0);
            if ( (di==0) && (dj==0) )
            {
                return empty;
            }
            if (!BoardUtil.CheckBounds(board.Size, i, j))
            {
                return empty;
            }
            if (board[i,j] == WColor.EMPTY)
            {
                double score = 1.0 / (di * di + dj * dj);
                if (board[i0, j0] != WColor.BLACK)
                {
                    score = -score;
                }
                return new Tuple<int, int, double>(i, j, score);
            }
            return empty;
        }
    }
}
