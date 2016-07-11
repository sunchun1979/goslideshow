using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using Weiqi;

namespace goslideshow
{
    public partial class Form1 : Form
    {
        FileStream dbFS = null;
        ZipArchive archive = null;
        List<ZipArchiveEntry> gameEntires = null;
        AGame currentGame = null;
        bool darkMode = false;
        bool continueToEnd = false;
        int moveCount = 0;
        int moveMax = 20;

        Brush boardBG;
        Brush blackStone;
        Brush whiteStone;
        Pen blackPen;
        Pen whitePen;
        Pen stoneHighlight;

        Timer timer = new Timer();

        public Form1()
        {
            InitializeComponent();
        }

        private void changeGame()
        {
            if (archive == null)
            {
                return;
            }
            Random r = new Random();
            int gameIndex = r.Next(gameEntires.Count);
            currentGame = new AGame(gameEntires[gameIndex].Open());
            int moveIndex = r.Next(currentGame.NumMoves - moveMax);
            currentGame.PlayRecord(moveIndex);
        }

        private void setColor()
        {
            if (darkMode)
            {
                boardBG = new SolidBrush(Color.FromArgb(10, 10, 10));
                blackStone = new SolidBrush(Color.Black);
                whiteStone = new SolidBrush(Color.FromArgb(20, 20, 20));
                blackPen = new Pen(Color.Black, 1);
                whitePen = new Pen(Color.FromArgb(20, 20, 20), 1);
                stoneHighlight = new Pen(Color.FromArgb(30, 30, 30), 1);
            }
            else
            {
                boardBG = new SolidBrush(Color.Gray);
                blackStone = new SolidBrush(Color.Black);
                whiteStone = new SolidBrush(Color.LightGray);
                blackPen = new Pen(Color.Black, 1);
                whitePen = new Pen(Color.LightGray, 1);
                stoneHighlight = new Pen(Color.White, 1);
            }
        }

        private void paintBoard()
        {
            if (archive == null)
            {
                return;
            }
            BufferedGraphics myBuffer;
            BufferedGraphicsContext currentContext;
            currentContext = BufferedGraphicsManager.Current;
            myBuffer = currentContext.Allocate(this.CreateGraphics(), this.DisplayRectangle);

            Graphics G = myBuffer.Graphics;
            setColor();
            G.FillRectangle(boardBG, 0, 0, this.Size.Width, this.Size.Height);

            int _cell = Math.Min(this.Height, this.Width) / 21;
            int _stoneSize = (int)(_cell * 0.9);
            int _xShift, _yShift;
            if (this.Height > this.Width)
            {
                _xShift = 0;
                _yShift = (this.Height - this.Width) / 2;
            }
            else
            {
                _xShift = (this.Width - this.Height) / 2;
                _yShift = 0;
            }

            int size = currentGame.Status.Board.Size;
            for (int i = 0; i < size; i++)
            {
                int x1 = _cell;
                int y1 = _cell + i * _cell;
                int x2 = size * _cell;
                int y2 = y1;
                G.DrawLine(whitePen, new Point(x1 + _xShift, y1 + _yShift), new Point(x2 + _xShift, y2 + _yShift));
            }
            for (int i = 0; i < size; i++)
            {
                int x1 = _cell + i * _cell;
                int y1 = _cell;
                int x2 = x1;
                int y2 = size * _cell;
                G.DrawLine(whitePen, new Point(x1 + _xShift, y1 + _yShift), new Point(x2 + _xShift, y2 + _yShift));
            }
            {
                int[] dotx = { 4, 10, 16, 4, 10, 16, 4, 10, 16 };
                int[] doty = { 4, 4, 4, 10, 10, 10, 16, 16, 16 };
                int stard = _cell / 10;
                for (int i = 0; i < dotx.Length; i++)
                {
                    int x0 = _cell * dotx[i];
                    int y0 = _cell * doty[i];
                    G.FillEllipse(whiteStone, x0 - stard + _xShift, y0 - stard + _yShift, 2 * stard, 2 * stard);
                }
            }

            for (int i = 0; i < size; i++)
            {
                for (int j = 0; j < size; j++)
                {
                    WColor c = currentGame.Status.Board.Get((byte)i, (byte)j);
                    int x0 = _cell * (i + 1);
                    int y0 = _cell * (j + 1);
                    if (c == WColor.BLACK)
                    {
                        G.FillEllipse(blackStone, x0 - _stoneSize / 2 + _xShift, y0 - _stoneSize / 2 + _yShift, _stoneSize, _stoneSize);
                    }
                    else if (c == WColor.WHITE)
                    {
                        G.FillEllipse(whiteStone, x0 - _stoneSize / 2 + _xShift, y0 - _stoneSize / 2 + _yShift, _stoneSize, _stoneSize);
                    }
                }
            }
            int cx0 = _cell * (currentGame.CurrentMove.Item1 + 1);
            int cy0 = _cell * (currentGame.CurrentMove.Item2 + 1);
            G.DrawEllipse(stoneHighlight, cx0 - _stoneSize / 2 + _xShift, cy0 - _stoneSize / 2 + _yShift, _stoneSize, _stoneSize);

            myBuffer.Render();
            myBuffer.Render(this.CreateGraphics());
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            WindowState = FormWindowState.Maximized;
            timer.Interval = 500;
            timer.Tick += new EventHandler(tick);
            timer.Start();
        }

        private void tick(Object obj, EventArgs args)
        {
            if (archive == null)
            {
                return;
            }
            if (moveCount == 0)
            {
                changeGame();
                continueToEnd = false;
            }
            if (!continueToEnd)
            {
                moveCount = (moveCount+1) % moveMax;
            }
            else
            {
                moveCount = moveMax - 1;
            }
            currentGame.PlayRecord(1);
            paintBoard();
        }

        private void Form1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)'F' || e.KeyChar == (char)'f')
            {
                if (this.FormBorderStyle != FormBorderStyle.None)
                {
                    this.FormBorderStyle = FormBorderStyle.None;
                }
                else
                {
                    this.FormBorderStyle = FormBorderStyle.Sizable;
                }
            }
            else if (e.KeyChar == (char)'C' || e.KeyChar == (char)'c')
            {
                continueToEnd = !continueToEnd;
            }
            else if (e.KeyChar == (char)'+')
            {
                if (timer.Interval >= 500)
                {
                    timer.Interval /= 2;
                }
            }
            else if (e.KeyChar == (char)'-')
            {
                if (timer.Interval <= 4000)
                {
                    timer.Interval *= 2;
                }
            }
            else if (e.KeyChar == (char)' ')
            {
                darkMode = !darkMode;
            }
        }

        private void Form1_MouseUp(object sender, MouseEventArgs e)
        {
            OpenFileDialog od = new OpenFileDialog();
            od.InitialDirectory = @"C:\Users\chunsun\SkyDrive\Projects\Weiqi\DataSet";
            od.Filter = "zip | *.zip";
            if (od.ShowDialog() == DialogResult.OK)
            {
                dbFS = new FileStream(od.FileName, FileMode.Open);
                archive = new ZipArchive(dbFS, ZipArchiveMode.Read);
                gameEntires = archive.Entries.ToList();
            }
        }
    }
}
