# prettysharp-json
A single file, C# serverside port of https://github.com/warfares/pretty-json

I have removed all javascript interactions and behaviours as I wanted just a pure standalone HTML output.

*Note:* This uses Newtonsoft.Json 7.0.1

See a sample output file here [sample](http://htmlpreview.github.com/?https://github.com/scullinan/prettysharp-json/blob/master/sample_output.html)

## Usage

```c#
var json = JObject.Parse("{ \"name\" : \"pretty\", \"wit\"  : \"sharp\"}");

var pretty = new Pretty.Node(new Pretty.Options { data = json });

var html = XElement.Parse(""+
	"<html>" +
		"<head>" +
			"<meta charset=\"utf-8\" />" +
			"<link rel=\"stylesheet\" type=\"text/css\" href=\"http://fonts.googleapis.com/css?family=Quicksand\" />" +
			"<link rel=\"stylesheet\" type=\"text/css\" href=\"http://warfares.github.io/pretty-json/css/pretty-json.css\" />" +
		"</head>" +
		"<body></body>"+
	"</html>");

var body = html.Descendants("body").First();

body.Add(pretty.Element);

File.WriteAllText("C:\\TEMP\\output.html", html.ToString());

```
