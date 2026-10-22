using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using System.Windows.Forms;
class DemonDetailsCheck {
 static BindingFlags flags=BindingFlags.Instance|BindingFlags.NonPublic;
 static void Need(bool ok,string why){if(!ok)throw new Exception(why);}
 static string Current(string s){int i=s.IndexOf("\nNext point:");return i<0?s:s.Substring(0,i);}
 static int Index(List<Skill> s,string n){return s.FindIndex(x=>x.Name==n);}
 [STAThread] static int Main(){try{Run();Console.WriteLine("Demon skill rank, gear, synergy, threshold and inline display checks passed.");return 0;}catch(Exception e){Console.Error.WriteLine(e);return 1;}}
 static void Run(){
  string dir=Path.Combine(Environment.CurrentDirectory,"test-builds",Guid.NewGuid().ToString("N"));Directory.CreateDirectory(dir);
  try{using(var app=new Atlas(dir)){
   var skills=(List<Skill>)typeof(Atlas).GetField("skills",flags).GetValue(app);
   var session=new PreviewTooltipSession("Warlock",skills);var build=new Build{Level=99};
   int bb=Index(skills,"Blood Boil"), eng=Index(skills,"Engorge"), con=Index(skills,"Consume"), bind=Index(skills,"Bind Demon"), oath=Index(skills,"Blood Oath"), mastery=Index(skills,"Demonic Mastery");
   Func<int,string> tip=i=>session.Render(skills[i],build);
   string t=tip(bb);Need(t.Contains("First point preview:")&&t.Contains("Fire damage: 10–20")&&t.Contains("Life cost with one demon: 15%"),"Blood Boil first point");
   int[] ranks={1,8,9,16,17,20};string[] damages={"10–20","66–90","76–102","146–186","158–204","194–258"};
   for(int i=0;i<ranks.Length;i++){build.Points[bb]=ranks[i];t=Current(tip(bb));Need(t.Contains("Fire damage: "+damages[i])&&t.Contains("Physical damage: "+damages[i]),"Blood Boil rank "+ranks[i]);}
   build.Points[oath]=20;build.Points[eng]=20;t=Current(tip(bb));Need(t.Contains("Fire damage: 970–1290")&&t.Contains("Physical damage: 970–1290")&&t.Contains("Life cost with one demon: 5%"),"Blood Boil synergies");
   build.Points[oath]=1;build.Points[eng]=1;build.Points[bb]=1;build.Gear[0]="Weapon";build.GearSkillBonuses[0]=new ItemSkillBonuses{Lines="Warlock Skills: +10"};
   t=Current(tip(bb));Need(t.Contains("Effective skill level: 11")&&t.Contains("Fire damage: 115–151")&&t.Contains("Physical damage: 115–151"),"Blood Boil gear or hard synergies");
   t=Current(tip(eng));Need(t.Contains("Duration: 35 seconds")&&t.Contains("Demon life healed: 40%")&&t.Contains("Demon defense from Blood Oath: +25")&&t.Contains("Demon regeneration from Blood Oath: +5"),"Engorge gear or hard synergy");
   build.Points[con]=1;t=Current(tip(con));Need(t.Contains("Duration: 240 seconds")&&t.Contains("Maximum life: +15%")&&t.Contains("If Tainted: fire skill damage +20%")&&t.Contains("If Goatman: damage bonus in Hell +75%"),"Consume bonuses");
   build.Points[bind]=9;build.Points[mastery]=1;t=tip(bind);
   Need(Current(t).Contains("Effective skill level: 19")&&Current(t).Contains("Champion binding unlocked: No")&&t.Contains("Champion binding unlocked: Yes")&&Current(t).Contains("Spectral Hit unlocked: Yes")&&Current(t).Contains("Aura Enchanted unlocked: No"),"Bind hard versus effective thresholds");
   build.Points[bind]=20;t=Current(tip(bind));Need(t.Contains("Super Unique binding unlocked: Yes")&&t.Contains("Aura Enchanted unlocked: Yes")&&t.Contains("Flat demon damage bonus: +775"),"Bind rank 30");
   app.Size=new System.Drawing.Size(1920,1080);app.PerformLayout();typeof(Atlas).GetMethod("LayoutSkills",flags).Invoke(app,null);typeof(Atlas).GetMethod("HoverSkill",flags).Invoke(app,new object[]{bb});
   var view=(SkillDetailView)typeof(Atlas).GetField("inlineDetail",flags).GetValue(app);Need(view.DetailsText.Contains("Blood Boil")&&view.DetailsText.Contains("First point preview:")&&view.DetailsText.Contains("Fire damage: 10–20"),"Inline hover detail");
  }}finally{Directory.Delete(dir,true);}
 }
}

