using System;

namespace GenericReadWriteLibrary
{
	public static class DataConverter
	{
		public static int GetRequiredBytes(byte value)
		{
			int bytes = 1;
			while ((value >> (7 * bytes)) != 0)
			{
				bytes++;
			}
			return bytes;
		}
		public static int GetRequiredBytes(sbyte value)
		{
			return GetRequiredBytes((byte)value);
		}
		public static void WriteByte(byte[] dst, ref int dstOffset, byte value)
		{
			dst[dstOffset++] = value;
		}
		public static void WriteSByte(byte[] dst, ref int dstOffset, sbyte value)
		{
			dst[dstOffset++] = (byte)value;
		}
		public static byte ReadByte(byte[] src, ref int srcOffset)
		{
			return src[srcOffset++];
		}
		public static sbyte ReadSByte(byte[] src, ref int srcOffset)
		{
			return (sbyte)src[srcOffset++];
		}
	}
}
