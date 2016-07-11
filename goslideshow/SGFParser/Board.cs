using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Weiqi
{
    [Serializable]
    public class BoardGeneric<T>
    {
        public byte Size { get; set; }
        protected T[] _board;

        public BoardGeneric(byte size)
        {
            Size = size;
            _board = new T[Size * Size];
        }

        protected BoardGeneric() { }

        public BoardGeneric(BoardGeneric<T> b) : this(b.Size)
        {
            Array.Copy(b._board, _board, _board.Length);
        }

        public void VSet(int row, int col, T value)
        {
            _board[row * Size + col] = value;
        }

        public T VGet(int row, int col)
        {
            return _board[row * Size + col];
        }

        public T this[int row, int col]
        {
            get
            {
                return VGet(row, col);
            }
            set
            {
                VSet(row, col, value);
            }
        }

        public T this[Tuple<int,int> index]
        {
            get
            {
                return VGet(index.Item1, index.Item2);
            }
            set
            {
                VSet(index.Item1, index.Item2, value);
            }
        }
    }

    [Serializable]
    public class BoardBase : BoardGeneric<byte>
    {
        public BoardBase(byte size) : base(size)
        {
        }

        protected BoardBase() { }

        public BoardBase(BoardBase b) : base(b)
        {
        }

        public void Set(int row, int col, WColor color)
        {
            _board[row * Size + col] &= (byte)0xFC;
            _board[row * Size + col] |= (byte)color;
        }

        public WColor Get(int row, int col)
        {
            return (WColor)(_board[row * Size + col] & (byte)0x03);
        }

        public new WColor this[int row, int col]
        {
            get
            {
                return Get(row, col);
            }
            set
            {
                Set(row, col, value);
            }
        }

        public new WColor this[Tuple<int,int> index]
        {
            get
            {
                return Get(index.Item1, index.Item2);
            }
            set
            {
                Set(index.Item1, index.Item2, value);
            }
        }

        public void Mark(int row, int col, Mark markid)
        {
            _board[row * Size + col] |= (byte)(1 << ((int)markid + 1));
        }

        public Mark GetMark(int row, int col)
        {
            return (Mark)((_board[row * Size + col] & (byte)0xFC) >> 2);
        }

        public void UnMark(int row, int col, Mark markid)
        {
            _board[row * Size + col] &= (byte)(~(1 << ((int)markid + 1)));
        }

        public void ClearMark(int row, int col)
        {
            _board[row * Size + col] &= (byte)0x03;
        }

        public IEnumerable<Tuple<int, int>> LoopPoints(Mask _mask)
        {
            if (_mask == null)
            {
                for (int i = 0; i < Size; i++)
                {
                    for (int j = 0; j < Size; j++)
                    {
                        yield return new Tuple<int, int>(i, j);
                    }
                }
            }
            else
            {
                foreach (var item in _mask.GetOpenMask())
                {
                    yield return item;
                }
            }
        }



    }
    
    public class Mask
    {
        public byte Size { get; set; }
        private BoardGeneric<bool> _mask;
        private HashSet<Tuple<int, int>> _border;

        public Mask(byte size)
        {
            Size = size;
            _mask = new BoardGeneric<bool>(Size);
            _border = new HashSet<Tuple<int, int>>();
        }

        private Mask()
        {
        }

        public List<Tuple<int,int>> Border
        {
            get
            {
                return _border.ToList();
            }
        }

        public void SetMaskClose(List<Tuple<byte,byte>> maskList)
        {
            foreach (var openPoint in maskList)
            {
                var row = openPoint.Item1;
                var col = openPoint.Item2;
                _mask[row, col] = true;
            }
        }

        public void SetMaskClose(int row, int col)
        {
            _mask[row, col] = true;
        }

        public void SetMaskOpen(List<Tuple<byte, byte>> maskList)
        {
            foreach (var openPoint in maskList)
            {
                var row = openPoint.Item1;
                var col = openPoint.Item2;
                _mask[row, col] = false;
            }
        }

        public void SetMaskOpen(int row, int col)
        {
            _mask[row, col] = false;
        }
        
        public void SetMaskCloseAll()
        {
            for (int i = 0; i < Size; i++)
            {
                for (int j = 0; j < Size; j++)
                {
                    _mask[i, j] = true;
                }
            }
        }

        public void SetMaskOpenAll()
        {
            for (int i = 0; i < Size; i++)
            {
                for (int j = 0; j < Size; j++)
                {
                    _mask[i, j] = false;
                }
            }
        }

        public bool CheckMaskClose(int row, int col)
        {
            return _mask[row, col];
        }

        public List<Tuple<int, int>> GetOpenMaskList()
        {
            List<Tuple<int, int>> retList = new List<Tuple<int, int>>();
            for (int i = 0; i < Size; i++)
            {
                for (int j = 0; j < Size; j++)
                {
                    if (!_mask[i, j])
                    {
                        retList.Add(new Tuple<int, int>(i, j));
                    }
                }
            }
            return retList;
        }

        public IEnumerable<Tuple<int,int>> GetOpenMask()
        {
            for (int i = 0; i < Size; i++)
            {
                for (int j = 0; j < Size; j++)
                {
                    if (!_mask[i,j])
                    {
                        yield return new Tuple<int, int>(i, j);
                    }
                }
            }
        }

        public IEnumerable<Tuple<int, int>> GetCloseMask()
        {
            for (int i = 0; i < Size; i++)
            {
                for (int j = 0; j < Size; j++)
                {
                    if (_mask[i, j])
                    {
                        yield return new Tuple<int, int>(i, j);
                    }
                }
            }
        }

        public void CalculateBorder()
        {
            _border.Clear();
            for (int i = 0; i < Size; i++)
            {
                for (int j = 0; j < Size; j++)
                {
                    if ((_mask[i,j]) && (!_border.Contains(new Tuple<int,int>(i,j))))
                    {
                        foreach (var item in LoopNeighbor(i, j, Size))
                        {
                            int x = item.Item1;
                            int y = item.Item2;
                            if (!_mask[x, y])
                            {
                                _border.Add(new Tuple<int, int>(i, j));
                                break;
                            }
                        }
                    }
                }
            }
        }

        public void ShrinkMask(ScoreEst est, BoardBase board)
        {
            var threashold = est.GetUncertaintyThreashold();
            foreach (var position in this.GetOpenMask())
            {
                if (board[position] == WColor.EMPTY)
                {
                    if (est.GetUncertainty(position.Item1, position.Item2) < threashold)
                    {
                        SetMaskClose(position.Item1, position.Item2);
                    }
                }
            }
            foreach (var position in this.GetCloseMask())
            {
                if (board[position] == WColor.EMPTY)
                {
                    bool revert = true;
                    foreach (var neighbor in LoopNeighbor(position.Item1, position.Item2, board.Size))
                    {
                        if (CheckMaskClose(neighbor.Item1, neighbor.Item2))
                        {
                            revert = false;
                        }
                    }
                    if (revert)
                    {
                        SetMaskOpen(position.Item1, position.Item2);
                    }
                }
            }
        }

        private IEnumerable<Tuple<int,int>> LoopNeighbor(int i, int j, int size)
        {
            for (int ii = -1; ii <= 1; ii++)
            {
                for (int jj = -1; jj <= 1; jj++)
                {
                    if (ii * jj != 0)
                    {
                        if (BoardUtil.CheckBounds(size, i + ii, j + jj))
                        {
                            yield return new Tuple<int, int>(i + ii, j + jj);
                        }
                    }
                }
            }
        }
    }

    [Serializable]
    public class BoardStatus : IComparable
    {
        public BoardBase Board { get; set; }
        public WColor Turn { get; set; }

        private BoardStatus() { }

        public BoardStatus(byte size)
        {
            Board = new BoardBase(size);
            Turn = WColor.EMPTY;
        }

        public BoardStatus(BoardStatus bs)
        {
            Board = new BoardBase(bs.Board);
            Turn = bs.Turn;
        }

        internal void FlipTurn()
        {
            Turn = ColorUtils.Flip(Turn);
        }

        public override string ToString()
        {
            using (var stream = new MemoryStream())
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize(stream, this);
                stream.Flush();
                stream.Position = 0;
                return Convert.ToBase64String(stream.ToArray());
            }
        }

        public int CompareTo(object obj)
        {
            return ToString().CompareTo(obj.ToString());
        }

    }

    public class PartialBoard
    {
        public BoardStatus boardStatus { get; set; }
        public Mask mask { get; set; }

        private PartialBoard() { }

        public PartialBoard(BoardStatus status, Mask inputMask)
        {
            boardStatus = status;
            mask = inputMask;
        }

        public void Play(byte row, byte col, WColor color)
        {

        }
    }

}
