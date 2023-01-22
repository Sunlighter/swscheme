/*
    This file is part of Sunlit World Scheme
    http://swscheme.codeplex.com/
    Copyright (c) 2010 by Edward Kiser (edkiser@gmail.com)

    This program is free software; you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation; either version 2 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License along
    with this program; if not, write to the Free Software Foundation, Inc.,
    51 Franklin Street, Fifth Floor, Boston, MA 02110-1301 USA.
*/

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace ExprObjModel
{
    namespace Drawing
    {
        public abstract class Context : IDisposable
        {
            public abstract Pen Pen { get; }
            public abstract Brush Brush { get; }
            public abstract Font Font { get; }

            public virtual void Dispose()
            {
            }
        }

        public class InitialContext : Context
        {
            private Pen p;
            private Brush b;
            private Font f;

            public InitialContext()
            {
                p = new Pen(Color.Black);
                b = new SolidBrush(Color.White);
                f = new Font("Lucida Console", 8.0f, GraphicsUnit.Point);
            }

            public override Pen Pen
            {
                get { return p; }
            }

            public override Brush Brush
            {
                get { return b; }
            }

            public override Font Font
            {
                get { return f; }
            }

            public override void Dispose()
            {
                p.Dispose();
                b.Dispose();
            }
        }

        public class ContextWithPen : Context
        {
            private Context ancestor;
            private Pen p;

            public ContextWithPen(Context ancestor, Pen p)
            {
                this.ancestor = ancestor;
                this.p = p;
            }

            public override Pen Pen
            {
                get { return p; }
            }

            public override Brush Brush
            {
                get { return ancestor.Brush; }
            }

            public override Font Font
            {
                get { return ancestor.Font; }
            }

            public override void Dispose()
            {
                p.Dispose();
            }
        }

        public class ContextWithBrush : Context
        {
            private Context ancestor;
            private Brush b;

            public ContextWithBrush(Context ancestor, Brush b)
            {
                this.ancestor = ancestor;
                this.b = b;
            }

            public override Pen Pen
            {
                get { return ancestor.Pen; }
            }

            public override Brush Brush
            {
                get { return b; }
            }

            public override Font Font
            {
                get { return ancestor.Font; }
            }

            public override void Dispose()
            {
                b.Dispose();
            }
        }

        public class ContextWithFont : Context
        {
            private Context ancestor;
            private Font f;

            public ContextWithFont(Context ancestor, Font f)
            {
                this.ancestor = ancestor;
                this.f = f;
            }

            public override Pen Pen
            {
                get { return ancestor.Pen; }
            }

            public override Brush Brush
            {
                get { return ancestor.Brush; }
            }

            public override Font Font
            {
                get { return f; }
            }

            public override void Dispose()
            {
                f.Dispose();
            }
        }

        [DescendantsWithPatterns]
        public abstract class ColorSpec
        {
            public abstract Color GetColor();
        }

        [Pattern("(color $name)")]
        public class NameColorSpec : ColorSpec
        {
            [Bind("$name")]
            public Symbol name;

            public override Color GetColor()
            {
                return Color.FromName(name.ToString());
            }
        }

        [Pattern("(rgb $r $g $b)")]
        public class RgbColorSpec : ColorSpec
        {
            [Bind("$r")]
            public int r;

            [Bind("$g")]
            public int g;

            [Bind("$b")]
            public int b;

            public override Color GetColor()
            {
                return Color.FromArgb(r, g, b);
            }
        }

        [DescendantsWithPatterns]
        public abstract class RegionSpec
        {
            public abstract Region CreateRegion();
        }

        [Pattern("(rect $x1 $y1 $x2 $y2)")]
        public class RectRegionSpec : RegionSpec
        {
            [Bind("$x1")]
            public float x1;

            [Bind("$y1")]
            public float y1;

            [Bind("$x2")]
            public float x2;

            [Bind("$y2")]
            public float y2;

            public override Region CreateRegion()
            {
                return new Region(new RectangleF(Math.Min(x1, x2), Math.Min(y1, y2), Math.Abs(x2 - x1), Math.Abs(y2 - y1)));
            }
        }

        [Pattern("(polygon . $x)")]
        public class PolyRegionSpec : RegionSpec
        {
            [Bind("$x")]
            public List<float> coords;

            public override Region CreateRegion()
            {
                if (coords.Count >= 6 && ((coords.Count & 1) == 0))
                {
                    int points = coords.Count / 2;
                    List<PointF> pf = new List<PointF>();
                    List<byte> b = new List<byte>();
                    for (int i = 0; i < coords.Count; i += 2)
                    {
                        pf.Add(new PointF(coords[i], coords[i + 1]));
                        if (i == 0) b.Add((byte)(System.Drawing.Drawing2D.PathPointType.Start));
                        else b.Add((byte)(System.Drawing.Drawing2D.PathPointType.Line));
                    }
                    using (System.Drawing.Drawing2D.GraphicsPath gp = new System.Drawing.Drawing2D.GraphicsPath(pf.ToArray(), b.ToArray(), System.Drawing.Drawing2D.FillMode.Winding))
                    {
                        return new Region(gp);
                    }
                }
                else if (coords.Count < 6) throw new SchemeRuntimeException("Polygon region requires at least 3 points");
                else throw new SchemeRuntimeException("Polygon region requires X and Y coordinates for each point");
            }
        }

        [Pattern("(union . $regions)")]
        public class UnionRegionSpec : RegionSpec
        {
            [Bind("$regions")]
            public List<RegionSpec> regions;

            public override Region CreateRegion()
            {
                if (regions.Count == 1)
                {
                    return regions[0].CreateRegion();
                }
                else
                {
                    Region r = new Region();
                    r.MakeEmpty();
                    foreach (RegionSpec r2 in regions)
                    {
                        Region r3 = r2.CreateRegion();
                        r.Union(r3);
                        r3.Dispose();
                    }
                    return r;
                }
            }
        }

        [Pattern("(intersection . $regions)")]
        public class IntersectionRegionSpec : RegionSpec
        {
            [Bind("$regions")]
            public List<RegionSpec> regions;

            public override Region CreateRegion()
            {
                if (regions.Count == 1)
                {
                    return regions[0].CreateRegion();
                }
                else
                {
                    Region r = new Region();
                    r.MakeInfinite();
                    foreach (RegionSpec r2 in regions)
                    {
                        Region r3 = r2.CreateRegion();
                        r.Intersect(r3);
                        r3.Dispose();
                    }
                    return r;
                }
            }
        }

        [Pattern("(difference $r1 . $regions)")]
        public class DifferenceRegionSpec : RegionSpec
        {
            [Bind("$r1")]
            public RegionSpec r1;

            [Bind("$regions")]
            public List<RegionSpec> regions;

            public override Region CreateRegion()
            {
                Region r = r1.CreateRegion();
                foreach (RegionSpec r2 in regions)
                {
                    Region r3 = r2.CreateRegion();
                    r.Exclude(r3);
                    r3.Dispose();
                }
                return r;
            }
        }

        [Pattern("(symmetric-difference . $regions)")]
        public class SymmetricDifferenceRegionSpec : RegionSpec
        {
            public List<RegionSpec> regions;

            public override Region CreateRegion()
            {
                if (regions.Count == 1)
                {
                    return regions[0].CreateRegion();
                }
                else
                {
                    Region r = new Region();
                    r.MakeEmpty();
                    foreach (RegionSpec r2 in regions)
                    {
                        Region r3 = r2.CreateRegion();
                        r.Xor(r3);
                        r3.Dispose();
                    }
                    return r;
                }
            }
        }

        [DescendantsWithPatterns]
        public abstract class FontSizeSpec
        {
            public abstract Tuple<float, GraphicsUnit> GetSize();
        }

        [Pattern("(point $p)")]
        public class PointFontSizeSpec : FontSizeSpec
        {
            [Bind("$p")]
            public float p;

            public override Tuple<float, GraphicsUnit> GetSize()
            {
                return new Tuple<float, GraphicsUnit>(p, GraphicsUnit.Point);
            }
        }

        [Pattern("(pixel $p)")]
        public class PixelFontSizeSpec : FontSizeSpec
        {
            [Bind("$p")]
            public float p;

            public override Tuple<float, GraphicsUnit> GetSize()
            {
                return new Tuple<float, GraphicsUnit>(p, GraphicsUnit.Pixel);
            }
        }

        [Pattern("(inch $p)")]
        public class InchFontSizeSpec : FontSizeSpec
        {
            [Bind("$p")]
            public float p;

            public override Tuple<float, GraphicsUnit> GetSize()
            {
                return new Tuple<float, GraphicsUnit>(p, GraphicsUnit.Inch);
            }
        }

        [Pattern("(millimeter $p)")]
        public class MillimeterFontSizeSpec : FontSizeSpec
        {
            [Bind("$p")]
            public float p;

            public override Tuple<float, GraphicsUnit> GetSize()
            {
                return new Tuple<float, GraphicsUnit>(p, GraphicsUnit.Millimeter);
            }
        }

        [DescendantsWithPatterns]
        public abstract class FontSpec
        {
            public abstract Font GetFontInternal(FontStyle style);

            public Font GetFont()
            {
                return GetFontInternal(FontStyle.Regular);
            }
        }

        [Pattern("(font $name $size)")]
        public class PlainFontSpec : FontSpec
        {
            [Bind("$name")]
            public string name;

            [Bind("$size")]
            public FontSizeSpec size;

            public override Font GetFontInternal(FontStyle style)
            {
                Tuple<float, GraphicsUnit> t = size.GetSize();
                return new Font(name, t.Item1, style, t.Item2);
            }
        }

        [Pattern("(bold $font)")]
        public class BoldFontSpec : FontSpec
        {
            [Bind("$font")]
            public FontSpec font;

            public override Font GetFontInternal(FontStyle style)
            {
                return font.GetFontInternal(style | FontStyle.Bold);
            }
        }

        [Pattern("(italic $font)")]
        public class ItalicFontSpec : FontSpec
        {
            [Bind("$font")]
            public FontSpec font;

            public override Font GetFontInternal(FontStyle style)
            {
                return font.GetFontInternal(style | FontStyle.Italic);
            }
        }

        [Pattern("(underline $font)")]
        public class UnderlineFontSpec : FontSpec
        {
            [Bind("$font")]
            public FontSpec font;

            public override Font GetFontInternal(FontStyle style)
            {
                return font.GetFontInternal(style | FontStyle.Underline);
            }
        }

        [Pattern("(strikeout $font)")]
        public class StrikeoutFontSpec : FontSpec
        {
            [Bind("$font")]
            public FontSpec font;

            public override Font GetFontInternal(FontStyle style)
            {
                return font.GetFontInternal(style | FontStyle.Strikeout);
            }
        }

        [DescendantsWithPatterns]
        public abstract class DrawForm
        {
            public abstract void Draw(Graphics g, Context c);
        }

        [Pattern("(clear $color)")]
        public class ClearForm : DrawForm
        {
            [Bind("$color")]
            public ColorSpec color;

            public override void Draw(Graphics g, Context c)
            {
                g.Clear(color.GetColor());
            }
        }

        [Pattern("(displace ($dx $dy) . $cmds)")]
        public class DisplaceForm : DrawForm
        {
            [Bind("$dx")]
            public float dx;

            [Bind("$dy")]
            public float dy;

            [Bind("$cmds")]
            public List<DrawForm> cmds;

            public override void Draw(Graphics g, Context c)
            {
                System.Drawing.Drawing2D.Matrix m = g.Transform;
                try
                {
                    g.MultiplyTransform(new System.Drawing.Drawing2D.Matrix(1.0f, 0.0f, 0.0f, 1.0f, dx, dy));

                    foreach (DrawForm d in cmds)
                    {
                        d.Draw(g, c);
                    }
                }
                finally
                {
                    g.Transform = m;
                }
            }
        }

        [Pattern("(clip-inside $region . $cmds)")]
        public class ClipInsideRectForm : DrawForm
        {
            [Bind("$region")]
            public RegionSpec r;

            [Bind("$cmds")]
            public List<DrawForm> cmds;

            public override void Draw(Graphics g, Context c)
            {
                Region r1 = g.Clip;
                Region s = r.CreateRegion();
                g.IntersectClip(s);
                s.Dispose();
                foreach (DrawForm d in cmds)
                {
                    d.Draw(g, c);
                }
                g.Clip = r1;
            }
        }

        [Pattern("(clip-outside $region . $cmds)")]
        public class ClipOutsideRectForm : DrawForm
        {
            [Bind("$region")]
            public RegionSpec r;


            [Bind("$cmds")]
            public List<DrawForm> cmds;

            public override void Draw(Graphics g, Context c)
            {
                Region r1 = g.Clip;
                Region s = r.CreateRegion();
                g.ExcludeClip(s);
                s.Dispose();
                foreach (DrawForm d in cmds)
                {
                    d.Draw(g, c);
                }
                g.Clip = r1;
            }
        }

        [Pattern("(line $x1 $y1 $x2 $y2)")]
        public class LineDrawForm : DrawForm
        {
            [Bind("$x1")]
            public float x1;

            [Bind("$y1")]
            public float y1;

            [Bind("$x2")]
            public float x2;

            [Bind("$y2")]
            public float y2;

            public override void Draw(Graphics g, Context c)
            {
                g.DrawLine(c.Pen, x1, y1, x2, y2);
            }
        }

        [Pattern("(fill-rect $x1 $y1 $x2 $y2)")]
        public class FillRectDrawForm : DrawForm
        {
            [Bind("$x1")]
            public float x1;

            [Bind("$y1")]
            public float y1;

            [Bind("$x2")]
            public float x2;

            [Bind("$y2")]
            public float y2;

            public override void Draw(Graphics g, Context c)
            {
                g.FillRectangle(c.Brush, Math.Min(x1, x2), Math.Min(y1, y2), Math.Abs(x2 - x1), Math.Abs(y2 - y1));
            }
        }

        [Pattern("(fill-polygon . $coords)")]
        public class FillPolygonDrawForm : DrawForm
        {
            [Bind("$coords")]
            public List<float> coords;

            public override void Draw(Graphics g, Context c)
            {
                if (coords.Count >= 6 && ((coords.Count & 1) == 0))
                {
                    int points = coords.Count / 2;
                    List<PointF> pf = new List<PointF>();
                    List<byte> b = new List<byte>();
                    for (int i = 0; i < coords.Count; i += 2)
                    {
                        pf.Add(new PointF(coords[i], coords[i + 1]));
                        if (i == 0) b.Add((byte)(System.Drawing.Drawing2D.PathPointType.Start));
                        else b.Add((byte)(System.Drawing.Drawing2D.PathPointType.Line));
                    }
                    using (System.Drawing.Drawing2D.GraphicsPath gp = new System.Drawing.Drawing2D.GraphicsPath(pf.ToArray(), b.ToArray(), System.Drawing.Drawing2D.FillMode.Winding))
                    {
                        g.FillPath(c.Brush, gp);
                    }
                }
                else if (coords.Count < 6) throw new SchemeRuntimeException("Polygon requires at least 3 points");
                else throw new SchemeRuntimeException("Polygon requires X and Y coordinates for each point");
            }
        }

        [Pattern("(text $x $y $text)")]
        public class TextDrawForm : DrawForm
        {
            [Bind("$x")]
            public float x;

            [Bind("$y")]
            public float y;

            [Bind("$text")]
            public string text;

            public override void Draw(Graphics g, Context c)
            {
                g.DrawString(text, c.Font, c.Brush, x, y);
            }
        }

        [Pattern("(with-pen $color . $cmds)")]
        public class WithPenDrawForm : DrawForm
        {
            [Bind("$color")]
            public ColorSpec color;

            [Bind("$cmds")]
            public List<DrawForm> cmds;

            public override void Draw(Graphics g, Context c)
            {
                using (Context c2 = new ContextWithPen(c, new Pen(color.GetColor())))
                {
                    foreach (DrawForm d in cmds)
                    {
                        d.Draw(g, c2);
                    }
                }
            }
        }

        [Pattern("(with-solid-brush $color . $cmds)")]
        public class WithSolidBrushDrawForm : DrawForm
        {
            [Bind("$color")]
            public ColorSpec color;

            [Bind("$cmds")]
            public List<DrawForm> cmds;

            public override void Draw(Graphics g, Context c)
            {
                using (Context c2 = new ContextWithBrush(c, new SolidBrush(color.GetColor())))
                {
                    foreach (DrawForm d in cmds)
                    {
                        d.Draw(g, c2);
                    }
                }
            }
        }

        [Pattern("(with-font $font . $cmds)")]
        public class WithFontForm : DrawForm
        {
            [Bind("$font")]
            public FontSpec font;

            [Bind("$cmds")]
            public List<DrawForm> cmds;

            public override void Draw(Graphics g, Context c)
            {
                using (Context c2 = new ContextWithFont(c, font.GetFont()))
                {
                    foreach (DrawForm d in cmds)
                    {
                        d.Draw(g, c2);
                    }
                }
            }
        }

        [Pattern("(begin . $cmds)")]
        public class BeginDrawForm : DrawForm
        {
            [Bind("$cmds")]
            public List<DrawForm> cmds;

            public override void Draw(Graphics g, Context c)
            {
                foreach (DrawForm cmd in cmds)
                {
                    cmd.Draw(g, c);
                }
            }
        }

        public static class Parser
        {
            private static Func<object, ExprObjModel.Option<object>> parseDrawForm = null;

            public static DrawForm Parse(object obj)
            {
                if (parseDrawForm == null)
                {
                    lock (typeof(Parser))
                    {
                        if (parseDrawForm == null)
                        {
                            parseDrawForm = ExprObjModel.Utils.MakeParser(typeof(DrawForm));
                        }
                    }
                }

                ExprObjModel.Option<object> opt = parseDrawForm(obj);
                if (opt is ExprObjModel.Some<object>)
                {
                    return (DrawForm)(((ExprObjModel.Some<object>)opt).value);
                }
                else return null;
            }

            public static Context NewInitialContext()
            {
                return new InitialContext();
            }
        }
    }

    namespace Procedures
    {
        public static partial class ProxyDiscovery
        {
            [SchemeFunction("graphics-draw!")]
            public static void DrawForm(Graphics g, object drawing)
            {
                ExprObjModel.Drawing.DrawForm d = ExprObjModel.Drawing.Parser.Parse(drawing);

                if (d == null) throw new SchemeRuntimeException("Unable to parse drawing");

                using (ExprObjModel.Drawing.Context c = ExprObjModel.Drawing.Parser.NewInitialContext())
                {
                    d.Draw(g, c);
                }
            }
        }
    }
}