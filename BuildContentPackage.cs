using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;

// Build a self-contained content snapshot from the bundled guide and recipe effects.
class BuildContentPackage {
 sealed class SkillRow { public string Tree,Name,Requires,Description;public int Level; }
 static string Decode(string value){return Regex.Unescape(value);}
 static Dictionary<string,List<SkillRow>> ReadSkills(string source){
  var found=new Dictionary<string,List<SkillRow>>(StringComparer.Ordinal);string current=null;
  var start=new Regex(@"void\s+InitWarlockSkills\s*\(",RegexOptions.Compiled);var classStart=new Regex(@"if\s*\(className\s*==\s*""(?<class>[A-Za-z]+)""\)",RegexOptions.Compiled);
  var add=new Regex(@"Add\(""(?<tree>(?:[^""\\]|\\.)*)"",\s*""(?<name>(?:[^""\\]|\\.)*)"",\s*(?<level>\d+),\s*""(?<requires>(?:[^""\\]|\\.)*)"",\s*""(?<description>(?:[^""\\]|\\.)*)""\);",RegexOptions.Compiled);
  foreach(string line in File.ReadAllLines(source)){if(start.IsMatch(line))current="Warlock";var classMatch=classStart.Match(line);if(classMatch.Success)current=classMatch.Groups["class"].Value;foreach(Match row in add.Matches(line)){if(current==null)throw new InvalidOperationException("Skill entry has no class.");List<SkillRow> rows;if(!found.TryGetValue(current,out rows)){rows=new List<SkillRow>();found.Add(current,rows);}rows.Add(new SkillRow{Tree=Decode(row.Groups["tree"].Value),Name=Decode(row.Groups["name"].Value),Level=Int32.Parse(row.Groups["level"].Value),Requires=Decode(row.Groups["requires"].Value),Description=Decode(row.Groups["description"].Value)});}}
  if(found.Count!=8||found.Values.Any(x=>x.Count!=30))throw new InvalidOperationException("Expected 30 skill entries for each class: "+String.Join(", ",found.Select(x=>x.Key+"="+x.Value.Count).ToArray()));return found;
 }
 static void Main(string[] args){
  if(args.Length!=3)throw new ArgumentException("Supply source XML, output XML and skill source paths.");
  var doc=new XmlDocument{XmlResolver=null};doc.Load(args[0]);
  if(doc.DocumentElement==null||doc.DocumentElement.Name!="runewords"||doc.SelectSingleNode("/runewords/guidebook")!=null)throw new InvalidOperationException("Expected runeword-only source.");
  var book=doc.CreateElement("guidebook");doc.DocumentElement.AppendChild(book);
  foreach(var region in GuideBook.Regions){var entry=doc.CreateElement("region");entry.SetAttribute("act",region.Act.ToString());entry.SetAttribute("name",region.Name);var details=doc.CreateElement("details");details.InnerText=region.Details();entry.AppendChild(details);book.AppendChild(entry);}
  var planner=doc.CreateElement("farmingplanner");doc.DocumentElement.AppendChild(planner);
  foreach(var route in FarmingPlanner.Routes){var entry=doc.CreateElement("route");entry.SetAttribute("act",route.Act.ToString());entry.SetAttribute("safety",route.Safety.ToString());entry.SetAttribute("rank",route.Rank.ToString());entry.SetAttribute("name",route.Name);entry.SetAttribute("goals",route.Goals);entry.SetAttribute("difficulty",route.Difficulty);entry.SetAttribute("level",route.SuggestedLevel);entry.SetAttribute("start",route.Start);entry.SetAttribute("targets",route.Targets);entry.SetAttribute("lookFor",route.LookFor);entry.SetAttribute("focus",route.Focus);entry.SetAttribute("caution",route.Caution);entry.SetAttribute("guide",route.GuideEntry);planner.AppendChild(entry);}
  var trees=doc.CreateElement("skilltrees");doc.DocumentElement.AppendChild(trees);foreach(var classPair in ReadSkills(args[2])){var classNode=doc.CreateElement("class");classNode.SetAttribute("name",classPair.Key);trees.AppendChild(classNode);foreach(var row in classPair.Value){var skill=doc.CreateElement("skill");skill.SetAttribute("tree",row.Tree);skill.SetAttribute("name",row.Name);skill.SetAttribute("level",row.Level.ToString());skill.SetAttribute("requires",row.Requires);var description=doc.CreateElement("description");description.InnerText=row.Description;skill.AppendChild(description);classNode.AppendChild(skill);}}
  var settings=new XmlWriterSettings{Indent=true,Encoding=new System.Text.UTF8Encoding(false)};using(var writer=XmlWriter.Create(args[1],settings))doc.Save(writer);
 }
}
