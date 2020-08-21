using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace GameOfLife
{
    public partial class Form1 : Form
    {
        // INIT NON-ESSENTIALS
        //bool states = false;
        string bounds = "Finite";
        int width = 50;
        int height = 50;

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
            InitializeComponent();

            // Setup the timer
            timer.Interval = 100; // milliseconds
            timer.Tick += Timer_Tick;

            timer.Enabled = false; // start timer running

            /*if (states == true)
            {
                timer.Enabled = true; // start timer running
            }
            else if (states == false)
            {
                timer.Enabled = false;
            }*/
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
                    } else if (toolStripFinite.Checked == true)
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

            if (gridColorToolStripMenuItem.Checked == true)
            {
                gridPen = new Pen(gridColor, 1);
            } else if (gridColorToolStripMenuItem.Checked == false)
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
            } else if (toolStripToroidal.Checked == true)
            {
                bounds = "Toroidal";
            }

            if (toolStripMenuItem1.Checked == true)
            {
                e.Graphics.DrawString("Generations: " + generations + "\nCell Count: " + isAlive + "\nBoundary Type: " + bounds + "\nUniverse Size: {Width = " + width + ", Height = " + height + "}", font, hudBrush, rect, stringFormat);
            } else if (toolStripMenuItem1.Checked == false)
            {
                e.Graphics.DrawString("Generations: " + generations + "\nCell Count: " + isAlive + "\nBoundary Type: " + bounds + "\nUniverse Size: {Width = " + width + ", Height = " + height + "}", font, Brushes.Transparent, rect, stringFormat);
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

        private void menuStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {

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
                writer.WriteLine("!This is my comment.");

                // Iterate through the universe one row at a time.
                for (int y = 0; y < /*universe.Height*/ 50; y++)
                {
                    // Create a string to represent the current row.
                    String currentRow = string.Empty;

                    // Iterate through the current row one cell at a time.
                    for (int x = 0; x < /*universe.Width*/ 50; x++)
                    {
                        if (universe[x, y] == true)
                        {
                            // If the universe[x,y] is alive then append 'O' (capital O)
                            // to the row string.

                        }
                        else if (universe[x, y] == false)
                        {
                            // Else if the universe[x,y] is dead then append '.' (period)
                            // to the row string.

                        }
                    }
                    // Once the current row has been read through and the 
                    // string constructed then write it to the file using WriteLine.
                    Console.WriteLine(currentRow);
                }

                // After all rows and columns have been written then close the file.
                writer.Close();
            }
        }

        /*private void Load()
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

                    // If the row is not a comment then it is a row of cells.
                    // Increment the maxHeight variable for each row read.

                    // Get the length of the current row string
                    // and adjust the maxWidth variable if necessary.
                }

                // Resize the current universe and scratchPad
                // to the width and height of the file calculated above.

                // Reset the file pointer back to the beginning of the file.
                reader.BaseStream.Seek(0, SeekOrigin.Begin);

                // Iterate through the file again, this time reading in the cells.
                while (!reader.EndOfStream)
                {
                    // Read one row at a time.
                    string row = reader.ReadLine();

                    // If the row begins with '!' then
                    // it is a comment and should be ignored.

                    // If the row is not a comment then 
                    // it is a row of cells and needs to be iterated through.
                    for (int xPos = 0; xPos < row.Length; xPos++)
                    {
                        // If row[xPos] is a 'O' (capital O) then
                        // set the corresponding cell in the universe to alive.

                        // If row[xPos] is a '.' (period) then
                        // set the corresponding cell in the universe to dead.
                    }
                }

                // Close the file.
                reader.Close();
            }
        }*/

        // MENUS

        private void settingsToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

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
            this.Close();
        }

        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            graphicsPanel1.Invalidate();
        }

        private void toolsToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void gridColorToolStripMenuItem_Click(object sender, EventArgs e)
        {
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
            ColorDialog dlg = new ColorDialog();
            dlg.Color = cellColor;

            if (DialogResult.OK == dlg.ShowDialog())
            {
                cellColor = dlg.Color;

                graphicsPanel1.Invalidate();
            }
        }

        private void fromTimeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            for (int y = 0; y < universe.GetLength(1); y++)
            {
                for (int x = 0; x < universe.GetLength(0); x++)
                {
                    int var = ran.Next(0,2);
                    if (var == 0)
                    {
                        universe[x, y] = true;
                    } else if (var == 1 || var == 2)
                    {
                        universe[x, y] = false;
                    }
                }
            }
            graphicsPanel1.Invalidate();
        }       
    }
}
