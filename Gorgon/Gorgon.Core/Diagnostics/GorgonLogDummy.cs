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
// Created: Friday, May 23, 2015 3:09:45 PM
// 
#endregion

using System;
using System.Threading;

namespace Gorgon.Diagnostics
{
	/// <summary>
	/// An implementation of the <see cref="IGorgonThreadedLog"/> type that does nothing.
	/// </summary>
	sealed public class GorgonLogDummy
		: IGorgonThreadedLog
	{
		#region Constructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonLogDummy"/> class.
		/// </summary>
		public GorgonLogDummy()
		{
			ThreadID = Thread.CurrentThread.ManagedThreadId;
		}
		#endregion

		#region IGorgonThreadedLog Members
		/// <summary>
		/// Property to return the ID of the thread that created the log object.
		/// </summary>
		public int ThreadID
		{
			get;
		}
		#endregion

		#region IGorgonLog Members
		#region Properties.
		/// <summary>
		/// Property to set or return the filtering level of this log.
		/// </summary>
		public LoggingLevel LogFilterLevel
		{
			get;
			set;
		}

		/// <summary>
		/// Property to return the name of the application that is being logged.
		/// </summary>
		public string LogApplication => string.Empty;

		/// <summary>
		/// Property to return whether or not the log is in a closed state.
		/// </summary>
		public bool IsClosed => true;

		#endregion

		#region Methods.
		/// <summary>
		/// Function to send an exception to the log file.
		/// </summary>
		/// <param name="ex">The exception to log.</param>
		/// <remarks>
		/// <para>
		/// This method will send a line of formatted text to the log. If the <see cref="Open" /> method has not been called, then this method will do nothing.
		/// </para>
		/// <para>
		/// If the <see cref="GorgonLogFile.LogFilterLevel" /> is set to <c>LoggingLevel.NoLogging</c>, then the exception will not be logged. If the filter is set to any other setting, it will be logged
		/// regardless of filter level.
		/// </para>
		/// </remarks>
		public void LogException(Exception ex)
		{
			// Intentionally left blank.
		}

		/// <summary>
		/// Function to print a line to the log.
		/// </summary>
		/// <param name="formatSpecifier">Format specifier for the line.</param>
		/// <param name="level">Level that this message falls under.</param>
		/// <param name="arguments">List of optional arguments.</param>
		/// <remarks>
		/// This method will send a line of formatted text to the log. If the <see cref="Open" /> method has not been called, then this method will do nothing.
		/// </remarks>
		public void Print(string formatSpecifier, LoggingLevel level, params object[] arguments)
		{
			// Intentionally left blank.
		}

		/// <summary>
		/// Function to close the log.
		/// </summary>
		/// <remarks>
		/// Applications must call this method, otherwise a resource leak may occur.
		/// </remarks>
		public void Close()
		{
			// Intentionally left blank.
		}

		/// <summary>
		/// Function to open the log.
		/// </summary>
		/// <remarks>
		/// This method should be called before calling the <see cref="Print" /> or <see cref="LogException" /> methods. Otherwise, no data
		/// will be sent to the log.
		/// </remarks>
		public void Open()
		{
			// Intentionally left blank.
		}
		#endregion
		#endregion
	}
}
