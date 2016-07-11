using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Weiqi
{
    public static class SGFParser
    {
        static public GameRecord ParseGameRecord(string buffer)
        {
            GameRecord gr = new GameRecord();
            buffer.Trim();
            buffer = buffer.Replace("\n", string.Empty);
            buffer = buffer.Replace("\r", string.Empty);
            buffer = buffer.Replace(" ", string.Empty);
            buffer = buffer.Replace(";", string.Empty);
            buffer = buffer.TrimStart('(');
            buffer = buffer.TrimEnd(')');

            int p1;
            int p2;
            while (buffer.Length > 0)
            {
                p1 = Regex.Match(buffer, "[A-Z]").Index;
                p2 = buffer.IndexOf('[', p1 + 1);
                p2 = buffer.IndexOf(']', p2 + 1);
                p2 = p2 + Regex.Match(buffer.Substring(p2 + 1), "[A-Z]").Index;

                string subBuffer = buffer.Substring(p1, p2 - p1 + 1);
                int b1 = subBuffer.IndexOf('[');
                string key = subBuffer.Substring(0, b1);
                string value = subBuffer.Substring(b1 + 1, subBuffer.Length - b1 - 2);

                SetValue(key, value, gr);

                buffer = buffer.Substring(p2 + 1);
            }

            return gr;
        }

        static public GameRecord GetGameRecord(string fileName)
        {
            try
            {
                using (StreamReader reader = new StreamReader(fileName))
                {
                    string buffer = reader.ReadToEnd();
                    GameRecord gr = ParseGameRecord(buffer);
                    gr.FileName = fileName;
                    return gr;
                }
            }
            catch (Exception ex)
            {
                if (ex is IOException)
                {
                    Console.WriteLine("unable to open " + Path.GetFileName(fileName));
                }
                else
                {
                    Console.WriteLine("unable to parse " + Path.GetFileName(fileName));
                    File.Copy(fileName, @"C:\Users\chunsun\SkyDrive\Projects\Weiqi\DataSet\temp\" + Path.GetFileName(fileName));
                }
                return null;
            }
        }

        private static void SetValue(string key, string value, GameRecord gr)
        {
            switch (key)
            {
                case "RE":
                    
                    gr.Result.Winner = WColor.EMPTY;
                    gr.Result.Margin = 0.0F;
                    gr.Result.Resign = false;

                    int winPos = (new[] { value.IndexOf("+"), value.IndexOf("胜"), value.IndexOf("勝") }).Max();

                    if (winPos > 0)
                    {
                        var bIndex = Math.Max(value.IndexOf("B"), value.IndexOf("黑"));
                        var wIndex = Math.Max(value.IndexOf('W'), value.IndexOf("白"));
                        if ((bIndex == -1) && (wIndex >= 0))
                        {
                            gr.Result.Winner = WColor.WHITE;
                        }
                        else if ((wIndex == -1) && (bIndex >= 0))
                        {
                            gr.Result.Winner = WColor.BLACK;
                        }
                        else if ((wIndex >= 0) && (bIndex >= 0) && (wIndex < bIndex))
                        {
                            gr.Result.Winner = WColor.WHITE;
                        }
                        else if ((wIndex >= 0) && (bIndex >= 0) && (wIndex > bIndex))
                        {
                            gr.Result.Winner = WColor.BLACK;
                        }
                        else
                        {
                            Console.WriteLine("Unable to parse file: " + Path.GetFileName(gr.FileName) + ": " + value);
                            File.Copy(gr.FileName, @"C:\Users\chunsun\SkyDrive\Projects\Weiqi\DataSet\temp\" + Path.GetFileName(gr.FileName));
                        }

                        if (value.Contains("R") || value.Contains("中"))
                        {
                            gr.Result.Resign = true;
                        }
                        else
                        {
                            int marginPos1 = winPos + 1;
                            int marginLength = value.Length - marginPos1;

                            int jpnPt = value.IndexOf("目");
                            int cnPt = value.IndexOf("子");


                            if (jpnPt > 0)
                            {
                                marginLength = jpnPt - marginPos1;
                            }
                            else if (cnPt > 0)
                            {
                                marginLength = cnPt - marginPos1;
                            }

                            string strMargin = value.Substring(marginPos1, marginLength);
                            float margin;
                            if (float.TryParse(strMargin, out margin))
                            {
                                if ((margin / 0.25) % 2 == 1)
                                {
                                    margin *= 2;
                                }
                                else if (cnPt > 0)
                                {
                                    margin *= 2;
                                }
                                gr.Result.Margin = margin;
                            }
                            else if (string.IsNullOrEmpty(strMargin) || ((jpnPt<0) && (cnPt<0)))
                            {
                                gr.Result.Margin = 0;
                                gr.Result.Resign = true;
                            }
                            else
                            {
                                Console.WriteLine("Unable to parse: " + Path.GetFileName(gr.FileName) + " : " + value);
                                File.Copy(gr.FileName, @"C:\Users\chunsun\SkyDrive\Projects\Weiqi\DataSet\temp\" + Path.GetFileName(gr.FileName));
                            }
                        }
                    }
                    break;
                case "B":
                    gr.GameSequence.Add(new Tuple<byte, byte, WColor>( (byte)(value[0]-'a'), (byte)(value[1]-'a'), WColor.BLACK));
                    break;
                case "W":
                    gr.GameSequence.Add(new Tuple<byte, byte, WColor>( (byte)(value[0]-'a'), (byte)(value[1]-'a'), WColor.WHITE));
                    break;
                case "AB":
                    AddGameSequence(gr, value, WColor.BLACK);
                    //gr.GameSequence.Add(new Tuple<byte, byte, WColor>( (byte)(value[0]-'a'), (byte)(value[1]-'a'), WColor.BLACK));
                    break;
                case "AW":
                    AddGameSequence(gr, value, WColor.WHITE);
                    //gr.GameSequence.Add(new Tuple<byte, byte, WColor>( (byte)(value[0]-'a'), (byte)(value[1]-'a'), WColor.WHITE));
                    break;
                case "PW":
                    gr.BlackName = value;
                    break;
                case "PB":
                    gr.WhiteName = value;
                    break;
                case "KM":
                    gr.Komi = float.Parse(value);
                    break;
                case "SZ":
                    gr.Size = (byte)int.Parse(value);
                    break;
                case "GM":
                case "FF":
                case "CA":
                case "HA":
                case "WR":
                case "BR":
                case "RU":
                case "EV":
                case "DT":
                case "RO":
                case "PC":
                case "WT":
                case "BT":
                case "TM":
                    break;
                default:
                    Console.WriteLine("[Exception] " + Path.GetFileName(gr.FileName) + " Key = " + key + " Value = " + value);
                    break;
            }
        }

        private static void AddGameSequence(GameRecord gr, string value, WColor wColor)
        {
            int p = 0;
            while (p + 1< value.Length)
            {
                gr.GameSequence.Add(new Tuple<byte, byte, WColor>((byte)(value[p] - 'a'), (byte)(value[p + 1] - 'a'), wColor));
                p++;
                while (p + 1 < value.Length)
                {
                    p++;
                    if ((value[p] >= 'a') && (value[p] <= 'z'))
                    {
                        break;
                    }
                }
            }
        }
    }
}
