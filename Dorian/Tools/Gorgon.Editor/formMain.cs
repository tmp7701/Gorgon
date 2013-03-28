﻿#region MIT.
// 
// Gorgon.
// Copyright (C) 2012 Michael Winsor
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
// Created: Monday, April 30, 2012 6:28:32 PM
// 
#endregion

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using KRBTabControl;
using GorgonLibrary.FileSystem;
using GorgonLibrary.Diagnostics;
using GorgonLibrary.UI;
using GorgonLibrary.Graphics;
using GorgonLibrary.IO;

namespace GorgonLibrary.Editor
{
	/// <summary>
	/// Main application object.
	/// </summary>
	public partial class formMain
		: ZuneForm
    {
        #region Variables.
		private static int _syncCounter = 0;							// Synchronization counter for multiple threads.
		private static readonly object _syncLock = new object();		// Synchronization lock.
        private RootNodeDirectory _rootNode = null;						// Our root node for the tree.
		private char[] _pathChars = Path.GetInvalidPathChars();			// Invalid path characters.
		private char[] _fileChars = Path.GetInvalidFileNameChars();		// Invalid filename characters.
		private object _cutCopyObject = null;							// Object being cut/copied.
		private bool? _isCutOperation = null;							// Flag for cut/copy operation.
		#endregion

		#region Methods.
        /// <summary>
        /// Handles the PropertyValueChanged event of the propertyItem control.
        /// </summary>
        /// <param name="s">The source of the event.</param>
        /// <param name="e">The <see cref="PropertyValueChangedEventArgs"/> instance containing the event data.</param>
        private void propertyItem_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
        {
            try
            {
                if ((Program.CurrentContent != null) && (Program.CurrentContent.HasProperties))
                {
                    Program.CurrentContent.PropertyChanged(e);
                }
            }
            catch (Exception ex)
            {
                GorgonDialogs.ErrorBox(this, ex);
            }
        }

        /// <summary>
        /// Function to validate the controls on the display.
        /// </summary>
        private void ValidateControls()
        {
            Text = Program.EditorFile + " - Gorgon Editor";

			if (_rootNode != null)
			{
				_rootNode.Redraw();
			}

            itemOpen.Enabled = Program.ScratchFiles.Providers.Count > 1;
            itemSaveAs.Enabled = (Program.WriterPlugIns.Count > 0);
            itemSave.Enabled = !string.IsNullOrWhiteSpace(Program.EditorFilePath) 
								&& ((Program.EditorFileChanged) 
									|| ((Program.CurrentContent != null) && (Program.CurrentContent.HasChanges)))
								&& itemSaveAs.Enabled;

            // Ensure we have plug-ins that can import.
			itemImport.Enabled = Program.ContentPlugIns.Any(item => item.Value.SupportsImport);
            // Check to see if the current content can export.
            itemExport.Enabled = ((Program.CurrentContent != null) && (Program.CurrentContent.CanExport));

            itemAdd.Enabled = false;
            popupItemAdd.Enabled = false;
            dropNewContent.Enabled = false;
            itemDelete.Enabled = popupItemDelete.Enabled = false;
            itemDelete.Text = popupItemDelete.Text = "Delete...";
            itemDelete.Visible = popupItemDelete.Visible = true;
            itemEdit.Visible = false;
            toolStripSeparator5.Visible = true;
            itemRenameFolder.Visible = true;
            itemRenameFolder.Enabled = false;
            itemRenameFolder.Text = "Rename...";
            itemCreateFolder.Visible = false;
			buttonEditContent.Enabled = false;
			buttonDeleteContent.Enabled = false;
			popupItemCut.Enabled = popupItemCopy.Enabled = popupItemPaste.Enabled = itemCopy.Enabled = itemCut.Enabled = itemPaste.Enabled = false;

            EditorTreeNode node = treeFiles.SelectedNode as EditorTreeNode;

            // No node is the same as selecting the root.
            if (treeFiles.SelectedNode == null)
            {
                node = _rootNode;
            }
                                
            if (node is TreeNodeDirectory)
            {
                itemAdd.Enabled = itemAdd.DropDownItems.Count > 0;
                popupItemAdd.Enabled = itemAdd.Enabled;
                dropNewContent.Enabled = dropNewContent.DropDownItems.Count > 0;
				buttonDeleteContent.Enabled = true;
                itemCreateFolder.Enabled = true;
                itemCreateFolder.Visible = true;
				itemDelete.Enabled = popupItemDelete.Enabled = (tabDocumentManager.SelectedTab == pageItems);
				itemDelete.Text = popupItemDelete.Text = "Delete Folder...";
				popupItemPaste.Enabled = itemPaste.Enabled = (_cutCopyObject != null);
					
				if (node != _rootNode)
                {
					popupItemCut.Enabled = popupItemCopy.Enabled = itemCopy.Enabled = itemCut.Enabled = true;
					itemRenameFolder.Enabled = true;
                    itemRenameFolder.Text = "Rename Folder...";
                }
                else
                {                        
					if (_rootNode.Nodes.Count == 0)
					{
                        buttonDeleteContent.Enabled = false;
						itemDelete.Visible = popupItemDelete.Visible = false;
					}
					else
					{
						itemDelete.Text = popupItemDelete.Text = "Delete all files and folders...";
					}

                    toolStripSeparator5.Visible = false;
                    itemRenameFolder.Visible = false;
                }                    
            }

            if (node is TreeNodeFile)
            {
				GorgonFileSystemFileEntry file = ((TreeNodeFile)node).File;

				popupItemPaste.Enabled = itemPaste.Enabled = (_cutCopyObject != null);
				popupItemCut.Enabled = popupItemCopy.Enabled = itemCopy.Enabled = itemCut.Enabled = true;
				buttonDeleteContent.Enabled = true;
				buttonEditContent.Enabled = itemEdit.Visible = itemEdit.Enabled = Program.ContentPlugIns.Any(item => item.Value.FileExtensions.ContainsKey(file.Extension.ToLower()));
                itemDelete.Enabled = popupItemDelete.Enabled = (tabDocumentManager.SelectedTab == pageItems);
                itemRenameFolder.Enabled = true;
            }
        }

		/// <summary>
		/// Function to handle the content "open/edit" event.
		/// </summary>
		private void ContentOpen()
		{
			TreeNodeFile fileNode = null;
			
			// If we have no node selected, then assume it's the top of the chain.
			if ((treeFiles.SelectedNode != null) && (!(treeFiles.SelectedNode is TreeNodeDirectory)))
			{
				fileNode = treeFiles.SelectedNode as TreeNodeFile;
			}
			else
			{
				return;
			}

			// Otherwise, we need to open this file.
			if (fileNode.PlugIn == null)
			{
				throw new IOException("Cannot open '" + fileNode.File.FullPath + "'.  There are no content plug-ins loaded that can open '" + fileNode.File.Extension + "' files.");
			}

			var content = fileNode.PlugIn.CreateContentObject();

			// Open the content from the file system.
			content.OpenContent(fileNode.File);

            // Open the content pane.
            LoadContentPane(ref content);

			treeFiles.Refresh();
		}

        /// <summary>
        /// Handles the Click event of the itemEdit control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void itemEdit_Click(object sender, EventArgs e)
        {
            Point pos = treeFiles.PointToClient(Cursor.Position);
            treeFiles_NodeMouseDoubleClick(this, new TreeNodeMouseClickEventArgs(treeFiles.SelectedNode, System.Windows.Forms.MouseButtons.Left, 1, pos.X, pos.Y));
        }
        
        /// <summary>
		/// Handles the NodeMouseDoubleClick event of the treeFiles control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="TreeNodeMouseClickEventArgs"/> instance containing the event data.</param>
		private void treeFiles_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
		{
			Cursor.Current = Cursors.WaitCursor;

			try
			{
				ContentOpen();
			}
			catch (Exception ex)
			{
				GorgonDialogs.ErrorBox(this, ex);
			}
			finally
			{
				Cursor.Current = Cursors.Default;
				ValidateControls();
			}
		}

		/// <summary>
		/// Handles the KeyDown event of the treeFiles control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="KeyEventArgs"/> instance containing the event data.</param>
		private void treeFiles_KeyDown(object sender, KeyEventArgs e)
		{			
			switch(e.KeyCode)
			{
				case Keys.Enter:
					{
						Cursor.Current = Cursors.WaitCursor;

						try
						{
							ContentOpen();
						}
						catch (Exception ex)
						{
							GorgonDialogs.ErrorBox(this, ex);
						}
						finally
						{
							Cursor.Current = Cursors.Default;
						}

						break;
					}
				case Keys.F2:
					if ((treeFiles.SelectedNode != null) && (itemRenameFolder.Enabled))
					{
						treeFiles.SelectedNode.BeginEdit();
					}
					break;
			}
		}

		/// <summary>
		/// Handles the AfterSelect event of the treeFiles control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="TreeViewEventArgs"/> instance containing the event data.</param>
		private void treeFiles_AfterSelect(object sender, TreeViewEventArgs e)
		{
			ValidateControls();
		}

		/// <summary>
		/// Handles the Click event of the buttonEditContent control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void buttonEditContent_Click(object sender, EventArgs e)
		{
			Cursor.Current = Cursors.WaitCursor;

			try
			{
				ContentOpen();
			}
			catch (Exception ex)
			{
				GorgonDialogs.ErrorBox(this, ex);
			}
			finally
			{
				ValidateControls();
				Cursor.Current = Cursors.Default;
			}
		}
        
        /// <summary>
		/// Handles the Click event of the itemExit control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void itemExit_Click(object sender, EventArgs e)
		{
			try
			{				
				Close();
			}
			catch (Exception ex)
			{
				GorgonDialogs.ErrorBox(this, ex);
			}
		}

		/// <summary>
		/// Handles the Click event of the itemResetValue control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void itemResetValue_Click(object sender, EventArgs e)
		{
			try
			{
				if ((propertyItem.SelectedObject == null) || (propertyItem.SelectedGridItem == null))
					return;

				propertyItem.SelectedGridItem.PropertyDescriptor.ResetValue(propertyItem.SelectedObject);
				propertyItem.Refresh();
			}
			catch (Exception ex)
			{
				GorgonDialogs.ErrorBox(this, ex);
			}
		}

		/// <summary>
		/// Handles the Opening event of the popupProperties control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.ComponentModel.CancelEventArgs"/> instance containing the event data.</param>
		private void popupProperties_Opening(object sender, CancelEventArgs e)
		{
			if ((propertyItem.SelectedObject == null) || (propertyItem.SelectedGridItem == null))
			{
				itemResetValue.Enabled = false;
				return;
			}

			itemResetValue.Enabled = (propertyItem.SelectedGridItem.PropertyDescriptor.CanResetValue(propertyItem.SelectedObject));
		}

		/// <summary>
		/// Function to load a content object into the main interface.
		/// </summary>
		/// <param name="contentPreLoad">Content object to load in.</param>
		private void LoadContentPane(ref ContentObject contentPreLoad)
		{
			ContentObject content = contentPreLoad;
			Control control = null;

			// Turn off rendering.
			Gorgon.ApplicationIdleLoopMethod = null;

			if (content == null)
			{
				content = new DefaultContent();		
			}

			// If we have content loaded, ensure we get a chance to save it.
			if (Program.CurrentContent != null)
			{
				if (!Program.CurrentContent.Close())
				{
					if (contentPreLoad != null)
					{
						contentPreLoad.Dispose();
						contentPreLoad = null;
					}

					return;
				}

				// Destroy the previous content.
                Program.CurrentContent.ContentPropertyChanged -= CurrentContent_ContentPropertyChanged;
				Program.CurrentContent.Dispose();
				Program.CurrentContent = null;

				treeFiles.Refresh();
			}

			// Create the content resources.
			control = content.InitializeContent();

			// We couldn't get an interface component, fall back to the default display.
			if (control == null)
			{
				content.Dispose();
				content = new DefaultContent();
				control = content.InitializeContent();
				contentPreLoad = null;
			}

			control.Dock = DockStyle.Fill;

			// Add to the main interface.
			Program.CurrentContent = content;
			splitPanel1.Controls.Add(control);

			// Activate the content.
			content.Activate();

			// If the current content has a renderer, then activate it.
			// Otherwise, turn it off to conserve cycles.
			if (content.HasRenderer)
			{
				Gorgon.ApplicationIdleLoopMethod = Idle;
			}

            if (content.HasProperties)
            {
                pageProperties.Enabled = true;
                propertyItem.SelectedObject = Program.CurrentContent.TypeDescriptor;
                propertyItem.Refresh();
                tabDocumentManager.SelectedTab = pageProperties;
    
                // Set up event.
                content.ContentPropertyChanged += CurrentContent_ContentPropertyChanged;
            }
            else
            {                
                tabDocumentManager.SelectedTab = pageItems;
                pageProperties.Enabled = false;
            }
		}

        /// <summary>
        /// Handles the ContentPropertyChanged event of the CurrentContent control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="ContentPropertyChangedEventArgs"/> instance containing the event data.</param>
        private void CurrentContent_ContentPropertyChanged(object sender, ContentPropertyChangedEventArgs e)
        {
            ContentObject content = sender as ContentObject;

			try
			{
				// If we change the name of an item, we must rename it.
				switch (e.PropertyName.ToLower())
				{
					case "name":
						// Find our node that corresponds to this content.
						var sourceNode = FindNode(content.File.FullPath) as TreeNodeFile;
						var destNode = sourceNode.Parent as TreeNodeDirectory;

						if ((sourceNode != null) && (destNode != null))
						{
							CopyFileNode(sourceNode, destNode, e.Value.ToString(), true);
						}
						break;
					default:
						Program.EditorFileChanged = true;
						break;
				}
			}
			catch (Exception ex)
			{
				GorgonDialogs.ErrorBox(this, ex);
			}
			finally
			{
				ValidateControls();
			}
        }

		/// <summary>
		/// Function to find the editor node.
		/// </summary>
		/// <param name="name">Name of the node to find.</param>
		/// <returns>The node if found, NULL if not.</returns>
		private EditorTreeNode FindNode(string name)
		{
			EditorTreeNode result = null;
			EditorTreeNode current = null;
			IList<string> parts = name.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);

			// Start at the root.
			if (!_rootNode.IsExpanded)
			{
				_rootNode.Expand();
			}

			current = _rootNode;
			while (result == null)
			{
				if (!current.IsExpanded)
				{
					current.Expand();
				}

				// We can't find a node in here, leave.
				if (current.Nodes.Count == 0)
				{
					break;
				}

				// Find the parts.
				for (int i = 0; i < current.Nodes.Count; i++)
				{
					EditorTreeNode nodePart = current.Nodes[i] as EditorTreeNode;
					int index = parts.IndexOf(nodePart.Text);

					// This node type is invalid, skip it.
					if (nodePart == null)
					{
						continue;
					}

					// We found our node, exit the loop.
					if (string.Compare(nodePart.Name, name, true) == 0)
					{
						result = nodePart;
						break;
					}

					if (index > -1)
					{
						current = current.Nodes[i] as EditorTreeNode;
						if (current != null)
						{
							break;
						}
					}
				}
			}

			return result;
		}

		/// <summary>
		/// Function to determine if the specified node is an ancestor of the parent node.
		/// </summary>
		/// <param name="childNode">Child node.</param>
		/// <param name="parentNode">Root parent node.</param>
		/// <returns>TRUE if the child is ancestor of parent, FALSE if not.</returns>
		private bool IsAncestor(EditorTreeNode childNode, EditorTreeNode parentNode)
		{
			TreeNode node = childNode.Parent;

			// Walk up the chain.
			while (node != null)
			{
				if (node == parentNode)
				{
					return true;
				}

				node = node.Parent;
			}

			return false;
		}

		/// <summary>
		/// Handles the Click event of the itemPaste control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void itemPaste_Click(object sender, EventArgs e)
		{
			try
			{
				// Use a lockless pattern to keep this method from becoming reentrant.
				if (Interlocked.Increment(ref _syncCounter) == 1)
				{
					Cursor.Current = Cursors.WaitCursor;

					try
					{
						if ((_cutCopyObject == null) || (_isCutOperation == null))
						{
							return;
						}

						// If we're cutting/copying some tree node, then find the current node
						// and perform the operation.
						var node = FindNode(_cutCopyObject.ToString());

						node.IsCut = false;
						node.Redraw();

						if (node != null)
						{
							TreeNodeDirectory dest = treeFiles.SelectedNode as TreeNodeDirectory;

							if (dest == null)
							{
								GorgonDialogs.ErrorBox(this, "Cannot paste into a non-directory node.");
								return;
							}

							// If we're moving, there are some restrictions.
							if ((dest == node) && (_isCutOperation.Value))
							{
								GorgonDialogs.ErrorBox(this, "Cannot cut and paste this item onto itself.");
								return;
							}

							if ((_isCutOperation.Value) && (IsAncestor(dest, node)))
							{
								GorgonDialogs.ErrorBox(this, "Cannot cut and paste this item onto its child directories.");
								return;
							}

							if (node is TreeNodeDirectory)
							{
								CopyDirectoryNode((TreeNodeDirectory)node, dest, node.Text, _isCutOperation.Value);
							}

							if (node is TreeNodeFile)
							{
								// Don't copy over the same file.
								if ((dest == node.Parent) && (_isCutOperation.Value))
								{
									return;
								}

								CopyFileNode((TreeNodeFile)node, dest, node.Text, _isCutOperation.Value);
							}
						}
					}
					catch (Exception ex)
					{
						GorgonDialogs.ErrorBox(this, ex);
					}
					finally
					{
						if ((_isCutOperation != null) && (_isCutOperation.Value))
						{
							_cutCopyObject = null;
							_isCutOperation = null;
						}
						ValidateControls();
						Cursor.Current = Cursors.Default;
					}
				}
			}
			finally
			{
				Interlocked.Decrement(ref _syncCounter);
			}
		}

		/// <summary>
		/// Handles the Click event of the itemCopy control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void itemCopy_Click(object sender, EventArgs e)
		{
			try
			{
				if ((treeFiles.SelectedNode == null) || (treeFiles.SelectedNode == _rootNode))
				{
					return;
				}

				EditorTreeNode node = (EditorTreeNode)treeFiles.SelectedNode;
				node.IsCut = false;
				node.Redraw();
				_isCutOperation = false;
				_cutCopyObject = node.Name;
			}
			finally
			{
				ValidateControls();
			}
		}

		/// <summary>
		/// Handles the Click event of the itemCut control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void itemCut_Click(object sender, EventArgs e)
		{
			try
			{
				if ((treeFiles.SelectedNode == null) || (treeFiles.SelectedNode == _rootNode))
				{
					return;
				}

				EditorTreeNode node = (EditorTreeNode)treeFiles.SelectedNode;
				node.IsCut = true;
				node.Redraw();
				_isCutOperation = true;
				_cutCopyObject = node.Name;
			}
			finally
			{
				ValidateControls();
			}
		}

		/// <summary>
		/// Handles the Click event of the itemNew control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void itemNew_Click(object sender, EventArgs e)
		{
			Cursor.Current = Cursors.WaitCursor;
			try
			{
				// Save any changes we have.
				if (ConfirmSave())
				{
					return;
				}

				Program.NewEditorFile();

				// Rebuild our tree.
				InitializeTree();
				
				// Load the default content panel.
				LoadContentPane<DefaultContent>();
			}
			catch (Exception ex)
			{
				GorgonDialogs.ErrorBox(this, ex);
			}
			finally
			{
				ValidateControls();
				Cursor.Current = Cursors.Default;
			}
		}

		/// <summary>
		/// Function to load content into the interface.
		/// </summary>
		/// <typeparam name="T">Type of content object to load.</typeparam>
		internal void LoadContentPane<T>()
			where T : ContentObject, new()
		{
			// Don't re-open the same screen.
			if ((Program.CurrentContent != null) && (typeof(T) == Program.CurrentContent.GetType()))
			{
				return;
			}

			// Load the content.
			ContentObject result = new T();
			LoadContentPane(ref result);
		}

		/// <summary>
		/// Function to pop up the save confirmation dialog.
		/// </summary>
		/// <returns>TRUE if canceled, FALSE if not.</returns>
		private bool ConfirmSave()
		{
			if ((Program.EditorFileChanged) && (Program.WriterPlugIns.Count > 0))
			{
				var result = GorgonDialogs.ConfirmBox(this, "The editor file '" + Program.EditorFile + "' has unsaved changes.  Would you like to save these changes?", true, false);

				if (result == ConfirmationResult.Cancel)
				{
					return true;
				}

				if (result == ConfirmationResult.Yes)
				{
					// If we have content open and it hasn't been persisted to the file system, 
					// then persist those changes.
					if ((Program.CurrentContent != null) && (Program.CurrentContent.HasChanges))
					{
						if (!Program.CurrentContent.Close())
						{
							return true;
						}

					}

					// If we haven't saved the file yet, then prompt us with a file name.
					if (string.IsNullOrWhiteSpace(Program.EditorFilePath))
					{
						itemSaveAs_Click(this, EventArgs.Empty);
					}
					else
					{
						itemSave_Click(this, EventArgs.Empty);
					}
				}
			}

			return false;
		}

		/// <summary>
		/// Raises the <see cref="E:System.Windows.Forms.Form.FormClosing"/> event.
		/// </summary>
		/// <param name="e">A <see cref="T:System.Windows.Forms.FormClosingEventArgs"/> that contains the event data.</param>
		protected override void OnFormClosing(FormClosingEventArgs e)
		{
			base.OnFormClosing(e);

			try
			{
				if (ConfirmSave())
				{
					e.Cancel = true;
					return;
				}

				// Destroy the current content.
                Program.CurrentContent.ContentPropertyChanged -= CurrentContent_ContentPropertyChanged;
				Program.CurrentContent.Dispose();
				Program.CurrentContent = null;
				
				if (this.WindowState != FormWindowState.Minimized)
				{
					Program.Settings.FormState = this.WindowState;
				}

				if (this.WindowState != FormWindowState.Normal)
				{
					Program.Settings.WindowDimensions = this.RestoreBounds;
				}
				else
				{
					Program.Settings.WindowDimensions = this.DesktopBounds;
				}
                
                // Remember the last file we had open.
                if (!string.IsNullOrWhiteSpace(Program.EditorFilePath))
                {
                    Program.Settings.LastEditorFile = Program.EditorFilePath;
                }

				Program.Settings.Save();
			}
#if DEBUG
			catch (Exception ex)
			{
				GorgonDialogs.ErrorBox(this, ex);
#else
			catch
			{
				// Eat this exception if in release.
#endif
			}
		}

		/// <summary>
		/// Function for idle time.
		/// </summary>
		/// <returns>TRUE to continue, FALSE to exit.</returns>
		private bool Idle()
		{
			Program.CurrentContent.Draw();

			return true;
		}

		/// <summary>
		/// Handles the BeforeExpand event of the treeFiles control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="TreeViewCancelEventArgs"/> instance containing the event data.</param>
		private void treeFiles_BeforeExpand(object sender, TreeViewCancelEventArgs e)
		{
			Cursor.Current = Cursors.WaitCursor;

			try
			{
				TreeNodeDirectory directoryNode = e.Node as TreeNodeDirectory;

				// Expand sub folders.
				if (directoryNode != null)
				{
					GetFolders(directoryNode);
					return;
				}
			}
			catch (Exception ex)
			{
				GorgonDialogs.ErrorBox(this, ex);
			}
			finally
			{
				ValidateControls();
				Cursor.Current = Cursors.Default;
			}
		}

		/// <summary>
		/// Function to retrieve the folder nodes.
		/// </summary>
		/// <param name="rootNode">Node to add folder information into.</param>
		private void GetFolders(TreeNodeDirectory rootNode)
		{
			// Get the sub directories.
			rootNode.Nodes.Clear();

			foreach (var subDirectory in rootNode.Directory.Directories.OrderBy(item => item.Name))
			{
				TreeNodeDirectory subNode = new TreeNodeDirectory(subDirectory);

				if ((subDirectory.Directories.Count > 0) 
                    || ((subDirectory.Files.Count > 0)
                    && (CanShowDirectoryFiles(subDirectory))))
				{
					subNode.Nodes.Add(new TreeNode("DummyNode"));
				}

				rootNode.Nodes.Add(subNode);
			}

			// Add file nodes.
			foreach (var file in rootNode.Directory.Files.OrderBy(item => item.Name))
			{
                // Do not display the blocked file.
                if (Program.BlockedFiles.Contains(file.Name))
                {
                    continue;
                }

				TreeNodeFile fileNode = new TreeNodeFile(file);
				rootNode.Nodes.Add(fileNode);
			}
		}

        /// <summary>
        /// Function to determine if all the files in this directory are blocked or not.
        /// </summary>
        /// <param name="directory">Directory to evaluate.</param>
        /// <returns>TRUE if some files can be shown, FALSE if not.</returns>
        private bool CanShowDirectoryFiles(GorgonFileSystemDirectory directory)
        {
			return directory.Files.Count(item => Program.BlockedFiles.Contains(item.Name)) < directory.Files.Count;
        }

		/// <summary>
		/// Function to initialize the files tree.
		/// </summary>
		private void InitializeTree()
		{            
			_rootNode = new RootNodeDirectory();

			// If we have files or sub directories, dump them in here.
			if ((Program.ScratchFiles.RootDirectory.Directories.Count > 0) 
                || ((Program.ScratchFiles.RootDirectory.Files.Count > 0)
                && (CanShowDirectoryFiles(Program.ScratchFiles.RootDirectory))))
			{
                _rootNode.Nodes.Add(new TreeNode("DummyNode"));
			}

			treeFiles.BeginUpdate();
            treeFiles.Nodes.Clear();
			treeFiles.Nodes.Add(_rootNode);
            if (_rootNode.Nodes.Count > 0)
            {
                _rootNode.Expand();
            }
			treeFiles.EndUpdate();
		}

        /// <summary>
        /// Function to retrieve the directory from the selected node.
        /// </summary>
        /// <returns>The selected node.</returns>
        private TreeNodeDirectory GetDirectoryFromNode()
        {
			TreeNodeDirectory directory = null;

            if (treeFiles.SelectedNode != null)
            {
				directory = treeFiles.SelectedNode as TreeNodeDirectory;		

				if (directory == null)
                {					
                    // If we've got a file hilighted, then add to the same directory that we're in.
					TreeNode parentNode = treeFiles.SelectedNode.Parent;

					while (!(parentNode is TreeNodeDirectory))
					{
						parentNode = parentNode.Parent;

						if (parentNode == null)
						{
							break;
						}
					}

					directory = (TreeNodeDirectory)parentNode;
                }

				directory.Expand();
            }
            else
            {
				treeFiles.Nodes[0].Expand();
				directory = ((TreeNodeDirectory)treeFiles.Nodes[0]);
            }

            return directory;
        }

        /// <summary>
        /// Function to create a new file node in the tree.
        /// </summary>
        /// <param name="content">Content object to use.</param>
        private void CreateNewFileNode(ContentObject content)
        {
            var directoryNode = GetDirectoryFromNode();
			string filePath = directoryNode.Directory.FullPath + content.Name;
            GorgonFileSystemFileEntry file = Program.ScratchFiles.GetFile(filePath);
            TreeNodeFile newNode = null;
            string extension = Path.GetExtension(content.Name).ToLower();

			if (file != null)
			{
				throw new IOException("The " + content.ContentType + " '" + filePath + "' already exists.");
			}

			// Write the file.
			file = Program.ScratchFiles.WriteFile(directoryNode.Directory.FullPath + content.Name, null);
			newNode = new TreeNodeFile(file);

			content.HasChanges = true;
			content.Persist(file);

            // Add to our changed item list.            

			// Add to tree and select.
			directoryNode.Nodes.Add(newNode);
			treeFiles.SelectedNode = newNode;

			// We set this to true to indicate that this is a new file.
			Program.EditorFileChanged = true;
		}

		/// <summary>
		/// Function to add content to the interface.
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void AddContent(object sender, EventArgs e)
		{		
			ContentObject content = null;
			ToolStripMenuItem item = sender as ToolStripMenuItem;
			ContentPlugIn plugIn = null;

			if (item == null)
			{
				return;
			}

			plugIn = item.Tag as ContentPlugIn;

			if (plugIn == null)
			{
				return;
			}

			Cursor.Current = Cursors.WaitCursor;
			try
			{
				content = plugIn.CreateContentObject();

				// Create the content settings.
				if (!content.CreateNew())
				{
					content.Dispose();
					content = null;
					return;
				}

                // Reset to a wait cursor.
                Cursor.Current = Cursors.WaitCursor;

                // Show the content in the editor.
				LoadContentPane(ref content);

				if (content != null)
				{
					// Create the node in the tree.
					CreateNewFileNode(content);
				}
			}
			catch (Exception ex)
			{
                // Load the default pane.
                LoadContentPane<DefaultContent>();

                GorgonDialogs.ErrorBox(this, ex);

				if (content != null)
				{
					content.Dispose();
					content = null;
				}
			}
			finally
			{
				treeFiles.Refresh();
				Cursor.Current = Cursors.Default;
                ValidateControls();
			}
		}

		/// <summary>
		/// Handles the Click event of the itemSave control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void itemSave_Click(object sender, EventArgs e)
		{
			Cursor.Current = Cursors.WaitCursor;

			try
			{
				// Ensure we can actually write the file.
				if ((Program.CurrentWriterPlugIn == null) || (string.IsNullOrWhiteSpace(Program.EditorFilePath)))
				{
					return;
				}

				Program.SaveEditorFile(Program.EditorFilePath);

				treeFiles.Refresh();
			}
			catch (Exception ex)
			{
				GorgonDialogs.ErrorBox(this, ex);
			}
			finally
			{
				ValidateControls();
				Cursor.Current = Cursors.Default;
			}
		}

		/// <summary>
		/// Function to delete a directory and all the files and subdirectories underneath it.
		/// </summary>
		/// <param name="directoryNode">The node for the directory.</param>
		private void DeleteDirectory(TreeNodeDirectory directoryNode)
		{
			Cursor.Current = Cursors.WaitCursor;

			if ((_cutCopyObject != null) && (_cutCopyObject.ToString() == directoryNode.Name))
			{
				_cutCopyObject = null;
			}

			// If we've selected the root node, then we need to destroy everything.
			if (directoryNode == _rootNode)
			{
				// Wipe out all the files/subdirs under the root.
				Program.ScratchFiles.DeleteDirectory("/");

				LoadContentPane<DefaultContent>();

				_rootNode.Nodes.Clear();
				Program.EditorFileChanged = true;
				return;
			}

			if ((Program.CurrentContent != null) && (Program.CurrentContent.File != null))
			{
				// If we have this file open, then close it.
				var files = Program.ScratchFiles.FindFiles(directoryNode.Directory.FullPath, Program.CurrentContent.File.Name, true).Any(item => item == Program.CurrentContent.File);

				if (files)
				{
                    Program.CurrentContent.ContentPropertyChanged -= CurrentContent_ContentPropertyChanged;
					Program.CurrentContent.Dispose();
					Program.CurrentContent = null;

					LoadContentPane<DefaultContent>();
				}
			}

			Program.ScratchFiles.DeleteDirectory(directoryNode.Directory.FullPath);
			Program.EditorFileChanged = true;
			if (directoryNode.Parent.Nodes.Count == 1)
			{
				directoryNode.Parent.Collapse();
			}
			directoryNode.Remove();
			treeFiles.Refresh();
		}

		/// <summary>
		/// Function to delete a file.
		/// </summary>
		/// <param name="fileNode">The node for the file.</param>
		private void DeleteFile(TreeNodeFile fileNode)
		{
			Cursor.Current = Cursors.WaitCursor;

			if ((_cutCopyObject != null) && (_cutCopyObject.ToString() == fileNode.Name))
			{
				_cutCopyObject = null;
			}

			if ((Program.CurrentContent != null) && (Program.CurrentContent.File == fileNode.File))
			{
                Program.CurrentContent.ContentPropertyChanged -= CurrentContent_ContentPropertyChanged;
				Program.CurrentContent.Dispose();
				Program.CurrentContent = null;

				LoadContentPane<DefaultContent>();
			}

			Program.ScratchFiles.DeleteFile(fileNode.File.FullPath);
			Program.EditorFileChanged = true;
			if (fileNode.Parent.Nodes.Count == 1)
			{
				fileNode.Parent.Collapse();
			}
			fileNode.Remove();
			treeFiles.Refresh();
		}

		/// <summary>
		/// Handles the Click event of the itemDelete control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void itemDelete_Click(object sender, EventArgs e)
		{
			try
			{
				if (treeFiles.SelectedNode == null)
				{
					return;
				}

				TreeNodeDirectory directory = treeFiles.SelectedNode as TreeNodeDirectory;

				if (directory != null)
				{
                    if (GorgonDialogs.ConfirmBox(this, "This will delete '" + directory.Directory.FullPath + "' and any files and/or directories it contains.  Are you sure you wish to do this?") == ConfirmationResult.No)
                    {
                        return;
                    }			
										
					DeleteDirectory(directory);
				}
				else
				{
					TreeNodeFile file = treeFiles.SelectedNode as TreeNodeFile;

					if (file != null)
					{
                        if (GorgonDialogs.ConfirmBox(this, "This will delete '" + file.File.FullPath + "'.  Are you sure you wish to do this?") == ConfirmationResult.No)
                        {
                            return;
                        }

						DeleteFile(file);
					}
				}
			}
			catch (Exception ex)
			{
				GorgonDialogs.ErrorBox(this, ex);
			}
			finally
			{
				Cursor.Current = Cursors.Default;
			}
		}

		/// <summary>
		/// Handles the Click event of the itemSaveAs control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void itemSaveAs_Click(object sender, EventArgs e)
		{
			List<FileWriterPlugIn> plugIns = new List<FileWriterPlugIn>();
			StringBuilder extensions = new StringBuilder(512);
			int counter = 0;
			int filterIndex = 0;
			string extension = string.Empty;

			try
			{
				foreach (var writerPlugIn in Program.WriterPlugIns)
				{
					// Create extensions for the dialog.
					foreach (var extensionValue in writerPlugIn.Value.FileExtensions)
					{
						if (extensions.Length > 0)
						{
							extensions.Append("|");
						}

						var wildCardExtension = extensionValue.Value.Item1;
						if (!wildCardExtension.StartsWith("*."))
						{
							wildCardExtension = "*." + wildCardExtension;
						}
						
						extensions.AppendFormat("{0}|{1}", extensionValue.Value.Item2, wildCardExtension);

						// If we have a current writer plug-in selected, then pick it as the default extension and index.
						if (Program.CurrentWriterPlugIn == writerPlugIn.Value)
						{
							extension = writerPlugIn.Value.FileExtensions.First().Value.Item2;

							if (extension.StartsWith("."))
							{
								extension = extension.Substring(1);						
							}

							filterIndex = counter;
						}

						plugIns.Add(writerPlugIn.Value);
						counter++;
					}					
				}

				// Set default extension and correct filter.
				if (string.IsNullOrWhiteSpace(extension))
				{
					dialogSaveFile.DefaultExt = Program.WriterPlugIns.First().Value.FileExtensions.First().Value.Item2;
					dialogSaveFile.FilterIndex = 1;
				}
				else
				{
					dialogSaveFile.DefaultExt = extension;
					dialogSaveFile.FilterIndex = filterIndex + 1;
				}
				dialogSaveFile.Filter = extensions.ToString();

				if (!string.IsNullOrWhiteSpace(Program.Settings.LastEditorFile))
				{
					dialogSaveFile.FileName = Path.GetFileName(Program.Settings.LastEditorFile);
				}

				// Open dialog.
				if (dialogSaveFile.ShowDialog(this) == System.Windows.Forms.DialogResult.OK)
				{					
					Cursor.Current = Cursors.WaitCursor;
					Program.CurrentWriterPlugIn = plugIns[dialogSaveFile.FilterIndex - 1];
					Program.SaveEditorFile(dialogSaveFile.FileName);

					treeFiles.Refresh();
				}
			}
			catch (Exception ex)
			{
				GorgonDialogs.ErrorBox(this, ex);
			}
			finally
			{
				ValidateControls();
				Cursor.Current = Cursors.Default;
			}
		}

		/// <summary>
		/// Handles the ItemDrag event of the treeFiles control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="ItemDragEventArgs"/> instance containing the event data.</param>
		private void treeFiles_ItemDrag(object sender, ItemDragEventArgs e)
		{
			try
			{
				var node = e.Item as EditorTreeNode;
				
				if ((node != null) && (node != _rootNode))
				{
					treeFiles.DoDragDrop(new Tuple<EditorTreeNode, MouseButtons>(node, e.Button), DragDropEffects.Copy | DragDropEffects.Move | DragDropEffects.Link);
				}
			}
			catch (Exception ex)
			{
				GorgonDialogs.ErrorBox(this, ex);
			}
		}

		/// <summary>
		/// Handles the AfterLabelEdit event of the treeFiles control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="NodeLabelEditEventArgs"/> instance containing the event data.</param>
		private void treeFiles_AfterLabelEdit(object sender, NodeLabelEditEventArgs e)
		{
			EditorTreeNode node = e.Node as EditorTreeNode;
			NodeEditState currentState = NodeEditState.None;
			string label = e.Label;

			Cursor.Current = Cursors.WaitCursor;

			try
			{
				e.CancelEdit = true;

				// This node isn't an editor node, we should get rid of it.
				if (node == null)
				{
					if (e.Node != null)
					{
						e.Node.Remove();
					}
					return;
				}

				// Ensure that whatever we type will work in the file system.
				if ((label.IndexOfAny(_fileChars) > -1) || (label.IndexOf('/') > -1) || (label.IndexOf('\\') > -1))
				{
					GorgonDialogs.ErrorBox(this, "The directory name contains invalid characters.\nThe following characters are not allowed:\n" + string.Join<char>(" ", _fileChars.Where(item => item > 32)));
					node.BeginEdit();
					return;
				}

				if ((node.EditState == NodeEditState.CreateDirectory) || (node.EditState == NodeEditState.RenameDirectory))
				{
					currentState = node.EditState;

					// Create the directory in the scratch area.
					if (currentState == NodeEditState.CreateDirectory)
					{
						// Empty strings can't be used.
						if (string.IsNullOrWhiteSpace(label))
						{
							e.Node.Remove();
							return;
						}

						label = label.FormatDirectory('/');

						TreeNodeDirectory parentNode = node.Parent as TreeNodeDirectory;
						var newDirectory = Program.ScratchFiles.CreateDirectory(parentNode.Directory.FullPath + label);

						// Set up a new node for the directory since our current node is here as a proxy.
						var treeNode = new TreeNodeDirectory(newDirectory);

						int nodeIndex = parentNode.Nodes.IndexOf(e.Node);
						parentNode.Nodes.Insert(nodeIndex, treeNode);
						// Remove proxy node.						
						e.Node.Remove();

						Program.EditorFileChanged = true;
					}
					else
					{
						if (string.IsNullOrWhiteSpace(label))
						{
							return;
						}

						// Rename the directory by moving it.
						TreeNodeDirectory selectedNode = node as TreeNodeDirectory;
						TreeNodeDirectory parentNode = selectedNode.Parent as TreeNodeDirectory;

                        CopyDirectoryNode(selectedNode, parentNode, label, true);
					}

					return;
				}
				else
				{
					if (string.IsNullOrWhiteSpace(label))
					{
						return;
					}
					
					// Rename the file by moving it.
					TreeNodeFile selectedNode = node as TreeNodeFile;
					TreeNodeDirectory parentNode = selectedNode.Parent as TreeNodeDirectory;

                    CopyFileNode(selectedNode, parentNode, label, true);
					return;
				}
			}
			catch (Exception ex)
			{
				// If we get an error, just remove any new nodes.
				if (((currentState == NodeEditState.CreateDirectory) || (currentState == NodeEditState.CreateFile))
					&& (node != null))
				{
					node.Remove();
				}

				e.CancelEdit = true;
				GorgonDialogs.ErrorBox(this, ex);

				if (Program.CurrentContent == null)
				{
					LoadContentPane<DefaultContent>();
				}
			}
			finally
			{
				Cursor.Current = Cursors.Default;
				ValidateControls();
			}
		}

		/// <summary>
		/// Handles the BeforeLabelEdit event of the treeFiles control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="NodeLabelEditEventArgs"/> instance containing the event data.</param>
		private void treeFiles_BeforeLabelEdit(object sender, NodeLabelEditEventArgs e)
		{
			try
			{
				if (e.Node == null)
				{
					return;
				}

				// We're being clever, don't allow that.
				if (string.IsNullOrWhiteSpace(e.Label))
				{					
					e.CancelEdit = true;
					e.Node.Remove();
					return;
				}

				// Do not attempt to rename the top level.
				if ((e.Node == _rootNode) || (e.Node.Parent == null))
				{
					e.CancelEdit = true;
					return;
				}
			}
			catch (Exception ex)
			{
				GorgonDialogs.ErrorBox(this, ex);
			}
			finally
			{
				Cursor.Current = Cursors.Default;
				ValidateControls();
			}
		}

		/// <summary>
		/// Handles the Click event of the itemRenameFolder control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void itemRenameFolder_Click(object sender, EventArgs e)
		{
			EditorTreeNode selectedNode = treeFiles.SelectedNode as EditorTreeNode;

			try
			{
				if (selectedNode == null)
				{
					return;
				}

				if (selectedNode is TreeNodeDirectory)
				{
					selectedNode.EditState = NodeEditState.RenameDirectory;
				}
				else
				{
					selectedNode.EditState = NodeEditState.RenameFile;
				}

				selectedNode.BeginEdit();
			}
			catch (Exception ex)
			{
				GorgonDialogs.ErrorBox(this, ex);
			}
			finally
			{
				Cursor.Current = Cursors.Default;
			}
		}

		/// <summary>
		/// Function to add a directory to the after the last directory in the parent, but before any files.
		/// </summary>
		/// <param name="parent">Parent node.</param>		
		/// <param name="newNode">New Node to add.</param>
		private void AddAfterLastFolder(EditorTreeNode parent, EditorTreeNode newNode)
		{
			// Add after other folders.
			var lastFolder = (from node in parent.Nodes.Cast<EditorTreeNode>()
							  where node is TreeNodeDirectory
							  select node).LastOrDefault();

			if (lastFolder != null)
			{
				int index = parent.Nodes.IndexOf(lastFolder);
				if (index + 1 < parent.Nodes.Count)
				{
					parent.Nodes.Insert(index + 1, newNode);
				}
				else
				{
					parent.Nodes.Add(newNode);
				}
			}
			else
			{
				parent.Nodes.Insert(0, newNode);
			}			
		}

		/// <summary>
		/// Handles the Click event of the itemCreateFolder control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void itemCreateFolder_Click(object sender, EventArgs e)
		{
			bool expandDisabled = false;
			EditorTreeNode tempNode = new EditorTreeNode();

			try
			{
				int nameIndex = -1;
				string defaultName = "Untitled";
				TreeNodeDirectory selectedNode = treeFiles.SelectedNode as TreeNodeDirectory;

				if (selectedNode == null)
				{
					return;
				}

				// Expand the node if it's not already done.
				if (!selectedNode.IsExpanded)
				{
					selectedNode.Expand();
				}

				tempNode.CollapsedImage = Properties.Resources.folder_16x16;

				// Update the name.
				while ((selectedNode.Directory.Directories.Contains(defaultName))
						|| (selectedNode.Directory.Files.Contains(defaultName)))
				{
					nameIndex++;
					defaultName = "Untitled_" + nameIndex.ToString();
				}

				tempNode.Text = defaultName;

				AddAfterLastFolder(selectedNode, tempNode);

				if ((!selectedNode.IsExpanded) && (selectedNode.Nodes.Count == 1))
				{
					treeFiles.BeforeExpand -= treeFiles_BeforeExpand;
					expandDisabled = true;
				}

				selectedNode.Expand();

				tempNode.BeginEdit();
				tempNode.EditState = NodeEditState.CreateDirectory;
			}
			catch (Exception ex)
			{
				tempNode.EndEdit(true);

				GorgonDialogs.ErrorBox(this, ex);
			}
			finally
			{
				if (expandDisabled)
				{
					treeFiles.BeforeExpand += treeFiles_BeforeExpand;
				}
				Cursor.Current = Cursors.Default;
			}
		}

        /// <summary>
        /// Function to copy a file node to another location.
        /// </summary>
        /// <param name="sourceFile">The file to copy.</param>
        /// <param name="destDirectory">The destination directory.</param>
        /// <param name="name">The new name for the file.</param>
        /// <param name="deleteSource">TRUE to delete the source file, FALSE to leave alone.</param>
        private void CopyFileNode(TreeNodeFile sourceFile, TreeNodeDirectory destDirectory, string name, bool deleteSource)
        {
            ConfirmationResult result = ConfirmationResult.None;

            if (string.IsNullOrWhiteSpace(name))
            {
                name = sourceFile.File.Name;
            }

            // If we dropped the extension, then replace it.
            if (!name.EndsWith(sourceFile.File.Extension, StringComparison.CurrentCultureIgnoreCase))
            {
                name += sourceFile.File.Extension;
            }

            var newFilePath = destDirectory.Directory.FullPath + name.FormatFileName();

            if (Program.ScratchFiles.GetFile(newFilePath) != null)
            {
                // If the file exists and we're renaming (not moving), then throw up an error and leave.
                if (deleteSource)
                {
                    if (string.Compare(name, sourceFile.File.Name, true) != 0)
                    {
                        GorgonDialogs.ErrorBox(this, "The file '" + newFilePath + "' already exists.");
                        return;
                    }
                    else
                    {
                        // We're trying to copy over ourselves, do nothing.
                        return;
                    }
                }

                result = GorgonDialogs.ConfirmBox(this, "The file '" + newFilePath + "' already exists.  Would you like to overwrite it?", true, false);

                if (result == ConfirmationResult.Cancel)
                {
                    return;
                }

                // If we specified no, then we have to create a new name.
				if (result == ConfirmationResult.No)
				{
					int counter = 1;
					string newName = sourceFile.File.BaseFileName + " (" + counter.ToString() + ")" + sourceFile.File.Extension;

					while (Program.ScratchFiles.GetFile(newName) != null)
					{
						newName = sourceFile.File.BaseFileName + " (" + (++counter).ToString() + ")" + sourceFile.File.Extension;
					}

					newFilePath = destDirectory.Directory.FullPath + newName;
				}
				else
				{
					// Do not copy over ourselves.
					if (string.Compare(newFilePath, sourceFile.File.FullPath, true) == 0)
					{
						return;
					}
				}
            }

			Cursor.Current = Cursors.WaitCursor;

            using (var inputStream = sourceFile.File.OpenStream(false))
            {
                using (var outputStream = Program.ScratchFiles.OpenStream(newFilePath, true))
                {
                    inputStream.CopyTo(outputStream);
                }
            }

			// Reroute any cut/copy operation.
			if (deleteSource)
			{
				if ((_cutCopyObject != null) && (_cutCopyObject.ToString() == sourceFile.Name))
				{
					_cutCopyObject = newFilePath;
				}

				// If this file is open, then update its handle.
				if ((Program.CurrentContent != null)
					&& (Program.CurrentContent.File == sourceFile.File))
				{
					var newFile = Program.ScratchFiles.GetFile(newFilePath);
					Program.CurrentContent.File = newFile;
				}
			}

            destDirectory.Collapse();

            if (destDirectory.Nodes.Count == 0)
            {
                destDirectory.Nodes.Add(new TreeNode("DUMMYNODE"));
            }

            if (deleteSource)
            {
                DeleteFile(sourceFile);
            }

            destDirectory.Expand();

            treeFiles.SelectedNode = destDirectory.Nodes[newFilePath];

            Program.EditorFileChanged = true;
        }

        /// <summary>
        /// Function to copy a directory node to another location.
        /// </summary>
        /// <param name="sourceDirectory">Directory to copy.</param>
        /// <param name="destDirectory">Directory to copy into.</param>
        /// <param name="name">The new name for the directory.</param>
        /// <param name="deleteSource">TRUE to delete the source directory, FALSE to leave alone.</param>
        private void CopyDirectoryNode(TreeNodeDirectory sourceDirectory, TreeNodeDirectory destDirectory, string name, bool deleteSource)
        {
            GorgonFileSystemFileEntry currentFile = null;
            ConfirmationResult result = ConfirmationResult.None;
            bool wasExpanded = sourceDirectory.IsExpanded;

            if (string.IsNullOrWhiteSpace(name))
            {
                name = sourceDirectory.Directory.Name;
            }

            // We're moving to the same place... leave.
            if ((deleteSource) && (sourceDirectory.Parent == destDirectory) && (string.Compare(name, sourceDirectory.Directory.Name, true) == 0))
            {
                return;
            }


            name = (destDirectory.Directory.FullPath + name).FormatDirectory('/');

            // We have a directory with this new name already, throw an error.
            if ((deleteSource) && (string.Compare(name, sourceDirectory.Directory.Name, true) != 0) && (Program.ScratchFiles.GetDirectory(name) != null))
            {
                GorgonDialogs.ErrorBox(this, "The directory '" + name + "' already exists.");
                return;
            }

            // Get the currently open file.
            if (Program.CurrentContent != null)
            {
                currentFile = Program.CurrentContent.File;
            }

            var directories = new List<GorgonFileSystemDirectory>(Program.ScratchFiles.FindDirectories(sourceDirectory.Directory.FullPath, "*", true));
            directories.Add(sourceDirectory.Directory);            
            
            foreach (var directory in directories)
            {
                // Update the path to point at the new parent.
                var newDirPath = name + directory.FullPath.Substring(directory.FullPath.Length);
                var newDirectory = Program.ScratchFiles.CreateDirectory(newDirPath);

                // Copy each file.
                foreach (var file in directory.Files)
                {
                    var newFilePath = newDirPath + file.Name;

                    // The file exists in the destination.  Ask the user what to do.
                    if (Program.ScratchFiles.GetFile(newFilePath) != null)
                    {
                        if ((result & ConfirmationResult.ToAll) == 0)
                        {
                            result = GorgonDialogs.ConfirmBox(this, "The file '" + newFilePath + "' already exists.  Would you like to overwrite it?", true, true);
							Cursor.Current = Cursors.WaitCursor;
                        }

						if (result == ConfirmationResult.Cancel)
						{
							break;
						}

                        // If we specified no, then we have to create a new name.
						if ((result & ConfirmationResult.No) == ConfirmationResult.No)
						{
							int counter = 1;
							string newName = file.BaseFileName + " (" + counter.ToString() + ")" + file.Extension;

							while (Program.ScratchFiles.GetFile(newName) != null)
							{
								newName = file.BaseFileName + " (" + (++counter).ToString() + ")" + file.Extension;
							}

							newFilePath = newDirPath + newName;
						}
						else
						{
							// Don't overwrite ourselves.
							if (string.Compare(newFilePath, file.FullPath, true) == 0)
							{
								continue;
							}
						}                            
                    }

                    using (var inputStream = file.OpenStream(false))
                    {
                        using (var outputStream = Program.ScratchFiles.OpenStream(newFilePath, true))
                        {
                            inputStream.CopyTo(outputStream);
                        }
                    }

										
					if (deleteSource)
					{
						// Reroute any cut/copy operation.
						if ((_cutCopyObject != null) && (_cutCopyObject.ToString() == file.FullPath))
						{
							_cutCopyObject = newFilePath;
						}

						// If we have this file open, and we're moving the file, then relink it.
						if (file == currentFile)
						{
							var newFile = Program.ScratchFiles.GetFile(newFilePath);
							Program.CurrentContent.File = newFile;
						}
					}
                }

                // We cancelled our copy, so leave.
                if (result == ConfirmationResult.Cancel)
                {
                    break;
                }
            }


			// Reroute cut/copy operation.
			if ((deleteSource) && (_cutCopyObject != null) && (_cutCopyObject.ToString() == sourceDirectory.Name))
			{
				_cutCopyObject = name;
			}

            destDirectory.Collapse();
            if (destDirectory.Nodes.Count == 0)
            {
                destDirectory.Nodes.Add(new TreeNode("DUMMYNODE"));
            }

            // Wipe out the source.
            if (deleteSource)
            {
                DeleteDirectory(sourceDirectory);
            }

            // Copy is done, update our tree nodes.            
            destDirectory.Expand();

            sourceDirectory = (TreeNodeDirectory)destDirectory.Nodes[name];
            treeFiles.SelectedNode = sourceDirectory;

            if (wasExpanded)
            {
                // Get the node and expand it if it was previously open.
                destDirectory.Nodes[name].Expand();
            }

            Program.EditorFileChanged = true;
        }

		/// <summary>
		/// Function to retrieve files/sub directories from windows explorer.
		/// </summary>
		/// <param name="directoryInfo">Root directory information.</param>
		/// <param name="destPath">New destination path.</param>
		/// <returns>The list of enumerated files/folders.</returns>
		private List<Tuple<string, string>> GetExplorerFiles(DirectoryInfo directoryInfo, string destPath)
		{
			List<Tuple<string, string>> result = new List<Tuple<string, string>>();

			foreach (var directory in directoryInfo.EnumerateDirectories())
			{
				// Don't add hidden, encrypted or system paths.
				if (((directory.Attributes & FileAttributes.Encrypted) == FileAttributes.Encrypted)
					|| ((directory.Attributes & FileAttributes.Hidden) == FileAttributes.Hidden)
					|| ((directory.Attributes & FileAttributes.System) == FileAttributes.System))
				{
					continue;
				}

				result.AddRange(GetExplorerFiles(directory, (destPath + directory.Name).FormatDirectory('/')));

				result.Add(new Tuple<string, string>(directory.FullName.FormatDirectory(Path.DirectorySeparatorChar), destPath + directory.Name.FormatDirectory('/')));
			}

			foreach (var file in directoryInfo.EnumerateFiles())
			{
				// Don't add hidden, encrypted or system paths.
				if (((file.Attributes & FileAttributes.Encrypted) == FileAttributes.Encrypted)
					|| ((file.Attributes & FileAttributes.Hidden) == FileAttributes.Hidden)
					|| ((file.Attributes & FileAttributes.System) == FileAttributes.System))
				{
					continue;
				}
				
				result.Add(new Tuple<string, string>(file.FullName, destPath + file.Name));
			}

			return result;
		}

		/// <summary>
		/// Function to asynchronously gather the files and directories for import from explorer.
		/// </summary>		
		/// <param name="destDir">Destination directory.</param>
		/// <param name="files">List of files to import.</param>
		/// <param name="sourceDirectories">Directories to copy.</param>
		/// <param name="sourceFiles">Files to copy.</param>
		/// <param name="ct">Cancellation token.</param>
		/// <returns>The list of directories and files.</returns>		
		private void GetFilesAndDirectories(GorgonFileSystemDirectory destDir, List<string> files, List<Tuple<string, string>> sourceDirectories, List<Tuple<string, string>> sourceFiles, CancellationToken ct)
		{
			foreach (string filePath in files)
			{
				DirectoryInfo pathInfo = new DirectoryInfo(filePath);
				string newPath = (destDir.FullPath + pathInfo.Name).FormatDirectory('/');

				if (ct.IsCancellationRequested)
				{
					sourceDirectories.Clear();
					sourceFiles.Clear();
					return;
				}

				// Don't add hidden, encrypted or system paths.
				if (((pathInfo.Attributes & FileAttributes.Encrypted) == FileAttributes.Encrypted)
					|| ((pathInfo.Attributes & FileAttributes.Hidden) == FileAttributes.Hidden)
					|| ((pathInfo.Attributes & FileAttributes.System) == FileAttributes.System))
				{
					continue;
				}

				// Do not copy files under our write folder.
				if (filePath.StartsWith(Program.ScratchFiles.WriteLocation, StringComparison.CurrentCultureIgnoreCase))
				{
					continue;
				}

				// If this is a sub directory, then create it.
				if ((pathInfo.Attributes & FileAttributes.Directory) == FileAttributes.Directory)
				{

					var paths = GetExplorerFiles(pathInfo, newPath);
					var directories = (from path in paths
									  select new Tuple<string, string>(Path.GetDirectoryName(path.Item1).FormatDirectory(Path.DirectorySeparatorChar), 
										  Path.GetDirectoryName(path.Item2).FormatDirectory('/'))).Distinct();
					var filePaths = from path in paths
									where !string.IsNullOrWhiteSpace(Path.GetFileName(path.Item2))
										&& (!path.Item1.StartsWith(Program.ScratchFiles.WriteLocation))
									select path;

					if (directories.Count() > 0)
					{
						sourceDirectories.AddRange(directories);
					}
					if (filePaths.Count() > 0)
					{
						sourceFiles.AddRange(filePaths);
					}
				}
				else
				{					
					// Don't add hidden, encrypted or system paths.
					if (((pathInfo.Attributes & FileAttributes.Encrypted) == FileAttributes.Encrypted)
						|| ((pathInfo.Attributes & FileAttributes.Hidden) == FileAttributes.Hidden)
						|| ((pathInfo.Attributes & FileAttributes.System) == FileAttributes.System))
					{
						continue;
					}

					// Add this directory to the list.
					sourceFiles.Add(new Tuple<string, string>(pathInfo.FullName, destDir.FullPath + pathInfo.Name));
				}

				if (ct.IsCancellationRequested)
				{
					sourceDirectories.Clear();
					sourceFiles.Clear();
					return;
				}
			}
		}

		/// <summary>
		/// Function to copy files from windows explorer into our file system.
		/// </summary>
		/// <param name="destDir">Destination directory node.</param>
		/// <param name="files">Paths to the files/directories to copy.</param>
		private void AddFilesFromExplorer(TreeNodeDirectory destDir, List<string> files)
		{			
			List<Tuple<string, string>> sourceDirectories = new List<Tuple<string, string>>();
			List<Tuple<string, string>> sourceFiles = new List<Tuple<string, string>>();
			
			using (var progForm = new formProcess())
			{
				using (var tokenSource = new CancellationTokenSource(Int32.MaxValue))
				{
					CancellationToken token = tokenSource.Token;

					progForm.labelStatus.Text = "Getting file info...";
					progForm.progressMeter.Style = ProgressBarStyle.Marquee;
					progForm.progressMeter.Value = 0;
					progForm.Text = "Scanning files";
					progForm.Task = Task.Factory.StartNew(() =>
						{
							GetFilesAndDirectories(destDir.Directory, files, sourceDirectories, sourceFiles, token);
						}, token);

					if (progForm.ShowDialog(this) == System.Windows.Forms.DialogResult.Cancel)
					{
						tokenSource.Cancel();
						return;
					}
				}
			}

			// If we have nothing to import, then we should leave.
			if ((sourceFiles.Count == 0) && (sourceDirectories.Count == 0))
			{
				GorgonDialogs.InfoBox(this, "There were no files/directories found that could be imported.");
				return;
			}
			
			// Don't just assume we want this.
			if (GorgonDialogs.ConfirmBox(this, sourceFiles.Count.ToString() + " files and " + sourceDirectories.Count.ToString() + " directories will be imported.  Are you sure you wish to do this?") == ConfirmationResult.No)
			{
				return;
			}

			using (var progForm = new formProcess())
			{
				using (var tokenSource = new CancellationTokenSource(Int32.MaxValue))
				{
					int counter = 0;
					CancellationToken token = tokenSource.Token;

					progForm.progressMeter.Style = ProgressBarStyle.Continuous;
					progForm.progressMeter.Value = 0;
					progForm.progressMeter.Maximum = 100;
					progForm.progressMeter.Minimum = 0;
					progForm.labelStatus.Text = string.Empty;
					progForm.Text = "Importing files";

					progForm.Task = Task.Factory.StartNew(() =>
						{
							// Begin copy procedure.
							ConfirmationResult result = ConfirmationResult.None;
							decimal max = sourceFiles.Count + sourceDirectories.Count;

							for (int i = 0; i < sourceDirectories.Count; i++)
							{
								if (token.IsCancellationRequested)
								{
									return;
								}

								var directory = sourceDirectories[i];

								progForm.UpdateStatusText("Creating '" + directory.Item2.Ellipses(45, true) + "'");
								Program.ScratchFiles.CreateDirectory(directory.Item2);
								progForm.SetProgress((int)(((decimal)i / max) * 100M));

								if (token.IsCancellationRequested)
								{
									return;
								}
							}

							for (int i = 0; i < sourceFiles.Count; i++)
							{
								if (token.IsCancellationRequested)
								{
									return;
								}
								var sourceFile = sourceFiles[i];

								progForm.UpdateStatusText("Copying '" + sourceFile.Item2.Ellipses(45, true) + "'");
								progForm.SetProgress((int)(((decimal)(i + sourceDirectories.Count) / max) * 100M));

								// Find out if this file already exists.
								var fileEntry = Program.ScratchFiles.GetFile(sourceFile.Item2);

								if (fileEntry != null)
								{
									if ((result & ConfirmationResult.ToAll) != ConfirmationResult.ToAll)
									{
										if (progForm.InvokeRequired)
										{
											progForm.Invoke(new MethodInvoker(() =>
												{
													result = GorgonDialogs.ConfirmBox(progForm, "The file '" + sourceFile.Item2 + "' already exists.  Would you like to overwrite it?", true, true);
												}));
										}
										else
										{
											result = GorgonDialogs.ConfirmBox(progForm, "The file '" + sourceFile.Item2 + "' already exists.  Would you like to overwrite it?", true, true);
										}
									}

									if (token.IsCancellationRequested)
									{
										return;
									}

									// Stop copying.
									if (result == ConfirmationResult.Cancel)
									{
										return;
									}

									// Skip this file.
									if ((result & ConfirmationResult.No) == ConfirmationResult.No)
									{
										continue;
									}

									// If we have the file opened, then we have a problem.
									if ((Program.CurrentContent != null)
										&& (Program.CurrentContent.File == fileEntry))
									{
										if (progForm.InvokeRequired)
										{
											progForm.Invoke(new MethodInvoker(() =>
												{
													GorgonDialogs.ErrorBox(progForm, "The file '" + fileEntry.FullPath + "' is opened for editing.  You must close this file and reimport it if you wish to overwrite it.");
												}));
										}
										else
										{
											GorgonDialogs.ErrorBox(progForm, "The file '" + fileEntry.FullPath + "' is opened for editing.  You must close this file and reimport it if you wish to over write it.");
										}
										continue;
									}
								}

								if (token.IsCancellationRequested)
								{
									return;
								}

								try
								{
									using (var inputStream = File.Open(sourceFile.Item1, FileMode.Open, FileAccess.Read, FileShare.Read))
									{
										using (var outputStream = Program.ScratchFiles.OpenStream(sourceFile.Item2, true))
										{
											inputStream.CopyTo(outputStream);
										}
									}
								}
								catch(Exception ex)
								{
									// Record the error and move on.
									if (progForm.InvokeRequired)
									{
										progForm.Invoke(new MethodInvoker(() =>
											{
												GorgonDialogs.ErrorBox(progForm, ex);
											}));
									}
									else
									{
										GorgonDialogs.ErrorBox(progForm, ex);
									}
								}

								// Increment the file counter.
								counter++;

								if (token.IsCancellationRequested)
								{
									return;
								}
							}
						}, token);

					if (progForm.ShowDialog(this) == System.Windows.Forms.DialogResult.Cancel)
					{
						tokenSource.Cancel();
					}

					if (destDir.IsExpanded)
					{
						destDir.Collapse();
					}

					if (destDir.Nodes.Count == 0)
					{
						destDir.Nodes.Add(new TreeNode("DUMMYNODE"));
					}

					destDir.Expand();
					treeFiles.SelectedNode = destDir;

					Program.EditorFileChanged = true;

					GorgonDialogs.InfoBox(this, counter.ToString() + " files successfully imported.\n" + (sourceFiles.Count - counter).ToString() + " files were skipped.");
				}
			}			
		}

		/// <summary>
		/// Handles the DragDrop event of the treeFiles control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="DragEventArgs"/> instance containing the event data.</param>
		private void treeFiles_DragDrop(object sender, DragEventArgs e)
		{
			try
			{
				if (e.Effect == DragDropEffects.None)
				{
					return;
				}

				Cursor.Current = Cursors.WaitCursor;

				EditorTreeNode overNode = (EditorTreeNode)treeFiles.GetNodeAt(treeFiles.PointToClient(new Point(e.X, e.Y)));
				TreeNodeDirectory destDir = overNode as TreeNodeDirectory;
				TreeNodeFile destFile = overNode as TreeNodeFile;

				// Handle explorer files.
				if (e.Data.GetDataPresent(DataFormats.FileDrop, false))
				{
					List<string> files = new List<string>((IEnumerable<string>)e.Data.GetData(DataFormats.FileDrop));
					var excludedFiles = files.Where(item => item.StartsWith(Program.ScratchFiles.WriteLocation, StringComparison.CurrentCultureIgnoreCase));

					// Don't allow files in our write path to be imported.
					if (excludedFiles.Count() > 0)
					{
						foreach(var filePath in excludedFiles)
						{
							files.Remove(filePath);
						}
					}

					if (files.Count > 0)
					{
						AddFilesFromExplorer(destDir, files);						
					}					
					return;
				}

				// If we're moving one of our directories or files, then process those items.
				if (e.Data.GetDataPresent(typeof(Tuple<EditorTreeNode, MouseButtons>)))
				{
					var data = (Tuple<EditorTreeNode, MouseButtons>)e.Data.GetData(typeof(Tuple<EditorTreeNode, MouseButtons>));

					// Perform a move.
					TreeNodeDirectory directory = data.Item1 as TreeNodeDirectory;

					// Our source data is a directory, so move it.
					if ((directory != null) && (destDir != null))
					{
                        CopyDirectoryNode(directory, destDir, directory.Directory.Name, (data.Item2 == MouseButtons.Left));
						return;
					}

					// We didn't have a directory, so move the file.
					TreeNodeFile file = data.Item1 as TreeNodeFile;

					if ((destDir != null) && (file != null))
					{
                        if (data.Item2 == System.Windows.Forms.MouseButtons.Left)
                        {
                            CopyFileNode(file, destDir, file.Name, true);
                        }
                        else
                        {
                            CopyFileNode(file, destDir, file.Name, false);
                        }

						return;
					}
				}
			}
			catch (Exception ex)
			{
				GorgonDialogs.ErrorBox(this, ex);

				if (Program.CurrentContent == null)
				{
					LoadContentPane<DefaultContent>();
				}
			}
			finally
			{
				ValidateControls();
				Cursor.Current = Cursors.Default;
			}
		}

		/// <summary>
		/// Handles the DragEnter event of the treeFiles control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="DragEventArgs"/> instance containing the event data.</param>
		private void treeFiles_DragEnter(object sender, DragEventArgs e)
		{
			try
			{
				e.Effect = DragDropEffects.None;

				if (e.Data.GetDataPresent(DataFormats.FileDrop, false)) 
				{
					e.Effect = DragDropEffects.Copy;
				}
			}
			catch (Exception ex)
			{
				GorgonDialogs.ErrorBox(this, ex);
			}
		}

		/// <summary>
		/// Handles the DragOver event of the treeFiles control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="DragEventArgs"/> instance containing the event data.</param>
		private void treeFiles_DragOver(object sender, DragEventArgs e)
		{
			try
			{
				EditorTreeNode overNode = treeFiles.GetNodeAt(treeFiles.PointToClient(new Point(e.X, e.Y))) as EditorTreeNode;
				TreeNodeDirectory destDirectory = overNode as TreeNodeDirectory;
				TreeNodeFile destFile = overNode as TreeNodeFile;

				e.Effect = DragDropEffects.None;

				// If we're over nothing, then assume we're over the root node.
				if (overNode == null)
				{
					overNode = _rootNode;
				}

				// Handle files from explorer.
				if (e.Data.GetDataPresent(DataFormats.FileDrop, false))
				{
					if (destDirectory == null)
					{
						return;
					}

					e.Effect = DragDropEffects.Copy;

					return;
				}

				if (e.Data.GetDataPresent(typeof(Tuple<EditorTreeNode, MouseButtons>)))
				{
					// Get our source data.
					Tuple<EditorTreeNode, MouseButtons> dragData = (Tuple<EditorTreeNode, MouseButtons>)e.Data.GetData(typeof(Tuple<EditorTreeNode, MouseButtons>));
					TreeNodeDirectory sourceDirectory = dragData.Item1 as TreeNodeDirectory;
					TreeNodeFile sourceFile = dragData.Item1 as TreeNodeFile;

					// Don't drag into ourselves, that's just dumb.
					// Likewise, if we're over our current parent, do nothing.
					if ((sourceDirectory == overNode) || (overNode == dragData.Item1.Parent))
					{
						return;
					}

					// If we drag a directory over a file, then we can't do use that.
					if (destFile != null)
					{
						if (sourceDirectory != null)
						{
							return;
						}

						// In the future, we may allow file linking, but we'll need to test for it.
						// Until that time, just disable the drag/drop.
						if (sourceFile != null)
						{
							return;
						}
					}

					treeFiles.SelectedNode = overNode;
					if (dragData.Item2 == System.Windows.Forms.MouseButtons.Left)
					{
						e.Effect = DragDropEffects.Move;
					}
					else
					{
						e.Effect = DragDropEffects.Copy;
					}
				}
			}
			catch (Exception ex)
			{
				GorgonDialogs.ErrorBox(this, ex);
			}
		}

        /// <summary>
        /// Handles the Click event of the itemOpen control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void itemOpen_Click(object sender, EventArgs e)
        {
            StringBuilder extensions = new StringBuilder(512);

            try
            {
				// Save the file if it's changed.
				if (ConfirmSave())
				{
					return;
				}

                if (!string.IsNullOrWhiteSpace(Program.Settings.LastEditorFile))
                {
                    dialogOpenFile.InitialDirectory = Path.GetDirectoryName(Program.Settings.LastEditorFile);
                }

				var distinctExtensions = (from provider in Program.ScratchFiles.Providers
								  from extension in provider.PreferredExtensions
								  select extension).Distinct();

                // Add extensions from file system providers.				
                foreach (var extension in distinctExtensions)
                {
                    if (extensions.Length > 0)
                    {
                        extensions.Append("|");
                    }
                    extensions.Append(extension);
                }
                                
                dialogOpenFile.Filter = extensions.ToString();

				if (dialogOpenFile.ShowDialog(this) == System.Windows.Forms.DialogResult.OK)
				{
					Cursor.Current = Cursors.WaitCursor;

					// If this file is already opened, then do nothing.
					if (string.Compare(Program.EditorFilePath, dialogOpenFile.FileName, true) == 0)
					{
						return;
					}

					// Close the current content.
					LoadContentPane<DefaultContent>();

					// Open the file.
					Program.OpenEditorFile(dialogOpenFile.FileName);

					// Update the tree.
					InitializeTree();
				}
            }
            catch (Exception ex)
            {
                if (!(Program.CurrentContent is DefaultContent))
                {
                    LoadContentPane<DefaultContent>();
                }
                GorgonDialogs.ErrorBox(this, ex);
            }
            finally
            {
                Cursor.Current = Cursors.Default;
                ValidateControls();
            }
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the tabDocumentManager control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void tabDocumentManager_SelectedIndexChanged(object sender, EventArgs e)
        {
            ValidateControls();
        }
        
        /// <summary>
		/// Function to initialize the global interface commands for each content plug-in.
		/// </summary>
		private void InitializeInterface()
		{
			foreach (var plugIn in Program.ContentPlugIns)
			{
				// Get the menu item.
				var createItem = plugIn.Value.GetCreateMenuItem();

				if (createItem != null)
				{
					// Add to the 3 "Add" loctaions.
					popupAddContentMenu.Items.Add(createItem);

					// Click event.
					createItem.Click += AddContent;
				}
			}

			// Enable the add items if we have anything new.
			popupItemAdd.Enabled = dropNewContent.Enabled = itemAdd.Enabled = itemAdd.DropDownItems.Count > 0;
		}

		/// <summary>
		/// Raises the <see cref="E:System.Windows.Forms.Form.Load"/> event.
		/// </summary>
		/// <param name="e">An <see cref="T:System.EventArgs"/> that contains the event data.</param>
		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			try
			{
				ToolStripManager.Renderer = new DarkFormsRenderer();

				this.Location = Program.Settings.WindowDimensions.Location;
				this.Size = Program.Settings.WindowDimensions.Size;

				// If this window can't be placed on a monitor, then shift it to the primary.
				if (!Screen.AllScreens.Any(item => item.Bounds.Contains(this.Location)))
				{
					this.Location = Screen.PrimaryScreen.Bounds.Location;
				}

				this.WindowState = Program.Settings.FormState;

				InitializeInterface();

				InitializeTree();
			}
			catch (Exception ex)
			{
				GorgonDialogs.ErrorBox(this, ex);
			}
			finally
			{
				ValidateControls();
			}
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="formMain"/> class.
		/// </summary>
		public formMain()
		{
			InitializeComponent();
		}
		#endregion
	}
}