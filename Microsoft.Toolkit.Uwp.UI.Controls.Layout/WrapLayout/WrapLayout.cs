﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Controls;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Microsoft.Toolkit.Uwp.UI.Controls
{
    public class WrapLayout : VirtualizingLayout
    {
        /// <summary>
        /// Gets or sets a uniform Horizontal distance (in pixels) between items when <see cref="Orientation"/> is set to Horizontal,
        /// or between columns of items when <see cref="Orientation"/> is set to Vertical.
        /// </summary>
        public double HorizontalSpacing
        {
            get { return (double)GetValue(HorizontalSpacingProperty); }
            set { SetValue(HorizontalSpacingProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="HorizontalSpacing"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty HorizontalSpacingProperty =
            DependencyProperty.Register(
                nameof(HorizontalSpacing),
                typeof(double),
                typeof(WrapLayout),
                new PropertyMetadata(0d, LayoutPropertyChanged));

        /// <summary>
        /// Gets or sets a uniform Vertical distance (in pixels) between items when <see cref="Orientation"/> is set to Vertical,
        /// or between rows of items when <see cref="Orientation"/> is set to Horizontal.
        /// </summary>
        public double VerticalSpacing
        {
            get { return (double)GetValue(VerticalSpacingProperty); }
            set { SetValue(VerticalSpacingProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="VerticalSpacing"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty VerticalSpacingProperty =
            DependencyProperty.Register(
                nameof(VerticalSpacing),
                typeof(double),
                typeof(WrapLayout),
                new PropertyMetadata(0d, LayoutPropertyChanged));

        /// <summary>
        /// Gets or sets the orientation of the WrapLayout.
        /// Horizontal means that child controls will be added horizontally until the width of the panel is reached, then a new row is added to add new child controls.
        /// Vertical means that children will be added vertically until the height of the panel is reached, then a new column is added.
        /// </summary>
        public Orientation Orientation
        {
            get { return (Orientation)GetValue(OrientationProperty); }
            set { SetValue(OrientationProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="Orientation"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty OrientationProperty =
            DependencyProperty.Register(
                nameof(Orientation),
                typeof(Orientation),
                typeof(WrapLayout),
                new PropertyMetadata(Orientation.Horizontal, LayoutPropertyChanged));

        /// <summary>
        /// Gets or sets the distance between the border and its child object.
        /// </summary>
        /// <returns>
        /// The dimensions of the space between the border and its child as a Thickness value.
        /// Thickness is a structure that stores dimension values using pixel measures.
        /// </returns>
        public Thickness Padding
        {
            get { return (Thickness)GetValue(PaddingProperty); }
            set { SetValue(PaddingProperty, value); }
        }

        /// <summary>
        /// Identifies the Padding dependency property.
        /// </summary>
        /// <returns>The identifier for the <see cref="Padding"/> dependency property.</returns>
        public static readonly DependencyProperty PaddingProperty =
            DependencyProperty.Register(
                nameof(Padding),
                typeof(Thickness),
                typeof(WrapLayout),
                new PropertyMetadata(default(Thickness), LayoutPropertyChanged));

        private static void LayoutPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is WrapLayout wp)
            {
                wp.InvalidateMeasure();
                wp.InvalidateArrange();
            }
        }

        protected override void InitializeForContextCore(VirtualizingLayoutContext context)
        {
            context.LayoutState = new WrapLayoutState();
            base.InitializeForContextCore(context);
        }

        protected override void UninitializeForContextCore(VirtualizingLayoutContext context)
        {
            context.LayoutState = null;
            base.UninitializeForContextCore(context);
        }

        /// <inheritdoc />
        protected override Size MeasureOverride(VirtualizingLayoutContext context, Size availableSize)
        {
            availableSize.Width = availableSize.Width - Padding.Left - Padding.Right;
            availableSize.Height = availableSize.Height - Padding.Top - Padding.Bottom;
            var totalMeasure = UvMeasure.Zero;
            var parentMeasure = new UvMeasure(Orientation, availableSize.Width, availableSize.Height);
            var spacingMeasure = new UvMeasure(Orientation, HorizontalSpacing, VerticalSpacing);
            var realizationBounds = new UvBounds(Orientation, context.RealizationRect);
            var lineMeasure = UvMeasure.Zero;

            var state = (WrapLayoutState)context.LayoutState;
            for (int i = 0; i < context.ItemCount; i++)
            {
                bool measured = false;
                WrapItem item = state.GetItemAt(i);
                if (item.Measure == null)
                {
                    UIElement child = context.GetOrCreateElementAt(i);
                    child.Measure(availableSize);
                    item.Measure = new UvMeasure(Orientation, child.DesiredSize.Width, child.DesiredSize.Height);
                    measured = true;
                }

                UvMeasure currentMeasure = item.Measure.Value;
                if (currentMeasure.U == 0)
                {
                    continue; // ignore collapsed items
                }

                // if this is the first item, do not add spacing. Spacing is added to the "left"
                double uChange = lineMeasure.U == 0
                    ? currentMeasure.U
                    : currentMeasure.U + spacingMeasure.U;
                if (parentMeasure.U >= uChange + lineMeasure.U)
                {
                    lineMeasure.U += uChange;
                    lineMeasure.V = Math.Max(lineMeasure.V, currentMeasure.V);
                }
                else
                {
                    // new line should be added
                    // to get the max U to provide it correctly to ui width ex: ---| or -----|
                    totalMeasure.U = Math.Max(lineMeasure.U, totalMeasure.U);
                    totalMeasure.V += lineMeasure.V + spacingMeasure.V;

                    // if the next new row still can handle more controls
                    if (parentMeasure.U > currentMeasure.U)
                    {
                        // set lineMeasure initial values to the currentMeasure to be calculated later on the new loop
                        lineMeasure = currentMeasure;
                    }

                    // the control will take one row alone
                    else
                    {
                        // validate the new control measures
                        totalMeasure.U = Math.Max(currentMeasure.U, totalMeasure.U);
                        totalMeasure.V += currentMeasure.V;

                        // add new empty line
                        lineMeasure = UvMeasure.Zero;
                    }
                }

                if (totalMeasure.V > realizationBounds.VMax)
                {
                    // Item is "below" the bounds.
                    break;
                }

                double vEnd = totalMeasure.V + currentMeasure.V;
                if (vEnd < realizationBounds.VMin)
                {
                    // Item is "above" the bounds
                }
                else if (measured == false)
                {
                    // Always measure elements that are within the bounds
                    UIElement child = context.GetOrCreateElementAt(i);
                    child.Measure(availableSize);
                }
            }

            // update value with the last line
            // if the the last loop is(parentMeasure.U > currentMeasure.U + lineMeasure.U) the total isn't calculated then calculate it
            // if the last loop is (parentMeasure.U > currentMeasure.U) the currentMeasure isn't added to the total so add it here
            // for the last condition it is zeros so adding it will make no difference
            // this way is faster than an if condition in every loop for checking the last item
            totalMeasure.U = Math.Max(lineMeasure.U, totalMeasure.U);
            totalMeasure.V += lineMeasure.V;

            totalMeasure.U = Math.Ceiling(totalMeasure.U);

            return Orientation == Orientation.Horizontal ? new Size(totalMeasure.U, totalMeasure.V) : new Size(totalMeasure.V, totalMeasure.U);
        }

        /// <inheritdoc />
        protected override Size ArrangeOverride(VirtualizingLayoutContext context, Size finalSize)
        {
            if (context.ItemCount > 0)
            {
                var parentMeasure = new UvMeasure(Orientation, finalSize.Width, finalSize.Height);
                var spacingMeasure = new UvMeasure(Orientation, HorizontalSpacing, VerticalSpacing);
                var paddingStart = new UvMeasure(Orientation, Padding.Left, Padding.Top);
                var paddingEnd = new UvMeasure(Orientation, Padding.Right, Padding.Bottom);
                var position = new UvMeasure(Orientation, Padding.Left, Padding.Top);
                var realizationBounds = new UvBounds(Orientation, context.RealizationRect);

                double currentV = 0;
                var state = (WrapLayoutState)context.LayoutState;
                bool arrange(WrapItem item, bool isLast = false)
                {
                    var desiredMeasure = item.Measure.Value;
                    if (desiredMeasure.U == 0)
                    {
                        return true; // if an item is collapsed, avoid adding the spacing
                    }

                    if ((desiredMeasure.U + position.U + paddingEnd.U) > parentMeasure.U)
                    {
                        // next row!
                        position.U = paddingStart.U;
                        position.V += currentV + spacingMeasure.V;
                        currentV = 0;
                    }

                    // Stretch the last item to fill the available space
                    if (isLast)
                    {
                        desiredMeasure.U = parentMeasure.U - position.U;
                    }

                    if (((position.V + desiredMeasure.V) >= realizationBounds.VMin) && (position.V <= realizationBounds.VMax))
                    {
                        // place the item
                        UIElement child = context.GetOrCreateElementAt(item.Index);
                        if (Orientation == Orientation.Horizontal)
                        {
                            child.Arrange(new Rect(position.U, position.V, desiredMeasure.U, desiredMeasure.V));
                        }
                        else
                        {
                            child.Arrange(new Rect(position.V, position.U, desiredMeasure.V, desiredMeasure.U));
                        }
                    }
                    else if (position.V > realizationBounds.VMax)
                    {
                        return false;
                    }

                    // adjust the location for the next items
                    position.U += desiredMeasure.U + spacingMeasure.U;
                    currentV = Math.Max(desiredMeasure.V, currentV);

                    return true;
                }

                for (var i = 0; i < context.ItemCount; i++)
                {
                    bool continueArranging = arrange(state.GetItemAt(i));
                    if (continueArranging == false)
                    {
                        break;
                    }
                }
            }

            return finalSize;
        }
    }
}