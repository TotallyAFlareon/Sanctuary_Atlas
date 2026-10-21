using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using System.Xml.Serialization;

class RunewordGearCheck {
 static void Need(bool condition,string message){if(!condition)throw new Exception(message);}
 [STAThread] static int Main(){try{Run();Console.WriteLine("Runeword equipment, rolls, variants and saved-build checks passed.");return 0;}catch(Exception ex){Console.Error.WriteLine(ex);return 1;}}
 static void Run(){
  string dir=Path.Combine(Environment.CurrentDirectory,"test-builds",Guid.NewGuid().ToString("N"));Directory.CreateDirectory(dir);
  try{using(var app=new Atlas(dir)){
   var flags=BindingFlags.NonPublic|BindingFlags.Instance;var words=(List<Word>)typeof(Atlas).GetField("words",flags).GetValue(app);var skills=(List<Skill>)typeof(Atlas).GetField("skills",flags).GetValue(app);
   foreach(var word in words){int count=0;foreach(string slot in new[]{"Weapon","Off-hand","Armor","Helm"})if(RunewordGear.Fits(word,slot)){count++;Need(RunewordGear.Effects(word,slot).Count>0,word.Name+" effects missing for "+slot);var rolls=RunewordGear.Rolls(word,slot,skills);var data=RunewordGear.Create(word,slot,skills,rolls.Select(r=>r.Min).ToArray());GearSkillBonuses.Parse(data.Lines,skills);}Need(count>0,word.Name+" has no slot");}
   var spirit=words.First(x=>x.Name=="Spirit");Need(RunewordGear.Fits(spirit,"Weapon")&&RunewordGear.Fits(spirit,"Off-hand")&&!RunewordGear.Fits(spirit,"Armor"),"Spirit slots");
   var sword=RunewordGear.Effects(spirit,"Weapon");var shield=RunewordGear.Effects(spirit,"Off-hand");Need(sword.Any(x=>x.Contains("Life Stolen"))&&!shield.Any(x=>x.Contains("Life Stolen"))&&shield.Any(x=>x.Contains("Cold Resist")),"Spirit variants");
   var rollsSpirit=RunewordGear.Rolls(spirit,"Weapon",skills);Need(rollsSpirit.Any(x=>x.Key=="FCR"&&x.Min==25&&x.Max==35)&&rollsSpirit.Any(x=>x.Key=="FHR"&&x.Min==55),"Spirit rolls");
   var chosen=rollsSpirit.Select(x=>x.Key=="FCR"?30:x.Min).ToArray();var item=RunewordGear.Create(spirit,"Weapon",skills,chosen);Need(item.Fcr==30&&item.Fhr==55&&item.Lines.Contains("All Skills: +2"),"Spirit bonuses");
   bool rejected=false;try{var invalid=rollsSpirit.Select(x=>x.Key=="FCR"?36:x.Min).ToArray();RunewordGear.Create(spirit,"Weapon",skills,invalid);}catch(ArgumentOutOfRangeException){rejected=true;}Need(rejected,"Invalid roll accepted");
   var armor=words.First(x=>x.Name=="Fortitude");Need(RunewordGear.Effects(armor,"Weapon").Any(x=>x.Contains("Deadly Strike"))&&!RunewordGear.Effects(armor,"Armor").Any(x=>x.Contains("Deadly Strike")),"Fortitude variants");
   var authority=words.First(x=>x.Name=="Authority");var authRolls=RunewordGear.Rolls(authority,"Armor",skills);var auth=RunewordGear.Create(authority,"Armor",skills,authRolls.Select(x=>x.Min).ToArray());Need(auth.Lines.Contains("Warlock Skills: +2")&&auth.Fhr==20,"Authority bonuses");
   var b=new Build();b.Gear[0]="Spirit";b.GearSkillBonuses[0]=item;b.Gear[3]="Authority";b.GearSkillBonuses[3]=auth;Need(GearSpeedBonuses.ForBuild(b).Fcr==30&&GearSpeedBonuses.ForBuild(b).Fhr==75,"Combined speed totals");var levels=GearSkillBonuses.ForBuild(b,skills);Need(levels.All==2&&levels.Class==2,"Combined Warlock skill totals");
   var serializer=new XmlSerializer(typeof(Build));string xml;using(var writer=new StringWriter()){serializer.Serialize(writer,b);xml=writer.ToString();}Build loaded;using(var reader=new StringReader(xml))loaded=(Build)serializer.Deserialize(reader);Need(loaded.GearSkillBonuses[0].RunewordName=="Spirit"&&loaded.GearSkillBonuses[0].Fcr==30&&loaded.GearSkillBonuses[0].RunewordEffects.Contains("Life Stolen"),"Runeword save and reload");
  }}finally{Directory.Delete(dir,true);}
 }
}
