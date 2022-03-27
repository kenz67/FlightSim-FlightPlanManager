using DataGridViewAutoFilter;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;

namespace FlightPlanManager.Controls
{
    // public class DataGridViewRatingColumn : DataGridViewAutoFilterStarRatingColumn
    public class DataGridViewRatingColumn : DataGridViewColumn
    {
        public DataGridViewRatingColumn() : base(new DataGridViewRatingCell())
        {
            base.ReadOnly = true;
            RatedStarColor = Color.Green;
            GrayStarColor = Color.LightGray;
            StarScale = 1;
        }

        public new bool ReadOnly { get; set; }

        private Color ratedStarColor;
        private Color grayStarColor;
        private float starScale;

        public Color RatedStarColor
        {
            get { return ratedStarColor; }
            set
            {
                if (ratedStarColor != value)
                {
                    ratedStarColor = value;
                    DataGridView?.InvalidateColumn(Index);
                }
            }
        }

        public Color GrayStarColor
        {
            get { return grayStarColor; }
            set
            {
                if (grayStarColor != value)
                {
                    grayStarColor = value;
                    DataGridView?.InvalidateColumn(Index);
                }
            }
        }

        public float StarScale
        {
            get { return starScale; }
            set
            {
                if (starScale != value)
                {
                    starScale = value;
                    DataGridViewRatingCell.UpdateBrushes(value);
                    DataGridView?.InvalidateColumn(Index);
                }
            }
        }
    }

    public class DataGridViewRatingCell : DataGridViewTextBoxCell
    {
        static DataGridViewRatingCell()
        {
            //Init star
            List<PointF> points = new List<PointF>();
            bool largeArc = true;
            R = 10;
            r = 4;
            center = new Point(R, R);
            for (float alpha = 90; alpha <= 414; alpha += 36)
            {
                int d = largeArc ? R : r;
                double radAlpha = alpha * Math.PI / 180;
                float x = (float)(d * Math.Cos(radAlpha));
                float y = (float)(d * Math.Sin(radAlpha));
                points.Add(new PointF(center.X + x, center.Y + y));
                largeArc = !largeArc;
            }
            star.AddPolygon(points.ToArray());
            star.Transform(new Matrix(1, 0, 0, -1, 0, center.Y * 2));
            //Init stars
            UpdateBrushes(1);
        }

        public DataGridViewRatingCell()
        {
            ValueType = typeof(int);
            ratedStarColor = Color.Green;
            grayStarColor = Color.LightGray;
            starScale = 1;
            UseColumnStarColor = true;
            UseColumnStarScale = true;
        }

        public override object DefaultNewRowValue
        {
            get
            {
                return 0;
            }
        }

        internal static void UpdateBrushes(float scale)
        {
            int space = 2 * R;
            for (int i = 0; i < 5; i++)
            {
                stars[i]?.Dispose();
                stars[i] = (GraphicsPath)star.Clone();
                stars[i].Transform(new Matrix(scale, 0, 0, scale, space * i * scale, 0));
                brushes[i] = CreateBrush(new RectangleF(center.X - R + (space * i * scale), center.Y - R, R * 2 * scale, R * 2 * scale));
            }
        }

        private static LinearGradientBrush CreateBrush(RectangleF bounds)
        {
            var brush = new LinearGradientBrush(bounds, Color.White, Color.Yellow, LinearGradientMode.ForwardDiagonal);
            ColorBlend cb = new ColorBlend();
            Color c = Color.Green;
            Color lightColor = Color.White;
            cb.Colors = new Color[] { c, c, lightColor, c, c };
            cb.Positions = new float[] { 0, 0.4f, 0.5f, 0.6f, 1 };
            brush.InterpolationColors = cb;
            return brush;
        }

        private void AdjustBrushColors(LinearGradientBrush brush, Color baseColor, Color lightColor)
        {
            //Note how we adjust the colors, using brush.InterpolationColors directly won't work.
            ColorBlend cb = brush.InterpolationColors;
            cb.Colors = new Color[] { baseColor, baseColor, lightColor, baseColor, baseColor };
            brush.InterpolationColors = cb;
        }

        private static readonly GraphicsPath star = new GraphicsPath();
        private static readonly GraphicsPath[] stars = new GraphicsPath[5];
        private static readonly LinearGradientBrush[] brushes = new LinearGradientBrush[5];
        private static Point center;
        private static readonly int R, r;
        private int currentValue = -1;
        private bool mouseOver;

        protected override void Paint(Graphics graphics, Rectangle clipBounds, Rectangle cellBounds,
            int rowIndex, DataGridViewElementStates elementState, object value, object formattedValue,
            string errorText, DataGridViewCellStyle cellStyle, DataGridViewAdvancedBorderStyle advancedBorderStyle, DataGridViewPaintParts paintParts)
        {
            base.Paint(graphics, clipBounds, cellBounds, rowIndex, elementState, value, formattedValue,
                errorText, cellStyle, advancedBorderStyle, paintParts & ~DataGridViewPaintParts.SelectionBackground & ~DataGridViewPaintParts.ContentForeground);
            //if (rowIndex == RowIndex && (paintParts & DataGridViewPaintParts.ContentForeground) != 0)
            if ((paintParts & DataGridViewPaintParts.ContentForeground) != 0)
            {
                graphics.SmoothingMode = SmoothingMode.AntiAlias;
                if (value != null) value = Math.Min(Math.Max(0, (int)value), 5);
                if (!mouseOver) currentValue = (int)(value ?? 0);
                PaintStars(graphics, cellBounds, 0, currentValue, true);
                PaintStars(graphics, cellBounds, currentValue, 5 - currentValue, false);
                graphics.SmoothingMode = SmoothingMode.Default;
            }
        }

        protected override void OnMouseMove(DataGridViewCellMouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (!mouseOver) mouseOver = true;
            if (IsReadOnly()) return;
            var lastStar = stars.Select((x, i) => new { x, i })
                                .LastOrDefault(x => x.x.IsVisible(e.Location));
            if (lastStar != null)
            {
                currentValue = lastStar.i + 1;
                DataGridView.Cursor = Cursors.Hand;
            }
            else if (RowIndex > -1)
            {
                currentValue = (int)(Value ?? 0);
                DataGridView.Cursor = Cursors.Default;
            }
            // DataGridView.InvalidateCell(this);
        }

        protected override void OnClick(DataGridViewCellEventArgs e)
        {
            base.OnClick(e);
            if (IsReadOnly()) return;
            Value = currentValue == 1 && (int?)Value == 1 ? 0 : currentValue;
        }

        protected override void OnMouseLeave(int rowIndex)
        {
            base.OnMouseLeave(rowIndex);
            mouseOver = false;
            if (IsReadOnly()) return;
            if (rowIndex == RowIndex)
            {
                currentValue = (int)(Value ?? 0);
                DataGridView.InvalidateCell(this);
            }
        }

        private bool IsReadOnly()
        {
            return OwningColumn is DataGridViewRatingColumn col && col.ReadOnly;
        }

        private void PaintStars(Graphics g, Rectangle bounds, int startIndex, int count, bool rated)
        {
            GraphicsState gs = g.Save();
            g.TranslateTransform(bounds.Left, bounds.Top);
            var col = OwningColumn as DataGridViewRatingColumn;
            Color ratedColor = col == null ? Color.Yellow :
                UseColumnStarColor ? col.RatedStarColor : RatedStarColor;
            Color grayColor = col == null ? Color.LightGray :
                UseColumnStarColor ? col.GrayStarColor : GrayStarColor;
            float starScale = col == null ? 1 :
                UseColumnStarScale ? col.StarScale : StarScale;
            UpdateBrushes(starScale);
            if (startIndex >= 0)
            {
                for (int i = startIndex; i < startIndex + count; i++)
                {
                    AdjustBrushColors(brushes[i], rated ? ratedColor : grayColor, rated ? Color.White : grayColor);
                    g.FillPath(brushes[i], stars[i]);
                    //g.DrawPath(Pens.Green, stars[i]);
                }
            }
            g.Restore(gs);
        }

        private Color ratedStarColor;
        private Color grayStarColor;
        private float starScale;

        public Color RatedStarColor
        {
            get { return ratedStarColor; }
            set
            {
                if (ratedStarColor != value)
                {
                    ratedStarColor = value;
                    if (OwningColumn is DataGridViewRatingColumn col && col.RatedStarColor != value)
                    {
                        UseColumnStarColor = false;
                        DataGridView.InvalidateCell(this);
                    }
                }
            }
        }

        public Color GrayStarColor
        {
            get { return grayStarColor; }
            set
            {
                if (grayStarColor != value)
                {
                    grayStarColor = value;
                    if (OwningColumn is DataGridViewRatingColumn col && col.GrayStarColor != value)
                    {
                        UseColumnStarColor = false;
                        DataGridView.InvalidateCell(this);
                    }
                }
            }
        }

        //Change the star size via scaling factor (default by 1)
        public float StarScale
        {
            get { return starScale; }
            set
            {
                if (starScale != value)
                {
                    starScale = value;
                    if (OwningColumn is DataGridViewRatingColumn col && col.StarScale != value)
                    {
                        UseColumnStarScale = false;
                        DataGridView.InvalidateCell(this);
                    }
                }
            }
        }

        public bool UseColumnStarColor { get; set; }
        public bool UseColumnStarScale { get; set; }
    }
}