//
// Copyright ©2006, 2007, Martin R. Gagné (martingagne@gmail.com)
// All rights reserved.
//
// Redistribution and use in source and binary forms, with or without modification, 
// are permitted provided that the following conditions are met:
//
//   - Redistributions of source code must retain the above copyright notice, 
//     this list of conditions and the following disclaimer.
//
//   - Redistributions in binary form must reproduce the above copyright notice, 
//     this list of conditions and the following disclaimer in the documentation 
//     and/or other materials provided with the distribution.
// 
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND 
// ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED 
// WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. 
// IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, 
// INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT 
// NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, 
// OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, 
// WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) 
// ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY 
// OF SUCH DAMAGE.
//

using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace ShomreiTorah.WinForms.Controls {
	///<summary>A built-in style for a loading circle.</summary>
	public enum PresetLoadingStyle {
		///<summary>The animation used by Mac OSX; a gradient sequence of lines.</summary>
		[SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "OSX")]
		MacOSX,
		///<summary>The animation used by Firefox; a gradient sequence of circles.</summary>
		Firefox,
		///<summary>The animation used by IE 7; a solid gradient band.</summary>
		IE7,
		///<summary>Custom animation settings</summary>
		Custom
	}
	///<summary>Displays a circular loading animation.</summary>
	[Description("Displays a circular loading animation.")]
	public partial class LoadingCircle : Control {
		// Constants =========================================================
		private const double NumberOfDegreesInCircle = 360;
		private const double NumberOfDegreesInHalfCircle = NumberOfDegreesInCircle / 2;
		private const int DefaultInnerCircleRadius = 8;
		private const int DefaultOuterCircleRadius = 10;
		private const int DefaultNumberOfSpoke = 10;
		private const int DefaultSpokeThickness = 4;
		private readonly Color DefaultColor = Color.DarkGray;

		private const int MacOSXInnerCircleRadius = 5;
		private const int MacOSXOuterCircleRadius = 11;
		private const int MacOSXNumberOfSpoke = 12;
		private const int MacOSXSpokeThickness = 2;

		private const int FireFoxInnerCircleRadius = 6;
		private const int FireFoxOuterCircleRadius = 7;
		private const int FireFoxNumberOfSpoke = 9;
		private const int FireFoxSpokeThickness = 4;

		private const int IE7InnerCircleRadius = 8;
		private const int IE7OuterCircleRadius = 9;
		private const int IE7NumberOfSpoke = 24;
		private const int IE7SpokeThickness = 4;

		// Fields ========================================================
		private Timer m_Timer;
		private bool m_IsTimerActive;
		private int m_NumberOfSpoke;
		private int m_SpokeThickness;
		private int m_ProgressValue;
		private int m_OuterCircleRadius;
		private int m_InnerCircleRadius;
		private Point center;
		private Color baseColor;
		private Color[] spokeColors;
		private double[] m_Angles;
		private PresetLoadingStyle m_StylePreset;


		///<summary>Gets or sets a value indicating whether the user can give the focus to this control using the TAB key.</summary>
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Browsable(false)]
		[DefaultValue(false)]
		public new bool TabStop {
			get { return base.TabStop; }
			set { base.TabStop = value; }
		}
		// Properties ========================================================
		/// <summary>
		/// Gets or sets the base color of the circle.
		/// </summary>
		/// <value>The lightest color of the circle.</value>
		[TypeConverter("System.Drawing.ColorConverter")]
		[Category("Appearance")]
		[Description("Gets or sets the base color of the circle.")]
		public Color Color {
			get {
				return baseColor;
			}
			set {
				baseColor = value;

				GenerateColorsPallete();
				Invalidate();
			}
		}

		/// <summary>
		/// Gets or sets the radius of the outer circle.
		/// </summary>
		/// <value>The outer circle radius.</value>
		[Description("Gets or sets the radius of the outer circle.")]
		[Category("Appearance")]
		public int OuterCircleRadius {
			get {
				if (m_OuterCircleRadius == 0)
					m_OuterCircleRadius = DefaultOuterCircleRadius;

				return m_OuterCircleRadius;
			}
			set {
				m_OuterCircleRadius = value;
				Invalidate();
			}
		}

		/// <summary>
		/// Gets or sets the radius of the inner circle.
		/// </summary>
		/// <value>The inner circle radius.</value>
		[Description("Gets or sets the radius of the inner circle.")]
		[Category("Appearance")]
		public int InnerCircleRadius {
			get {
				if (m_InnerCircleRadius == 0)
					m_InnerCircleRadius = DefaultInnerCircleRadius;

				return m_InnerCircleRadius;
			}
			set {
				m_InnerCircleRadius = value;
				Invalidate();
			}
		}

		/// <summary>
		/// Gets or sets the number of spokes.
		/// </summary>
		/// <value>The number of spokes.</value>
		[Description("Gets or sets the number of spokes.")]
		[Category("Appearance")]
		public int SpokeCount {
			get {
				if (m_NumberOfSpoke == 0)
					m_NumberOfSpoke = DefaultNumberOfSpoke;

				return m_NumberOfSpoke;
			}
			set {
				if (m_NumberOfSpoke != value && m_NumberOfSpoke > 0) {
					m_NumberOfSpoke = value;
					GenerateColorsPallete();
					UpdateSpokesAngles();

					Invalidate();
				}
			}
		}

		/// <summary>
		/// Gets or sets a value indicating whether this <see cref="T:LoadingCircle"/> is active.
		/// </summary>
		/// <value><c>true</c> if active; otherwise, <c>false</c>.</value>
		[Description("Indicates whether the animation is running.")]
		[Category("Behavior")]
		public bool Active {
			get {
				return m_IsTimerActive;
			}
			set {
				m_IsTimerActive = value;
				UpdateTimer();
			}
		}

		/// <summary>
		/// Gets or sets the spoke thickness.
		/// </summary>
		/// <value>The spoke thickness.</value>
		[Description("Gets or sets the thickness of a spoke.")]
		[Category("Appearance")]
		public int SpokeThickness {
			get {
				if (m_SpokeThickness <= 0)
					m_SpokeThickness = DefaultSpokeThickness;

				return m_SpokeThickness;
			}
			set {
				m_SpokeThickness = value;
				Invalidate();
			}
		}

		/// <summary>
		/// Gets or sets the rotation speed.
		/// </summary>
		/// <value>The rotation speed.</value>
		[Description("Gets or sets the rotation speed. Higher values result in a slower animation.")]
		[Category("Appearance")]
		public int RotationSpeed {
			get {
				return m_Timer.Interval;
			}
			set {
				if (value > 0)
					m_Timer.Interval = value;
			}
		}

		/// <summary>
		/// Quickly sets the style to one of these presets, or a custom style if desired
		/// </summary>
		/// <value>The style preset.</value>
		[Category("Appearance")]
		[Description("Quickly sets the style to one of these presets, or a custom style if desired")]
		[DefaultValue(typeof(PresetLoadingStyle), "Custom")]
		[RefreshProperties(RefreshProperties.All)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public PresetLoadingStyle StylePreset {
			get { return m_StylePreset; }
			set {
				m_StylePreset = value;

				switch (m_StylePreset) {
					case PresetLoadingStyle.MacOSX:
						SetCircleAppearance(MacOSXNumberOfSpoke,
							MacOSXSpokeThickness, MacOSXInnerCircleRadius,
							MacOSXOuterCircleRadius);
						break;
					case PresetLoadingStyle.Firefox:
						SetCircleAppearance(FireFoxNumberOfSpoke,
							FireFoxSpokeThickness, FireFoxInnerCircleRadius,
							FireFoxOuterCircleRadius);
						break;
					case PresetLoadingStyle.IE7:
						SetCircleAppearance(IE7NumberOfSpoke,
							IE7SpokeThickness, IE7InnerCircleRadius,
							IE7OuterCircleRadius);
						break;
					case PresetLoadingStyle.Custom:
						SetCircleAppearance(DefaultNumberOfSpoke,
							DefaultSpokeThickness,
							DefaultInnerCircleRadius,
							DefaultOuterCircleRadius);
						break;
				}
			}
		}

		// Construtor ========================================================
		/// <summary>
		/// Initializes a new instance of the <see cref="T:LoadingCircle"/> class.
		/// </summary>
		public LoadingCircle() {
			SetStyle(ControlStyles.UserPaint, true);
			SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
			SetStyle(ControlStyles.ResizeRedraw, true);
			SetStyle(ControlStyles.SupportsTransparentBackColor, true);
			TabStop = false;

			baseColor = DefaultColor;

			GenerateColorsPallete();
			UpdateSpokesAngles();
			UpdateCenterPoint();

			m_Timer = new Timer();
			m_Timer.Tick += aTimer_Tick;
			UpdateTimer();
		}

		///<summary>Raises the Resize event.</summary>
		protected override void OnResize(EventArgs e) {
			base.OnResize(e);
			UpdateCenterPoint();
		}

		// Events ============================================================

		/// <summary>
		/// Handles the Tick event of the aTimer control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="T:System.EventArgs"/> instance containing the event data.</param>
		void aTimer_Tick(object sender, EventArgs e) {
			m_ProgressValue = ++m_ProgressValue % m_NumberOfSpoke;
			Invalidate();
		}

		/// <summary>
		/// Raises the <see cref="E:System.Windows.Forms.Control.Paint"></see> event.
		/// </summary>
		/// <param name="e">A <see cref="T:System.Windows.Forms.PaintEventArgs"></see> that contains the event data.</param>
		protected override void OnPaint(PaintEventArgs e) {
			if (m_NumberOfSpoke > 0) {
				e.Graphics.SmoothingMode = SmoothingMode.HighQuality;

				int intPosition = m_ProgressValue;
				for (int intCounter = 0; intCounter < m_NumberOfSpoke; intCounter++) {
					intPosition = intPosition % m_NumberOfSpoke;
					DrawLine(e.Graphics,
							 GetCoordinate(center, m_InnerCircleRadius, m_Angles[intPosition]),
							 GetCoordinate(center, m_OuterCircleRadius, m_Angles[intPosition]),
							 spokeColors[intCounter], m_SpokeThickness);
					intPosition++;
				}
			}

			base.OnPaint(e);
		}

		// Overridden Methods ================================================
		/// <summary>
		/// Retrieves the size of a rectangular area into which a control can be fitted.
		/// </summary>
		/// <param name="proposedSize">The custom-sized area for a control.</param>
		/// <returns>
		/// An ordered pair of type <see cref="T:System.Drawing.Size"></see> representing the width and height of a rectangle.
		/// </returns>
		public override Size GetPreferredSize(Size proposedSize) {
			proposedSize.Width = proposedSize.Height = (m_OuterCircleRadius + m_SpokeThickness) * 2;

			return proposedSize;
		}

		// Methods ===========================================================
		/// <summary>
		/// Darkens a specified color.
		/// </summary>
		/// <param name="color">Color to darken.</param>
		/// <param name="percent">The percent of darken.</param>
		/// <returns>The new darker color.</returns>
		private static Color Darken(Color color, int percent) {
			return Color.FromArgb(percent, Math.Min(color.R, byte.MaxValue), Math.Min(color.G, byte.MaxValue), Math.Min(color.B, byte.MaxValue));
		}

		/// <summary>
		/// Generates the colors pallet.
		/// </summary>
		private void GenerateColorsPallete() {
			bool useGradient = Active;
			spokeColors = new Color[SpokeCount];

			// Value is used to simulate a gradient feel... For each spoke, the 
			// color will be darkened by darkStep.
			byte darkStep = (byte)(byte.MaxValue / SpokeCount);

			//Reset variable in case of multiple passes
			byte darkness = 0;

			for (int intCursor = 0; intCursor < SpokeCount; intCursor++) {
				if (!useGradient)
					spokeColors[intCursor] = baseColor;
				else {
					if (intCursor == 0)
						spokeColors[intCursor] = baseColor;
					else {
						// Increment alpha channel color
						darkness += darkStep;

						// Ensure that we don't exceed the maximum alpha
						// channel value (255)
						if (darkness > byte.MaxValue)
							darkness = byte.MaxValue;

						// Determine the spoke forecolor
						spokeColors[intCursor] = Darken(baseColor, darkness);
					}
				}
			}
		}

		private void UpdateCenterPoint() {
			center = new Point(Width / 2, Height / 2 - 1);
		}

		/// <summary>
		/// Draws the line with GDI+.
		/// </summary>
		/// <param name="g">The Graphics object.</param>
		/// <param name="from">The point one.</param>
		/// <param name="to">The point two.</param>
		/// <param name="color">Color of the spoke.</param>
		/// <param name="thickness">The thickness of spoke.</param>
		private static void DrawLine(Graphics g, PointF from, PointF to,
							  Color color, int thickness) {
			using (Pen objPen = new Pen(new SolidBrush(color), thickness)) {
				objPen.StartCap = LineCap.Round;
				objPen.EndCap = LineCap.Round;
				g.DrawLine(objPen, from, to);
			}
		}

		private static PointF GetCoordinate(PointF center, int radius, double angle) {
			double dblAngle = Math.PI * angle / NumberOfDegreesInHalfCircle;

			return new PointF(center.X + radius * (float)Math.Cos(dblAngle),
							  center.Y + radius * (float)Math.Sin(dblAngle));
		}

		private void UpdateSpokesAngles() {
			m_Angles = new double[SpokeCount];
			double dblAngle = (double)NumberOfDegreesInCircle / SpokeCount;

			m_Angles[0] = dblAngle;
			for (int i = 1; i < SpokeCount; i++)
				m_Angles[i] = m_Angles[i - 1] + dblAngle;
		}
		//TODO: Call this instead
		double GetAngle(int spokeIndex) {
			double angleStep = (double)NumberOfDegreesInCircle / SpokeCount;
			return angleStep * spokeIndex;
		}

		private void UpdateTimer() {
			if (m_IsTimerActive)
				m_Timer.Start();
			else {
				m_Timer.Stop();
				m_ProgressValue = 0;
			}

			GenerateColorsPallete();
			Invalidate();
		}

		/// <summary>
		/// Sets the circle appearance.
		/// </summary>
		/// <param name="numberSpoke">The number spoke.</param>
		/// <param name="spokeThickness">The spoke thickness.</param>
		/// <param name="innerCircleRadius">The inner circle radius.</param>
		/// <param name="outerCircleRadius">The outer circle radius.</param>
		public void SetCircleAppearance(int numberSpoke, int spokeThickness,
			int innerCircleRadius, int outerCircleRadius) {
			SpokeCount = numberSpoke;
			SpokeThickness = spokeThickness;
			InnerCircleRadius = innerCircleRadius;
			OuterCircleRadius = outerCircleRadius;
		}
	}
}