using System;
using System.Globalization;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Scripting;
 
[System.Serializable]
[StructLayout(LayoutKind.Sequential)]
public struct Vector4Int : IEquatable<Vector4Int>, IFormattable{
	public int x { get { return m_X; } set { m_X = value; } }
	public int y { get { return m_Y; } set { m_Y = value; } }
	public int z { get { return m_Z; } set { m_Z = value; } }
	public int w { get { return m_W; } set { m_W = value; } }
	
	private int m_X, m_Y, m_Z, m_W;
	
	public Vector4Int(int x = 0, int y = 0, int z = 0, int w = 0){
		m_X = x;
		m_Y = y;
		m_Z = z;
		m_W = w;
	}
	public Vector4Int(float x = 0f, float y = 0f, float z = 0f, float w = 0f){
		m_X = Helper.floor(x);
		m_Y = Helper.floor(y);
		m_Z = Helper.floor(z);
		m_W = Helper.floor(w);
	}
	public Vector4Int(Vector4 p){
		m_X = (int) p.x;
		m_Y = (int) p.y;
		m_Z = (int) p.z;
		m_W = (int) p.w;
	}
	public Vector4Int(Vector3 p){
		m_X = (int) p.x;
		m_Y = (int) p.y;
		m_Z = (int) p.z;
		m_W = 0;
	}
	public Vector4Int(Vector3Int p){
		m_X = p.x;
		m_Y = p.y;
		m_Z = p.z;
		m_W = 0;
	}
	public Vector4Int(Vector2 p){
		m_X = (int) p.x;
		m_Y = (int) p.y;
		m_Z = 0;
		m_W = 0;
	}
	public Vector4Int(Vector2Int p){
		m_X = p.x;
		m_Y = p.y;
		m_Z = 0;
		m_W = 0;
	}
	// Set x, y and z components of an existing Vector.
	public void Set(int x, int y, int z, int w){
		m_X = x;
		m_Y = y;
		m_Z = z;
		m_W = w;
	}
	// Access the /x/, /y/ or /z/ component using [0], [1] or [2] respectively.
	public int this[int index]{
		get {
			switch(index){
				case 0: return x;
				case 1: return y;
				case 2: return z;
				case 3: return w;
				default:
					throw new IndexOutOfRangeException(
						"Invalid Vector4Int index addressed: " + index + "!"
					);
			}
		}
		set{
			switch(index){
				case 0: x = value; break;
				case 1: y = value; break;
				case 2: z = value; break;
				case 3: w = value; break;
				default:
					throw new IndexOutOfRangeException(
						"Invalid Vector4Int index addressed: " + index + "!"
					);
			}
		}
	}
	// Returns the length of this vector (RO).
	public float magnitude{
		get { return Mathf.Sqrt((float) (x * x + y * y + z * z + w * w)); }
	}
	// Returns the squared length of this vector (RO).
	public int sqrMagnitude{
		get { return x * x + y * y + z * z + w * w; }
	}
	// Returns the distance between /a/ and /b/.
	public static float Distance(Vector4Int a, Vector4Int b){
		return (a - b).magnitude;
	}
	// Returns a vector that is made from the smallest components of two vectors.
	public static Vector4Int Min(Vector4Int lhs, Vector4Int rhs){
		return new Vector4Int(
			Mathf.Min(lhs.x, rhs.x),
			Mathf.Min(lhs.y, rhs.y),
			Mathf.Min(lhs.z, rhs.z),
			Mathf.Min(lhs.w, rhs.w)
		);
	}
	// Returns a vector that is made from the largest components of two vectors.
	public static Vector4Int Max(Vector4Int lhs, Vector4Int rhs){
		return new Vector4Int(
			Mathf.Max(lhs.x, rhs.x),
			Mathf.Max(lhs.y, rhs.y),
			Mathf.Max(lhs.z, rhs.z),
			Mathf.Max(lhs.w, rhs.w)
		);
	}
	// Multiplies two vectors component-wise.
	public static Vector4Int Scale(Vector4Int a, Vector4Int b){
		return new Vector4Int(a.x * b.x, a.y * b.y, a.z * b.z, a.w * b.w);
	}
	// Multiplies every component of this vector by the same component of /scale/.
	public Vector4Int Scale(Vector4Int scale){
		x *= scale.x;
		y *= scale.y;
		z *= scale.z;
		w *= scale.w;
		
		return this;
	}
	public Vector4Int Clamp(Vector4Int min, Vector4Int max){
		return new Vector4Int(
			Math.Min(max.x, Math.Max(min.x, x)),
			Math.Min(max.y, Math.Max(min.y, y)),
			Math.Min(max.z, Math.Max(min.z, z)),
			Math.Min(max.w, Math.Max(min.w, w))
		);
	}
	public static implicit operator Vector4(Vector4Int v){
		return new Vector4(v.x, v.y, v.z, v.w);
	}
	public static explicit operator Vector3(Vector4Int v){
		return new Vector3(v.x, v.y, v.z);
	}
	public static explicit operator Vector3Int(Vector4Int v){
		return new Vector3Int(v.x, v.y, v.z);
	}
	public static explicit operator Vector2(Vector4Int v){
		return new Vector2(v.x, v.y);
	}
	public static explicit operator Vector2Int(Vector4Int v){
		return new Vector2Int(v.x, v.y);
	}
	public static explicit operator int[](Vector4Int v){
		return new int[4] { v.x, v.y, v.z, v.w };
	}
	public static Vector4Int FloorToInt(Vector4 v){
		return new Vector4Int(
			Mathf.FloorToInt(v.x),
			Mathf.FloorToInt(v.y),
			Mathf.FloorToInt(v.z),
			Mathf.FloorToInt(v.w)
		);
	}
	public static Vector4Int CeilToInt(Vector4 v){
		return new Vector4Int(
			Mathf.CeilToInt(v.x),
			Mathf.CeilToInt(v.y),
			Mathf.CeilToInt(v.z),
			Mathf.CeilToInt(v.w)
		);
	}
	public static Vector4Int RoundToInt(Vector4 v){
		return new Vector4Int(
			Mathf.RoundToInt(v.x),
			Mathf.RoundToInt(v.y),
			Mathf.RoundToInt(v.z),
			Mathf.RoundToInt(v.w)
		);
	}
	public static Vector4Int operator +(Vector4Int a, Vector4Int b){
		return new Vector4Int(a.x + b.x, a.y + b.y, a.z + b.z, a.w + b.w);
	}
	public static Vector4 operator +(Vector4Int a, Vector4 b){
		return new Vector4(a.x + b.x, a.y + b.y, a.z + b.z, a.w + b.w);
	}
	public static Vector4 operator +(Vector4 a, Vector4Int b){
		return new Vector4(a.x + b.x, a.y + b.y, a.z + b.z, a.w + b.w);
	}
	public static Vector4Int operator +(Vector4Int a, Vector3Int b){
		return new Vector4Int(a.x + b.x, a.y + b.y, a.z + b.z, a.w);
	}
	public static Vector4Int operator +(Vector3Int a, Vector4Int b){
		return new Vector4Int(a.x + b.x, a.y + b.y, a.z + b.z, b.w);
	}
	public static Vector4Int operator -(Vector4Int a, Vector4Int b){
		return new Vector4Int(a.x - b.x, a.y - b.y, a.z - b.z, a.w - b.w);
	}
	public static Vector4 operator -(Vector4Int a, Vector4 b){
		return new Vector4(a.x - b.x, a.y - b.y, a.z - b.z, a.w - b.w);
	}
	public static Vector4 operator -(Vector4 a, Vector4Int b){
		return new Vector4(a.x - b.x, a.y - b.y, a.z - b.z, a.w - b.w);
	}
	public static Vector4Int operator -(Vector4Int a, Vector3Int b){
		return new Vector4Int(a.x - b.x, a.y - b.y, a.z - b.z, a.w);
	}
	public static Vector4Int operator -(Vector3Int a, Vector4Int b){
		return new Vector4Int(a.x - b.x, a.y - b.y, a.z - b.z, - b.w);
	}
	public static Vector4Int operator -(Vector4Int a){
		return new Vector4Int(- a.x, - a.y, - a.z, - a.w);
	}
	public static Vector4Int operator *(Vector4Int a, Vector4Int b){
		return new Vector4Int(a.x * b.x, a.y * b.y, a.z * b.z, a.w * b.w);
	}
	public static Vector4Int operator *(Vector4Int a, int b){
		return new Vector4Int(a.x * b, a.y * b, a.z * b, a.w * b);
	}
	public static Vector4Int operator *(int a, Vector4Int b){
		return new Vector4Int(a * b.x, a * b.y, a * b.z, a * b.w);
	}
	public static Vector4 operator *(Vector4Int a, float b){
		return new Vector4(a.x * b, a.y * b, a.z * b, a.w * b);
	}
	public static Vector4 operator *(float a, Vector4Int b){
		return new Vector4(a * b.x, a * b.y, a * b.z, a * b.w);
	}
	public static Vector4Int operator /(Vector4Int a, int b){
		return new Vector4Int(a.x / b, a.y / b, a.z / b, a.w / b);
	}
	public static Vector4 operator /(Vector4Int a, float b){
		return new Vector4(a.x / b, a.y / b, a.z / b, a.w / b);
	}
	public static Vector4 operator /(float b, Vector4Int a){
		return new Vector4(b / (float) a.x, b / (float) a.y, b / (float) a.z, b / (float) a.w);
	}
	public static Vector4Int operator /(int b, Vector4Int a){
		return new Vector4Int(b / a.x, b / a.y, b / a.z, b / a.w);
	}
	public static bool operator == (Vector4Int lhs, Vector4Int rhs){
		return lhs.x == rhs.x && lhs.y == rhs.y && lhs.z == rhs.z && lhs.w == rhs.w;
	}
	public static bool operator == (Vector4Int lhs, Vector4 rhs){
		Vector4 l = (Vector4) lhs;
		return l.x == rhs.x && l.y == rhs.y && l.z == rhs.z && l.w == rhs.w;
	}
	public static bool operator == (Vector4 lhs, Vector4Int rhs){
		Vector4 r = (Vector4) rhs;
		return lhs.x == r.x && lhs.y == r.y && lhs.z == r.z && lhs.w == r.w;
	}
	public static bool operator != (Vector4Int lhs, Vector4Int rhs){
		return !(lhs == rhs);
	}
	public static bool operator != (Vector4 lhs, Vector4Int rhs){
		return !(lhs == rhs);
	}
	public static bool operator != (Vector4Int lhs, Vector4 rhs){
		return !(lhs == rhs);
	}
	public static bool operator > (Vector4Int lhs, Vector4Int rhs){
		return lhs.x > rhs.x && lhs.y > rhs.y && lhs.z > rhs.z && lhs.w > rhs.w;
	}
	public static bool operator < (Vector4Int lhs, Vector4Int rhs){
		return lhs.x < rhs.x && lhs.y < rhs.y && lhs.z < rhs.z && lhs.w < rhs.w;
	}
	public static bool operator >= (Vector4Int lhs, Vector4Int rhs){
		return lhs.x >= rhs.x && lhs.y >= rhs.y && lhs.z >= rhs.z && lhs.w >= rhs.w;
	}
	public static bool operator <= (Vector4Int lhs, Vector4Int rhs){
		return lhs.x <= rhs.x && lhs.y <= rhs.y && lhs.z <= rhs.z && lhs.w <= rhs.w;
	}
	public override bool Equals(object other){
		if (!(other is Vector4Int || other is Vector4)) return false;
		return Equals((Vector4Int) other);
	}
	public bool Equals(Vector4Int other){
		return this == other;
	}
	public override int GetHashCode(){
		var yHash = y.GetHashCode();
		var zHash = z.GetHashCode();
		var wHash = w.GetHashCode();
		return x.GetHashCode() ^ (yHash << 8) ^ (yHash >> 24) ^ (zHash << 16) ^ (zHash >> 16) ^ (wHash << 24) ^ (wHash >> 8);
	}
	public override string ToString(){
		return ToString(null, CultureInfo.InvariantCulture.NumberFormat);
	}
	public string ToString(string format){
		return ToString(format, CultureInfo.InvariantCulture.NumberFormat);
	}
	public string ToString(string format, IFormatProvider formatProvider){
		return "(" +
			x.ToString(format, formatProvider) + ", " +
			y.ToString(format, formatProvider) + ", " +
			z.ToString(format, formatProvider) + ", " +
			w.ToString(format, formatProvider) + ")";
	}
	public static Vector4 Lerp(Vector4Int a, Vector4Int b, float t){
		return a + ((b - a) * t);
	}
	public static Vector4Int zero	{ get { return s_zero; } }
	public static Vector4Int one	{ get { return s_one; } }
	public static Vector4Int front	{ get { return s_front; } }
	public static Vector4Int back	{ get { return s_back; } }
	public static Vector4Int up	{ get { return s_up; } }
	public static Vector4Int down	{ get { return s_down; } }
	public static Vector4Int left	{ get { return s_left; } }
	public static Vector4Int right	{ get { return s_right; } }
	public static Vector4Int w_front{ get { return s_w_front; } }
	public static Vector4Int w_back	{ get { return s_w_back; } }
	
	private static readonly Vector4Int	s_zero	= new Vector4Int(0, 0, 0, 0),
						s_one	= new Vector4Int(1, 1, 1, 1),
						s_front	= new Vector4Int(0, 0, 1, 0),
						s_back	= new Vector4Int(0, 0, -1, 0),
						s_up	= new Vector4Int(0, 1, 0, 0),
						s_down	= new Vector4Int(0, -1, 0, 0),
						s_left	= new Vector4Int(-1, 0, 0, 0),
						s_right	= new Vector4Int(1, 0, 0, 0),
						s_w_front	= new Vector4Int(0, 0, 0, 1),
						s_w_back	= new Vector4Int(0, 0, 0, -1);
}