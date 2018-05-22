using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
namespace Interpolator
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            this.Model = new PlotModel();
            this.Model.Axes.Add(this.Xaxis);
            this.Model.Axes.Add(this.Yaxis);
            var series = new LineSeries
            {
                MarkerType = MarkerType.Circle,
                MarkerSize = 1,
                MarkerStroke = OxyColors.White
            };
            series.Points.Add(new DataPoint(0, 0));
            series.Smooth = true;
            this.Model.Series.Add(series);
            this.Model.MouseDown += (s, e) =>
            {
                if (e.ChangedButton == OxyMouseButton.Left)
                {
                    this.PlotLeftClick(s, e);
                }
            };
            this.User_Points.MouseDown += (s, e) =>
            {
                if(e.ChangedButton == OxyMouseButton.Right)
                {
                    this.PlotRightClick(s, e);
    }
            };
        }

        public IPlotController plotController;
        public PlotModel Model { get; set; }
        public LinearAxis Xaxis = new LinearAxis
        {
            Position = AxisPosition.Bottom, Minimum = 0, Maximum = 10, MaximumRange = 10, IsPanEnabled = false,
            MinimumRange = -10, Title = "X"
        };
        public LinearAxis Yaxis = new LinearAxis
        {
            Position = AxisPosition.Left, Maximum = 10, Minimum = 0, MinimumRange = -10, MaximumRange = 10,
            Title = "Y"
        };
        public ScatterSeries User_Points = new ScatterSeries
        {
            MarkerType = MarkerType.Circle,
            MarkerSize = 5
        };
        public List<PointContainer> user_points_gui = new List<PointContainer>();
        public int AxisMaximum = 10;
        public int AxisMinimum = 0;

        private void CreatePointInStack(double x, double y)
        {
            PointContainer Point = new PointContainer(x, y);
            ScatterPoint point = new ScatterPoint(x, y);
            this.User_Points.Points.Add(point);
            this.user_points_gui.Add(Point);
            //PointsContainer.Children.Add(Point.Container);
            Point.button.Click += (object send, RoutedEventArgs ev) =>
            {
                this.User_Points.Points.Remove(point);
                this.user_points_gui.Remove(Point);
                PointsContainer.Children.Remove(Point.Container);
            };
        }

        private void RefreshPlot()
        {
            this.Model.Series.Clear();
            this.Model.Series.Add(this.User_Points);
            this.Model.InvalidatePlot(true);
        }

        private void addPoint_Click(object sender, RoutedEventArgs e)
        {
            double x = double.Parse(x_input.Text);
            double y = double.Parse(y_input.Text);
            this.CreatePointInStack(x, y);
        }

        private double LagrangeInterpolator(double x)
        {
            double z = 0, p1, p2;
            for (var j = 0; j < this.User_Points.Points.Count; j++)
            {
                p1 = 1; p2 = 1;
                for (var i = 0; i < this.User_Points.Points.Count; i++)
                {
                    if(i == j)
                    {
                        p1 *= 1; p2 *= 1;
                    }
                    else
                    {
                        p1 *= x - this.User_Points.Points[i].X;
                        p2 *= this.User_Points.Points[j].X - this.User_Points.Points[i].X;
                    }
                }
                z += this.User_Points.Points[j].Y * p1 / p2;
            }
            return z;
        }
        
        private bool AreClose(ScatterPoint p1, ScatterPoint p2)
        {
            double x_diff = p1.X - p2.X;
            double y_diff = p2.Y - p2.Y;
            return (Math.Abs(x_diff) < 0.1 && Math.Abs(y_diff) < 0.1) ? true : false;
        }

        private void PlotLeftClick(object sender, OxyMouseDownEventArgs e)
        {
            if (e.ChangedButton == OxyMouseButton.Left)
            {
                DataPoint temp_point = Axis.InverseTransform(e.Position, this.Xaxis, this.Yaxis);
                ScatterPoint new_point = new ScatterPoint(temp_point.X, temp_point.Y);
                this.User_Points.Points.Add(new_point);
                this.RefreshPlot();
            }
        }

        private void PlotRightClick(object sender, OxyMouseDownEventArgs e)
        {
            DataPoint temp = Axis.InverseTransform(e.Position, this.Xaxis, this.Yaxis);
            ScatterPoint selected_point = new ScatterPoint(temp.X, temp.Y);
            foreach (ScatterPoint point in this.User_Points.Points)
            {
                if (this.AreClose(point, selected_point))
                {
                    this.User_Points.Points.Remove(point);
                    this.RefreshPlot();
                    this.Model.InvalidatePlot(true);
                    break;
                }
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var series = new LineSeries()
            {
                MarkerType = MarkerType.Circle,
                MarkerSize = 1,
                MarkerStroke = OxyColors.Red
            };

            for(double i = this.AxisMinimum; i < this.AxisMaximum; i += 0.5)
            {
                var point = new DataPoint(i, this.LagrangeInterpolator(i));
                series.Points.Add(point);
            }

            series.Smooth = true;
            this.RefreshPlot();
            this.Model.Series.Add(series);
            this.Model.InvalidatePlot(true);
            plot.Model = this.Model;
            plot.InvalidatePlot(true);
        }
    }
}
