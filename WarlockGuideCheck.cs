using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Xml.Linq;
using System.Xml.Serialization;

class WarlockGuideCheck {
 static BindingFlags flags=BindingFlags.NonPublic|BindingFlags.Instance;
 static object Field(Atlas a,string n){return typeof(Atlas).GetField(n,flags).GetValue(a);}
 static void Set(Atlas a,string n,object v){typeof(Atlas).GetField(n,flags).SetValue(a,v);}
 static object Call(Atlas a,string n,params object[] args){return typeof(Atlas).GetMethod(n,flags).Invoke(a,args);}
 static void Need(bool ok,string why){if(!ok)throw new Exception(why);}
 static void CheckSpeedBonuses(){
  var b=new Build();b.Gear[0]="Weapon";b.GearSkillBonuses[0]=new ItemSkillBonuses{Fcr=35,Fhr=20,Lines="Warlock Skills: +2",OtherStats=new List<ItemStat>{new ItemStat{Name="Increased Attack Speed (%)",Value=20}}};
  b.Gear[1]="Shield";b.GearSkillBonuses[1]=new ItemSkillBonuses{Fcr=20,Fbr=15};
  b.GearSkillBonuses[2]=new ItemSkillBonuses{Fcr=99}; // Empty slots never count.
  b.Inventory[0]="Charm";b.InventorySkillBonuses[0]=new ItemSkillBonuses{Fhr=5,ActiveCharm=true};
  b.Inventory[1]="Stashed item";b.InventorySkillBonuses[1]=new ItemSkillBonuses{Fcr=90,Fhr=90,Fbr=90};
  var v=GearSpeedBonuses.ForBuild(b);Need(v.Fcr==55&&v.Fhr==25&&v.Fbr==15,"Equipment or charm totals");
  b.InventorySkillBonuses[0].ActiveCharm=false;v=GearSpeedBonuses.ForBuild(b);Need(v.Fhr==20,"Inactive charm counted");
  var serializer=new XmlSerializer(typeof(Build));string xml;using(var writer=new StringWriter()){serializer.Serialize(writer,b);xml=writer.ToString();}
  var doc=XDocument.Parse(xml);foreach(var name in new[]{"Fcr","Fhr","Fbr","OtherStats"})foreach(var element in doc.Descendants(name).ToArray())element.Remove();
  Build old;using(var reader=doc.CreateReader())old=(Build)serializer.Deserialize(reader);Need(old.GearSkillBonuses[0].Lines=="Warlock Skills: +2"&&GearSpeedBonuses.ForBuild(old).Fcr==0,"Older saved item migration");
  using(var reader=new StringReader(xml))old=(Build)serializer.Deserialize(reader);Need(old.GearSkillBonuses[0].Fcr==35&&old.GearSkillBonuses[1].Fbr==15&&old.GearSkillBonuses[0].OtherStats.Single().Value==20,"Speed bonuses must survive reload");
 }
 static void CheckBreakpoints(){
  foreach(string cls in new[]{"Warlock","Amazon","Assassin","Barbarian","Druid","Necromancer","Paladin","Sorceress"})foreach(string kind in new[]{"FCR","FHR","FBR"}){
   var table=Breakpoints.For(cls,kind);Need(table.Thresholds[0]==0,"Breakpoint zero "+cls+kind);
   for(int i=1;i<table.Thresholds.Length;i++){int threshold=table.Thresholds[i];Need(threshold>table.Thresholds[i-1],"Ordered "+cls+kind);Need(table.Describe(threshold-1).Contains("need +1%"),"Before threshold "+cls+kind+threshold);Need(table.Describe(threshold).Contains("Reached: "+threshold+"%"),"At threshold "+cls+kind+threshold);}
   Need(table.Describe(9999).Contains("Highest listed"),"Maximum "+cls+kind);
  }
  Need(Breakpoints.For("Warlock","FCR").Describe(25).Contains("Reached: 18%")&&Breakpoints.For("Warlock","FCR").Describe(25).Contains("need +5%"),"Screenshot FCR breakpoint");
  Need(EquipmentStatOptions.ForSlot("Weapon").Contains("Increased Attack Speed (%)")&&EquipmentStatOptions.ForSlot("Weapon").Contains("Enhanced Weapon Damage (%)")&&!EquipmentStatOptions.ForSlot("Belt").Contains("Enhanced Weapon Damage (%)"),"Slot-specific stat choices");
  Need(Breakpoints.For("Warlock","FCR").Describe(74).Contains("11 frames")&&Breakpoints.For("Warlock","FCR").Describe(75).Contains("10 frames"),"Warlock FCR");
  Need(Breakpoints.For("Warlock","FHR").Describe(86).Contains("6 frames")&&Breakpoints.For("Sorceress","FCR").Describe(105).Contains("8 frames"),"Class-specific frames");
  Need(CuratedBuilds.Warlock.Length>=3&&CuratedBuilds.Warlock.All(x=>x.Details.Contains("REFERENCE")&&x.Details.Contains("SKILL PRIORITIES")),"Curated builds");
 }
 static void CheckDemonTooltips(List<Skill> skills){
  var b=new Build{Level=99};b.Gear[0]="Test weapon";b.GearSkillBonuses[0]=new ItemSkillBonuses{Lines="Warlock Skills: +10"};
  int goat=skills.FindIndex(s=>s.Name=="Summon Goatman"),master=skills.FindIndex(s=>s.Name=="Demonic Mastery"),oath=skills.FindIndex(s=>s.Name=="Blood Oath"),mark=skills.FindIndex(s=>s.Name=="Death Mark"),tainted=skills.FindIndex(s=>s.Name=="Summon Tainted"),defiler=skills.FindIndex(s=>s.Name=="Summon Defiler");
  var tips=new PreviewTooltipSession("Warlock",skills);
  b.Points[master]=4;
  string t=tips.Render(skills[master],b);
  Need(t.Contains("Effective skill level: 14")&&t.Contains("Demon damage bonus: +140%")&&t.Contains("Maximum demons: 1")&&t.Contains("Maximum demons: 2"),"Mastery hard point threshold or preview");
  b.Points[master]=9;t=tips.Render(skills[master],b);Need(t.Contains("Maximum demons: 2")&&t.Contains("Maximum demons: 3"),"Mastery second threshold");
  b.Points[master]=20;b.Points[oath]=10;b.Points[goat]=20;b.Points[mark]=3;b.Points[tainted]=20;b.Points[defiler]=20;
  t=tips.Render(skills[goat],b);Need(t.Contains("Effective skill level: 30")&&t.Contains("Summon life bonus: 1730%")&&t.Contains("Damage bonus: +600%")&&t.Contains("Attack rating bonus: +2050")&&t.Contains("Crushing Blow: 8%"),"Goatman gear or synergies");
  t=tips.Render(skills[oath],b);Need(t.Contains("Damage shared with demon: 25%")&&t.Contains("Demon resist all: +66%")&&t.Contains("Demon life bonus: +715%")&&t.Contains("Demon physical damage reduction: 10%"),"Blood Oath level 20");
  t=tips.Render(skills[tainted],b);Need(t.Contains("Defense bonus: +655")&&t.Contains("Fire mastery bonus: +300%"),"Tainted gear effects");
  t=tips.Render(skills[defiler],b);Need(t.Contains("Defense bonus: +705")&&t.Contains("Damage bonus: +490%"),"Defiler mastery must use invested points");
  foreach(string name in new[]{"Summon Goatman","Demonic Mastery","Blood Oath","Summon Tainted","Summon Defiler"})Need(!tips.Render(skills.First(s=>s.Name==name),b).Contains("unavailable"),name+" formula failed");
  var levels=new SkillLevels();levels.SetHard(1,1);
  int[] speed={10,14,17,19,22,23,25,26,27,28,29,29,30,31,31,32,32,32,33,33,33,34,34,34,34,35,35,35,35,35};
  int[] resist={14,23,30,36,41,44,47,50,53,54,56,58,60,61,62,63,64,65,66,66,67,68,69,69,69,70,71,71,72,72};
  for(int i=1;i<=30;i++){levels.SetSkillBonus(1,i-1);Need(SkillFormula.Evaluate("5+idiv(34*min(100,idiv(110*lvl,lvl+6)),100)",1,levels,null)==speed[i-1],"Mastery speed level "+i);Need(SkillFormula.Evaluate("3+idiv(76*min(100,idiv(110*lvl,lvl+6)),100)",1,levels,null)==resist[i-1],"Oath resistance level "+i);}
 }
 [STAThread] static int Main(){try{Run();return 0;}catch(Exception ex){Console.Error.WriteLine(ex);return 1;}}
 static void Run(){
  string dir=Path.Combine(Environment.CurrentDirectory,"test-builds",Guid.NewGuid().ToString("N"));Directory.CreateDirectory(dir);
  try{
   var old=new Build{Level=60,Quests=4,Notes="Old journal"};old.Points[0]=1;old.Gear[0]="Old weapon";old.Inventory[0]="Old charm";old.Runes[1]=3;
   string path=Path.Combine(dir,"build.xml");using(var fs=File.Create(path))new XmlSerializer(typeof(Build)).Serialize(fs,old);
   var xml=XDocument.Load(path);var gearNode=xml.Root.Element("GearSkillBonuses");var inventoryNode=xml.Root.Element("InventorySkillBonuses");if(gearNode!=null)gearNode.Remove();if(inventoryNode!=null)inventoryNode.Remove();xml.Save(path);
   using(var app=new Atlas(dir)){
    Set(app,"loading",true); // UI exercises must never open a save-error dialog.
    var build=(Build)Field(app,"build");var skills=(List<Skill>)Field(app,"skills");CheckDemonTooltips(skills);CheckBreakpoints();CheckSpeedBonuses();
    Need(build.Notes=="Old journal"&&build.Gear[0]=="Old weapon"&&build.Inventory[0]=="Old charm"&&build.Runes[1]==3&&build.Points[0]==1,"Old save migration");
    Need(build.GearSkillBonuses.Length==10&&build.InventorySkillBonuses.Length==40,"New fields not initialized");
    build.GearSkillBonuses[0]=new ItemSkillBonuses{Lines="All Skills: +2\nWarlock Skills: +1\nChaos Skills: +3\nRing of Fire: +2\nFire Skills: +2"};
    build.InventorySkillBonuses[0]=new ItemSkillBonuses{Lines="All Skills: +1",ActiveCharm=true};
    build.Inventory[1]="Stashed charm";build.InventorySkillBonuses[1]=new ItemSkillBonuses{Lines="All Skills: +100"};
    var totals=GearSkillBonuses.ForBuild(build,skills);var levels=new SkillLevels();GearSkillBonuses.Apply(totals,skills,levels);
    int ring=skills.FindIndex(s=>s.Name=="Ring of Fire")+1,flame=skills.FindIndex(s=>s.Name=="Flame Wave")+1;
    Need(levels.Effective(ring)==11,"Granted skill and active equipment calculation");Need(levels.Effective(flame)==0,"Fire bonuses granted an unlearned skill");
    var tip=new PreviewTooltipSession("Warlock",skills).Render(skills[ring-1],build);
    Need(tip.Contains("Invested points: 0 / 20")&&tip.Contains("Effective skill level: 11")&&tip.Contains("Current values:"),"Granted skill tooltip");
    Need(GearSkillBonuses.Parse("Hex: Bane: +3",skills).Individual["Hex: Bane"]==3,"Colon in skill name");
    Need(GearSkillBonuses.Parse("+10 to Warlock Skill Levels",skills).Class==10,"In-game bonus wording");
    Need(GearSkillBonuses.Parse("Warlock skills +10",skills).Class==10,"Alternative bonus wording");
    int mark=skills.FindIndex(s=>s.Name=="Death Mark");build.Points[mark]=1;
    var markTip=new PreviewTooltipSession("Warlock",skills).Render(skills[mark],build);
    Need(markTip.Contains("Effective skill level: 5")&&markTip.Contains("Duration: 7.08 seconds")&&markTip.Contains("Target defense: -190"),"Death Mark scaling with gear");
    bool rejected=false;try{GearSkillBonuses.Parse("All Skills: -1",skills);}catch(FormatException){rejected=true;}Need(rejected,"Negative bonus accepted");
    int wave=skills.FindIndex(s=>s.Name=="Flame Wave");build.Points[wave]=20;build.GearSkillBonuses[0].Lines+="\nFlame Wave: +50";
    tip=new PreviewTooltipSession("Warlock",skills).Render(skills[ring-1],build);Need(tip.Contains("Fire Damage: 216–258"),"Synergy should use invested points only");
    build.GearSkillBonuses[0].Fcr=35;Call(app,"SelectSlot",false,0);Set(app,"moving",true);Call(app,"SelectSlot",true,2);
    Need(build.Inventory[2]=="Old weapon"&&build.InventorySkillBonuses[2]!=null&&build.InventorySkillBonuses[2].Fcr==35&&build.Gear[0]==null&&GearSpeedBonuses.ForBuild(build).Fcr==0,"Moved item lost bonuses or counted in stash");
    Call(app,"PutSlot",true,2,"");Need(build.InventorySkillBonuses[2]==null,"Clearing slot retained bonuses");
    build.Gear[0]="Test weapon";build.GearSkillBonuses[0]=new ItemSkillBonuses{Lines="Warlock Skills: +10",Fcr=35,Fhr=20};
    build.Gear[1]="Test shield";build.GearSkillBonuses[1]=new ItemSkillBonuses{Fcr=20,Fbr=15};
    build.InventorySkillBonuses[0].ActiveCharm=false;build.Points=new int[30];build.Points[0]=1;
    Call(app,"ShowSkills");var skillList=(SkillTreePanel)Field(app,"skillList");
    var goatCard=skillList.Controls.OfType<GothicPanel>().First(c=>c.Controls.OfType<Label>().Any(l=>l.Text=="Summon Goatman"));
    Need(goatCard.Controls.OfType<Label>().Any(l=>l.Text=="1/20 +10"),"Weapon bonus not visible on skill card");
    Need(new PreviewTooltipSession("Warlock",skills).Render(skills[0],build).Contains("Effective skill level: 11"),"Weapon bonus not applied in tooltip");
    Call(app,"ShowGear");var gearContent=(Panel)Field(app,"content");Need(gearContent.Controls.OfType<Label>().Any(l=>l.Text.Contains("+10 Warlock Skills")),"Active bonus summary missing");
    Need(gearContent.Controls.OfType<Button>().Any(x=>x.Text=="UNIQUE PRESETS"),"Unique presets button missing");
    var preset=VerifiedEquipment.Bonuses(VerifiedEquipment.Find("Harlequin Crest"));
    Need((bool)Call(app,"VerifiedFits",preset,false,2)&&!(bool)Call(app,"VerifiedFits",preset,false,0)&&(bool)Call(app,"VerifiedFits",preset,true,0),"Verified slot and inventory rules");
    Need(gearContent.Controls.OfType<Button>().Any(x=>x.Text=="BREAKPOINTS"),"Equipment breakpoint button");
    Call(app,"ShowBreakpoints");var breakpointContent=(Panel)Field(app,"content");var fcr=breakpointContent.Controls.OfType<NumericUpDown>().First(x=>x.AccessibleName=="FCR extra percentage");
    var targetLabels=breakpointContent.Controls.OfType<Label>().Where(x=>x.Text.StartsWith("Targets:")).OrderBy(x=>x.Top).ToArray();var resultLabels=breakpointContent.Controls.OfType<Label>().Where(x=>x.AccessibleName!=null&&x.AccessibleName.EndsWith("breakpoint result")).OrderBy(x=>x.Top).ToArray();Need(targetLabels.Length==3&&resultLabels.Length==3,"Breakpoint rows");for(int i=0;i<3;i++)Need(!targetLabels[i].Bounds.IntersectsWith(resultLabels[i].Bounds),"Breakpoint targets overlap result");
    Need(breakpointContent.Controls.OfType<Label>().Any(x=>x.AccessibleName=="FCR breakpoint result"&&x.Text.Contains("Total: 55%")&&x.Text.Contains("Next: 75%")),"Equipment breakpoint total");fcr.Value=20;
    Need(breakpointContent.Controls.OfType<Label>().Any(x=>x.AccessibleName=="FCR breakpoint result"&&x.Text.Contains("Total: 75%")&&x.Text.Contains("10 frames")),"Additional bonus breakpoint result");
    Call(app,"ShowGear");Call(app,"ShowBreakpoints");breakpointContent=(Panel)Field(app,"content");Need(breakpointContent.Controls.OfType<NumericUpDown>().First(x=>x.AccessibleName=="FCR extra percentage").Value==20,"Extra lost while navigating");
    Call(app,"PutSlot",false,1,"");Call(app,"ShowBreakpoints");breakpointContent=(Panel)Field(app,"content");Need(breakpointContent.Controls.OfType<Label>().Any(x=>x.AccessibleName=="FCR breakpoint result"&&x.Text.Contains("Total: 55%")),"Cleared item still counted");
    Call(app,"ShowCuratedBuilds");var curated=(Panel)Field(app,"content");var buildList=curated.Controls.OfType<ListBox>().Single();var buildDetail=curated.Controls.OfType<TextBox>().Single();Need(buildList.Items.Count==3&&buildDetail.Text.Contains("DEMON SUMMONER"),"Build browser");buildList.SelectedIndex=1;Need(buildDetail.Text.Contains("FIRE CASTER")&&build.Points[0]==1,"Build selection changed points");
    Call(app,"ShowGuide");Need(GuideBook.Regions.Select(x=>x.Act).Distinct().Count()==5&&GuideBook.Regions.Length>=25,"Guide does not cover all acts and regions");
    Need(GuideBook.Regions.First(x=>x.Name=="Black Marsh & Forgotten Tower").Details().Contains("Key of Terror"),"In-depth guide information missing");
    var content=(Panel)Field(app,"content");var list=content.Controls.OfType<ListBox>().Single();Need(list.Items.Count>0,"Guide region list empty");
    var details=content.Controls.OfType<TextBox>().Single();Need(details.Text.Contains("QUESTS")&&details.Text.Contains("FARMING TIPS"),"Guide region details missing");
    Call(app,"ShowGuideNotes");content=(Panel)Field(app,"content");Need(content.Controls.OfType<TextBox>().Single().Text=="Old journal","Saved notes not available in guide");
    build.Gear[7]="Test amulet";build.GearSkillBonuses[7]=new ItemSkillBonuses{Lines="All Skills: +2",Fcr=10};
    using(var fs=File.Create(path))new XmlSerializer(typeof(Build)).Serialize(fs,build);
   }
   using(var reloaded=new Atlas(dir)){var build=(Build)Field(reloaded,"build");Need(build.Notes=="Old journal"&&!build.InventorySkillBonuses[0].ActiveCharm,"Save and reload");Need(build.GearSkillBonuses[7]!=null&&build.GearSkillBonuses[7].Lines=="All Skills: +2"&&build.GearSkillBonuses[7].Fcr==10,"A non-first equipment slot did not survive reload");Need(GearSpeedBonuses.ForBuild(build).Fcr==45,"Reloaded totals");Set(reloaded,"loading",true);Call(reloaded,"SwitchClass","Amazon");build=(Build)Field(reloaded,"build");build.Gear[0]="Amazon weapon";build.GearSkillBonuses[0]=new ItemSkillBonuses{Fcr=99};Call(reloaded,"ShowBreakpoints");var pane=(Panel)Field(reloaded,"content");Need(pane.Controls.OfType<Label>().Any(x=>x.AccessibleName=="FCR breakpoint result"&&x.Text.Contains("Total: 99%")&&x.Text.Contains("12 frames")),"Non-Warlock equipment breakpoint");}
   Console.WriteLine("Warlock gear, save migration, guide and journal checks passed.");
  }finally{Directory.Delete(dir,true);}
 }
}
