using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml.Controls;

namespace Microsoft.Toolkit.Uwp.UI.Controls.Primitives
{
    public class TabPanel : Panel
    { 
        protected override Size MeasureOverride(Size availableSize)
        {
            double height = 0;
            Size headerSize = new Size(availableSize.Width / Children.Count, availableSize.Height);
            foreach (var child in Children)
            {
                child.Measure(headerSize);
                height = Math.Max(height, child.DesiredSize.Height);
            }

            return new Size(availableSize.Width, height);
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            double x = 0;
            foreach (var child in Children)
            {
                child.Arrange(new Rect(x, 0, child.DesiredSize.Width, child.DesiredSize.Height));
                x += child.DesiredSize.Width;
            }

            return finalSize;
        }
    }
}
