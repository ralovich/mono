//
// Expression.cs: Stores references to items or properties.
//
// Author:
//   Marek Sieradzki (marek.sieradzki@gmail.com)
// 
// (C) 2005 Marek Sieradzki
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

#if NET_2_0

using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Microsoft.Build.BuildEngine {
	internal class Expression {
	
		ExpressionCollection expressionCollection;
	
		public Expression ()
		{
			this.expressionCollection = new ExpressionCollection ();
		}

		public void Parse (string expression)
		{
			expression = expression.Replace ('/', Path.DirectorySeparatorChar);
			expression = expression.Replace ('\\', Path.DirectorySeparatorChar);
		
			string [] parts = expression.Split (';');

			List <ArrayList> p1 = new List <ArrayList> (parts.Length);
			List <ArrayList> p2 = new List <ArrayList> (parts.Length);
			List <ArrayList> p3 = new List <ArrayList> (parts.Length);

			Prepare (p1, parts.Length);
			Prepare (p2, parts.Length);
			Prepare (p3, parts.Length);

			for (int i = 0; i < parts.Length; i++) {
				p1 [i] = SplitItems (parts [i]);
			}

			for (int i = 0; i < parts.Length; i++) {
				p2 [i] = new ArrayList ();
				foreach (object o in p1 [i]) {
					if (o is string)
						p2 [i].AddRange (SplitProperties ((string) o));
					else
						p2 [i].Add (o);
				}
			}

			for (int i = 0; i < parts.Length; i++) {
				p3 [i] = new ArrayList ();
				foreach (object o in p2 [i]) {
					if (o is string)
						p3 [i].AddRange (SplitMetadata ((string) o));
					else
						p3 [i].Add (o);
				}
			}

			CopyToExpressionCollection (p3);
		}

		void Prepare (List <ArrayList> l, int length)
		{
			for (int i = 0; i < length; i++)
				l.Add (null);
		}
		
		void CopyToExpressionCollection (List <ArrayList> lists)
		{
			for (int i = 0; i < lists.Count; i++) {
				foreach (object o in lists [i]) {
					if (o is string)
						expressionCollection.Add ((string) o);
					else if (o is ItemReference)
						expressionCollection.Add ((ItemReference) o);
					else if (o is PropertyReference)
						expressionCollection.Add ((PropertyReference) o);
					else if (o is MetadataReference)
						expressionCollection.Add ((MetadataReference) o);
				}
				if (i < lists.Count - 1)
					expressionCollection.Add (";");
			}
		}

		ArrayList SplitItems (string text)
		{
			ArrayList phase1 = new ArrayList ();
			Match m;
			Regex item = new Regex (
				@"@\(\s*"
				+ @"(?<itemname>[_A-Za-z][_0-9a-zA-Z]*)"
				+ @"(?<has_transform>\s*->\s*'(?<transform>[^']*)')?"
				+ @"(?<has_separator>\s*,\s*'(?<separator>[^']*)')?"
				+ @"\s*\)");
			m = item.Match (text);

			while (m.Success) {
				string name = null, transform = null, separator = null;
				ItemReference ir;
				
				name = m.Groups [item.GroupNumberFromName ("itemname")].Value;
				
				if (m.Groups [item.GroupNumberFromName ("has_transform")].Success)
					transform = m.Groups [item.GroupNumberFromName ("transform")].Value;
				
				if (m.Groups [item.GroupNumberFromName ("has_separator")].Success)
					separator = m.Groups [item.GroupNumberFromName ("separator")].Value;

				ir = new ItemReference (name, transform, separator, m.Groups [0].Index, m.Groups [0].Length);
				phase1.Add (ir);
				m = m.NextMatch ();
			}

			ArrayList phase2 = new ArrayList ();
			int last_end = -1;
			int end = text.Length - 1;

			foreach (ItemReference ir in phase1) {
				int a,b;

				a = last_end;
				b = ir.Start;

				if (b - a - 1 > 0) {
					phase2.Add (text.Substring (a + 1, b - a - 1));
				}

				last_end = ir.End;
				phase2.Add (ir);
			}

			if (last_end < end)
				phase2.Add (text.Substring (last_end + 1, end - last_end));

			return phase2;
		}

		ArrayList SplitProperties (string text)
		{
			ArrayList phase1 = new ArrayList ();
			Match m;
			Regex property = new Regex (
				@"\$\(\s*"
				+ @"(?<name>[_a-zA-Z][_0-9a-zA-Z]*)"
				+ @"\s*\)");
			m = property.Match (text);

			while (m.Success) {
				string name = null;
				PropertyReference pr;
				
				name = m.Groups [property.GroupNumberFromName ("name")].Value;
				
				pr = new PropertyReference (name, m.Groups [0].Index, m.Groups [0].Length);
				phase1.Add (pr);
				m = m.NextMatch ();
			}

			ArrayList phase2 = new ArrayList ();
			int last_end = -1;
			int end = text.Length - 1;

			foreach (PropertyReference pr in phase1) {
				int a,b;

				a = last_end;
				b = pr.Start;

				if (b - a - 1 > 0) {
					phase2.Add (text.Substring (a + 1, b - a - 1));
				}

				last_end = pr.End;
				phase2.Add (pr);
			}

			if (last_end < end)
				phase2.Add (text.Substring (last_end + 1, end - last_end));

			return phase2;
		}

		ArrayList SplitMetadata (string text)
		{
			ArrayList phase1 = new ArrayList ();
			Match m;
			Regex metadata = new Regex (
				@"%\(\s*"
				+ @"((?<name>[_a-zA-Z][_0-9a-zA-Z]*)\.)?"
				+ @"(?<meta>[_a-zA-Z][_0-9a-zA-Z]*)"
				+ @"\s*\)");
			m = metadata.Match (text);

			while (m.Success) {
				string name = null, meta = null;
				MetadataReference mr;
				
				if (m.Groups [metadata.GroupNumberFromName ("name")].Success)
					name = m.Groups [metadata.GroupNumberFromName ("name")].Value;
				
				meta = m.Groups [metadata.GroupNumberFromName ("meta")].Value;
				
				mr = new MetadataReference (name, meta, m.Groups [0].Index, m.Groups [0].Length);
				phase1.Add (mr);
				m = m.NextMatch ();
			}

			ArrayList phase2 = new ArrayList ();
			int last_end = -1;
			int end = text.Length - 1;

			foreach (MetadataReference mr in phase1) {
				int a,b;

				a = last_end;
				b = mr.Start;

				if (b - a - 1> 0) {
					phase2.Add (text.Substring (a + 1, b - a - 1));
				}

				last_end = mr.End;
				phase2.Add (mr);
			}

			if (last_end < end)
				phase2.Add (text.Substring (last_end + 1, end - last_end));

			return phase2;
		}
		
		public object ConvertTo (Project project, Type type)
		{
			return expressionCollection.ConvertTo (project, type);
		}

		public ExpressionCollection Collection {
			get { return expressionCollection; }
		}
	}
}

#endif
