using System;
using System.Drawing;
using System.Windows.Forms;

namespace GameOfLife
{
    public partial class Form1 : Form
    {
        // INIT NON-ESSENTIALS
        bool states = false;

        // The universe array
        bool[,] universe = new bool[50, 50];
        bool[,] scratchpad = new bool[50, 50];

        // Drawing colors
        Color gridColor = Color.Black;
        Color cellColor = Color.Gray;

        // The Timer class
        Timer timer = new Timer();

        // Generation count
        int generations = 0;

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
            for (int y = 0; y < universe.GetLength(1); y++)
            {
                for (int x = 0; x < universe.GetLength(0); x++)
                {
                    int count = CountNeighborsFinite(x, y); //update neighbor coords

                    if (universe[x, y] == true){
                        //Living cells with less than 2 living neighbors die in the next generation.
                        //Living cells with more than 3 living neighbors die in the next generation.
                        if (count < 2 || count > 3) { scratchpad[x, y] = false; }

                        //Living cells with 2 or 3 living neighbors live in the next generation.
                        if (count == 2 || count == 3) { scratchpad[x, y] = true; }

                    } else if (universe[x,y] == false)
                    {
                        //Dead cells with exactly 3 living neighbors live in the next generation.
                        if (count == 3) { scratchpad[x, y] = true; }
                    }
                }
            }

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

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            //START BUTTON
            states = false;
            timer.Enabled = true;
        }

        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            //PAUSE BUTTON
            states = true;
            timer.Enabled = false;
        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            //SKIP BUTTON
            NextGeneration();
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

                    if (universe[xCheck, yCheck] == true) count++;

                }

            }


            return count;

        }
    }
}
