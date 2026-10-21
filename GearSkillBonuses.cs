using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

// The user enters bonuses as printed on each item. This does not infer item stats,
// check slot compatibility, or treat charged skills as permanent skill levels.
[Serializable] public sealed class ItemSkillBonuses {
 public string Lines="";
 public bool ActiveCharm;
}
public sealed class WarlockBonusTotals {
 public int All,Class,Demon,Eldritch,Chaos,Fire;
 public readonly Dictionary<string,int> Individual=new Dictionary<string,int>(StringComparer.OrdinalIgnoreCase);
 public int InvalidItems;
}
public static class GearSkillBonuses {
 static void Add(WarlockBonusTotals t,string label,int value,List<Skill> skills){
  if(label.Equals("All Skills",StringComparison.OrdinalIgnoreCase))t.All=checked(t.All+value);
  else if(label.Equals("Warlock Skills",StringComparison.OrdinalIgnoreCase))t.Class=checked(t.Class+value);
  else if(label.Equals("Demon Skills",StringComparison.OrdinalIgnoreCase))t.Demon=checked(t.Demon+value);
  else if(label.Equals("Eldritch Skills",StringComparison.OrdinalIgnoreCase))t.Eldritch=checked(t.Eldritch+value);
  else if(label.Equals("Chaos Skills",StringComparison.OrdinalIgnoreCase))t.Chaos=checked(t.Chaos+value);
  else if(label.Equals("Fire Skills",StringComparison.OrdinalIgnoreCase))t.Fire=checked(t.Fire+value);
  else{
   var skill=skills.FirstOrDefault(s=>s.Name.Equals(label,StringComparison.OrdinalIgnoreCase));
   if(skill==null)throw new FormatException("Unknown bonus: "+label+". Use a listed category or an exact Warlock skill name.");
   int old;t.Individual.TryGetValue(skill.Name,out old);t.Individual[skill.Name]=checked(old+value);
  }
 }
 public static WarlockBonusTotals Parse(string lines,List<Skill> skills){
  var total=new WarlockBonusTotals();if(String.IsNullOrWhiteSpace(lines))return total;
  foreach(string raw in lines.Replace("\r","").Split('\n')){
   string line=raw.Trim();if(line.Length==0)continue;int split=line.LastIndexOf(':');int amount;
   if(split<1||!Int32.TryParse(line.Substring(split+1).Trim(),NumberStyles.AllowLeadingSign,CultureInfo.InvariantCulture,out amount)||amount<0||amount>100)
    throw new FormatException("Use one bonus per line, for example: All Skills: +2");
   Add(total,line.Substring(0,split).Trim(),amount,skills);
  }
  return total;
 }
 static void Merge(WarlockBonusTotals target,WarlockBonusTotals source,List<Skill> skills){
  target.All=checked(target.All+source.All);target.Class=checked(target.Class+source.Class);
  target.Demon=checked(target.Demon+source.Demon);target.Eldritch=checked(target.Eldritch+source.Eldritch);target.Chaos=checked(target.Chaos+source.Chaos);target.Fire=checked(target.Fire+source.Fire);
  foreach(var v in source.Individual)Add(target,v.Key,v.Value,skills);
 }
 static void Read(WarlockBonusTotals total,string item,ItemSkillBonuses data,List<Skill> skills){
  if(String.IsNullOrWhiteSpace(item)||data==null||String.IsNullOrWhiteSpace(data.Lines))return;
  try{Merge(total,Parse(data.Lines,skills),skills);}catch(FormatException){total.InvalidItems++;}catch(OverflowException){total.InvalidItems++;}
 }
 public static WarlockBonusTotals ForBuild(Build build,List<Skill> skills){
  var total=new WarlockBonusTotals();
  if(build.Gear!=null&&build.GearSkillBonuses!=null)for(int i=0;i<Math.Min(build.Gear.Length,build.GearSkillBonuses.Length);i++)Read(total,build.Gear[i],build.GearSkillBonuses[i],skills);
  if(build.Inventory!=null&&build.InventorySkillBonuses!=null)for(int i=0;i<Math.Min(build.Inventory.Length,build.InventorySkillBonuses.Length);i++){
   var data=build.InventorySkillBonuses[i];if(data!=null&&data.ActiveCharm)Read(total,build.Inventory[i],data,skills);
  }
  return total;
 }
 public static void Apply(WarlockBonusTotals total,List<Skill> skills,SkillLevels levels){
  levels.AllSkillsBonus=checked(total.All+total.Class);
  levels.SetTreeBonus(0,total.Demon);levels.SetTreeBonus(1,total.Eldritch);levels.SetTreeBonus(2,total.Chaos);
  for(int i=0;i<skills.Count;i++){
   int tree=skills[i].Tree=="Demon"?0:skills[i].Tree=="Eldritch"?1:skills[i].Tree=="Chaos"?2:-1;
   if(tree>=0)levels.SetTree(i+1,tree);
   // Element-tagged skills in build 3.3.93847; fire bonuses do not grant unlearned skills.
   if(new[]{"Summon Tainted","Blood Boil","Ring of Fire","Flame Wave","Sigil: Death","Apocalypse"}.Contains(skills[i].Name))levels.SetConditionalBonus(i+1,total.Fire);
   int value;if(total.Individual.TryGetValue(skills[i].Name,out value))levels.SetSkillBonus(i+1,value);
  }
 }
}
