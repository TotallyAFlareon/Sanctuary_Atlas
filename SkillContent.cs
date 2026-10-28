using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

// Skill-tree entries in the content package are metadata overrides for the bundled tree.
// The executable still owns the planner's rules, point allocation and UI.
public sealed class SkillContentEntry {
 public string ClassName,Name,Tree,Requires,Description;
 public int Level;
}
public static class SkillContent {
 static readonly HashSet<string> Classes=new HashSet<string>(StringComparer.Ordinal){"Warlock","Amazon","Assassin","Barbarian","Druid","Necromancer","Paladin","Sorceress"};
 static string Key(string className,string name){return className+"\u001f"+name;}
 static string Attribute(XmlNode node,string name,int max,bool allowEmpty){var attr=node.Attributes[name];string value=attr==null?null:attr.Value.Trim();if(value==null||(!allowEmpty&&value.Length==0)||value.Length>max)throw new InvalidOperationException("Invalid skill content.");return value;}
 public static Dictionary<string,SkillContentEntry> Parse(byte[] data){
  var settings=new XmlReaderSettings{DtdProcessing=DtdProcessing.Prohibit,XmlResolver=null,MaxCharactersInDocument=12000000};var doc=new XmlDocument{XmlResolver=null};using(var stream=new MemoryStream(data))using(var reader=XmlReader.Create(stream,settings))doc.Load(reader);
  if(doc.DocumentElement==null||doc.DocumentElement.Name!="runewords")throw new InvalidOperationException("Invalid content package.");
  var sections=doc.SelectNodes("/runewords/skilltrees");if(sections.Count>1)throw new InvalidOperationException("Duplicate skill tree section.");var result=new Dictionary<string,SkillContentEntry>(StringComparer.Ordinal);
  if(sections.Count==0)return result;var classNodes=sections[0].SelectNodes("class");if(classNodes.Count>Classes.Count)throw new InvalidOperationException("Too many skill classes.");
  var classNames=new HashSet<string>(StringComparer.Ordinal);foreach(XmlNode classNode in classNodes){string className=Attribute(classNode,"name",40,false);if(!Classes.Contains(className)||!classNames.Add(className))throw new InvalidOperationException("Invalid or duplicate skill class.");var skills=classNode.SelectNodes("skill");if(skills.Count==0||skills.Count>80)throw new InvalidOperationException("Invalid skill tree.");
   foreach(XmlNode node in skills){string name=Attribute(node,"name",120,false),tree=Attribute(node,"tree",120,false),requires=Attribute(node,"requires",500,true);int level;var levelAttr=Attribute(node,"level",3,false);if(!Int32.TryParse(levelAttr,out level)||level<1||level>99)throw new InvalidOperationException("Invalid skill level.");var descriptions=node.SelectNodes("description");if(descriptions.Count!=1||String.IsNullOrWhiteSpace(descriptions[0].InnerText)||descriptions[0].InnerText.Length>12000)throw new InvalidOperationException("Invalid skill description.");string key=Key(className,name);if(result.ContainsKey(key))throw new InvalidOperationException("Duplicate skill content.");result.Add(key,new SkillContentEntry{ClassName=className,Name=name,Tree=tree,Requires=requires,Level=level,Description=descriptions[0].InnerText.Replace("\r\n","\n").Replace("\r","\n").Replace("\n","\r\n")});}
  }return result;
 }
 public static void Apply(List<Skill> skills,string className,Dictionary<string,SkillContentEntry> updates){foreach(var skill in skills){SkillContentEntry value;if(updates.TryGetValue(Key(className,skill.Name),out value)){skill.Tree=value.Tree;skill.Level=value.Level;skill.Requires=value.Requires;skill.Desc=value.Description;}}}
}
