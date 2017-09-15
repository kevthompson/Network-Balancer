using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Threading;

using System.Windows.Forms;

namespace NetworkBalancer
{
    class NetworkBalancer
    {
        static void Main(string[] args)
        {
            ApplicationException.run(display = new Display());
        }
    }

    public partial class Display : Form
    {
        private void DrawNode(int x, int y)
        {
            System.Drawing.Pen myPen = new System.Drawing.Pen(Color.Blue);
            System.Drawing.Graphics formGraphics;
            formGraphics = this.CreateGraphics();
            formGraphics.DrawEllipse(myPen, new Rectangle(x, y, 2, 2));
            myPen.Dispose();
            formGraphics.Dispose();


        }

        private void DrawEdge(int node1, int node2, List<Point> NodeList)
        {
            Pen pen;
            if (AdjMat[node1, node2] > 0)
            {
                pen = new Pen(Color.Green);
            }
            else
            {
                pen = new Pen(Color.Red);
            }

            Graphics g = this.CreateGraphics();
            g.DrawLine(pen, NodeList[node1], NodeList[node2]);


        }

        public Display(double[,] Mat, bool isBal)
        {
            if (isBal)
            {
                this.Text = "True";
            }
            else
            {
                this.Text = "False";
            }
            AdjMat = Mat;
            //InitializeComponent();
            ClientSize = new Size(600, 600);

            this.Paint += new PaintEventHandler(Display_Load);
        }
        private void Display_Load(object sender, PaintEventArgs e)
        {

            Graphics g = e.Graphics;

            List<Point> NodeCoordinates = new List<Point>();

            int radius = 100;
            double Theta = 2 * Math.PI / AdjMat.GetLength(0);

            for (int i = 0; i < AdjMat.GetLength(0); i++)
            {
                int x_coord = (int)(radius * Math.Cos(Theta * i));
                int y_coord = (int)(radius * Math.Sin(Theta * i));

                NodeCoordinates.Add(new Point(x_coord + ClientRectangle.Width / 2, y_coord + ClientRectangle.Height / 2));

                this.DrawNode(x_coord + ClientRectangle.Width / 2, y_coord + ClientRectangle.Height / 2);
            }

            for (int i = 0; i < AdjMat.GetLength(0); i++)
            {
                for (int j = i + 1; j < AdjMat.GetLength(0); j++)
                {
                    this.DrawEdge(i, j, NodeCoordinates);
                }
            }

            Thread.Sleep(1000);
            this.Close();
        }
        double[,] AdjMat;

        private void Display_Load(object sender, EventArgs e)
        {

        }
    }
}
