using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;

namespace ShomreiTorah.WinForms {
	static class NativeMethods {
		public const int LayeredWindow = 0x80000;//WS_EX_LAYERED

		#region Drawing
		[DllImport("User32.dll", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool UpdateLayeredWindow(IntPtr handle, IntPtr screenDc, ref Point windowLocation, ref Size windowSize, IntPtr imageDc, ref Point dcLocation, int colorKey, ref BlendFunction blendInfo, UlwType type);

		[DllImport("gdi32.dll")]
		public static extern IntPtr CreateCompatibleDC(IntPtr hDC);

		[DllImport("User32.dll")]
		public static extern IntPtr GetDC(IntPtr hWnd);

		[DllImport("User32.dll")]
		public static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);

		[DllImport("gdi32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool DeleteDC(IntPtr hdc);

		[DllImport("gdi32.dll")]
		public static extern IntPtr SelectObject(IntPtr hDC, IntPtr hObject);

		[DllImport("gdi32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool DeleteObject(IntPtr hObject);
		#endregion
	}
	struct BlendFunction {
		public byte BlendOp;
		public byte BlendFlags;
		public byte SourceConstantAlpha;
		public byte AlphaFormat;
	}
	#region Enums
	enum UlwType : int {
		None = 0,
		ColorKey = 0x00000001,
		Alpha = 0x00000002,
		Opaque = 0x00000004
	}
	enum HitTest {
		Caption = 2,
		Transparent = -1,
		Nowhere = 0,
		Client = 1,
		Left = 10,
		Right = 11,
		Top = 12,
		TopLeft = 13,
		TopRight = 14,
		Bottom = 15,
		BottomLeft = 16,
		BottomRight = 17,
		Border = 18
	}
	enum ImageType {
		Bitmap,
		Cursor,
		Icon
	}
	[SuppressMessage("Microsoft.Design", "CA1008:EnumsShouldHaveZeroValue", Justification = "API")]
	enum SystemCursorId {
		Normal = 32512,
		IBeam = 32513,
		Wait = 32514,
		Cross = 32515,
		Up = 32516,
		Size = 32640,
		Icon = 32641,
		SizeNwse = 32642,
		SizeNesw = 32643,
		SizeWE = 32644,
		SizeNS = 32645,
		SizeAll = 32646,
		IcoCur = 32647,
		No = 32648,
		Hand = 32649,
		AppStarting = 32650
	}
	#endregion
}
