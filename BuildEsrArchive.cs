using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Xml;

// Converts the public ESR reference tables into the small, safe XML format consumed by Sanctuary Atlas.
class BuildEsrArchive {
 static readonly Regex Tables=new Regex(@"<table\b[^>]*>(?<body>.*?)</table>",RegexOptions.IgnoreCase|RegexOptions.Singleline|RegexOptions.Compiled);
 static readonly Regex Rows=new Regex(@"<tr\b[^>]*>(?<body>.*?)</tr>",RegexOptions.IgnoreCase|RegexOptions.Singleline|RegexOptions.Compiled);
 static readonly Regex Cells=new Regex(@"<t[dh]\b[^>]*>(?<body>.*?)</t[dh]>",RegexOptions.IgnoreCase|RegexOptions.Singleline|RegexOptions.Compiled);
 static readonly Regex Tags=new Regex(@"<[^>]+>",RegexOptions.Singleline|RegexOptions.Compiled);
 static string Clean(string html){
  string text=Regex.Replace(html??"",@"<(?:br|/p|/div|/li|/h[1-6])\b[^>]*>","\n",RegexOptions.IgnoreCase);text=Tags.Replace(text,"");text=WebUtility.HtmlDecode(text).Replace('\u00a0',' ').Replace("\r","");
  var lines=text.Split('\n').Select(x=>Regex.Replace(x,@"[ \t]+"," ").Trim()).Where(x=>x.Length>0);return String.Join("\n",lines.ToArray());
 }
 static List<List<string>> TableRows(string html){var rows=new List<List<string>>();foreach(Match row in Rows.Matches(html)){var cells=Cells.Matches(row.Groups["body"].Value).Cast<Match>().Select(x=>Clean(x.Groups["body"].Value)).ToList();if(cells.Count>0)rows.Add(cells);}return rows;}
 static bool Header(List<string> row,string a,string b){return row.Count>=2&&row[0].IndexOf(a,StringComparison.OrdinalIgnoreCase)>=0&&row[1].IndexOf(b,StringComparison.OrdinalIgnoreCase)>=0;}
 static string SocketCount(string name){var match=Regex.Match(name??"",@"\((?<count>\d+)\s*Socket",RegexOptions.IgnoreCase);return match.Success?match.Groups["count"].Value:"";}
 static string Name(string name){return Regex.Replace(name??"",@"\s*\(\d+\s*Socket(?:s)?\)\s*"," ",RegexOptions.IgnoreCase).Trim();}
 static void WriteText(XmlWriter writer,string name,string value){writer.WriteStartElement(name);writer.WriteString(value??"");writer.WriteEndElement();}
 static void WriteArchive(string runewordHtml,string recipeHtml,string output){
  var words=new List<List<string>>();foreach(Match table in Tables.Matches(runewordHtml)){var rows=TableRows(table.Groups["body"].Value);if(rows.Any(r=>r.Count>=3&&r[0].IndexOf("Rune Word",StringComparison.OrdinalIgnoreCase)>=0&&r[1].IndexOf("Runes",StringComparison.OrdinalIgnoreCase)>=0)){words.AddRange(rows.SkipWhile(r=>r.Count<3||r[0].IndexOf("Rune Word",StringComparison.OrdinalIgnoreCase)<0).Skip(1).Where(r=>r.Count>=3&&!String.IsNullOrWhiteSpace(r[0])));}}
  var recipes=new List<Tuple<string,string,string>>();foreach(Match table in Tables.Matches(recipeHtml)){var rows=TableRows(table.Groups["body"].Value);if(rows.Count<2)continue;int header=rows.FindIndex(r=>Header(r,"Input","Output"));if(header<0)continue;string category=rows.Take(header).SelectMany(r=>r).FirstOrDefault(x=>!String.IsNullOrWhiteSpace(x))??"Other";foreach(var row in rows.Skip(header+1)){if(row.Count<2)continue;string input=row[0],outputValue=row[1];if(String.IsNullOrWhiteSpace(input)||String.IsNullOrWhiteSpace(outputValue)||Header(row,"Input","Output"))continue;recipes.Add(Tuple.Create(category,input,outputValue));}}
  if(words.Count<100||recipes.Count<100)throw new InvalidOperationException("The ESR reference pages did not yield the expected catalog entries. Runewords="+words.Count+", recipes="+recipes.Count);
  var settings=new XmlWriterSettings{Indent=true,Encoding=new System.Text.UTF8Encoding(false)};using(var writer=XmlWriter.Create(output,settings)){
   writer.WriteStartDocument();writer.WriteStartElement("esrarchive");writer.WriteAttributeString("edition","Eastern Sun Resurrected 2.31");writer.WriteAttributeString("runewordSource","https://d2esrdb.github.io/Eastern_Sun_Resurrected/runewords.htm");writer.WriteAttributeString("recipeSource","https://d2esrdb.github.io/Eastern_Sun_Resurrected/Eastern%20Sun%20Resurrected%20Cube%20Recipes.html");
   writer.WriteStartElement("runewords");foreach(var row in words){string title=Name(row[0]);if(title.Length==0)continue;writer.WriteStartElement("word");writer.WriteAttributeString("name",title);string sockets=SocketCount(row[0]);if(sockets.Length>0)writer.WriteAttributeString("sockets",sockets);WriteText(writer,"runes",row.Count>1?row[1]:"");WriteText(writer,"bases",row.Count>2?row[2]:"");string[] labels={"Weapons / Gloves","Helms / Boots","Armor / Shields / Belts"};for(int i=3;i<row.Count&&i<6;i++)if(!String.IsNullOrWhiteSpace(row[i])){writer.WriteStartElement("effects");writer.WriteAttributeString("slot",labels[i-3]);writer.WriteString(row[i]);writer.WriteEndElement();}writer.WriteEndElement();}writer.WriteEndElement();
   writer.WriteStartElement("recipes");foreach(var recipe in recipes){writer.WriteStartElement("recipe");writer.WriteAttributeString("category",recipe.Item1);WriteText(writer,"input",recipe.Item2);WriteText(writer,"output",recipe.Item3);writer.WriteEndElement();}writer.WriteEndElement();writer.WriteEndElement();writer.WriteEndDocument();
  }
  Console.WriteLine("Built ESR archive: "+words.Count+" runewords, "+recipes.Count+" recipes.");
 }
 static void Main(string[] args){if(args.Length!=3)throw new ArgumentException("Supply ESR runeword HTML, ESR recipe HTML and output XML.");WriteArchive(File.ReadAllText(args[0]),File.ReadAllText(args[1]),args[2]);}
}
