using System;

namespace ViTiet.DataStructure
{
    [System.Serializable]
    public class Data : IComparable<Data>
    {
        public Data() { Value = 0; }
        public Data(int value) { Value = value; }
        public Data(float value) { Value = value; }

        public float Value { get; set; }

        public int CompareTo(Data other)
        {
            // If other is not a valid object reference, this instance is greater.
            if (other == null) return 1;
            return Value.CompareTo(other.Value);
        }

        // Define the is greater than operator.
        public static bool operator >(Data lhs, Data rhs)
        {
            return lhs.CompareTo(rhs) == 1;
        }

        // Define the is less than operator.
        public static bool operator <(Data lhs, Data rhs)
        {
            return lhs.CompareTo(rhs) == -1;
        }

        // Define the is greater than or equal to operator.
        public static bool operator >=(Data lhs, Data rhs)
        {
            return lhs.CompareTo(rhs) >= 0;
        }

        // Define the is less than or equal to operator.
        public static bool operator <=(Data lhs, Data rhs)
        {
            return lhs.CompareTo(rhs) <= 0;
        }
    }
}