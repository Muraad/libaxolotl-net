using System;
using System.Linq;
using System.Collections.Generic;

namespace Axolotl.Util
{
	public class ByteUtil
	{
		public static byte[] Combine (params byte[][] args)
		{
			List<byte> result = new List<byte> ();

			foreach (var arg in args) {
				result.AddRange (arg);
			}
			return result.ToArray ();
		}

		public static byte[][] Split(byte[] input, int firstLength, int secondLength)
		{
			byte[][] parts = new byte[2][];

			parts [0] = new byte[firstLength];
			Array.Copy (input, 0, parts [0], 0, firstLength);


			parts [1] = new byte[secondLength];
			Array.Copy (input, firstLength, parts [1], 0, secondLength);

			return parts;
		}

		public static byte[][] Split(byte[] input, int firstLength, int secondLength, int thirdLength)
		{
			if (input == null || firstLength < 0 || secondLength < 0 || thirdLength < 0 ||
			    input.Length < firstLength + secondLength + thirdLength)
			{
				throw new Exception ("Input too small");
			}

			byte[][] parts = new byte[3][];

			parts[0] = new byte[firstLength];
			Array.Copy(input, 0, parts[0], 0, firstLength);

			parts[1] = new byte[secondLength];
			Array.Copy(input, firstLength, parts[1], 0, secondLength);

			parts[2] = new byte[thirdLength];
			Array.Copy(input, firstLength + secondLength, parts[2], 0, thirdLength);

			return parts;
		}

		public static byte[] Trim(byte[] input, int length) {
			byte[] result = new byte[length];
			Array.Copy(input, 0, result, 0, result.Length);

			return result;
		}

		public static byte[] CopyFrom(byte[] input) {
			byte[] output = new byte[input.Length];
			Array.Copy (input, 0, output, 0, output.Length);

			return output;
		}

		public static byte IntsToByteHighAndLow(UInt32 highValue, UInt32 lowValue) {
			return (byte)((highValue << 4 | lowValue) & 0xFF);
		}

		public static UInt32 HighBitsToUInt(byte value) {
			return (UInt32)(value & 0xFF) >> 4;
		}

		public static UInt32 LowBitsToUInt(byte value) {
			return (UInt32)(value & 0xF);
		}

		public static UInt32 HighBitsToMedium(int value) {
			return (UInt32)(value >> 12);
		}

		public static UInt32 LowBitsToMedium(int value) {
			return (UInt32)(value & 0xFFF);
		}

		public static byte[] ShortToByteArray(int value) {
			byte[] bytes = new byte[2];
			ShortToByteArray(bytes, 0, value);
			return bytes;
		}

		public static int ShortToByteArray(byte[] bytes, int offset, int value) {
			bytes[offset+1] = (byte)value;
			bytes[offset]   = (byte)(value >> 8);
			return 2;
		}

		public static int ShortToLittleEndianByteArray(byte[] bytes, int offset, int value) {
			bytes[offset]   = (byte)value;
			bytes[offset+1] = (byte)(value >> 8);
			return 2;
		}

		public static byte[] MediumToByteArray(int value) {
			byte[] bytes = new byte[3];
			MediumToByteArray(bytes, 0, value);
			return bytes;
		}

		public static int MediumToByteArray(byte[] bytes, int offset, int value) {
			bytes[offset + 2] = (byte)value;
			bytes[offset + 1] = (byte)(value >> 8);
			bytes[offset]     = (byte)(value >> 16);
			return 3;
		}

		public static byte[] IntToByteArray(int value) {
			byte[] bytes = new byte[4];
			IntToByteArray(bytes, 0, value);
			return bytes;
		}

		public static int IntToByteArray(byte[] bytes, int offset, int value) {
			bytes[offset + 3] = (byte)value;
			bytes[offset + 2] = (byte)(value >> 8);
			bytes[offset + 1] = (byte)(value >> 16);
			bytes[offset]     = (byte)(value >> 24);
			return 4;
		}

		public static int IntToLittleEndianByteArray(byte[] bytes, int offset, int value) {
			bytes[offset]   = (byte)value;
			bytes[offset+1] = (byte)(value >> 8);
			bytes[offset+2] = (byte)(value >> 16);
			bytes[offset+3] = (byte)(value >> 24);
			return 4;
		}

		public static byte[] LongToByteArray(long l) {
			byte[] bytes = new byte[8];
			LongToByteArray(bytes, 0, l);
			return bytes;
		}

		public static int LongToByteArray(byte[] bytes, int offset, long value) {
			bytes[offset + 7] = (byte)value;
			bytes[offset + 6] = (byte)(value >> 8);
			bytes[offset + 5] = (byte)(value >> 16);
			bytes[offset + 4] = (byte)(value >> 24);
			bytes[offset + 3] = (byte)(value >> 32);
			bytes[offset + 2] = (byte)(value >> 40);
			bytes[offset + 1] = (byte)(value >> 48);
			bytes[offset]     = (byte)(value >> 56);
			return 8;
		}

		public static int LongTo4ByteArray(byte[] bytes, int offset, long value) {
			bytes[offset + 3] = (byte)value;
			bytes[offset + 2] = (byte)(value >> 8);
			bytes[offset + 1] = (byte)(value >> 16);
			bytes[offset + 0] = (byte)(value >> 24);
			return 4;
		}

		public static int ByteArrayToShort(byte[] bytes) {
			return ByteArrayToShort(bytes, 0);
		}

		public static int ByteArrayToShort(byte[] bytes, int offset) {
			return
				(bytes[offset] & 0xff) << 8 | (bytes[offset + 1] & 0xff);
		}

		// The SSL patented 3-byte Value.
		public static int ByteArrayToMedium(byte[] bytes, int offset) {
			return
				(bytes[offset]     & 0xff) << 16 |
					(bytes[offset + 1] & 0xff) << 8  |
					(bytes[offset + 2] & 0xff);
		}

		public static int ByteArrayToInt(byte[] bytes) {
			return ByteArrayToInt(bytes, 0);
		}

		public static int ByteArrayToInt(byte[] bytes, int offset)  {
			return
				(bytes[offset]     & 0xff) << 24 |
					(bytes[offset + 1] & 0xff) << 16 |
					(bytes[offset + 2] & 0xff) << 8  |
					(bytes[offset + 3] & 0xff);
		}

		public static int ByteArrayToIntLittleEndian(byte[] bytes, int offset) {
			return
				(bytes[offset + 3] & 0xff) << 24 |
					(bytes[offset + 2] & 0xff) << 16 |
					(bytes[offset + 1] & 0xff) << 8  |
					(bytes[offset]     & 0xff);
		}

		public static long ByteArrayToLong(byte[] bytes) {
			return ByteArrayToLong(bytes, 0);
		}

		public static long ByteArray4ToLong(byte[] bytes, int offset) {
			return
				((bytes[offset + 0] & 0xffL) << 24) |
					((bytes[offset + 1] & 0xffL) << 16) |
					((bytes[offset + 2] & 0xffL) << 8)  |
					((bytes[offset + 3] & 0xffL));
		}

		public static long ByteArrayToLong(byte[] bytes, int offset) {
			return
				((bytes[offset]     & 0xffL) << 56) |
					((bytes[offset + 1] & 0xffL) << 48) |
					((bytes[offset + 2] & 0xffL) << 40) |
					((bytes[offset + 3] & 0xffL) << 32) |
					((bytes[offset + 4] & 0xffL) << 24) |
					((bytes[offset + 5] & 0xffL) << 16) |
					((bytes[offset + 6] & 0xffL) << 8)  |
					((bytes[offset + 7] & 0xffL));
		}
	}
}

