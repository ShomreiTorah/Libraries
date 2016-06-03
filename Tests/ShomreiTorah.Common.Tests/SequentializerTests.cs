using Microsoft.VisualStudio.TestTools.UnitTesting;
using ShomreiTorah.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShomreiTorah.Common.Tests {
	[TestClass]
	public class SequentializerTests {
		[TestMethod]
		public void SimpleTest() {
			var s = new Sequentializer();
			var results = new List<int>();

			s.Execute(() => results.Add(1));
			s.Execute(() => results.Add(2));

			CollectionAssert.AreEqual(new[] { 1, 2 }, results);
		}
		[TestMethod]
		public void ReentrantTest() {
			var s = new Sequentializer();
			var results = new List<int>();
			s.Execute(() => {
				results.Add(1);
				s.Execute(() => results.Add(3));
				results.Add(2);
			});
			CollectionAssert.AreEqual(new[] { 1, 2, 3 }, results);
		}
		[TestMethod]
		public void VeryReentrantTest() {
			var s = new Sequentializer();
			var results = new List<int>();
			s.Execute(() => {
				results.Add(1);
				s.Execute(() => {
					results.Add(3);
					s.Execute(() => results.Add(5));
					results.Add(4);
				});
				results.Add(2);
			});
			CollectionAssert.AreEqual(new[] { 1, 2, 3, 4, 5 }, results);
		}
	}
}