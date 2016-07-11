using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Weiqi
{
    public class GameResult
    {
        public WColor Winner { get; set; }
        public float Margin { get; set; }
        public bool Resign { get; set; }

        public GameResult()
        {
            Winner = WColor.EMPTY;
            Margin = 0;
            Resign = false;
        }

        public override string ToString()
        {
            if (Winner == WColor.EMPTY)
            {
                return "DRAW";
            }
            else
            {
                string ret = "";
                if (Winner == WColor.WHITE)
                {
                    ret = "W+";
                }
                else
                {
                    ret = "B+";
                }
                if (Resign)
                {
                    ret += "R";
                }
                else
                {
                    ret += Margin.ToString("0.0");
                }
                return ret;
            }
        }
    };

    public class GameRecord
    {
        public string FileName { get; set; }
        public string BlackName { get; set; }
        public string WhiteName { get; set; }
        public float Komi { get; set; }
        public byte Size { get; set; }

        public GameResult Result { get; set; }

        public List<Tuple<byte, byte, WColor>> GameSequence { get; set; }
        public string Comments { get; set; }

        public GameRecord()
        {
            Size = 19;
            Result = new GameResult();
            GameSequence = new List<Tuple<byte, byte, WColor>>();
        }
    }
}
