using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace CarboLifeUI.UI
{
    public class CarboPieSeries : System.Windows.Controls.DataVisualization.Charting.PieSeries
    {
        protected override DataPoint CreateDataPoint()
        {
            return new PieDataPoint();
        }
    }

    public class PieDataPoint : System.Windows.Controls.DataVisualization.Charting.PieDataPoint
    {
        public static readonly DependencyProperty TextedGeometryProperty =
            DependencyProperty.Register("TextedGeometry", typeof(Geometry), typeof(PieDataPoint));

        public Geometry TextedGeometry
        {
            get { return (Geometry)GetValue(TextedGeometryProperty); }
            set { SetValue(TextedGeometryProperty, value); }
        }

        static PieDataPoint()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(PieDataPoint),
                new FrameworkPropertyMetadata(typeof(PieDataPoint)));
        }

        public PieDataPoint()
        {
            DependencyPropertyDescriptor dependencyPropertyDescriptor
                = DependencyPropertyDescriptor.FromProperty(GeometryProperty, GetType());

            dependencyPropertyDescriptor.AddValueChanged(this, OnGeometryValueChanged);
        }

        private double LabelFontSize
        {
            get
            {
                FrameworkElement parentFrameworkElement = Parent as FrameworkElement;
                return Math.Max(8, Math.Min(parentFrameworkElement.ActualWidth,
                    parentFrameworkElement.ActualHeight) / 30);
            }
        }

        private void OnGeometryValueChanged(object sender, EventArgs arg)
        {
            Point point;
            FormattedText formattedText;

            CombinedGeometry combinedGeometry = new CombinedGeometry();
            combinedGeometry.GeometryCombineMode = GeometryCombineMode.Exclude;

            formattedText = new FormattedText(FormattedRatio,
                CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                new Typeface("Arial"),
                LabelFontSize,
                Brushes.White);

            if (ActualRatio == 1)
            {
                EllipseGeometry ellipseGeometry = Geometry as EllipseGeometry;

                point = new Point(ellipseGeometry.Center.X - formattedText.Width / 2,
                    ellipseGeometry.Center.Y - formattedText.Height / 2);
            }
            else if (ActualRatio == 0)
            {
                TextedGeometry = null;
                return;
            }
            else
            {
                Point tangent;
                Point half;
                Point origin;

                PathGeometry pathGeometry = Geometry as PathGeometry;
                pathGeometry.GetPointAtFractionLength(.5, out half, out tangent);
                pathGeometry.GetPointAtFractionLength(0, out origin, out tangent);

                point = new Point(origin.X + ((half.X - origin.X) / 2) - formattedText.Width / 2,
                    origin.Y + ((half.Y - origin.Y) / 2) - formattedText.Height / 2);

            }

            combinedGeometry.Geometry1 = Geometry;
            combinedGeometry.Geometry2 = formattedText.BuildGeometry(point);

            TextedGeometry = combinedGeometry;
        }
    }
}
