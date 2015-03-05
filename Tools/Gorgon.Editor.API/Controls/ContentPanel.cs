﻿#region MIT.
// 
// Gorgon.
// Copyright (C) 2013 Michael Winsor
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
// 
// Created: Saturday, March 9, 2013 3:36:32 PM
// 
#endregion

using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using GorgonLibrary.Design;
using GorgonLibrary.Editor.Properties;
using GorgonLibrary.UI;

namespace GorgonLibrary.Editor
{
	/// <summary>
	/// A base interface for content display in the main interface.
	/// </summary>
	public partial class ContentPanel
		: UserControl, IContentPanel
	{
		#region Variables.
		// Flag to indicate that the caption is visible.
		private bool _captionVisible = true;
		// The theme used by the parent for this control.
		private FlatFormTheme _theme;
		// Control used to receive rendering.
		private Control _renderControl;
		// The panel where content will be placed.
		[System.Runtime.CompilerServices.AccessedThroughProperty("PanelDisplay")]
		private Panel _panelContentDisplay;
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the display panel.
		/// </summary>
        [Browsable(false)]
		public Panel PanelDisplay
		{
			get
			{
				return _panelContentDisplay;
			}
		}

		/// <summary>
		/// Property to set or return the content object to be manipulated by this interface.
		/// </summary>
		[Browsable(false)]
		public IContent Content
		{
            get;
			private set;
		}

	    /// <summary>
	    /// Property to set or return the text caption for this control.
	    /// </summary>
	    [Browsable(true)]
	    [LocalCategory(typeof(Resources), "PROP_CATEGORY_APPEARANCE")]
	    [LocalDescription(typeof(Resources), "PROP_TEXT_CONTENT_PANEL_DESC")]
	    [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
	    public new virtual string Text
	    {
	        get
	        {
	            return base.Text;
	        }
	        set
	        {
	            base.Text = value;
	            UpdateCaption();
	        }
	    }
		#endregion

		#region Methods.
		/// <summary>
		/// Handles the BeforeClosedEvent event of the Content control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="eventArgs">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void Content_BeforeClosedEvent(object sender, EventArgs eventArgs)
		{
			if (Content == null)
			{
				return;
			}

			// If we have rendering happening, then turn it off.
			if (Renderer != null)
			{
				Renderer.Dispose();
				Renderer = null;
			}

			OnBeforeContentClosed();

			// Disable our link to the content.
			Content = null;
		}

		/// <summary>
		/// Handles the MouseEnter event of the labelClose control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void labelClose_MouseEnter(object sender, EventArgs e)
		{
			labelClose.ForeColor = _theme.WindowCloseIconForeColorHilight;
			labelClose.BackColor = _theme.WindowCloseIconBackColorHilight;
		}

		/// <summary>
		/// Handles the MouseLeave event of the labelClose control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void labelClose_MouseLeave(object sender, EventArgs e)
		{
			labelClose.ForeColor = _theme.WindowCloseIconForeColor;
			labelClose.BackColor = _theme.WindowBackground;
		}

		/// <summary>
		/// Handles the MouseMove event of the labelClose control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="MouseEventArgs"/> instance containing the event data.</param>
		private void labelClose_MouseMove(object sender, MouseEventArgs e)
		{
			labelClose.ForeColor = _theme.WindowCloseIconForeColorHilight;
			labelClose.BackColor = _theme.WindowCloseIconBackColorHilight;
		}

		/// <summary>
		/// Handles the Click event of the labelClose control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void labelClose_Click(object sender, EventArgs e)
		{
            Cursor.Current = Cursors.WaitCursor;

            try
            {
				if (Content == null)
				{
					return;
				}

				labelClose.Enabled = false;

                OnCloseClicked();
            }
            catch (Exception ex)
            {
                GorgonDialogs.ErrorBox(ParentForm, ex);
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }
		}

		/// <summary>
		/// Function to set the theme for the control.
		/// </summary>
		private void SetTheme()
		{
			BackColor = _theme.WindowBackground;
			ForeColor = _theme.ForeColor;
			_panelContentDisplay.BackColor = _theme.ContentPanelBackground;
			labelClose.ForeColor = _theme.WindowCloseIconForeColor;

			ApplyTheme(_theme);
		}

		/// <summary>
		/// Function called before the content object is closed.
		/// </summary>
		/// <remarks>This method can be overridden to provide clean up facilities on the UI when the content is closed.</remarks>
		protected virtual void OnBeforeContentClosed()
		{
			// We have nothing in the base class to make use of this.	
		}

		/// <summary>
		/// Raises the <see cref="E:System.Windows.Forms.UserControl.Load" /> event.
		/// </summary>
		/// <param name="e">An <see cref="T:System.EventArgs" /> that contains the event data.</param>
		protected override void OnLoad(EventArgs e)
		{
			if (!DesignMode)
			{
				SetTheme();		
			}

			base.OnLoad(e);
		}

		/// <summary>
		/// Function called when the close button is clicked.
		/// </summary>
		protected virtual void OnCloseClicked()
		{
			if (CloseClick != null)
			{
				CloseClick(this, new GorgonCancelEventArgs(false));
			}
		}

		/// <summary>
		/// Function called when a property is changed on the related content.
		/// </summary>
		/// <param name="propertyName">Name of the property.</param>
		/// <param name="value">New value assigned to the property.</param>
		protected internal virtual void OnContentPropertyChanged(string propertyName, object value)
		{
			UpdateCaption();
		}

        /// <summary>
        /// Function called when the settings for the editor or content plug-in have changed.
        /// </summary>
        /// <remarks>Plug-in implementors should implement this method to facilitate the updating of the UI when a plug-in setting has changed.  This 
        /// only applies to plug-ins that implement <see cref="GorgonLibrary.Editor.IPlugInSettingsUI"/>.</remarks>
        protected internal virtual void OnEditorSettingsChanged()
        {
        }

		/// <summary>
		/// Function to apply theme settings to any child controls that may need it.
		/// </summary>
		protected virtual void ApplyTheme(FlatFormTheme theme)
		{
		}

		/// <summary>
		/// Function called when the content data has been persisted.
		/// </summary>
		internal void ContentPersisted()
		{
			RefreshContent();
		}

		/// <summary>
		/// Function to update the caption label.
		/// </summary>
		private void UpdateCaption()
		{
			if (Content == null)
			{
				labelCaption.Text = string.Format("{0} - {1}", base.Text, Resources.GOREDIT_TEXT_UNTITLED);
				return;
			}

			labelCaption.Text = string.Format("{0} - {1}{2}", base.Text, Content.Name, Content.HasChanges ? "*" : string.Empty);
		}

		/// <summary>
        /// Function called when the content has changed.
        /// </summary>
        public virtual void RefreshContent()
		{
			UpdateCaption();
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="ContentPanel"/> class.
		/// </summary>
		public ContentPanel()
		{
			InitializeComponent();			
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ContentPanel"/> class.
		/// </summary>
		/// <param name="content">The content.</param>
		/// <param name="renderer">The renderer to use to display the content.</param>
		public ContentPanel(IContent content, IEditorContentRenderer renderer)
		{
			if (content == null)
			{
				throw new ArgumentNullException("content");
			}

			// Done in both places to keep the designer happy.
			InitializeComponent();

			Content = content;
			UpdateCaption();
			Renderer = renderer;
			content.BeforeContentClosed += Content_BeforeClosedEvent;
		}
		#endregion

		#region IContentPanel Members
		#region Events.
		/// <summary>
		/// Event fired when the close button is clicked.
		/// </summary>
		public event EventHandler<GorgonCancelEventArgs> CloseClick;
		#endregion

		#region Properties.
		/// <summary>
		/// Property to set or return the current theme for the application.
		/// </summary>
		[Browsable(false)]
		public FlatFormTheme CurrentTheme
		{
			get
			{
				return _theme;
			}
			set
			{
				if (_theme == value)
				{
					return;
				}

				_theme = value;
				ApplyTheme(value);
			}
		}

		/// <summary>
		/// Property to set or return whether the caption for the content panel is visible or not.
		/// </summary>
		[Browsable(true)]
		[LocalCategory(typeof(Resources), "PROP_CATEGORY_APPEARANCE")]
		[LocalDescription(typeof(Resources), "PROP_TEXT_CAPTION_VISIBLE_DESC")]
		[DefaultValue(true)]
		public bool CaptionVisible
		{
			get
			{
				return _captionVisible;
			}
			set
			{
				_captionVisible = value;
				panelCaption.Visible = _captionVisible;
			}
		}


		/// <summary>
		/// Property to set or return whether the control uses an external renderer.
		/// </summary>
		[Browsable(false)]
		public bool UsesRenderer
		{
			get
			{
				return Renderer != null;
			}
		}

		/// <summary>
		/// Property to set or return the control that will receive rendering.
		/// </summary>
		/// <remarks>When this property is set to NULL (Nothing in VB.Net), then the content area of this control will be used for rendering.</remarks>
		[Browsable(false)]
		public Control RenderControl
		{
			get
			{
				return _renderControl ?? _panelContentDisplay;
			}
			set
			{
				_renderControl = value;
			}
		}

		/// <summary>
		/// Property to set or return the renderer for the content panel.
		/// </summary>
		[Browsable(false)]
		public IEditorContentRenderer Renderer
		{
			get;
			private set;
		}
		#endregion
		#endregion
	}
}