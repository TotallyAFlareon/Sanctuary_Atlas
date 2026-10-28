using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;

public sealed class EsrRuneword {
 public string Name="",Sockets="",Runes="",Bases=""; public List<EsrEffectGroup> Effects=new List<EsrEffectGroup>();
 static string OneLine(string value){return (value??"").Replace("\r","").Replace("\n"," / ");}
 public override string ToString(){return Name+(String.IsNullOrWhiteSpace(Sockets)?"":"  ["+Sockets+"S]")+"  •  "+OneLine(Runes);}
 public string Details(){var lines=new List<string>();lines.Add(Name.ToUpperInvariant()+(String.IsNullOrWhiteSpace(Sockets)?"":"  •  "+Sockets+" SOCKET"+(Sockets=="1"?"":"S")));lines.Add("");lines.Add("RUNES");lines.Add(Runes);lines.Add("");lines.Add("ALLOWED ITEMS");lines.Add(Bases);foreach(var group in Effects){lines.Add("");lines.Add(group.Slot.ToUpperInvariant());lines.Add(group.Text);}return String.Join("\r\n",lines.ToArray());}
 public string SearchText(){return Name+" "+Sockets+" "+Runes+" "+Bases+" "+String.Join(" ",Effects.Select(x=>x.Slot+" "+x.Text).ToArray());}
}
public sealed class EsrEffectGroup { public string Slot="",Text=""; }
public sealed class EsrRecipe {
 public string Category="",Input="",Output=""; static string OneLine(string value){return (value??"").Replace("\r","").Replace("\n"," / ");}
 public override string ToString(){return Category+"  •  "+OneLine(Input)+"  →  "+OneLine(Output);}
 public string Details(){return Category.ToUpperInvariant()+"\r\n\r\nINPUT\r\n"+Input+"\r\n\r\nOUTPUT\r\n"+Output;}
 public string SearchText(){return Category+" "+Input+" "+Output;}
}
public sealed class EsrArchive {
 public string Edition="Eastern Sun Resurrected",RunewordSource="",RecipeSource=""; public List<EsrRuneword> Runewords=new List<EsrRuneword>(); public List<EsrRecipe> Recipes=new List<EsrRecipe>();
 public bool HasEntries{get{return Runewords.Count>0&&Recipes.Count>0;}}
 static string Text(XmlNode parent,string name){var node=parent.SelectSingleNode(name);return node==null?"":node.InnerText.Trim();}
 public static EsrArchive Parse(byte[] data){
  var result=new EsrArchive();if(data==null||data.Length==0)return result;if(data.Length>12000000)throw new InvalidOperationException("The ESR archive is too large.");
  var settings=new XmlReaderSettings{DtdProcessing=DtdProcessing.Prohibit,XmlResolver=null,MaxCharactersInDocument=12000000};var doc=new XmlDocument{XmlResolver=null};using(var stream=new MemoryStream(data))using(var reader=XmlReader.Create(stream,settings))doc.Load(reader);
  XmlNode root=doc.SelectSingleNode("/runewords/esrarchive")??doc.SelectSingleNode("/esrarchive");if(root==null)return result;result.Edition=root.Attributes["edition"]==null?result.Edition:root.Attributes["edition"].Value;result.RunewordSource=root.Attributes["runewordSource"]==null?"":root.Attributes["runewordSource"].Value;result.RecipeSource=root.Attributes["recipeSource"]==null?"":root.Attributes["recipeSource"].Value;
  foreach(XmlNode node in root.SelectNodes("runewords/word")){string name=node.Attributes["name"]==null?"":node.Attributes["name"].Value.Trim();if(name.Length==0)continue;var word=new EsrRuneword{Name=name,Sockets=node.Attributes["sockets"]==null?"":node.Attributes["sockets"].Value.Trim(),Runes=Text(node,"runes"),Bases=Text(node,"bases")};foreach(XmlNode group in node.SelectNodes("effects")){string value=group.InnerText.Trim();if(value.Length>0)word.Effects.Add(new EsrEffectGroup{Slot=group.Attributes["slot"]==null?"Effects":group.Attributes["slot"].Value,Text=value});}if(word.Runes.Length>0&&word.Bases.Length>0)result.Runewords.Add(word);}
  foreach(XmlNode node in root.SelectNodes("recipes/recipe")){string category=node.Attributes["category"]==null?"Other":node.Attributes["category"].Value.Trim();string input=Text(node,"input"),output=Text(node,"output");if(input.Length>0&&output.Length>0)result.Recipes.Add(new EsrRecipe{Category=category,Input=input,Output=output});}
  if(result.Runewords.Count>100||result.Recipes.Count>100)return result;throw new InvalidOperationException("The ESR archive is incomplete.");
 }
}
