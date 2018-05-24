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
        public IPlotController plotController;
        public PlotModel Model { get; set; }
        public LinearAxis Xaxis = new LinearAxis
        {
            Position = AxisPosition.Bottom,
            Minimum = 0,
            Maximum = 10,
            Title = "X"
        };
        public LinearAxis Yaxis = new LinearAxis
        {
            Position = AxisPosition.Left,
            Maximum = 10,
            Minimum = 0,
            Title = "Y"
        };
        public ScatterSeries User_Points = new ScatterSeries
        {
            MarkerType = MarkerType.Circle,
            MarkerSize = 5
        };
        public int AxisMaximum = 10;
        public int AxisMinimum = 0;
        public double Step = 0.1;
        public MainWindow()
        {
            this.Model = new PlotModel();
            this.Model.Axes.Add(this.Xaxis);
            this.Model.Axes.Add(this.Yaxis);
            this.Model.Series.Add(this.User_Points);
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

        private void RefreshPlot()
        {
            this.Model.Series.Clear();
            this.Model.Series.Add(this.User_Points);
            this.Model.InvalidatePlot(true);
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
            DataPoint temp_point = Axis.InverseTransform(e.Position, this.Xaxis, this.Yaxis);
            ScatterPoint new_point = new ScatterPoint(temp_point.X, temp_point.Y);
            this.User_Points.Points.Add(new_point);
            //this.RefreshPlot();
            this.Model.InvalidatePlot(true);
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

        private double[] CalculateCoefficients()
        {
            int i, j, k, K = 3;
            double s, t, M;
            var User_Points = this.User_Points.Points;
            var length = int.Parse(User_Points.Count.ToString());
            double[,] sums = new double[length, length];
            double[] b = new double[length];
            double[] a = new double[4];
            //Сортируем точки по координатам X
            for(i = 0; i < this.User_Points.Points.Count; i++)
            {
                for(j = i; j >= 1; j--)
                {
                    if(this.User_Points.Points[i].X < this.User_Points.Points[j-1].X)
                    {
                        ScatterPoint temp = User_Points[j - 1];
                        User_Points[j - 1] = User_Points[j];
                        User_Points[j] = temp;
                    }
                }
            }
            //Заполняем коэффициенты системы уравнений
            for(i = 0; i < K + 1; i++)
            {
                for(j = 0; j < K + 1; j++)
                {
                    sums[i,j] = 0;
                    for(k = 0; k < length; k++)
                    {
                        sums[i, j] += Math.Pow(User_Points[k].X, i + j);
                    }
                }
            }
            //Заполняем столбец свободных членов
            for(i = 0; i < K + 1; i++)
            {
                b[i] = 0;
                for(k = 0; k < length; k++)
                {
                    b[i] += Math.Pow(User_Points[i].X, i) * User_Points[i].Y;
                }
            }
            //Применяем метод Гаусса для приведения матрицы системы к треугольному виду
            for(k = 0; k<K+1; k++)
            {
                for(i = k+1; i < K + 1; i++)
                {
                    M = sums[i, k] / sums[k, k];
                    for(j = k; j < K + 1; j++)
                    {
                        sums[i, j] -= M * sums[k, j];
                    }
                    b[i] -= M * b[k];
                }
            }
            for(i = K; i >= 0; i--)
            {
                s = 0;
                for (j = i; j < K + 1; j++)
                    s += sums[i, j] * a[j];
                a[i] = (b[i] - s) / sums[i, i];
            }
            return a;
        }

        private Func<double, double> getEquation(double [] ratios)
        {
            Func<double, double> function = (x) => ratios[0] * Math.Pow(x, 3) + ratios[1] * Math.Pow(x, 2) + ratios[2] * x + ratios[3];
            return function;
        }

        private FunctionSeries PolynomInterpolator()
        {
            double[] ratios = this.CalculateCoefficients();
            Func<double, double> function = this.getEquation(ratios);
            FunctionSeries functionSeries = new FunctionSeries(function, this.AxisMinimum, this.AxisMaximum, this.Step);
            return functionSeries;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (this.Model.Series.Count == 0)
                return;
            var LagrangeSeries = new LineSeries()
            {
                MarkerType = MarkerType.Circle,
                MarkerSize = 1,
                MarkerStroke = OxyColors.Red
            };

            for(double i = this.AxisMinimum; i < this.AxisMaximum; i += this.Step)
            {
                var point = new DataPoint(i, this.LagrangeInterpolator(i));
                LagrangeSeries.Points.Add(point);
            }
            var PolynomSeries = this.PolynomInterpolator();
            LagrangeSeries.Smooth = true;
            this.RefreshPlot();
            this.Model.Series.Add(PolynomSeries);
            this.Model.Series.Add(LagrangeSeries);
            this.Model.InvalidatePlot(true);
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            Console.WriteLine(this.User_Points.Points.Count);
        }
    }
}
