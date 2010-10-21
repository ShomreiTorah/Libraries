using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DevExpress.Utils;
using System.Drawing;
using DevExpress.Utils.Drawing;
using DevExpress.LookAndFeel;
using System.Diagnostics.CodeAnalysis;

namespace ShomreiTorah.WinForms {
	///<summary>Contains miscellaneous useful methods.</summary>
	public static class Utilities {
		///<summary>Creates a DevExpress SuperToolTip object from a title and body text.</summary>
		[SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed", Justification = "Optional for named parameters only")]
		public static SuperToolTip CreateSuperTip(string title = null, string body = null) {
			var retVal = new SuperToolTip();
			if (!String.IsNullOrEmpty(title))
				retVal.Items.AddTitle(title);
			if (!String.IsNullOrEmpty(body))
				retVal.Items.Add(body);
			return retVal;
		}
		///<summary>Gets the color of the border around a column header in a skin.</summary>
		public static Color GetHeaderLineColor(this UserLookAndFeel lnf) {
			if (lnf.ActiveStyle == ActiveLookAndFeelStyle.Skin) {
				var image = (Bitmap)((SkinHeaderObjectPainter)lnf.Painter.Header).Element.Image.GetImages().Images[0];
				return image.GetPixel(image.Width - 1, image.Height - 1);
			} else
				return Color.DarkGray;
		}
	}
}
