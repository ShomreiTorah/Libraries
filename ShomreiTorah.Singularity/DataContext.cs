using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ShomreiTorah.Singularity {
	///<summary>Contains tables in a Singularity database.</summary>
	public class DataContext {
		///<summary>Creates a new DataContext.</summary>
		public DataContext() { Tables = new TableCollection(); }

		///<summary>Gets the tables in this context.</summary>
		public TableCollection Tables { get; private set; }
	}
}
