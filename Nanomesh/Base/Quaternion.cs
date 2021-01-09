using System;
using System.Runtime.InteropServices;

namespace Nanomesh
{
    [StructLayout(LayoutKind.Sequential)]
    public partial struct Quaternion : IEquatable<Quaternion>
    {
		const double radToDeg = 180.0 / Math.PI;
		const double degToRad = Math.PI / 180.0;

		public const double kEpsilon = 1E-20; // should probably be used in the 0 tests in LookRotation or Slerp

		public Vector3 xyz
		{
			set
			{
				x = value.x;
				y = value.y;
				z = value.z;
			}
			get
			{
				return new Vector3(x, y, z);
			}
		}

		public double x;

		public double y;

		public double z;

		public double w;
		
		public double this[int index]
		{
			get
			{
				switch (index)
				{
					case 0:
						return this.x;
					case 1:
						return this.y;
					case 2:
						return this.z;
					case 3:
						return this.w;
					default:
						throw new IndexOutOfRangeException("Invalid Quaternion index: " + index + ", can use only 0,1,2,3");
				}
			}
			set
			{
				switch (index)
				{
					case 0:
						this.x = value;
						break;
					case 1:
						this.y = value;
						break;
					case 2:
						this.z = value;
						break;
					case 3:
						this.w = value;
						break;
					default:
						throw new IndexOutOfRangeException("Invalid Quaternion index: " + index + ", can use only 0,1,2,3");
				}
			}
		}
		/// <summary>
		///   <para>The identity rotation (RO).</para>
		/// </summary>
		public static Quaternion identity
		{
			get
			{
				return new Quaternion(0, 0, 0, 1);
			}
		}

		/// <summary>
		/// Gets the length (magnitude) of the quaternion.
		/// </summary>
		/// <seealso cref="LengthSquared"/>
		public double Length
		{
			get
			{
				return (double)System.Math.Sqrt(x * x + y * y + z * z + w * w);
			}
		}

		/// <summary>
		/// Gets the square of the quaternion length (magnitude).
		/// </summary>
		public double LengthSquared
		{
			get
			{
				return x * x + y * y + z * z + w * w;
			}
		}

		/// <summary>
		///   <para>Constructs new Quaternion with given x,y,z,w components.</para>
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <param name="z"></param>
		/// <param name="w"></param>
		public Quaternion(double x, double y, double z, double w)
		{
			this.x = x;
			this.y = y;
			this.z = z;
			this.w = w;
		}

		/// <summary>
		/// Construct a new Quaternion from vector and w components
		/// </summary>
		/// <param name="v">The vector part</param>
		/// <param name="w">The w part</param>
		public Quaternion(Vector3 v, double w)
		{
			this.x = v.x;
			this.y = v.y;
			this.z = v.z;
			this.w = w;
		}

		/// <summary>
		///   <para>Set x, y, z and w components of an existing Quaternion.</para>
		/// </summary>
		/// <param name="new_x"></param>
		/// <param name="new_y"></param>
		/// <param name="new_z"></param>
		/// <param name="new_w"></param>
		public void Set(double new_x, double new_y, double new_z, double new_w)
		{
			this.x = new_x;
			this.y = new_y;
			this.z = new_z;
			this.w = new_w;
		}

		/// <summary>
		/// Scales the Quaternion to unit length.
		/// </summary>
		public static Quaternion Normalize(Quaternion q)
		{
			double mag = Math.Sqrt(Dot(q, q));

			if (mag < kEpsilon)
				return Quaternion.identity;

			return new Quaternion(q.x / mag, q.y / mag, q.z / mag, q.w / mag);
		}

		/// <summary>
		/// Scale the given quaternion to unit length
		/// </summary>
		/// <param name="q">The quaternion to normalize</param>
		/// <param name="result">The normalized quaternion</param>
		public void Normalize()
		{
			this = Normalize(this);
		}

		/// <summary>
		///   <para>The dot product between two rotations.</para>
		/// </summary>
		/// <param name="a"></param>
		/// <param name="b"></param>
		public static double Dot(Quaternion a, Quaternion b)
		{
			return a.x * b.x + a.y * b.y + a.z * b.z + a.w * b.w;
		}

		/// <summary>
		///   <para>Creates a rotation which rotates /angle/ degrees around /axis/.</para>
		/// </summary>
		/// <param name="angle"></param>
		/// <param name="axis"></param>
		public static Quaternion AngleAxis(double angle, Vector3 axis)
		{
			return Quaternion.AngleAxis(angle, ref axis);
		}

		private static Quaternion AngleAxis(double degress, ref Vector3 axis)
		{
			if (axis.LengthSquared == 0.0)
				return identity;

			Quaternion result = identity;
			var radians = degress * degToRad;
			radians *= 0.5;
			axis = axis.Normalized;
			axis = axis * Math.Sin(radians);
			result.x = axis.x;
			result.y = axis.y;
			result.z = axis.z;
			result.w = Math.Cos(radians);

			return Normalize(result);
		}

		public void ToAngleAxis(out double angle, out Vector3 axis)
		{
			Quaternion.ToAxisAngleRad(this, out axis, out angle);
			angle *= radToDeg;
		}

		/// <summary>
		///   <para>Creates a rotation which rotates from /fromDirection/ to /toDirection/.</para>
		/// </summary>
		/// <param name="fromDirection"></param>
		/// <param name="toDirection"></param>
		public static Quaternion FromToRotation(Vector3 fromDirection, Vector3 toDirection)
		{
			Vector3 xyz = Vector3.Cross(fromDirection, toDirection);
			double w = Math.Sqrt((fromDirection.LengthSquared) * (toDirection.LengthSquared)) + Vector3.Dot(fromDirection, toDirection);
			Quaternion q = new Quaternion(xyz, w);

			return q;

			return RotateTowards(LookRotation(fromDirection), LookRotation(toDirection), double.MaxValue);
		}

		/// <summary>
		///   <para>Creates a rotation which rotates from /fromDirection/ to /toDirection/.</para>
		/// </summary>
		/// <param name="fromDirection"></param>
		/// <param name="toDirection"></param>
		public void SetFromToRotation(Vector3 fromDirection, Vector3 toDirection)
		{
			this = Quaternion.FromToRotation(fromDirection, toDirection);
		}

		/// <summary>
		///   <para>Creates a rotation with the specified /forward/ and /upwards/ directions.</para>
		/// </summary>
		/// <param name="forward">The direction to look in.</param>
		/// <param name="upwards">The vector that defines in which direction up is.</param>
		public static Quaternion LookRotation(Vector3 forward, Vector3 upwards)
		{
			return Quaternion.LookRotation(ref forward, ref upwards);
		}

		public static Quaternion LookRotation(Vector3 forward)
		{
			Vector3 up = new Vector3(1, 0, 0);
			return Quaternion.LookRotation(ref forward, ref up);
		}

		private static Quaternion LookRotation(ref Vector3 forward, ref Vector3 up)
		{
			forward = Vector3.Normalize(forward);
			Vector3 right = Vector3.Normalize(Vector3.Cross(up, forward));
			up = Vector3.Cross(forward, right);
			var m00 = right.x;
			var m01 = right.y;
			var m02 = right.z;
			var m10 = up.x;
			var m11 = up.y;
			var m12 = up.z;
			var m20 = forward.x;
			var m21 = forward.y;
			var m22 = forward.z;

			double num8 = (m00 + m11) + m22;
			var quaternion = new Quaternion();
			if (num8 > 0)
			{
				var num = Math.Sqrt(num8 + 1);
				quaternion.w = num * 0.5;
				num = 0.5 / num;
				quaternion.x = (m12 - m21) * num;
				quaternion.y = (m20 - m02) * num;
				quaternion.z = (m01 - m10) * num;
				return quaternion;
			}
			if ((m00 >= m11) && (m00 >= m22))
			{
				var num7 = Math.Sqrt(((1 + m00) - m11) - m22);
				var num4 = 0.5 / num7;
				quaternion.x = 0.5 * num7;
				quaternion.y = (m01 + m10) * num4;
				quaternion.z = (m02 + m20) * num4;
				quaternion.w = (m12 - m21) * num4;
				return quaternion;
			}
			if (m11 > m22)
			{
				var num6 = Math.Sqrt(((1 + m11) - m00) - m22);
				var num3 = 0.5 / num6;
				quaternion.x = (m10 + m01) * num3;
				quaternion.y = 0.5 * num6;
				quaternion.z = (m21 + m12) * num3;
				quaternion.w = (m20 - m02) * num3;
				return quaternion;
			}
			var num5 = Math.Sqrt(((1 + m22) - m00) - m11);
			var num2 = 0.5 / num5;
			quaternion.x = (m20 + m02) * num2;
			quaternion.y = (m21 + m12) * num2;
			quaternion.z = 0.5 * num5;
			quaternion.w = (m01 - m10) * num2;
			return quaternion;
		}

		public void SetLookRotation(Vector3 view)
		{
			Vector3 up = new Vector3(1, 0, 0);
			this.SetLookRotation(view, up);
		}

		/// <summary>
		///   <para>Creates a rotation with the specified /forward/ and /upwards/ directions.</para>
		/// </summary>
		/// <param name="view">The direction to look in.</param>
		/// <param name="up">The vector that defines in which direction up is.</param>
		public void SetLookRotation(Vector3 view, Vector3 up)
		{
			this = Quaternion.LookRotation(view, up);
		}

		/// <summary>
		///   <para>Spherically interpolates between /a/ and /b/ by t. The parameter /t/ is clamped to the range [0, 1].</para>
		/// </summary>
		/// <param name="a"></param>
		/// <param name="b"></param>
		/// <param name="t"></param>
		public static Quaternion Slerp(Quaternion a, Quaternion b, double t)
		{
			return Quaternion.Slerp(ref a, ref b, t);
		}

		private static Quaternion Slerp(ref Quaternion a, ref Quaternion b, double t)
		{
			if (t > 1) t = 1;
			if (t < 0) t = 0;
			return SlerpUnclamped(ref a, ref b, t);
		}

		/// <summary>
		///   <para>Spherically interpolates between /a/ and /b/ by t. The parameter /t/ is not clamped.</para>
		/// </summary>
		/// <param name="a"></param>
		/// <param name="b"></param>
		/// <param name="t"></param>
		public static Quaternion SlerpUnclamped(Quaternion a, Quaternion b, double t)
		{
			
			return Quaternion.SlerpUnclamped(ref a, ref b, t);
		}
		private static Quaternion SlerpUnclamped(ref Quaternion a, ref Quaternion b, double t)
		{
			// if either input is zero, return the other.
			if (a.LengthSquared == 0.0)
			{
				if (b.LengthSquared == 0.0)
				{
					return identity;
				}
				return b;
			}
			else if (b.LengthSquared == 0.0)
			{
				return a;
			}

			double cosHalfAngle = a.w * b.w + Vector3.Dot(a.xyz, b.xyz);

			if (cosHalfAngle >= 1.0 || cosHalfAngle <= -1.0)
			{
				// angle = 0.0f, so just return one input.
				return a;
			}
			else if (cosHalfAngle < 0.0)
			{
				b.xyz = -b.xyz;
				b.w = -b.w;
				cosHalfAngle = -cosHalfAngle;
			}

			double blendA;
			double blendB;
			if (cosHalfAngle < 0.99)
			{
				// do proper slerp for big angles
				double halfAngle = Math.Acos(cosHalfAngle);
				double sinHalfAngle = Math.Sin(halfAngle);
				double oneOverSinHalfAngle = 1.0 / sinHalfAngle;
				blendA = Math.Sin(halfAngle * (1.0 - t)) * oneOverSinHalfAngle;
				blendB = Math.Sin(halfAngle * t) * oneOverSinHalfAngle;
			}
			else
			{
				// do lerp if angle is really small.
				blendA = 1.0f - t;
				blendB = t;
			}

			Quaternion result = new Quaternion(blendA * a.xyz + blendB * b.xyz, blendA * a.w + blendB * b.w);
			if (result.LengthSquared > 0.0)
				return Normalize(result);
			else
				return identity;
		}

		/// <summary>
		///   <para>Interpolates between /a/ and /b/ by /t/ and normalizes the result afterwards. The parameter /t/ is clamped to the range [0, 1].</para>
		/// </summary>
		/// <param name="a"></param>
		/// <param name="b"></param>
		/// <param name="t"></param>
		public static Quaternion Lerp(Quaternion a, Quaternion b, double t)
		{
			if (t > 1) t = 1;
			if (t < 0) t = 0;
			return Slerp(ref a, ref b, t); // TODO: use lerp not slerp, "Because quaternion works in 4D. Rotation in 4D are linear" ???
		}

		/// <summary>
		///   <para>Interpolates between /a/ and /b/ by /t/ and normalizes the result afterwards. The parameter /t/ is not clamped.</para>
		/// </summary>
		/// <param name="a"></param>
		/// <param name="b"></param>
		/// <param name="t"></param>
		public static Quaternion LerpUnclamped(Quaternion a, Quaternion b, double t)
		{
			return Slerp(ref a, ref b, t);
		}

		/// <summary>
		///   <para>Rotates a rotation /from/ towards /to/.</para>
		/// </summary>
		/// <param name="from"></param>
		/// <param name="to"></param>
		/// <param name="maxDegreesDelta"></param>
		public static Quaternion RotateTowards(Quaternion from, Quaternion to, double maxDegreesDelta)
		{
			double num = Quaternion.Angle(from, to);
			if (num == 0)
			{
				return to;
			}
			double t = Math.Min(1, maxDegreesDelta / num);
			return Quaternion.SlerpUnclamped(from, to, t);
		}

		/// <summary>
		///   <para>Returns the Inverse of /rotation/.</para>
		/// </summary>
		/// <param name="rotation"></param>
		public static Quaternion Inverse(Quaternion rotation)
		{
			double lengthSq = rotation.LengthSquared;
			if (lengthSq != 0.0)
			{
				double i = 1.0 / lengthSq;
				return new Quaternion(rotation.xyz * -i, rotation.w * i);
			}
			return rotation;
		}

		/// <summary>
		///   <para>Returns a nicely formatted string of the Quaternion.</para>
		/// </summary>
		/// <param name="format"></param>
		public override string ToString()
		{
			return $"{this.x}, {this.y}, {this.z}, {this.w}";
		}

		/// <summary>
		///   <para>Returns a nicely formatted string of the Quaternion.</para>
		/// </summary>
		/// <param name="format"></param>
		public string ToString(string format)
		{
			return string.Format("({0}, {1}, {2}, {3})", this.x.ToString(format), this.y.ToString(format), this.z.ToString(format), this.w.ToString(format));
		}

		/// <summary>
		///   <para>Returns the angle in degrees between two rotations /a/ and /b/.</para>
		/// </summary>
		/// <param name="a"></param>
		/// <param name="b"></param>
		public static double Angle(Quaternion a, Quaternion b)
		{
			double f = Quaternion.Dot(a, b);
			return Math.Acos(Math.Min(Math.Abs(f), 1)) * 2 * radToDeg;
		}

		/// <summary>
		///   <para>Returns a rotation that rotates z degrees around the z axis, x degrees around the x axis, and y degrees around the y axis (in that order).</para>
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <param name="z"></param>
		public static Quaternion Euler(double x, double y, double z)
		{
			return Quaternion.FromEulerRad(new Vector3((double)x, (double)y, (double)z) * degToRad);
		}

		/// <summary>
		///   <para>Returns a rotation that rotates z degrees around the z axis, x degrees around the x axis, and y degrees around the y axis (in that order).</para>
		/// </summary>
		/// <param name="euler"></param>
		public static Quaternion Euler(Vector3 euler)
		{
			return Quaternion.FromEulerRad(euler * degToRad);
		}

		private static double NormalizeAngle(double angle)
		{
			while (angle > 360)
				angle -= 360;
			while (angle < 0)
				angle += 360;
			return angle;
		}

		private static Quaternion FromEulerRad(Vector3 euler)
		{
			var yaw = euler.x;
			var pitch = euler.y;
			var roll = euler.z;
			double rollOver2 = roll * 0.5;
			double sinRollOver2 = (double)System.Math.Sin((double)rollOver2);
			double cosRollOver2 = (double)System.Math.Cos((double)rollOver2);
			double pitchOver2 = pitch * 0.5;
			double sinPitchOver2 = (double)System.Math.Sin((double)pitchOver2);
			double cosPitchOver2 = (double)System.Math.Cos((double)pitchOver2);
			double yawOver2 = yaw * 0.5;
			double sinYawOver2 = (double)System.Math.Sin((double)yawOver2);
			double cosYawOver2 = (double)System.Math.Cos((double)yawOver2);
			Quaternion result;
			result.x = cosYawOver2 * cosPitchOver2 * cosRollOver2 + sinYawOver2 * sinPitchOver2 * sinRollOver2;
			result.y = cosYawOver2 * cosPitchOver2 * sinRollOver2 - sinYawOver2 * sinPitchOver2 * cosRollOver2;
			result.z = cosYawOver2 * sinPitchOver2 * cosRollOver2 + sinYawOver2 * cosPitchOver2 * sinRollOver2;
			result.w = sinYawOver2 * cosPitchOver2 * cosRollOver2 - cosYawOver2 * sinPitchOver2 * sinRollOver2;
			return result;
		}

		private static void ToAxisAngleRad(Quaternion q, out Vector3 axis, out double angle)
		{
			if (System.Math.Abs(q.w) > 1.0)
				q.Normalize();
			angle = 2.0f * (double)System.Math.Acos(q.w); // angle
			double den = (double)System.Math.Sqrt(1.0 - q.w * q.w);
			if (den > 0.0001)
			{
				axis = q.xyz / den;
			}
			else
			{
				// This occurs when the angle is zero. 
				// Not a problem: just set an arbitrary normalized axis.
				axis = new Vector3(1, 0, 0);
			}
		}

		public override int GetHashCode()
		{
			return this.x.GetHashCode() ^ this.y.GetHashCode() << 2 ^ this.z.GetHashCode() >> 2 ^ this.w.GetHashCode() >> 1;
		}
		public override bool Equals(object other)
		{
			if (!(other is Quaternion))
			{
				return false;
			}
			Quaternion quaternion = (Quaternion)other;
			return this.x.Equals(quaternion.x) && this.y.Equals(quaternion.y) && this.z.Equals(quaternion.z) && this.w.Equals(quaternion.w);
		}

		public bool Equals(Quaternion other)
		{
			return this.x.Equals(other.x) && this.y.Equals(other.y) && this.z.Equals(other.z) && this.w.Equals(other.w);
		}

		public static Quaternion operator *(Quaternion lhs, Quaternion rhs)
		{
			return new Quaternion(lhs.w * rhs.x + lhs.x * rhs.w + lhs.y * rhs.z - lhs.z * rhs.y, lhs.w * rhs.y + lhs.y * rhs.w + lhs.z * rhs.x - lhs.x * rhs.z, lhs.w * rhs.z + lhs.z * rhs.w + lhs.x * rhs.y - lhs.y * rhs.x, lhs.w * rhs.w - lhs.x * rhs.x - lhs.y * rhs.y - lhs.z * rhs.z);
		}

		public static Vector3 operator *(Quaternion rotation, Vector3 point)
		{
			double num = rotation.x * 2;
			double num2 = rotation.y * 2;
			double num3 = rotation.z * 2;
			double num4 = rotation.x * num;
			double num5 = rotation.y * num2;
			double num6 = rotation.z * num3;
			double num7 = rotation.x * num2;
			double num8 = rotation.x * num3;
			double num9 = rotation.y * num3;
			double num10 = rotation.w * num;
			double num11 = rotation.w * num2;
			double num12 = rotation.w * num3;

			return new Vector3(
				(1 - (num5 + num6)) * point.x + (num7 - num12) * point.y + (num8 + num11) * point.z,
				(num7 + num12) * point.x + (1 - (num4 + num6)) * point.y + (num9 - num10) * point.z,
				(num8 - num11) * point.x + (num9 + num10) * point.y + (1 - (num4 + num5)) * point.z);
		}

		public static bool operator ==(Quaternion lhs, Quaternion rhs)
		{
			return Quaternion.Dot(lhs, rhs) > 0.999999999;
		}

		public static bool operator !=(Quaternion lhs, Quaternion rhs)
		{
			return Quaternion.Dot(lhs, rhs) <= 0.999999999;
		}
	}
}