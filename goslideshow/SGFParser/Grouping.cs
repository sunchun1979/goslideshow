using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Weiqi;

namespace Weiqi
{
    public class StoneGroup
    {
        public int Label;
        public List<Tuple<int, int>> Stones;
        public StoneGroup()
        {
            Stones = new List<Tuple<int, int>>();
            Label = 0;
        }
    };

    public class Grouping
    {
        private BoardBase _boardBase;
        private BoardGeneric<int> _groupId;
        private List<StoneGroup> _groupStones;

        private const int GRP_NOT_ASSIGNED = -1;
        private const int BOXDIM = 2;

        private Grouping() { }

        public Grouping(BoardBase status)
        {
            _boardBase = status;
            _groupId = new BoardGeneric<int>(status.Size);
            _groupStones = new List<StoneGroup>();
            CalculateGroup2();
        }

        private void CalculateGroup2()
        {
            int size = _boardBase.Size;
            for (int i = 0; i < size; i++)
                for (int j = 0; j < size; j++)
                {
                    _groupId[i, j] = GRP_NOT_ASSIGNED;
                }
            _groupStones.Clear();
            var candidateMap = new Dictionary<int, HashSet<Tuple<int,int>>>();
            candidateMap.Add(GRP_NOT_ASSIGNED, new HashSet<Tuple<int, int>>());
            for (int i = 0; i < size; i++)
            {
                for (int j = 0; j < size; j++)
                {
                    if (_boardBase[i, j] != WColor.EMPTY)
                    {
                        candidateMap[GRP_NOT_ASSIGNED].Add(new Tuple<int,int>(i, j));
                    }
                }
            }
            int currentGroup = 0;
            while ( candidateMap[GRP_NOT_ASSIGNED].Count > 0)
            {
                Tuple<int, int> stone = candidateMap[GRP_NOT_ASSIGNED].First();
                candidateMap.Add(currentGroup, new HashSet<Tuple<int, int>>());
                var candidateQueue = new Queue<Tuple<int, int>>();
                candidateQueue.Enqueue(stone);
                while (candidateQueue.Count > 0)
                {
                    var headStone = candidateQueue.Dequeue();
                    candidateMap[currentGroup].Add(headStone);
                    candidateMap[GRP_NOT_ASSIGNED].Remove(headStone);
                    var x0 = headStone.Item1;
                    var y0 = headStone.Item2;
                    {
                        int[] dx = { -1, 0, 1, 0 };
                        int[] dy = { 0, 1, 0, -1 };
                        for (int i = 0; i < 4; i++)
                        {
                            var x = x0 + dx[i];
                            var y = y0 + dy[i];
                            if (BoardUtil.CheckBounds(size, x, y))
                            {
                                if ((_boardBase[x, y] == _boardBase[x0, y0]) && (candidateMap[GRP_NOT_ASSIGNED].Contains(new Tuple<int, int>(x, y))))
                                {
                                    candidateQueue.Enqueue(new Tuple<int, int>(x, y));
                                }
                            }
                        }
                    }
                    {
                        int[] dx = { -1, 1, 1, -1 };
                        int[] dy = { -1, 1, -1, 1 };
                        for (int i = 0; i < 4; i++)
                        {
                            var x = x0 + dx[i];
                            var y = y0 + dy[i];
                            if (BoardUtil.CheckBounds(size, x, y))
                            {
                                if ((_boardBase[x, y] == _boardBase[x0, y0]) && (candidateMap[GRP_NOT_ASSIGNED].Contains(new Tuple<int, int>(x, y))))
                                {
                                    WColor revColor = ColorUtils.Flip(_boardBase[x0, y0]);
                                    if ((_boardBase[x0 + dx[i], y0] != revColor) || (_boardBase[x0, y0 + dy[i]] != revColor))
                                    {
                                        candidateQueue.Enqueue(new Tuple<int, int>(x, y));
                                    }
                                }
                            }
                        }
                    }
                }
                currentGroup++;
            }
            foreach (var group in candidateMap)
            {
                if (group.Key != GRP_NOT_ASSIGNED)
                {
                    var stoneGrp = new StoneGroup();
                    stoneGrp.Label = group.Key;
                    stoneGrp.Stones = group.Value.ToList();
                    _groupStones.Add(stoneGrp);
                    foreach (var stone in stoneGrp.Stones)
                    {
                        _groupId[stone] = stoneGrp.Label;
                    }
                }
            }
        }

        public int GetNumOfGroups()
        {
            return _groupStones.Count();
        }

        public WColor GetGroupColor(int groupIdx)
        {
            var stone = _groupStones[groupIdx].Stones.First();
            return _boardBase[stone];
        }

        public List<Tuple<int,int>> GetGroup(int groupIdx)
        {
            if ((groupIdx < 0) || (groupIdx >= _groupStones.Count()))
            {
                return null;
            }
            else
            {
                return _groupStones[groupIdx].Stones;
            }
        }

        public List<Tuple<int,int>> GetGroupLiberty(int groupIdx)
        {
            if ((groupIdx < 0) || (groupIdx >= _groupStones.Count()))
            {
                return null;
            }
            else
            {
                int[] dx = new int[4] { 1, -1, 0, 0 };
                int[] dy = new int[4] { 0, 0, -1, 1 };
                var libertyList = new HashSet<Tuple<int, int>>();
                foreach (var stone in _groupStones[groupIdx].Stones)
                {
                    for (int i = 0; i < 4; i++)
                    {
                        int x = stone.Item1 + dx[i];
                        int y = stone.Item2 + dy[i];
                        if (!libertyList.Contains(new Tuple<int, int>(x, y)))
                        {
                            if (BoardUtil.CheckBounds(_boardBase.Size, (byte)x, (byte)y))
                            {
                                if (_boardBase[x, y] == WColor.EMPTY)
                                {
                                    libertyList.Add(new Tuple<int, int>(x, y));
                                }
                            }
                        }
                    }
                }
                return libertyList.ToList();
            }
        }

        public int GetGroupId(byte row, byte col)
        {
            return _groupId[row, col];
        }
    }
}
