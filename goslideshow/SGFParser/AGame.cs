using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Weiqi
{
    public class AGame
    {
        private GameRecord _gameRecord;
        private int _numMoves;
        private int _currentRecMove;
        private SortedSet<string> boardHistory;

        private Tuple<byte,byte> _LatestMove;
        private BoardStatus _currentStatus;

        private byte _size;

        private AGame()
        {
            _gameRecord = null;
            boardHistory = new SortedSet<string>();
        }

        public AGame(byte size) : this()
        {
            _size = size;
            _LatestMove = null;
            _currentStatus = new BoardStatus(_size);
            _currentStatus.Turn = WColor.BLACK;
            _numMoves = 0;
            _currentRecMove = 0;
            boardHistory.Add(_currentStatus.ToString());
        }

        public AGame(string fileName)
        {
            _gameRecord = SGFParser.GetGameRecord(fileName);
            boardHistory = new SortedSet<string>();
            _LatestMove = null;
            _size = _gameRecord.Size;
            _currentStatus = new BoardStatus(_size);
            _numMoves = 0;
            _currentRecMove = 0;
            boardHistory.Add(_currentStatus.ToString());
        }
        public AGame(Stream stream)
        {
            using (StreamReader reader = new StreamReader(stream))
            {
                string buffer = reader.ReadToEnd();
                _gameRecord = SGFParser.ParseGameRecord(buffer);
            }
            boardHistory = new SortedSet<string>();
            _LatestMove = null;
            _size = _gameRecord.Size;
            _currentStatus = new BoardStatus(_size);
            _numMoves = 0;
            _currentRecMove = 0;
            boardHistory.Add(_currentStatus.ToString());
        }

        public GameRecord Record 
        {
            get
            {
                return _gameRecord;
            }
        }

        public BoardStatus Status 
        { 
            get 
            {
                return _currentStatus;
            }
        }

        public Tuple<byte,byte> CurrentMove 
        { 
            get
            {
                return _LatestMove;
            }
        }

        public void Reset()
        {
            boardHistory.Clear();
            _LatestMove = null;
            _currentStatus = new BoardStatus(_size);
            _currentRecMove = 0;
            boardHistory.Add(_currentStatus.ToString());
            _numMoves = 0;
        }

        public bool Play(byte row, byte col)
        {
            if ( !BoardUtil.CheckBounds(_size, row, col) )
            {
                return false;
            }
            if (_currentStatus.Board.Get(row, col) != WColor.EMPTY)
            {
                return false;
            }
            else
            {
                BoardStatus temptNew = _currentStatus;
                if (!MoveUtils.Move(ref temptNew, row, col))
                {
                    return false;
                }
                if (!boardHistory.Contains(temptNew.ToString()))
                {
                    boardHistory.Add(temptNew.ToString());
                    _currentStatus = temptNew;
                    _LatestMove = new Tuple<byte, byte>(row, col);
                    _numMoves++;
                    return true;
                }
                return false;
            }
        }

        public void Play(byte row, byte col, WColor color)
        {
            _currentStatus.Turn = color;
            Play(row, col);
        }

        public int NumMoves { get { return _gameRecord.GameSequence.Count; } }

        public void PlayRecord(int nSteps)
        {
            for (int i = 0; i < nSteps; i++)
            {
                int p = _currentRecMove + i;
                Play(_gameRecord.GameSequence[p].Item1, _gameRecord.GameSequence[p].Item2, _gameRecord.GameSequence[p].Item3);
            }
            _currentRecMove += nSteps;
        }
    }
}
