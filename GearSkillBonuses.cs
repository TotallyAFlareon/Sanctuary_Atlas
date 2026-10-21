using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

// The user enters bonuses as printed on each item. This does not infer item stats,
// check slot compatibility, or treat charged skills as permanent skill levels.
[Serializable] public sealed class ItemSkillBonuses {
 public string Lines="";
 public bool ActiveCharm;
 public int Fcr, Fhr, Fbr;
 // Nonempty only for items created from the verified unsocketed-unique list.
 public string VerifiedName="";
 public string RunewordName="", RunewordBase="", RunewordEffects="";
 public List<ItemStat> OtherStats=new List<ItemStat>();
}
[Serializable] public sealed class ItemStat {
 public string Name="";
 public int Value;
}
public sealed class ItemEditorLine {
 public string SkillLine;
 public ItemStat Stat;
 public override string ToString(){return Stat==null?SkillLine:Stat.Name+": "+(Stat.Value>=0?"+":"")+Stat.Value;}
}
public static class EquipmentStatOptions {
 // Common modifiers for manually entered items. A slot alone does not prove
 // that a particular base, quality, roll or combination can exist.
 public static string[] ForSlot(string slot){
  var list=new List<string>{"Fire Resistance (%)","Cold Resistance (%)","Lightning Resistance (%)","Poison Resistance (%)","All Resistances (%)","Life","Mana","Strength","Dexterity","Vitality","Energy","Magic Find (%)"};
  if(slot=="Weapon"||slot=="Off-hand"||slot=="Gloves"||slot=="Amulet"||slot=="Ring 1"||slot=="Ring 2")list.AddRange(new[]{"Minimum Damage","Maximum Damage","Attack Rating"});
  if(slot=="Weapon"||slot=="Gloves")list.Add("Increased Attack Speed (%)");
  if(slot=="Weapon")list.AddRange(new[]{"Enhanced Weapon Damage (%)","Damage to Demons (%)","Damage to Undead (%)","Life Stolen Per Hit (%)","Mana Stolen Per Hit (%)"});
  return list.ToArray();
 }
}
public sealed class SpeedBonusTotals {
 public int Fcr, Fhr, Fbr;
}
public static class GearSpeedBonuses {
 static void Read(SpeedBonusTotals totals,string item,ItemSkillBonuses data){
  if(String.IsNullOrWhiteSpace(item)||data==null)return;
  totals.Fcr+=Math.Max(0,Math.Min(1000,data.Fcr));totals.Fhr+=Math.Max(0,Math.Min(1000,data.Fhr));totals.Fbr+=Math.Max(0,Math.Min(1000,data.Fbr));
 }
 public static SpeedBonusTotals ForBuild(Build build){
  var totals=new SpeedBonusTotals();
  if(build.Gear!=null&&build.GearSkillBonuses!=null)for(int i=0;i<Math.Min(build.Gear.Length,build.GearSkillBonuses.Length);i++)Read(totals,build.Gear[i],build.GearSkillBonuses[i]);
  if(build.Inventory!=null&&build.InventorySkillBonuses!=null)for(int i=0;i<Math.Min(build.Inventory.Length,build.InventorySkillBonuses.Length);i++){
   var data=build.InventorySkillBonuses[i];if(data!=null&&data.ActiveCharm)Read(totals,build.Inventory[i],data);
  }
  return totals;
 }
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
   string line=raw.Trim();if(line.Length==0)continue;int amount;string label;
   var printed=Regex.Match(line,@"^\+?(\d+)\s+to\s+(.+?)(?:\s*\(Warlock Only\))?$",RegexOptions.IgnoreCase);
   if(printed.Success){label=printed.Groups[2].Value.Trim();if(!Int32.TryParse(printed.Groups[1].Value,out amount))throw new FormatException("Bonus amount is too large.");}
   else{var trailing=Regex.Match(line,@"^(.+?)\s+\+(\d+)$");if(trailing.Success){label=trailing.Groups[1].Value.Trim().TrimEnd(':').Trim();if(!Int32.TryParse(trailing.Groups[2].Value,out amount))throw new FormatException("Bonus amount is too large.");}else{int split=line.LastIndexOf(':');if(split<1||!Int32.TryParse(line.Substring(split+1).Trim(),NumberStyles.AllowLeadingSign,CultureInfo.InvariantCulture,out amount))throw new FormatException("Use one bonus per line, for example: Warlock Skills: +10 or +10 to Warlock Skill Levels.");label=line.Substring(0,split).Trim();}}
   if(amount<0||amount>100)throw new FormatException("Each bonus must be between 0 and 100.");
   if(label.Equals("All Skill Levels",StringComparison.OrdinalIgnoreCase))label="All Skills";
   if(label.Equals("Warlock Skill Levels",StringComparison.OrdinalIgnoreCase))label="Warlock Skills";
   Add(total,label,amount,skills);
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
 public static SkillLevels LevelsForBuild(Build build,List<Skill> skills){
  var levels=new SkillLevels{CharacterLevel=build.Level};
  for(int i=0;i<skills.Count&&i<build.Points.Length;i++)levels.SetHard(i+1,build.Points[i]);
  Apply(ForBuild(build,skills),skills,levels);return levels;
 }
 public static string Summary(WarlockBonusTotals t){
  var parts=new List<string>();
  if(t.All>0)parts.Add("+"+t.All+" All Skills");if(t.Class>0)parts.Add("+"+t.Class+" Warlock Skills");
  if(t.Demon>0)parts.Add("+"+t.Demon+" Demon");if(t.Eldritch>0)parts.Add("+"+t.Eldritch+" Eldritch");if(t.Chaos>0)parts.Add("+"+t.Chaos+" Chaos");if(t.Fire>0)parts.Add("+"+t.Fire+" Fire Skills");
  foreach(var item in t.Individual.OrderBy(x=>x.Key))if(item.Value>0)parts.Add("+"+item.Value+" "+item.Key);
  if(t.InvalidItems>0)parts.Add("Some entries need correction");
  return parts.Count==0?"No active skill bonuses entered.":String.Join(", ",parts.ToArray());
 }
}
