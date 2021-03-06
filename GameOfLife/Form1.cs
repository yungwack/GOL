﻿using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace GameOfLife
{
    public partial class Form1 : Form
    {
        // init globals
        string bounds = "Finite";
        int width = 50;
        int height = 50;
        int min = 0; // minimum value for run to window
        int seed = 420691337;

        // The universe array
        bool[,] universe = new bool[50, 50];
        bool[,] scratchpad = new bool[50, 50];

        // Drawing colors
        Color gridColor = Color.Black;
        Color cellColor = Color.Gray;

        // The Timer class
        Timer timer = new Timer();

        //RNG
        Random ran = new Random();

        // Generation count
        int generations = 0;

        //Font
        Font font = new Font("Arial", 12f);

        //Cell count
        int isAlive;

        public Form1()
        {
            InitializeComponent(); // init program

            // get and load settings
            graphicsPanel1.BackColor = Properties.Settings.Default.PanelColor;
            gridColor = Properties.Settings.Default.GridColor;
            cellColor = Properties.Settings.Default.CellColor;
            width = Properties.Settings.Default.UniverseWidth;
            height = Properties.Settings.Default.UniverseHeight;
            
            // Setup the timer
            timer.Interval = Properties.Settings.Default.Milliseconds; // milliseconds
            timer.Tick += Timer_Tick;

            timer.Enabled = false; // start timer running
        }

        // Calculate the next generation of cells
        private void NextGeneration()
        {
            int count = 0;
            isAlive = 0;

            for (int y = 0; y < universe.GetLength(1); y++)
            {
                for (int x = 0; x < universe.GetLength(0); x++)
                {
                    scratchpad[x, y] = false;

                    if (toolStripToroidal.Checked == true)
                    {
                        count = CountNeighborsToroidal(x, y); //update neighbor coords
                    }
                    else if (toolStripFinite.Checked == true)
                    {
                        count = CountNeighborsFinite(x, y); //update neighbor coords
                    }
                    if (universe[x, y] == true)
                    {
                        //Living cells with less than 2 living neighbors die in the next generation.
                        //Living cells with more than 3 living neighbors die in the next generation.
                        if (count < 2 || count > 3) { scratchpad[x, y] = false; }

                        //Living cells with 2 or 3 living neighbors live in the next generation.
                        if (count == 2 || count == 3) { scratchpad[x, y] = true; }

                    }
                    else if (universe[x, y] == false)
                    {
                        //Dead cells with exactly 3 living neighbors live in the next generation.
                        if (count == 3) { scratchpad[x, y] = true; }
                    }
                    if (scratchpad[x, y] == true) { isAlive++; }
                }
            }

            //Swap array
            bool[,] temp = universe;
            universe = scratchpad;
            scratchpad = temp;

            // Increment generation count
            generations++;

            // Update status strip generations
            toolStripStatusLabelGenerations.Text = "Generations = " + generations.ToString();

            graphicsPanel1.Invalidate();
        }

        // The event called by the timer every Interval milliseconds.
        private void Timer_Tick(object sender, EventArgs e)
        {
            NextGeneration();
        }

        private void graphicsPanel1_Paint(object sender, PaintEventArgs e)
        {

            //convert to floats
            // Calculate the width and height of each cell in pixels
            // CELL WIDTH = WINDOW WIDTH / NUMBER OF CELLS IN X
            float cellWidth = (float)graphicsPanel1.ClientSize.Width / (float)universe.GetLength(0);
            // CELL HEIGHT = WINDOW HEIGHT / NUMBER OF CELLS IN Y
            float cellHeight = (float)graphicsPanel1.ClientSize.Height / (float)universe.GetLength(1);

            // A Pen for drawing the grid lines (color, width)
            Pen gridPen = new Pen(gridColor, 1);

            if (toolStripMenuItem2.Checked == true)
            {
                gridPen = new Pen(gridColor, 1);
            }
            else if (toolStripMenuItem2.Checked == false)
            {
                gridPen = new Pen(Color.Transparent, 1);
            }

            // A Brush for filling living cells interiors (color)
            Brush cellBrush = new SolidBrush(cellColor);

            // Iterate through the universe in the y, top to bottom
            for (int y = 0; y < universe.GetLength(1); y++)
            {
                // Iterate through the universe in the x, left to right
                for (int x = 0; x < universe.GetLength(0); x++)
                {
                    //float conversions

                    // A rectangle to represent each cell in pixels
                    RectangleF cellRect = RectangleF.Empty;
                    cellRect.X = (float)x * (float)cellWidth;
                    cellRect.Y = (float)y * (float)cellHeight;
                    cellRect.Width = (float)cellWidth;
                    cellRect.Height = (float)cellHeight;

                    // Fill the cell with a brush if alive
                    if (universe[x, y] == true)
                    {
                        e.Graphics.FillRectangle(cellBrush, cellRect);
                    }

                    // Outline the cell with a pen
                    e.Graphics.DrawRectangle(gridPen, cellRect.X, cellRect.Y, cellRect.Width, cellRect.Height);
                }
            }
            Brush hudBrush = new SolidBrush(Color.Red);

            StringFormat stringFormat = new StringFormat();
            stringFormat.Alignment = StringAlignment.Near;
            stringFormat.LineAlignment = StringAlignment.Far;

            Rectangle rect = graphicsPanel1.ClientRectangle;

            if (toolStripFinite.Checked == true)
            {
                bounds = "Finite";
            }
            else if (toolStripToroidal.Checked == true)
            {
                bounds = "Toroidal";
            }

            if (toolStripMenuItem1.Checked == true)
            {
                e.Graphics.DrawString("Generations: " + generations + "\nCell Count: " + isAlive + "\nSeed Value: "+ seed + "\nBoundary Type: " + bounds + "\nUniverse Size: {Width = " + width + ", Height = " + height + "}", font, hudBrush, rect, stringFormat);
            }
            else if (toolStripMenuItem1.Checked == false)
            {
                e.Graphics.DrawString("Generations: " + generations + "\nCell Count: " + isAlive + "\nBoundary Type: " + bounds + "\nUniverse Size: {Width = " + width + ", Height = " + height + "}", font, Brushes.Transparent, rect, stringFormat);
            }

            if (generations == min)
            {
                timer.Enabled = false; // stop timer when run to limit is reached
            }

            // Cleaning up pens and brushes
            gridPen.Dispose();
            cellBrush.Dispose();
        }

        private void graphicsPanel1_MouseClick(object sender, MouseEventArgs e)
        {
            // If the left mouse button was clicked
            if (e.Button == MouseButtons.Left)
            {
                // Calculate the width and height of each cell in pixels
                float cellWidth = (float)graphicsPanel1.ClientSize.Width / universe.GetLength(0);
                float cellHeight = (float)graphicsPanel1.ClientSize.Height / universe.GetLength(1);

                // Calculate the cell that was clicked in
                // CELL X = MOUSE X / CELL WIDTH
                int x = (int)(e.X / cellWidth);
                // CELL Y = MOUSE Y / CELL HEIGHT
                int y = (int)(e.Y / cellHeight);

                // Toggle the cell's state
                universe[x, y] = !universe[x, y];

                // Tell Windows you need to repaint
                graphicsPanel1.Invalidate();
            }
        }

        private void newToolStripButton_Click(object sender, EventArgs e)
        {
            for (int y = 0; y < universe.GetLength(1); y++)
            {
                // Iterate through the universe in the x, left to right
                for (int x = 0; x < universe.GetLength(0); x++)
                {
                    universe[x, y] = false;
                    scratchpad[x, y] = false;
                }
            }
            generations = 0;

            timer.Enabled = false;
            toolStripStatusLabelGenerations.Text = "Generations = " + generations.ToString();
            graphicsPanel1.Invalidate(); //nuke cells
        }

        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
            for (int y = 0; y < universe.GetLength(1); y++)
            {
                // Iterate through the universe in the x, left to right
                for (int x = 0; x < universe.GetLength(0); x++)
                {
                    universe[x, y] = false;
                    scratchpad[x, y] = false;
                }
            }

            generations = 0;
            timer.Enabled = false;
            toolStripStatusLabelGenerations.Text = "Generations = " + generations.ToString();
            graphicsPanel1.Invalidate(); //nuke cells
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            //START BUTTON
            timer.Enabled = true;
        }

        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            //PAUSE BUTTON
            timer.Enabled = false;
        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            //SKIP BUTTON
            NextGeneration();
        }

        /*private int RandomizeCells(int mode)
        {
            if (mode == 1)
            {

            }
        }
        */
        private int CountNeighborsToroidal(int x, int y)

        {

            int count = 0;

            int xLen = universe.GetLength(0);

            int yLen = universe.GetLength(1);


            for (int yOffset = -1; yOffset <= 1; yOffset++)

            {

                for (int xOffset = -1; xOffset <= 1; xOffset++)

                {

                    int xCheck = x + xOffset;

                    int yCheck = y + yOffset;
                    // if xOffset and yOffset are both equal to 0 then continue
                    if (xOffset == 0 && yOffset == 0) { continue; }

                    // if xCheck is less than 0 then continue
                    if (xCheck < 0) { xCheck = xLen - 1; }

                    // if yCheck is less than 0 then continue
                    if (yCheck < 0) { yCheck = yLen - 1; }

                    // if xCheck is greater than or equal too xLen then continue
                    if (xCheck >= xLen) { xCheck = 0; }

                    // if yCheck is greater than or equal too yLen then continue
                    if (yCheck >= yLen) { yCheck = 0; }

                    if (universe[xCheck, yCheck] == true) count++;

                }

            }
            return count;
        }

        private int CountNeighborsFinite(int x, int y)
        {
            int count = 0;

            int xLen = universe.GetLength(0);

            int yLen = universe.GetLength(1);


            for (int yOffset = -1; yOffset <= 1; yOffset++)

            {

                for (int xOffset = -1; xOffset <= 1; xOffset++)

                {

                    int xCheck = x + xOffset;

                    int yCheck = y + yOffset;

                    // if xOffset and yOffset are both equal to 0 then continue
                    if (xOffset == 0 && yOffset == 0) { continue; }

                    // if xCheck is less than 0 then continue
                    if (xCheck < 0) { continue; }

                    // if yCheck is less than 0 then continue
                    if (yCheck < 0) { continue; }

                    // if xCheck is greater than or equal too xLen then continue
                    if (xCheck >= xLen) { continue; }
                    // if yCheck is greater than or equal too yLen then continue
                    if (yCheck >= yLen) { continue; }

                    if (universe[xCheck, yCheck] == true) { count++; }

                }
            }
            return count;
        }

        private void SaveAs()
        {
            SaveFileDialog dlg = new SaveFileDialog();
            dlg.Filter = "All Files|*.*|Cells|*.cells";
            dlg.FilterIndex = 2; dlg.DefaultExt = "cells";


            if (DialogResult.OK == dlg.ShowDialog())
            {
                StreamWriter writer = new StreamWriter(dlg.FileName);

                // Write any comments you want to include first.
                // Prefix all comment strings with an exclamation point.
                // Use WriteLine to write the strings to the file. 
                // It appends a CRLF for you.
                // Iterate through the universe one row at a time.
                for (int y = 0; y < height; y++)
                {
                    // Create a string to represent the current row.
                    String currentRow = string.Empty;

                    // Iterate through the current row one cell at a time.
                    for (int x = 0; x < width; x++)
                    {
                        // If the universe[x,y] is alive then append 'O' (capital O)
                        // to the row string.
                        if (universe[x, y] == true)
                        {
                            currentRow = "O";
                        }

                        // Else if the universe[x,y] is dead then append '.' (period)
                        // to the row string.
                        else if (universe[x, y] == false)
                        {
                            currentRow = ".";
                        }
                        writer.Write(currentRow);
                    }
                    writer.Write("\n");
                    // Once the current row has been read through and the 
                    // string constructed then write it to the file using WriteLine.
                }

                // After all rows and columns have been written then close the file.
                writer.Close();
            }
        }

        private void LoadAs()
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Filter = "All Files|*.*|Cells|*.cells";
            dlg.FilterIndex = 2;

            if (DialogResult.OK == dlg.ShowDialog())
            {
                StreamReader reader = new StreamReader(dlg.FileName);

                // Create a couple variables to calculate the width and height
                // of the data in the file.
                int maxWidth = 0;
                int maxHeight = 0;

                // Iterate through the file once to get its size.
                while (!reader.EndOfStream)
                {
                    // Read one row at a time.
                    string row = reader.ReadLine();

                    // If the row begins with '!' then it is a comment
                    // and should be ignored.
                    if (row == "!")
                    {
                        continue;
                    }

                    // If the row is not a comment then it is a row of cells.
                    // Increment the maxHeight variable for each row read.
                    if (row != "!")
                    {
                        maxHeight++;
                    }

                    // Get the length of the current row string
                    // and adjust the maxWidth variable if necessary.
                    maxWidth = row.Length;
                }

                // Resize the current universe and scratchPad
                // to the width and height of the file calculated above.
                universe = new bool[maxWidth, maxHeight];
                scratchpad = new bool[maxWidth, maxHeight];
                // Reset the file pointer back to the beginning of the file.
                reader.BaseStream.Seek(0, SeekOrigin.Begin);
                int y = 0;
                // Iterate through the file again, this time reading in the cells.
                while (!reader.EndOfStream)
                {
                    // Read one row at a time.
                    string row = reader.ReadLine();

                    // If the row begins with '!' then
                    // it is a comment and should be ignored.
                    if (row == "!")
                    {
                        continue;
                    }
                    // If the row is not a comment then 
                    // it is a row of cells and needs to be iterated through.
                    for (int xPos = 0; xPos < row.Length; xPos++)
                    {
                        // If row[xPos] is a 'O' (capital O) then
                        // set the corresponding cell in the universe to alive.
                        if (row[xPos] == 'O')
                        {
                            universe[xPos, y] = true;
                        }

                        // If row[xPos] is a '.' (period) then
                        // set the corresponding cell in the universe to dead.
                        if (row[xPos] == '.')
                        {
                            universe[xPos, y] = false;
                        }
                    }
                    // Increment by one on the y value
                    y++;
                }
                // Close the file.
                reader.Close();
            }
            generations = 0;          
            graphicsPanel1.Invalidate();
        }

        // MENUS

        private void toolStripFinite_Click(object sender, EventArgs e)
        {
            //finite mode
            toolStripToroidal.Checked = false;
            toolStripFinite.Checked = true;
            graphicsPanel1.Invalidate();
        }

        private void toolStripToroidal_Click(object sender, EventArgs e)
        {
            //toroidal mode
            toolStripToroidal.Checked = true;
            toolStripFinite.Checked = false;
            graphicsPanel1.Invalidate();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close(); // KILL
        }

        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            graphicsPanel1.Invalidate(); // HIDE GRID
        }

        private void gridColorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // GRID COLOR DIALOG
            ColorDialog dlg = new ColorDialog();
            dlg.Color = gridColor;

            if (DialogResult.OK == dlg.ShowDialog())
            {
                gridColor = dlg.Color;

                graphicsPanel1.Invalidate();
            }
        }

        private void backColorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // BACKGROUND COLOR DIALOG
            ColorDialog dlg = new ColorDialog();
            dlg.Color = graphicsPanel1.BackColor;

            if (DialogResult.OK == dlg.ShowDialog())
            {
                graphicsPanel1.BackColor = dlg.Color;

                graphicsPanel1.Invalidate();
            }
        }

        private void cellColorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // CELL COLOR DIALOG
            ColorDialog dlg = new ColorDialog();
            dlg.Color = cellColor;

            if (DialogResult.OK == dlg.ShowDialog())
            {
                cellColor = dlg.Color;

                graphicsPanel1.Invalidate();
            }
        }

        private void gridColorToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            // GRID COLOR DIALOG - CONTEXT MENU
            ColorDialog dlg = new ColorDialog();
            dlg.Color = gridColor;

            if (DialogResult.OK == dlg.ShowDialog())
            {
                gridColor = dlg.Color;

                graphicsPanel1.Invalidate();
            }
        }

        private void toolStripMenuItem3_Click(object sender, EventArgs e)
        {
            // BACKGROUND COLOR DIALOG - CONTEXT MENU
            ColorDialog dlg = new ColorDialog();
            dlg.Color = graphicsPanel1.BackColor;

            if (DialogResult.OK == dlg.ShowDialog())
            {
                graphicsPanel1.BackColor = dlg.Color;

                graphicsPanel1.Invalidate();
            }
        }

        private void cellColorToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            // CELL COLOR DIALOG - CONTEXT MENU
            ColorDialog dlg = new ColorDialog();
            dlg.Color = cellColor;

            if (DialogResult.OK == dlg.ShowDialog())
            {
                cellColor = dlg.Color;

                graphicsPanel1.Invalidate();
            }
        }

        private void toolStripMenuItem2_Click(object sender, EventArgs e)
        {
            // toggle grid visibility
            graphicsPanel1.Invalidate();
        }

        private void fromSeedToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // from seed method
            Form4 f = new Form4();
            f.Seed = seed;
            if (DialogResult.OK == f.ShowDialog())
            {
                seed = (int)f.Seed;
                RandSeed(seed);
            }
            graphicsPanel1.Invalidate();
        }

        private void fromTimeToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            // RANDOMIZE FROM TIME
            int num = ran.Next();
            seed = num;
            RandSeed(seed);
            graphicsPanel1.Invalidate();
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            // get current settings at shutdown
            Properties.Settings.Default.PanelColor = graphicsPanel1.BackColor;
            Properties.Settings.Default.GridColor = gridColor;
            Properties.Settings.Default.CellColor = cellColor;
            Properties.Settings.Default.UniverseWidth = width;
            Properties.Settings.Default.UniverseHeight = height;
            Properties.Settings.Default.Milliseconds = timer.Interval;

            // save 
            Properties.Settings.Default.Save();
        }

        private void fromCurrentSeedToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // from current seed
            RandSeed(seed);
            graphicsPanel1.Invalidate();
        }

        private void RandInt(Random val)
        {
            //from seed 
            for (int y = 0; y < universe.GetLength(1); y++)
            {
                for (int x = 0; x < universe.GetLength(0); x++)
                {
                    int num = val.Next(0, 3);
                    if (num == 0)
                    {
                        universe[x, y] = true;
                    }
                    else if (num == 1 || num == 2)
                    {
                        universe[x, y] = false;
                    }
                }
                val.NextDouble();
            }
            graphicsPanel1.Invalidate();
        }

        private void RandSeed(int val2)
        {
            Random inp = new Random(val2);
            RandInt(inp);
        }

        private void optionsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // load options form - context menu
            Form2 f = new Form2();
            f.f2Timer = timer.Interval;
            f.f2Width = width;
            f.f2Height = height;
            if (DialogResult.OK == f.ShowDialog())
            {
                timer.Interval = f.f2Timer;
                width = f.f2Width;
                height = f.f2Height;
                universe = new bool[width, height];
                scratchpad = new bool[width, height];
            }
            graphicsPanel1.Invalidate();
        }

        private void optionsToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            // load options form
            Form2 f = new Form2();
            f.f2Timer = timer.Interval;
            f.f2Width = width;
            f.f2Height = height;
            if (DialogResult.OK == f.ShowDialog())
            {
                timer.Interval = f.f2Timer;
                width = f.f2Width;
                height = f.f2Height;
                universe = new bool[width, height];
                scratchpad = new bool[width, height];
            }
            graphicsPanel1.Invalidate();
        }

        private void runToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            // RUN
            timer.Enabled = true;
        }

        private void pauseToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // PAUSE
            timer.Enabled = false;
        }

        private void nextToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // SKIP
            NextGeneration();
        }

        private void runToToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // OPEN RUN TO
            Form3 f = new Form3();
            f.Minimum = min + 1;
            f.ToNum = generations + 1;
            if (DialogResult.OK == f.ShowDialog())
            {
                min = f.ToNum;
                timer.Enabled = true;
            }
        }
        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // SAVE FILE
            SaveAs();
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // LOAD FILE
            LoadAs();
        }

        private void saveToolStripButton_Click(object sender, EventArgs e)
        {
            // SAVE FILE
            SaveAs();
        }

        private void openToolStripButton_Click(object sender, EventArgs e)
        {
            // LOAD FILE
            LoadAs();
        }

        private void resetToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // NUKE THE PROGRAM
            for (int y = 0; y < universe.GetLength(1); y++)
            {
                for (int x = 0; x < universe.GetLength(0); x++)
                {
                    // clear graphics panel
                    universe[x,y] = false;
                    scratchpad[x, y] = false;
                }
            }
            // set all global vars to default init values
            width = 50;
            height = 50;
            timer.Interval = 100;
            graphicsPanel1.BackColor = Color.White;
            gridColor = Color.Black;
            cellColor = Color.Gray;

            // refresh graphics panel
            universe = new bool[50, 50];
            scratchpad = new bool[50, 50];

            graphicsPanel1.Invalidate(); // KILL
        }

        private void reloadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // set all global vars to last saved values
            width = Properties.Settings.Default.UniverseWidth;
            height = Properties.Settings.Default.UniverseHeight;
            timer.Interval = Properties.Settings.Default.Milliseconds;
            graphicsPanel1.BackColor = Properties.Settings.Default.PanelColor;
            gridColor = Properties.Settings.Default.GridColor;
            cellColor = Properties.Settings.Default.CellColor;
        }
    }
}
