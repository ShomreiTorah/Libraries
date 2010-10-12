using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ShomreiTorah.Data.UI.Grid {
	///<summary>Implements a single behavior for a SmartGridView.</summary>
	///<remarks>Implementations must be reusable and stateless.
	///Any per-view state should be stored in closures or in a separate class by Apply.</remarks>
	public interface IGridBehavior {
		///<summary>Applies the behavior to a SmartGridView.</summary>
		///<remarks>This method will be called multiple times on a single instance.</remarks>
		void Apply(SmartGridView view);
	}
}
