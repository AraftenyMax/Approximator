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
        PlotLogic Logic = new PlotLogic();
        public PlotModel Model { get; set; }
        public ScatterSeries Series { get; set; }
        public MainWindow()
        {
            Model = Logic.getModel();
            Series = Logic.getPoints();
            Model.MouseDown += (s, e) =>
            {
                if (e.ChangedButton == OxyMouseButton.Left)
                {
                    this.PlotLeftClick(s, e);
                }
            };
            Series.MouseDown += (s, e) =>
            {
                if(e.ChangedButton == OxyMouseButton.Right)
                {
                    this.PlotRightClick(s, e);
                }
            };
        }

        private void PlotLeftClick(object sender, OxyMouseDownEventArgs e)
        {
            this.Logic.addPoint(e.Position);
            this.Model.InvalidatePlot(true);
        }

        private void PlotRightClick(object sender, OxyMouseDownEventArgs e)
        {
            this.Logic.deletePoint(e.Position);
            this.Model.InvalidatePlot(true);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Logic.BuildPlot();
            this.Model.InvalidatePlot(true);
        }
    }
}
