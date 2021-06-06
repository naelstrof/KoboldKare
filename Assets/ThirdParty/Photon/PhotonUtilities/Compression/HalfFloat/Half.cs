/*
* Copyright (c) 2007-2010 SlimMath Group
*
* Permission is hereby granted, free of charge, to any person obtaining a copy
* of this software and associated documentation files (the "Software"), to deal
* in the Software without restriction, including without limitation the rights
* to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
* copies of the Software, and to permit persons to whom the Software is
* furnished to do so, subject to the following conditions:
*
* The above copyright notice and this permission notice shall be included in
* all copies or substantial portions of the Software.
*
* THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
* IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
* FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
* AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
* LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
* OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
* THE SOFTWARE.
*/

using System;
using System.Globalization;
using System.Runtime.InteropServices;
//using System.ComponentModel;

namespace Photon.Compression.HalfFloat
{
	/// <summary>
	/// A half precision (16 bit) floating point value.
	/// </summary>
	[Serializable]
	[StructLayout(LayoutKind.Sequential)]
	public struct Half : IConvertible, IComparable, IComparable<Half>, IEquatable<Half>, IFormattable
	{
		private ushort value;

		/// <summary>
		/// Number of decimal digits of precision.
		/// </summary>
		public const int PrecisionDigits = 3;

		/// <summary>
		/// Number of bits in the mantissa.
		/// </summary>
		public const int MantissaBits = 11;

		/// <summary>
		/// Maximum decimal exponent.
		/// </summary>
		public const int MaximumDecimalExponent = 4;

		/// <summary>
		/// Maximum binary exponent.
		/// </summary>
		public const int MaximumBinaryExponent = 15;

		/// <summary>
		/// Minimum decimal exponent.
		/// </summary>
		public const int MinimumDecimalExponent = -4;

		/// <summary>
		/// Minimum binary exponent.
		/// </summary>
		public const int MinimumBinaryExponent = -14;

		/// <summary>
		/// Exponent radix.
		/// </summary>
		public const int ExponentRadix = 2;

		/// <summary>
		/// Additional rounding.
		/// </summary>
		public const int AdditionRounding = 1;

		/// <summary>
		/// Smallest value such that 1.0 + epsilon != 1.0
		/// </summary>
		public static readonly Half Epsilon = new Half(0.0004887581f);

		/// <summary>
		/// Maximum value of the number.
		/// </summary>
		public static readonly Half MaxValue = new Half(65504f);

		/// <summary>
		/// Minimum value of the number.
		/// </summary>
		public static readonly Half MinValue = new Half(6.103516E-05f);

		/// <summary>
		/// Represents not a number (NaN).
		/// </summary>
		public static readonly Half NaN = new Half(0.0f / 0.0f);

		/// <summary>
		/// Represents negative infinity.
		/// </summary>
		public static readonly Half NegativeInfinity = new Half(-1.0f / 0.0f);

		/// <summary>
		/// Represents positive infinity.
		/// </summary>
		public static readonly Half PositiveInfinity = new Half(1.0f / 0.0f);

		/// <summary>
		/// Initializes a new instance of the <see cref="T:SlimMath.Half"/> structure.
		/// </summary>
		/// <param name="value">The floating point value that should be stored in 16 bit format.</param>
		public Half(float value)
		{
			this.value = HalfUtilities.Pack(value);
		}

		/// <summary>
		/// Gets the raw 16 bit value used to back this half-float.
		/// </summary>
		//[CLSCompliant(false)]
		public ushort RawValue
		{
			get { return value; }
		}

		/// <summary>
		/// Converts an array of half precision values into full precision values.
		/// </summary>
		/// <param name="values">The values to be converted.</param>
		/// <returns>An array of converted values.</returns>
		public static float[] ConvertToFloat(Half[] values)
		{
			float[] results = new float[values.Length];

			for (int i = 0; i < results.Length; i++)
				results[i] = HalfUtilities.Unpack(values[i].RawValue);

			return results;
		}

		/// <summary>
		/// Converts an array of full precision values into half precision values.
		/// </summary>
		/// <param name="values">The values to be converted.</param>
		/// <returns>An array of converted values.</returns>
		public static Half[] ConvertToHalf(float[] values)
		{
			Half[] results = new Half[values.Length];

			for (int i = 0; i < results.Length; i++)
				results[i] = new Half(values[i]);

			return results;
		}

		/// <summary>
		/// Returns a value indicating whether the specified number evaluates to negative or positive infinity.
		/// </summary>
		/// <returns><c>true</c> if <paramref name="half"/> evaluates to positive or negative infinity; otherwise, <c>false</c>.</returns>
		public static bool IsInfinity(Half half)
		{
			return (half == PositiveInfinity) || (half == NegativeInfinity);
		}

		/// <summary>
		/// Returns a value indicating whether the specified number evaluates to NaN.
		/// </summary>
		/// <returns><c>true</c> if <paramref name="half"/> evaluates to NaN; otherwise, <c>false</c>.</returns>
		public static bool IsNaN(Half half)
		{
			return half == NaN;
		}

		/// <summary>
		/// Returns a value indicating whether the specified number evaluates to negative infinity.
		/// </summary>
		/// <returns><c>true</c> if <paramref name="half"/> evaluates to negative infinity; otherwise, <c>false</c>.</returns>
		public static bool IsNegativeInfinity(Half half)
		{
			return half == NegativeInfinity;
		}

		/// <summary>
		/// Returns a value indicating whether the specified number evaluates to positive infinity.
		/// </summary>
		/// <returns><c>true</c> if <paramref name="half"/> evaluates to positive infinity; otherwise, <c>false</c>.</returns>
		public static bool IsPositiveInfinity(Half half)
		{
			return half == PositiveInfinity;
		}

		/// <summary>
		/// Returns a value that indicates whether a specified value is
		/// less than another specified value.
		/// </summary>
		/// <param name="left">The first value to compare.</param>
		/// <param name="right">The second value to compare.</param>
		/// <returns><c>true</c> if <paramref name="left"/> is less than <paramref name="right"/>; otherwise, <c>false</c>.</returns>
		public static bool operator <(Half left, Half right)
		{
			return (float)left < (float)right;
		}

		/// <summary>
		/// Returns a value that indicates whether a specified value is
		/// greater than another specified value.
		/// </summary>
		/// <param name="left">The first value to compare.</param>
		/// <param name="right">The second value to compare.</param>
		/// <returns><c>true</c> if <paramref name="left"/> is greater than <paramref name="right"/>; otherwise, <c>false</c>.</returns>
		public static bool operator >(Half left, Half right)
		{
			return (float)left > (float)right;
		}

		/// <summary>
		/// Returns a value that indicates whether a specified value is
		/// less than or equal to another specified value.
		/// </summary>
		/// <param name="left">The first value to compare.</param>
		/// <param name="right">The second value to compare.</param>
		/// <returns><c>true</c> if <paramref name="left"/> is less than or equal to <paramref name="right"/>; otherwise, <c>false</c>.</returns>
		public static bool operator <=(Half left, Half right)
		{
			return (float)left <= (float)right;
		}

		/// <summary>
		/// Returns a value that indicates whether a specified value is
		/// greater than or equal to another specified value.
		/// </summary>
		/// <param name="left">The first value to compare.</param>
		/// <param name="right">The second value to compare.</param>
		/// <returns><c>true</c> if <paramref name="left"/> is greater than or equal to right; otherwise, <c>false</c>.</returns>
		public static bool operator >=(Half left, Half right)
		{
			return (float)left >= (float)right;
		}

		/// <summary>
		/// Tests for equality between two objects.
		/// </summary>
		/// <param name="left">The first value to compare.</param>
		/// <param name="right">The second value to compare.</param>
		/// <returns>
		/// <c>true</c> if <paramref name="left"/> has the same value as <paramref name="right"/>; otherwise, <c>false</c>.</returns>
		public static bool operator ==(Half left, Half right)
		{
			return left.Equals(right);
		}

		/// <summary>
		/// Tests for inequality between two objects.
		/// </summary>
		/// <param name="left">The first value to compare.</param>
		/// <param name="right">The second value to compare.</param>
		/// <returns>
		/// <c>true</c> if <paramref name="left"/> has a different value than <paramref name="right"/>; otherwise, <c>false</c>.</returns>
		public static bool operator !=(Half left, Half right)
		{
			return !left.Equals(right);
		}

		/// <summary>
		/// Performs an explicit conversion from <see cref="T:System.Single"/> to <see cref="T:SlimMath.Half"/>.
		/// </summary>
		/// <param name="value">The value to be converted.</param>
		/// <returns>The converted value.</returns>
		public static explicit operator Half(float value)
		{
			return new Half(value);
		}

		/// <summary>
		/// Performs an implicit conversion from <see cref="T:SlimMath.Half"/> to <see cref="T:System.Single"/>.
		/// </summary>
		/// <param name="value">The value to be converted.</param>
		/// <returns>The converted value.</returns>
		public static implicit operator float(Half value)
		{
			return HalfUtilities.Unpack(value.value);
		}

		/// <summary>
		/// Converts the value of the object to its equivalent string representation.
		/// </summary>
		/// <returns>The string representation of the value of this instance.</returns>
		public override string ToString()
		{
			return string.Format(CultureInfo.CurrentCulture, ((float)this).ToString());
		}

		/// <summary>
		/// Returns a <see cref="System.String"/> that represents this instance.
		/// </summary>
		/// <param name="format">The format.</param>
		/// <returns>
		/// A <see cref="System.String"/> that represents this instance.
		/// </returns>
		public string ToString(string format)
		{
			if (format == null)
				return ToString();

			return string.Format(CultureInfo.CurrentCulture, ((float)this).ToString(format, CultureInfo.CurrentCulture));
		}

		/// <summary>
		/// Returns a <see cref="System.String"/> that represents this instance.
		/// </summary>
		/// <param name="formatProvider">The format provider.</param>
		/// <returns>
		/// A <see cref="System.String"/> that represents this instance.
		/// </returns>
		public string ToString(IFormatProvider formatProvider)
		{
			return string.Format(formatProvider, ((float)this).ToString());
		}

		/// <summary>
		/// Returns a <see cref="System.String"/> that represents this instance.
		/// </summary>
		/// <param name="format">The format.</param>
		/// <param name="formatProvider">The format provider.</param>
		/// <returns>
		/// A <see cref="System.String"/> that represents this instance.
		/// </returns>
		public string ToString(string format, IFormatProvider formatProvider)
		{
			if (format == null)
				ToString(formatProvider);

			return string.Format(formatProvider, ((float)this).ToString(format, formatProvider));
		}

		/// <summary>
		/// Returns the hash code for this instance.
		/// </summary>
		/// <returns>A 32-bit signed integer hash code.</returns>
		public override int GetHashCode()
		{
			return (((value * 3) / 2) ^ value);
		}

		/// <summary>
		/// Compares this instance to another instance of the same type.
		/// </summary>
		/// <param name="value">The other value to compare to this instance.</param>
		/// <returns>A value that specifies if this instance is greater than, less than, or equal to <paramref name="value"/>.</returns>
		public int CompareTo(Half value)
		{
			if (this < value)
			{
				return -1;
			}

			if (this > value)
			{
				return 1;
			}

			if (this != value)
			{
				if (!IsNaN(this))
				{
					return 1;
				}

				if (!IsNaN(value))
				{
					return -1;
				}
			}

			return 0;
		}

		/// <summary>
		/// Compares this instance to another instance of another type.
		/// </summary>
		/// <param name="value">The other value to compare to this instance.</param>
		/// <returns>A value that specifies if this instance is greater than, less than, or equal to <paramref name="value"/>.</returns>
		public int CompareTo(object value)
		{
			if (value == null)
			{
				return 1;
			}

			if (!(value is Half))
			{
				throw new ArgumentException("The argument value must be a SlimMath.Half.");
			}

			Half f = (Half)value;
			if (this < f)
			{
				return -1;
			}

			if (this > f)
			{
				return 1;
			}

			if (this != f)
			{
				if (!IsNaN(this))
				{
					return 1;
				}

				if (!IsNaN(f))
				{
					return -1;
				}
			}

			return 0;
		}

        /// <summary>
        /// Determines whether the specified object instances are considered equal.
        /// </summary>
        /// <param name="value1"></param>
        /// <param name="value2"></param>
        /// <returns>
        /// <c>true</c> if <paramref name="value1"/> is the same instance as <paramref name="value2"/> or 
        /// if both are <c>null</c> references or if <c>value1.Equals(value2)</c> returns <c>true</c>; otherwise, <c>false</c>.
        /// </returns>
		public static bool Equals(ref Half value1, ref Half value2)
		{
			return value1.value == value2.value;
		}

		/// <summary>
		/// Returns a value that indicates whether the current instance is equal to the specified object.
		/// </summary>
		/// <param name="other">Object to make the comparison with.</param>
		/// <returns>
		/// <c>true</c> if the current instance is equal to the specified object; <c>false</c> otherwise.</returns>
		public bool Equals(Half other)
		{
			return other.value == value;
		}

		/// <summary>
		/// Returns a value that indicates whether the current instance is equal to a specified object.
		/// </summary>
		/// <param name="obj">Object to make the comparison with.</param>
		/// <returns>
		/// <c>true</c> if the current instance is equal to the specified object; <c>false</c> otherwise.</returns>
		public override bool Equals(object obj)
		{
			if (obj == null)
				return false;

			if (obj.GetType() != GetType())
				return false;

			return Equals((Half)obj);
		}

		/// <summary>
		/// Returns the <see cref="System.TypeCode"/> for value type.
		/// </summary>
		/// <returns>The type code.</returns>
		public TypeCode GetTypeCode()
		{
			return Type.GetTypeCode(typeof(Half));
		}

		bool IConvertible.ToBoolean(IFormatProvider provider)
		{
			return Convert.ToBoolean(this);
		}

		byte IConvertible.ToByte(IFormatProvider provider)
		{
			return Convert.ToByte(this);
		}

		char IConvertible.ToChar(IFormatProvider provider)
		{
			throw new InvalidCastException("Invalid cast from SlimMath.Half to System.Char.");
		}

		DateTime IConvertible.ToDateTime(IFormatProvider provider)
		{
			throw new InvalidCastException("Invalid cast from SlimMath.Half to System.DateTime.");
		}

		decimal IConvertible.ToDecimal(IFormatProvider provider)
		{
			return Convert.ToDecimal(this);
		}

		double IConvertible.ToDouble(IFormatProvider provider)
		{
			return Convert.ToDouble(this);
		}

		short IConvertible.ToInt16(IFormatProvider provider)
		{
			return Convert.ToInt16(this);
		}

		int IConvertible.ToInt32(IFormatProvider provider)
		{
			return Convert.ToInt32(this);
		}

		long IConvertible.ToInt64(IFormatProvider provider)
		{
			return Convert.ToInt64(this);
		}

		sbyte IConvertible.ToSByte(IFormatProvider provider)
		{
			return Convert.ToSByte(this);
		}

		float IConvertible.ToSingle(IFormatProvider provider)
		{
			return this;
		}

		object IConvertible.ToType(Type type, IFormatProvider provider)
		{
			return ((IConvertible)((float)this)).ToType(type, provider);
		}

		ushort IConvertible.ToUInt16(IFormatProvider provider)
		{
			return Convert.ToUInt16(this);
		}

		uint IConvertible.ToUInt32(IFormatProvider provider)
		{
			return Convert.ToUInt32(this);
		}

		ulong IConvertible.ToUInt64(IFormatProvider provider)
		{
			return Convert.ToUInt64(this);
		}
	}
}