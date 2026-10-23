using System;
using System.Collections.Generic;

// Totals entered or supported gear bonuses; not the in-game character sheet.
public static class GearOverview {
 static void Add(Dictionary<string,int> totals,string name,ItemSkillBonuses data){
  if(String.IsNullOrWhiteSpace(name)||data==null||data.OtherStats==null)return;
  foreach(var stat in data.OtherStats){if(stat==null||String.IsNullOrWhiteSpace(stat.Name))continue;int n;totals.TryGetValue(stat.Name,out n);totals[stat.Name]=checked(n+stat.Value);}
 }
 public static Dictionary<string,int> ForBuild(Build build){
  var totals=new Dictionary<string,int>(StringComparer.OrdinalIgnoreCase);
  if(build.Gear!=null&&build.GearSkillBonuses!=null)for(int i=0;i<Math.Min(build.Gear.Length,build.GearSkillBonuses.Length);i++)Add(totals,build.Gear[i],build.GearSkillBonuses[i]);
  if(build.Inventory!=null&&build.InventorySkillBonuses!=null)for(int i=0;i<Math.Min(build.Inventory.Length,build.InventorySkillBonuses.Length);i++){var b=build.InventorySkillBonuses[i];if(b!=null&&b.ActiveCharm)Add(totals,build.Inventory[i],b);}
  return totals;
 }
 public static string Format(Build build,List<Skill> skills,bool warlock){
  var speed=GearSpeedBonuses.ForBuild(build);var lines=new List<string>();
  lines.Add("EQUIPPED GEAR + ACTIVE CHARMS");lines.Add("");
  lines.Add("Faster Cast Rate: "+speed.Fcr+"%");lines.Add("Faster Hit Recovery: "+speed.Fhr+"%");lines.Add("Faster Block Rate: "+speed.Fbr+"%");
  if(warlock){lines.Add("");lines.Add("Skill bonuses: "+GearSkillBonuses.Summary(GearSkillBonuses.ForBuild(build,skills)));}
  var totals=ForBuild(build);lines.Add("");lines.Add("OTHER ENTERED BONUSES");
  if(totals.Count==0)lines.Add("None entered.");else{var keys=new List<string>(totals.Keys);keys.Sort(StringComparer.OrdinalIgnoreCase);foreach(string key in keys)lines.Add(key+": "+(totals[key]>=0?"+":"")+totals[key]);}
  lines.Add("");lines.Add("Only stored bonuses are counted. Presets and runewords currently supply a limited set of effects; item names alone do not add stats. These are gear bonuses, not final character values: difficulty penalties, base attributes, caps, skills, auras and unmodeled modifiers are not included.");
  if(!warlock)lines.Add("Skill bonus totals are currently available for Warlocks.");
  return String.Join("\r\n",lines.ToArray());
 }
}

