using System;
using System.Collections.Generic;

namespace ControlledWindowLib.Scheduling
{
    public struct SignalID : IEquatable<SignalID>, IComparable<SignalID>
    {
        public ulong id;

        public SignalID(ulong id)
        {
            this.id = id;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is SignalID)) return false;
            SignalID d = (SignalID)obj;
            return this.id == d.id;
        }

        public override int GetHashCode()
        {
            return id.GetHashCode();
        }

        public override string ToString()
        {
            return "(signal id: " + id.ToString() + ")";
        }

        public static bool operator ==(SignalID a, SignalID b)
        {
            return a.id == b.id;
        }

        public static bool operator !=(SignalID a, SignalID b)
        {
            return a.id != b.id;
        }

        public int CompareTo(SignalID other)
        {
            if (id < other.id) return -1;
            if (id > other.id) return 1;
            return 0;
        }

        public static bool operator <(SignalID a, SignalID b)
        {
            return a.id < b.id;
        }

        public static bool operator >(SignalID a, SignalID b)
        {
            return a.id > b.id;
        }

        public static bool operator <=(SignalID a, SignalID b)
        {
            return a.id <= b.id;
        }

        public static bool operator >=(SignalID a, SignalID b)
        {
            return a.id >= b.id;
        }

        public static explicit operator ulong(SignalID d)
        {
            return d.id;
        }

        public static explicit operator SignalID(ulong d)
        {
            return new SignalID(d);
        }

        public static SignalID operator ++(SignalID d)
        {
            return new SignalID(d.id + 1ul);
        }

        public bool Equals(SignalID other)
        {
            return this.id == other.id;
        }

        public static SignalID MinValue { get { return new SignalID(ulong.MinValue); } }

        public static SignalID MaxValue { get { return new SignalID(ulong.MaxValue); } }
    }

    internal struct TimerID : IEquatable<TimerID>, IComparable<TimerID>
    {
        public ushort id;

        public TimerID(ushort id)
        {
            this.id = id;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is TimerID)) return false;
            TimerID d = (TimerID)obj;
            return this.id == d.id;
        }

        public override int GetHashCode()
        {
            return id.GetHashCode();
        }

        public override string ToString()
        {
            return "(timer id: " + id.ToString() + ")";
        }

        public static bool operator ==(TimerID a, TimerID b)
        {
            return a.id == b.id;
        }

        public static bool operator !=(TimerID a, TimerID b)
        {
            return a.id != b.id;
        }

        public int CompareTo(TimerID other)
        {
            if (id < other.id) return -1;
            if (id > other.id) return 1;
            return 0;
        }

        public static bool operator <(TimerID a, TimerID b)
        {
            return a.id < b.id;
        }

        public static bool operator >(TimerID a, TimerID b)
        {
            return a.id > b.id;
        }

        public static bool operator <=(TimerID a, TimerID b)
        {
            return a.id <= b.id;
        }

        public static bool operator >=(TimerID a, TimerID b)
        {
            return a.id >= b.id;
        }

        public static explicit operator ushort(TimerID d)
        {
            return d.id;
        }

        public static explicit operator TimerID(ushort d)
        {
            return new TimerID(d);
        }

        public static TimerID operator ++(TimerID d)
        {
            return new TimerID((ushort)(d.id + 1u));
        }

        public bool Equals(TimerID other)
        {
            return this.id == other.id;
        }

        public static TimerID MinValue { get { return new TimerID(ushort.MinValue); } }

        public static TimerID MaxValue { get { return new TimerID(ushort.MaxValue); } }
    }

    public struct ObjectID : IEquatable<ObjectID>, IComparable<ObjectID>
    {
        public uint id;

        public ObjectID(uint id)
        {
            this.id = id;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is ObjectID)) return false;
            ObjectID d = (ObjectID)obj;
            return this.id == d.id;
        }

        public override int GetHashCode()
        {
            return id.GetHashCode();
        }

        public override string ToString()
        {
            return "(object id: " + id.ToString() + ")";
        }

        public static bool operator ==(ObjectID a, ObjectID b)
        {
            return a.id == b.id;
        }

        public static bool operator !=(ObjectID a, ObjectID b)
        {
            return a.id != b.id;
        }

        public int CompareTo(ObjectID other)
        {
            if (id < other.id) return -1;
            if (id > other.id) return 1;
            return 0;
        }

        public static bool operator <(ObjectID a, ObjectID b)
        {
            return a.id < b.id;
        }

        public static bool operator >(ObjectID a, ObjectID b)
        {
            return a.id > b.id;
        }

        public static bool operator <=(ObjectID a, ObjectID b)
        {
            return a.id <= b.id;
        }

        public static bool operator >=(ObjectID a, ObjectID b)
        {
            return a.id >= b.id;
        }

        public static explicit operator uint(ObjectID d)
        {
            return d.id;
        }

        public static explicit operator ObjectID(uint d)
        {
            return new ObjectID(d);
        }

        public static ObjectID operator ++(ObjectID d)
        {
            return new ObjectID(d.id + 1u);
        }

        public bool Equals(ObjectID other)
        {
            return this.id == other.id;
        }

        public static ObjectID MinValue { get { return new ObjectID(uint.MinValue); } }

        public static ObjectID MaxValue { get { return new ObjectID(uint.MaxValue); } }
    }

    internal struct WaitID : IEquatable<WaitID>, IComparable<WaitID>
    {
        public uint id;

        public WaitID(uint id)
        {
            this.id = id;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is WaitID)) return false;
            WaitID d = (WaitID)obj;
            return this.id == d.id;
        }

        public override int GetHashCode()
        {
            return id.GetHashCode();
        }

        public override string ToString()
        {
            return "(wait id: " + id.ToString() + ")";
        }

        public static bool operator ==(WaitID a, WaitID b)
        {
            return a.id == b.id;
        }

        public static bool operator !=(WaitID a, WaitID b)
        {
            return a.id != b.id;
        }

        public int CompareTo(WaitID other)
        {
            if (id < other.id) return -1;
            if (id > other.id) return 1;
            return 0;
        }

        public static bool operator <(WaitID a, WaitID b)
        {
            return a.id < b.id;
        }

        public static bool operator >(WaitID a, WaitID b)
        {
            return a.id > b.id;
        }

        public static bool operator <=(WaitID a, WaitID b)
        {
            return a.id <= b.id;
        }

        public static bool operator >=(WaitID a, WaitID b)
        {
            return a.id >= b.id;
        }

        public static explicit operator uint(WaitID d)
        {
            return d.id;
        }

        public static explicit operator WaitID(uint d)
        {
            return new WaitID(d);
        }

        public static WaitID operator ++(WaitID d)
        {
            return new WaitID(d.id + 1u);
        }

        public bool Equals(WaitID other)
        {
            return this.id == other.id;
        }

        public static WaitID MinValue { get { return new WaitID(uint.MinValue); } }

        public static WaitID MaxValue { get { return new WaitID(uint.MaxValue); } }
    }

}

