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
        Pen stoneHighlight;

        Timer timer = new Timer();

        public Form1()
        {
            InitializeComponent();
        }

        private void changeGame()
        {
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
                boardBG = new SolidBrush(Color.FromArgb(5, 5, 5));
                blackStone = new SolidBrush(Color.Black);
                whiteStone = new SolidBrush(Color.FromArgb(10, 10, 10));
                stoneHighlight = new Pen(Color.FromArgb(20, 20, 20), 1);
            }
            else
            {
                boardBG = new SolidBrush(Color.Gray);
                blackStone = new SolidBrush(Color.Black);
                whiteStone = new SolidBrush(Color.LightGray);
                stoneHighlight = new Pen(Color.White, 1);
            }
        }

        private void paintForm()
        {
            if (archive == null)
            {
                return;
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            timer.Interval = 500;
            timer.Tick += new EventHandler(tick);
            timer.Start();
        }

        private void tick(Object obj, EventArgs args)
        {
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
                moveCount = moveMax;
            }
            currentGame.PlayRecord(1);
        }

        private void Form1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)'O' || e.KeyChar == (char)'o')
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
            else if (e.KeyChar == (char)'F' || e.KeyChar == (char)'f')
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
    }
}
