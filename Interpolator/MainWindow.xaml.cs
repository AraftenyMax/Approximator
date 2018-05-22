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
            this.Model = new PlotModel { Title = "Plot" };
            this.Model.Axes.Add(this.Xaxis);
            this.Model.Axes.Add(this.Yaxis);
            this.Model.MouseDown += (s, e) =>
            {
                if (e.ChangedButton == OxyMouseButton.Left)
                {
                    this.PlotLeftClick(s, e);
                }
            };
            var series = new LineSeries
            {
                MarkerType = MarkerType.Circle,
                MarkerSize = 1,
                MarkerStroke = OxyColors.White
            };
            series.Points.Add(new DataPoint(0, 0));
            series.Smooth = true;
            this.Model.Series.Add(series);
        }
        public IPlotController plotController;
        public PlotModel Model { get; set; }
        public LinearAxis Xaxis = new LinearAxis
        {
            Position = AxisPosition.Bottom, Maximum = 10, AbsoluteMaximum = 10, AbsoluteMinimum = -10, Title = "X", IsZoomEnabled = false
        };
        public LinearAxis Yaxis = new LinearAxis
        {
            Position = AxisPosition.Top, Maximum = 10, AbsoluteMinimum = -10, AbsoluteMaximum = 10, Title = "Y", IsZoomEnabled = false
        };
        public Dictionary<double, double> Points = new Dictionary<double, double>();
        public ScatterSeries User_Points = new ScatterSeries
        {
            MarkerType = MarkerType.Circle,
            MarkerSize = 5
        };
        public List<PointContainer> user_points_gui = new List<PointContainer>();
        public int AxisMaximum = 10;
        public int AxisMinimum = 0;

        private void addPoint_Click(object sender, RoutedEventArgs e)
        {
            double x = double.Parse(x_input.Text);
            double y = double.Parse(y_input.Text);
            this.Points.Add(x, y);
            PointContainer Point = new PointContainer(x, y);
            this.user_points_gui.Add(Point);
            PointsContainer.Children.Add(Point.Container);
            Point.button.Click += (object send, RoutedEventArgs ev) =>
            {
                this.Points.Remove(x);
                this.user_points_gui.Remove(Point);
                PointsContainer.Children.Remove(Point.Container);
            };
        }

        private double LagrangeInterpolator(double x)
        {
            double z = 0, p1, p2;
            for (var j = 0; j < this.Points.Count; j++)
            {
                p1 = 1; p2 = 1;
                for (var i = 0; i < this.Points.Count; i++)
                {
                    if(i == j)
                    {
                        p1 *= 1; p2 *= 1;
                    }
                    else
                    {
                        p1 *= x - this.Points.Keys.ElementAt(i);
                        p2 *= this.Points.Keys.ElementAt(j) - this.Points.Keys.ElementAt(i);
                    }
                }
                z += this.Points.Values.ElementAt(j) * p1 / p2;
            }
            return z;
        }

        private void PlotLeftClick(object sender, OxyMouseDownEventArgs e)
        {
            CursorPoint.Content = String.Format("{0}, {1}", e.Position.X.ToString(), e.Position.Y.ToString());
            if (e.ChangedButton == OxyMouseButton.Left)
            {
                foreach (ScatterPoint point in this.User_Points.Points)
                {
                    if (point.X == e.Position.X && point.Y == e.Position.Y)
                    {
                        return;
                    }
                }
                DataPoint temp_point = Axis.InverseTransform(new ScreenPoint(e.Position.X, e.Position.Y), this.Xaxis, this.Yaxis);
                ScatterPoint new_point = new ScatterPoint(temp_point.X, temp_point.Y);
                this.User_Points.Points.Add(new_point);
                this.Model.Series.Clear();
                this.Model.Series.Add(this.User_Points);
                this.Model.InvalidatePlot(true);
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var series = new LineSeries()
            {
                MarkerType = MarkerType.Circle,
                MarkerSize = 3,
                MarkerStroke = OxyColors.Red
            };

            for(double i = this.AxisMinimum; i < this.AxisMaximum; i += 0.5)
            {
                var point = new DataPoint(i, this.LagrangeInterpolator(i));
                series.Points.Add(point);
            }

            series.Smooth = true;
            this.Model.Series.Clear();
            this.Model.Series.Add(series);
            this.Model.InvalidatePlot(true);
            plot.Model = this.Model;
            plot.InvalidatePlot(true);
        }
    }
}
