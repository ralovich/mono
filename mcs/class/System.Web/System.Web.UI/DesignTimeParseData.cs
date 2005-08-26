//
// System.Web.HttpRequest.cs 
//
// 
// Author:
//	Miguel de Icaza (miguel@novell.com)
//

//
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.Web.UI;
using System.ComponentModel.Design;

namespace System.Web.UI {

	public sealed class DesignTimeParseData {
		EventHandler db_handler;
		string text;
	        IDesignerHost host;
		string durl;
		
		public DesignTimeParseData (IDesignerHost designerHost, string parseText)
		{
			host = designerHost;
			text = parseText;
		}

		public EventHandler DataBindingHandler {
			get {
				return db_handler;
			}

			set {
				db_handler = value;
			}
		}

		public IDesignerHost DesignerHost {
			get {
				return host;
			}
		}

		public string DocumentUrl {
			get {
				return durl;
			}

			set {
				durl = value;
			}
		}

		public string ParseText {
			get {
				return text;
			}
		}
		
	}
}

