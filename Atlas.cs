using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Reflection;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Xml.Serialization;

public class GothicButton : Button {
 public bool GearSlot=false; public int GlyphIndex=-1; bool hovering=false,down=false;
 public GothicButton(){SetStyle(ControlStyles.UserPaint|ControlStyles.AllPaintingInWmPaint|ControlStyles.OptimizedDoubleBuffer,true);FlatStyle=FlatStyle.Flat;}
 protected override void OnMouseEnter(EventArgs e){hovering=true;Invalidate();base.OnMouseEnter(e);} protected override void OnMouseLeave(EventArgs e){hovering=false;down=false;Invalidate();base.OnMouseLeave(e);} protected override void OnMouseDown(MouseEventArgs e){down=true;Invalidate();base.OnMouseDown(e);} protected override void OnMouseUp(MouseEventArgs e){down=false;Invalidate();base.OnMouseUp(e);}
 protected override void OnPaint(PaintEventArgs e){var g=e.Graphics;g.SmoothingMode=SmoothingMode.AntiAlias;var r=ClientRectangle;if(r.Width<3||r.Height<3)return;bool active=BackColor.R>70;Color top=active?Color.FromArgb(100,66,38):Color.FromArgb(47,37,31), bottom=active?Color.FromArgb(54,31,20):Color.FromArgb(19,16,15);if(hovering&&Enabled){top=Color.FromArgb(119,78,42);bottom=Color.FromArgb(55,31,22);}if(down){var t=top;top=bottom;bottom=t;}using(var brush=new LinearGradientBrush(r,top,bottom,90f))g.FillRectangle(brush,r);using(var p=new Pen(active||hovering?Color.FromArgb(194,151,88):Color.FromArgb(116,88,59)))g.DrawRectangle(p,0,0,r.Width-1,r.Height-1);using(var p=new Pen(Color.FromArgb(43,30,22)))g.DrawRectangle(p,2,2,r.Width-5,r.Height-5);using(var p=new Pen(Color.FromArgb(132,98,57))) {g.DrawLine(p,4,4,Math.Min(11,r.Width-4),4);g.DrawLine(p,4,4,4,Math.Min(11,r.Height-4));g.DrawLine(p,r.Width-5,r.Height-5,Math.Max(4,r.Width-12),r.Height-5);}
 Rectangle tr=new Rectangle(5,3,r.Width-10,r.Height-6);if(GearSlot && GlyphIndex>=0){DrawGlyph(g,GlyphIndex,r.Width/2,17);tr=new Rectangle(5,23,r.Width-10,r.Height-26);}var flags=TextFormatFlags.WordBreak|TextFormatFlags.VerticalCenter|TextFormatFlags.EndEllipsis;if(TextAlign==ContentAlignment.MiddleLeft)flags|=TextFormatFlags.Left;else flags|=TextFormatFlags.HorizontalCenter;TextRenderer.DrawText(g,Text,Font,tr,Enabled?ForeColor:Color.FromArgb(115,103,91),flags);if(Focused&&ShowFocusCues)ControlPaint.DrawFocusRectangle(g,new Rectangle(4,4,r.Width-8,r.Height-8)); }
 void DrawGlyph(Graphics g,int i,int cx,int cy){using(var p=new Pen(Color.FromArgb(135,122,100,73),1.7f)){if(i==0){g.DrawLine(p,cx-4,cy+7,cx+5,cy-8);g.DrawLine(p,cx-8,cy+2,cx+1,cy+7);g.DrawLine(p,cx-5,cy+8,cx-9,cy+12);}else if(i==1){g.DrawPolygon(p,new Point[]{new Point(cx-8,cy-8),new Point(cx+8,cy-8),new Point(cx+7,cy+2),new Point(cx,cy+10),new Point(cx-7,cy+2)});}else if(i==2){g.DrawArc(p,cx-8,cy-8,16,17,180,180);g.DrawLine(p,cx-8,cy,cx+8,cy);g.DrawLine(p,cx,cy-8,cx,cy+5);}else if(i==3){g.DrawPolygon(p,new Point[]{new Point(cx-8,cy-8),new Point(cx-3,cy-10),new Point(cx,cy-5),new Point(cx+3,cy-10),new Point(cx+8,cy-8),new Point(cx+6,cy+9),new Point(cx-6,cy+9)});}else if(i==5){g.DrawRectangle(p,cx-10,cy-4,20,8);g.DrawRectangle(p,cx-2,cy-3,4,6);}else if(i>=7){g.DrawEllipse(p,cx-5,cy-5,10,10);g.DrawLine(p,cx-3,cy-6,cx,cy-10);g.DrawLine(p,cx,cy-10,cx+3,cy-6);}else {g.DrawPolygon(p,new Point[]{new Point(cx-6,cy-8),new Point(cx+5,cy-8),new Point(cx+5,cy+4),new Point(cx+9,cy+8),new Point(cx-6,cy+8)});}}}
}
public class GothicPanel : Panel { protected override void OnPaint(PaintEventArgs e){base.OnPaint(e);var r=ClientRectangle;if(r.Width<4||r.Height<4)return;using(var p=new Pen(Color.FromArgb(105,78,48)))e.Graphics.DrawRectangle(p,0,0,r.Width-1,r.Height-1);using(var p=new Pen(Color.FromArgb(52,38,27)))e.Graphics.DrawRectangle(p,2,2,r.Width-5,r.Height-5);}}
public class SkillTreePanel : Panel {
 public Control BackgroundOwner;
 readonly VScrollBar bar;
 Bitmap backgroundCache; public int BackgroundCacheBuilds;
 int scrollOffset;
 const int ContentHeight=672;
 public SkillTreePanel(){
  SetStyle(ControlStyles.UserPaint|ControlStyles.AllPaintingInWmPaint|ControlStyles.OptimizedDoubleBuffer|ControlStyles.ResizeRedraw,true);
  bar=new VScrollBar{Dock=DockStyle.Right,SmallChange=40};
  bar.ValueChanged+=(s,e)=>SetOffset(bar.Value);
  Controls.Add(bar);
 }
 public new Point AutoScrollPosition {get{return new Point(0,-scrollOffset);}set{SetOffset(value.Y);}}
 void SetOffset(int value){
  int next=Math.Max(0,Math.Min(value,Math.Max(0,ContentHeight-ClientSize.Height)));
  if(scrollOffset==next && bar.Value==next)return;
  scrollOffset=next;
  if(bar.Value!=next)bar.Value=next;
  SuspendLayout();
  foreach(Control c in Controls)if(c!=bar && c.Tag is Point){Point p=(Point)c.Tag;c.Location=new Point(p.X,p.Y-scrollOffset);if(c is Label)c.Invalidate();}
  ResumeLayout(false);
 }
 public void ClearSkills(){foreach(Control c in Controls.Cast<Control>().Where(c=>c!=bar).ToArray()){Controls.Remove(c);c.Dispose();}}
 public void FinishSkills(int desiredScroll){
  foreach(Control c in Controls)if(c!=bar)c.Tag=c.Location;
  UpdateScrollbar();scrollOffset=0;SetOffset(desiredScroll);bar.BringToFront();
 }
 void UpdateScrollbar(){
  if(bar==null)return;
  int height=Math.Max(1,ClientSize.Height);
  bar.Maximum=ContentHeight-1;bar.LargeChange=Math.Min(ContentHeight,height);
  bar.Visible=height<ContentHeight;
  int max=Math.Max(0,ContentHeight-height);
  if(scrollOffset>max)SetOffset(max);
 }
 public void RefreshBackground(){if(backgroundCache!=null){backgroundCache.Dispose();backgroundCache=null;}Invalidate(true);}
 protected override void OnResize(EventArgs e){base.OnResize(e);UpdateScrollbar();RefreshBackground();}
 protected override void OnLocationChanged(EventArgs e){base.OnLocationChanged(e);RefreshBackground();}
 protected override void OnMouseWheel(MouseEventArgs e){base.OnMouseWheel(e);SetOffset(scrollOffset-e.Delta);}
 protected override void OnPaintBackground(PaintEventArgs e){
  if(backgroundCache==null && ClientSize.Width>0 && ClientSize.Height>0 && BackgroundOwner!=null && BackgroundOwner.BackgroundImage!=null && IsHandleCreated && BackgroundOwner.IsHandleCreated){
   backgroundCache=new Bitmap(ClientSize.Width,ClientSize.Height);BackgroundCacheBuilds++;
   Point offset=BackgroundOwner.PointToClient(PointToScreen(Point.Empty));
   using(Graphics g=Graphics.FromImage(backgroundCache))g.DrawImage(BackgroundOwner.BackgroundImage,new Rectangle(-offset.X,-offset.Y,BackgroundOwner.ClientSize.Width,BackgroundOwner.ClientSize.Height));
  }
  if(backgroundCache!=null)e.Graphics.DrawImageUnscaled(backgroundCache,0,0);
  else e.Graphics.Clear(Color.FromArgb(19,16,15));
 }
 protected override void Dispose(bool disposing){if(disposing && backgroundCache!=null){backgroundCache.Dispose();backgroundCache=null;}base.Dispose(disposing);}
}
[Serializable] public class Build { public int Level=30, Quests=4; public int[] Points=new int[30]; public int[] Runes=new int[33]; public string[] Gear=new string[10]; public string[] Inventory=new string[40]; public ItemSkillBonuses[] GearSkillBonuses=new ItemSkillBonuses[10]; public ItemSkillBonuses[] InventorySkillBonuses=new ItemSkillBonuses[40]; public string Notes=""; }
public class Skill { public string Name, Tree, Requires, Desc; public int Level; public Skill(string t,string n,int l,string r,string d){Tree=t;Name=n;Level=l;Requires=r;Desc=d;} }
public class Word { public string Name, Runes, Base, Note, Effects=""; public Word(string n,string r,string b,string a){Name=n;Runes=r;Base=b;Note=a;} }
public class Cube { public string Name, Formula, Note; public Cube(string n,string f,string d){Name=n;Formula=f;Note=d;} }
public class Atlas : Form {
 static Color bg=Color.FromArgb(19,16,15), panel=Color.FromArgb(35,27,23), gold=Color.FromArgb(210,172,100), text=Color.FromArgb(236,224,199), muted=Color.FromArgb(184,169,146);
 string dataDir=Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),"SanctuaryAtlas"); string save; string className="Warlock"; static string[] classes={"Warlock","Amazon","Assassin","Barbarian","Druid","Necromancer","Paladin","Sorceress"};
 Build build=new Build(); List<Skill> skills=new List<Skill>(); List<Word> words=new List<Word>(); Panel content; Label budget; NumericUpDown level,quests; SkillTreePanel skillList; ComboBox tree; ListBox results; TextBox search,detail; ToolTip slotTip=new ToolTip(); SkillHoverTip skillTip=new SkillHoverTip(); bool loading=false; int selectedSlot=-1; bool selectedInventory=false, moving=false; CheckBox craftable; List<Cube> cubes=new List<Cube>(); static string[] runeNames="El Eld Tir Nef Eth Ith Tal Ral Ort Thul Amn Sol Shael Dol Hel Io Lum Ko Fal Lem Pul Um Mal Ist Gul Vex Ohm Lo Sur Ber Jah Cham Zod".Split(' ');
 static Font f(float s,FontStyle st=FontStyle.Regular){return new Font("Segoe UI",s,st);} 
 public Atlas():this(null){} public Atlas(string testDataDir){ if(!String.IsNullOrEmpty(testDataDir))dataDir=testDataDir; Icon=Icon.ExtractAssociatedIcon(Application.ExecutablePath); Text="Sanctuary Atlas | D2R companion | Update 37"; Size=new Size(1150,790); MinimumSize=new Size(950,680); BackColor=bg; ForeColor=text; Font=f(10); StartPosition=FormStartPosition.CenterScreen; save=Path.Combine(dataDir,"build.xml"); InitData(); LoadBuild(); MakeShell(); ShowSkills(); }
 protected override void Dispose(bool disposing){if(disposing){skillTip.Dispose();slotTip.Dispose();}base.Dispose(disposing);}
 void Add(string t,string n,int l,string r,string d){skills.Add(new Skill(t,n,l,r,d));}
 void InitWarlockSkills(){
 Add("Demon","Summon Goatman",1,"","Summons a melee demon ally."); Add("Demon","Demonic Mastery",1,"Summon Goatman","Strengthens summoned and bound demons."); Add("Demon","Blood Oath",6,"Demonic Mastery","Links your protection to your demon."); Add("Demon","Death Mark",6,"Summon Goatman","Directs your demon to an enemy."); Add("Demon","Summon Tainted",12,"Summon Goatman","Summons a ranged demon ally."); Add("Demon","Blood Boil",18,"Death Mark","Erupts your demon's blood to damage nearby enemies."); Add("Demon","Summon Defiler",18,"Summon Tainted","Summons a demon that binds enemy souls."); Add("Demon","Engorge",24,"Blood Boil","Feeds a corpse to heal and empower your demon."); Add("Demon","Consume",30,"Blood Oath","Sacrifices your demon for power."); Add("Demon","Bind Demon",30,"Engorge|Summon Defiler","Turns an injured demon into an ally.");
 Add("Eldritch","Levitation Mastery",1,"","Improves the use of your levitated weapon."); Add("Eldritch","Hex: Bane",1,"","Hexes your weapon to impair enemies."); Add("Eldritch","Cleave",6,"Levitation Mastery","Attacks enemies in an arc."); Add("Eldritch","Echoing Strike",12,"Levitation Mastery","Creates weapon echoes that pierce enemies."); Add("Eldritch","Hex: Purge",12,"Hex: Bane","Hexes your weapon to make enemies erupt."); Add("Eldritch","Blade Warp",18,"Echoing Strike","Hurls a weapon and teleports you to its impact."); Add("Eldritch","Psychic Ward",18,"Cleave","Creates a protective mental shield."); Add("Eldritch","Eldritch Blast",24,"Psychic Ward","Blasts hexed enemies and refreshes their hex."); Add("Eldritch","Hex: Siphon",24,"Hex: Purge","Hexes your weapon to extract life from slain enemies."); Add("Eldritch","Mirrored Blades",30,"Blade Warp|Eldritch Blast","Attacks using duplicates of your weapon.");
 Add("Chaos","Miasma Bolt",1,"","Fires a bolt that releases a cloud of entropy."); Add("Chaos","Ring of Fire",6,"","Releases expanding hellfire."); Add("Chaos","Sigil: Lethargy",6,"","Lays a sigil that slows and weakens enemies."); Add("Chaos","Sigil: Rancor",12,"Sigil: Lethargy","Lays a sigil that enrages enemies."); Add("Chaos","Miasma Chain",12,"Miasma Bolt","Summons chains that emit miasma."); Add("Chaos","Flame Wave",18,"Ring of Fire","Unleashes a rolling wall of fire."); Add("Chaos","Sigil: Death",24,"Sigil: Rancor","Lays a sigil that hastens the death of injured enemies."); Add("Chaos","Enhanced Entropy",24,"Miasma Chain","Improves your mastery of shadow magic."); Add("Chaos","Apocalypse",30,"Flame Wave|Sigil: Death","Engulfs enemies in hellfire."); Add("Chaos","Abyss",30,"Enhanced Entropy","Creates a tear that pulls enemies in before collapsing.");
}
 void InitOtherClass(){
if(className == "Amazon") {
Add("Bow and Crossbow Skills","Magic Arrow",1,"","Active skill.");
Add("Bow and Crossbow Skills","Fire Arrow",1,"","Active skill.");
Add("Bow and Crossbow Skills","Cold Arrow",6,"","Active skill.");
Add("Bow and Crossbow Skills","Multiple Shot",6,"Magic Arrow","Active skill.");
Add("Bow and Crossbow Skills","Exploding Arrow",12,"Fire Arrow|Multiple Shot","Active skill.");
Add("Bow and Crossbow Skills","Ice Arrow",18,"Cold Arrow","Active skill.");
Add("Bow and Crossbow Skills","Guided Arrow",18,"Cold Arrow|Multiple Shot","Active skill.");
Add("Bow and Crossbow Skills","Strafe",24,"Guided Arrow","Active skill.");
Add("Bow and Crossbow Skills","Immolation Arrow",24,"Exploding Arrow","Active skill.");
Add("Bow and Crossbow Skills","Freezing Arrow",30,"Ice Arrow","Active skill.");
Add("Passive and Magic Skills","Inner Sight",1,"","Active skill.");
Add("Passive and Magic Skills","Critical Strike",1,"","Passive skill.");
Add("Passive and Magic Skills","Dodge",6,"","Passive skill.");
Add("Passive and Magic Skills","Slow Missiles",12,"Inner Sight","Active skill.");
Add("Passive and Magic Skills","Avoid",12,"Dodge","Passive skill.");
Add("Passive and Magic Skills","Penetrate",18,"Critical Strike","Passive skill.");
Add("Passive and Magic Skills","Decoy",24,"Slow Missiles","Summon skill.");
Add("Passive and Magic Skills","Evade",24,"Avoid","Passive skill.");
Add("Passive and Magic Skills","Valkyrie",30,"Decoy|Evade","Summon skill.");
Add("Passive and Magic Skills","Pierce",30,"Penetrate","Passive skill.");
Add("Javelin and Spear Skills","Jab",1,"","Active skill.");
Add("Javelin and Spear Skills","Power Strike",6,"Jab","Active skill.");
Add("Javelin and Spear Skills","Poison Javelin",6,"","Active skill.");
Add("Javelin and Spear Skills","Impale",12,"Jab","Active skill.");
Add("Javelin and Spear Skills","Lightning Bolt",12,"Poison Javelin","Active skill.");
Add("Javelin and Spear Skills","Charged Strike",18,"Power Strike|Lightning Bolt","Active skill.");
Add("Javelin and Spear Skills","Plague Javelin",18,"Lightning Bolt","Active skill.");
Add("Javelin and Spear Skills","Fend",24,"Impale","Active skill.");
Add("Javelin and Spear Skills","Lightning Strike",30,"Charged Strike","Active skill.");
Add("Javelin and Spear Skills","Lightning Fury",30,"Plague Javelin","Active skill.");
}
if(className == "Assassin") {
Add("Traps","Fire Blast",1,"","Active skill.");
Add("Traps","Shock Web",6,"Fire Blast","Active skill.");
Add("Traps","Blade Sentinel",6,"","Summon skill.");
Add("Traps","Charged Bolt Sentry",12,"Shock Web","Summon skill.");
Add("Traps","Wake of Fire",12,"Fire Blast","Summon skill.");
Add("Traps","Blade Fury",18,"Blade Sentinel|Wake of Fire","Active skill.");
Add("Traps","Lightning Sentry",24,"Charged Bolt Sentry","Summon skill.");
Add("Traps","Wake of Inferno",24,"Wake of Fire","Summon skill.");
Add("Traps","Death Sentry",30,"Lightning Sentry","Summon skill.");
Add("Traps","Blade Shield",30,"Blade Fury","Active skill.");
Add("Shadow Disciplines","Claw Mastery",1,"","Passive skill.");
Add("Shadow Disciplines","Psychic Hammer",1,"","Active skill.");
Add("Shadow Disciplines","Burst of Speed",6,"Claw Mastery","Active skill.");
Add("Shadow Disciplines","Weapon Block",12,"Claw Mastery","Passive skill.");
Add("Shadow Disciplines","Cloak of Shadows",12,"Psychic Hammer","Active skill.");
Add("Shadow Disciplines","Fade",18,"Burst of Speed","Active skill.");
Add("Shadow Disciplines","Shadow Warrior",18,"Cloak of Shadows|Weapon Block","Summon skill.");
Add("Shadow Disciplines","Mind Blast",24,"Cloak of Shadows","Active skill.");
Add("Shadow Disciplines","Venom",30,"Fade","Active skill.");
Add("Shadow Disciplines","Shadow Master",30,"Shadow Warrior","Summon skill.");
Add("Martial Arts","Tiger Strike",1,"","Active skill.");
Add("Martial Arts","Dragon Talon",1,"","Active skill.");
Add("Martial Arts","Fists of Fire",6,"","Active skill.");
Add("Martial Arts","Dragon Claw",6,"Dragon Talon","Active skill.");
Add("Martial Arts","Cobra Strike",12,"Tiger Strike","Active skill.");
Add("Martial Arts","Claws of Thunder",18,"Fists of Fire","Active skill.");
Add("Martial Arts","Dragon Tail",18,"Dragon Claw","Active skill.");
Add("Martial Arts","Blades of Ice",24,"Claws of Thunder","Active skill.");
Add("Martial Arts","Dragon Flight",24,"Dragon Tail","Active skill.");
Add("Martial Arts","Phoenix Strike",30,"Cobra Strike|Blades of Ice","Active skill.");
}
if(className == "Barbarian") {
Add("Combat Skills","Bash",1,"","Active skill.");
Add("Combat Skills","Leap",6,"","Active skill.");
Add("Combat Skills","Double Swing",6,"Bash","Active skill.");
Add("Combat Skills","Stun",12,"Bash","Active skill.");
Add("Combat Skills","Double Throw",12,"Double Swing","Active skill.");
Add("Combat Skills","Leap Attack",18,"Leap","Active skill.");
Add("Combat Skills","Concentrate",18,"Stun","Active skill.");
Add("Combat Skills","Frenzy",24,"Double Throw","Active skill.");
Add("Combat Skills","Whirlwind",30,"Leap Attack|Concentrate","Active skill.");
Add("Combat Skills","Berserk",30,"Concentrate","Active skill.");
Add("Masteries","Blade Mastery",1,"","Passive skill.");
Add("Masteries","Axe Mastery",1,"","Passive skill.");
Add("Masteries","Mace Mastery",1,"","Passive skill.");
Add("Masteries","Polearm Mastery",6,"","Passive skill.");
Add("Masteries","Throwing Mastery",6,"","Passive skill.");
Add("Masteries","Spear Mastery",6,"","Passive skill.");
Add("Masteries","Increased Stamina",12,"","Passive skill.");
Add("Masteries","Iron Skin",18,"","Passive skill.");
Add("Masteries","Increased Speed",24,"Increased Stamina","Passive skill.");
Add("Masteries","Natural Resistance",30,"Iron Skin","Passive skill.");
Add("Warcries","Howl",1,"","Active skill.");
Add("Warcries","Find Potion",1,"","Active skill.");
Add("Warcries","Taunt",6,"Howl","Active skill.");
Add("Warcries","Shout",6,"Howl","Active skill.");
Add("Warcries","Find Item",12,"Find Potion","Active skill.");
Add("Warcries","Battle Cry",18,"Taunt","Active skill.");
Add("Warcries","Battle Orders",24,"Shout","Active skill.");
Add("Warcries","Grim Ward",24,"Find Item","Active skill.");
Add("Warcries","War Cry",30,"Battle Cry|Battle Orders","Active skill.");
Add("Warcries","Battle Command",30,"Battle Orders","Active skill.");
}
if(className == "Druid") {
Add("Summoning Skills","Raven",1,"","Summon skill.");
Add("Summoning Skills","Poison Creeper",1,"","Summon skill.");
Add("Summoning Skills","Oak Sage",6,"","Summon skill.");
Add("Summoning Skills","Summon Spirit Wolf",6,"Raven","Summon skill.");
Add("Summoning Skills","Carrion Vine",12,"Poison Creeper","Summon skill.");
Add("Summoning Skills","Heart of Wolverine",18,"Oak Sage","Summon skill.");
Add("Summoning Skills","Summon Dire Wolf",18,"Oak Sage|Summon Spirit Wolf","Summon skill.");
Add("Summoning Skills","Solar Creeper",24,"Carrion Vine","Summon skill.");
Add("Summoning Skills","Spirit of Barbs",30,"Heart of Wolverine","Summon skill.");
Add("Summoning Skills","Summon Grizzly",30,"Summon Dire Wolf","Summon skill.");
Add("Shape Shifting Skills","Werewolf",1,"","Active skill.");
Add("Shape Shifting Skills","Lycanthropy",1,"Werewolf","Passive skill.");
Add("Shape Shifting Skills","Werebear",6,"","Active skill.");
Add("Shape Shifting Skills","Feral Rage",12,"Werewolf","Active skill.");
Add("Shape Shifting Skills","Maul",12,"Werebear","Active skill.");
Add("Shape Shifting Skills","Rabies",18,"Feral Rage","Active skill.");
Add("Shape Shifting Skills","Fire Claws",18,"Feral Rage|Maul","Active skill.");
Add("Shape Shifting Skills","Hunger",24,"Fire Claws","Active skill.");
Add("Shape Shifting Skills","Shock Wave",24,"Maul","Active skill.");
Add("Shape Shifting Skills","Fury",30,"Rabies","Active skill.");
Add("Elemental Skills","Firestorm",1,"","Active skill.");
Add("Elemental Skills","Molten Boulder",6,"Firestorm","Active skill.");
Add("Elemental Skills","Arctic Blast",6,"","Active skill.");
Add("Elemental Skills","Fissure",12,"Molten Boulder","Active skill.");
Add("Elemental Skills","Cyclone Armor",12,"Arctic Blast","Active skill.");
Add("Elemental Skills","Twister",18,"Cyclone Armor","Active skill.");
Add("Elemental Skills","Volcano",24,"Fissure","Active skill.");
Add("Elemental Skills","Tornado",24,"Twister","Active skill.");
Add("Elemental Skills","Armageddon",30,"Volcano","Active skill.");
Add("Elemental Skills","Hurricane",30,"Tornado","Active skill.");
}
if(className == "Necromancer") {
Add("Curses","Amplify Damage",1,"","Active skill.");
Add("Curses","Dim Vision",6,"","Active skill.");
Add("Curses","Weaken",6,"Amplify Damage","Active skill.");
Add("Curses","Iron Maiden",12,"Amplify Damage","Active skill.");
Add("Curses","Terror",12,"Weaken","Active skill.");
Add("Curses","Confuse",18,"Dim Vision","Active skill.");
Add("Curses","Life Tap",18,"Iron Maiden","Active skill.");
Add("Curses","Attract",24,"Confuse","Active skill.");
Add("Curses","Decrepify",24,"Terror","Active skill.");
Add("Curses","Lower Resist",30,"Life Tap|Decrepify","Active skill.");
Add("Poison and Bone Skills","Teeth",1,"","Active skill.");
Add("Poison and Bone Skills","Bone Armor",1,"","Active skill.");
Add("Poison and Bone Skills","Poison Dagger",6,"","Active skill.");
Add("Poison and Bone Skills","Corpse Explosion",6,"Teeth","Active skill.");
Add("Poison and Bone Skills","Bone Wall",12,"Bone Armor","Summon skill.");
Add("Poison and Bone Skills","Poison Explosion",18,"Poison Dagger|Corpse Explosion","Active skill.");
Add("Poison and Bone Skills","Bone Spear",18,"Corpse Explosion","Active skill.");
Add("Poison and Bone Skills","Bone Prison",24,"Bone Wall|Bone Spear","Summon skill.");
Add("Poison and Bone Skills","Poison Nova",30,"Poison Explosion","Active skill.");
Add("Poison and Bone Skills","Bone Spirit",30,"Bone Spear","Active skill.");
Add("Summoning Skills","Skeleton Mastery",1,"Raise Skeleton","Passive skill.");
Add("Summoning Skills","Raise Skeleton",1,"","Summon skill.");
Add("Summoning Skills","Clay Golem",6,"","Summon skill.");
Add("Summoning Skills","Golem Mastery",12,"Clay Golem","Passive skill.");
Add("Summoning Skills","Raise Skeletal Mage",12,"Raise Skeleton","Summon skill.");
Add("Summoning Skills","Blood Golem",18,"Clay Golem","Summon skill.");
Add("Summoning Skills","Summon Resist",24,"Golem Mastery","Passive skill.");
Add("Summoning Skills","Iron Golem",24,"Blood Golem","Summon skill.");
Add("Summoning Skills","Fire Golem",30,"Iron Golem","Summon skill.");
Add("Summoning Skills","Revive",30,"Raise Skeletal Mage|Iron Golem","Active skill.");
}
if(className == "Paladin") {
Add("Combat Skills","Sacrifice",1,"","Active skill.");
Add("Combat Skills","Smite",1,"","Active skill.");
Add("Combat Skills","Holy Bolt",6,"","Active skill.");
Add("Combat Skills","Zeal",12,"Sacrifice","Active skill.");
Add("Combat Skills","Charge",12,"Smite","Active skill.");
Add("Combat Skills","Vengeance",18,"Zeal","Active skill.");
Add("Combat Skills","Blessed Hammer",18,"Holy Bolt","Active skill.");
Add("Combat Skills","Conversion",24,"Vengeance","Active skill.");
Add("Combat Skills","Holy Shield",24,"Charge|Blessed Hammer","Active skill.");
Add("Combat Skills","Fist of the Heavens",30,"Blessed Hammer|Conversion","Active skill.");
Add("Offensive Auras","Might",1,"","Aura skill.");
Add("Offensive Auras","Holy Fire",6,"Might","Aura skill.");
Add("Offensive Auras","Thorns",6,"","Aura skill.");
Add("Offensive Auras","Blessed Aim",12,"Might","Aura skill.");
Add("Offensive Auras","Concentration",18,"Blessed Aim","Aura skill.");
Add("Offensive Auras","Holy Freeze",18,"Holy Fire","Aura skill.");
Add("Offensive Auras","Holy Shock",24,"Holy Freeze","Aura skill.");
Add("Offensive Auras","Sanctuary",24,"Thorns|Holy Freeze","Aura skill.");
Add("Offensive Auras","Fanaticism",30,"Concentration","Aura skill.");
Add("Offensive Auras","Conviction",30,"Sanctuary","Aura skill.");
Add("Defensive Auras","Prayer",1,"","Aura skill.");
Add("Defensive Auras","Resist Fire",1,"","Aura skill.");
Add("Defensive Auras","Defiance",6,"","Aura skill.");
Add("Defensive Auras","Resist Cold",6,"","Aura skill.");
Add("Defensive Auras","Cleansing",12,"Prayer","Aura skill.");
Add("Defensive Auras","Resist Lightning",12,"","Aura skill.");
Add("Defensive Auras","Vigor",18,"Cleansing|Defiance","Aura skill.");
Add("Defensive Auras","Meditation",24,"Cleansing","Aura skill.");
Add("Defensive Auras","Redemption",30,"Vigor","Aura skill.");
Add("Defensive Auras","Salvation",30,"","Aura skill.");
}
if(className == "Sorceress") {
Add("Fire Skills","Fire Bolt",1,"","Active skill.");
Add("Fire Skills","Warmth",1,"","Passive skill.");
Add("Fire Skills","Inferno",6,"","Active skill.");
Add("Fire Skills","Blaze",12,"Inferno","Active skill.");
Add("Fire Skills","Fire Ball",12,"Fire Bolt","Active skill.");
Add("Fire Skills","Fire Wall",18,"Blaze","Active skill.");
Add("Fire Skills","Enchant",18,"Warmth|Fire Ball","Active skill.");
Add("Fire Skills","Meteor",24,"Fire Ball|Fire Wall","Active skill.");
Add("Fire Skills","Fire Mastery",30,"","Passive skill.");
Add("Fire Skills","Hydra",30,"Enchant","Summon skill.");
Add("Lightning Skills","Charged Bolt",1,"","Active skill.");
Add("Lightning Skills","Static Field",6,"","Active skill.");
Add("Lightning Skills","Telekinesis",6,"","Active skill.");
Add("Lightning Skills","Nova",12,"Static Field","Active skill.");
Add("Lightning Skills","Lightning",12,"Charged Bolt","Active skill.");
Add("Lightning Skills","Chain Lightning",18,"Lightning","Active skill.");
Add("Lightning Skills","Teleport",18,"Telekinesis","Active skill.");
Add("Lightning Skills","Thunder Storm",24,"Nova|Chain Lightning","Active skill.");
Add("Lightning Skills","Energy Shield",24,"Teleport|Chain Lightning","Active skill.");
Add("Lightning Skills","Lightning Mastery",30,"","Passive skill.");
Add("Cold Skills","Ice Bolt",1,"","Active skill.");
Add("Cold Skills","Frozen Armor",1,"","Active skill.");
Add("Cold Skills","Frost Nova",6,"","Active skill.");
Add("Cold Skills","Ice Blast",6,"Ice Bolt","Active skill.");
Add("Cold Skills","Shiver Armor",12,"Ice Blast|Frozen Armor","Active skill.");
Add("Cold Skills","Glacial Spike",18,"Ice Blast","Active skill.");
Add("Cold Skills","Blizzard",24,"Frost Nova|Glacial Spike","Active skill.");
Add("Cold Skills","Chilling Armor",24,"Shiver Armor","Active skill.");
Add("Cold Skills","Frozen Orb",30,"Blizzard","Active skill.");
Add("Cold Skills","Cold Mastery",30,"","Passive skill.");
}
}
 void InitData(){InitWarlockSkills();
 W("Ancients' Pledge","Ral + Ort + Tal","3-socket Shields","Minimum level 21. Reign of the Warlock build 3.3.93847; the base may raise the requirement. Check game mode and edition availability.");
W("Authority","Hel + Shael + Ral","3-socket Body Armor","Minimum level 29. Reign of the Warlock build 3.3.93847; the base may raise the requirement. Check game mode and edition availability.");
W("Beast","Ber + Tir + Um + Mal + Lum","5-socket Axes, Scepters, Hammers","Minimum level 63. Reign of the Warlock build 3.3.93847; the base may raise the requirement. Check game mode and edition availability.");
W("Black","Thul + Io + Nef","3-socket Clubs, Hammers, Maces","Minimum level 35. Reign of the Warlock build 3.3.93847; the base may raise the requirement. Check game mode and edition availability.");
W("Bone","Sol + Um + Um","3-socket Body Armor","Minimum level 47. Reign of the Warlock build 3.3.93847; the base may raise the requirement. Check game mode and edition availability.");
W("Bramble","Ral + Ohm + Sur + Eth","4-socket Body Armor","Minimum level 61. Reign of the Warlock build 3.3.93847; the base may raise the requirement. Check game mode and edition availability.");
W("Brand","Jah + Lo + Mal + Gul","4-socket Missile Weapons","Minimum level 65. Reign of the Warlock build 3.3.93847; the base may raise the requirement. Check game mode and edition availability.");
W("Breath of the Dying","Vex + Hel + El + Eld + Zod + Eth","6-socket Weapons","Minimum level 69. Reign of the Warlock build 3.3.93847; the base may raise the requirement. Check game mode and edition availability.");
W("Bulwark","Shael + Io + Sol","3-socket Helms","Minimum level 35. Reign of the Warlock build 3.3.93847; the base may raise the requirement. Check game mode and edition availability.");
W("Call to Arms","Amn + Ral + Mal + Ist + Ohm","5-socket Weapons","Minimum level 57. Reign of the Warlock build 3.3.93847; the base may raise the requirement. Check game mode and edition availability.");
W("Chains of Honor","Dol + Um + Ber + Ist","4-socket Body Armor","Minimum level 63. Reign of the Warlock build 3.3.93847; the base may raise the requirement. Check game mode and edition availability.");
W("Chaos","Fal + Ohm + Um","3-socket Claws","Minimum level 57. Reign of the Warlock build 3.3.93847; the base may raise the requirement. Check game mode and edition availability.");
W("Coven","Ist + Ral + Io","3-socket Helms","Minimum level 51. Reign of the Warlock build 3.3.93847; the base may raise the requirement. Check game mode and edition availability.");
W("Crescent Moon","Shael + Um + Tir","3-socket Axes, Swords, Polearms","Minimum level 47. Reign of the Warlock build 3.3.93847; the base may raise the requirement. Check game mode and edition availability.");
W("Cure","Shael + Io + Tal","3-socket Helms","Minimum level 35. Reign of the Warlock build 3.3.93847; the base may raise the requirement. Check game mode and edition availability.");
W("Death","Hel + El + Vex + Ort + Gul","5-socket Swords, Axes","Minimum level 55. Reign of the Warlock build 3.3.93847; the base may raise the requirement. Check game mode and edition availability.");
W("Delirium","Lem + Ist + Io","3-socket Helms","Minimum level 51. Reign of the Warlock build 3.3.93847; the base may raise the requirement. Check game mode and edition availability.");
W("Destruction","Vex + Lo + Ber + Jah + Ko","5-socket Polearms, Swords","Minimum level 65. Reign of the Warlock build 3.3.93847; the base may raise the requirement. Check game mode and edition availability.");
W("Doom","Hel + Ohm + Um + Lo + Cham","5-socket Axes, Polearms, Hammers","Minimum level 67. Reign of the Warlock build 3.3.93847; the base may raise the requirement. Check game mode and edition availability.");
W("Dragon","Sur + Lo + Sol","3-socket Body Armor, Shields","Minimum level 61. Reign of the Warlock build 3.3.93847; the base may raise the requirement. Check game mode and edition availability.");
W("Dream","Io + Jah + Pul","3-socket Helms, Shields","Minimum level 65. Reign of the Warlock build 3.3.93847; the base may raise the requirement. Check game mode and edition availability.");
W("Duress","Shael + Um + Thul","3-socket Body Armor","Minimum level 47. Reign of the Warlock build 3.3.93847; the base may raise the requirement. Check game mode and edition availability.");
W("Edge","Tir + Tal + Amn","3-socket Missile Weapons","Minimum level 25. Reign of the Warlock build 3.3.93847; the base may raise the requirement. Check game mode and edition availability.");
W("Enigma","Jah + Ith + Ber","3-socket Body Armor","Minimum level 65. Reign of the Warlock build 3.3.93847; the base may raise the requirement. Check game mode and edition availability.");
W("Enlightenment","Pul + Ral + Sol","3-socket Body Armor","Minimum level 45. Reign of the Warlock build 3.3.93847; the base may raise the requirement. Check game mode and edition availability.");
W("Eternity","Amn + Ber + Ist + Sol + Sur","5-socket Melee Weapons","Minimum level 63. Reign of the Warlock build 3.3.93847; the base may raise the requirement. Check game mode and edition availability.");
W("Exile","Vex + Ohm + Ist + Dol","4-socket Paladin Shields","Minimum level 57. Reign of the Warlock build 3.3.93847; the base may raise the requirement. Check game mode and edition availability.");
W("Faith","Ohm + Jah + Lem + Eld","4-socket Missile Weapons","Minimum level 65. Reign of the Warlock build 3.3.93847; the base may raise the requirement. Check game mode and edition availability.");
W("Famine","Fal + Ohm + Ort + Jah","4-socket Axes, Hammers","Minimum level 65. Reign of the Warlock build 3.3.93847; the base may raise the requirement. Check game mode and edition availability.");
W("Flickering Flame","Nef + Pul + Vex","3-socket Helms","Minimum level 55. Reign of the Warlock build 3.3.93847; the base may raise the requirement. Check game mode and edition availability.");
W("Fortitude","El + Sol + Dol + Lo","4-socket Weapons, Body Armor","Minimum level 59. Reign of the Warlock build 3.3.93847; the base may raise the requirement. Check game mode and edition availability.");
W("Fury","Jah + Gul + Eth","3-socket Melee Weapons","Minimum level 65. Reign of the Warlock build 3.3.93847; the base may raise the requirement. Check game mode and edition availability.");
W("Gloom","Fal + Um + Pul","3-socket Body Armor","Minimum level 47. Reign of the Warlock build 3.3.93847; the base may raise the requirement. Check game mode and edition availability.");
W("Grief","Eth + Tir + Lo + Mal + Ral","5-socket Swords, Axes","Minimum level 59. Reign of the Warlock build 3.3.93847; the base may raise the requirement. Check game mode and edition availability.");
W("Ground","Shael + Io + Ort","3-socket Helms","Minimum level 35. Reign of the Warlock build 3.3.93847; the base may raise the requirement. Check game mode and edition availability.");
W("Hand of Justice","Sur + Cham + Amn + Lo","4-socket Weapons","Minimum level 67. Reign of the Warlock build 3.3.93847; the base may raise the requirement. Check game mode and edition availability.");
W("Harmony","Tir + Ith + Sol + Ko","4-socket Missile Weapons","Minimum level 39. Reign of the Warlock build 3.3.93847; the base may raise the requirement. Check game mode and edition availability.");
W("Heart of the Oak","Ko + Vex + Pul + Thul","4-socket Staves, Maces","Minimum level 55. Reign of the Warlock build 3.3.93847; the base may raise the requirement. Check game mode and edition availability.");
W("Hearth","Shael + Io + Thul","3-socket Helms","Minimum level 35. Reign of the Warlock build 3.3.93847; the base may raise the requirement. Check game mode and edition availability.");
W("Holy Thunder","Eth + Ral + Ort + Tal","4-socket Scepters","Minimum level 24. Reign of the Warlock build 3.3.93847; the base may raise the requirement. Check game mode and edition availability.");
W("Honor","Amn + El + Ith + Tir + Sol","5-socket Melee Weapons","Minimum level 27. Reign of the Warlock build 3.3.93847; the base may raise the requirement. Check game mode and edition availability.");
W("Hysteria","Shael + Ko + Eld","3-socket Body Armor","Minimum level 39. Reign of the Warlock build 3.3.93847; the base may raise the requirement. Check game mode and edition availability.");
W("Ice","Amn + Shael + Jah + Lo","4-socket Missile Weapons","Minimum level 65. Reign of the Warlock build 3.3.93847; the base may raise the requirement. Check game mode and edition availability.");
W("Infinity","Ber + Mal + Ber + Ist","4-socket Polearms, Spears","Minimum level 63. Reign of the Warlock build 3.3.93847; the base may raise the requirement. Check game mode and edition availability.");
W("Insight","Ral + Tir + Tal + Sol","4-socket Polearms, Staves, Missile Weapons","Minimum level 27. Reign of the Warlock build 3.3.93847; the base may raise the requirement. Check game mode and edition availability.");
W("King's Grace","Amn + Ral + Thul","3-socket Swords, Scepters","Minimum level 25. Reign of the Warlock build 3.3.93847; the base may raise the requirement. Check game mode and edition availability.");
W("Kingslayer","Mal + Um + Gul + Fal","4-socket Swords, Axes","Minimum level 53. Reign of the Warlock build 3.3.93847; the base may raise the requirement. Check game mode and edition availability.");
W("Last Wish","Jah + Mal + Jah + Sur + Jah + Ber","6-socket Swords, Hammers, Axes","Minimum level 65. Reign of the Warlock build 3.3.93847; the base may raise the requirement. Check game mode and edition availability.");
W("Lawbringer","Amn + Lem + Ko","3-socket Swords, Hammers, Scepters","Minimum level 43. Reign of the Warlock build 3.3.93847; the base may raise the requirement. Check game mode and edition availability.");
W("Leaf","Tir + Ral","2-socket Staves","Minimum level 19. Reign of the Warlock build 3.3.93847; the base may raise the requirement. Check game mode and edition availability.");
W("Lionheart","Hel + Lum + Fal","3-socket Body Armor","Minimum level 41. Reign of the Warlock build 3.3.93847; the base may raise the requirement. Check game mode and edition availability.");
W("Lore","Ort + Sol","2-socket Helms","Minimum level 27. Reign of the Warlock build 3.3.93847; the base may raise the requirement. Check game mode and edition availability.");
W("Malice","Ith + El + Eth","3-socket Melee Weapons","Minimum level 15. Reign of the Warlock build 3.3.93847; the base may raise the requirement. Check game mode and edition availability.");
W("Mania","Shael + Ko + Eld","3-socket Weapons","Minimum level 39. Reign of the Warlock build 3.3.93847; the base may raise the requirement. Check game mode and edition availability.");
W("Melody","Shael + Ko + Nef","3-socket Missile Weapons","Minimum level 39. Reign of the Warlock build 3.3.93847; the base may raise the requirement. Check game mode and edition availability.");
W("Memory","Lum + Io + Sol + Eth","4-socket Staves","Minimum level 37. Reign of the Warlock build 3.3.93847; the base may raise the requirement. Check game mode and edition availability.");
W("Metamorphosis","Io + Cham + Fal","3-socket Helms","Minimum level 67. Reign of the Warlock build 3.3.93847; the base may raise the requirement. Check game mode and edition availability.");
W("Mist","Cham + Shael + Gul + Thul + Ith","5-socket Missile Weapons","Minimum level 67. Reign of the Warlock build 3.3.93847; the base may raise the requirement. Check game mode and edition availability.");
W("Mosaic","Mal + Gul + Amn","3-socket Claws","Minimum level 53. Reign of the Warlock build 3.3.93847; the base may raise the requirement. Check game mode and edition availability.");
W("Myth","Hel + Amn + Nef","3-socket Body Armor","Minimum level 25. Reign of the Warlock build 3.3.93847; the base may raise the requirement. Check game mode and edition availability.");
W("Nadir","Nef + Tir","2-socket Helms","Minimum level 13. Reign of the Warlock build 3.3.93847; the base may raise the requirement. Check game mode and edition availability.");
W("Oath","Shael + Pul + Mal + Lum","4-socket Swords, Axes, Maces","Minimum level 49. Reign of the Warlock build 3.3.93847; the base may raise the requirement. Check game mode and edition availability.");
W("Obedience","Hel + Ko + Thul + Eth + Fal","5-socket Polearms, Spears","Minimum level 41. Reign of the Warlock build 3.3.93847; the base may raise the requirement. Check game mode and edition availability.");
W("Obsession","Zod + Ist + Lem + Lum + Io + Nef","6-socket Staves","Minimum level 69. Reign of the Warlock build 3.3.93847; the base may raise the requirement. Check game mode and edition availability.");
W("Passion","Dol + Ort + Eld + Lem","4-socket Weapons","Minimum level 43. Reign of the Warlock build 3.3.93847; the base may raise the requirement. Check game mode and edition availability.");
W("Pattern","Tal + Ort + Thul","3-socket Claws","Minimum level 23. Reign of the Warlock build 3.3.93847; the base may raise the requirement. Check game mode and edition availability.");
W("Peace","Shael + Thul + Amn","3-socket Body Armor","Minimum level 29. Reign of the Warlock build 3.3.93847; the base may raise the requirement. Check game mode and edition availability.");
W("Phoenix","Vex + Vex + Lo + Jah","4-socket Weapons, Shields","Minimum level 65. Reign of the Warlock build 3.3.93847; the base may raise the requirement. Check game mode and edition availability.");
W("Plague","Cham + Shael + Um","3-socket Swords, Daggers, Claws","Minimum level 67. Reign of the Warlock build 3.3.93847; the base may raise the requirement. Check game mode and edition availability.");
W("Pride","Cham + Sur + Io + Lo","4-socket Polearms, Spears","Minimum level 67. Reign of the Warlock build 3.3.93847; the base may raise the requirement. Check game mode and edition availability.");
W("Principle","Ral + Gul + Eld","3-socket Body Armor","Minimum level 53. Reign of the Warlock build 3.3.93847; the base may raise the requirement. Check game mode and edition availability.");
W("Prudence","Mal + Tir","2-socket Body Armor","Minimum level 49. Reign of the Warlock build 3.3.93847; the base may raise the requirement. Check game mode and edition availability.");
W("Radiance","Nef + Sol + Ith","3-socket Helms","Minimum level 27. Reign of the Warlock build 3.3.93847; the base may raise the requirement. Check game mode and edition availability.");
W("Rain","Ort + Mal + Ith","3-socket Body Armor","Minimum level 49. Reign of the Warlock build 3.3.93847; the base may raise the requirement. Check game mode and edition availability.");
W("Rhyme","Shael + Eth","2-socket Shields","Minimum level 29. Reign of the Warlock build 3.3.93847; the base may raise the requirement. Check game mode and edition availability.");
W("Rift","Hel + Ko + Lem + Gul","4-socket Polearms, Scepters","Minimum level 53. Reign of the Warlock build 3.3.93847; the base may raise the requirement. Check game mode and edition availability.");
W("Ritual","Amn + Shael + Ohm","3-socket Daggers","Minimum level 57. Reign of the Warlock build 3.3.93847; the base may raise the requirement. Check game mode and edition availability.");
W("Sanctuary","Ko + Ko + Mal","3-socket Shields","Minimum level 49. Reign of the Warlock build 3.3.93847; the base may raise the requirement. Check game mode and edition availability.");
W("Silence","Dol + Eld + Hel + Ist + Tir + Vex","6-socket Weapons","Minimum level 55. Reign of the Warlock build 3.3.93847; the base may raise the requirement. Check game mode and edition availability.");
W("Smoke","Nef + Lum","2-socket Body Armor","Minimum level 37. Reign of the Warlock build 3.3.93847; the base may raise the requirement. Check game mode and edition availability.");
W("Spirit","Tal + Thul + Ort + Amn","4-socket Swords, Shields","Minimum level 25. Reign of the Warlock build 3.3.93847; the base may raise the requirement. Check game mode and edition availability.");
W("Splendor","Eth + Lum","2-socket Shields","Minimum level 37. Reign of the Warlock build 3.3.93847; the base may raise the requirement. Check game mode and edition availability.");
W("Stealth","Tal + Eth","2-socket Body Armor","Minimum level 17. Reign of the Warlock build 3.3.93847; the base may raise the requirement. Check game mode and edition availability.");
W("Steel","Tir + El","2-socket Swords, Axes, Maces","Minimum level 13. Reign of the Warlock build 3.3.93847; the base may raise the requirement. Check game mode and edition availability.");
W("Stone","Shael + Um + Pul + Lum","4-socket Body Armor","Minimum level 47. Reign of the Warlock build 3.3.93847; the base may raise the requirement. Check game mode and edition availability.");
W("Strength","Amn + Tir","2-socket Melee Weapons","Minimum level 25. Reign of the Warlock build 3.3.93847; the base may raise the requirement. Check game mode and edition availability.");
W("Temper","Shael + Io + Ral","3-socket Helms","Minimum level 35. Reign of the Warlock build 3.3.93847; the base may raise the requirement. Check game mode and edition availability.");
W("Treachery","Shael + Thul + Lem","3-socket Body Armor","Minimum level 43. Reign of the Warlock build 3.3.93847; the base may raise the requirement. Check game mode and edition availability.");
W("Unbending Will","Fal + Io + Ith + Eld + El + Hel","6-socket Swords","Minimum level 41. Reign of the Warlock build 3.3.93847; the base may raise the requirement. Check game mode and edition availability.");
W("Venom","Tal + Dol + Mal","3-socket Weapons","Minimum level 49. Reign of the Warlock build 3.3.93847; the base may raise the requirement. Check game mode and edition availability.");
W("Vigilance","Dol + Gul","2-socket Grimoires, Shields, Shrunken Heads, Auric Shields","Minimum level 53. Reign of the Warlock build 3.3.93847; the base may raise the requirement. Check game mode and edition availability.");
W("Voice of Reason","Lem + Ko + El + Eld","4-socket Swords, Maces","Minimum level 43. Reign of the Warlock build 3.3.93847; the base may raise the requirement. Check game mode and edition availability.");
W("Void","Thul + Zod + Ist","3-socket Daggers","Minimum level 69. Reign of the Warlock build 3.3.93847; the base may raise the requirement. Check game mode and edition availability.");
W("Wealth","Lem + Ko + Tir","3-socket Body Armor","Minimum level 43. Reign of the Warlock build 3.3.93847; the base may raise the requirement. Check game mode and edition availability.");
W("White","Dol + Io","2-socket Wands","Minimum level 35. Reign of the Warlock build 3.3.93847; the base may raise the requirement. Check game mode and edition availability.");
W("Wind","Sur + El","2-socket Melee Weapons","Minimum level 61. Reign of the Warlock build 3.3.93847; the base may raise the requirement. Check game mode and edition availability.");
W("Wisdom","Pul + Ith + Eld","3-socket Helms","Minimum level 45. Reign of the Warlock build 3.3.93847; the base may raise the requirement. Check game mode and edition availability.");
W("Wrath","Pul + Lum + Ber + Mal","4-socket Missile Weapons","Minimum level 63. Reign of the Warlock build 3.3.93847; the base may raise the requirement. Check game mode and edition availability.");
W("Zephyr","Ort + Eth","2-socket Missile Weapons","Minimum level 21. Reign of the Warlock build 3.3.93847; the base may raise the requirement. Check game mode and edition availability.");
 LoadWordEffects();InitCube();
 }
 string contentHash; Dictionary<string,SkillContentEntry> skillContent=new Dictionary<string,SkillContentEntry>(StringComparer.Ordinal);
 List<GuideRegion> guideRegions=new List<GuideRegion>(GuideBook.Regions);
 ListBox guideList;
 string ContentPath(){return Path.Combine(dataDir,"SanctuaryAtlasContent.xml");}
 Dictionary<string,string> ParseWordEffects(byte[] data){
  if(data==null||data.Length==0||data.Length>2000000)throw new InvalidOperationException("Invalid runeword content size.");
  var settings=new System.Xml.XmlReaderSettings{DtdProcessing=System.Xml.DtdProcessing.Prohibit,XmlResolver=null,MaxCharactersInDocument=2000000};var doc=new System.Xml.XmlDocument{XmlResolver=null};
  using(var stream=new MemoryStream(data))using(var reader=System.Xml.XmlReader.Create(stream,settings))doc.Load(reader);
  if(doc.DocumentElement==null||doc.DocumentElement.Name!="runewords")throw new InvalidOperationException("Invalid runeword content format.");
  var result=new Dictionary<string,string>(StringComparer.Ordinal);foreach(System.Xml.XmlNode node in doc.SelectNodes("/runewords/word")){
   var attr=node.Attributes["name"];if(attr==null||words.Find(w=>w.Name==attr.Value)==null||result.ContainsKey(attr.Value))throw new InvalidOperationException("Unknown or duplicate runeword in content.");
   var sections=new List<string>();foreach(System.Xml.XmlNode group in node.SelectNodes("group")){var lines=new List<string>();if(group.Attributes["bases"]!=null)lines.Add(group.Attributes["bases"].Value.ToUpperInvariant()+":");var effects=group.SelectNodes("effect");if(effects.Count==0)throw new InvalidOperationException("Runeword effects are missing.");foreach(System.Xml.XmlNode effect in effects){if(String.IsNullOrWhiteSpace(effect.InnerText))throw new InvalidOperationException("Empty runeword effect.");lines.Add("• "+effect.InnerText);}sections.Add(String.Join("\r\n",lines.ToArray()));}
   if(sections.Count==0)throw new InvalidOperationException("Runeword effects are missing.");result.Add(attr.Value,String.Join("\r\n\r\n",sections.ToArray()));
  }if(result.Count!=words.Count)throw new InvalidOperationException("The runeword content is incomplete.");return result;
 }
 static string ContentDigest(byte[] data){using(var sha=System.Security.Cryptography.SHA256.Create())return BitConverter.ToString(sha.ComputeHash(data)).Replace("-","");}
 void ApplySkillContent(){SkillContent.Apply(skills,className,skillContent);}
 void SetWordEffects(Dictionary<string,string> effects,List<GuideRegion> guides,Dictionary<string,SkillContentEntry> skillUpdates,byte[] data){foreach(var word in words)word.Effects=effects[word.Name];guideRegions=guides;skillContent=skillUpdates;ApplySkillContent();contentHash=ContentDigest(data);}
 void LoadWordEffects(){
  byte[] data=null;try{if(File.Exists(ContentPath())){data=File.ReadAllBytes(ContentPath());SetWordEffects(ParseWordEffects(data),GuideContent.Parse(data),SkillContent.Parse(data),data);return;}}catch{}
  using(var stream=Assembly.GetExecutingAssembly().GetManifestResourceStream("RunewordEffects")){if(stream==null)throw new InvalidOperationException("Runeword effects data is missing.");using(var ms=new MemoryStream()){stream.CopyTo(ms);data=ms.ToArray();}}
  SetWordEffects(ParseWordEffects(data),GuideContent.Parse(data),SkillContent.Parse(data),data);
 }
 void ApplyWordContent(byte[] data){
  var effects=ParseWordEffects(data);var guides=GuideContent.Parse(data);var skillUpdates=SkillContent.Parse(data);Directory.CreateDirectory(dataDir);string path=ContentPath(),temp=path+"."+Guid.NewGuid().ToString("N")+".tmp";
  try{File.WriteAllBytes(temp,data);if(File.Exists(path))File.Replace(temp,path,null);else File.Move(temp,path);}finally{if(File.Exists(temp))File.Delete(temp);}
  SetWordEffects(effects,guides,skillUpdates,data);if(search!=null&&!search.IsDisposed&&search.Parent==content){string selected=results.SelectedItem as string;FilterWords();if(selected!=null&&results.Items.Contains(selected))results.SelectedItem=selected;}if(guideList!=null&&!guideList.IsDisposed&&guideList.Parent==content){string selected=(guideList.SelectedItem as GuideRegion)==null?null:((GuideRegion)guideList.SelectedItem).Name;RefreshGuideList(selected);}if(skillList!=null&&!skillList.IsDisposed&&skillList.Parent==content){string selected=tree==null?null:tree.SelectedItem as string;ShowSkills();if(selected!=null&&tree.Items.Contains(selected))tree.SelectedItem=selected;}
 }
 void W(string n,string r,string b,string a){words.Add(new Word(n,r,b,a));}
 Label LabelAt(string s,int x,int y,int w,int h,float size,Color c,FontStyle st=FontStyle.Regular){var a=new Label{Text=s,Location=new Point(x,y),Size=new Size(w,h),ForeColor=c,Font=f(size,st),AutoEllipsis=true,BackColor=Color.Transparent};return a;}
 Button Btn(string s,int x,int y,int w,int h){var b=new GothicButton{Text=s,Location=new Point(x,y),Size=new Size(w,h),FlatStyle=FlatStyle.Flat,BackColor=panel,ForeColor=text,Font=f(9,FontStyle.Bold),Cursor=Cursors.Hand};b.FlatAppearance.BorderColor=Color.FromArgb(72,65,56);return b;}
 void StyleCombo(ComboBox box){box.DrawMode=DrawMode.OwnerDrawFixed;box.ItemHeight=20;box.DrawItem+=(s,e)=>{e.Graphics.FillRectangle(Brushes.White,e.Bounds);if(e.Index>=0){string label=box.Items[e.Index].ToString();TextRenderer.DrawText(e.Graphics,label,box.Font,e.Bounds,Color.Black,TextFormatFlags.Left|TextFormatFlags.VerticalCenter|TextFormatFlags.EndEllipsis);}e.DrawFocusRectangle();};}
 void MakeShell(){var side=new Panel{Dock=DockStyle.Left,Width=205,BackColor=Color.FromArgb(28,21,18)}; side.Paint+=(s,e)=>{using(var b=new LinearGradientBrush(side.ClientRectangle,Color.FromArgb(42,29,22),Color.FromArgb(13,12,11),0f))e.Graphics.FillRectangle(b,side.ClientRectangle);using(var p=new Pen(Color.FromArgb(118,84,48),2))e.Graphics.DrawLine(p,side.Width-2,8,side.Width-2,side.Height-8);}; Controls.Add(side); side.Controls.Add(LabelAt("S A N C T U A R Y",20,30,170,23,9,gold,FontStyle.Bold)); side.Controls.Add(LabelAt("ATLAS",20,55,170,35,23,text,FontStyle.Bold)); side.Controls.Add(LabelAt("D2R COMPANION",20,96,170,20,8,muted)); string[] nav={"01   SKILL PLANNER","02   EQUIPMENT","03   RUNEWORDS","04   RUNE STASH","05   CUBE RECIPES","06   RUNE CALCULATOR","07   GUIDE BOOK","08   CURATED BUILDS","09   SHOPPING GUIDE","10   ITEM LIBRARY"}; for(int i=0;i<nav.Length;i++){int k=i;var b=Btn(nav[i],12,155+i*44,181,38);b.TextAlign=ContentAlignment.MiddleLeft;b.Click+=(s,e)=>{if(k==0)ShowSkills();if(k==1)ShowGear();if(k==2)ShowWords();if(k==3)ShowRunes();if(k==4)ShowCube();if(k==5)ShowRuneCalculator();if(k==6)ShowGuide();if(k==7)ShowCuratedBuilds();if(k==8)ShowShoppingGuide();if(k==9)ShowItemLibrary();};side.Controls.Add(b);} AddUpdateCorner(side); content=new Panel{Dock=DockStyle.Fill,AutoScroll=true,BackColor=bg};using(var stream=Assembly.GetExecutingAssembly().GetManifestResourceStream("SanctuaryBackground")){if(stream!=null){using(var image=Image.FromStream(stream))content.BackgroundImage=new Bitmap(image);}}content.BackgroundImageLayout=ImageLayout.Stretch;content.Resize+=(s,e)=>{LayoutSkills();if(skillList!=null&&!skillList.IsDisposed)skillList.RefreshBackground();};content.Scroll+=(s,e)=>{if(skillList!=null&&!skillList.IsDisposed)skillList.RefreshBackground();};Controls.Add(content);content.BringToFront(); }
 void Header(string eyebrow,string title,string subtitle){inlineDetail=null;wideDetails=false;slotTip.Active=true;slotTip.RemoveAll();skillTip.Active=true;skillTip.RemoveAll();var oldControls=content.Controls.Cast<Control>().ToArray();content.Controls.Clear();foreach(var c in oldControls)c.Dispose();content.Controls.Add(LabelAt(eyebrow.ToUpper(),30,24,800,20,9,gold,FontStyle.Bold));var heading=LabelAt(title.ToUpperInvariant(),30,49,850,44,23,text,FontStyle.Bold);heading.Font=new Font("Georgia",22,FontStyle.Bold);content.Controls.Add(heading);content.Controls.Add(LabelAt(subtitle,32,101,850,38,10,muted));}
 void LayoutSkills(){
  if(skillList==null||skillList.IsDisposed||skillList.Parent!=content)return;
  int height=Math.Max(120,content.ClientSize.Height-342);int detailX=30+850+18;
  bool wide=inlineDetail!=null&&!inlineDetail.IsDisposed&&inlineDetail.Parent==content&&content.ClientSize.Width-detailX-30>=320;
  wideDetails=wide;skillTip.Active=!wide;
  skillList.Size=new Size(wide?850:Math.Max(850,content.ClientSize.Width-60),height);
  if(inlineDetail!=null&&!inlineDetail.IsDisposed&&inlineDetail.Parent==content){
   inlineDetail.Visible=wide;
   if(wide){inlineDetail.SetBounds(detailX,322,Math.Min(560,content.ClientSize.Width-detailX-30),Math.Min(600,height));inlineDetail.BringToFront();
    if(detailWindow!=null&&!detailWindow.IsDisposed){int selected=detailSkillIndex;detailWindow.Dispose();detailWindow=null;detailSkillIndex=selected;}
    RefreshSkillDetail();
   }
  }
 }
 void ShowSkills(){Header("Character  /  build planner",className+" skill planner","Choose a class and skill tree. On Warlock cards, +N shows equipped skill levels beside invested points.");
 for(int i=0;i<classes.Length;i++){string choice=classes[i];var tab=Btn(choice,32+i*103,145,99,32);tab.Font=f(8,FontStyle.Bold);if(choice==className){tab.BackColor=Color.FromArgb(92,65,39);tab.ForeColor=Color.White;}tab.Click+=(s,e)=>SwitchClass(choice);content.Controls.Add(tab);}
 content.Controls.Add(LabelAt("CHARACTER LEVEL",32,198,140,20,8,gold,FontStyle.Bold)); level=new NumericUpDown{Location=new Point(32,220),Width=95,Minimum=1,Maximum=99,Value=build.Level,BackColor=panel,ForeColor=text};content.Controls.Add(level);
 content.Controls.Add(LabelAt("QUEST POINTS",154,198,120,20,8,gold,FontStyle.Bold)); quests=new NumericUpDown{Location=new Point(154,220),Width=95,Minimum=0,Maximum=12,Value=build.Quests,BackColor=panel,ForeColor=text};content.Controls.Add(quests);budget=LabelAt("",285,217,370,30,12,text,FontStyle.Bold);content.Controls.Add(budget);
 var reset=Btn("RESET POINTS",700,211,150,34);reset.Click+=(s,e)=>{if(MessageBox.Show("Reset all skill points?","Sanctuary Atlas",MessageBoxButtons.YesNo)==DialogResult.Yes){build.Points=new int[30];Save();DrawSkills();}};content.Controls.Add(reset);
 level.ValueChanged+=(s,e)=>{build.Level=(int)level.Value;Save();DrawSkills();};quests.ValueChanged+=(s,e)=>{build.Quests=(int)quests.Value;Save();DrawSkills();};
 tree=new ComboBox();tree.Items.AddRange(skills.Select(x=>x.Tree).Distinct().Cast<object>().ToArray());var tabs=new List<Button>();for(int i=0;i<tree.Items.Count;i++){string choice=(string)tree.Items[i];var b=Btn(choice,32+i*274,271,266,36);b.Font=f(9,FontStyle.Bold);b.Click+=(s,e)=>tree.SelectedItem=choice;tabs.Add(b);content.Controls.Add(b);}tree.SelectedIndexChanged+=(s,e)=>{for(int i=0;i<tabs.Count;i++){bool active=i==tree.SelectedIndex;tabs[i].BackColor=active?Color.FromArgb(92,65,39):panel;tabs[i].ForeColor=active?Color.White:text;}DrawSkills();};
 skillList=new SkillTreePanel{BackgroundOwner=content,Location=new Point(30,322),Size=new Size(850,438),BackColor=Color.FromArgb(19,16,15),AutoScroll=false,Anchor=AnchorStyles.Top|AnchorStyles.Left};content.Controls.Add(skillList);inlineDetail=new SkillDetailView();content.Controls.Add(inlineDetail);LayoutSkills();tree.SelectedIndex=0; }
 void SwitchClass(string next){if(next==className)return;CloseSkillDetail();selectedSlot=-1;moving=false;className=next; skills.Clear();if(next=="Warlock")InitWarlockSkills();else InitOtherClass();ApplySkillContent();save=Path.Combine(dataDir,next=="Warlock"?"build.xml":"build-"+next.ToLowerInvariant()+".xml");LoadBuild();ShowSkills();}
 int Index(string name){return skills.FindIndex(x=>x.Name==name);} int Spent(){return build.Points.Sum();}
 string PrerequisiteProblem(){
  for(int i=0;i<skills.Count;i++)if(build.Points[i]>0 && skills[i].Requires!="")foreach(string req in skills[i].Requires.Split('|')){
   int requiredIndex=Index(req);
   if(requiredIndex<0 || build.Points[requiredIndex]==0)return skills[i].Name+" requires a point in "+req+".";
  }
  return null;
 }
 string BuildProblem(){
  int available=build.Level-1+build.Quests;
  if(Spent()>available)return "This build uses "+Spent()+" points, but only "+available+" are available. Increase your character level or quest points, or remove points.";
  for(int i=0;i<skills.Count;i++)if(build.Points[i]>0){
   var sk=skills[i];
   if(build.Level<sk.Level)return sk.Name+" requires character level "+sk.Level+".";
   int max=Math.Min(20,build.Level-sk.Level+1);
   if(build.Points[i]>max)return sk.Name+" can have at most "+max+" point(s) at character level "+build.Level+". Reach level "+(sk.Level+build.Points[i]-1)+" to add this point.";
  }
  return PrerequisiteProblem();
 }
 bool Valid(){return BuildProblem()==null;}
 PreviewTooltipSession ResearchSession(){ return new PreviewTooltipSession(className,skills); }
 SkillDetailWindow detailWindow;SkillDetailView inlineDetail;bool wideDetails;int detailSkillIndex=-1;
 void CloseSkillDetail(){if(detailWindow!=null){detailWindow.Dispose();detailWindow=null;}detailSkillIndex=-1;if(inlineDetail!=null&&!inlineDetail.IsDisposed)inlineDetail.SetDetails("SKILL DETAILS","Hover over a skill to see its effects.");}
 void OpenSkillDetail(int index,bool show){
  if(index<0||index>=skills.Count)return;
  if(wideDetails){detailSkillIndex=index;inlineDetail.Pinned=true;RefreshSkillDetail();return;}
  if(detailWindow==null||detailWindow.IsDisposed){detailWindow=new SkillDetailWindow();detailWindow.FormClosed+=(s,e)=>{detailWindow=null;detailSkillIndex=-1;};}
  detailSkillIndex=index;RefreshSkillDetail();if(show&&!detailWindow.Visible)detailWindow.Show(this);else if(show)detailWindow.Activate();
 }
 void RefreshSkillDetail(){
  if(detailSkillIndex<0||detailSkillIndex>=skills.Count)return;
  var skill=skills[detailSkillIndex];string tip=skill.Name+"\n"+skill.Desc+"\nDetailed effects coming in a future update.";
  try{tip=ResearchSession().Render(skill,build);}catch(Exception){}
  if(detailWindow!=null&&!detailWindow.IsDisposed)detailWindow.SetDetails(skill.Name,tip);
  if(inlineDetail!=null&&!inlineDetail.IsDisposed)inlineDetail.SetDetails(skill.Name,tip);
 }
 void HoverSkill(int index){if(!wideDetails||inlineDetail.Pinned||index==detailSkillIndex)return;detailSkillIndex=index;RefreshSkillDetail();}
 void SetSkillTooltips(Control control,string tip,int index){skillTip.SetToolTip(control,tip);control.MouseEnter+=(s,e)=>HoverSkill(index);if(!(control is Button))control.Click+=(s,e)=>{if(!wideDetails)OpenSkillDetail(index,true);};foreach(Control child in control.Controls)SetSkillTooltips(child,tip,index);}
 void DrawSkills(){if(skillList==null||tree==null)return;skillTip.RemoveAll();PreviewTooltipSession research=null;SkillLevels gearLevels=null;try{research=ResearchSession();if(className=="Warlock")gearLevels=GearSkillBonuses.LevelsForBuild(build,skills);}catch(Exception){}budget.Text="POINTS  "+Spent()+" / "+(build.Level-1+build.Quests)+(Valid()?"":"    • Check level or quest points");budget.ForeColor=Valid()?gold:Color.Salmon;var scroll=skillList.AutoScrollPosition;skillList.AutoScrollPosition=Point.Empty;skillList.ClearSkills();int rowIndex=0;foreach(int tier in new int[]{1,6,12,18,24,30}){int y=rowIndex++*112;skillList.Controls.Add(LabelAt("LV\n"+tier,4,y+24,35,54,9,gold,FontStyle.Bold));var items=skills.Where(x=>x.Tree==(string)tree.SelectedItem&&x.Level==tier).ToList();for(int j=0;j<items.Count;j++){var sk=items[j];int idx=Index(sk.Name);var card=new GothicPanel{Location=new Point(42+j*250,y+2),Size=new Size(244,102),BackColor=Color.FromArgb(35,27,23)};card.Controls.Add(LabelAt(sk.Name,10,8,220,23,10,text,FontStyle.Bold));string requirement=sk.Requires==""?"No prerequisite":"Requires: "+sk.Requires.Replace("|"," + ");card.Controls.Add(LabelAt(requirement,10,31,220,30,8,muted));string tip=sk.Name+"\n"+sk.Desc+"\n"+requirement+"\nDetailed effects coming in a future update.";if(research!=null)try{tip=research.Render(sk,build);}catch(Exception){}var minus=Btn("−",10,66,35,27);int hard=build.Points[idx],effective=gearLevels==null?hard:gearLevels.Effective(idx+1);string points=hard+"/20"+(effective>hard?" +"+(effective-hard):"");var count=LabelAt(points,50,70,76,22,8,gold,FontStyle.Bold);count.AccessibleName="Invested "+hard+" of 20; effective level "+effective;var plus=Btn("+",129,66,35,27);minus.Click+=(s,e)=>Change(idx,-1);plus.Click+=(s,e)=>Change(idx,1);card.Controls.Add(minus);card.Controls.Add(count);card.Controls.Add(plus);SetSkillTooltips(card,tip,idx);skillList.Controls.Add(card);}} skillList.FinishSkills(-scroll.Y);RefreshSkillDetail(); }
 bool PrerequisitesValid(){return PrerequisiteProblem()==null;}
 void Change(int i,int delta){
  int old=build.Points[i]; if(old+delta<0||old+delta>20)return;
  build.Points[i]+=delta;
  string problem=delta>0?BuildProblem():PrerequisiteProblem();
  if(problem!=null){build.Points[i]=old;MessageBox.Show("Skill: "+skills[i].Name+"\n\n"+problem,"Cannot change skill point");return;}
  Save();DrawSkills();
 }
 static string[] gearNames={"Weapon","Off-hand","Helm","Armor","Gloves","Belt","Boots","Amulet","Ring 1","Ring 2"};
 string SlotText(bool inventory,int index){return inventory?build.Inventory[index]:build.Gear[index];}
 ItemSkillBonuses SlotBonuses(bool inventory,int index){return inventory?build.InventorySkillBonuses[index]:build.GearSkillBonuses[index];}
 void PutBonuses(bool inventory,int index,ItemSkillBonuses value){if(inventory)build.InventorySkillBonuses[index]=value;else build.GearSkillBonuses[index]=value;}
 void PutSlot(bool inventory,int index,string value){if(inventory)build.Inventory[index]=value;else build.Gear[index]=value;if(String.IsNullOrWhiteSpace(value))PutBonuses(inventory,index,null);}
 bool VerifiedFits(ItemSkillBonuses item,bool inventory,int index){if(item==null||String.IsNullOrEmpty(item.VerifiedName)||inventory)return true;return VerifiedEquipment.FitsSlot(VerifiedEquipment.Find(item.VerifiedName),gearNames[index]);}
 void SelectSlot(bool inventory,int index){if(moving && selectedSlot>=0){string from=SlotText(selectedInventory,selectedSlot),to=SlotText(inventory,index);var fromStats=SlotBonuses(selectedInventory,selectedSlot);var toStats=SlotBonuses(inventory,index);if(!VerifiedFits(fromStats,inventory,index)||!VerifiedFits(toStats,selectedInventory,selectedSlot)){MessageBox.Show("A verified unique cannot be equipped in that slot.","Equipment slot");return;}PutSlot(selectedInventory,selectedSlot,to);PutSlot(inventory,index,from);PutBonuses(selectedInventory,selectedSlot,toStats);PutBonuses(inventory,index,fromStats);moving=false;Save();}selectedInventory=inventory;selectedSlot=index;ShowGear();}
 void EditSlot(){
  if(selectedSlot<0)return;bool warlock=className=="Warlock";var existing=SlotBonuses(selectedInventory,selectedSlot);
  if(existing!=null&&!String.IsNullOrEmpty(existing.VerifiedName)){if(selectedInventory){MessageBox.Show("Move this item to its equipment slot to choose a different preset.","Verified unique");return;}ChooseVerifiedUnique();return;}
  using(var dlg=new Form{Text="Edit "+(selectedInventory?"inventory cell":gearNames[selectedSlot]),Size=new Size(570,570),StartPosition=FormStartPosition.CenterParent,FormBorderStyle=FormBorderStyle.FixedDialog,MaximizeBox=false,MinimizeBox=false,BackColor=bg,ForeColor=text}){
   dlg.Controls.Add(LabelAt("ITEM NAME OR NOTES",20,20,500,22,9,gold,FontStyle.Bold));
   var entry=new TextBox{Location=new Point(20,48),Width=515,Text=SlotText(selectedInventory,selectedSlot)??"",BackColor=panel,ForeColor=text};dlg.Controls.Add(entry);
   ListBox bonus=null;CheckBox charm=null;
   dlg.Controls.Add(LabelAt("ITEM STATS  •  CHOOSE A STAT AND VALUE",20,85,500,22,9,gold,FontStyle.Bold));
   var choices=new ComboBox{Location=new Point(20,112),Width=300,DropDownStyle=ComboBoxStyle.DropDownList,BackColor=panel,ForeColor=text,AccessibleName="Item stat type"};
   int skillChoiceCount=0;
   if(warlock){choices.Items.AddRange(new object[]{"All Skills","Warlock Skills","Demon Skills","Eldritch Skills","Chaos Skills","Fire Skills"});foreach(var skill in skills)choices.Items.Add(skill.Name);skillChoiceCount=choices.Items.Count;}
   choices.Items.AddRange(EquipmentStatOptions.ForSlot(selectedInventory?"Inventory":gearNames[selectedSlot]));choices.SelectedIndex=0;dlg.Controls.Add(choices);
   var amount=new NumericUpDown{Location=new Point(330,112),Size=new Size(70,28),Minimum=warlock?1:-1000,Maximum=warlock?100:10000,Value=1,BackColor=panel,ForeColor=text,AccessibleName="Item stat amount"};dlg.Controls.Add(amount);
   choices.SelectedIndexChanged+=(s,e)=>{bool skill=choices.SelectedIndex<skillChoiceCount;amount.Minimum=skill?1:-1000;amount.Maximum=skill?100:10000;if(amount.Value==0)amount.Value=1;};
   bonus=new ListBox{Location=new Point(20,154),Size=new Size(380,134),BackColor=panel,ForeColor=text,AccessibleName="Item stats"};
   if(warlock&&existing!=null&&!String.IsNullOrWhiteSpace(existing.Lines))foreach(string line in existing.Lines.Replace("\r","").Split('\n'))if(!String.IsNullOrWhiteSpace(line))bonus.Items.Add(new ItemEditorLine{SkillLine=line.Trim()});
   if(existing!=null&&existing.OtherStats!=null)foreach(var stat in existing.OtherStats)if(stat!=null)bonus.Items.Add(new ItemEditorLine{Stat=new ItemStat{Name=stat.Name,Value=stat.Value}});
   dlg.Controls.Add(bonus);
   var addBonus=Btn("ADD",410,109,125,30);addBonus.Click+=(s,e)=>{if(choices.SelectedIndex<skillChoiceCount){if(amount.Value<1||amount.Value>100){MessageBox.Show("Skill bonus must be between 1 and 100.","Item stats");return;}bonus.Items.Add(new ItemEditorLine{SkillLine=choices.SelectedItem+": +"+amount.Value});}else bonus.Items.Add(new ItemEditorLine{Stat=new ItemStat{Name=choices.SelectedItem.ToString(),Value=(int)amount.Value}});};dlg.Controls.Add(addBonus);
   var removeBonus=Btn("REMOVE",410,154,125,30);removeBonus.Click+=(s,e)=>{if(bonus.SelectedIndex>=0)bonus.Items.RemoveAt(bonus.SelectedIndex);};dlg.Controls.Add(removeBonus);
   if(existing!=null&&!String.IsNullOrEmpty(existing.RunewordName)){var view=Btn("RUNEWORD",410,196,125,30);view.Click+=(s,e)=>{using(var info=new Form{Text=existing.RunewordName+" effects",Size=new Size(550,480),StartPosition=FormStartPosition.CenterParent,BackColor=bg}){var body=new TextBox{Dock=DockStyle.Fill,Multiline=true,ReadOnly=true,ScrollBars=ScrollBars.Vertical,BackColor=panel,ForeColor=text,Font=f(10),Text="RECIPE EFFECTS (ranges shown)\r\n\r\n"+(existing.RunewordEffects??"").Replace("\n","\r\n")+"\r\n\r\nOnly the skill and speed bonuses listed in the editor affect calculations. Other effects are reference text."};info.Controls.Add(body);info.ShowDialog(dlg);}};dlg.Controls.Add(view);}
   if(existing==null||String.IsNullOrEmpty(existing.RunewordName))dlg.Controls.Add(LabelAt("Select an entry to remove or replace it.",410,193,125,65,8,muted));
   int speedY=300;
   dlg.Controls.Add(LabelAt("ITEM SPEED BONUSES (%)",20,speedY,490,22,9,gold,FontStyle.Bold));
   string[] names={"FCR","FHR","FBR"};int[] stored={existing==null?0:existing.Fcr,existing==null?0:existing.Fhr,existing==null?0:existing.Fbr};var speeds=new NumericUpDown[3];
   for(int i=0;i<3;i++){int x=20+i*140;dlg.Controls.Add(LabelAt(names[i],x,speedY+25,105,20,9,muted));speeds[i]=new NumericUpDown{Location=new Point(x,speedY+47),Size=new Size(105,28),Minimum=0,Maximum=1000,Value=Math.Max(0,Math.Min(1000,stored[i])),BackColor=panel,ForeColor=text,AccessibleName=names[i]+" item bonus"};dlg.Controls.Add(speeds[i]);}
   if(selectedInventory){charm=new CheckBox{Location=new Point(20,speedY+83),Size=new Size(510,28),Text="Active charm in inventory",ForeColor=text,Checked=existing!=null&&existing.ActiveCharm};dlg.Controls.Add(charm);}
   dlg.Controls.Add(LabelAt("Skill and speed bonuses affect calculations. Other stats are saved for reference; manual items are not fully validated.",20,speedY+115,510,34,8,muted));
   var ok=Btn("SAVE",385,speedY+154,150,35);ok.Click+=(s,e)=>{
    string item=entry.Text.Trim();ItemSkillBonuses data=existing;
    string bonusLines=warlock?String.Join("\n",bonus.Items.Cast<ItemEditorLine>().Where(x=>x.Stat==null).Select(x=>x.SkillLine).ToArray()):(existing==null?"":existing.Lines);
    var otherStats=bonus.Items.Cast<ItemEditorLine>().Where(x=>x.Stat!=null).Select(x=>x.Stat).ToList();
    if(warlock){try{GearSkillBonuses.Parse(bonusLines,skills);}catch(Exception ex){if(ex is FormatException||ex is OverflowException){MessageBox.Show(ex.Message,"Skill bonuses");return;}throw;}
     if(item.Length==0&&!String.IsNullOrWhiteSpace(bonusLines)){MessageBox.Show("Enter an item name before adding bonuses.","Skill bonuses");return;}
    }
    if(item.Length==0&&(speeds.Any(n=>n.Value>0)||otherStats.Count>0)){MessageBox.Show("Enter an item name before adding bonuses.","Item bonuses");return;}
    data=new ItemSkillBonuses{Lines=bonusLines,ActiveCharm=selectedInventory?charm.Checked:existing!=null&&existing.ActiveCharm,Fcr=(int)speeds[0].Value,Fhr=(int)speeds[1].Value,Fbr=(int)speeds[2].Value,OtherStats=otherStats,RunewordName=existing==null?"":existing.RunewordName,RunewordBase=existing==null?"":existing.RunewordBase,RunewordEffects=existing==null?"":existing.RunewordEffects};
    PutSlot(selectedInventory,selectedSlot,item);PutBonuses(selectedInventory,selectedSlot,item.Length==0?null:data);Save();dlg.DialogResult=DialogResult.OK;dlg.Close();
   };dlg.Controls.Add(ok);dlg.AcceptButton=ok;dlg.ShowDialog(this);
  }ShowGear();
 }
 void SlotButton(Panel host,bool inv,int index,int x,int y,int w,int h,ToolTip tips){string value=SlotText(inv,index)??"";string label=inv?(String.IsNullOrEmpty(value)?"":(value.Length>3?value.Substring(0,3):value)):gearNames[index]+"\n"+(String.IsNullOrEmpty(value)?"[ empty ]":value);var b=Btn(label,x,y,w,h);var themed=b as GothicButton;if(themed!=null&&!inv){themed.GearSlot=true;themed.GlyphIndex=index;}b.Font=f(inv?8:8,FontStyle.Regular);b.BackColor=selectedSlot==index && selectedInventory==inv?Color.FromArgb(94,67,40):Color.FromArgb(44,41,40);b.FlatAppearance.BorderColor=Color.FromArgb(101,84,61);var entry=SlotBonuses(inv,index);string slotHelp=(inv?"Inventory cell "+(index+1):gearNames[index])+": "+(value==""?"empty":value);if(entry!=null){if(!String.IsNullOrWhiteSpace(entry.Lines))slotHelp+="\n"+entry.Lines;if(entry.OtherStats!=null)foreach(var stat in entry.OtherStats)if(stat!=null)slotHelp+="\n"+stat.Name+": "+(stat.Value>=0?"+":"")+stat.Value;if(entry.Fcr>0||entry.Fhr>0||entry.Fbr>0)slotHelp+="\nFCR: "+entry.Fcr+"%  FHR: "+entry.Fhr+"%  FBR: "+entry.Fbr+"%";if(inv)slotHelp+=entry.ActiveCharm?"\nActive charm":"\nInactive in inventory";}tips.SetToolTip(b,slotHelp);b.Click+=(s,e)=>SelectSlot(inv,index);host.Controls.Add(b);}
 void ShowGear(){Header("Character  /  inventory & equipment",className+" equipment","Select an item to edit its name and bonuses, or move it between slots.");
 content.Controls.Add(LabelAt("EQUIPMENT",32,153,300,23,10,gold,FontStyle.Bold));content.Controls.Add(LabelAt("INVENTORY  •  10 × 4",480,153,340,23,10,gold,FontStyle.Bold));var tips=slotTip;
 int[] xs={32,302,167,167,32,167,302,32,302,167};int[] ys={259,259,184,259,365,365,365,184,184,440};int[] ws={125,125,125,125,125,125,125,125,125,125};int[] hs={95,95,65,95,65,65,65,65,65,65};for(int i=0;i<10;i++)SlotButton(content,false,i,xs[i],ys[i],ws[i],hs[i],tips);
 for(int i=0;i<40;i++)SlotButton(content,true,i,480+(i%10)*36,184+(i/10)*52,34,49,tips);
 string caption=selectedSlot<0?"Select an equipment slot or inventory cell.":(selectedInventory?"INVENTORY CELL "+(selectedSlot+1):gearNames[selectedSlot].ToUpper())+"\n"+(String.IsNullOrEmpty(SlotText(selectedInventory,selectedSlot))?"Empty":SlotText(selectedInventory,selectedSlot));if(moving)caption="MOVE / SWAP\nSelect the destination slot or cell.";var summary=LabelAt(caption,480,405,360,64,10,text);content.Controls.Add(summary);
 var edit=Btn("EDIT ITEM",480,478,112,36);edit.Enabled=selectedSlot>=0&&!moving;edit.Click+=(s,e)=>EditSlot();content.Controls.Add(edit);var move=Btn(moving?"CANCEL":"MOVE / SWAP",600,478,112,36);move.Enabled=selectedSlot>=0;move.Click+=(s,e)=>{moving=!moving;ShowGear();};content.Controls.Add(move);var clear=Btn("CLEAR",720,478,112,36);clear.Enabled=selectedSlot>=0&&!moving&&!String.IsNullOrEmpty(SlotText(selectedInventory,selectedSlot));clear.Click+=(s,e)=>{if(MessageBox.Show("Clear this slot?","Sanctuary Atlas",MessageBoxButtons.YesNo)==DialogResult.Yes){PutSlot(selectedInventory,selectedSlot,"");Save();ShowGear();}};content.Controls.Add(clear);
 var breakpoints=Btn("BREAKPOINTS",480,610,170,36);breakpoints.Click+=(s,e)=>ShowBreakpoints();content.Controls.Add(breakpoints);var verified=Btn("UNIQUE PRESETS",660,610,172,36);verified.Enabled=selectedSlot>=0&&!selectedInventory&&!moving;verified.Click+=(s,e)=>ChooseVerifiedUnique();content.Controls.Add(verified);
 var rw=Btn("RUNEWORD LIBRARY",480,720,170,36);rw.Click+=(s,e)=>ShowWords();content.Controls.Add(rw);var overview=Btn("GEAR OVERVIEW",660,720,172,36);overview.Click+=(s,e)=>ShowGearOverview();content.Controls.Add(overview);
 content.Controls.Add(LabelAt("Enter speed bonuses on each item. Mark inventory charms active to include them. Item sizes and equipment compatibility are not checked.",480,534,360,78,9,muted));string active=className=="Warlock"?GearSkillBonuses.Summary(GearSkillBonuses.ForBuild(build,skills)):"Skill bonuses are currently supported for Warlocks.";var speedTotals=GearSpeedBonuses.ForBuild(build);content.Controls.Add(LabelAt("ACTIVE SPEED: "+speedTotals.Fcr+"% FCR  /  "+speedTotals.Fhr+"% FHR  /  "+speedTotals.Fbr+"% FBR\nSKILL BONUSES: "+active,32,665,810,64,9,muted));}
 void ShowGearOverview(){
  Header("Character  /  equipment","Equipment bonus overview","Review bonuses currently tracked from your equipped items and active charms.");
  var back=Btn("BACK TO EQUIPMENT",32,150,190,35);back.Click+=(s,e)=>ShowGear();content.Controls.Add(back);
  var view=new TextBox{Location=new Point(32,200),Size=new Size(818,450),Multiline=true,ReadOnly=true,ScrollBars=ScrollBars.Vertical,BackColor=panel,ForeColor=text,Font=f(11),AccessibleName="Equipment bonus overview",Text=GearOverview.Format(build,skills,className=="Warlock")};content.Controls.Add(view);
 }
 void ChooseVerifiedUnique(){
  if(selectedSlot<0||selectedInventory||moving)return;var options=VerifiedEquipment.ForSlot(gearNames[selectedSlot]);
  using(var dlg=new Form{Text="Unique item presets",Size=new Size(560,420),StartPosition=FormStartPosition.CenterParent,FormBorderStyle=FormBorderStyle.FixedDialog,MaximizeBox=false,MinimizeBox=false,BackColor=bg,ForeColor=text}){
   dlg.Controls.Add(LabelAt("UNSOCKETED UNIQUE ITEMS  •  "+gearNames[selectedSlot].ToUpperInvariant(),20,18,500,24,10,gold,FontStyle.Bold));
   dlg.Controls.Add(LabelAt("Choose a known item. The app fills its supported skill and speed bonuses. More items, sockets and other properties are still being added.",20,48,500,52,9,muted));
   var list=new ListBox{Location=new Point(20,110),Size=new Size(500,180),BackColor=panel,ForeColor=text,AccessibleName="Unique item presets"};
   foreach(var option in options)list.Items.Add(option.Name+"  •  "+option.Base);dlg.Controls.Add(list);
   if(options.Count==0)dlg.Controls.Add(LabelAt("No presets for this slot yet.",20,300,330,24,9,muted));
   var add=Btn("EQUIP",370,320,150,35);add.Enabled=options.Count>0;add.Click+=(s,e)=>{
    if(list.SelectedIndex<0){MessageBox.Show("Choose an item first.","Unique item presets");return;}
    var record=options[list.SelectedIndex];if(!String.IsNullOrWhiteSpace(SlotText(false,selectedSlot))&&MessageBox.Show("Replace the item in "+gearNames[selectedSlot]+"?","Replace gear",MessageBoxButtons.YesNo)!=DialogResult.Yes)return;
    PutSlot(false,selectedSlot,record.Name);PutBonuses(false,selectedSlot,VerifiedEquipment.Bonuses(record));Save();dlg.DialogResult=DialogResult.OK;dlg.Close();
   };dlg.Controls.Add(add);dlg.AcceptButton=add;dlg.ShowDialog(this);
  }ShowGear();
 }
 bool updateChecking=false;
 Label updateStatus;
 Button updateButton;
 string pendingInstaller;
 void SetUpdateStatus(string message,bool busy){if(updateStatus!=null&&!updateStatus.IsDisposed)updateStatus.Text=message;if(updateButton!=null&&!updateButton.IsDisposed){updateButton.Enabled=!busy;updateButton.Text=pendingInstaller==null?"CHECK FOR UPDATES":"RESTART TO UPDATE";}}
 void AddUpdateCorner(Panel side){
  var box=new Panel{Dock=DockStyle.Bottom,Height=76,BackColor=Color.FromArgb(25,19,16)};
  updateStatus=LabelAt("UPDATE "+AtlasUpdates.CurrentVersion+"  •  READY",12,4,178,19,8,gold,FontStyle.Bold);box.Controls.Add(updateStatus);
  updateButton=Btn("CHECK FOR UPDATES",12,27,145,34);updateButton.Click+=(s,e)=>{if(pendingInstaller!=null)ApplyPendingUpdate();else CheckForUpdates(true);};box.Controls.Add(updateButton);
  var settings=Btn("...",162,27,31,34);settings.AccessibleName="Update settings";slotTip.SetToolTip(settings,"Update settings");settings.Click+=(s,e)=>ShowUpdates();box.Controls.Add(settings);side.Controls.Add(box);box.BringToFront();
 }
 string UpdateRepositoryPath(){return Path.Combine(dataDir,"update-repository.txt");}
 string UpdateRepository(){try{return File.Exists(UpdateRepositoryPath())?File.ReadAllText(UpdateRepositoryPath()).Trim():AtlasUpdates.DefaultRepository;}catch{return "";}}
 void ShowUpdates(){Header("Sanctuary Atlas  /  updates","App updates","Get new versions without reinstalling by hand once a release source is connected.");
  content.Controls.Add(LabelAt("RELEASE SOURCE",32,155,350,24,10,gold,FontStyle.Bold));
  content.Controls.Add(LabelAt("Enter a GitHub repository as owner/repository. Leave blank to turn off update checks.",32,185,760,48,10,text));
  var repository=new TextBox{Location=new Point(32,240),Width=390,Text=UpdateRepository(),BackColor=panel,ForeColor=text};content.Controls.Add(repository);
  var save=Btn("SAVE SOURCE",32,282,180,36);content.Controls.Add(save);
  var check=Btn("CHECK FOR UPDATES",220,282,200,36);content.Controls.Add(check);
  var info=LabelAt(UpdateRepository()==""?"Automatic update checks are off. Save a public release repository to enable them.":"The app checks this repository when it starts. Guide, runeword and skill-tree content can refresh immediately. Releases with an installer update the program and restart the app. The repository and releases must be public.",32,340,780,85,10,muted);content.Controls.Add(info);
  save.Click+=(sender,e)=>{string value=repository.Text.Trim();if(value!=""&&!AtlasUpdates.ValidRepository(value)){MessageBox.Show("Enter the repository as owner/repository.","App updates");return;}Directory.CreateDirectory(dataDir);if(value==""){File.WriteAllText(UpdateRepositoryPath(),"");info.Text="Automatic update checks are off.";}else{File.WriteAllText(UpdateRepositoryPath(),value);info.Text="Release source saved. The app will check when it starts.";}};
  check.Click+=(sender,e)=>{string value=repository.Text.Trim();if(value!=UpdateRepository()){MessageBox.Show("Save the release source first.","App updates");return;}CheckForUpdates(true);};
 }
 void CheckForUpdates(bool manual){
  string repo=UpdateRepository();if(repo==""){SetUpdateStatus("UPDATE CHECKS OFF",false);if(manual)MessageBox.Show("No release source is connected yet. Add one in App updates.","App updates");return;}
  if(!AtlasUpdates.ValidRepository(repo)){SetUpdateStatus("INVALID UPDATE SOURCE",false);if(manual)MessageBox.Show("The saved release source is invalid.","App updates");return;}
  if(updateChecking)return;updateChecking=true;SetUpdateStatus("CHECKING FOR UPDATES...",true);
  var thread=new System.Threading.Thread(()=>{try{
   UpdateRelease release=AtlasUpdates.Fetch(repo);
   if(IsDisposed||!IsHandleCreated)return;
   BeginInvoke((Action)(()=>{if(IsDisposed)return;
    if(AtlasUpdates.IsProgramUpdate(release)){if(manual){SetUpdateStatus("DOWNLOADING UPDATE "+release.Version+"...",true);DownloadUpdate(release);}else{updateChecking=false;SetUpdateStatus("UPDATE "+release.Version+" AVAILABLE",false);}return;}
    if(release.ContentHash!=null&&!String.Equals(release.ContentHash,contentHash,StringComparison.OrdinalIgnoreCase)){if(manual){SetUpdateStatus("UPDATING CONTENT...",true);DownloadWordContent(release);}else{updateChecking=false;SetUpdateStatus("CONTENT UPDATE AVAILABLE",false);}return;}
    updateChecking=false;SetUpdateStatus("UP TO DATE  •  UPDATE "+AtlasUpdates.CurrentVersion,false);
   }));
  }catch(Exception ex){if(!IsDisposed&&IsHandleCreated)try{BeginInvoke((Action)(()=>{updateChecking=false;SetUpdateStatus("UPDATE CHECK FAILED",false);if(manual&&!IsDisposed)MessageBox.Show("Could not check for updates: "+ex.Message,"App updates");}));}catch(InvalidOperationException){}}});
  thread.IsBackground=true;thread.Start();
 }
 void DownloadUpdate(UpdateRelease release){
  var thread=new System.Threading.Thread(()=>{string file=null;try{file=AtlasUpdates.Download(release);if(IsDisposed||!IsHandleCreated){try{File.Delete(file);}catch{}return;}string downloaded=file;BeginInvoke((Action)(()=>{updateChecking=false;pendingInstaller=downloaded;SetUpdateStatus("UPDATE "+release.Version+" READY",false);ApplyPendingUpdate();}));file=null;}
  catch(Exception ex){if(file!=null)try{File.Delete(file);}catch{}if(!IsDisposed&&IsHandleCreated)try{BeginInvoke((Action)(()=>{updateChecking=false;SetUpdateStatus("UPDATE DOWNLOAD FAILED",false);MessageBox.Show("Could not download the update: "+ex.Message,"App updates");}));}catch(InvalidOperationException){}}});thread.IsBackground=true;thread.Start();
 }
 void ApplyPendingUpdate(){if(pendingInstaller==null)return;try{System.Diagnostics.Process.Start(pendingInstaller,"--apply-update "+System.Diagnostics.Process.GetCurrentProcess().Id);pendingInstaller=null;Close();}catch(Exception ex){SetUpdateStatus("UPDATE READY  •  RETRY",false);MessageBox.Show("Could not start the update: "+ex.Message,"App updates");}}
 void DownloadWordContent(UpdateRelease release){
  var thread=new System.Threading.Thread(()=>{try{byte[] data=AtlasUpdates.DownloadContent(release);if(IsDisposed||!IsHandleCreated)return;BeginInvoke((Action)(()=>{try{ApplyWordContent(data);updateChecking=false;SetUpdateStatus("CONTENT UPDATED",false);}catch(Exception ex){updateChecking=false;SetUpdateStatus("CONTENT UPDATE FAILED",false);MessageBox.Show("Could not apply updated content: "+ex.Message,"App updates");}}));}
   catch(Exception ex){if(!IsDisposed&&IsHandleCreated)try{BeginInvoke((Action)(()=>{updateChecking=false;SetUpdateStatus("CONTENT UPDATE FAILED",false);MessageBox.Show("Could not download updated content: "+ex.Message,"App updates");}));}catch(InvalidOperationException){}}});thread.IsBackground=true;thread.Start();
 }

 // Optional bonuses outside equipment are scratch values for the current session.
 readonly Dictionary<string,int[]> breakpointTotals=new Dictionary<string,int[]>();
 void ShowBreakpoints(){
  Header("Character  /  equipment",className+" breakpoints","Your entered equipment bonuses count automatically. Add other bonuses if needed.");
  var back=Btn("BACK TO EQUIPMENT",32,145,185,34);back.Click+=(s,e)=>ShowGear();content.Controls.Add(back);
  int[] values;if(!breakpointTotals.TryGetValue(className,out values)){values=new int[3];breakpointTotals[className]=values;}
  var equipment=GearSpeedBonuses.ForBuild(build);int[] gear={equipment.Fcr,equipment.Fhr,equipment.Fbr};
  string[] kinds={"FCR","FHR","FBR"};
  for(int i=0;i<kinds.Length;i++){
   int index=i,y=195+i*150;var table=Breakpoints.For(className,kinds[i]);
   content.Controls.Add(LabelAt(kinds[i]+"  /  "+table.Name.ToUpperInvariant(),32,y,520,24,10,gold,FontStyle.Bold));
   content.Controls.Add(LabelAt("Gear: "+gear[i]+"%",32,y+33,150,24,10,text));
   content.Controls.Add(LabelAt("Extra: ",190,y+33,62,24,9,muted));
   var input=new NumericUpDown{Location=new Point(248,y+30),Size=new Size(90,28),Minimum=0,Maximum=9999,Value=values[i],BackColor=panel,ForeColor=text,AccessibleName=kinds[i]+" extra percentage"};content.Controls.Add(input);
   var result=LabelAt("Total: "+(gear[i]+values[i])+"%  |  "+table.Describe(gear[i]+values[i]),360,y+25,480,47,10,text);result.AccessibleName=kinds[i]+" breakpoint result";content.Controls.Add(result);
   input.ValueChanged+=(s,e)=>{values[index]=(int)input.Value;result.Text="Total: "+(gear[index]+values[index])+"%  |  "+table.Describe(gear[index]+values[index]);};
   content.Controls.Add(LabelAt("Targets: "+table.Steps(),32,y+75,820,32,9,muted));
   content.Controls.Add(LabelAt(table.Assumption,32,y+110,820,27,8,muted));
  }
  content.Controls.Add(LabelAt("Gear totals include equipped items and active charms. Extra is optional and kept for this session. Enter item stats in Equipment; names alone do not supply stats. IAS depends on weapon and skill.",32,660,820,55,9,muted));
  content.Controls.Add(LabelAt("Reference: d2r.org/tools/breakpoints  •  game animation data 3.3.93847",32,720,820,25,8,muted));
 }
 void ShowShoppingGuide(){
  Header("The horadric archive  /  shopping","Shopping & gambling","Choose a target to see useful levels, item bases and practical tips.");
  content.Controls.Add(LabelAt("CHARACTER LEVEL",32,150,150,23,9,gold,FontStyle.Bold));
  var targetLevel=new NumericUpDown{Location=new Point(32,175),Size=new Size(95,28),Minimum=1,Maximum=99,Value=Math.Max(1,Math.Min(99,build.Level)),BackColor=panel,ForeColor=text,AccessibleName="Shopping guide character level"};
  content.Controls.Add(targetLevel);
  content.Controls.Add(LabelAt("Starts at your planner level. Change it here to compare another character; your build is not changed.",145,175,700,39,9,muted));
  content.Controls.Add(LabelAt("WHAT ARE YOU LOOKING FOR?",32,220,340,24,9,gold,FontStyle.Bold));
  var topics=new ListBox{Location=new Point(32,250),Size=new Size(265,425),BackColor=panel,ForeColor=text,BorderStyle=BorderStyle.None,Font=f(10),IntegralHeight=false,AccessibleName="Shopping guide topics"};
  var notes=new TextBox{Location=new Point(315,250),Size=new Size(530,425),Multiline=true,ReadOnly=true,ScrollBars=ScrollBars.Vertical,BackColor=panel,ForeColor=text,BorderStyle=BorderStyle.FixedSingle,Font=f(10),AccessibleName="Shopping guide details"};
  Action refresh=()=>{var topic=topics.SelectedItem as ShoppingTopic;notes.Text=topic==null?"":topic.Details((int)targetLevel.Value);};
  foreach(var topic in ShoppingGuide.Topics)topics.Items.Add(topic);
  topics.SelectedIndexChanged+=(s,e)=>refresh();targetLevel.ValueChanged+=(s,e)=>refresh();
  content.Controls.Add(topics);content.Controls.Add(notes);if(topics.Items.Count>0)topics.SelectedIndex=0;
 }
 void ShowCuratedBuilds(){
  Header("Character  /  build library","Curated builds","Browse Warlock builds and scroll the guide for progression and equipment. Your build is never changed.");
  content.Controls.Add(LabelAt("WARLOCK BUILDS",32,152,360,24,10,gold,FontStyle.Bold));
  var list=new ListBox{Location=new Point(32,184),Size=new Size(265,410),BackColor=panel,ForeColor=text,BorderStyle=BorderStyle.None,Font=f(11),IntegralHeight=false};
  var details=new TextBox{Location=new Point(315,236),Size=new Size(530,418),Multiline=true,ReadOnly=true,ScrollBars=ScrollBars.Vertical,BackColor=panel,ForeColor=text,BorderStyle=BorderStyle.FixedSingle,Font=f(10)};
  var checkpointLabel=LabelAt("LEVELING CHECKPOINT",315,184,150,24,8,gold,FontStyle.Bold);
  var checkpoints=new ComboBox{Location=new Point(470,183),Size=new Size(135,28),DropDownStyle=ComboBoxStyle.DropDownList,BackColor=panel,ForeColor=text,AccessibleName="Build progression checkpoint"};
  var guide=Btn("FULL GUIDE",615,181,113,32);
  var preview=Btn("PREVIEW LEVEL",735,181,110,32);
  bool showCheckpoint=false;
  Action refresh=()=>{var selected=list.SelectedItem as CuratedBuild;var point=checkpoints.SelectedItem as BuildCheckpoint;
   details.Text=selected==null?"":showCheckpoint&&point!=null?CuratedProgression.Preview(point):selected.Details;
   guide.Enabled=selected!=null;preview.Enabled=point!=null;
  };
  foreach(var item in CuratedBuilds.Warlock)list.Items.Add(item);
  list.SelectedIndexChanged+=(s,e)=>{
   showCheckpoint=false;checkpoints.Items.Clear();
   foreach(var point in CuratedProgression.For(list.SelectedItem as CuratedBuild))checkpoints.Items.Add(point);
   checkpoints.Enabled=checkpoints.Items.Count>0;
   if(checkpoints.Items.Count>0)checkpoints.SelectedIndex=0;
   refresh();
  };
  checkpoints.SelectedIndexChanged+=(s,e)=>{if(showCheckpoint)refresh();};
  guide.Click+=(s,e)=>{showCheckpoint=false;refresh();};
  preview.Click+=(s,e)=>{showCheckpoint=true;refresh();};
  content.Controls.Add(list);content.Controls.Add(details);content.Controls.Add(checkpointLabel);content.Controls.Add(checkpoints);content.Controls.Add(guide);content.Controls.Add(preview);
  if(list.Items.Count>0)list.SelectedIndex=0;
  content.Controls.Add(LabelAt("More classes and builds can be added over time.",32,606,265,50,9,muted));
 }

 void ShowItemLibrary(){
  Header("The horadric archive  /  equipment","Item library","Search verified unique presets and runewords by name, base, rune or supported effect.");
  var query=new TextBox{Location=new Point(32,155),Width=260,BackColor=panel,ForeColor=text,AccessibleName="Search item library"};
  var kind=new ComboBox{Location=new Point(304,155),Width=130,DropDownStyle=ComboBoxStyle.DropDownList,AccessibleName="Item category"};kind.Items.AddRange(new object[]{"All","Uniques","Runewords"});kind.SelectedIndex=0;
  var list=new ListBox{Location=new Point(32,192),Size=new Size(402,410),BackColor=panel,ForeColor=text,Font=f(10),IntegralHeight=false,AccessibleName="Item library results"};
  var info=new TextBox{Location=new Point(450,192),Size=new Size(400,340),Multiline=true,ReadOnly=true,ScrollBars=ScrollBars.Vertical,BackColor=panel,ForeColor=text,Font=f(10),AccessibleName="Item library information"};
  var slots=new ComboBox{Location=new Point(450,548),Width=180,DropDownStyle=ComboBoxStyle.DropDownList,AccessibleName="Equipment destination"};
  var equip=Btn("EQUIP PRESET",650,544,200,32);var open=Btn("VIEW RUNEWORD",650,584,200,32);
  Action selection=()=>{var item=list.SelectedItem as ItemLibraryEntry;info.Text=ItemLibrary.Description(item);slots.Items.Clear();if(item!=null&&item.Unique!=null)foreach(string s in gearNames)if(VerifiedEquipment.FitsSlot(item.Unique,s))slots.Items.Add(s);if(slots.Items.Count>0)slots.SelectedIndex=0;slots.Enabled=slots.Items.Count>0;equip.Enabled=slots.Items.Count>0;open.Enabled=item!=null&&item.Runeword!=null;};
  Action filter=()=>{list.BeginUpdate();list.Items.Clear();foreach(var item in ItemLibrary.Search(words,query.Text,(string)kind.SelectedItem))list.Items.Add(item);list.EndUpdate();if(list.Items.Count>0)list.SelectedIndex=0;else selection();};
  query.TextChanged+=(s,e)=>filter();kind.SelectedIndexChanged+=(s,e)=>filter();list.SelectedIndexChanged+=(s,e)=>selection();
  equip.Click+=(s,e)=>{var item=list.SelectedItem as ItemLibraryEntry;if(item==null||item.Unique==null||slots.SelectedItem==null)return;int i=Array.IndexOf(gearNames,(string)slots.SelectedItem);if(i<0||!VerifiedEquipment.FitsSlot(item.Unique,gearNames[i]))return;if(!String.IsNullOrWhiteSpace(build.Gear[i])&&MessageBox.Show("Replace the item in "+gearNames[i]+"?","Replace gear",MessageBoxButtons.YesNo)!=DialogResult.Yes)return;PutSlot(false,i,item.Unique.Name);PutBonuses(false,i,VerifiedEquipment.Bonuses(item.Unique));selectedSlot=i;selectedInventory=false;moving=false;Save();ShowGear();};
  open.Click+=(s,e)=>{var item=list.SelectedItem as ItemLibraryEntry;if(item==null||item.Runeword==null)return;string name=item.Name;ShowWords();search.Text=name;};
  content.Controls.Add(query);content.Controls.Add(kind);content.Controls.Add(list);content.Controls.Add(info);content.Controls.Add(slots);content.Controls.Add(equip);content.Controls.Add(open);
  content.Controls.Add(LabelAt("Starting collection: 14 unique presets plus the runeword library. Unique entries show only the supported bonuses.",32,630,818,48,9,muted));filter();
 }
 void ShowWords(){Header("The horadric archive  /  99 entries","Runeword library",className+" stash  •  Search by name, rune, base or effect. Select a recipe for its details.");search=new TextBox{Location=new Point(32,155),Width=390,BackColor=panel,ForeColor=text,BorderStyle=BorderStyle.FixedSingle};search.TextChanged+=(s,e)=>FilterWords();content.Controls.Add(search); craftable=new CheckBox{Text="Only runes I have",Location=new Point(450,153),Size=new Size(240,27),ForeColor=text};craftable.CheckedChanged+=(s,e)=>FilterWords();content.Controls.Add(craftable);var add=Btn("ADD TO GEAR",700,150,150,31);add.Click+=(s,e)=>AddToGear();content.Controls.Add(add);results=new ListBox{Location=new Point(32,190),Size=new Size(390,440),BackColor=panel,ForeColor=text,BorderStyle=BorderStyle.None,Font=f(11),IntegralHeight=false};results.SelectedIndexChanged+=(s,e)=>ShowWordDetail();content.Controls.Add(results);detail=new TextBox{Location=new Point(450,190),Size=new Size(400,440),Multiline=true,ReadOnly=true,BackColor=panel,ForeColor=text,BorderStyle=BorderStyle.None,Font=f(11),ScrollBars=ScrollBars.Vertical};content.Controls.Add(detail);FilterWords();}
 void AddToGear(){
  if(results.SelectedItem==null)return;var word=words.Find(x=>x.Name==(string)results.SelectedItem);if(word==null)return;
  var slots=gearNames.Where(x=>RunewordGear.Fits(word,x)).ToArray();if(slots.Length==0){MessageBox.Show("The listed bases do not match a supported equipment slot.","Runeword gear");return;}
  using(var dlg=new Form{Text="Create "+word.Name,Size=new Size(720,690),StartPosition=FormStartPosition.CenterParent,FormBorderStyle=FormBorderStyle.FixedDialog,MaximizeBox=false,MinimizeBox=false,BackColor=bg,ForeColor=text}){
   dlg.Controls.Add(LabelAt("CREATE RUNEWORD EQUIPMENT",20,16,650,24,10,gold,FontStyle.Bold));
   dlg.Controls.Add(LabelAt(word.Name+"   •   "+word.Runes,20,43,650,24,10,text));
   dlg.Controls.Add(LabelAt("Allowed bases: "+word.Base,20,68,660,38,9,muted));
   var slot=new ComboBox{Location=new Point(20,109),Width=200,DropDownStyle=ComboBoxStyle.DropDownList,BackColor=panel,ForeColor=text,AccessibleName="Runeword equipment slot"};slot.Items.AddRange(slots);dlg.Controls.Add(slot);
   dlg.Controls.Add(LabelAt("Select the equipment slot and enter the rolled values for supported bonuses.",240,107,430,40,9,muted));
   var rollPanel=new Panel{Location=new Point(20,145),Size=new Size(650,130),AutoScroll=true,BackColor=panel};dlg.Controls.Add(rollPanel);
   var desc=new TextBox{Location=new Point(20,288),Size=new Size(650,225),Multiline=true,ReadOnly=true,ScrollBars=ScrollBars.Vertical,BackColor=panel,ForeColor=text,Font=f(9),AccessibleName="Runeword effects"};dlg.Controls.Add(desc);
   var inputs=new List<NumericUpDown>();
   Action refresh=()=>{rollPanel.Controls.Clear();inputs.Clear();string chosen=(string)slot.SelectedItem;var rolls=RunewordGear.Rolls(word,chosen,className=="Warlock"?skills:null);int y=5;foreach(var r in rolls){rollPanel.Controls.Add(LabelAt(r.Label,6,y+3,500,26,9,text));var n=new NumericUpDown{Location=new Point(520,y),Size=new Size(105,25),Minimum=r.Min,Maximum=r.Max,Value=r.Min,Enabled=r.Min!=r.Max,BackColor=panel,ForeColor=text,AccessibleName=r.Key+" runeword roll"};rollPanel.Controls.Add(n);inputs.Add(n);y+=30;}if(rolls.Count==0)rollPanel.Controls.Add(LabelAt("No bonuses currently used by the skill or speed calculators.",6,5,600,40,9,muted));desc.Text=String.Join("\r\n",RunewordGear.Effects(word,chosen).ToArray());};
   slot.SelectedIndexChanged+=(s,e)=>refresh();slot.SelectedIndex=0;
   dlg.Controls.Add(LabelAt("Full recipe effects are shown above. Other variable effects and base-item bonuses are not calculated. The base type, item requirements, and game mode still need checking in game.",20,522,650,54,9,muted));
   var equip=Btn("EQUIP",520,585,150,36);equip.Click+=(s,e)=>{int index=Array.IndexOf(gearNames,(string)slot.SelectedItem);if(index<0)return;if(!String.IsNullOrWhiteSpace(build.Gear[index])&&MessageBox.Show("Replace the current entry in "+gearNames[index]+"?","Replace gear",MessageBoxButtons.YesNo)!=DialogResult.Yes)return;var data=RunewordGear.Create(word,gearNames[index],className=="Warlock"?skills:null,inputs.Select(x=>(int)x.Value).ToArray());PutSlot(false,index,word.Name);PutBonuses(false,index,data);Save();dlg.DialogResult=DialogResult.OK;dlg.Close();};dlg.Controls.Add(equip);dlg.AcceptButton=equip;dlg.ShowDialog(this);
  }
 }
 void FilterWords(){string q=search.Text.ToLowerInvariant();results.Items.Clear();foreach(var w in words.Where(x=>(x.Name+" "+x.Runes+" "+x.Base+" "+x.Effects).ToLowerInvariant().Contains(q) && (!craftable.Checked || Missing(x)=="")))results.Items.Add(w.Name);if(results.Items.Count>0)results.SelectedIndex=0;else detail.Text="No matching runewords.";}
 void ShowWordDetail(){if(results.SelectedItem==null)return;var w=words.Find(x=>x.Name==(string)results.SelectedItem); detail.Text=w.Name.ToUpper()+"\r\n\r\nRUNE ORDER\r\n"+w.Runes+"\r\n\r\nELIGIBLE BASE\r\n"+w.Base+"\r\n\r\nEFFECTS\r\n"+w.Effects+"\r\n\r\nRUNE STASH\r\n"+(Missing(w)==""?"You have the runes for one copy.":"Missing: "+Missing(w))+"\r\n\r\n"+w.Note+"\r\n\r\nUse a non-magical socketed item with exactly the listed number of sockets. Insert the runes in the order shown. Expansion, patch, and ladder rules can affect availability; verify in your game before using rare runes. The rune-stash check does not check your base or game mode.";}
 
 string Missing(Word w){var needed=w.Runes.Split(new string[]{" + "},StringSplitOptions.None).GroupBy(x=>x); var missing=new List<string>();foreach(var g in needed){int i=Array.IndexOf(runeNames,g.Key);int own=i<0?0:build.Runes[i];int n=g.Count()-own;if(n>0)missing.Add(n+" x "+g.Key);}return String.Join(", ",missing.ToArray());}
 void ShowRunes(){Header("Materials  /  your rune stash","Rune inventory",""+className+" build  •  Enter quantities you own. The runeword library can filter by these counts.");for(int i=0;i<33;i++){int k=i;int col=i/11,row=i%11,x=32+col*270,y=153+row*45;content.Controls.Add(LabelAt((i+1).ToString("00")+"   "+runeNames[i],x,y+3,140,25,10,text,FontStyle.Bold));var n=new NumericUpDown{Location=new Point(x+150,y),Size=new Size(72,28),Minimum=0,Maximum=999,Value=Math.Min(999,Math.Max(0,build.Runes[i])),BackColor=panel,ForeColor=text};n.ValueChanged+=(s,e)=>{build.Runes[k]=(int)n.Value;Save();};content.Controls.Add(n);}content.Controls.Add(LabelAt("Counts are saved locally per class. Filters check runes only, not the base or edition.",32,661,800,30,9,muted));}
 static readonly string[] upgradeGems={"","","","","","","","","","Chipped Topaz","Chipped Amethyst","Chipped Sapphire","Chipped Ruby","Chipped Emerald","Chipped Diamond","Flawed Topaz","Flawed Amethyst","Flawed Sapphire","Flawed Ruby","Flawed Emerald","Flawed Diamond","Topaz","Amethyst","Sapphire","Ruby","Emerald","Diamond","Flawless Topaz","Flawless Amethyst","Flawless Sapphire","Flawless Ruby","Flawless Emerald"};
 static string RuneUpgradePlan(int start,int target,int quantity){
  if(start<0||target>=runeNames.Length||start>=target||quantity<1||quantity>999)return "Choose a starting rune below the target rune.";
  long[] need=new long[33];need[target]=quantity;
  for(int i=target-1;i>=start;i--)need[i]=checked(need[i+1]*(i<20?3L:2L));
  var lines=new List<string>();lines.Add("START WITH  "+need[start].ToString("N0")+" x "+runeNames[start]);lines.Add("");lines.Add("TRANSMUTE IN ORDER");
  var gems=new Dictionary<string,long>();for(int i=start;i<target;i++){long operations=need[i+1];string gem=upgradeGems[i];lines.Add(need[i].ToString("N0")+" x "+runeNames[i]+(gem==""?"":" + "+operations.ToString("N0")+" x "+gem)+"  →  "+operations.ToString("N0")+" x "+runeNames[i+1]);if(gem!="")gems[gem]=operations;}
  lines.Add("");lines.Add("TOTAL GEMS NEEDED");if(gems.Count==0)lines.Add("None");else foreach(var gem in gems)lines.Add(gem.Value.ToString("N0")+" x "+gem.Key);
  lines.Add("");lines.Add("This plan starts with one rune type and does not use your saved stash. Each transmute uses one gem per recipe, when listed.");return String.Join("\r\n",lines.ToArray());
 }
 void ShowRuneCalculator(){
  Header("The horadric archive  /  rune upgrades","Rune upgrade calculator","Plan a chain of cube recipes from one rune to a higher rune.");
  var back=Btn("←  CUBE RECIPES",650,142,200,34);back.Click+=(s,e)=>ShowCube();content.Controls.Add(back);
  content.Controls.Add(LabelAt("STARTING RUNE",32,155,200,22,9,gold,FontStyle.Bold));content.Controls.Add(LabelAt("TARGET RUNE",260,155,200,22,9,gold,FontStyle.Bold));content.Controls.Add(LabelAt("TARGET QUANTITY",488,155,160,22,9,gold,FontStyle.Bold));
  var start=new ComboBox{Location=new Point(32,181),Width=200,DropDownStyle=ComboBoxStyle.DropDownList,Font=f(10)};var target=new ComboBox{Location=new Point(260,181),Width=200,DropDownStyle=ComboBoxStyle.DropDownList,Font=f(10)};start.Items.AddRange(runeNames);target.Items.AddRange(runeNames);StyleCombo(start);StyleCombo(target);
  var amount=new NumericUpDown{Location=new Point(488,181),Width=110,Minimum=1,Maximum=999,Value=1,BackColor=panel,ForeColor=text};var result=new TextBox{Location=new Point(32,230),Size=new Size(818,410),Multiline=true,ReadOnly=true,BackColor=panel,ForeColor=text,BorderStyle=BorderStyle.FixedSingle,Font=f(10),ScrollBars=ScrollBars.Vertical};content.Controls.Add(start);content.Controls.Add(target);content.Controls.Add(amount);content.Controls.Add(result);
  Action refresh=()=>{if(start.SelectedIndex>=0&&target.SelectedIndex>=0)result.Text=RuneUpgradePlan(start.SelectedIndex,target.SelectedIndex,(int)amount.Value);};start.SelectedIndexChanged+=(s,e)=>refresh();target.SelectedIndexChanged+=(s,e)=>refresh();amount.ValueChanged+=(s,e)=>refresh();start.SelectedIndex=9;target.SelectedIndex=10;refresh();
 }
 void C(string n,string f,string note){cubes.Add(new Cube(n,f,note));}
 void InitCube(){
 string socketNote="Uses a normal-quality, unsocketed base; low-quality and superior items do not work. Ethereal items can be used. Socket count is random and capped by the base and item level.";
 C("Socket a body armor","Tal + Thul + Perfect Topaz + normal body armor",socketNote);C("Socket a weapon","Ral + Amn + Perfect Amethyst + normal weapon",socketNote);C("Socket a helm","Ral + Thul + Perfect Sapphire + normal helm",socketNote);C("Socket a shield","Tal + Amn + Perfect Ruby + normal shield",socketNote);
 C("Clear sockets","Hel + Scroll of Town Portal + socketed item","Destroys all socketed runes, gems and jewels. The base and its sockets remain."); C("Repair weapon","Ort + weapon","Restores durability. Ethereal items cannot be repaired this way.");C("Repair armor","Ral + armor","Restores durability. Ethereal items cannot be repaired this way.");C("Repair and recharge weapon","Ort + any chipped gem + weapon","Repairs and recharges; ethereal items cannot be repaired this way.");C("Repair and recharge armor","Ral + any flawed gem + armor","Repairs and recharges; ethereal items cannot be repaired this way.");
 C("Upgrade a gem","3 gems of the same type and grade","Creates one gem of the same type at the next grade, up to perfect. Skulls follow the same pattern.");C("Full rejuvenation potion","3 rejuvenation potions","Creates one full rejuvenation potion.");C("Rejuvenation potion","3 health potions + 3 mana potions + 1 chipped gem or skull","Creates one rejuvenation potion.");
 C("Upgrade normal unique weapon","Ral + Sol + Perfect Emerald + normal unique weapon","Upgrades the base to exceptional, keeping its unique properties. Check edition and mode rules.");C("Upgrade exceptional unique weapon","Lum + Pul + Perfect Emerald + exceptional unique weapon","Upgrades the base to elite, keeping its unique properties. Check edition and mode rules.");C("Upgrade normal unique armor","Tal + Shael + Perfect Diamond + normal unique armor","Upgrades the base to exceptional, keeping its unique properties. Check edition and mode rules.");C("Upgrade exceptional unique armor","Ko + Lem + Perfect Diamond + exceptional unique armor","Upgrades the base to elite, keeping its unique properties. Check edition and mode rules.");
 C("Upgrade normal rare weapon","Ort + Amn + Perfect Sapphire + normal rare weapon","Upgrades the base to exceptional. Check edition and mode rules.");C("Upgrade exceptional rare weapon","Fal + Um + Perfect Sapphire + exceptional rare weapon","Upgrades the base to elite. Check edition and mode rules.");C("Upgrade normal rare armor","Ral + Thul + Perfect Amethyst + normal rare armor","Upgrades the base to exceptional. Check edition and mode rules.");C("Upgrade exceptional rare armor","Ko + Pul + Perfect Amethyst + exceptional rare armor","Upgrades the base to elite. Check edition and mode rules.");
 for(int i=0;i<32;i++){int count=i<20?3:2;C("Upgrade "+runeNames[i]+" to "+runeNames[i+1],count+" x "+runeNames[i]+(upgradeGems[i]==""?"":" + "+upgradeGems[i]),"Creates one "+runeNames[i+1]+" rune. Availability can vary by game edition and mode.");}
 }
 void ShowCube(){Header("The horadric archive  /  "+cubes.Count+" entries","Cube recipes","Search by material or result. Read each recipe's restrictions before transmuting.");var q=new TextBox{Location=new Point(32,155),Width=390,BackColor=panel,ForeColor=text,BorderStyle=BorderStyle.FixedSingle};var list=new ListBox{Location=new Point(32,190),Size=new Size(390,440),BackColor=panel,ForeColor=text,BorderStyle=BorderStyle.None,Font=f(10),IntegralHeight=false};var info=new TextBox{Location=new Point(450,190),Size=new Size(400,440),Multiline=true,ReadOnly=true,BackColor=panel,ForeColor=text,BorderStyle=BorderStyle.None,Font=f(11),ScrollBars=ScrollBars.Vertical};content.Controls.Add(q);content.Controls.Add(list);content.Controls.Add(info);Action filter=()=>{list.Items.Clear();foreach(var c in cubes.Where(x=>(x.Name+" "+x.Formula).ToLowerInvariant().Contains(q.Text.ToLowerInvariant())))list.Items.Add(c.Name);if(list.Items.Count>0)list.SelectedIndex=0;else info.Text="No matching recipes.";};list.SelectedIndexChanged+=(s,e)=>{if(list.SelectedItem==null)return;var c=cubes.Find(x=>x.Name==(string)list.SelectedItem);info.Text=c.Name.ToUpper()+"\r\n\r\nINGREDIENTS\r\n"+c.Formula+"\r\n\r\n"+c.Note;};q.TextChanged+=(s,e)=>filter();filter();}
 int guideAct=1;
 void RefreshGuideList(string selected){if(guideList==null||guideList.IsDisposed)return;guideList.Items.Clear();foreach(var region in guideRegions)if(region.Act==guideAct)guideList.Items.Add(region);int index=-1;for(int i=0;i<guideList.Items.Count;i++)if(((GuideRegion)guideList.Items[i]).Name==selected){index=i;break;}if(guideList.Items.Count>0)guideList.SelectedIndex=index<0?0:index;}
 void ShowGuide(){
  Header("Sanctuary  /  travel & quests","Guide book","Choose an act and region for quest landmarks and farming ideas, or open your saved notes.");
  string[] acts={"ACT I","ACT II","ACT III","ACT IV","ACT V"};
  for(int i=0;i<acts.Length;i++){int act=i+1;var tab=Btn(acts[i],32+i*102,150,96,34);if(act==guideAct)tab.BackColor=Color.FromArgb(94,67,40);tab.Click+=(s,e)=>{guideAct=act;ShowGuide();};content.Controls.Add(tab);}
  var notes=Btn("MY NOTES",562,150,125,34);notes.Click+=(s,e)=>ShowGuideNotes();content.Controls.Add(notes);
  content.Controls.Add(LabelAt("REGIONS",32,200,235,22,9,gold,FontStyle.Bold));
  var list=new ListBox{Location=new Point(32,226),Size=new Size(240,350),BackColor=panel,ForeColor=text,BorderStyle=BorderStyle.FixedSingle,Font=f(10),IntegralHeight=false};
  guideList=list;
  var details=new TextBox{Location=new Point(290,226),Size=new Size(550,420),Multiline=true,ReadOnly=true,ScrollBars=ScrollBars.Vertical,BackColor=panel,ForeColor=text,BorderStyle=BorderStyle.FixedSingle,Font=f(10)};
  list.SelectedIndexChanged+=(s,e)=>{var region=list.SelectedItem as GuideRegion;details.Text=region==null?"":region.Details();};
  content.Controls.Add(list);content.Controls.Add(details);RefreshGuideList(null);
  content.Controls.Add(LabelAt("A curated starting guide; more regions and details can be added over time.",32,590,240,55,9,muted));
 }
 void ShowGuideNotes(){
  Header("Guide book  /  journal","My notes",className+" build  •  Your notes are saved locally as you type.");
  var back=Btn("BACK TO GUIDE",32,150,160,35);back.Click+=(s,e)=>ShowGuide();content.Controls.Add(back);
  var box=new TextBox{Location=new Point(32,205),Size=new Size(810,430),Multiline=true,ScrollBars=ScrollBars.Vertical,BackColor=panel,ForeColor=text,BorderStyle=BorderStyle.FixedSingle,Text=build.Notes,Font=f(11)};box.TextChanged+=(s,e)=>{build.Notes=box.Text;Save();};content.Controls.Add(box);
 }
 void Save(){if(loading)return;try{Directory.CreateDirectory(Path.GetDirectoryName(save));string temp=save+".tmp";using(var fs=File.Create(temp))new XmlSerializer(typeof(Build)).Serialize(fs,build);if(File.Exists(save))File.Replace(temp,save,save+".bak",true);else File.Move(temp,save);}catch(Exception ex){MessageBox.Show("Could not save your build: "+ex.Message);}}
 void LoadBuild(){build=new Build();try{if(File.Exists(save)){using(var fs=File.OpenRead(save))build=(Build)new XmlSerializer(typeof(Build)).Deserialize(fs);if(build.Points==null||build.Points.Length!=30)build.Points=new int[30];if(build.Gear==null||build.Gear.Length!=10)build.Gear=new string[10];if(build.Inventory==null||build.Inventory.Length!=40)build.Inventory=new string[40];if(build.Runes==null||build.Runes.Length!=33)build.Runes=new int[33];if(build.GearSkillBonuses==null||build.GearSkillBonuses.Length!=10)build.GearSkillBonuses=new ItemSkillBonuses[10];if(build.InventorySkillBonuses==null||build.InventorySkillBonuses.Length!=40)build.InventorySkillBonuses=new ItemSkillBonuses[40];}}catch{MessageBox.Show("Your saved build could not be read. A new build will be opened.");build=new Build();}}
 [STAThread] public static void Main(string[] args){
  Application.EnableVisualStyles();Application.SetCompatibleTextRenderingDefault(false);
  if(args.Length>0&&args[0]=="--self-test"){
   string dir=Path.Combine(Path.GetTempPath(),"SanctuaryAtlas-SelfTest-"+Guid.NewGuid().ToString("N"));
   using(var test=new Atlas(dir)){if(test.skills.Count!=30||test.words.Count!=99||test.Index("Summon Goatman")<0)Environment.Exit(1);}
   return;
  }
  var app=new Atlas();app.Shown+=(sender,e)=>app.CheckForUpdates(false);Application.Run(app);
 }
}

