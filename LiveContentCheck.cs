using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using System.Xml.Linq;
using System.Xml.Serialization;

class LiveContentCheck {
 static BindingFlags flags=BindingFlags.NonPublic|BindingFlags.Instance;
 static object Field(Atlas a,string name){return typeof(Atlas).GetField(name,flags).GetValue(a);}
 static object Call(Atlas a,string name,params object[] values){return typeof(Atlas).GetMethod(name,flags).Invoke(a,values);}
 static void Need(bool condition,string why){if(!condition)throw new Exception(why);}
 static byte[] Bytes(XDocument doc){using(var ms=new MemoryStream()){doc.Save(ms);return ms.ToArray();}}
 [STAThread] static int Main(){try{Run();Console.WriteLine("Live content refresh, persistence, rollback and legacy compatibility passed.");return 0;}catch(Exception e){Console.Error.WriteLine(e);return 1;}}
 static void Run(){
  string dir=Path.Combine(Environment.CurrentDirectory,"test-builds",Guid.NewGuid().ToString("N"));Directory.CreateDirectory(dir);
  try{
   var b=new Build{Notes="Keep my journal",Level=35};b.Points[0]=1;b.Gear[0]="Keep my weapon";
   using(var stream=File.Create(Path.Combine(dir,"build.xml")))new XmlSerializer(typeof(Build)).Serialize(stream,b);
   var legacy=File.ReadAllBytes("runeword-effects.xml");var package=File.ReadAllBytes("SanctuaryAtlasContent.xml");
   Need(GuideContent.Parse(legacy).Count==GuideBook.Regions.Length,"Legacy package fallback");
   string hash=new string('a',64);string contentOnly="{\"tag_name\":\"v38\",\"assets\":[{\"name\":\"SanctuaryAtlasContent.xml\",\"browser_download_url\":\"https://github.com/TotallyAFlareon/Sanctuary_Atlas/releases/download/v38/SanctuaryAtlasContent.xml\",\"digest\":\"sha256:"+hash+"\",\"size\":1}]}";var contentRelease=AtlasUpdates.ParseRelease(contentOnly,"TotallyAFlareon/Sanctuary_Atlas");Need(contentRelease.Url==null&&contentRelease.ContentUrl!=null&&!AtlasUpdates.IsProgramUpdate(contentRelease),"Content-only release requested a restart");
   string program="{\"tag_name\":\"v38\",\"assets\":[{\"name\":\"SanctuaryAtlasSetup.exe\",\"browser_download_url\":\"https://github.com/TotallyAFlareon/Sanctuary_Atlas/releases/download/v38/SanctuaryAtlasSetup.exe\",\"digest\":\"sha256:"+hash+"\",\"size\":1}]}";Need(AtlasUpdates.IsProgramUpdate(AtlasUpdates.ParseRelease(program,"TotallyAFlareon/Sanctuary_Atlas")),"Program release did not request a restart");
   using(var app=new Atlas(dir)){
    Call(app,"ShowGuide");var panel=(Panel)Field(app,"content");var list=panel.Controls.OfType<ListBox>().Single();var details=panel.Controls.OfType<TextBox>().Single();
    list.SelectedIndex=1;string selected=((GuideRegion)list.SelectedItem).Name;string oldText=details.Text;
    var document=XDocument.Parse(Encoding.UTF8.GetString(package));var guide=document.Root.Element("guidebook");
    guide.Elements("region").First(x=>(string)x.Attribute("name")==selected).Element("details").Value="UPDATED GUIDE DETAILS";
    guide.Add(new XElement("region",new XAttribute("act",1),new XAttribute("name","New test region"),new XElement("details","NEW REGION DETAILS")));
    byte[] update=Bytes(document);Call(app,"ApplyWordContent",update);
    Need(Object.ReferenceEquals(list,((Panel)Field(app,"content")).Controls.OfType<ListBox>().Single()),"Window was rebuilt");
    Need(((GuideRegion)list.SelectedItem).Name==selected&&details.Text=="UPDATED GUIDE DETAILS"&&details.Text!=oldText,"Selected guide did not refresh");
    Need(list.Items.Cast<GuideRegion>().Any(x=>x.Name=="New test region"),"New region not visible");
    Call(app,"ShowSkills");var skillDocument=new XDocument(document);var goatman=skillDocument.Root.Element("skilltrees").Elements("class").Single(x=>(string)x.Attribute("name")=="Warlock").Elements("skill").Single(x=>(string)x.Attribute("name")=="Summon Goatman");goatman.Element("description").Value="LIVE SKILL DESCRIPTION";byte[] skillUpdate=Bytes(skillDocument);Call(app,"ApplyWordContent",skillUpdate);
    var liveSkills=(System.Collections.Generic.List<Skill>)Field(app,"skills");Need(liveSkills.Single(x=>x.Name=="Summon Goatman").Desc=="LIVE SKILL DESCRIPTION"&&!app.IsDisposed,"Skill tree did not refresh in place");
    Call(app,"SwitchClass","Amazon");var amazonSkills=(System.Collections.Generic.List<Skill>)Field(app,"skills");Need(amazonSkills.Single(x=>x.Name=="Magic Arrow").Desc=="Active skill.","Skill content did not apply after changing class");Call(app,"SwitchClass","Warlock");liveSkills=(System.Collections.Generic.List<Skill>)Field(app,"skills");Need(liveSkills.Single(x=>x.Name=="Summon Goatman").Desc=="LIVE SKILL DESCRIPTION","Updated skill content was lost after changing class");
    string path=(string)Call(app,"ContentPath");var saved=File.ReadAllBytes(path);Need(saved.SequenceEqual(skillUpdate),"Content not persisted");
    var duplicate=new XDocument(document);duplicate.Root.Element("guidebook").Add(new XElement(duplicate.Root.Element("guidebook").Elements("region").First()));
    bool rejected=false;try{Call(app,"ApplyWordContent",Bytes(duplicate));}catch(TargetInvocationException ex){rejected=ex.InnerException is InvalidOperationException;}
    Need(rejected&&File.ReadAllBytes(path).SequenceEqual(saved)&&liveSkills.Single(x=>x.Name=="Summon Goatman").Desc=="LIVE SKILL DESCRIPTION","Invalid update changed active or saved content");
    var live=(Build)Field(app,"build");Need(live.Points[0]==1&&live.Gear[0]=="Keep my weapon"&&live.Notes=="Keep my journal","Build changed on content update");
   }
   using(var app=new Atlas(dir)){Call(app,"ShowGuide");var list=((Panel)Field(app,"content")).Controls.OfType<ListBox>().Single();var details=((Panel)Field(app,"content")).Controls.OfType<TextBox>().Single();int index=-1;for(int i=0;i<list.Items.Count;i++)if(((GuideRegion)list.Items[i]).Name=="New test region")index=i;Need(index>=0,"Saved region not reloaded");list.SelectedIndex=index;Need(details.Text=="NEW REGION DETAILS","Saved guide details not reloaded");}
  }finally{Directory.Delete(dir,true);}
 }
}

