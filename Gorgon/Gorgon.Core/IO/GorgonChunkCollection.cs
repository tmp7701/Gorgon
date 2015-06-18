﻿#region MIT
// 
// Gorgon.
// Copyright (C) 2015 Michael Winsor
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
// Created: Sunday, June 14, 2015 9:42:32 PM
// 
#endregion

using System;
using System.Collections;
using System.Collections.Generic;

namespace Gorgon.IO
{
	/// <summary>
	/// A collection of chunks within a chunked file.
	/// </summary>
	class GorgonChunkCollection
		: IList<IGorgonChunk>, IGorgonReadOnlyChunkCollection
	{
		#region Variables.
		// The backing store for the chunks.
		private readonly List<IGorgonChunk> _list = new List<IGorgonChunk>();
		#endregion

		#region IList<IGorgonChunk> Members
		#region Properties.
		/// <summary>
		/// Property to set or return a chunk at the specified index.
		/// </summary>
		public IGorgonChunk this[int index]
		{
			get
			{
				return _list[index];
			}
			set
			{
				_list[index] = value;
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Determines the index of a specific item in the <see cref="T:System.Collections.Generic.IList`1" />.
		/// </summary>
		/// <param name="item">The object to locate in the <see cref="T:System.Collections.Generic.IList`1" />.</param>
		/// <returns>The index of <paramref name="item" /> if found in the list; otherwise, -1.</returns>
		public int IndexOf(IGorgonChunk item)
		{
			return _list.IndexOf(item);
		}

		/// <summary>
		/// Inserts an item to the <see cref="T:System.Collections.Generic.IList`1" /> at the specified index.
		/// </summary>
		/// <param name="index">The zero-based index at which <paramref name="item" /> should be inserted.</param>
		/// <param name="item">The object to insert into the <see cref="T:System.Collections.Generic.IList`1" />.</param>
		public void Insert(int index, IGorgonChunk item)
		{
			_list.Insert(index, item);
		}

		/// <summary>
		/// Removes the <see cref="T:System.Collections.Generic.IList`1" /> item at the specified index.
		/// </summary>
		/// <param name="index">The zero-based index of the item to remove.</param>
		public void RemoveAt(int index)
		{
			_list.RemoveAt(index);
		}
		#endregion
		#endregion

		#region ICollection<IGorgonChunk> Members
		#region Properties.
		/// <summary>
		/// Gets the number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1" />.
		/// </summary>
		/// <value>The count.</value>
		public int Count
		{
			get
			{
				return _list.Count;
			}
		}

		/// <summary>
		/// Gets a value indicating whether the <see cref="T:System.Collections.Generic.ICollection`1" /> is read-only.
		/// </summary>
		/// <value><b>true</b> if this instance is read only; otherwise, <b>false</b>.</value>
		bool ICollection<IGorgonChunk>.IsReadOnly
		{
			get
			{
				return false;
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Adds an item to the <see cref="T:System.Collections.Generic.ICollection`1" />.
		/// </summary>
		/// <param name="item">The object to add to the <see cref="T:System.Collections.Generic.ICollection`1" />.</param>
		public void Add(IGorgonChunk item)
		{
			_list.Add(item);
		}

		/// <summary>
		/// Removes all items from the <see cref="T:System.Collections.Generic.ICollection`1" />.
		/// </summary>
		public void Clear()
		{
			_list.Clear();
		}

		/// <summary>
		/// Determines whether the <see cref="T:System.Collections.Generic.ICollection`1" /> contains a specific value.
		/// </summary>
		/// <param name="item">The object to locate in the <see cref="T:System.Collections.Generic.ICollection`1" />.</param>
		/// <returns>true if <paramref name="item" /> is found in the <see cref="T:System.Collections.Generic.ICollection`1" />; otherwise, false.</returns>
		public bool Contains(IGorgonChunk item)
		{
			return IndexOf(item) != -1;
		}

		/// <summary>
		/// Copies to.
		/// </summary>
		/// <param name="array">The array.</param>
		/// <param name="arrayIndex">Index of the array.</param>
		public void CopyTo(IGorgonChunk[] array, int arrayIndex)
		{
			_list.CopyTo(array, arrayIndex);
		}

		/// <summary>
		/// Removes the first occurrence of a specific object from the <see cref="T:System.Collections.Generic.ICollection`1" />.
		/// </summary>
		/// <param name="item">The object to remove from the <see cref="T:System.Collections.Generic.ICollection`1" />.</param>
		/// <returns>true if <paramref name="item" /> was successfully removed from the <see cref="T:System.Collections.Generic.ICollection`1" />; otherwise, false. This method also returns false if <paramref name="item" /> is not found in the original <see cref="T:System.Collections.Generic.ICollection`1" />.</returns>
		public bool Remove(IGorgonChunk item)
		{
			return _list.Remove(item);
		}
		#endregion
		#endregion

		#region IEnumerable<IGorgonChunk> Members
		/// <summary>
		/// Returns an enumerator that iterates through the collection.
		/// </summary>
		/// <returns>A <see cref="T:System.Collections.Generic.IEnumerator`1" /> that can be used to iterate through the collection.</returns>
		public IEnumerator<IGorgonChunk> GetEnumerator()
		{
			return _list.GetEnumerator();
		}
		#endregion

		#region IEnumerable Members
		/// <summary>
		/// Returns an enumerator that iterates through a collection.
		/// </summary>
		/// <returns>An <see cref="T:System.Collections.IEnumerator" /> object that can be used to iterate through the collection.</returns>
		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable)_list).GetEnumerator();
		}
		#endregion

		#region IGorgonReadOnlyChunkCollection Members
		#region Properties.
		/// <summary>
		/// Property to return a chunk by a string identifier.
		/// </summary>
		/// <remarks>
		/// If the chunk is not found, then this property will return <b>null</b> (<i>Nothing</i> in VB.Net).
		/// </remarks>
		public IGorgonChunk this[string chunkName]
		{
			get
			{
				int index = IndexOf(chunkName);

				return index == -1 ? null : _list[index];
			}
		}

		/// <summary>
		/// Property to return a chunk by its <see cref="ulong"/> ID.
		/// </summary>
		/// <remarks>
		/// If the chunk is not found, then this property will return <b>null</b> (<i>Nothing</i> in VB.Net).
		/// </remarks>
		public IGorgonChunk this[ulong ID]
		{
			get
			{
				int index = IndexOf(ID);

				return index == -1 ? null : _list[index];
			}
		}
		#endregion

		#region Methods.
		/// <inheritdoc/>
		public int IndexOf(string chunkName)
		{
			if (string.IsNullOrEmpty(chunkName))
			{
				return -1;
			}

			UInt64 id = chunkName.ChunkID();

			return IndexOf(id);
		}

		/// <inheritdoc/>
		public bool Contains(string chunkName)
		{
			return IndexOf(chunkName) != -1;
		}

		/// <inheritdoc/>
		public int IndexOf(ulong chunkID)
		{
			for (int i = 0; i < _list.Count; ++i)
			{
				if (_list[i].ID == chunkID)
				{
					return i;
				}
			}

			return -1;
		}

		/// <inheritdoc/>
		public bool Contains(ulong chunkID)
		{
			return IndexOf(chunkID) != -1;
		}
		#endregion
		#endregion
	}
}
