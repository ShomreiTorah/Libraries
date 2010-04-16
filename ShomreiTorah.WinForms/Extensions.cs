using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace ShomreiTorah.WinForms {
	///<summary>Contains miscellaneous extension methods.</summary>
	public static class Extensions {
		#region Graphics
		///<summary>Measures the exact width of a string.</summary>
		///<param name="graphics">A Graphics object to measure the string on.</param>
		///<param name="text">The string to measure.</param>
		///<param name="font">The font used to draw the string.</param>
		///<returns>The exact width of the string in pixels.</returns>
		public static float MeasureStringWidth(this Graphics graphics, string text, Font font) {
			if (graphics == null) throw new ArgumentNullException("graphics");
			if (text == null) throw new ArgumentNullException("text");
			if (font == null) throw new ArgumentNullException("font");

			using (var format = new StringFormat()) {
				format.SetMeasurableCharacterRanges(new[] { new CharacterRange(0, text.Length) });

				using (var region = graphics.MeasureCharacterRanges(text, font, new Rectangle(0, 0, int.MaxValue / 2, int.MaxValue / 2), format)[0]) {
					return region.GetBounds(graphics).Width;
				}
			}
		}
		#endregion

		#region Geometry
		///<summary>Inflates a rectangle by a padding.</summary>
		public static Rectangle InflateBy(this Rectangle rect, Padding padding) {
			rect.X -= padding.Left;
			rect.Y -= padding.Top;
			rect.Width += padding.Horizontal;
			rect.Height += padding.Vertical;
			return rect;
		}

		///<summary>Deflates a rectangle by a padding.</summary>
		public static Rectangle DeflateBy(this Rectangle rect, Padding padding) {
			rect.X += padding.Left;
			rect.Y += padding.Top;
			rect.Width -= padding.Horizontal;
			rect.Height -= padding.Vertical;
			return rect;
		}
		#endregion

		///<summary>Gets the data of a given type.</summary>
		///<typeparam name="TData">The type of data to get.</typeparam>
		[SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter", Justification = "No, they shouldn't")]
		public static TData GetData<TData>(this IDataObject dataObject) {
			if (dataObject == null) throw new ArgumentNullException("dataObject");
			return (TData)dataObject.GetData(typeof(TData));
		}

		///<summary>Shows a form as a modal dialog, and disposes the form when the dialog is closed.</summary>
		public static DialogResult ShowDisposingDialog(this Form form) { using (form) return form.ShowDialog(); }
		///<summary>Shows a form as a modal dialog, and disposes the form when the dialog is closed.</summary>
		public static DialogResult ShowDisposingDialog(this Form form, IWin32Window owner) { using (form) return form.ShowDialog(owner); }
	}
}
