using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using DevExpress.LookAndFeel;
using DevExpress.Utils.Drawing;

namespace ShomreiTorah.WinForms {
	static class SkinUtilities {
		public static Color GetHeaderLineColor(this UserLookAndFeel lnf) {
			if (lnf.ActiveStyle == ActiveLookAndFeelStyle.Skin) {
				var image = (Bitmap)((SkinHeaderObjectPainter)lnf.Painter.Header).Element.Image.GetImages().Images[0];
				return image.GetPixel(image.Width - 1, image.Height - 1);
			} else
				return Color.DarkGray;
		}
	}
}
