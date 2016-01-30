using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

public class Pretty
{
	internal class Templates
	{
		public static XElement Leaf(dynamic model)
		{
			var tpl = "" +
					"<span class=\"leaf-container\">" +
						"<span class=\"<%=type%>\"> <%=data%></span><span><%=coma%></span>" +
					"</span>";

			tpl = tpl.Replace("<%=type%>", (string)model.type)
				.Replace("<%=data%>", model.data.Value.ToString())
				.Replace("<%=coma%>", (string)model.coma);

			return XElement.Parse(tpl);
		}

		public static XElement Node()
		{
			return XElement.Parse("" +
				   "<span class=\"node-container\">" +
					   //top.
					   "<span class=\"node-top node-bracket\" />" +
					   //content.
					   "<span class=\"node-content-wrapper\">" +
						   "<ul class=\"node-body\"></ul>" +
					   "</span>" +
					   //bottom.
					   "<span class=\"node-down node-bracket\" />" +
				   "</span>");
		}
	}

	public abstract class BaseNode
	{
		protected XElement el;
		protected dynamic els;
		protected Options options;
		internal Node parent;

		public BaseNode(Options opt)
		{
			options = opt;
		}

		protected XElement FindFirst(XElement e, string @class, string element)
		{
			return e.Descendants(element)
				.Where(s => ((string)s.Attribute("class")).Contains(@class))
				.FirstOrDefault();
		}

		public abstract XElement Render();

		public XElement Element { get { return el; } }
	}

	public class Node : BaseNode
	{
		internal List<BaseNode> childs = new List<BaseNode>();
		public Node(Options opt) : base(opt)
		{
			var m = GetMeta();
			options.type = m.type;
			options.size = m.size;
			options.isLast = options.data.Next == null;
			Render();
		}

		private dynamic GetMeta()
		{
			return new
			{
				size = options.data.Count(),
				type = (options.data is JArray) ? "array" : "object"
			};
		}

		private dynamic GetBrackets()
		{
			var suffix = (options.data.Next == null) ? "" : ",";
			dynamic v = new { top = "{", bottom = "}" + suffix };
			if (options.data is JArray)
				v = new { top = "[", bottom = "]" + suffix };

			return v;
		}

		private void Elements()
		{
			els = new
			{
				container = FindFirst(el, "node-container", "span"),
				contentWrapper = FindFirst(el, "node-content-wrapper", "span"),
				top = FindFirst(el, "node-top", "span"),
				down = FindFirst(el, "node-down", "span"),
				ul = FindFirst(el, "node-body", "ul")
			};
		}

		public override XElement Render()
		{
			el = Templates.Node();
			Elements();
			var b = GetBrackets();
			els.top.Add(b.top);
			els.down.Add(b.bottom);

			RenderChilds();
			return el;
		}

		private void RenderChilds()
		{
			int count = 1;
			int size = options.data.Count();
			foreach (JToken item in options.data)
			{
				var prop = item as JProperty;
				JToken value = item;
				string name = "";
				if (prop != null)
				{
					value = prop.Value;
					name = prop.Name;
				}

				var isLast = (count == size);
				count++;

				var opt = new Options()
				{
					key = name,
					data = value,
					parent = this,
					dateFormat = options.dateFormat,
					isLast = isLast
				};

				var child = ((value is JValue)) ? (BaseNode)new Leaf(opt) : new Node(opt);

				var li = new XElement("li");
				var column = " : ";
				var left = new XElement("span");
				var right = new XElement("span", child.Element);
				if (options.type == "array")
				{
					left.Add("");
				}
				else
				{
					left.Add(name + column);
				}

				left.Add(right);
				li.Add(left);
				els.ul.Add(li);

				child.parent = this;
				childs.Add(child);
			}
		}
	}

	public class Leaf : BaseNode
	{
		public Leaf(Options opt) : base(opt)
		{
			options.type = GetJsonType();
			Render();
		}

		public override XElement Render()
		{
			var state = GetState();

			if (state.type == "date" && options.dateFormat != null)
			{
				state.data = DateTime.Parse((string)state.data).ToString(options.dateFormat);
			}

			if (state.type == "null")
			{
				state.data = null;
			}

			el = Templates.Leaf(state);
			return el;
		}

		private dynamic GetState()
		{
			var coma = options.isLast ? "" : ",";
			return new
			{
				data = options.data,
				type = options.type,
				coma = coma
			};
		}

		private string GetJsonType()
		{
			var m = "string";
			var d = options.data as JValue;

			int n;
			bool b;
			DateTime dt;
			if (d == null) { m = null; }
			else if (int.TryParse((string)d, out n)) { m = "number"; }
			else if (bool.TryParse(d.ToString(), out b)) { m = "boolean"; }
			else if (DateTime.TryParse((string)d, out dt)) { m = "date"; }

			return m;
		}
	}

	public struct Options
	{
		public JToken data;
		public string dateFormat;
		internal bool isLast;
		internal string type;
		internal int size;
		internal string key;
		internal Node parent;
	}
}
