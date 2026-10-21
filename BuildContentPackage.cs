using System;
using System.Xml;

// Build a self-contained content snapshot from the bundled guide and recipe effects.
class BuildContentPackage {
 static void Main(string[] args){
  if(args.Length!=2)throw new ArgumentException("Supply source XML and output XML paths.");
  var doc=new XmlDocument{XmlResolver=null};doc.Load(args[0]);
  if(doc.DocumentElement==null||doc.DocumentElement.Name!="runewords"||doc.SelectSingleNode("/runewords/guidebook")!=null)throw new InvalidOperationException("Expected runeword-only source.");
  var book=doc.CreateElement("guidebook");doc.DocumentElement.AppendChild(book);
  foreach(var region in GuideBook.Regions){var entry=doc.CreateElement("region");entry.SetAttribute("act",region.Act.ToString());entry.SetAttribute("name",region.Name);var details=doc.CreateElement("details");details.InnerText=region.Details();entry.AppendChild(details);book.AppendChild(entry);}
  var settings=new XmlWriterSettings{Indent=true,Encoding=new System.Text.UTF8Encoding(false)};using(var writer=XmlWriter.Create(args[1],settings))doc.Save(writer);
 }
}
