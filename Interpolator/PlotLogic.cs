using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using OxyPlot;
using OxyPlot.Axes;
using MathNet.Numerics.Interpolation;
using OxyPlot.Series;

namespace Interpolator
{

    public static class PointsContainer
    {
        public static ScatterSeries SelectedPoints = new ScatterSeries
        {
            MarkerType = MarkerType.Circle,
            MarkerSize = 5
        };
        public static LineSeries PlotPoints = new LineSeries
        {
            MarkerType = MarkerType.Circle,
            MarkerSize = 5
        };
        public static LineSeries LagrangeSeries = new LineSeries
        {
            MarkerType = MarkerType.Circle,
            MarkerSize = 0.5
        };
        public static FunctionSeries PolynomSeries = new FunctionSeries
        {
            MarkerType = MarkerType.Circle,
            MarkerSize = 0.5
        };
    }

    public class PlotLogic
    {
        public PlotLogic()
        {
            this.AttachSeries();
        }
        private int AxisMaximum = 10;
        private int AxisMinimum = 0;
        private double Step = 0.1;
        private static PlotModel Model = new PlotModel();
        private LinearAxis Xaxis = new LinearAxis
        {
            Position = AxisPosition.Bottom,
            Minimum = 0,
            Maximum = 10,
            Title = "X"
        };
        private LinearAxis Yaxis = new LinearAxis
        {
            Position = AxisPosition.Left,
            Maximum = 10,
            Minimum = 0,
            Title = "Y"
        };

        private bool AreClose(ScatterPoint p1, ScatterPoint p2)
        {
            double x_diff = p1.X - p2.X;
            double y_diff = p2.Y - p2.Y;
            return (Math.Abs(x_diff) < 0.1 && Math.Abs(y_diff) < 0.1) ? true : false;
        }

        public PlotModel getModel()
        {
            return Model;
        }

        public ScatterSeries getPoints()
        {
            return PointsContainer.SelectedPoints;
        }

        public void AttachSeries()
        {
            if(Model.Series.Count == 0)
            {
                Model.Axes.Add(Xaxis);
                Model.Axes.Add(Yaxis);
                Model.Series.Add(PointsContainer.SelectedPoints);
                Model.Series.Add(PointsContainer.LagrangeSeries);
                Model.Series.Add(PointsContainer.PolynomSeries);
            }
        }

        public void addPoint(ScreenPoint screenPoint)
        {
            DataPoint PlotPoint = Axis.InverseTransform(screenPoint, this.Xaxis, this.Yaxis);
            if (double.IsInfinity(PlotPoint.X) || double.IsInfinity(PlotPoint.Y))
                return;
            ScatterPoint SelectedPoint = new ScatterPoint(PlotPoint.X, PlotPoint.Y);
            PointsContainer.PlotPoints.Points.Add(PlotPoint);
            PointsContainer.SelectedPoints.Points.Add(SelectedPoint);
        }

        public void deletePoint(ScreenPoint screenPoint)
        {
            DataPoint temp_point = Axis.InverseTransform(screenPoint, this.Xaxis, this.Yaxis);
            ScatterPoint point = new ScatterPoint(temp_point.X, temp_point.Y);
            foreach(ScatterPoint Point in PointsContainer.SelectedPoints.Points)
            {
                if (this.AreClose(point, Point)){
                    temp_point = new DataPoint(Point.X, Point.Y);
                    point = Point;
                    break;
                }
            }
            PointsContainer.PlotPoints.Points.Remove(temp_point);
            PointsContainer.SelectedPoints.Points.Remove(point);
        }

        private double LagrangeInterpolator(double x)
        {
            double z = 0, p1, p2;
            var User_Points = PointsContainer.PlotPoints.Points;
            for (var j = 0; j < User_Points.Count; j++)
            {
                p1 = 1; p2 = 1;
                for (var i = 0; i < User_Points.Count; i++)
                {
                    if (i == j)
                    {
                        p1 *= 1; p2 *= 1;
                    }
                    else
                    {
                        p1 *= x - User_Points[i].X;
                        p2 *= User_Points[j].X - User_Points[i].X;
                    }
                }
                z += User_Points[j].Y * p1 / p2;
            }
            return z;
        }

        private void CalculateCoefficients()
        {
            int k;
            int j = 1;
            int quantity = PointsContainer.PlotPoints.Points.Count+1;
            double[] x = new double[quantity];
            double[] y = new double[quantity];
            double[] h = new double[quantity];
            double[] l = new double[quantity];
            double[] delta = new double[quantity];
            double[] lambda = new double[quantity];
            double[] c = new double[quantity];
            double[] d = new double[quantity];
            double[] b = new double[quantity];
            foreach (DataPoint p in PointsContainer.PlotPoints.Points)
            {
                x[j] = p.X;
                y[j] = p.Y;
                j++;
            }

            for(k = 1; k <= quantity-1; k++)
            {
                h[k] = x[k] - x[k - 1];
                l[k] = (y[k] - y[k - 1]) / h[k];
            }

            delta[1] = -h[2] / (2 * (h[1] + h[2]));
            lambda[1] = 1.5 * (l[2] - l[1]) / (h[1] + h[2]);
            for(k = 3; k <= quantity-1; k++)
            {
                delta[k - 1] = -h[k] / (2 * h[k - 1] + 2 * h[k] + h[k - 1] * delta[k - 2]);
                lambda[k - 1] = (3 * l[k] - 3 * l[k - 1] - h[k - 1] * delta[k - 2]) /
                               (2 * h[k - 1] + 2 * h[k] + h[k - 1] * delta[k - 2]);
            }
            c[0] = 0;
            c[quantity-1] = 0;
            for(k = quantity-1; k >= 2; k--)
            {
                c[k - 1] = delta[k - 1] * c[k] + lambda[k - 1];
            }
            for(k = 1; k <= quantity-1; k++)
            {
                d[k] = (c[k] - c[k - 1]) / (3 * h[k]);
                b[k] = l[k] + (2 * c[k] * h[k] + h[k] * c[k - 1]) / 3;
            }
            this.ApplyRatios(y, b, c, d, x, y);
        }

        private FunctionSeries getSplinePart(double a, double b, double c, double d, double from, double to)
        {
            Func<double, double> function = (x) => a + x * b + Math.Pow(x, 2) * c + Math.Pow(x, 3) * d;
            FunctionSeries Series = new FunctionSeries(function, from, to, this.Step);
            return Series;
        }

        private void ApplyRatios(double[] a, double[] b, double[] c, double[] d, double[] x, double[] y)
        {
            for(int i = 1; i < a.Length; i++)
            {
                FunctionSeries Series = this.getSplinePart(a[i], b[i], c[i], d[i], x[i-1], x[i]);
                foreach (DataPoint Point in Series.Points)
                    PointsContainer.PolynomSeries.Points.Add(Point);
            }
        }

        private void CalculateLagrangeSeries()
        {
            PointsContainer.LagrangeSeries.Points.Clear();
            for(double i = this.AxisMinimum; i < this.AxisMaximum; i += this.Step)
            {
                DataPoint Point = new DataPoint(i, this.LagrangeInterpolator(i));
                PointsContainer.LagrangeSeries.Points.Add(Point);
            }
        }

        private void CalculatePolynomSeries()
        {
            PointsContainer.PolynomSeries.Points.Clear();
            this.CalculateCoefficients();
        }

        public void BuildPlot()
        {
            this.CalculateLagrangeSeries();
            this.CalculatePolynomSeries();
        }
    }
}
