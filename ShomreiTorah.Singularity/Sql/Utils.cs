using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ShomreiTorah.Singularity.Sql {
	static class Utils {
		public static string EscapeSqlIdentifier(this string identifier) {
			return "[" + identifier.Replace("]", "]]") + "]";
		}
	}
}
