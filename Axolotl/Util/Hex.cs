using System;
using System.Text;

namespace Axolotl.Util
{
	public class Hex
	{
		private static char[] HEX_DIGITS = {
			'0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'a', 'b', 'c', 'd', 'e', 'f'
		};

		public static string ToString(byte[] bytes) {
			return ToString(bytes, 0, bytes.Length);
		}

		public static string ToString(byte[] bytes, int offset, int length) {
			StringBuilder buf = new StringBuilder();
			for (int i = 0; i < length; i++) {
				AppendHexChar(buf, bytes[offset + i]);
				buf.Append(" ");
			}
			return buf.ToString();
		}

		public static string ToStringCondensed(byte[] bytes) {
			StringBuilder buf = new StringBuilder();
			for (int i = 0; i < bytes.Length; i++) {
				AppendHexChar(buf, bytes[i]);
			}
			return buf.ToString();
		}

		public static byte[] FromStringCondensed(string encoded) {
			char[] data = encoded.ToCharArray();
			int len  = data.Length;

			if ((len & 0x01) != 0) {
				throw new Exception("Odd number of characters.");
			}

			byte[] result = new byte[len >> 1];

			for (int i = 0, j = 0; j < len; i++) {
				int f = Convert.ToByte(data[j]) << 4;
				j++;
				f = f | Convert.ToByte(data[j]);
				j++;
				result[i] = (byte) (f & 0xFF);
			}

			return result;
		}

		private static void AppendHexChar(StringBuilder buf, int b) {
			buf.Append(HEX_DIGITS[(b >> 4) & 0xf]);
			buf.Append(HEX_DIGITS[b & 0xf]);
		}
	}
}

