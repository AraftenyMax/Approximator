using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Interpolator
{
    public class PointContainer
    {
        public Button button { get; set; }
        public TextBox x_input { get; set; }
        public TextBox y_input { get; set; }
        public Label x_label;
        public Label y_label;
        public WrapPanel Container { get; set; }

        public PointContainer(double x, double y)
        {
            this.createElements(x, y);
            this.fillContainer();
        }

        public void createElements(double x, double y)
        {
            this.button = new Button() { Content = "Delete" };
            this.x_input = new TextBox() { Text = x.ToString() };
            this.y_input = new TextBox() { Text = y.ToString() };
            this.x_label = new Label() { Content = "X:" };
            this.y_label = new Label() { Content = "Y:" };
            this.Container = new WrapPanel();
        }

        public void fillContainer()
        {
            this.Container.Width = 140;
            this.x_input.Width = 20;
            this.y_input.Width = 20;
            this.x_label.Width = 20;
            this.y_label.Width = 20;
            this.button.Width = 50;
            this.Container.Children.Add(this.x_label);
            this.Container.Children.Add(this.x_input);
            this.Container.Children.Add(this.y_label);
            this.Container.Children.Add(this.y_input);
            this.Container.Children.Add(this.button);
        }
    }
}
