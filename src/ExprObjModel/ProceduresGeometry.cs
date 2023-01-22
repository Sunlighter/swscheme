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
using System.Linq;
using BigMath;
using ControlledWindowLib;

namespace ExprObjModel
{
    public enum HashDelimiters : byte
    {
        Vector2 = 0x61,
        Vertex2 = 0x65,
        Vector3 = 0x71,
        Vertex3 = 0x72,
        Quaternion = 0x6B,
    }

    [SchemeIsAFunction("vector2?")]
    public class Vector2 : IHashable, IEquatable<Vector2>
    {
        private BigRational x;
        private BigRational y;

        [SchemeFunction("make-vector2")]
        public Vector2(BigRational x, BigRational y)
        {
            this.x = x;
            this.y = y;
        }

        public BigRational X { get { return x; } }
        public BigRational Y { get { return y; } }

        public static Vector2 operator +(Vector2 a, Vector2 b)
        {
            return new Vector2(a.x + b.x, a.y + b.y);
        }

        public static Vector2 operator -(Vector2 a, Vector2 b)
        {
            return new Vector2(a.x - b.x, a.y - b.y);
        }

        public static Vector2 operator -(Vector2 a)
        {
            return new Vector2(-a.x, -a.y);
        }

        public BigRational Dot(Vector2 other)
        {
            return (x * other.x) + (y * other.y);
        }

        public BigRational Cross(Vector2 other)
        {
            return (x * other.y) - (y * other.x);
        }

        public Vector2 R90()
        {
            return new Vector2(y, -x);
        }

        public bool IsParallelTo(Vector2 other)
        {
            return this.Cross(other) == BigRational.Zero;
        }

        public bool IsPerpendicularTo(Vector2 other)
        {
            return this.Dot(other) == BigRational.Zero;
        }

        public BigRational LengthSquared { [SchemeFunction("vector2-length-squared")] get { return this.Dot(this); } }

        [SchemeFunction("vector2-scaled-length-along")]
        public BigRational ScaledLengthAlong(Vector2 axis)
        {
            return this.Dot(axis) / axis.Dot(axis);
        }

        [SchemeFunction("vector3-component-along")]
        public Vector2 ComponentAlong(Vector2 axis)
        {
            return axis * ScaledLengthAlong(axis);
        }

        [SchemeFunction("vector3-component-ortho")]
        public Vector2 ComponentOrtho(Vector2 axis)
        {
            return this - ComponentAlong(axis);
        }

        public static Vector2 operator *(Vector2 a, BigInteger b)
        {
            BigRational bb = new BigRational(b, BigInteger.One);
            return new Vector2(a.x * bb, a.y * bb);
        }

        public static Vector2 operator *(Vector2 a, BigRational b)
        {
            return new Vector2(a.x * b, a.y * b);
        }

        public static Vector2 operator *(BigInteger a, Vector2 b)
        {
            BigRational aa = new BigRational(a, BigInteger.One);
            return new Vector2(aa * b.x, aa * b.y);
        }

        public static Vector2 operator *(BigRational a, Vector2 b)
        {
            return new Vector2(a * b.x, a * b.y);
        }

        public static Vector2 operator /(Vector2 a, BigInteger b)
        {
            BigRational bb = new BigRational(b, BigInteger.One);
            return new Vector2(a.x / bb, a.y / bb);
        }

        public static Vector2 operator /(Vector2 a, BigRational b)
        {
            return new Vector2(a.x / b, a.y / b);
        }

        private static Vector2 zero = null;

        public static Vector2 Zero
        {
            get
            {
                if (zero == null)
                {
                    lock (typeof(Vector2))
                    {
                        if (zero == null)
                        {
                            zero = new Vector2(BigRational.Zero, BigRational.Zero);
                        }
                    }
                }
                return zero;
            }
        }

        public static bool operator ==(Vector2 a, Vector2 b)
        {
            if (object.ReferenceEquals(a, null) && object.ReferenceEquals(b, null)) return true;
            if (object.ReferenceEquals(a, null) || object.ReferenceEquals(b, null)) return false;
            return (a.x == b.x) && (a.y == b.y);
        }

        public static bool operator !=(Vector2 a, Vector2 b)
        {
            if (object.ReferenceEquals(a, null) && object.ReferenceEquals(b, null)) return false;
            if (object.ReferenceEquals(a, null) || object.ReferenceEquals(b, null)) return true;
            return (a.x != b.x) || (a.y != b.y);
        }

        public void AddToHash(IHashGenerator hg)
        {
            hg.Add((byte)(HashDelimiters.Vector2));
            x.AddToHash(hg);
            hg.Add((byte)(HashDelimiters.Vector2));
            y.AddToHash(hg);
            hg.Add((byte)(HashDelimiters.Vector2));
        }

        public bool Equals(Vector2 other)
        {
            return (this == other);
        }

        public override bool Equals(object obj)
        {
            if (obj is Vector2) return Equals((Vector2)obj);
            else return false;
        }

        public override int GetHashCode()
        {
            HashGenerator hg = new HashGenerator();
            AddToHash(hg);
            return hg.Hash;
        }

        public override string ToString()
        {
            return SchemeDataWriter.ItemToString(this);
        }
    }

    [SchemeIsAFunction("vertex2?")]
    public class Vertex2 : IHashable, IEquatable<Vertex2>
    {
        private BigRational x;
        private BigRational y;

        [SchemeFunction("make-vertex2")]
        public Vertex2(BigRational x, BigRational y)
        {
            this.x = x;
            this.y = y;
        }

        public BigRational X { get { return x; } }
        public BigRational Y { get { return y; } }

        public static Vertex2 operator +(Vertex2 a, Vector2 b)
        {
            return new Vertex2(a.x + b.X, a.y + b.Y);
        }

        public static Vertex2 operator +(Vector2 a, Vertex2 b)
        {
            return new Vertex2(a.X + b.x, a.Y + b.y);
        }

        public static Vertex2 operator -(Vertex2 a, Vector2 b)
        {
            return new Vertex2(a.x - b.X, a.y - b.Y);
        }

        public static Vector2 operator -(Vertex2 a, Vertex2 b)
        {
            return new Vector2(a.x - b.x, a.y - b.y);
        }

        private static Vertex2 origin = null;

        public static Vertex2 Origin
        {
            get
            {
                if (origin == null)
                {
                    lock (typeof(Vertex2))
                    {
                        if (origin == null)
                        {
                            origin = new Vertex2(BigRational.Zero, BigRational.Zero);
                        }
                    }
                }
                return origin;
            }
        }

        public static bool operator ==(Vertex2 a, Vertex2 b)
        {
            if (object.ReferenceEquals(a, null) && object.ReferenceEquals(b, null)) return true;
            if (object.ReferenceEquals(a, null) || object.ReferenceEquals(b, null)) return false;
            return (a.x == b.x) && (a.y == b.y);
        }

        public static bool operator !=(Vertex2 a, Vertex2 b)
        {
            if (object.ReferenceEquals(a, null) && object.ReferenceEquals(b, null)) return false;
            if (object.ReferenceEquals(a, null) || object.ReferenceEquals(b, null)) return true;
            return (a.x != b.x) || (a.y != b.y);
        }

        public void AddToHash(IHashGenerator hg)
        {
            hg.Add((byte)(HashDelimiters.Vertex2));
            x.AddToHash(hg);
            hg.Add((byte)(HashDelimiters.Vertex2));
            y.AddToHash(hg);
            hg.Add((byte)(HashDelimiters.Vertex2));
        }

        public bool Equals(Vertex2 other)
        {
            return (this == other);
        }

        public override bool Equals(object obj)
        {
            if (obj is Vertex2) return Equals((Vertex2)obj);
            else return false;
        }

        public override int GetHashCode()
        {
            HashGenerator hg = new HashGenerator();
            AddToHash(hg);
            return hg.Hash;
        }

        public override string ToString()
        {
            return SchemeDataWriter.ItemToString(this);
        }
    }

    [SchemeIsAFunction("vector3?")]
    public class Vector3 : IHashable, IEquatable<Vector3>
    {
        private BigRational x;
        private BigRational y;
        private BigRational z;

        [SchemeFunction("make-vector3")]
        public Vector3(BigRational x, BigRational y, BigRational z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public BigRational X { get { return x; } }
        public BigRational Y { get { return y; } }
        public BigRational Z { get { return z; } }

        public static Vector3 operator +(Vector3 a, Vector3 b)
        {
            return new Vector3(a.x + b.x, a.y + b.y, a.z + b.z);
        }

        public static Vector3 operator -(Vector3 a, Vector3 b)
        {
            return new Vector3(a.x - b.x, a.y - b.y, a.z - b.z);
        }

        public static Vector3 operator -(Vector3 a)
        {
            return new Vector3(-a.x, -a.y, -a.z);
        }

        public BigRational Dot(Vector3 other)
        {
            return (x * other.x) + (y * other.y) + (z * other.z);
        }

        public Vector3 Cross(Vector3 other)
        {
            return new Vector3
            (
                y * other.z - z * other.y,
                z * other.x - x * other.z,
                x * other.y - y * other.x
            );
        }

        public bool IsParallelTo(Vector3 other)
        {
            return this.Cross(other) == Vector3.Zero;
        }

        public bool IsPerpendicularTo(Vector3 other)
        {
            return this.Dot(other) == BigRational.Zero;
        }

        public BigRational LengthSquared { [SchemeFunction("vector3-length-squared")] get { return this.Dot(this); } }

        [SchemeFunction("vector3-scaled-length-along")]
        public BigRational ScaledLengthAlong(Vector3 axis)
        {
            return this.Dot(axis) / axis.Dot(axis);
        }

        [SchemeFunction("vector3-component-along")]
        public Vector3 ComponentAlong(Vector3 axis)
        {
            return axis * ScaledLengthAlong(axis);
        }

        [SchemeFunction("vector3-component-ortho")]
        public Vector3 ComponentOrtho(Vector3 axis)
        {
            return this - ComponentAlong(axis);
        }

        public static Vector3 operator *(Vector3 a, BigInteger b)
        {
            BigRational bb = new BigRational(b, BigInteger.One);
            return new Vector3(a.x * bb, a.y * bb, a.z * bb);
        }

        public static Vector3 operator *(Vector3 a, BigRational b)
        {
            return new Vector3(a.x * b, a.y * b, a.z * b);
        }

        public static Vector3 operator *(BigInteger a, Vector3 b)
        {
            BigRational aa = new BigRational(a, BigInteger.One);
            return new Vector3(aa * b.x, aa * b.y, aa * b.z);
        }

        public static Vector3 operator *(BigRational a, Vector3 b)
        {
            return new Vector3(a * b.x, a * b.y, a * b.z);
        }

        public static Vector3 operator /(Vector3 a, BigInteger b)
        {
            BigRational bb = new BigRational(b, BigInteger.One);
            return new Vector3(a.x / bb, a.y / bb, a.z / bb);
        }

        public static Vector3 operator /(Vector3 a, BigRational b)
        {
            return new Vector3(a.x / b, a.y / b, a.z / b);
        }

        private static Vector3 zero = null;

        public static Vector3 Zero
        {
            get
            {
                if (zero == null)
                {
                    lock (typeof(Vector3))
                    {
                        if (zero == null)
                        {
                            zero = new Vector3(BigRational.Zero, BigRational.Zero, BigRational.Zero);
                        }
                    }
                }
                return zero;
            }
        }

        public static bool operator ==(Vector3 a, Vector3 b)
        {
            if (object.ReferenceEquals(a, null) && object.ReferenceEquals(b, null)) return true;
            if (object.ReferenceEquals(a, null) || object.ReferenceEquals(b, null)) return false;
            return (a.x == b.x) && (a.y == b.y) && (a.z == b.z);
        }

        public static bool operator !=(Vector3 a, Vector3 b)
        {
            if (object.ReferenceEquals(a, null) && object.ReferenceEquals(b, null)) return false;
            if (object.ReferenceEquals(a, null) || object.ReferenceEquals(b, null)) return true;
            return (a.x != b.x) || (a.y != b.y) || (a.z != b.z);
        }

        public void AddToHash(IHashGenerator hg)
        {
            hg.Add((byte)(HashDelimiters.Vector3));
            x.AddToHash(hg);
            hg.Add((byte)(HashDelimiters.Vector3));
            y.AddToHash(hg);
            hg.Add((byte)(HashDelimiters.Vector3));
            z.AddToHash(hg);
            hg.Add((byte)(HashDelimiters.Vector3));
        }

        public bool Equals(Vector3 other)
        {
            return (this == other);
        }

        public override bool Equals(object obj)
        {
            if (obj is Vector3) return Equals((Vector3)obj);
            else return false;
        }

        public override int GetHashCode()
        {
            HashGenerator hg = new HashGenerator();
            AddToHash(hg);
            return hg.Hash;
        }

        public override string ToString()
        {
            return SchemeDataWriter.ItemToString(this);
        }
    }

    [SchemeIsAFunction("vertex3?")]
    public class Vertex3 : IHashable, IEquatable<Vertex3>
    {
        private BigRational x;
        private BigRational y;
        private BigRational z;

        [SchemeFunction("make-vertex3")]
        public Vertex3(BigRational x, BigRational y, BigRational z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public BigRational X { get { return x; } }
        public BigRational Y { get { return y; } }
        public BigRational Z { get { return z; } }

        public static Vertex3 operator +(Vertex3 a, Vector3 b)
        {
            return new Vertex3(a.x + b.X, a.y + b.Y, a.z + b.Z);
        }

        public static Vertex3 operator +(Vector3 a, Vertex3 b)
        {
            return new Vertex3(a.X + b.x, a.Y + b.y, a.Z + b.z);
        }

        public static Vertex3 operator -(Vertex3 a, Vector3 b)
        {
            return new Vertex3(a.x - b.X, a.y - b.Y, a.z - b.Z);
        }

        public static Vector3 operator -(Vertex3 a, Vertex3 b)
        {
            return new Vector3(a.x - b.x, a.y - b.y, a.z - b.z);
        }

        private static Vertex3 origin = null;

        public static Vertex3 Origin
        {
            get
            {
                if (origin == null)
                {
                    lock (typeof(Vertex3))
                    {
                        if (origin == null)
                        {
                            origin = new Vertex3(BigRational.Zero, BigRational.Zero, BigRational.Zero);
                        }
                    }
                }
                return origin;
            }
        }

        public static bool operator ==(Vertex3 a, Vertex3 b)
        {
            if (object.ReferenceEquals(a, null) && object.ReferenceEquals(b, null)) return true;
            if (object.ReferenceEquals(a, null) || object.ReferenceEquals(b, null)) return false;
            return (a.x == b.x) && (a.y == b.y) && (a.z == b.z);
        }

        public static bool operator !=(Vertex3 a, Vertex3 b)
        {
            if (object.ReferenceEquals(a, null) && object.ReferenceEquals(b, null)) return false;
            if (object.ReferenceEquals(a, null) || object.ReferenceEquals(b, null)) return true;
            return (a.x != b.x) || (a.y != b.y) || (a.z != b.z);
        }

        public void AddToHash(IHashGenerator hg)
        {
            hg.Add((byte)(HashDelimiters.Vertex3));
            x.AddToHash(hg);
            hg.Add((byte)(HashDelimiters.Vertex3));
            y.AddToHash(hg);
            hg.Add((byte)(HashDelimiters.Vertex3));
            z.AddToHash(hg);
            hg.Add((byte)(HashDelimiters.Vertex3));
        }

        public bool Equals(Vertex3 other)
        {
            return (this == other);
        }

        public override bool Equals(object obj)
        {
            if (obj is Vertex3) return Equals((Vertex3)obj);
            else return false;
        }

        public override int GetHashCode()
        {
            HashGenerator hg = new HashGenerator();
            AddToHash(hg);
            return hg.Hash;
        }

        public override string ToString()
        {
            return SchemeDataWriter.ItemToString(this);
        }
    }

    [SchemeIsAFunction("quaternion?")]
    public class Quaternion : IHashable, IEquatable<Quaternion>
    {
        private BigRational w;
        private Vector3 v;

        [SchemeFunction("make-quaternion-from-scalar-and-vector")]
        public Quaternion(BigRational w, Vector3 v)
        {
            this.w = w;
            this.v = v;
        }

        [SchemeFunction("make-quaternion")]
        public Quaternion(BigRational w, BigRational x, BigRational y, BigRational z)
        {
            this.w = w;
            this.v = new Vector3(x, y, z);
        }

        public Vector3 V { get { return v; } }
        public BigRational W { get { return w; } }

        public BigRational X { get { return v.X; } }
        public BigRational Y { get { return v.Y; } }
        public BigRational Z { get { return v.Z; } }

        public static Quaternion operator +(Quaternion a, Quaternion b)
        {
            return new Quaternion(a.W + b.W, a.V + b.V);
        }

        public static Quaternion operator -(Quaternion a, Quaternion b)
        {
            return new Quaternion(a.W - b.W, a.V - b.V);
        }

        public static Quaternion operator -(Quaternion a)
        {
            return new Quaternion(-a.W, -a.V);
        }

        public static Quaternion operator *(Quaternion a, Quaternion b)
        {
            return new Quaternion((a.W * b.W) - (a.V.Dot(b.V)), (a.W * b.V) + (a.V * b.W) + a.V.Cross(b.V));
        }

        public static Quaternion operator *(Quaternion a, BigInteger b)
        {
            return new Quaternion(a.w * b, a.v * b);
        }

        public static Quaternion operator *(Quaternion a, BigRational b)
        {
            return new Quaternion(a.w * b, a.v * b);
        }

        public static Quaternion operator *(BigInteger a, Quaternion b)
        {
            return new Quaternion(a * b.w, a * b.v);
        }

        public static Quaternion operator *(BigRational a, Quaternion b)
        {
            return new Quaternion(a * b.w, a * b.v);
        }

        public static Quaternion operator /(Quaternion a, BigInteger b)
        {
            return new Quaternion(a.w / b, a.v / b);
        }

        public static Quaternion operator /(Quaternion a, BigRational b)
        {
            return new Quaternion(a.w / b, a.v / b);
        }

        public static Quaternion operator /(Quaternion a, Quaternion b)
        {
            return a * b.Reciprocal();
        }

        public static Quaternion operator /(BigInteger a, Quaternion b)
        {
            return a * b.Reciprocal();
        }

        public static Quaternion operator /(BigRational a, Quaternion b)
        {
            return a * b.Reciprocal();
        }

        public Quaternion Conjugate()
        {
            return new Quaternion(w, -v);
        }

        public BigRational LengthSquared()
        {
            return (w * w) + (v.X * v.X) + (v.Y * v.Y) + (v.Z * v.Z);
        }

        public Quaternion Reciprocal()
        {
            return Conjugate() / LengthSquared();
        }

        public Vector3 Rotate(Vector3 v)
        {
            Quaternion q2 = this * new Quaternion(BigRational.Zero, v) * this.Conjugate();
            System.Diagnostics.Debug.Assert(q2.w == BigRational.Zero);
            return q2.v;
        }

        private static Quaternion zero = null;

        public static Quaternion Zero
        {
            get
            {
                if (zero == null)
                {
                    lock (typeof(Quaternion))
                    {
                        if (zero == null)
                        {
                            zero = new Quaternion(BigRational.Zero, Vector3.Zero);
                        }
                    }
                }
                return zero;
            }
        }

        private static Quaternion one = null;

        public static Quaternion One
        {
            get
            {
                if (one == null)
                {
                    lock (typeof(Quaternion))
                    {
                        if (one == null)
                        {
                            one = new Quaternion(BigRational.One, Vector3.Zero);
                        }
                    }
                }
                return one;
            }
        }

        public static bool operator ==(Quaternion a, Quaternion b)
        {
            if (object.ReferenceEquals(a, null) && object.ReferenceEquals(b, null)) return true;
            if (object.ReferenceEquals(a, null) || object.ReferenceEquals(b, null)) return false;
            return (a.w == b.w) && (a.v == b.v);
        }

        public static bool operator !=(Quaternion a, Quaternion b)
        {
            if (object.ReferenceEquals(a, null) && object.ReferenceEquals(b, null)) return false;
            if (object.ReferenceEquals(a, null) || object.ReferenceEquals(b, null)) return true;
            return (a.w != b.w) || (a.v != b.v);
        }

        public void AddToHash(IHashGenerator hg)
        {
            hg.Add((byte)(HashDelimiters.Quaternion));
            w.AddToHash(hg);
            hg.Add((byte)(HashDelimiters.Quaternion));
            v.AddToHash(hg);
            hg.Add((byte)(HashDelimiters.Quaternion));
        }

        public bool Equals(Quaternion other)
        {
            return (this == other);
        }

        public override bool Equals(object obj)
        {
            if (obj is Quaternion) return Equals((Quaternion)obj);
            else return false;
        }

        public override int GetHashCode()
        {
            HashGenerator hg = new HashGenerator();
            AddToHash(hg);
            return hg.Hash;
        }

        public override string ToString()
        {
            return SchemeDataWriter.ItemToString(this);
        }
    }

    [SchemeIsAFunction("line3?")]
    public class Line3
    {
        private Vertex3 origin;
        private Vector3 direction;

        [SchemeFunction("make-line3-origin-direction")]
        public Line3(Vertex3 origin, Vector3 direction)
        {
            this.origin = origin;
            this.direction = direction;
        }

        public Vertex3 Origin { [SchemeFunction("line3-origin")] get { return origin; } }
        public Vector3 Direction { [SchemeFunction("line3-direction")] get { return direction; } }

        public bool Contains(Vertex3 pt)
        {
            return direction.IsParallelTo(pt - origin);
        }

        public bool IsParallelTo(Line3 line)
        {
            return direction.IsParallelTo(line.direction);
        }

        public bool IsParallelTo(Plane3 plane)
        {
            return direction.IsPerpendicularTo(plane.Normal);
        }

        public bool Intersects(Line3 line)
        {
            if (this.IsParallelTo(line)) return false;
            return new Plane3(this.origin, this.direction.Cross(line.direction)).Contains(line.origin);
        }

        public bool IsCoincidentWith(Line3 line)
        {
            return this.IsParallelTo(line) && this.Contains(line.Origin);
        }

        public Vertex3 NearestPointTo(Vertex3 pt)
        {
            return origin + ((pt - origin).ComponentAlong(direction));
        }

        public BigRational ScaledCoordinateOf(Vertex3 pt)
        {
            return (pt - origin).ScaledLengthAlong(direction);
        }

        [SchemeFunction("make-line3-two-points")]
        public static Line3 FromTwoPoints(Vertex3 a, Vertex3 b)
        {
            return new Line3(a, b - a);
        }
    }

    [SchemeIsAFunction("plane3?")]
    public class Plane3
    {
        private Vertex3 origin;
        private Vector3 normal;

        [SchemeFunction("make-plane3-origin-normal")]
        public Plane3(Vertex3 origin, Vector3 normal)
        {
            this.origin = origin;
            this.normal = normal;
        }

        public Vertex3 Origin { [SchemeFunction("plane3-origin")] get { return origin; } }
        public Vector3 Normal { [SchemeFunction("plane3-normal")] get { return normal; } }

        [SchemeFunction("plane3-flip")]
        public Plane3 Flip()
        {
            return new Plane3(origin, -normal);
        }

        public bool Contains(Vertex3 pt)
        {
            return normal.IsPerpendicularTo(pt - origin);
        }

        public bool IsParallelTo(Line3 line)
        {
            return line.Direction.IsPerpendicularTo(normal);
        }

        public bool IsParallelTo(Plane3 plane)
        {
            return plane.normal.IsParallelTo(normal);
        }

        public bool IsPerpendicularTo(Line3 line)
        {
            return line.Direction.IsParallelTo(normal);
        }

        public bool Contains(Line3 line)
        {
            return this.IsParallelTo(line) && this.Contains(line.Origin);
        }

        public bool IsCoincidentWith(Plane3 plane)
        {
            return this.IsParallelTo(plane) && this.Contains(plane.Origin);
        }

        public Vertex3 NearestPointTo(Vertex3 pt)
        {
            return origin + ((pt - origin).ComponentOrtho(normal));
        }

        public BigRational ScaledDistanceTo(Vertex3 pt)
        {
            return (pt - origin).ScaledLengthAlong(normal);
        }

        [SchemeFunction("plane3-excludes?")]
        public bool Includes(Vertex3 pt)
        {
            return this.ScaledDistanceTo(pt) < BigRational.Zero;
        }

        [SchemeFunction("plane3-includes?")]
        public bool Excludes(Vertex3 pt)
        {
            return this.ScaledDistanceTo(pt) > BigRational.Zero;
        }

        [SchemeFunction("plane3-flip-to-include")]
        public Plane3 FlipToInclude(Vertex3 pt)
        {
            if (this.Includes(pt)) return this; else return this.Flip();
        }

        [SchemeFunction("make-plane3-two-points")]
        public static Plane3 FromTwoPoints(Vertex3 a, Vertex3 b)
        {
            return new Plane3(a, b - a);
        }

        [SchemeFunction("make-plane3-three-points")]
        public static Plane3 FromThreePoints(Vertex3 a, Vertex3 b, Vertex3 c)
        {
            return new Plane3(a, (b - a).Cross(c - a));
        }

        [SchemeFunction("make-plane3-four-points")]
        public static Plane3 FromFourPoints(Vertex3 a, Vertex3 b, Vertex3 c, Vertex3 keep)
        {
            Plane3 p = Plane3.FromThreePoints(a, b, c);
            if (p.Contains(keep))
            {
                throw new SchemeRuntimeException("make-plane3-four-points: Fourth point lies in the plane of the first three");
            }
            else
            {
                return p.FlipToInclude(keep);
            }
        }
    }

    public enum PointStatus
    {
        Outside,
        OnCorner,
        OnEdge,
        OnFace,
        Inside
    }

    [SchemeIsAFunction("convex-hull?")]
    public abstract class ConvexHull
    {
        public abstract PointStatus GetPointStatus(Vertex3 vt);

        [SchemeFunction("convex-hull-add")]
        public abstract ConvexHull Add(Vertex3 vt);
    }

    public class CH_Empty : ConvexHull
    {
        public CH_Empty() { }

        public override PointStatus GetPointStatus(Vertex3 vt)
        {
            return PointStatus.Outside;
        }

        public override ConvexHull Add(Vertex3 vt)
        {
            return new CH_SinglePoint(vt);
        }
    }

    public class CH_SinglePoint : ConvexHull
    {
        private Vertex3 vertex;

        public CH_SinglePoint(Vertex3 vertex)
        {
            this.vertex = vertex;
        }

        public override PointStatus GetPointStatus(Vertex3 vt)
        {
            if (vt == vertex) return PointStatus.OnCorner;
            else return PointStatus.Outside;
        }

        public override ConvexHull Add(Vertex3 vt)
        {
            if (vt == vertex) return this;
            return new CH_LineSegment(vertex, vt);
        }
    }

    public class CH_LineSegment : ConvexHull
    {
        private Vertex3 v1;
        private Vertex3 v2;

        public CH_LineSegment(Vertex3 v1, Vertex3 v2)
        {
            this.v1 = v1;
            this.v2 = v2;
        }

        public Line3 Line { get { return Line3.FromTwoPoints(v1, v2); } }

        public override PointStatus GetPointStatus(Vertex3 vt)
        {
            if (v1 == vt || v2 == vt)
            {
                return PointStatus.OnCorner;
            }
            else if (this.Line.Contains(vt))
            {
                BigRational scaledPos = (vt - v1).ScaledLengthAlong(v2 - v1);
                if (scaledPos < BigRational.Zero || scaledPos > BigRational.One)
                {
                    return PointStatus.Outside;
                }
                else
                {
                    return PointStatus.OnEdge;
                }
            }
            else
            {
                return PointStatus.Outside;
            }
        }

        public override ConvexHull Add(Vertex3 vt)
        {
            if (this.Line.Contains(vt))
            {
                BigRational scaledPos = (vt - v1).ScaledLengthAlong(v2 - v1);
                if (scaledPos < BigRational.Zero)
                {
                    return new CH_LineSegment(vt, v2);
                }
                else if (scaledPos > BigRational.One)
                {
                    return new CH_LineSegment(v1, vt);
                }
                else return this;
            }
            else
            {
                return new CH_Polygon(new Vertex3[] { v1, v2, vt });
            }
        }
    }

    public class CH_Polygon : ConvexHull
    {
        private Vertex3[] vertices;
        private Vector3 normal;

        public CH_Polygon(Vertex3[] vertices)
        {
            System.Diagnostics.Debug.Assert(vertices.Length >= 3);

            this.vertices = vertices;
            this.normal = (vertices[1] - vertices[0]).Cross(vertices[2] - vertices[1]);

            CheckIntegrity();
        }

        private void CheckIntegrity()
        {
            int iEnd = vertices.Length;
            foreach (Edge e in this.Edges)
            {
                for (int i = 0; i < iEnd; ++i)
                {
                    if (i == e.StartIndex || i == e.EndIndex) continue;
                    if (!(e.Plane.Includes(vertices[i]))) throw new ArgumentException("Invalid polygon");
                }
            }
        }

        public int Count { get { return vertices.Length; } }

        public Plane3 Plane
        {
            get
            {
                return new Plane3(vertices[0], normal);
            }
        }

        public Vector3 Normal
        {
            get
            {
                return normal;
            }
        }

        public IReadOnlyArray<Vertex3> Vertices { get { return vertices.AsReadOnlyArray(); } }

        public CH_Polygon Flip()
        {
            int iEnd = vertices.Length;
            Vertex3[] v2 = new Vertex3[iEnd];
            for (int i = 0; i < iEnd; ++i) v2[i] = vertices[iEnd - i - 1];
            return new CH_Polygon(v2);
        }

        public CH_Polygon FlipToInclude(Vertex3 v)
        {
            if (this.Plane.Contains(v)) throw new ArgumentException("Polygon cannot be flipped to include a point that lies in the polygon's plane");
            if (this.Plane.Includes(v)) return this;
            else return this.Flip();
        }

        public class Edge
        {
            private CH_Polygon parent;
            private int startIndex;

            public Edge(CH_Polygon parent, int startIndex)
            {
                this.parent = parent;
                this.startIndex = startIndex;
            }

            public int StartIndex { get { return startIndex; } }
            public Vertex3 Start { get { return parent.vertices[startIndex]; } }
            public int EndIndex { get { int e = startIndex + 1; int end = parent.vertices.Length; return (e >= end) ? 0 : e; } }
            public Vertex3 End { get { return parent.vertices[EndIndex]; } }
            public int BeyondIndex { get { int b = startIndex + 2; int end = parent.vertices.Length; return (b >= end) ? (b - end) : b; } }
            public Vertex3 Beyond { get { return parent.vertices[BeyondIndex]; } }

            public Line3 Line { get { return Line3.FromTwoPoints(Start, End); } }
            public Plane3 Plane { get { return Plane3.FromFourPoints(Start, End, Start + parent.normal, Beyond); } }
        }

        public IReadOnlyArray<Edge> Edges
        {
            get
            {
                return new ReadOnlyArrayFunc<Edge>(vertices.Length, delegate(int index) { return new Edge(this, index); } );
            }
        }

        public override PointStatus GetPointStatus(Vertex3 vt)
        {
            if (Plane.Contains(vt))
            {
                foreach (Edge e in Edges)
                {
                    if (e.Plane.Excludes(vt)) return PointStatus.Outside;
                    if (e.Start == vt || e.End == vt) return PointStatus.OnCorner;
                    if (e.Line.Contains(vt))
                    {
                        BigRational b = e.Line.ScaledCoordinateOf(vt);
                        if (b < BigRational.Zero || b > BigRational.One) return PointStatus.Outside;
                        else return PointStatus.OnEdge;
                    }
                }
                return PointStatus.OnFace;
            }
            else
            {
                return PointStatus.Outside;
            }
        }

        public override ConvexHull Add(Vertex3 vt)
        {
            if (Plane.Contains(vt))
            {
                if (!Edges.Any(x => x.Plane.Excludes(vt))) return this;

                Deque<Edge> d = new Deque<Edge>(Edges);
                int sentryCount = d.Count;
                while (true)
                {
                    if (!(d.Front.Plane.Includes(vt)) && d.Back.Plane.Includes(vt)) break;
                    Edge e = d.PopFront();
                    d.PushBack(e);
                    --sentryCount;
                    if (sentryCount < 0) throw new InvalidOperationException("Bug in convex hull routine would have caused an infinite loop");
                }
                while (!(d.Front.Plane.Includes(vt)))
                {
                    d.PopFront();
                }
                List<Vertex3> vertexlist = new List<Vertex3>();
                vertexlist.Add(vt);
                vertexlist.AddRange(d.Select(x => x.Start));
                vertexlist.Add(d.Back.End);
                return new CH_Polygon(vertexlist.ToArray());
            }
            else
            {
                return CH_Polyhedron.Make(Utils.SingleItem(FlipToInclude(vt)), vt);
            }
        }
    }

    public class CH_Polyhedron : ConvexHull
    {
        private CH_Polygon[] faces;

        public CH_Polyhedron(CH_Polygon[] faces)
        {
            this.faces = faces;
            CheckIntegrity();
        }

        private void CheckIntegrity()
        {
            int iEnd = faces.Length;
            DualIndexedSet<Tuple<Vertex3, Vertex3>> edgeMap = new DualIndexedSet<Tuple<Vertex3,Vertex3>>(x => new Tuple<Vertex3, Vertex3>(x.Item2, x.Item1));
            Dictionary<int, int> edgeToFace = new Dictionary<int, int>();
            for (int i = 0; i < iEnd; ++i)
            {
                foreach (CH_Polygon.Edge edge in faces[i].Edges)
                {
                    int eIndex = edgeMap.EnsureAdded(new Tuple<Vertex3, Vertex3>(edge.Start, edge.End));
                    if (edgeToFace.ContainsKey(eIndex)) throw new ArgumentException("Invalid polyhedron (edge traversed in same direction by more than one face)");
                    edgeToFace.Add(eIndex, i);
                }
                for (int j = 0; j < iEnd; ++j)
                {
                    if (i == j) continue;
                    if (faces[i].Plane.IsCoincidentWith(faces[j].Plane)) throw new ArgumentException("Invalid polyhedron (two faces have the same plane)");
                    if (faces[j].Vertices.Any(x => faces[i].Plane.Excludes(x))) throw new ArgumentException("Invalid polyhedron (one face cuts another)");
                }
            }
            int iEnd2 = edgeMap.Count;
            for (int i = 0; i < iEnd2; ++i)
            {
                if (!(edgeToFace.ContainsKey(i)) || !(edgeToFace.ContainsKey(~i)))
                {
                    throw new ArgumentException("Invalid polyhedron (loose edge)");
                }
            }
        }

        public IReadOnlyArray<CH_Polygon> Faces { get { return faces.AsReadOnlyArray(); } }

        public override PointStatus GetPointStatus(Vertex3 vt)
        {
            if (faces.Any(x => x.Plane.Excludes(vt))) return PointStatus.Outside;
            foreach (CH_Polygon f in faces)
            {
                PointStatus ps = f.GetPointStatus(vt);
                if (ps != PointStatus.Outside) return ps;
            }
            return PointStatus.Inside;
        }

        public static CH_Polyhedron Make(IEnumerable<CH_Polygon> facesAway, Vertex3 v)
        {
            List<CH_Polygon> facesAway1 = facesAway.ToList();
            foreach (CH_Polygon face in facesAway1)
            {
                if (!(face.Plane.Includes(v))) throw new ArgumentException("Polygons must face away from new point");
            }
            
            DualIndexedSet<Tuple<Vertex3, Vertex3>> edgeMap = new DualIndexedSet<Tuple<Vertex3, Vertex3>>(x => new Tuple<Vertex3, Vertex3>(x.Item2, x.Item1));
            Dictionary<int, int> edgeToFace = new Dictionary<int,int>();
            int iEnd = facesAway1.Count;
            for(int i = 0; i < iEnd; ++i)
            {
                foreach (CH_Polygon.Edge edge in facesAway1[i].Edges)
                {
                    int eIndex = edgeMap.EnsureAdded(new Tuple<Vertex3, Vertex3>(edge.Start, edge.End));
                    if (edgeToFace.ContainsKey(eIndex)) throw new ArgumentException("Invalid polyhedron (edge traversed in same direction by more than one face)");
                    edgeToFace.Add(eIndex, i);
                }
            }
            Dictionary<Vertex3, int> startToEdge = new Dictionary<Vertex3, int>();
            int iEnd2 = edgeMap.Count;
            for (int i = 0; i < iEnd2; ++i)
            {
                if (!(edgeToFace.ContainsKey(i))) startToEdge.Add(edgeMap[i].Item1, i);
                if (!(edgeToFace.ContainsKey(~i))) startToEdge.Add(edgeMap[~i].Item1, ~i);
            }

            Deque<int> edges = new Deque<int>();
            edges.PushBack(startToEdge.First().Value);
            int sentinel = startToEdge.Count;
            while (true)
            {
                Vertex3 end = edgeMap[edges.Back].Item2;
                if (!(startToEdge.ContainsKey(end))) throw new ArgumentException("Unable to walk loose edges");
                int nextEdge = startToEdge[end];
                if (nextEdge == edges.Front) break;
                edges.PushBack(nextEdge);
                --sentinel;
                if (sentinel < 0) throw new InvalidOperationException("Loose edges don't loop properly");
            }
            if (edges.Count != startToEdge.Count) throw new InvalidOperationException("Not all loose edges were used");

            sentinel = edges.Count;
            Func<int, int, bool> inSamePlane = delegate(int edge1, int edge2)
            {
                Tuple<Vertex3, Vertex3> e1 = edgeMap[edge1];
                Tuple<Vertex3, Vertex3> e2 = edgeMap[edge2];
                Plane3 p = Plane3.FromThreePoints(e1.Item1, e1.Item2, v);
                return p.Contains(e2.Item1) && p.Contains(e2.Item2);
            };

            while (true)
            {
                if (!inSamePlane(edges.Front, edges.Back)) break;
                int x = edges.PopFront();
                edges.PushBack(x);
                --sentinel;
                if (sentinel < 0) throw new InvalidOperationException("Loose edges all lie in the same plane as the vertex (?!)");
            }

            List<CH_Polygon> newPolys = new List<CH_Polygon>();
            List<Vertex3> corners = new List<Vertex3>();
            int? lastEdge = null;

            Action flush = delegate()
            {
                corners.Add(edgeMap[lastEdge.Value].Item2);
                corners.Add(v);
                newPolys.Add(new CH_Polygon(corners.ToArray()));
            };

            Action<int> addEdge = delegate(int edge)
            {
                if (lastEdge == null || inSamePlane(edge, lastEdge.Value))
                {
                    corners.Add(edgeMap[edge].Item1);
                    lastEdge = edge;
                }
                else
                {
                    flush();
                    corners.Clear();
                    corners.Add(edgeMap[edge].Item1);
                    lastEdge = edge;
                }
            };

            foreach (int edge in edges) addEdge(edge);
            flush();

            return new CH_Polyhedron(newPolys.Concat(facesAway1).ToArray());
        }

        public override ConvexHull Add(Vertex3 vt)
        {
            if (GetPointStatus(vt) != PointStatus.Outside) return this;

            return Make(faces.Where(x => x.Plane.Includes(vt)), vt);
        }
    }

    namespace Procedures
    {
        public static partial class ProxyDiscovery
        {
            [SchemeFunction("vector2-x")]
            public static object Vector2X(Vector2 a)
            {
                return MathUtils.Normalize(a.X);
            }

            [SchemeFunction("vector2-y")]
            public static object Vector2Y(Vector2 a)
            {
                return MathUtils.Normalize(a.Y);
            }

            [SchemeFunction("vector2-zero?")]
            public static bool Vector2IsZero(Vector2 a)
            {
                return a == Vector2.Zero;
            }

            [SchemeFunction("vector3-x")]
            public static object Vector3X(Vector3 a)
            {
                return MathUtils.Normalize(a.X);
            }

            [SchemeFunction("vector3-y")]
            public static object Vector3Y(Vector3 a)
            {
                return MathUtils.Normalize(a.Y);
            }

            [SchemeFunction("vector3-z")]
            public static object Vector3Z(Vector3 a)
            {
                return MathUtils.Normalize(a.Z);
            }

            [SchemeFunction("vector3-zero?")]
            public static bool Vector3IsZero(Vector3 a)
            {
                return a == Vector3.Zero;
            }

            [SchemeFunction("vertex3-x")]
            public static object Vertex3X(Vertex3 a)
            {
                return MathUtils.Normalize(a.X);
            }

            [SchemeFunction("vertex3-y")]
            public static object Vertex3Y(Vertex3 a)
            {
                return MathUtils.Normalize(a.Y);
            }

            [SchemeFunction("vertex3-z")]
            public static object Vertex3Z(Vertex3 a)
            {
                return MathUtils.Normalize(a.Z);
            }

            [SchemeFunction("vertex3-origin?")]
            public static object Vertex3IsOrigin(Vertex3 a)
            {
                return a == Vertex3.Origin;
            }

            [SchemeFunction("interpolate")]
            public static object Interpolate(object x1, object y1, object x2, object y2, object x3)
            {
                //  (y3 - y1)   (x3 - x1)
                //  --------- = ---------
                //  (y2 - y1)   (y2 - y1)
                
                //                        (x3 - x1)
                //  y3 = y1 + (y2 - y1) * ---------
                //                        (x2 - x1)

                return MathUtils.Normalize(MathUtils.Add2(y1, MathUtils.Multiply2(MathUtils.Subtract2(y2, y1), MathUtils.Divide2(MathUtils.Subtract2(x3, x1), MathUtils.Subtract2(x2, x1)))));
            }

            private static object IntersectLines(Line3 line1, Line3 line2)
            {
                if (line1.IsCoincidentWith(line2))
                {
                    return new Symbol("coincident");
                }
                else if (line1.IsParallelTo(line2))
                {
                    return new Symbol("parallel");
                }
                else if (!(line1.Intersects(line2)))
                {
                    return new Symbol("skew");
                }
                else
                {
                    Vector3 convergence = line2.Direction.ComponentOrtho(line1.Direction);
                    //Vector3 run = line2.Direction.ComponentAlong(line1.Direction);
                    Vector3 p1 = line2.Origin - line1.Origin;
                    Vector3 p2 = line2.Origin + line2.Direction - line1.Origin;
                    return Interpolate(p1.ScaledLengthAlong(convergence), line2.Origin, p2.ScaledLengthAlong(convergence), line2.Origin + line2.Direction, BigInteger.Zero);
                }
            }

            private static object IntersectLinePlane(Line3 line, Plane3 plane)
            {
                if (plane.Contains(line))
                {
                    return new Symbol("coincident");
                }
                else if (plane.IsParallelTo(line))
                {
                    return new Symbol("parallel");
                }
                else
                {
                    Vector3 convergence = line.Direction.ComponentAlong(plane.Normal);
                    //Vector3 run = line.Direction.ComponentOrtho(plane.Normal);
                    Vector3 p1 = line.Origin - plane.Origin;
                    Vector3 p2 = (line.Origin + line.Direction) - plane.Origin;
                    return Interpolate(p1.ScaledLengthAlong(convergence), line.Origin, p2.ScaledLengthAlong(convergence), line.Origin + line.Direction, BigInteger.Zero);
                }
            }

            private static object IntersectTwoPlanes(Plane3 plane1, Plane3 plane2)
            {
                if (plane1.IsCoincidentWith(plane2))
                {
                    return new Symbol("coincident");
                }
                else if (plane1.IsParallelTo(plane2))
                {
                    return new Symbol("parallel");
                }
                else
                {
                    Vector3 binormal = plane1.Normal.Cross(plane2.Normal);
                    Vertex3 origin1 = plane1.Origin;
                    Vertex3 origin2 = new Plane3(origin1, binormal).NearestPointTo(plane2.Origin);

                    Line3 line1 = new Line3(origin1, plane1.Normal);
                    Line3 line2 = new Line3(origin2, plane2.Normal);

                    Vector3 convergence = line2.Direction.ComponentOrtho(line1.Direction);
                    Vector3 run = line2.Direction.ComponentAlong(line1.Direction);
                    Vector3 p1 = line2.Origin - line1.Origin;
                    Vector3 p2 = line2.Origin + line2.Direction - line1.Origin;
                    
                    Vertex3 isect = (Vertex3)Interpolate(p1.ScaledLengthAlong(convergence), line2.Origin, p2.ScaledLengthAlong(convergence), line2.Origin + line2.Direction, BigInteger.Zero);

                    return new Line3(isect, binormal);
                }
            }

            [SchemeFunction("intersection")]
            public static object Intersection(object obj1, object obj2)
            {
                if (obj1 is Line3)
                {
                    if (obj2 is Line3)
                    {
                        return IntersectLines((Line3)obj1, (Line3)obj2);
                    }
                    else if (obj2 is Plane3)
                    {
                        return IntersectLinePlane((Line3)obj1, (Plane3)obj2);
                    }
                    else
                    {
                        throw new SchemeRuntimeException("intersect2: argument 2 of type " + obj2.GetType() + " not supported");
                    }
                }
                else if (obj1 is Plane3)
                {

                    if (obj2 is Line3)
                    {
                        return IntersectLinePlane((Line3)obj2, (Plane3)obj1);
                    }
                    else if (obj2 is Plane3)
                    {
                        return IntersectTwoPlanes((Plane3)obj1, (Plane3)obj2);
                    }
                    else
                    {
                        throw new SchemeRuntimeException("intersect2: argument 2 of type " + obj2.GetType() + " not supported");
                    }
                }
                else
                {
                    throw new SchemeRuntimeException("intersect2: argument 1 of type " + obj1.GetType() + " not supported");
                }
            }

            [SchemeFunction("parallel?")]
            public static bool Parallel(object obj1, object obj2)
            {
                if (obj1 is Vector3)
                {
                    Vector3 v1 = (Vector3)obj1;
                    if (obj2 is Vector3)
                    {
                        Vector3 v2 = (Vector3)obj2;
                        return v1.IsParallelTo(v2);
                    }
                    else if (obj2 is Line3)
                    {
                        Line3 l2 = (Line3)obj2;
                        return v1.IsParallelTo(l2.Direction);
                    }
                    else if (obj2 is Plane3)
                    {
                        Plane3 p2 = (Plane3)obj2;
                        return v1.IsPerpendicularTo(p2.Normal);
                    }
                    else
                    {
                        throw new SchemeRuntimeException("parallel?: argument 2 of type " + obj2.GetType() + " not supported");
                    }
                }
                else if (obj1 is Line3)
                {
                    Line3 l1 = (Line3)obj1;
                    if (obj2 is Vector3)
                    {
                        Vector3 v2 = (Vector3)obj2;
                        return l1.Direction.IsParallelTo(v2);
                    }
                    else if (obj2 is Line3)
                    {
                        Line3 l2 = (Line3)obj2;
                        return l1.IsParallelTo(l2);
                    }
                    else if (obj2 is Plane3)
                    {
                        Plane3 p2 = (Plane3)obj2;
                        return p2.IsParallelTo(l1);
                    }
                    else
                    {
                        throw new SchemeRuntimeException("parallel?: argument 2 of type " + obj2.GetType() + " not supported");
                    }
                }
                else if (obj1 is Plane3)
                {
                    Plane3 p1 = (Plane3)obj1;
                    if (obj2 is Vector3)
                    {
                        Vector3 v2 = (Vector3)obj2;
                        return p1.Normal.IsPerpendicularTo(v2);
                    }
                    else if (obj2 is Line3)
                    {
                        Line3 l2 = (Line3)obj2;
                        return p1.IsParallelTo(l2);
                    }
                    else if (obj2 is Plane3)
                    {
                        Plane3 p2 = (Plane3)obj2;
                        return p1.IsParallelTo(p2);
                    }
                    else
                    {
                        throw new SchemeRuntimeException("parallel?: argument 2 of type " + obj2.GetType() + " not supported");
                    }
                }
                else
                {
                    throw new SchemeRuntimeException("parallel?: argument 1 of type " + obj1.GetType() + " not supported");
                }
            }

            [SchemeFunction("coincident?")]
            public static bool Coincident(object obj1, object obj2)
            {
                if (obj1 is Line3)
                {
                    Line3 l1 = (Line3)obj1;
                    if (obj2 is Vertex3)
                    {
                        Vertex3 v2 = (Vertex3)obj2;
                        return l1.Contains(v2);
                    }
                    else if (obj2 is Line3)
                    {
                        Line3 l2 = (Line3)obj2;
                        return l1.IsCoincidentWith(l2);
                    }
                    else if (obj2 is Plane3)
                    {
                        Plane3 p2 = (Plane3)obj2;
                        return p2.Contains(l1);
                    }
                    else
                    {
                        throw new SchemeRuntimeException("coincident?: argument 2 of type " + obj2.GetType() + " not supported");
                    }
                }
                else if (obj1 is Plane3)
                {
                    Plane3 p1 = (Plane3)obj1;
                    if (obj2 is Vertex3)
                    {
                        Vertex3 v2 = (Vertex3)obj2;
                        return p1.Contains(v2);
                    }
                    else if (obj2 is Line3)
                    {
                        Line3 l2 = (Line3)obj2;
                        return p1.Contains(l2);
                    }
                    else if (obj2 is Plane3)
                    {
                        Plane3 p2 = (Plane3)obj2;
                        return p1.IsCoincidentWith(p2);
                    }
                    else
                    {
                        throw new SchemeRuntimeException("coincident?: argument 2 of type " + obj2.GetType() + " not supported");
                    }
                }
                else if (obj1 is Vertex3)
                {
                    Vertex3 v1 = (Vertex3)obj1;
                    if (obj2 is Vertex3)
                    {
                        Vertex3 v2 = (Vertex3)obj2;
                        return v1 == v2;
                    }
                    else if (obj2 is Line3)
                    {
                        Line3 l2 = (Line3)obj2;
                        return l2.Contains(v1);
                    }
                    else if (obj2 is Plane3)
                    {
                        Plane3 p2 = (Plane3)obj2;
                        return p2.Contains(v1);
                    }
                    else
                    {
                        throw new SchemeRuntimeException("coincident?: argument 2 of type " + obj2.GetType() + " not supported");
                    }
                }
                else
                {
                    throw new SchemeRuntimeException("coincident?: argument 1 of type " + obj1.GetType() + " not supported");
                }
            }

            [SchemeFunction("skew?")]
            public static bool Skew(object obj1, object obj2)
            {
                if (obj1 is Line3)
                {
                    Line3 l1 = (Line3)obj1;
                    if (obj2 is Line3)
                    {
                        Line3 l2 = (Line3)obj2;
                        return (!(l1.IsParallelTo(l2)) && !(l1.Intersects(l2)));
                    }
                    else if (obj2 is Plane3)
                    {
                        return false;
                    }
                    else
                    {
                        throw new SchemeRuntimeException("skew?: argument 2 of type " + obj2.GetType() + " not supported");
                    }
                }
                else if (obj1 is Plane3)
                {
                    if (obj2 is Line3)
                    {
                        return false;
                    }
                    else if (obj2 is Plane3)
                    {
                        return false;
                    }
                    else
                    {
                        throw new SchemeRuntimeException("skew?: argument 2 of type " + obj2.GetType() + " not supported");
                    }
                }
                else
                {
                    throw new SchemeRuntimeException("skew?: argument 1 of type " + obj1.GetType() + " not supported");
                }
            }

            [SchemeFunction("intersecting?")]
            public static bool Intersecting(object obj1, object obj2)
            {
                if (obj1 is Line3)
                {
                    Line3 l1 = (Line3)obj1;
                    if (obj2 is Line3)
                    {
                        Line3 l2 = (Line3)obj2;
                        return l1.Intersects(l2);
                    }
                    else if (obj2 is Plane3)
                    {
                        Plane3 p2 = (Plane3)obj2;
                        return !(p2.IsParallelTo(l1));
                    }
                    else
                    {
                        throw new SchemeRuntimeException("intersecting?: argument 2 of type " + obj2.GetType() + " not supported");
                    }
                }
                else if (obj1 is Plane3)
                {
                    Plane3 p1 = (Plane3)obj1;
                    if (obj2 is Line3)
                    {
                        Line3 l2 = (Line3)obj2;
                        return !(p1.IsParallelTo(l2));
                    }
                    else if (obj2 is Plane3)
                    {
                        Plane3 p2 = (Plane3)obj2;
                        return !(p1.IsParallelTo(p2));
                    }
                    else
                    {
                        throw new SchemeRuntimeException("intersecting?: argument 2 of type " + obj2.GetType() + " not supported");
                    }
                }
                else
                {
                    throw new SchemeRuntimeException("intersecting?: argument 1 of type " + obj1.GetType() + " not supported");
                }
            }

            [SchemeFunction("perpendicular?")]
            public static bool Perpendicular(object obj1, object obj2)
            {
                if (obj1 is Vector3)
                {
                    Vector3 v1 = (Vector3)obj1;
                    if (obj2 is Vector3)
                    {
                        Vector3 v2 = (Vector3)obj2;
                        return v1.IsPerpendicularTo(v2);
                    }
                    else if (obj2 is Line3)
                    {
                        Line3 l2 = (Line3)obj2;
                        return v1.IsPerpendicularTo(l2.Direction);
                    }
                    else if (obj2 is Plane3)
                    {
                        Plane3 p2 = (Plane3)obj2;
                        return v1.IsParallelTo(p2.Normal);
                    }
                    else
                    {
                        throw new SchemeRuntimeException("perpendicular?: argument 2 of type " + obj2.GetType() + " not supported");
                    }
                }
                else if (obj1 is Line3)
                {
                    Line3 l1 = (Line3)obj1;
                    if (obj2 is Vector3)
                    {
                        Vector3 v2 = (Vector3)obj2;
                        return l1.Direction.IsPerpendicularTo(v2);
                    }
                    else if (obj2 is Line3)
                    {
                        Line3 l2 = (Line3)obj2;
                        return l1.Direction.IsPerpendicularTo(l2.Direction);
                    }
                    else if (obj2 is Plane3)
                    {
                        Plane3 p2 = (Plane3)obj2;
                        return l1.Direction.IsParallelTo(p2.Normal);
                    }
                    else
                    {
                        throw new SchemeRuntimeException("perpendicular?: argument 2 of type " + obj2.GetType() + " not supported");
                    }
                }
                else if (obj1 is Plane3)
                {
                    Plane3 p1 = (Plane3)obj1;
                    if (obj2 is Vector3)
                    {
                        Vector3 v2 = (Vector3)obj2;
                        return p1.Normal.IsParallelTo(v2);
                    }
                    else if (obj2 is Line3)
                    {
                        Line3 l2 = (Line3)obj2;
                        return p1.Normal.IsParallelTo(l2.Direction);
                    }
                    else if (obj2 is Plane3)
                    {
                        Plane3 p2 = (Plane3)obj2;
                        return p1.Normal.IsPerpendicularTo(p2.Normal);
                    }
                    else
                    {
                        throw new SchemeRuntimeException("perpendicular?: argument 2 of type " + obj2.GetType() + " not supported");
                    }
                }
                else
                {
                    throw new SchemeRuntimeException("perpendicular?: argument 1 of type " + obj2.GetType() + " not supported");
                }
            }

            [SchemeFunction("nearest-point")]
            public static Vertex3 NearestPoint(object obj, Vertex3 pt)
            {
                if (obj is Line3)
                {
                    Line3 l = (Line3)obj;
                    return l.NearestPointTo(pt);
                }
                else if (obj is Plane3)
                {
                    Plane3 p = (Plane3)obj;
                    return p.NearestPointTo(pt);
                }
                else
                {
                    throw new SchemeRuntimeException("nearest-point: argument 1 of type " + obj.GetType() + " not supported");
                }
            }

            [SchemeFunction("convex-hull-point-status")]
            public static Symbol ConvexHullPointStatus(ConvexHull ch, Vertex3 pt)
            {
                PointStatus ps = ch.GetPointStatus(pt);
                return new Symbol(ps.ToString().ToLower());
            }
        }
    }
}
