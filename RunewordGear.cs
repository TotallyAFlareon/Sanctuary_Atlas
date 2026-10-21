using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

public sealed class RunewordRoll {
 public string Key, Label;
 public int Min, Max;
}

// The recipe text is a category reference, not a complete base-item database.
public static class RunewordGear {
 public static bool Fits(Word word,string slot){
  if(word==null)return false;
  var m=Regex.Match(word.Base??"",@"^\d+-socket\s+(.+)$",RegexOptions.IgnoreCase);
  if(!m.Success)return false;
  foreach(string raw in m.Groups[1].Value.Split(',')){
   string b=raw.Trim();
   if(slot=="Armor"&&b.Equals("Body Armor",StringComparison.OrdinalIgnoreCase))return true;
   if(slot=="Helm"&&b.Equals("Helms",StringComparison.OrdinalIgnoreCase))return true;
   if(slot=="Off-hand"&&(b.IndexOf("Shield",StringComparison.OrdinalIgnoreCase)>=0||b.Equals("Grimoires",StringComparison.OrdinalIgnoreCase)||b.Equals("Shrunken Heads",StringComparison.OrdinalIgnoreCase)))return true;
   if(slot=="Weapon"&&!b.Equals("Body Armor",StringComparison.OrdinalIgnoreCase)&&!b.Equals("Helms",StringComparison.OrdinalIgnoreCase)&&b.IndexOf("Shield",StringComparison.OrdinalIgnoreCase)<0&&!b.Equals("Grimoires",StringComparison.OrdinalIgnoreCase)&&!b.Equals("Shrunken Heads",StringComparison.OrdinalIgnoreCase))return true;
  }
  return false;
 }
 public static List<string> Effects(Word word,string slot){
  var found=new List<string>();bool include=true;
  foreach(string raw in (word.Effects??"").Replace("\r","").Split('\n')){
   string line=raw.Trim();if(line.Length==0)continue;
   if(!line.StartsWith("•")){
    if(!line.EndsWith(":"))continue;
    string group=line.TrimEnd(':').Trim();
    include=slot=="Armor"?group.IndexOf("ARMOR",StringComparison.OrdinalIgnoreCase)>=0:slot=="Helm"?group.IndexOf("HELM",StringComparison.OrdinalIgnoreCase)>=0:slot=="Off-hand"?group.IndexOf("SHIELD",StringComparison.OrdinalIgnoreCase)>=0:slot=="Weapon"&&(group.IndexOf("WEAPON",StringComparison.OrdinalIgnoreCase)>=0||group.IndexOf("SWORD",StringComparison.OrdinalIgnoreCase)>=0);
   }else if(include)found.Add(line.Substring(1).Trim());
  }
  return found;
 }
 public static List<RunewordRoll> Rolls(Word word,string slot,List<Skill> skills){
  var output=new List<RunewordRoll>();int index=0;
  foreach(string effect in Effects(word,slot)){
   var m=Regex.Match(effect,@"^\+?(\d+)(?:-(\d+))?(%)?\s+(?:to\s+)?(.+?)(?:\s*\(varies\))?$",RegexOptions.IgnoreCase);
   if(!m.Success){index++;continue;}
   string label=m.Groups[4].Value.Trim(),key=null;
   if(m.Groups[3].Success){if(label.Equals("Faster Cast Rate",StringComparison.OrdinalIgnoreCase))key="FCR";else if(label.Equals("Faster Hit Recovery",StringComparison.OrdinalIgnoreCase))key="FHR";else if(label.Equals("Faster Block Rate",StringComparison.OrdinalIgnoreCase))key="FBR";}
   else if(label.Equals("All Skills",StringComparison.OrdinalIgnoreCase)||label.Equals("All Skill Levels",StringComparison.OrdinalIgnoreCase))key="All Skills";
   else if(label.Equals("Warlock Skill Levels",StringComparison.OrdinalIgnoreCase))key="Warlock Skills";
   else if(label.Equals("Fire Skills",StringComparison.OrdinalIgnoreCase))key="Fire Skills";
   else if(skills!=null){var skill=skills.FirstOrDefault(x=>x.Name.Equals(label,StringComparison.OrdinalIgnoreCase));if(skill!=null)key=skill.Name;}
   if(key!=null){int min=Int32.Parse(m.Groups[1].Value),max=m.Groups[2].Success?Int32.Parse(m.Groups[2].Value):min;output.Add(new RunewordRoll{Key=key,Label=effect,Min=min,Max=max});}index++;
  }return output;
 }
 public static ItemSkillBonuses Create(Word word,string slot,List<Skill> skills,IList<int> values){
  if(!Fits(word,slot))throw new ArgumentException("This recipe does not list a base for that slot.");
  var effects=Effects(word,slot);if(effects.Count==0)throw new ArgumentException("No effects are available for that base.");
  var rolls=Rolls(word,slot,skills);if(values==null||values.Count!=rolls.Count)throw new ArgumentException("Choose values for all supported bonuses.");
  var item=new ItemSkillBonuses{RunewordName=word.Name,RunewordBase=slot,RunewordEffects=String.Join("\n",effects.ToArray())};var lines=new List<string>();
  for(int i=0;i<rolls.Count;i++){
   var r=rolls[i];int value=values[i];if(value<r.Min||value>r.Max)throw new ArgumentOutOfRangeException("values");
   if(r.Key=="FCR")item.Fcr+=value;else if(r.Key=="FHR")item.Fhr+=value;else if(r.Key=="FBR")item.Fbr+=value;else lines.Add(r.Key+": +"+value);
  }item.Lines=String.Join("\n",lines.ToArray());return item;
 }
}
