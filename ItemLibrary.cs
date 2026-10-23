using System;
using System.Collections.Generic;
using System.Linq;

// Search references already available in the app. Unique records currently
// include only the supported fixed skill and speed modifiers, not every stat.
public sealed class ItemLibraryEntry {
 public VerifiedEquipmentRecord Unique;
 public Word Runeword;
 public string Name {get{return Unique!=null?Unique.Name:Runeword.Name;}}
 public override string ToString(){return (Unique!=null?"UNIQUE  |  ":"RUNEWORD  |  ")+Name;}
}
public static class ItemLibrary {
 public static List<ItemLibraryEntry> Search(IEnumerable<Word> words,string query,string kind){
  string q=(query??"").Trim();var items=new List<ItemLibraryEntry>();
  if(kind!="Runewords")foreach(var u in VerifiedEquipment.All()){
   string text=u.Name+" "+u.Base+" "+u.Slot+" "+(u.AllSkills>0?"all skills ":"")+(u.FireSkills>0?"fire skills ":"")+(u.Fcr>0?"fcr faster cast rate ":"")+(u.Fhr>0?"fhr faster hit recovery ":"")+(u.Fbr>0?"fbr faster block rate":"");
   if(text.IndexOf(q,StringComparison.OrdinalIgnoreCase)>=0)items.Add(new ItemLibraryEntry{Unique=u});
  }
  if(kind!="Uniques"&&words!=null)foreach(var w in words){
   if((w.Name+" "+w.Base+" "+w.Runes+" "+w.Effects).IndexOf(q,StringComparison.OrdinalIgnoreCase)>=0)items.Add(new ItemLibraryEntry{Runeword=w});
  }
  return items.OrderBy(x=>x.Name,StringComparer.OrdinalIgnoreCase).ToList();
 }
 public static string Description(ItemLibraryEntry item){
  if(item==null)return "No matching items.";
  if(item.Unique!=null){var u=item.Unique;var lines=new List<string>();
   if(u.AllSkills>0)lines.Add("+"+u.AllSkills+" to All Skills");
   if(u.FireSkills>0)lines.Add("+"+u.FireSkills+" to Fire Skills");
   if(u.Fcr>0)lines.Add(u.Fcr+"% Faster Cast Rate");
   if(u.Fhr>0)lines.Add(u.Fhr+"% Faster Hit Recovery");
   if(u.Fbr>0)lines.Add(u.Fbr+"% Faster Block Rate");
   return u.Name.ToUpperInvariant()+"\r\nUnique "+u.Base+"  •  "+u.Slot+"\r\n\r\nSUPPORTED BONUSES\r\n"+String.Join("\r\n",lines.ToArray())+"\r\n\r\nThese are the verified skill and speed modifiers supported by the planner. Other properties, variable rolls, sockets and upgrades are not included yet.";
  }
  var w=item.Runeword;return w.Name.ToUpperInvariant()+"\r\nRuneword\r\n\r\nRUNE ORDER\r\n"+w.Runes+"\r\n\r\nELIGIBLE BASE\r\n"+w.Base+"\r\n\r\nEFFECTS\r\n"+(w.Effects??"").Replace("\n","\r\n")+"\r\n\r\n"+w.Note+"\r\n\r\nUse the runeword library to check your stash and create gear. Verify base and game-mode restrictions in game.";
 }
}

