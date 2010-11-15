using System;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using System.IO;

namespace Osiris.Content.Pipeline
{
	[ContentImporter(".tif", ".tiff", DisplayName = "TIFF Texture - Osiris Framework", DefaultProcessor = "FloatingPointTextureProcessor")]
	public class TiffImporter : ContentImporter<TextureContent>
	{
		#region Fields

		private BinaryReader m_pReader;

		private int m_nWidth;
		private int m_nHeight;

		private int m_nStripOffset;

		private ushort[] m_pImageData;

		#endregion

		#region Methods

		public override TextureContent Import(string filename, ContentImporterContext context)
		{
			// load tiff data
			m_pReader = new BinaryReader(File.OpenRead(filename));
			int nOffsetFirstIFD = ReadHeader();
			ReadAllIfds(nOffsetFirstIFD);
			m_pReader.Close();

			// import into standard XNA bitmap container
			PixelBitmapContent<float> bitmapContent = new PixelBitmapContent<float>(m_nWidth, m_nHeight);
			for (int y = 0; y < m_nHeight; y++)
				for (int x = 0; x < m_nWidth; x++)
					bitmapContent.SetPixel(x, y, m_pImageData[(y * m_nWidth) + x]);

			// create and return one-mipmap-level
			Texture2DContent content = new Texture2DContent();
			content.Mipmaps.Add(bitmapContent);
			return content;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns>Offset in bytes, from the beginning of the file, to the first IFD</returns>
		private int ReadHeader()
		{
			// bytes 0-1 - the byte order used within the file
			short hByteOrder = m_pReader.ReadInt16();
			if (hByteOrder != 0x4949)
			{
				throw new NotSupportedException("Only little-endian order is supported");
			}

			// bytes 2-3 - 42!
			short h42 = m_pReader.ReadInt16();
			if (h42 != 42)
			{
				throw new NotSupportedException("Could not read 42");
			}

			// bytes 4-7 - offset (in bytes) of the first IFD
			int nOffsetFirstIFD = m_pReader.ReadInt32();
			return nOffsetFirstIFD;
		}

		private void ReadAllIfds(int nOffset)
		{
			/*while (nOffset != 0)
			{
					nOffset = ReadIfd(nOffset);
			}*/

			// just read first IFD at the moment
			ReadIfd(nOffset);
		}

		private int ReadIfd(int nOffset)
		{
			m_pReader.BaseStream.Seek(nOffset, SeekOrigin.Begin);

			// bytes 0-1 - count of the number of directory entries (i.e. the number of fields)
			short hDirectoryEntryCount = m_pReader.ReadInt16();

			// sequence of 12-byte field entries
			for (int i = 0, length = hDirectoryEntryCount; i < length; i++)
			{
				ReadIfdEntry();
			}

			ReadImageData();

			// 4-byte offset of the next IFD, or 0 if none
			int nNextOffset = m_pReader.ReadInt32();
			return nNextOffset;
		}

		private void ReadIfdEntry()
		{
			// bytes 0-1 - the Tag that identifies the field
			Tag eTag = (Tag)m_pReader.ReadInt16();

			// bytes 2-3 - the field Type
			FieldType eFieldType = (FieldType)m_pReader.ReadInt16();

			// bytes 4-7 - the number of values, Count of the indicated Type
			int nNumValues = m_pReader.ReadInt32();

			// bytes 8-11 - the Value Offset, the file offset (in bytes) of the Value for the field
			int nValueOffset = m_pReader.ReadInt32();

			ProcessTag(eTag, eFieldType, nNumValues, nValueOffset);
		}

		private void ProcessTag(Tag eTag, FieldType eFieldType, int nNumValues, int nValueOffset)
		{
			switch (eTag)
			{
				case Tag.ImageWidth:
					m_nWidth = nValueOffset;
					break;
				case Tag.ImageLength:
					m_nHeight = nValueOffset;
					break;
				case Tag.BitsPerSample:
					if (nValueOffset != 16)
					{
						throw new Exception("Currently only support 16 bits per sample");
					}
					break;
				case Tag.StripOffsets:
					//long lCurrentPos = m_pReader.BaseStream.Position;
					//m_pReader.BaseStream.Seek(nValueOffset, SeekOrigin.Begin);
					//m_nStripOffset = m_pReader.ReadInt32();
					//m_pReader.BaseStream.Seek(lCurrentPos, SeekOrigin.Begin);
					m_nStripOffset = nValueOffset;
					break;
			}
		}

		private void ReadImageData()
		{
			m_pReader.BaseStream.Seek(m_nStripOffset, SeekOrigin.Begin);

			m_pImageData = new ushort[m_nWidth * m_nHeight];
			for (int i = 0, length = m_pImageData.Length; i < length; i++)
			{
				m_pImageData[i] = (ushort)m_pReader.ReadInt16();
			}
		}

		#endregion

		#region Enums

		private enum FieldType : short
		{
			/// <summary>
			/// 8-bit unsigned integer
			/// </summary>
			Byte = 1,

			/// <summary>
			/// 8-bit byte that contains a 7-bit ASCII code; the last byte
			/// must be NUL (binary zero)
			/// </summary>
			Ascii = 2,

			/// <summary>
			/// 16-bit (2-byte) unsigned integer
			/// </summary>
			Short = 3,

			/// <summary>
			/// 32-bit (4-byte) unsigned integer
			/// </summary>
			Long = 4,

			/// <summary>
			/// Two LONGs: the first represents the numerator of a
			/// fraction; the second, the denominator
			/// </summary>
			Rational = 5,

			/// <summary>
			/// An 8-bit signed (twos-complement) integer
			/// </summary>
			SByte = 6,

			/// <summary>
			/// An 8-bit byte that may contain anything, depending on
			/// the definition of the field
			/// </summary>
			Undefined = 7,

			/// <summary>
			/// A 16-bit (2-byte) signed (twos-complement) integer
			/// </summary>
			SShort = 8,

			/// <summary>
			/// A 32-bit (4-byte) signed (twos-complement) integer
			/// </summary>
			SLong = 9,

			/// <summary>
			/// Two SLONG’s: the first represents the numerator of a
			/// fraction, the second the denominator
			/// </summary>
			SRational = 10,

			/// <summary>
			/// Single precision (4-byte) IEEE format
			/// </summary>
			Float = 11,

			/// <summary>
			/// Double precision (8-byte) IEEE format
			/// </summary>
			Double = 12
		}

		private enum Tag : short
		{
			/// <summary>
			/// Type = SHORT or LONG
			/// The number of columns in the image, i.e., the number of pixels per row
			/// N = 1
			/// No default. See also ImageLength.
			/// </summary>
			ImageWidth = 256,

			/// <summary>
			/// Type = SHORT or LONG
			/// The number of rows of pixels in the image.
			/// N = 1
			/// No default. See also ImageWidth.
			/// </summary>
			ImageLength = 257,

			/// <summary>
			/// Type = SHORT
			/// The number of bits per component.
			/// Allowable values for Baseline TIFF grayscale images are 4 and 8, allowing either
			/// 16 or 256 distinct shades of gray.
			/// </summary>
			BitsPerSample = 258,

			/// <summary>
			/// Type = SHORT or LONG
			/// For each strip, the byte offset of that strip.
			/// </summary>
			StripOffsets = 273,

			/// <summary>
			/// Type = SHORT or LONG
			/// The number of rows in each strip (except possibly the last strip.)
			/// For example, if ImageLength is 24, and RowsPerStrip is 10, then there are 3
			/// strips, with 10 rows in the first strip, 10 rows in the second strip, and 4 rows in the
			/// third strip. (The data in the last strip is not padded with 6 extra rows of dummy
			/// data.)
			/// </summary>
			RowsPerStrip = 278,

			/// <summary>
			/// Type = SHORT or LONG
			/// For each strip, the number of bytes in the strip after compression.
			/// </summary>
			StripByteCounts = 279
		}

		#endregion
	}
}