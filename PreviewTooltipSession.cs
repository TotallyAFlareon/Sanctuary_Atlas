using System;
using System.Collections.Generic;
using System.Linq;

// Small independently authored preview. Other skills deliberately have no calculated stats.
// References: https://d2r.org/mechanics/summon-goatman, /ring-of-fire,
// /demonic-mastery, /blood-oath, /summon-tainted, /summon-defiler,
// /cold-arrow, /ice-bolt, /frost-nova and /stun (build 3.3.93847).
public sealed class PreviewTooltipSession {
 readonly string className;
 readonly List<Skill> skills;
 public PreviewTooltipSession(string className,List<Skill> skills){this.className=className;this.skills=skills;}
 int Id(string name){int n=skills.FindIndex(s=>s.Name==name);if(n<0)throw new InvalidOperationException("Missing skill: "+name);return n+1;}
 static void Stat(SkillTooltipDefinition d,string template,string formula,bool whole=false,string end=null){d.Stats.Add(new SkillTooltipStat{Template=template,Formula=formula,FormulaEnd=end,WholeNumber=whole});}
 public string Render(Skill skill,Build build){
  var d=new SkillTooltipDefinition{Id=Id(skill.Name),Name=skill.Name,Description=skill.Desc,RequiredLevel=skill.Level,Prerequisites=(skill.Requires??"").Replace("|"," + ")};
  bool hasDetails=true;
  if(className=="Warlock"&&skill.Name=="Summon Goatman"){
   string oath="slvl("+Id("Blood Oath")+")", mastery="slvl("+Id("Demonic Mastery")+")";
   Stat(d,"Mana Cost: {value}","15+(lvl-1)/2");
   Stat(d,"Defense: {value}","100+20*(lvl-1)",true);
   Stat(d,"Summon life bonus: {value}%","35*(lvl-1)+( "+oath+">0 ? 50+35*("+oath+"-1) : 0)",true);
   Stat(d,"Damage bonus: +{value}%","10*lvl+10*"+mastery,true);
   Stat(d,"Flat damage bonus: +{value}","1+2*(lvl-1)",true);
   Stat(d,"Attack rating bonus: +{value}","100+40*lvl+25*"+mastery,true);
   Stat(d,"Attack speed bonus: +{value}%","min(9+lvl,25)+("+mastery+">0?min(4+"+mastery+",25):0)",true);
   Stat(d,"Crushing Blow: {value}%","5+blvl("+Id("Death Mark")+")",true);
   d.Notes.Add("Life is a bonus, not total life. Damage and attack rating include Demonic Mastery; Crushing Blow uses invested Death Mark points.");
   d.Notes.Add("Goatman gains Stun at level 2, Berserk at 3, Frenzy at 4, and Cleave at 5.");
  }else if(className=="Warlock"&&skill.Name=="Demonic Mastery"){
   Stat(d,"Demon damage bonus: +{value}%","10*lvl",true);
   Stat(d,"Demon attack speed: +{value}%","min(4+lvl,25)",true);
   Stat(d,"Demon attack rating: +{value}","25*lvl",true);
   Stat(d,"Demon run/walk speed: +{value}%","5+idiv(34*min(100,idiv(110*lvl,lvl+6)),100)",true);
   Stat(d,"Maximum demons: {value}","1+(blvl("+Id("Demonic Mastery")+")>=5?1:0)+(blvl("+Id("Demonic Mastery")+")>=10?1:0)",true);
   d.Notes.Add("A second demon requires 5 invested points; a third requires 10. Equipment levels do not count toward these thresholds.");
   d.Notes.Add("The Tainted uses the damage bonus as fire mastery; Defiler damage instead uses invested Mastery points.");
  }else if(className=="Warlock"&&(skill.Name=="Summon Tainted"||skill.Name=="Summon Defiler")){
   bool tainted=skill.Name=="Summon Tainted";
   string oath="slvl("+Id("Blood Oath")+")", mastery="slvl("+Id("Demonic Mastery")+")";
   Stat(d,"Mana Cost: {value}",(tainted?"25":"35")+"+(lvl-1)/2");
   Stat(d,"Defense bonus: +{value}",(tainted?"75":"125")+"+20*(lvl-1)",true);
   Stat(d,"Summon life bonus: +{value}%","35*(lvl-1)+( "+oath+">0 ? 50+35*("+oath+"-1) : 0)",true);
   if(tainted){
    Stat(d,"Fire mastery bonus: +{value}%","10*"+mastery,true);
    Stat(d,"Attack speed bonus: +{value}%","min(9+lvl,25)+("+mastery+">0?min(4+"+mastery+",25):0)",true);
    Stat(d,"Blood Boil cast level: {value}","blvl("+Id("Blood Boil")+")",true);
    d.Notes.Add("The Tainted's Blood Boil level uses invested points. Fire Ball damage and Resist Fire are not included yet.");
   }else{
    Stat(d,"Damage bonus: +{value}%","10*((lvl-1)+blvl("+Id("Demonic Mastery")+"))",true);
    Stat(d,"Attack speed bonus: +{value}%",mastery+">0?min(4+"+mastery+",25):0",true);
    d.Notes.Add("Defiler damage counts invested Demonic Mastery points. Health Link effects are not included yet.");
   }
   d.Notes.Add("Life is a bonus, not total life; Blood Oath contributes when learned. Defense is a skill bonus, not total defense.");
  }else if(className=="Warlock"&&skill.Name=="Death Mark"){
   // https://d2r.org/data/skills/death-mark (build 3.3.93847)
   Stat(d,"Mana Cost: {value}","2+lvl/4");
   Stat(d,"Duration: {value} seconds","(125+13*(lvl-1))/25");
   Stat(d,"Target defense: -{value}","50+35*(lvl-1)",true);
   Stat(d,"Physical damage taken per hit: +{value}","5+2*(lvl-1)",true);
   Stat(d,"Magic damage taken per hit: +{value}","5+2*(lvl-1)",true);
   d.Notes.Add("Requires an active demon. Casting delay: 0.8 seconds.");
  }else if(className=="Warlock"&&skill.Name=="Blood Oath"){
   // https://d2r.org/mechanics/blood-oath (build 3.3.93847)
   Stat(d,"Damage shared with demon: {value}%","-1+idiv(31*min(100,idiv(110*lvl,lvl+6)),100)",true);
   Stat(d,"Demon resist all: +{value}%","3+idiv(76*min(100,idiv(110*lvl,lvl+6)),100)",true);
   Stat(d,"Demon life bonus: +{value}%","50+35*(lvl-1)",true);
   Stat(d,"Demon physical damage reduction: {value}%","1+idiv(11*min(100,idiv(110*lvl,lvl+6)),100)",true);
  }else if(className=="Warlock"&&skill.Name=="Ring of Fire"){
   string syn="(100+10*(blvl("+Id("Flame Wave")+")+blvl("+Id("Apocalypse")+")))";
   string min="6+6*(min(lvl,8)-1)+8*max(min(lvl,16)-8,0)+10*max(min(lvl,22)-16,0)+11*max(min(lvl,28)-22,0)+12*max(lvl-28,0)";
   string max="10+7*(min(lvl,8)-1)+9*max(min(lvl,16)-8,0)+11*max(min(lvl,22)-16,0)+13*max(min(lvl,28)-22,0)+15*max(lvl-28,0)";
   Stat(d,"Mana Cost: {value}","8+(lvl-1)/2");
   Stat(d,"Fire Damage: {value}","idiv(("+min+")*"+syn+",100)",true,"idiv(("+max+")*"+syn+",100)");
   d.Notes.Add("Flame Wave and Apocalypse each add 10% fire damage per invested point.");
  }else if(className=="Warlock"&&skill.Name=="Flame Wave"){
   // Current build 3.3.93847: https://d2r.org/mechanics/flame-wave
   // Older patch-note values differ; this uses the referenced game-table progression.
   string syn="(100+15*(blvl("+Id("Ring of Fire")+")+blvl("+Id("Apocalypse")+")))";
   string min="13+8*(min(lvl,8)-1)+12*max(min(lvl,16)-8,0)+17*max(min(lvl,22)-16,0)+18*max(min(lvl,28)-22,0)+21*max(lvl-28,0)";
   string max="17+9*(min(lvl,8)-1)+14*max(min(lvl,16)-8,0)+18*max(min(lvl,22)-16,0)+20*max(min(lvl,28)-22,0)+23*max(lvl-28,0)";
   Stat(d,"Mana Cost: {value}","7+(lvl-1)/2");
   Stat(d,"Fire Damage: {value}","idiv(("+min+")*"+syn+",100)",true,"idiv(("+max+")*"+syn+",100)");
   d.Notes.Add("Ring of Fire and Apocalypse each add 15% fire damage per invested point.");
   d.Notes.Add("Burning ground damage and duration are not included yet.");
  }else if(className=="Warlock"&&skill.Name=="Apocalypse"){
   // Current build 3.3.93847: https://d2r.org/mechanics/apocalypse
   string syn="(100+10*(blvl("+Id("Ring of Fire")+")+blvl("+Id("Flame Wave")+")))";
   string min="80+23*(min(lvl,8)-1)+46*max(min(lvl,16)-8,0)+69*max(min(lvl,22)-16,0)+75*max(min(lvl,28)-22,0)+95*max(lvl-28,0)";
   string max="100+25*(min(lvl,8)-1)+50*max(min(lvl,16)-8,0)+75*max(min(lvl,22)-16,0)+95*max(min(lvl,28)-22,0)+115*max(lvl-28,0)";
   Stat(d,"Mana Cost: {value}","31+lvl",true);
   Stat(d,"Radius: {value} yards","lvl>=20?17:(lvl>=10?15:13)",true);
   Stat(d,"Enemy fire resistance for Apocalypse: -{value}%","min(lvl+4,40)",true);
   Stat(d,"Fire Damage: {value}","idiv(("+min+")*"+syn+",100)",true,"idiv(("+max+")*"+syn+",100)");
   d.Notes.Add("Ring of Fire and Flame Wave each add 10% fire damage per invested point.");
   d.Notes.Add("Casting delay: 1.8 seconds.");
  }else if(className=="Amazon"&&skill.Name=="Cold Arrow"){
   Stat(d,"Cold Length: {value} sec","(100+30*(lvl-1))/25");
   d.Notes.Add("Cold duration can differ by difficulty and target.");
  }else if(className=="Sorceress"&&skill.Name=="Ice Bolt"){
   Stat(d,"Cold Length: {value} sec","(150+35*(lvl-1))/25");
   d.Notes.Add("Cold duration can differ by difficulty and target.");
  }else if(className=="Sorceress"&&skill.Name=="Frost Nova"){
   Stat(d,"Cold Length: {value} sec","(200+25*(lvl-1))/25");
   d.Notes.Add("Cold duration can differ by difficulty and target.");
  }else if(className=="Barbarian"&&skill.Name=="Stun"){
   string warcry="blvl("+Id("War Cry")+")";
   Stat(d,"Stun Length: {value} sec","min(250,idiv((30+5*(min(lvl,16)-1)+2*max(lvl-16,0))*(100+5*"+warcry+"),100))/25");
   d.Notes.Add("War Cry adds 5% stun length per invested point, up to 10 seconds.");
  }else hasDetails=false;
  d.Notes.Add(hasDetails?"Preview: additional effects are still being added.":"Detailed effects are coming in a future update.");
  SkillLevels levels;
  if(className=="Warlock"){
   var bonuses=GearSkillBonuses.ForBuild(build,skills);levels=GearSkillBonuses.LevelsForBuild(build,skills);
   d.Notes.Add("Equipment bonuses are entered manually; only inventory items marked as active charms count.");
   if(bonuses.InvalidItems>0)d.Notes.Add("Some item bonuses could not be read. Check your equipment entries.");
  }else{levels=new SkillLevels{CharacterLevel=build.Level};for(int i=0;i<skills.Count&&i<build.Points.Length;i++)levels.SetHard(i+1,build.Points[i]);d.Notes.Add("Equipment bonuses are not included yet.");}
  return SkillTooltipRenderer.Render(d,levels,null);
 }
}
