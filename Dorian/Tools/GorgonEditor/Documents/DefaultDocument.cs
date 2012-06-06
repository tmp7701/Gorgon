﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using System.ComponentModel;
using KRBTabControl;
using SlimMath;
using GorgonLibrary.Diagnostics;
using GorgonLibrary.Graphics;
using GorgonLibrary.Renderers;

namespace GorgonLibrary.GorgonEditor
{
	/// <summary>
	/// A document for the editor.
	/// </summary>
	class DefaultDocument
		: Document
	{
		#region Variables.
		private GorgonTexture2D _logo = null;					// Logo.
		private RectangleF[] _blurStates = null;				// Images for blur states.
		private RectangleF _sourceState = RectangleF.Empty;		// Source image state.
		private RectangleF _destState = RectangleF.Empty;		// Destination image state.
		private float _alphaDelta = 0.025f;						// Alpha delta value.
		private float _alpha = 0.0f;							// Alpha value.
		private GorgonFont _debugFont = null;					// Debug font.	
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the type of document.
		/// </summary>
		[Browsable(false)]
		public override string DocumentType
		{
			get
			{
				return string.Empty;
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to draw to the control.
		/// </summary>
		/// <param name="timing">Timing data.</param>
		/// <returns>Number of vertical retraces to wait.</returns>
		protected override int Draw(GorgonFrameRate timing)
		{
			RectangleF logoBounds = RectangleF.Empty;
			SizeF logoSize = _logo.Settings.Size;
			float aspect = 0.0f;

			logoSize.Height = 124;

			if (RenderWindow.ClientSize.Width < logoSize.Width)
				logoBounds.Width = logoSize.Width * RenderWindow.ClientSize.Width / logoSize.Width;
			else
				logoBounds.Width = logoSize.Width;

			aspect = logoSize.Height / logoSize.Width;
			logoBounds.Height = logoBounds.Width * aspect;

			logoBounds.X = RenderWindow.ClientSize.Width / 2.0f - logoBounds.Width / 2.0f;
			logoBounds.Y = RenderWindow.ClientSize.Height / 2.0f - logoBounds.Height / 2.0f;

			Renderer.Drawing.SmoothingMode = SmoothingMode.Smooth;
			Renderer.Drawing.FilledRectangle(new RectangleF(2, logoBounds.Y, RenderWindow.ClientSize.Width - 4, logoBounds.Height), Color.FromArgb(160, 160, 160));

			Renderer.Drawing.BlendingMode = BlendingMode.PreMultiplied;
			Renderer.Drawing.FilledRectangle(logoBounds, new GorgonColor(1, 1, 1, 1.0f - _alpha), _logo, _sourceState);
			Renderer.Drawing.BlendingMode = BlendingMode.Modulate;
			Renderer.Drawing.FilledRectangle(logoBounds, new GorgonColor(1, 1, 1, _alpha), _logo, _destState);

			_alpha += _alphaDelta * timing.FrameDelta;

			if ((_alpha > 1.0f) && (_alphaDelta > 0))
			{
				if (_destState != _blurStates[0])
				{
					_sourceState = _blurStates[1];
					_destState = _blurStates[0];
					_alpha = 0.0f;
				}
				else
				{
					_destState = _blurStates[0];
					_sourceState = _blurStates[1];
					_alpha = 1.0f;
					_alphaDelta = -_alphaDelta;
				}
			}

			if ((_alpha < 0.0f) && (_alphaDelta < 0))
			{
				if (_destState != _blurStates[1])
				{
					_destState = _blurStates[1];
					_sourceState = _blurStates[2];
					_alpha = 1.0f;
				}
				else
				{
					_destState = _blurStates[1];
					_sourceState = _blurStates[2];
					_alpha = 0.0f;
					_alphaDelta = -_alphaDelta;
				}
			}

			Renderer.Drawing.DrawString(_debugFont, "FPS: " + timing.FPS.ToString("0.0##"), Vector2.Zero, Color.White);

			return 2;
		}

		/// <summary>
		/// Function to initialize graphics and other items for the document.
		/// </summary>
		protected override void LoadGraphics()
		{
			SizeF logoSize = SizeF.Empty;
			GorgonRenderTarget blur = null;	// Medium blur.

			_logo = Program.Graphics.Textures.FromGDIBitmap("Logo", Properties.Resources.Gorgon_2_Logo_Full, new GorgonTexture2DSettings()
			{
				Width = 0,
				Height = 372,
				FileFilter = ImageFilters.None,
				FileMipFilter = ImageFilters.None,
				Format = BufferFormat.Unknown,
				ArrayCount = 1,
				IsTextureCube = false,
				MipCount = 1,
				Usage = BufferUsage.Default
			});

			logoSize = new SizeF(_logo.Settings.Width, 124.0f);

			blur = Program.Graphics.Output.CreateRenderTarget("Blur.Medium", new GorgonRenderTargetSettings()
			{
				Width = (int)logoSize.Width,
				Height = (int)logoSize.Height,
				Format = _logo.Settings.Format
			});

			try
			{
				// Blur the logo.
				blur.Clear(Control.BackColor);

				// Maximum blur.
				Renderer.Effects.GaussianBlur.BlurRenderTargetsSize = new Size(64, 64);
				Renderer.Effects.GaussianBlur.BlurAmount = 3.2f;
				Renderer.Effects.GaussianBlur.Render((int pass) =>
				{
					if (pass == 0)
						Renderer.Drawing.FilledRectangle(new RectangleF(Vector2.Zero, Renderer.Effects.GaussianBlur.BlurRenderTargetsSize), Color.White, _logo, new RectangleF(Vector2.Zero, logoSize));
					else
					{
						Renderer.Target = blur;
						Renderer.Drawing.FilledRectangle(new RectangleF(Vector2.Zero, logoSize), Color.White, Renderer.Effects.GaussianBlur.BlurredTexture, new RectangleF(Vector2.Zero, Renderer.Effects.GaussianBlur.BlurRenderTargetsSize));
					}
				});
				_logo.CopySubResource(blur.Texture, new Rectangle(0, 0, blur.Settings.Width, blur.Settings.Height), new Vector2(0, 124));

				// Medium blur.
				Renderer.Effects.GaussianBlur.BlurRenderTargetsSize = new Size(128, 128);
				Renderer.Effects.GaussianBlur.BlurAmount = 2.7f;
				Renderer.Effects.GaussianBlur.Render((int pass) =>
				{
					if (pass == 0)
						Renderer.Drawing.FilledRectangle(new RectangleF(Vector2.Zero, Renderer.Effects.GaussianBlur.BlurRenderTargetsSize), Color.White, _logo, new RectangleF(Vector2.Zero, logoSize));
					else
					{
						Renderer.Target = blur;
						Renderer.Drawing.FilledRectangle(new RectangleF(Vector2.Zero, logoSize), Color.White, Renderer.Effects.GaussianBlur.BlurredTexture, new RectangleF(Vector2.Zero, Renderer.Effects.GaussianBlur.BlurRenderTargetsSize));
					}
				});
				_logo.CopySubResource(blur.Texture, new Rectangle(0, 0, blur.Settings.Width, blur.Settings.Height), new Vector2(0, 248));

				_blurStates = new[] {
					new RectangleF(0, 0, _logo.Settings.Width, 124),		// No blur.
					new RectangleF(0, 248, _logo.Settings.Width, 124),		// Medium blur.
					new RectangleF(0, 124, _logo.Settings.Width, 124),		// Max blur.
					};

				_sourceState = _blurStates[2];
				_destState = _blurStates[1];

				Renderer.Target = null;
			}
			finally
			{
				Renderer.Effects.GaussianBlur.FreeResources();

				if (blur != null)
					blur.Dispose();
			}

			_debugFont = Program.Graphics.Fonts.CreateFont("My font", new GorgonFontSettings()
			{
				AntiAliasingMode = FontAntiAliasMode.AntiAliasHQ,
				FontFamilyName = "Arial",
				FontStyle = FontStyle.Bold,
				FontHeightMode = FontHeightMode.Points,
				Size = 14.0f
			});
		}

		/// <summary>
		/// Function to remove graphics and other items for the document.
		/// </summary>
		protected override void UnloadGraphics()
		{
			if (_debugFont != null)
				_debugFont.Dispose();

			if (_logo != null)
				_logo.Dispose();

			_debugFont = null;
			_logo = null;
		}

		/// <summary>
		/// Function to initialize the editor control.
		/// </summary>
		/// <returns>
		/// The editor control.
		/// </returns>
		protected override Control InitializeEditorControl()
		{
			Control result = new controlDefault();
			result.Name = "default" + Name;
			result.BackColor = Color.FromKnownColor(KnownColor.DimGray);
			result.Dock = DockStyle.Fill;
			RenderWindow = ((controlDefault)result).panelLogo;

			return result;
		}
		#endregion

		#region Constructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="DefaultDocument"/> class.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <param name="allowClose">TRUE to allow the document to close, FALSE to keep open.</param>
		/// <param name="folder">Folder that contains the document.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="name"/> parameter is NULL (Nothing in VB.Net).</exception>
		///   
		/// <exception cref="System.ArgumentException">Thrown when the <paramref name="name"/> parameter is an empty string.</exception>
		public DefaultDocument(string name, bool allowClose, ProjectFolder folder)
			: base(name, allowClose, folder)
		{
			TabImageIndex = 0;
		}
		#endregion
	}
}