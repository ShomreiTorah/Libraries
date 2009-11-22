using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace ShomreiTorah.Administration {
	/// <summary>
	/// An exception thrown when the user's input contains a problem.
	/// </summary>
	[Serializable]
	public class UserInputException : Exception {
		///<summary>Creates a UserInputException for a specific problem.</summary>
		public UserInputException(UserInputProblem problem) : this(problem.ToString()) { Problem = problem; }
		///<summary>Creates a UserInputException for a specific problem.</summary>
		public UserInputException(UserInputProblem problem, Exception innerException) : this(problem.ToString(), innerException) { Problem = problem; }

		///<summary>Gets the problem that caused this exception.</summary>
		public UserInputProblem Problem { get; private set; }

		///<summary>Sets the SerializationInfo with information about the exception.</summary>
		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
		public override void GetObjectData(SerializationInfo info, StreamingContext context) {
			Problem = (UserInputProblem)info.GetValue("Problem", typeof(UserInputProblem));
			base.GetObjectData(info, context);
		}

		/// <summary>
		/// Constructs a new UserInputException.
		/// </summary>
		public UserInputException() { }
		/// <summary>
		/// Constructs a new UserInputException.
		/// </summary>
		/// <param name="message">The exception message</param>
		public UserInputException(string message) : base(message) { }
		/// <summary>
		/// Constructs a new UserInputException.
		/// </summary>
		/// <param name="message">The exception message</param>
		/// <param name="innerException">The inner exception</param>
		public UserInputException(string message, Exception innerException) : base(message, innerException) { }
		/// <summary>
		/// Serialization constructor.
		/// </summary>
		protected UserInputException(SerializationInfo info, StreamingContext context)
			: base(info, context) {
			info.AddValue("Problem", Problem);
		}
	}

	///<summary>A specific problem with user input.</summary>
	public enum UserInputProblem {
		///<summary>The input had no known specific problem.</summary>
		Unknown,
		///<summary>The user tried to re-join a list.</summary>
		Duplicate,
	}
}
