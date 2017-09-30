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
        int MatSize = 16;
        Random rand;
   
        public Display()
        {
            //initialize variables
            iteration = 0;
            rand = new Random();
            isBalanced = false;
            FaceList = new List<List<int>>();
            AdjMat = new AdjacencyMatrix(MatSize);
            Theta = 2 * Math.PI / MatSize;
            radius = 300;

            //Randomly fill the Adjacency Array
            for (int i = 0; i < MatSize; i++)
            {
                for (int j = 0; j < MatSize; j++)
                {
                    if (j != i)
                    {
                        double k = rand.NextDouble() * 2 - 1;
                        AdjMat.SetValue(i, j, k);
                    }
                }
            }
            //List<Node> NodeList = new List<Node>();
            //List<Edge> EdgeList = new List<Edge>();

            //generateNodeList(NodeList, AdjMat);
            //generateEdgeList(EdgeList, AdjMat);
            GetFaces();

            //InitializeComponent();
            this.ClientSize = new Size(600, 600);

            this.Paint += new PaintEventHandler(Display_Load);
        }


        public void GetFaces()
        {
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
            foreach (List<int> face in FaceList)
            {
                int i = face[0];
                int j = face[1];
                int k = face[2];
                double val1 = AdjMat.GetValue(i, j);
                double val2 = AdjMat.GetValue(i, k);
                double val3 = AdjMat.GetValue(j, k);

                //Console.WriteLine(val1 + " " + val2 + " " + val3 + " " + i + " " + j + " " + k);

                val1 *= (1 - Math.Pow(val1, 2)) * val2 * val3;
                val2 *= (1 - Math.Pow(val2, 2)) * val1 * val3;
                val3 *= (1 - Math.Pow(val3, 2)) * val1 * val2;

                Console.WriteLine(val1);

                AdjMat.SetValue(i, j, val1);
                AdjMat.SetValue(i, k, val2);
                AdjMat.SetValue(j, k, val3);

            }
        }

        private void DrawNode(int x, int y)
        {
            System.Drawing.Pen myPen = new System.Drawing.Pen(System.Drawing.Color.Blue);
            System.Drawing.Graphics formGraphics;
            formGraphics = this.CreateGraphics();
            formGraphics.DrawEllipse(myPen, new Rectangle(x-5, y-5, 10, 10));
            myPen.Dispose();
            formGraphics.Dispose();
        }

        private void DrawEdge(int node1, int node2, List<Point> NodeList)
        {
            Pen pen;
            if(AdjMat.GetValue(node1, node2) > 0)
            { 
                pen = new Pen(Color.Green);
            }
            else if(AdjMat.GetValue(node1, node2) < 0)
            {
                pen = new Pen(Color.Red);
            }
            else
            {
                return;
            }

            Graphics g  = this.CreateGraphics();
            g.DrawLine(pen, NodeList[node1], NodeList[node2]);


        }

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
            iteration++;
            AdjMat.checkIsBalanced(FaceList, ref isBalanced);    
            iterate();

            Graphics g = e.Graphics;

            NodeCoordinates = new List<Point>();

            DrawNodes();
            DrawEdges();

            Thread.Sleep(1000);
            if (iteration < 10)
            {
                this.Refresh();
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
        public AdjacencyMatrix(int MatSize)
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
                        if (rand.NextDouble() < 0.1) Mat[x, y] = 0;
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


