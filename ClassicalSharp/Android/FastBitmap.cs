﻿#if ANDROID
using System;
using Android.Graphics;
using Java.Nio;

namespace ClassicalSharp {

	/// <summary> Wrapper around a bitmap that allows extremely fast manipulation of 32bpp images. </summary>
	public unsafe class FastBitmap : IDisposable {
		
		public FastBitmap( Bitmap bmp, bool lockBits ) {
			Bitmap = bmp;
			if( lockBits ) {
				LockBits();
			}
		}
		
		public FastBitmap( int width, int height, int stride, IntPtr scan0 ) {
			Width = width;
			Height = height;
			Stride = stride;
			Scan0 = scan0;
			scan0Byte = (byte*)scan0;
		}
		
		public Bitmap Bitmap;
		ByteBuffer data;
		byte* scan0Byte;
		
		public bool IsLocked {
			get { return data != null; }
		}
		
		public IntPtr Scan0;
		public int Stride;
		public int Width, Height;
		
		public void LockBits() {
			if( Bitmap == null ) throw new InvalidOperationException( "Underlying bitmap is null." );
			if( data != null ) return;

			data = ByteBuffer.AllocateDirect( Bitmap.Width * Bitmap.Height * 4 );
			Bitmap.CopyPixelsToBuffer( data );
			scan0Byte = (byte*)data.GetDirectBufferAddress();
			Scan0 = data.GetDirectBufferAddress();
			Stride = Bitmap.Width * 4;
			Width = Bitmap.Width;
			Height = Bitmap.Height;
		}
		
		public void Dispose() {
			UnlockBits();
		}
		
		public void UnlockBits() {
			if( Bitmap != null && data != null ) {
				data.Rewind();
				Bitmap.CopyPixelsFromBuffer( data ); // TODO: Only if not readonly
				data.Dispose();

				data = null;
				scan0Byte = (byte*)IntPtr.Zero;
				Scan0 = IntPtr.Zero;
				Width = Height = Stride = 0;
			}
		}
		
		/// <summary> Returns a pointer to the start of the y'th scanline. </summary>
		public int* GetRowPtr( int y ) {
			return (int*)(scan0Byte + (y * Stride));
		}
		
		public static void MovePortion( int srcX, int srcY, int dstX, int dstY, FastBitmap src, FastBitmap dst, int size ) {
			for( int y = 0; y < size; y++ ) {
				int* srcRow = src.GetRowPtr( srcY + y );
				int* dstRow = dst.GetRowPtr( dstY + y );
				for( int x = 0; x < size; x++ ) {
					dstRow[dstX + x] = srcRow[srcX + x];
				}
			}
		}
	}
}
#endif