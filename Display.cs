using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

namespace NetworkBalancer
{
    public partial class Display : Form
    {

        List<Point> NodeCoordinates;
        int radius;
        double Theta;

        int iteration;
        bool isBalanced;
        AdjacencyMatrix AdjMat;
        List<List<int>> FaceList;
        List<double[]> NodeVectors;
        int MatSize = 20;
        Random rand;
        bool updateFaces;
        int NodeAttributes = 30;
   
        public Display()
        {
            //initialize variables
            iteration = 0;
            rand = new Random();
            isBalanced = false;
            NodeVectors = new List<double[]>();
            FaceList = new List<List<int>>();
            AdjMat = new AdjacencyMatrix(MatSize, NodeVectors);
            Theta = 2 * Math.PI / MatSize;
            radius = 300;
            updateFaces = true;

            //Randomly fill NodeVectors
            for(int i = 0; i < MatSize; i++)
            {
                double[] temp = new double[NodeAttributes];
                for(int j = 0; j < NodeAttributes; j++)
                {
                    temp[j] = 2 * rand.NextDouble()- 1;
                }
                NodeVectors.Add(temp);
            }

            //Fill the Adjacency Array
            for (int i = 0; i < MatSize; i++)
            {
                for (int j = 0; j < MatSize; j++)
                {
                    if (j != i)
                    {
                        double k = Dot(NodeVectors[i], NodeVectors[j]);
                        AdjMat.SetValue(i, j, k);
                    }
                }
            }

            //InitializeComponent();
            this.ClientSize = new Size(600, 600);

            this.Paint += new PaintEventHandler(Display_Load);
        }

        //Dot product function cuz I couldn't find one
        public double Dot(double[] a, double[] b)
        {
            double sum = 0;
            for (int i = 0; i < a.Length; i++)
            {
                sum += a[i] * b[i];
            }
            return sum;
        }

        //Fills FaceList with triplets of nodes that are all connected
        public void GetFaces()
        {
            FaceList.Clear();
            for (int i = 0; i < MatSize; i++)
            {
                for (int j = i + 1; j < MatSize; j++)
                {
                    if (AdjMat.GetValue(i, j) != 0)
                    {
                        for (int k = j + 1; k < MatSize; k++)
                        {
                            if (AdjMat.GetValue(i, k) != 0 && AdjMat.GetValue(j, k) != 0)
                            {
                                FaceList.Add(new List<int> { i, j, k });
                            }

                        }
                    }
                }
            }
        }

        //check if every set of nodes is in a balanced state
        private void iterate()
        {
            if (updateFaces) { GetFaces(); updateFaces = false; }
            foreach (List<int> face in FaceList)
            {
                int i = face[0];
                int j = face[1];
                int k = face[2];

                //updates each component of NodeAttributes individually
                for (int z = 0; z < NodeAttributes; z++)
                {
                    //I think this is a good function for what I'm trying to do
                    NodeVectors[i][z] += -NodeVectors[j][z] * NodeVectors[k][z] * Math.Sin(NodeVectors[i][z] / Math.PI);
                    NodeVectors[j][z] += -NodeVectors[i][z] * NodeVectors[k][z] * Math.Sin(NodeVectors[j][z] / Math.PI);
                    NodeVectors[k][z] += -NodeVectors[i][z] * NodeVectors[j][z] * Math.Sin(NodeVectors[k][z] / Math.PI);


                    //manually clamping values of each component in (-1, 1)
                    NodeVectors[i][z] = (NodeVectors[i][z] < -1) ? -1 : (NodeVectors[i][z] > 1) ? 1 : NodeVectors[i][z];
                    NodeVectors[j][z] = (NodeVectors[j][z] < -1) ? -1 : (NodeVectors[j][z] > 1) ? 1 : NodeVectors[j][z];
                    NodeVectors[k][z] = (NodeVectors[k][z] < -1) ? -1 : (NodeVectors[k][z] > 1) ? 1 : NodeVectors[k][z];
                }
        
                //Finding dot products
                double val1 = Dot(NodeVectors[i], NodeVectors[j]);
                double val2 = Dot(NodeVectors[j], NodeVectors[k]);
                double val3 = Dot(NodeVectors[i], NodeVectors[k]);

                //If a value is low enough, set it to zero and make sure to update FaceList
                double threshold = 0.05;
                if (Math.Abs(val1) < threshold) { val1 = 0; updateFaces = true; }
                if (Math.Abs(val2) < threshold) { val2 = 0; updateFaces = true; }
                if (Math.Abs(val3) < threshold) { val3 = 0; updateFaces = true; }
                
                if (Double.IsNaN(val1)) Console.WriteLine(val1 + " " + val2 + " " + val3);

                //clamping the values
                val1 = (val1 < -1) ? -1 : (val1 > 1) ? 1 : val1;
                val2 = (val2 < -1) ? -1 : (val2 > 1) ? 1 : val2;
                val3 = (val3 < -1) ? -1 : (val3 > 1) ? 1 : val3;

                //sending the new info into the matrix
                AdjMat.SetValue(i, j, val1);
                AdjMat.SetValue(i, k, val2);
                AdjMat.SetValue(j, k, val3);

            }
        }

        //Draws blue circles for Nodes
        private void DrawNode(int x, int y)
        {
            System.Drawing.Pen myPen = new System.Drawing.Pen(System.Drawing.Color.Blue);
            System.Drawing.Graphics formGraphics;
            formGraphics = this.CreateGraphics();
            formGraphics.DrawEllipse(myPen, new Rectangle(x-5, y-5, 7, 7));
            myPen.Dispose();
            formGraphics.Dispose();
        }

        //Draws lines which are red for negative, green for positive, and opacity proportional to magnitudes
        private void DrawEdge(int node1, int node2, List<Point> NodeList)
        {
            Pen pen;
            double v = AdjMat.GetValue(node1, node2);
            //Console.WriteLine(v);
            if (v > 0)
            {
                pen = new Pen(Color.FromArgb((int) Math.Abs(130 * v + 125), Color.Green));
            }
            else if(AdjMat.GetValue(node1, node2) < 0)
            {
                pen = new Pen(Color.FromArgb((int)Math.Abs(130 * v + 125), Color.Red));
            }
            else
            {
                return;
            }

            Graphics g  = this.CreateGraphics();
            g.DrawLine(pen, NodeList[node1], NodeList[node2]);
        }

        //Where it actually draws the graph
        private void Display_Load(object sender, PaintEventArgs e)
        {
            this.GetFaces();
            if (isBalanced)
            {
                this.Text = "True ";
            }
            else
            {
                this.Text = "False ";
            }
            this.Text += iteration;
            AdjMat.checkIsBalanced(FaceList, ref isBalanced);    
            iterate();

            iteration++;

            Graphics g = e.Graphics;

            NodeCoordinates = new List<Point>();

            Console.WriteLine(iteration);
            if (iteration % 20 == 1)
            {
                DrawNodes();
                DrawEdges();
                Thread.Sleep(100);
            }


            this.Refresh();

            if (isBalanced)
            {
                Thread.Sleep(1000);
                this.Close();
                System.Environment.Exit(0);
            }

        }

        private void Display_Load(object sender, EventArgs e)
        {

        }

        private void DrawNodes()
        {
            for (int i = 0; i < MatSize; i++)
            {
                int x_coord = (int)(radius * Math.Cos(Theta * i));
                int y_coord = (int)(radius * Math.Sin(Theta * i));

                NodeCoordinates.Add(new Point(x_coord + ClientRectangle.Width / 2, y_coord + ClientRectangle.Height / 2));

                this.DrawNode(x_coord + ClientRectangle.Width / 2, y_coord + ClientRectangle.Height / 2);
            }
        }

        
        private void DrawEdges()
        {
            for (int i = 0; i < MatSize; i++)
            {
                for (int j = i + 1; j < MatSize; j++)
                {
                    this.DrawEdge(i, j, NodeCoordinates);
                }
            }
        }
    }

    internal class AdjacencyMatrix
    {
        public AdjacencyMatrix(int MatSize, List<double[]> NodeVectors)
        {
            size = MatSize;
            if (size % 2 == 1) size--;
            Mat = new double[size, size / 2];
            int x;
            int y;
            Random rand = new Random();

            for(int i = 0; i < size; i++)
            {
                for(int j = 0; j < size / 2; j++)
                {
                    if(j != i)
                    {
                        if(j < size / 2)
                        {
                            x = i;
                            y = j;
                        }
                        else
                        {
                            x = size - i - 1;
                            y = size - j - 1;
                        }
                        if (rand.NextDouble() < 0.05) Mat[x, y] = 0;
                        else Mat[x, y] = 2 * rand.NextDouble() - 1;
                    }
                }
            }

        }

        public double GetValue(int i, int j)
        {
            int x;
            int y;
            if(j < size / 2)
            {
                x = i;
                y = j;
            }
            else
            {
                x = size - i - 1;
                y = size - j - 1;
            }
            
            return Mat[x, y];
        }

        public void SetValue(int i, int j, double newVal)
        {
            int x;
            int y;
            if (j < size / 2)
            {
                x = i;
                y = j;
            }
            else
            {
                x = size - i - 1;
                y = size - j - 1;
            }
            Mat[x, y] = newVal;
        }

        
        public void checkIsBalanced(List<List<int>> FaceList, ref bool isBalanced)
        {
            foreach (List<int> face in FaceList)
            {
                int i = face[0];
                int j = face[1];
                int k = face[2];
                double val1 = this.GetValue(i, j);
                double val2 = this.GetValue(i, k);
                double val3 = this.GetValue(j, k);
                double prod = val1 * val2 * val3;

                if (prod < 0)
                {
                    isBalanced = false;
                    return;
                }

            }
            isBalanced = true;
            return;
        }

        private int size;
        double[,] Mat;
    }
}


