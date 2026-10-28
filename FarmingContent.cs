using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;

// Route descriptions are content data. The executable owns filtering and navigation.
public static class FarmingContent {
 static string Attribute(XmlNode node,string name,int max){var attr=node.Attributes[name];string value=attr==null?null:attr.Value.Trim();if(String.IsNullOrWhiteSpace(value)||value.Length>max)throw new InvalidOperationException("Invalid farming route.");return value;}
 static int Number(XmlNode node,string name,int min,int max){int value;if(!Int32.TryParse(Attribute(node,name,8),out value)||value<min||value>max)throw new InvalidOperationException("Invalid farming route.");return value;}
 public static List<FarmingRoute> Parse(byte[] data){
  var settings=new XmlReaderSettings{DtdProcessing=DtdProcessing.Prohibit,XmlResolver=null,MaxCharactersInDocument=12000000};var doc=new XmlDocument{XmlResolver=null};using(var stream=new MemoryStream(data))using(var reader=XmlReader.Create(stream,settings))doc.Load(reader);
  if(doc.DocumentElement==null||doc.DocumentElement.Name!="runewords")throw new InvalidOperationException("Invalid content package.");
  var result=new List<FarmingRoute>(FarmingPlanner.Routes);var sections=doc.SelectNodes("/runewords/farmingplanner");if(sections.Count>1)throw new InvalidOperationException("Duplicate farming planner section.");if(sections.Count==0)return result;
  var routes=sections[0].SelectNodes("route");if(routes.Count>100)throw new InvalidOperationException("Too many farming routes.");var seen=new HashSet<string>(StringComparer.OrdinalIgnoreCase);
  foreach(XmlNode node in routes){string name=Attribute(node,"name",120);if(!seen.Add(name))throw new InvalidOperationException("Duplicate farming route.");int act=Number(node,"act",1,5),safety=Number(node,"safety",1,3),rank=Number(node,"rank",1,99);string goals=Attribute(node,"goals",200),difficulty=Attribute(node,"difficulty",100),level=Attribute(node,"level",100),start=Attribute(node,"start",800),targets=Attribute(node,"targets",800),lookFor=Attribute(node,"lookFor",1200),focus=Attribute(node,"focus",1200),caution=Attribute(node,"caution",1200),guide=Attribute(node,"guide",120);
   int index=result.FindIndex(x=>x.Name.Equals(name,StringComparison.OrdinalIgnoreCase));if(index>=0&&result[index].Act!=act)throw new InvalidOperationException("Farming route act does not match the bundled entry.");var updated=new FarmingRoute(act,safety,rank,name,goals,difficulty,level,start,targets,lookFor,focus,caution,guide);if(index<0)result.Add(updated);else result[index]=updated;
  }
  return result;
 }
}
