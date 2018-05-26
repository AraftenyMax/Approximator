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
using MathNet.Numerics;
using OxyPlot.Series;

namespace Interpolator
{

    public static class PointsContainer
    {
        public static ScatterSeries SelectedPoints = new ScatterSeries
        {
            Title = "User Points",
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
            Title = "Lagrange Series",
            MarkerType = MarkerType.Circle,
            MarkerSize = 0.5
        };
        public static FunctionSeries PolynomSeries = new FunctionSeries
        {
            Title = "Polynom Series",
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
                Model.LegendTitle = "Legend";
                Model.LegendPosition = LegendPosition.BottomRight;
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
            int quantity = PointsContainer.PlotPoints.Points.Count;
            double[] x = new double[quantity];
            double[] y = new double[quantity];
            int counter = 0;
            foreach(DataPoint Point in PointsContainer.PlotPoints.Points)
            {
                x[counter] = Point.X;
                y[counter] = Point.Y;
                counter++;
            }
            IInterpolation cubicSpline = Interpolate.CubicSpline(x, y);
            for (double i = this.AxisMinimum; i < this.AxisMaximum; i += this.Step)
                PointsContainer.PolynomSeries.Points.Add(new DataPoint(i, cubicSpline.Interpolate(i)));
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
