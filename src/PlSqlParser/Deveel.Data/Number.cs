// 
//  Copyright 2014  Deveel
// 
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
// 
//        http://www.apache.org/licenses/LICENSE-2.0
// 
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.

using System;
using System.Globalization;

using Deveel.Math;

namespace Deveel.Data {
	[Serializable]
	public sealed class Number : IComparable<Number>, IComparable, IConvertible {
		private static readonly BigDecimal BdZero = new BigDecimal(0);

		public static readonly Number One = FromInt64(1L);
		public static readonly Number Zero = FromInt64(0L);

		public static readonly Number NegativeInfinity = new Number(NumberState.NegativeInfinity, null);
		public static readonly Number PositiveInfinity = new Number(NumberState.PositiveInfinity, null);
		public static readonly Number NaN = new Number(NumberState.NotANumber, null);

		private BigDecimal bigDecimal;

		private byte byteCount = 120;

		private long longRepresentation;
		private readonly NumberState numberState;

		internal Number(NumberState numberState, BigDecimal bigDecimal) {
			this.numberState = numberState;
			if (numberState == NumberState.None)
				SetBigDecimal(bigDecimal);
		}

		private Number(byte[] buf, int scale, NumberState state) {
			numberState = state;
			if (numberState == NumberState.None) {
				BigInteger bigint = new BigInteger(buf);
				SetBigDecimal(new BigDecimal(bigint, scale));
			}
		}

		public bool CanBeLong {
			get { return byteCount <= 8; }
		}

		public bool CanBeInt {
			get { return byteCount <= 4; }
		}

		public int Scale {
			get { return numberState == 0 ? bigDecimal.Scale : -1; }
		}

		public NumberState State {
			get { return numberState; }
		}

		private NumberState InverseState {
			get {
				if (numberState == NumberState.NegativeInfinity)
					return NumberState.PositiveInfinity;
				if (numberState == NumberState.PositiveInfinity)
					return NumberState.NegativeInfinity;
				return numberState;
			}
		}

		// Only call this from a constructor!
		private void SetBigDecimal(BigDecimal value) {
			bigDecimal = value;
			if (bigDecimal.Scale == 0) {
				BigInteger bint = value.ToBigInteger();
				int bitCount = bint.BitLength;
				if (bitCount < 30) {
					longRepresentation = bint.ToInt64();
					byteCount = 4;
				} else if (bitCount < 60) {
					longRepresentation = bint.ToInt64();
					byteCount = 8;
				}
			}
		}

		public byte[] ToByteArray() {
			return numberState == 0 ? bigDecimal.MovePointRight(bigDecimal.Scale).ToBigInteger().ToByteArray() : new byte[0];
		}

		public override string ToString() {
			switch (numberState) {
				case (NumberState.None):
					return bigDecimal.ToString();
				case (NumberState.NegativeInfinity):
					return "-Infinity";
				case (NumberState.PositiveInfinity):
					return "Infinity";
				case (NumberState.NotANumber):
					return "NaN";
				default:
					throw new ApplicationException("Unknown number state");
			}
		}

		public double ToDouble() {
			switch (numberState) {
				case (NumberState.None):
					return bigDecimal.ToDouble();
				case (NumberState.NegativeInfinity):
					return Double.NegativeInfinity;
				case (NumberState.PositiveInfinity):
					return Double.PositiveInfinity;
				case (NumberState.NotANumber):
					return Double.NaN;
				default:
					throw new ApplicationException("Unknown number state");
			}
		}

		public float ToSingle() {
			switch (numberState) {
				case (NumberState.None):
					return bigDecimal.ToSingle();
				case (NumberState.NegativeInfinity):
					return Single.NegativeInfinity;
				case (NumberState.PositiveInfinity):
					return Single.PositiveInfinity;
				case (NumberState.NotANumber):
					return Single.NaN;
				default:
					throw new ApplicationException("Unknown number state");
			}
		}

		public long ToInt64() {
			if (CanBeLong)
				return longRepresentation;
			switch (numberState) {
				case (NumberState.None):
					return bigDecimal.ToInt64();
				default:
					return (long)ToDouble();
			}
		}

		public int ToInt32() {
			if (CanBeLong)
				return (int)longRepresentation;
			switch (numberState) {
				case (NumberState.None):
					return bigDecimal.ToInt32();
				default:
					return (int)ToDouble();
			}
		}

		public short ToInt16() {
			return (short)ToInt32();
		}

		public byte ToByte() {
			return (byte)ToInt32();
		}

		public BigDecimal ToBigDecimal() {
			if (numberState != NumberState.None)
				throw new ArithmeticException("NaN, +Infinity or -Infinity can't be translated to a BigDecimal");

			return bigDecimal;
		}

		public bool ToBoolean() {
			int value = ToInt16();
			if (value == 0)
				return false;
			if (value == 1)
				return true;
			throw new InvalidCastException();
		}

		public int CompareTo(object obj) {
			return CompareTo((Number)obj);
		}

		public int CompareTo(Number number) {
			if (Equals(this, number))
				return 0;

			// If this is a non-infinity number
			if (numberState == 0) {
				// If both values can be represented by a long value
				if (CanBeLong && number.CanBeLong) {
					// Perform a long comparison check,
					if (longRepresentation > number.longRepresentation)
						return 1;
					if (longRepresentation < number.longRepresentation)
						return -1;
					return 0;
				}

				// And the compared number is non-infinity then use the BigDecimal
				// compareTo method.
				if (number.numberState == 0)
					return bigDecimal.CompareTo(number.bigDecimal);

				// Comparing a regular number with a NaN number.
				// If positive infinity or if NaN
				if (number.numberState == NumberState.PositiveInfinity ||
					number.numberState == NumberState.NotANumber) {
					return -1;
				}
				// If negative infinity
				if (number.numberState == NumberState.NegativeInfinity)
					return 1;
				throw new ApplicationException("Unknown number state.");
			}

			// This number is a NaN number.
			// Are we comparing with a regular number?
			if (number.numberState == 0) {
				// Yes, negative infinity
				if (numberState == NumberState.NegativeInfinity)
					return -1;

				// positive infinity or NaN
				if (numberState == NumberState.PositiveInfinity ||
					numberState == NumberState.NotANumber)
					return 1;

				throw new ApplicationException("Unknown number state.");
			}

			// Comparing NaN number with a NaN number.
			// This compares -Inf less than Inf and NaN and NaN greater than
			// Inf and -Inf.  -Inf < Inf < NaN
			return (numberState - number.numberState);
		}

		public override bool Equals(object obj) {
			Number other;
			if (obj is int) {
				other = FromInt32((int)obj);
			} else if (obj is long) {
				other = FromInt64((long)obj);
			} else if (obj is double) {
				other = FromDouble((double)obj);
			} else if (obj is float) {
				other = FromSingle((float)obj);
			} else if (obj is Number) {
				other = (Number)obj;
			} else {
				return false;
			}

			return numberState != NumberState.None
				? numberState == other.numberState
				: bigDecimal.Equals(other.bigDecimal);
		}

		public override int GetHashCode() {
			return bigDecimal.GetHashCode() ^ numberState.GetHashCode();
		}

		public Number BitWiseOr(Number number) {
			if (numberState == NumberState.None && Scale == 0 &&
				number.numberState == 0 && number.Scale == 0) {
				BigInteger bi1 = bigDecimal.ToBigInteger();
				BigInteger bi2 = number.bigDecimal.ToBigInteger();
				return new Number(NumberState.None, new BigDecimal(bi1.Or(bi2)));
			}
			return null;
		}

		public Number Add(Number number) {
			if (numberState == NumberState.None) {
				if (number.numberState == NumberState.None)
					return new Number(NumberState.None, bigDecimal.Add(number.bigDecimal));
				return new Number(number.numberState, null);
			}
			return new Number(numberState, null);
		}

		public Number Subtract(Number number) {
			if (numberState == NumberState.None) {
				if (number.numberState == NumberState.None)
					return new Number(NumberState.None, bigDecimal.Subtract(number.bigDecimal));
				return new Number(number.InverseState, null);
			}

			return new Number(numberState, null);
		}

		public Number Multiply(Number number) {
			if (numberState == NumberState.None) {
				if (number.numberState == 0)
					return new Number(NumberState.None, bigDecimal.Multiply(number.bigDecimal));
				return new Number(number.numberState, null);
			}
			return new Number(numberState, null);
		}

		public Number Divide(Number number) {
			if (numberState == 0) {
				if (number.numberState == 0) {
					BigDecimal divBy = number.bigDecimal;
					if (divBy.CompareTo(BdZero) != 0) {
						return new Number(NumberState.None, bigDecimal.Divide(divBy, 10, RoundingMode.HalfUp));
					}
				}
			}
			// Return NaN if we can't divide
			return new Number(NumberState.NotANumber, null);
		}

		public Number Modulus(Number number) {
			if (numberState == 0) {
				if (number.numberState == 0) {
					BigDecimal divBy = number.bigDecimal;
					if (divBy.CompareTo(BdZero) != 0) {
						BigDecimal remainder = bigDecimal.Remainder(divBy);
						return new Number(NumberState.None, remainder);
					}
				}
			}

			return new Number(NumberState.NotANumber, null);
		}

		public Number Abs() {
			if (numberState == 0)
				return new Number(NumberState.None, bigDecimal.Abs());
			if (numberState == NumberState.NegativeInfinity)
				return new Number(NumberState.PositiveInfinity, null);
			return new Number(numberState, null);
		}

		public int Signum() {
			if (numberState == 0)
				return bigDecimal.Signum();
			if (numberState == NumberState.NegativeInfinity)
				return -1;
			return 1;
		}

		public Number SetScale(int d, RoundingMode rounding) {
			if (numberState == NumberState.None)
				return new Number(NumberState.None, bigDecimal.SetScale(d, rounding));

			// Can't round -inf, +inf and NaN
			return this;
		}

		private static readonly BigDecimal SqrtDig = new BigDecimal(150);
		private static readonly BigDecimal SqrtPre = new BigDecimal(10).Pow(SqrtDig.ToInt32());

		private static BigDecimal SqrtNewtonRaphson(BigDecimal c, BigDecimal xn, BigDecimal precision) {
			BigDecimal fx = xn.Pow(2).Add(c.Negate());
			BigDecimal fpx = xn.Multiply(new BigDecimal(2));
			BigDecimal xn1 = fx.Divide(fpx, 2*SqrtDig.ToInt32(), RoundingMode.HalfDown);
			xn1 = xn.Add(xn1.Negate());
			//----
			BigDecimal currentSquare = xn1.Pow(2);
			BigDecimal currentPrecision = currentSquare.Subtract(c);
			currentPrecision = currentPrecision.Abs();
			if (currentPrecision.CompareTo(precision) <= -1) {
				return xn1;
			}

			return SqrtNewtonRaphson(c, xn1, precision);
		}

		public Number Sqrt() {
			var c =  SqrtNewtonRaphson(bigDecimal, new BigDecimal(1), new BigDecimal(1).Divide(SqrtPre));
			return new Number(numberState, c);
		}

		public Number Pow(int exp) {
			if (State == NumberState.NegativeInfinity)
				return NegativeInfinity;
			if (State == NumberState.PositiveInfinity)
				return PositiveInfinity;
			if (State == NumberState.NotANumber)
				return NaN;

			return new Number(State, bigDecimal.Pow(exp));
		}

		public static Number FromDouble(double value) {
			if (double.IsNegativeInfinity(value))
				return NegativeInfinity;
			if (double.IsPositiveInfinity(value))
				return PositiveInfinity;
			if (double.IsNaN(value))
				return NaN;

			return new Number(NumberState.None, BigDecimal.ValueOf(value));
		}

		public static Number FromSingle(float value) {
			if (float.IsNegativeInfinity(value))
				return NegativeInfinity;
			if (float.IsPositiveInfinity(value))
				return PositiveInfinity;
			if (float.IsNaN(value))
				return NaN;

			return new Number(NumberState.None, BigDecimal.ValueOf(value));
		}

		public static Number FromInt32(int value) {
			return new Number(NumberState.None, BigDecimal.ValueOf(value));
		}

		public static Number FromInt64(long value) {
			return new Number(NumberState.None, BigDecimal.ValueOf(value));
		}

		public static explicit operator Number(double value) {
			return FromDouble(value);
		}

		public static explicit operator Number(float value) {
			return FromSingle(value);
		}

		public static explicit operator Number(long value) {
			return FromInt64(value);
		}

		public static explicit operator Number(int value) {
			return FromInt32(value);
		}

		public static Number Parse(string str) {
			if (str.Equals("Infinity"))
				return PositiveInfinity;
			if (str.Equals("-Infinity"))
				return NegativeInfinity;
			if (str.Equals("NaN"))
				return NaN;

			return new Number(NumberState.None, new BigDecimal(str));
		}

		public static Number Create(byte[] bytes, int scale, NumberState state) {
			if (state == NumberState.None) {
				// This inlines common numbers to save a bit of memory.
				if (scale == 0 && bytes.Length == 1) {
					if (bytes[0] == 0)
						return Zero;
					if (bytes[0] == 1)
						return One;
				}
				return new Number(bytes, scale, state);
			}
			if (state == NumberState.NegativeInfinity)
				return NegativeInfinity;
			if (state == NumberState.PositiveInfinity)
				return PositiveInfinity;
			if (state == NumberState.NotANumber)
				return NaN;
			throw new ApplicationException("Unknown number state.");
		}

		TypeCode IConvertible.GetTypeCode() {
			if (CanBeInt)
				return TypeCode.Int32;
			if (CanBeLong)
				return TypeCode.Int64;
			return TypeCode.Object;
		}

		bool IConvertible.ToBoolean(IFormatProvider provider) {
			return ToBoolean();
		}

		char IConvertible.ToChar(IFormatProvider provider) {
			short value = ToInt16();
			if (value > Char.MaxValue || value < Char.MinValue)
				throw new InvalidCastException();
			return (char)value;
		}

		sbyte IConvertible.ToSByte(IFormatProvider provider) {
			throw new NotSupportedException();
		}

		byte IConvertible.ToByte(IFormatProvider provider) {
			return ToByte();
		}

		short IConvertible.ToInt16(IFormatProvider provider) {
			return ToInt16();
		}

		ushort IConvertible.ToUInt16(IFormatProvider provider) {
			throw new NotSupportedException();
		}

		int IConvertible.ToInt32(IFormatProvider provider) {
			return ToInt32();
		}

		uint IConvertible.ToUInt32(IFormatProvider provider) {
			throw new NotSupportedException();
		}

		long IConvertible.ToInt64(IFormatProvider provider) {
			return ToInt64();
		}

		ulong IConvertible.ToUInt64(IFormatProvider provider) {
			throw new NotSupportedException();
		}

		float IConvertible.ToSingle(IFormatProvider provider) {
			return ToSingle();
		}

		double IConvertible.ToDouble(IFormatProvider provider) {
			return ToDouble();
		}

		decimal IConvertible.ToDecimal(IFormatProvider provider) {
			throw new NotSupportedException();
		}

		DateTime IConvertible.ToDateTime(IFormatProvider provider) {
			throw new NotSupportedException();
		}

		string IConvertible.ToString(IFormatProvider provider) {
			return ToString();
		}

		object IConvertible.ToType(Type conversionType, IFormatProvider provider) {
			if (conversionType == typeof(bool))
				return (this as IConvertible).ToBoolean(provider);
			if (conversionType == typeof(byte))
				return ToByte();
			if (conversionType == typeof(short))
				return ToInt16();
			if (conversionType == typeof(int))
				return ToInt32();
			if (conversionType == typeof(long))
				return ToInt64();
			if (conversionType == typeof(float))
				return ToSingle();
			if (conversionType == typeof(double))
				return ToDouble();

			if (conversionType == typeof(BigDecimal))
				return ToBigDecimal();
			if (conversionType == typeof(byte[]))
				return ToByteArray();

			throw new NotSupportedException();
		}
	}
}