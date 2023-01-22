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

namespace ExprObjModel.Procedures
{
#if false
    [SchemeIsAFunction("disposable-list?")]
    public class DisposableList
    {
        private List<IDisposable> dList;
        private bool isDisposed;

        [SchemeFunction("make-disposable-list")]
        public DisposableList()
        {
            dList = new List<IDisposable>();
            isDisposed = false;
        }

        public void Add(IDisposable d)
        {
            dList.Add(d);
        }

        public bool IsDisposed { [SchemeFunction("is-disposed?")] get { return isDisposed; } }

        [SchemeFunction("dispose-item!")]
        public void Remove(IDisposable d)
        {
            int iLast = dList.Count - 1;
            if (iLast < 0) return;

            int i = dList.Count;
            while (i > 0)
            {
                --i;
                if (object.ReferenceEquals(dList[i], d))
                {
                    d.Dispose();
                    if (i != iLast)
                    {
                        dList[i] = dList[iLast];
                    }
                    dList.RemoveAt(iLast);
                    break;
                }
            }
        }

        [SchemeFunction("dispose-all!")]
        public void DisposeAll()
        {
            if (isDisposed) return;
            isDisposed = true;
            foreach (IDisposable d in dList)
            {
                d.Dispose();
            }
            dList.Clear();
        }
    }
#endif
    
    [SchemeIsAFunction("byterect?")]
    public class ByteRectangle
    {
        private SchemeByteArray array;
        private int offset;
        private int width;
        private int stride;
        private int height;

        [SchemeFunction("make-byterect")]
        public ByteRectangle(SchemeByteArray array, int offset, int width, int height, int stride)
        {
            this.array = array;
            this.offset = offset;
            this.width = width;
            this.height = height;
            this.stride = stride;
        }

        public bool IsValid
        {
            [SchemeFunction("byterect-valid?")]
            get
            {
                if (array == null) return false;
                if (offset > array.Length) return false;
                if (offset + width > array.Length) return false;
                long totalLength = (long)stride * (long)(height - 1) + width;
                if (((long)offset + totalLength) > array.Length) return false;
                return true;
            }
        }

        [SchemeFunction("byterect-get-line")]
        public ByteRange GetScanLine(int i)
        {
            return new ByteRange(array, offset + (stride * i), width);
        }

        public SchemeByteArray Array { [SchemeFunction("get-byterect-array")] get { return array; } }
        public int Offset { [SchemeFunction("get-byterect-offset")] get { return offset; } }
        public int Width { [SchemeFunction("get-byterect-width")] get { return width; } }
        public int Stride { [SchemeFunction("get-byterect-stride")] get { return stride; } }
        public int Height { [SchemeFunction("get-byterect-height")] get { return height; } }
    }

    public static partial class ProxyDiscovery
    {
        [SchemeFunction("copy-byterect!")]
        public static void CopyRect(ByteRectangle src, ByteRectangle dest)
        {
            if (src.Width != dest.Width) throw new SchemeRuntimeException("copy-rect requires equal widths");
            if (src.Height != dest.Height) throw new SchemeRuntimeException("copy-rect requires equal heights");

            byte[] srcArray = src.Array.Bytes;
            byte[] destArray = dest.Array.Bytes;

            int yEnd = src.Height;
            for (int y = 0; y < yEnd; ++y)
            {
                int src1 = src.Offset + y * src.Stride;
                int dest1 = dest.Offset + y * dest.Stride;

                int xEnd = src.Width;
                for (int x = 0; x < xEnd; ++x)
                {
                    destArray[x + dest1] = srcArray[x + src1];
                }
            }
        }

        [SchemeFunction("fill-byterect!")]
        public static void FillRect(ByteRectangle dest, byte b)
        {
            byte[] destArray = dest.Array.Bytes;

            int yEnd = dest.Height;
            for (int y = 0; y < yEnd; ++y)
            {
                int dest1 = dest.Offset + y * dest.Stride;

                int xEnd = dest.Width;
                for (int x = 0; x < xEnd; ++x)
                {
                    destArray[x + dest1] = b;
                }
            }
        }

        [SchemeFunction("make-bitmap")]
        public static DisposableID MakeBitmap(IGlobalState gs, int xSize, int ySize)
        {
            Bitmap b = new Bitmap(xSize, ySize, System.Drawing.Imaging.PixelFormat.Format32bppRgb);
            DisposableID d = gs.RegisterDisposable(b, "Bitmap (" + xSize + " x " + ySize + ")");
            return d;
        }

        private class BitmapMaker : IProcedure
        {
            private Func<ByteRectangle, int> calcWidth;
            private Func<ByteRectangle, int> calcHeight;
            private Action<ByteRectangle, int[], System.Drawing.Imaging.BitmapData> copyBits;

            public BitmapMaker
            (
                Func<ByteRectangle, int> calcWidth,
                Func<ByteRectangle, int> calcHeight,
                Action<ByteRectangle, int[], System.Drawing.Imaging.BitmapData> copyBits
            )
            {
                this.calcWidth = calcWidth;
                this.calcHeight = calcHeight;
                this.copyBits = copyBits;
            }

            #region IProcedure Members

            public int Arity
            {
                get { return 2; }
            }

            public bool More
            {
                get { return false; }
            }

            public IRunnableStep Call(IGlobalState gs, FList<object> argList, IContinuation k)
            {
                try
                {
                    if (argList == null) throw new SchemeRuntimeException("make-bitmap-user: Insufficient arguments");
                    if (!(argList.Head is ByteRectangle)) throw new SchemeRuntimeException("make-bitmap-user: byterect expected");
                    ByteRectangle pixels = (ByteRectangle)(argList.Head);
                    argList = argList.Tail;
                    if (argList == null) throw new SchemeRuntimeException("make-bitmap-user: Insufficient arguments");
                    if (!(argList.Head is ByteRange)) throw new SchemeRuntimeException("make-bitmap-user: byterange expected");
                    ByteRange playpal = (ByteRange)(argList.Head);
                    argList = argList.Tail;

                    if (argList != null) throw new SchemeRuntimeException("make-bitmap-user: Too many arguments");

                    if (!(pixels.IsValid)) throw new SchemeRuntimeException("make-bitmap-user: Pixel byterect not valid");
                    if (!(playpal.IsValid)) throw new SchemeRuntimeException("make-bitmap-user: Palette byterange not valid");
                    if (playpal.Length < 3L) throw new SchemeRuntimeException("make-bitmap-user: Palette must contain at least 1 entry");

                    //System.Diagnostics.Stopwatch s = new System.Diagnostics.Stopwatch();
                    //s.Start();

                    int paletteCount = (int)(Math.Min(playpal.Length, 768L)) / 3;
                    int[] palette = new int[256];
                    for (int i = 0; i < paletteCount; ++i)
                    {
                        int pIndex = i * 3 + playpal.Offset;
                        if (playpal.Array.BigEndian)
                        {
                            palette[i] = ((int)(playpal.Array.Bytes[pIndex]) << 16) + ((int)(playpal.Array.Bytes[pIndex + 1]) << 8) + (int)(playpal.Array.Bytes[pIndex + 2]);
                        }
                        else
                        {
                            palette[i] = ((int)(playpal.Array.Bytes[pIndex])) + ((int)(playpal.Array.Bytes[pIndex + 1]) << 8) + (int)(playpal.Array.Bytes[pIndex + 2] << 16);
                        }
                    }

                    int bWidth = calcWidth(pixels);
                    int bHeight = calcHeight(pixels);
                    Bitmap b = new Bitmap(bWidth, bHeight, System.Drawing.Imaging.PixelFormat.Format32bppRgb);
                    DisposableID d = gs.RegisterDisposable(b, "Bitmap (" + bWidth + " x " + bHeight + ")");
                    Rectangle r = new Rectangle(0, 0, pixels.Width, pixels.Height);
                    System.Drawing.Imaging.BitmapData bData = b.LockBits(r, System.Drawing.Imaging.ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format32bppRgb);

                    copyBits(pixels, palette, bData);

                    b.UnlockBits(bData);
                    //s.Stop();
                    //Console.WriteLine("Time: " + s.Elapsed.TotalSeconds + " (" + s.ElapsedTicks + " ticks)");
                    return new RunnableReturn(k, d);
                }
                catch (Exception exc)
                {
                    return new RunnableThrow(k, exc);
                }
            }

            #endregion
        }

        [SchemeFunction("make-bitmap-maker")]
        public static IProcedure MakeBitmapMaker(object calcWidth, object calcHeight, object copyBits)
        {
            Pascalesque.One.IExpression calcWidthE = Pascalesque.One.Syntax.SyntaxAnalyzer.AnalyzeExpr(calcWidth);
            Pascalesque.One.IExpression calcHeightE = Pascalesque.One.Syntax.SyntaxAnalyzer.AnalyzeExpr(calcHeight);
            Pascalesque.One.IExpression copyBitsE = Pascalesque.One.Syntax.SyntaxAnalyzer.AnalyzeExpr(copyBits);

            if (calcWidthE == null) throw new Pascalesque.PascalesqueException("Syntax error in calcWidth");
            if (calcHeightE == null) throw new Pascalesque.PascalesqueException("Syntax error in calcHeight");
            if (copyBitsE == null) throw new Pascalesque.PascalesqueException("Syntax error in copyBits");
            List<Delegate> d = Pascalesque.One.Compiler.CompileRunAndCollect
            (
                new Pascalesque.One.MethodToBuild[]
                {
                    new Pascalesque.One.MethodToBuild
                    (
                        typeof(Func<ByteRectangle, int>),
                        new Symbol("calcWidth"),
                        new Pascalesque.One.ParamInfo[]
                        {
                            new Pascalesque.One.ParamInfo(new Symbol("pixels"), typeof(ByteRectangle))
                        },
                        typeof(int),
                        calcWidthE
                    ),
                    new Pascalesque.One.MethodToBuild
                    (
                        typeof(Func<ByteRectangle, int>),
                        new Symbol("calcHeight"),
                        new Pascalesque.One.ParamInfo[]
                        {
                            new Pascalesque.One.ParamInfo(new Symbol("pixels"), typeof(ByteRectangle))
                        },
                        typeof(int),
                        calcHeightE
                    ),
                    new Pascalesque.One.MethodToBuild
                    (
                        typeof(Action<ByteRectangle, int[], System.Drawing.Imaging.BitmapData>),
                        new Symbol("copyBits"),
                        new Pascalesque.One.ParamInfo[]
                        {
                            new Pascalesque.One.ParamInfo(new Symbol("pixels"), typeof(ByteRectangle)),
                            new Pascalesque.One.ParamInfo(new Symbol("palette"), typeof(int[])),
                            new Pascalesque.One.ParamInfo(new Symbol("lockedBits"), typeof(System.Drawing.Imaging.BitmapData))
                        },
                        typeof(void),
                        copyBitsE
                    )
                }
            );
            
            return new BitmapMaker
            (
                (Func<ByteRectangle, int>)(d[0]),
                (Func<ByteRectangle, int>)(d[1]),
                (Action<ByteRectangle, int[], System.Drawing.Imaging.BitmapData>)(d[2])
            );
        }

        [SchemeFunction("bitmap-x-size")]
        public static int BitmapXSize(Bitmap b)
        {
            return b.Width;
        }

        [SchemeFunction("bitmap-y-size")]
        public static int BitmapYSize(Bitmap b)
        {
            return b.Height;
        }

        [SchemeFunction("pixel-ref")]
        public static Color PixelRef(Bitmap b, int x, int y)
        {
            return b.GetPixel(x, y);
        }

        [SchemeFunction("pixel-set!")]
        public static void PlotPixel(Bitmap b, int x, int y, Color c)
        {
            b.SetPixel(x, y, c);
        }

        [SchemeFunction("make-graphics-for-bitmap")]
        public static DisposableID MakeGraphicsForBitmap(IGlobalState gs, Bitmap b)
        {
            Graphics g = Graphics.FromImage(b);
            DisposableID d = gs.RegisterDisposable(g, "Graphics");
            return d;
        }

        [SchemeFunction("rgb")]
        public static Color Rgb(byte r, byte g, byte b)
        {
            return Color.FromArgb(r, g, b);
        }

        [SchemeFunction("make-pen")]
        public static DisposableID MakePen(IGlobalState gs, Color c)
        {
            Pen p = new Pen(c);
            DisposableID d = gs.RegisterDisposable(p, "Pen");
            return d;
        }

        [SchemeFunction("make-pen-with-width")]
        public static DisposableID MakePen(IGlobalState gs, Color c, float width)
        {
            Pen p = new Pen(c, width);
            DisposableID d = gs.RegisterDisposable(p, "Pen");
            return d;
        }

        [SchemeFunction("make-solid-brush")]
        public static DisposableID MakeSolidBrush(IGlobalState gs, Color c)
        {
            Brush b = new SolidBrush(c);
            DisposableID d = gs.RegisterDisposable(b, "Solid Brush");
            return d;
        }

        [SchemeFunction("pointf")]
        public static PointF Point(float x, float y)
        {
            return new PointF(x, y);
        }

        [SchemeFunction("get-pointf-x")]
        public static float GetPointfX(PointF f)
        {
            return f.X;
        }

        [SchemeFunction("get-pointf-y")]
        public static float GetPointfY(PointF f)
        {
            return f.Y;
        }

        [SchemeFunction("make-gradient-brush")]
        public static DisposableID MakeGradientBrush(IGlobalState gs, PointF p1, Color c1, PointF p2, Color c2)
        {
            Brush b = new System.Drawing.Drawing2D.LinearGradientBrush(p1, p2, c1, c2);
            DisposableID d = gs.RegisterDisposable(b, "Gradient Brush");
            return d;
        }

        [SchemeFunction("graphics-draw-bitmap!")]
        public static void DrawImage(Graphics g, Bitmap b, PointF p1)
        {
            g.DrawImage(b, p1);
        }

        [SchemeFunction("graphics-line!")]
        public static void DrawLine(Graphics g, Pen p, PointF p1, PointF p2)
        {
            g.DrawLine(p, p1, p2);
        }

        [SchemeFunction("graphics-fill-rect!")]
        public static void FillRect(Graphics g, Brush b, PointF p1, PointF p2)
        {
            PointF pLow = new PointF(Math.Min(p1.X, p2.X), Math.Min(p1.Y, p2.Y));
            PointF pHigh = new PointF(Math.Max(p1.X, p2.X), Math.Max(p1.Y, p2.Y));
            SizeF size = new SizeF(pHigh.X - pLow.X, pHigh.Y - pLow.Y);

            g.FillRectangle(b, new RectangleF(pLow, size));
        }

        [SchemeFunction("graphics-clear!")]
        public static void GraphicsClear(Graphics gr, Color c)
        {
            gr.Clear(c);
        }

        [SchemeFunction("display-bitmap!")]
        public static void DisplayBitmap(Bitmap b)
        {
            // this is a horrible way to do it...

            Form f = new Form();
            f.FormBorderStyle = FormBorderStyle.FixedSingle;
            f.ClientSize = b.Size;
            f.BackgroundImage = b;
            f.BackgroundImageLayout = ImageLayout.Center;
            f.Text = "display-bitmap!";

            Application.Run(f);
        }

        [SchemeFunction("save-bitmap-png!")]
        public static void SaveBitmap(Bitmap b, string filename)
        {
            b.Save(filename, System.Drawing.Imaging.ImageFormat.Png);
        }

        public static FList<Symbol> ParseSymbolList(object obj)
        {
            FList<Symbol> f = null;
            while (obj is ConsCell)
            {
                ConsCell ccObj = (ConsCell)obj;
                if (ccObj.car is Symbol)
                {
                    Symbol s1 = (Symbol)ccObj.car;
                    f = new FList<Symbol>(s1, f);
                }
                else
                {
                    throw new SchemeRuntimeException("Non-symbol in symbol list.");
                }
                obj = ccObj.cdr;
            }
            if (!(obj is SpecialValue) || ((SpecialValue)obj) != SpecialValue.EMPTY_LIST)
            {
                throw new SchemeRuntimeException("Improper symbol list.");
            }
            return FListUtils.Reverse(f);
        }

        [SchemeFunction("fontstyle")]
        public static FontStyle MakeFontStyle(object obj)
        {
            if (obj is SpecialValue && ((SpecialValue)obj) == SpecialValue.EMPTY_LIST) return FontStyle.Regular;
            else
            {
                FList<Symbol> sList = ParseSymbolList(obj);
                FontStyle f = (FontStyle)0;
                while (sList != null)
                {
                    Symbol s1 = sList.Head;
                    if (s1.IsInterned)
                    {
                        if (s1.Name == "bold" || s1.Name == "b")
                        {
                            f = f | FontStyle.Bold;
                        }
                        else if (s1.Name == "italic" || s1.Name == "italics" || s1.Name == "i")
                        {
                            f = f | FontStyle.Italic;
                        }
                        else if (s1.Name == "strikethrough" || s1.Name == "strikeout" || s1.Name == "s")
                        {
                            f = f | FontStyle.Strikeout;
                        }
                        else if (s1.Name == "underline" || s1.Name == "u")
                        {
                            f = f | FontStyle.Underline;
                        }
                    }
                    sList = sList.Tail;
                }
                return f;
            }
        }

        [SchemeFunction("fontstyle->list")]
        public static object FontStyleToList(FontStyle f)
        {
            object obj = SpecialValue.EMPTY_LIST;
            if ((f & FontStyle.Strikeout) != 0) obj = new ConsCell(new Symbol("s"), obj);
            if ((f & FontStyle.Underline) != 0) obj = new ConsCell(new Symbol("u"), obj);
            if ((f & FontStyle.Italic) != 0) obj = new ConsCell(new Symbol("i"), obj);
            if ((f & FontStyle.Bold) != 0) obj = new ConsCell(new Symbol("b"), obj);
            return obj;
        }

        [SchemeFunction("fontstyle?")]
        public static bool IsFontStyle(object obj)
        {
            return (obj is FontStyle);
        }

        [SchemeFunction("make-font")]
        public static DisposableID MakeFont(IGlobalState gs, string name, float size, FontStyle fontStyle)
        {
            Font f = new Font(name, size, fontStyle, GraphicsUnit.Pixel);
            DisposableID d = gs.RegisterDisposable(f, "Font");
            return d;
        }

        [SchemeFunction("font-get-height")]
        public static float GetHeight(Font f)
        {
            return f.GetHeight();
        }

        [SchemeFunction("font-get-cell-ascent")]
        public static int GetCellAscent(Font f)
        {
            return f.FontFamily.GetCellAscent(f.Style);
        }

        [SchemeFunction("font-get-cell-descent")]
        public static int GetCellDescent(Font f)
        {
            return f.FontFamily.GetCellDescent(f.Style);
        }

        [SchemeFunction("font-get-em-height")]
        public static int GetEmHeight(Font f)
        {
            return f.FontFamily.GetEmHeight(f.Style);
        }

        [SchemeFunction("font-get-line-spacing")]
        public static int GetLineSpacing(Font f)
        {
            return f.FontFamily.GetLineSpacing(f.Style);
        }

        [SchemeFunction("font-is-style-available?")]
        public static bool IsStyleAvailable(Font f, FontStyle fs)
        {
            return f.FontFamily.IsStyleAvailable(fs);
        }

        [SchemeFunction("graphics-draw-text!")]
        public static void GraphicsDrawText(Graphics g, string str, Font f, Brush b, PointF pt)
        {
            g.DrawString(str, f, b, pt);
        }

        [SchemeFunction("get-parameter-types")]
        public static object GetParameterTypes(object obj)
        {
            if (obj is IProxyProcedure)
            {
                IProxyProcedure pp = (IProxyProcedure)obj;
                System.Reflection.MethodBase mb = pp.WrappedMethod; 
                object result = SpecialValue.EMPTY_LIST;

                if (!mb.IsStatic && !mb.IsConstructor)
                {
                    result = new ConsCell(mb.DeclaringType, result);
                }
                System.Reflection.ParameterInfo[] pi = pp.WrappedMethod.GetParameters();
                foreach (System.Reflection.ParameterInfo p in pi)
                {
                    result = new ConsCell(p.ParameterType, result);
                }
                ConsCell.Reverse(ref result);
                return result;
            }
            else if (obj is IProcedure)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        [SchemeFunction("get-return-type")]
        public static object GetReturnType(object obj)
        {
            if (obj is IProxyProcedure)
            {
                IProxyProcedure pp = (IProxyProcedure)obj;
                System.Reflection.MethodBase mb = pp.WrappedMethod;

                if (mb.IsConstructor)
                {
                    return mb.DeclaringType;
                }
                else
                {
                    return ((System.Reflection.MethodInfo)mb).ReturnType;
                }
            }
            else if (obj is IProcedure)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }

    [SchemeSingleton("graphics-measure-text")]
    public class GraphicsMeasureText : IProcedure
    {
        public GraphicsMeasureText()
        {
        }

        #region IProcedure Members

        public int Arity { get { return 4; } } // (graphics-measure-text <graphics> <str> <font> <proc>) => (<proc> <width> <height>)

        public bool More { get { return false; } }

        public IRunnableStep Call(IGlobalState gs, FList<object> argList, IContinuation k)
        {
            try
            {
                if (FListUtils.CountUpTo(argList, 5) != 4) throw new SchemeRuntimeException("Incorrect arity");
                Graphics g = (Graphics)(argList.Head);
                argList = argList.Tail;
                SchemeString str = (SchemeString)(argList.Head);
                argList = argList.Tail;
                Font f = (Font)(argList.Head);
                argList = argList.Tail;
                IProcedure proc = (IProcedure)(argList.Head);
                argList = argList.Tail;

                if (proc.Arity > 2) throw new SchemeRuntimeException("graphics-measure-text: procedure expects too many arguments");
                if (proc.Arity < 2 && !proc.More) throw new SchemeRuntimeException("graphics-measure-text: procedure doesn't allow enough arguments");

                SizeF size = g.MeasureString(str.TheString, f);

                FList<object> result = null;
                result = new FList<object>((double)(size.Height), result);
                result = new FList<object>((double)(size.Width), result);

                return new RunnableCall(proc, result, k);
            }
            catch (Exception exc)
            {
                return new RunnableThrow(k, exc);
            }
        }

        #endregion
    }
}