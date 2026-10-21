using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;

// A content package is a complete snapshot. Omitted guide entries fall back to the bundled guide.
public static class GuideContent {
 public static List<GuideRegion> Parse(byte[] data){
  var settings=new XmlReaderSettings{DtdProcessing=DtdProcessing.Prohibit,XmlResolver=null,MaxCharactersInDocument=2000000};
  var doc=new XmlDocument{XmlResolver=null};using(var stream=new MemoryStream(data))using(var reader=XmlReader.Create(stream,settings))doc.Load(reader);
  if(doc.DocumentElement==null||doc.DocumentElement.Name!="runewords")throw new InvalidOperationException("Invalid content package.");
  var result=new List<GuideRegion>(GuideBook.Regions);var sections=doc.SelectNodes("/runewords/guidebook");if(sections.Count>1)throw new InvalidOperationException("Duplicate guide book section.");
  var seen=new HashSet<string>(StringComparer.OrdinalIgnoreCase);
  if(sections.Count==0)return result;
  var regions=sections[0].SelectNodes("region");if(regions.Count>500)throw new InvalidOperationException("Too many guide regions.");
  foreach(XmlNode node in regions){
   int act;var actAttr=node.Attributes["act"];var nameAttr=node.Attributes["name"];string name=nameAttr==null?"":nameAttr.Value.Trim();
   if(actAttr==null||!Int32.TryParse(actAttr.Value,out act)||act<1||act>5||name.Length<1||name.Length>120||!seen.Add(name))throw new InvalidOperationException("Invalid or duplicate guide region.");
   var details=node.SelectNodes("details");if(details.Count!=1||String.IsNullOrWhiteSpace(details[0].InnerText)||details[0].InnerText.Length>30000)throw new InvalidOperationException("Invalid guide details.");
   string body=details[0].InnerText.Replace("\r\n","\n").Replace("\r","\n").Replace("\n","\r\n");
   int index=result.FindIndex(x=>x.Name.Equals(name,StringComparison.OrdinalIgnoreCase));
   if(index>=0&&result[index].Act!=act)throw new InvalidOperationException("Guide region act does not match the bundled entry.");
   var updated=new GuideRegion(act,name,"","",""){UpdatedDetails=body};if(index<0)result.Add(updated);else result[index]=updated;
  }
  return result;
 }
}
