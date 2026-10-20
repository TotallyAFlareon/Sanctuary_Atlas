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
[Serializable] public class Build { public int Level=30, Quests=4; public int[] Points=new int[30]; public int[] Runes=new int[33]; public string[] Gear=new string[10]; public string[] Inventory=new string[40]; public string Notes=""; }
public class Skill { public string Name, Tree, Requires, Desc; public int Level; public Skill(string t,string n,int l,string r,string d){Tree=t;Name=n;Level=l;Requires=r;Desc=d;} }
public class Word { public string Name, Runes, Base, Note, Effects=""; public Word(string n,string r,string b,string a){Name=n;Runes=r;Base=b;Note=a;} }
public class Cube { public string Name, Formula, Note; public Cube(string n,string f,string d){Name=n;Formula=f;Note=d;} }
public class Atlas : Form {
 static Color bg=Color.FromArgb(19,16,15), panel=Color.FromArgb(35,27,23), gold=Color.FromArgb(210,172,100), text=Color.FromArgb(236,224,199), muted=Color.FromArgb(184,169,146);
 string dataDir=Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),"SanctuaryAtlas"); string save; string className="Warlock"; static string[] classes={"Warlock","Amazon","Assassin","Barbarian","Druid","Necromancer","Paladin","Sorceress"};
 Build build=new Build(); List<Skill> skills=new List<Skill>(); List<Word> words=new List<Word>(); Panel content; Label budget; NumericUpDown level,quests; SkillTreePanel skillList; ComboBox tree; ListBox results; TextBox search,detail; ToolTip slotTip=new ToolTip(); bool loading=false; int selectedSlot=-1; bool selectedInventory=false, moving=false; CheckBox craftable; List<Cube> cubes=new List<Cube>(); static string[] runeNames="El Eld Tir Nef Eth Ith Tal Ral Ort Thul Amn Sol Shael Dol Hel Io Lum Ko Fal Lem Pul Um Mal Ist Gul Vex Ohm Lo Sur Ber Jah Cham Zod".Split(' ');
 static Font f(float s,FontStyle st=FontStyle.Regular){return new Font("Segoe UI",s,st);} 
 public Atlas(){ Icon=Icon.ExtractAssociatedIcon(Application.ExecutablePath); Text="Sanctuary Atlas | D2R companion | Update 12"; Size=new Size(1150,790); MinimumSize=new Size(950,680); BackColor=bg; ForeColor=text; Font=f(10); StartPosition=FormStartPosition.CenterScreen; save=Path.Combine(dataDir,"build.xml"); InitData(); LoadBuild(); MakeShell(); ShowSkills(); }
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
 void LoadWordEffects(){using(var stream=Assembly.GetExecutingAssembly().GetManifestResourceStream("RunewordEffects")){if(stream==null)throw new InvalidOperationException("Runeword effects data is missing.");var doc=new System.Xml.XmlDocument();doc.Load(stream);foreach(System.Xml.XmlNode node in doc.SelectNodes("/runewords/word")){var word=words.Find(w=>w.Name==node.Attributes["name"].Value);if(word==null)continue;var sections=new List<string>();foreach(System.Xml.XmlNode group in node.SelectNodes("group")){var lines=new List<string>();if(group.Attributes["bases"]!=null)lines.Add(group.Attributes["bases"].Value.ToUpperInvariant()+":");foreach(System.Xml.XmlNode effect in group.SelectNodes("effect"))lines.Add("• "+effect.InnerText);sections.Add(String.Join("\r\n",lines.ToArray()));}word.Effects=String.Join("\r\n\r\n",sections.ToArray());}}}
 void W(string n,string r,string b,string a){words.Add(new Word(n,r,b,a));}
 Label LabelAt(string s,int x,int y,int w,int h,float size,Color c,FontStyle st=FontStyle.Regular){var a=new Label{Text=s,Location=new Point(x,y),Size=new Size(w,h),ForeColor=c,Font=f(size,st),AutoEllipsis=true,BackColor=Color.Transparent};return a;}
 Button Btn(string s,int x,int y,int w,int h){var b=new GothicButton{Text=s,Location=new Point(x,y),Size=new Size(w,h),FlatStyle=FlatStyle.Flat,BackColor=panel,ForeColor=text,Font=f(9,FontStyle.Bold),Cursor=Cursors.Hand};b.FlatAppearance.BorderColor=Color.FromArgb(72,65,56);return b;}
 void StyleCombo(ComboBox box){box.DrawMode=DrawMode.OwnerDrawFixed;box.ItemHeight=20;box.DrawItem+=(s,e)=>{e.Graphics.FillRectangle(Brushes.White,e.Bounds);if(e.Index>=0){string label=box.Items[e.Index].ToString();TextRenderer.DrawText(e.Graphics,label,box.Font,e.Bounds,Color.Black,TextFormatFlags.Left|TextFormatFlags.VerticalCenter|TextFormatFlags.EndEllipsis);}e.DrawFocusRectangle();};}
 void MakeShell(){var side=new Panel{Dock=DockStyle.Left,Width=205,BackColor=Color.FromArgb(28,21,18)}; side.Paint+=(s,e)=>{using(var b=new LinearGradientBrush(side.ClientRectangle,Color.FromArgb(42,29,22),Color.FromArgb(13,12,11),0f))e.Graphics.FillRectangle(b,side.ClientRectangle);using(var p=new Pen(Color.FromArgb(118,84,48),2))e.Graphics.DrawLine(p,side.Width-2,8,side.Width-2,side.Height-8);}; Controls.Add(side); side.Controls.Add(LabelAt("S A N C T U A R Y",20,30,170,23,9,gold,FontStyle.Bold)); side.Controls.Add(LabelAt("ATLAS",20,55,170,35,23,text,FontStyle.Bold)); side.Controls.Add(LabelAt("D2R COMPANION",20,96,170,20,8,muted)); string[] nav={"01   SKILL PLANNER","02   EQUIPMENT","03   RUNEWORDS","04   RUNE STASH","05   CUBE RECIPES","06   FIELD NOTES","07   UPDATES"}; for(int i=0;i<nav.Length;i++){int k=i;var b=Btn(nav[i],12,160+i*51,181,42);b.TextAlign=ContentAlignment.MiddleLeft;b.Click+=(s,e)=>{if(k==0)ShowSkills();if(k==1)ShowGear();if(k==2)ShowWords();if(k==3)ShowRunes();if(k==4)ShowCube();if(k==5)ShowNotes();if(k==6)ShowUpdates();};side.Controls.Add(b);} side.Controls.Add(LabelAt("ALL EIGHT CLASSES\nPersonal build workspace\n\nUnofficial fan tool",20,630,165,80,9,muted)); content=new Panel{Dock=DockStyle.Fill,AutoScroll=true,BackColor=bg};using(var stream=Assembly.GetExecutingAssembly().GetManifestResourceStream("SanctuaryBackground")){if(stream!=null){using(var image=Image.FromStream(stream))content.BackgroundImage=new Bitmap(image);}}content.BackgroundImageLayout=ImageLayout.Stretch;content.Resize+=(s,e)=>{LayoutSkills();if(skillList!=null&&!skillList.IsDisposed)skillList.RefreshBackground();};content.Scroll+=(s,e)=>{if(skillList!=null&&!skillList.IsDisposed)skillList.RefreshBackground();};Controls.Add(content);content.BringToFront(); }
 void Header(string eyebrow,string title,string subtitle){slotTip.RemoveAll();var oldControls=content.Controls.Cast<Control>().ToArray();content.Controls.Clear();foreach(var c in oldControls)c.Dispose();content.Controls.Add(LabelAt(eyebrow.ToUpper(),30,24,800,20,9,gold,FontStyle.Bold));var heading=LabelAt(title.ToUpperInvariant(),30,49,850,44,23,text,FontStyle.Bold);heading.Font=new Font("Georgia",22,FontStyle.Bold);content.Controls.Add(heading);content.Controls.Add(LabelAt(subtitle,32,101,850,38,10,muted));}
 void LayoutSkills(){if(skillList==null||skillList.IsDisposed||skillList.Parent!=content)return;skillList.Size=new Size(Math.Max(850,content.ClientSize.Width-60),Math.Max(120,content.ClientSize.Height-342));}
 void ShowSkills(){Header("Character  /  build planner",className+" skill planner","Choose a class and a skill tree. Each class has its own saved build.");
 for(int i=0;i<classes.Length;i++){string choice=classes[i];var tab=Btn(choice,32+i*103,145,99,32);tab.Font=f(8,FontStyle.Bold);if(choice==className){tab.BackColor=Color.FromArgb(92,65,39);tab.ForeColor=Color.White;}tab.Click+=(s,e)=>SwitchClass(choice);content.Controls.Add(tab);}
 content.Controls.Add(LabelAt("CHARACTER LEVEL",32,198,140,20,8,gold,FontStyle.Bold)); level=new NumericUpDown{Location=new Point(32,220),Width=95,Minimum=1,Maximum=99,Value=build.Level,BackColor=panel,ForeColor=text};content.Controls.Add(level);
 content.Controls.Add(LabelAt("QUEST POINTS",154,198,120,20,8,gold,FontStyle.Bold)); quests=new NumericUpDown{Location=new Point(154,220),Width=95,Minimum=0,Maximum=12,Value=build.Quests,BackColor=panel,ForeColor=text};content.Controls.Add(quests);budget=LabelAt("",285,217,370,30,12,text,FontStyle.Bold);content.Controls.Add(budget);
 var reset=Btn("RESET POINTS",700,211,150,34);reset.Click+=(s,e)=>{if(MessageBox.Show("Reset all skill points?","Sanctuary Atlas",MessageBoxButtons.YesNo)==DialogResult.Yes){build.Points=new int[30];Save();DrawSkills();}};content.Controls.Add(reset);
 level.ValueChanged+=(s,e)=>{build.Level=(int)level.Value;Save();DrawSkills();};quests.ValueChanged+=(s,e)=>{build.Quests=(int)quests.Value;Save();DrawSkills();};
 tree=new ComboBox();tree.Items.AddRange(skills.Select(x=>x.Tree).Distinct().Cast<object>().ToArray());var tabs=new List<Button>();for(int i=0;i<tree.Items.Count;i++){string choice=(string)tree.Items[i];var b=Btn(choice,32+i*274,271,266,36);b.Font=f(9,FontStyle.Bold);b.Click+=(s,e)=>tree.SelectedItem=choice;tabs.Add(b);content.Controls.Add(b);}tree.SelectedIndexChanged+=(s,e)=>{for(int i=0;i<tabs.Count;i++){bool active=i==tree.SelectedIndex;tabs[i].BackColor=active?Color.FromArgb(92,65,39):panel;tabs[i].ForeColor=active?Color.White:text;}DrawSkills();};
 skillList=new SkillTreePanel{BackgroundOwner=content,Location=new Point(30,322),Size=new Size(850,438),BackColor=Color.FromArgb(19,16,15),AutoScroll=false,Anchor=AnchorStyles.Top|AnchorStyles.Left};content.Controls.Add(skillList);LayoutSkills();tree.SelectedIndex=0; }
 void SwitchClass(string next){if(next==className)return;selectedSlot=-1;moving=false;className=next; skills.Clear();if(next=="Warlock")InitWarlockSkills();else InitOtherClass();save=Path.Combine(dataDir,next=="Warlock"?"build.xml":"build-"+next.ToLowerInvariant()+".xml");LoadBuild();ShowSkills();}
 int Index(string name){return skills.FindIndex(x=>x.Name==name);} int Spent(){return build.Points.Sum();}
 bool Valid(){if(Spent()>build.Level-1+build.Quests)return false;for(int i=0;i<30;i++){var sk=skills[i];if(build.Points[i]>0&&(build.Level<sk.Level || build.Points[i]>Math.Min(20,build.Level-sk.Level+1)))return false;if(build.Points[i]>0&&sk.Requires!="")foreach(string req in sk.Requires.Split('|'))if(build.Points[Index(req)]==0)return false;}return true;}
 void DrawSkills(){if(skillList==null||tree==null)return;budget.Text="POINTS  "+Spent()+" / "+(build.Level-1+build.Quests)+(Valid()?"":"    • Check level or quest points");budget.ForeColor=Valid()?gold:Color.Salmon;var scroll=skillList.AutoScrollPosition;skillList.AutoScrollPosition=Point.Empty;skillList.ClearSkills();int rowIndex=0;foreach(int tier in new int[]{1,6,12,18,24,30}){int y=rowIndex++*112;skillList.Controls.Add(LabelAt("LV\n"+tier,4,y+24,35,54,9,gold,FontStyle.Bold));var items=skills.Where(x=>x.Tree==(string)tree.SelectedItem&&x.Level==tier).ToList();for(int j=0;j<items.Count;j++){var sk=items[j];int idx=Index(sk.Name);var card=new GothicPanel{Location=new Point(42+j*250,y+2),Size=new Size(244,102),BackColor=Color.FromArgb(35,27,23)};card.Controls.Add(LabelAt(sk.Name,10,8,220,23,10,text,FontStyle.Bold));string requirement=sk.Requires==""?"No prerequisite":"Requires: "+sk.Requires.Replace("|"," + ");card.Controls.Add(LabelAt(requirement,10,31,220,30,8,muted));slotTip.SetToolTip(card,sk.Name+"\n"+sk.Desc+"\n"+requirement);var minus=Btn("−",10,66,35,27);var count=LabelAt(build.Points[idx]+" / 20",52,70,70,22,9,gold,FontStyle.Bold);var plus=Btn("+",129,66,35,27);minus.Click+=(s,e)=>Change(idx,-1);plus.Click+=(s,e)=>Change(idx,1);card.Controls.Add(minus);card.Controls.Add(count);card.Controls.Add(plus);skillList.Controls.Add(card);}} skillList.FinishSkills(-scroll.Y); }
 bool PrerequisitesValid(){for(int i=0;i<skills.Count;i++)if(build.Points[i]>0 && skills[i].Requires!="")foreach(string req in skills[i].Requires.Split('|'))if(Index(req)<0 || build.Points[Index(req)]==0)return false;return true;}
 void Change(int i,int delta){int old=build.Points[i]; if(old+delta<0||old+delta>20)return; build.Points[i]+=delta;if((delta>0&&!Valid())||(delta<0&&!PrerequisitesValid())){build.Points[i]=old;MessageBox.Show("This change would exceed your available points, character-level limit, or a skill prerequisite. Adjust your level or quest points, or invest in prerequisites first.","Cannot allocate point");return;}Save();DrawSkills();}
 static string[] gearNames={"Weapon","Off-hand","Helm","Armor","Gloves","Belt","Boots","Amulet","Ring 1","Ring 2"};
 string SlotText(bool inventory,int index){return inventory?build.Inventory[index]:build.Gear[index];}
 void PutSlot(bool inventory,int index,string value){if(inventory)build.Inventory[index]=value;else build.Gear[index]=value;}
 void SelectSlot(bool inventory,int index){if(moving && selectedSlot>=0){string from=SlotText(selectedInventory,selectedSlot),to=SlotText(inventory,index);PutSlot(selectedInventory,selectedSlot,to);PutSlot(inventory,index,from);moving=false;Save();}selectedInventory=inventory;selectedSlot=index;ShowGear();}
 void EditSlot(){if(selectedSlot<0)return;using(var dlg=new Form{Text="Edit "+(selectedInventory?"inventory cell":gearNames[selectedSlot]),Size=new Size(490,205),StartPosition=FormStartPosition.CenterParent,FormBorderStyle=FormBorderStyle.FixedDialog,MaximizeBox=false,MinimizeBox=false,BackColor=bg,ForeColor=text}){dlg.Controls.Add(LabelAt("ITEM NAME OR NOTES",20,20,420,22,9,gold,FontStyle.Bold));var entry=new TextBox{Location=new Point(20,48),Width=430,Text=SlotText(selectedInventory,selectedSlot)??"",BackColor=panel,ForeColor=text};dlg.Controls.Add(entry);var ok=Btn("SAVE",300,95,150,35);ok.Click+=(s,e)=>{PutSlot(selectedInventory,selectedSlot,entry.Text.Trim());Save();dlg.DialogResult=DialogResult.OK;dlg.Close();};dlg.Controls.Add(ok);dlg.AcceptButton=ok;dlg.ShowDialog(this);}ShowGear();}
 void SlotButton(Panel host,bool inv,int index,int x,int y,int w,int h,ToolTip tips){string value=SlotText(inv,index)??"";string label=inv?(String.IsNullOrEmpty(value)?"":(value.Length>3?value.Substring(0,3):value)):gearNames[index]+"\n"+(String.IsNullOrEmpty(value)?"[ empty ]":value);var b=Btn(label,x,y,w,h);var themed=b as GothicButton;if(themed!=null&&!inv){themed.GearSlot=true;themed.GlyphIndex=index;}b.Font=f(inv?8:8,FontStyle.Regular);b.BackColor=selectedSlot==index && selectedInventory==inv?Color.FromArgb(94,67,40):Color.FromArgb(44,41,40);b.FlatAppearance.BorderColor=Color.FromArgb(101,84,61);tips.SetToolTip(b,(inv?"Inventory cell "+(index+1):gearNames[index])+": "+(value==""?"empty":value));b.Click+=(s,e)=>SelectSlot(inv,index);host.Controls.Add(b);}
 void ShowGear(){Header("Character  /  inventory & equipment",className+" equipment","Select a slot to edit it, or move an item between slots. This is a manual planning grid.");
 content.Controls.Add(LabelAt("EQUIPMENT",32,153,300,23,10,gold,FontStyle.Bold));content.Controls.Add(LabelAt("INVENTORY  •  10 × 4",480,153,340,23,10,gold,FontStyle.Bold));var tips=slotTip;
 int[] xs={32,302,167,167,32,167,302,32,302,167};int[] ys={259,259,184,259,365,365,365,184,184,440};int[] ws={125,125,125,125,125,125,125,125,125,125};int[] hs={95,95,65,95,65,65,65,65,65,65};for(int i=0;i<10;i++)SlotButton(content,false,i,xs[i],ys[i],ws[i],hs[i],tips);
 for(int i=0;i<40;i++)SlotButton(content,true,i,480+(i%10)*36,184+(i/10)*52,34,49,tips);
 string caption=selectedSlot<0?"Select an equipment slot or inventory cell.":(selectedInventory?"INVENTORY CELL "+(selectedSlot+1):gearNames[selectedSlot].ToUpper())+"\n"+(String.IsNullOrEmpty(SlotText(selectedInventory,selectedSlot))?"Empty":SlotText(selectedInventory,selectedSlot));if(moving)caption="MOVE / SWAP\nSelect the destination slot or cell.";var summary=LabelAt(caption,480,405,360,64,10,text);content.Controls.Add(summary);
 var edit=Btn("EDIT ITEM",480,478,112,36);edit.Enabled=selectedSlot>=0&&!moving;edit.Click+=(s,e)=>EditSlot();content.Controls.Add(edit);var move=Btn(moving?"CANCEL":"MOVE / SWAP",600,478,112,36);move.Enabled=selectedSlot>=0;move.Click+=(s,e)=>{moving=!moving;ShowGear();};content.Controls.Add(move);var clear=Btn("CLEAR",720,478,112,36);clear.Enabled=selectedSlot>=0&&!moving&&!String.IsNullOrEmpty(SlotText(selectedInventory,selectedSlot));clear.Click+=(s,e)=>{if(MessageBox.Show("Clear this slot?","Sanctuary Atlas",MessageBoxButtons.YesNo)==DialogResult.Yes){PutSlot(selectedInventory,selectedSlot,"");Save();ShowGear();}};content.Controls.Add(clear);
 content.Controls.Add(LabelAt("Click a slot, then edit its name or choose Move / Swap and click a destination. Each cell holds one planning entry; item sizes and compatibility are not checked.",480,534,360,78,9,muted));content.Controls.Add(LabelAt("Gear and inventory are saved for this class. Values are not read from the game.",32,628,810,35,9,muted));}
 bool updateChecking=false;
 string UpdateRepositoryPath(){return Path.Combine(dataDir,"update-repository.txt");}
 string UpdateRepository(){try{return File.Exists(UpdateRepositoryPath())?File.ReadAllText(UpdateRepositoryPath()).Trim():AtlasUpdates.DefaultRepository;}catch{return "";}}
 void ShowUpdates(){Header("Sanctuary Atlas  /  updates","App updates","Get new versions without reinstalling by hand once a release source is connected.");
  content.Controls.Add(LabelAt("RELEASE SOURCE",32,155,350,24,10,gold,FontStyle.Bold));
  content.Controls.Add(LabelAt("Enter a GitHub repository as owner/repository. Leave blank to turn off update checks.",32,185,760,48,10,text));
  var repository=new TextBox{Location=new Point(32,240),Width=390,Text=UpdateRepository(),BackColor=panel,ForeColor=text};content.Controls.Add(repository);
  var save=Btn("SAVE SOURCE",32,282,180,36);content.Controls.Add(save);
  var check=Btn("CHECK FOR UPDATES",220,282,200,36);content.Controls.Add(check);
  var info=LabelAt(UpdateRepository()==""?"Automatic update checks are off. Save a public release repository to enable them.":"The app checks this repository when it starts. You decide whether to install an update. The repository and releases must be public.",32,340,780,85,10,muted);content.Controls.Add(info);
  save.Click+=(sender,e)=>{string value=repository.Text.Trim();if(value!=""&&!AtlasUpdates.ValidRepository(value)){MessageBox.Show("Enter the repository as owner/repository.","App updates");return;}Directory.CreateDirectory(dataDir);if(value==""){File.WriteAllText(UpdateRepositoryPath(),"");info.Text="Automatic update checks are off.";}else{File.WriteAllText(UpdateRepositoryPath(),value);info.Text="Release source saved. The app will check when it starts.";}};
  check.Click+=(sender,e)=>{string value=repository.Text.Trim();if(value!=UpdateRepository()){MessageBox.Show("Save the release source first.","App updates");return;}CheckForUpdates(true);};
 }
 void CheckForUpdates(bool showNoUpdate){
  string repo=UpdateRepository();if(repo==""){if(showNoUpdate)MessageBox.Show("No release source is connected yet. Add one in App updates when it is available.","App updates");return;}
  if(!AtlasUpdates.ValidRepository(repo)){if(showNoUpdate)MessageBox.Show("The saved release source is invalid.","App updates");return;}
  if(updateChecking){if(showNoUpdate)MessageBox.Show("An update check is already in progress.","App updates");return;}
  updateChecking=true;
  var thread=new System.Threading.Thread(()=>{try{
   UpdateRelease release=AtlasUpdates.Fetch(repo);
   if(IsDisposed||!IsHandleCreated)return;
   BeginInvoke((Action)(()=>{updateChecking=false;if(IsDisposed)return;if(release.Version<=AtlasUpdates.CurrentVersion){if(showNoUpdate)MessageBox.Show("Sanctuary Atlas is up to date.","App updates");return;}
    if(MessageBox.Show("Update "+release.Version+" is available. Download and install it now? The app will restart.","App update",MessageBoxButtons.YesNo,MessageBoxIcon.Question)==DialogResult.Yes)DownloadUpdate(release);
   }));
  }catch(Exception ex){if(!IsDisposed&&IsHandleCreated)BeginInvoke((Action)(()=>{updateChecking=false;if(showNoUpdate&&!IsDisposed)MessageBox.Show("Could not check for updates: "+ex.Message,"App updates");}));}});
  thread.IsBackground=true;thread.Start();
 }
 void DownloadUpdate(UpdateRelease release){
  var thread=new System.Threading.Thread(()=>{try{string file=AtlasUpdates.Download(release);if(IsDisposed||!IsHandleCreated){try{File.Delete(file);}catch{}return;}BeginInvoke((Action)(()=>{try{System.Diagnostics.Process.Start(file,"--apply-update "+System.Diagnostics.Process.GetCurrentProcess().Id);Close();}catch(Exception ex){MessageBox.Show("Could not start the update: "+ex.Message,"App updates");}}));}
  catch(Exception ex){if(!IsDisposed&&IsHandleCreated)BeginInvoke((Action)(()=>MessageBox.Show("Could not download the update: "+ex.Message,"App updates")));}});thread.IsBackground=true;thread.Start();
 }
 void ShowWords(){Header("The horadric archive  /  99 entries","Runeword library",className+" stash  •  Search by name, rune, base or effect. Select a recipe for its details.");search=new TextBox{Location=new Point(32,155),Width=390,BackColor=panel,ForeColor=text,BorderStyle=BorderStyle.FixedSingle};search.TextChanged+=(s,e)=>FilterWords();content.Controls.Add(search); craftable=new CheckBox{Text="Only runes I have",Location=new Point(450,153),Size=new Size(240,27),ForeColor=text};craftable.CheckedChanged+=(s,e)=>FilterWords();content.Controls.Add(craftable);var add=Btn("ADD TO GEAR",700,150,150,31);add.Click+=(s,e)=>AddToGear();content.Controls.Add(add);results=new ListBox{Location=new Point(32,190),Size=new Size(390,440),BackColor=panel,ForeColor=text,BorderStyle=BorderStyle.None,Font=f(11),IntegralHeight=false};results.SelectedIndexChanged+=(s,e)=>ShowWordDetail();content.Controls.Add(results);detail=new TextBox{Location=new Point(450,190),Size=new Size(400,440),Multiline=true,ReadOnly=true,BackColor=panel,ForeColor=text,BorderStyle=BorderStyle.None,Font=f(11),ScrollBars=ScrollBars.Vertical};content.Controls.Add(detail);FilterWords();}
 void AddToGear(){if(results.SelectedItem==null)return;string name=(string)results.SelectedItem;using(var dlg=new Form{Text="Add "+name+" to gear",Size=new Size(540,390),StartPosition=FormStartPosition.CenterParent,FormBorderStyle=FormBorderStyle.FixedDialog,MaximizeBox=false,MinimizeBox=false,BackColor=bg,ForeColor=text}){dlg.Controls.Add(LabelAt("CHOOSE AN EQUIPMENT SLOT",20,18,470,24,10,gold,FontStyle.Bold));dlg.Controls.Add(LabelAt("Verify base and slot compatibility yourself.",20,48,470,24,9,muted));for(int i=0;i<gearNames.Length;i++){int k=i;var button=Btn(gearNames[i],20+(i%2)*240,82+(i/2)*48,224,38);button.Click+=(s,e)=>{if(!String.IsNullOrWhiteSpace(build.Gear[k])&&MessageBox.Show("Replace the current entry in "+gearNames[k]+"?","Replace gear",MessageBoxButtons.YesNo)!=DialogResult.Yes)return;build.Gear[k]=name;Save();dlg.Close();};dlg.Controls.Add(button);}dlg.ShowDialog(this);}}
 void FilterWords(){string q=search.Text.ToLowerInvariant();results.Items.Clear();foreach(var w in words.Where(x=>(x.Name+" "+x.Runes+" "+x.Base+" "+x.Effects).ToLowerInvariant().Contains(q) && (!craftable.Checked || Missing(x)=="")))results.Items.Add(w.Name);if(results.Items.Count>0)results.SelectedIndex=0;else detail.Text="No matching runewords.";}
 void ShowWordDetail(){if(results.SelectedItem==null)return;var w=words.Find(x=>x.Name==(string)results.SelectedItem); detail.Text=w.Name.ToUpper()+"\r\n\r\nRUNE ORDER\r\n"+w.Runes+"\r\n\r\nELIGIBLE BASE\r\n"+w.Base+"\r\n\r\nEFFECTS\r\n"+w.Effects+"\r\n\r\nRUNE STASH\r\n"+(Missing(w)==""?"You have the runes for one copy.":"Missing: "+Missing(w))+"\r\n\r\n"+w.Note+"\r\n\r\nUse a non-magical socketed item with exactly the listed number of sockets. Insert the runes in the order shown. Expansion, patch, and ladder rules can affect availability; verify in your game before using rare runes. The rune-stash check does not check your base or game mode.";}
 
 string Missing(Word w){var needed=w.Runes.Split(new string[]{" + "},StringSplitOptions.None).GroupBy(x=>x); var missing=new List<string>();foreach(var g in needed){int i=Array.IndexOf(runeNames,g.Key);int own=i<0?0:build.Runes[i];int n=g.Count()-own;if(n>0)missing.Add(n+" x "+g.Key);}return String.Join(", ",missing.ToArray());}
 void ShowRunes(){Header("Materials  /  your rune stash","Rune inventory",""+className+" build  •  Enter quantities you own. The runeword library can filter by these counts.");for(int i=0;i<33;i++){int k=i;int col=i/11,row=i%11,x=32+col*270,y=153+row*45;content.Controls.Add(LabelAt((i+1).ToString("00")+"   "+runeNames[i],x,y+3,140,25,10,text,FontStyle.Bold));var n=new NumericUpDown{Location=new Point(x+150,y),Size=new Size(72,28),Minimum=0,Maximum=999,Value=Math.Min(999,Math.Max(0,build.Runes[i])),BackColor=panel,ForeColor=text};n.ValueChanged+=(s,e)=>{build.Runes[k]=(int)n.Value;Save();};content.Controls.Add(n);}content.Controls.Add(LabelAt("Counts are saved locally per class. Filters check runes only, not the base or edition.",32,661,800,30,9,muted));}
 void C(string n,string f,string note){cubes.Add(new Cube(n,f,note));}
 void InitCube(){
 string socketNote="Uses a normal-quality, unsocketed base; low-quality and superior items do not work. Ethereal items can be used. Socket count is random and capped by the base and item level.";
 C("Socket a body armor","Tal + Thul + Perfect Topaz + normal body armor",socketNote);C("Socket a weapon","Ral + Amn + Perfect Amethyst + normal weapon",socketNote);C("Socket a helm","Ral + Thul + Perfect Sapphire + normal helm",socketNote);C("Socket a shield","Tal + Amn + Perfect Ruby + normal shield",socketNote);
 C("Clear sockets","Hel + Scroll of Town Portal + socketed item","Destroys all socketed runes, gems and jewels. The base and its sockets remain."); C("Repair weapon","Ort + weapon","Restores durability. Ethereal items cannot be repaired this way.");C("Repair armor","Ral + armor","Restores durability. Ethereal items cannot be repaired this way.");C("Repair and recharge weapon","Ort + any chipped gem + weapon","Repairs and recharges; ethereal items cannot be repaired this way.");C("Repair and recharge armor","Ral + any flawed gem + armor","Repairs and recharges; ethereal items cannot be repaired this way.");
 C("Upgrade a gem","3 gems of the same type and grade","Creates one gem of the same type at the next grade, up to perfect. Skulls follow the same pattern.");C("Full rejuvenation potion","3 rejuvenation potions","Creates one full rejuvenation potion.");C("Rejuvenation potion","3 health potions + 3 mana potions + 1 chipped gem or skull","Creates one rejuvenation potion.");
 C("Upgrade normal unique weapon","Ral + Sol + Perfect Emerald + normal unique weapon","Upgrades the base to exceptional, keeping its unique properties. Check edition and mode rules.");C("Upgrade exceptional unique weapon","Lum + Pul + Perfect Emerald + exceptional unique weapon","Upgrades the base to elite, keeping its unique properties. Check edition and mode rules.");C("Upgrade normal unique armor","Tal + Shael + Perfect Diamond + normal unique armor","Upgrades the base to exceptional, keeping its unique properties. Check edition and mode rules.");C("Upgrade exceptional unique armor","Ko + Lem + Perfect Diamond + exceptional unique armor","Upgrades the base to elite, keeping its unique properties. Check edition and mode rules.");
 C("Upgrade normal rare weapon","Ort + Amn + Perfect Sapphire + normal rare weapon","Upgrades the base to exceptional. Check edition and mode rules.");C("Upgrade exceptional rare weapon","Fal + Um + Perfect Sapphire + exceptional rare weapon","Upgrades the base to elite. Check edition and mode rules.");C("Upgrade normal rare armor","Ral + Thul + Perfect Amethyst + normal rare armor","Upgrades the base to exceptional. Check edition and mode rules.");C("Upgrade exceptional rare armor","Ko + Pul + Perfect Amethyst + exceptional rare armor","Upgrades the base to elite. Check edition and mode rules.");
 string[] gems={"","","","","","","","","","Chipped Topaz","Chipped Amethyst","Chipped Sapphire","Chipped Ruby","Chipped Emerald","Chipped Diamond","Flawed Topaz","Flawed Amethyst","Flawed Sapphire","Flawed Ruby","Flawed Emerald","Flawed Diamond","Topaz","Amethyst","Sapphire","Ruby","Emerald","Diamond","Flawless Topaz","Flawless Amethyst","Flawless Sapphire","Flawless Ruby","Flawless Emerald"};for(int i=0;i<32;i++){int count=i<20?3:2;C("Upgrade "+runeNames[i]+" to "+runeNames[i+1],count+" x "+runeNames[i]+(gems[i]==""?"":" + "+gems[i]),"Creates one "+runeNames[i+1]+" rune. Availability can vary by game edition and mode.");}
 }
 void ShowCube(){Header("The horadric archive  /  "+cubes.Count+" entries","Cube recipes","Search by material or result. Read each recipe's restrictions before transmuting.");var q=new TextBox{Location=new Point(32,155),Width=390,BackColor=panel,ForeColor=text,BorderStyle=BorderStyle.FixedSingle};var list=new ListBox{Location=new Point(32,190),Size=new Size(390,440),BackColor=panel,ForeColor=text,BorderStyle=BorderStyle.None,Font=f(10),IntegralHeight=false};var info=new TextBox{Location=new Point(450,190),Size=new Size(400,440),Multiline=true,ReadOnly=true,BackColor=panel,ForeColor=text,BorderStyle=BorderStyle.None,Font=f(11),ScrollBars=ScrollBars.Vertical};content.Controls.Add(q);content.Controls.Add(list);content.Controls.Add(info);Action filter=()=>{list.Items.Clear();foreach(var c in cubes.Where(x=>(x.Name+" "+x.Formula).ToLowerInvariant().Contains(q.Text.ToLowerInvariant())))list.Items.Add(c.Name);if(list.Items.Count>0)list.SelectedIndex=0;else info.Text="No matching recipes.";};list.SelectedIndexChanged+=(s,e)=>{if(list.SelectedItem==null)return;var c=cubes.Find(x=>x.Name==(string)list.SelectedItem);info.Text=c.Name.ToUpper()+"\r\n\r\nINGREDIENTS\r\n"+c.Formula+"\r\n\r\n"+c.Note;};q.TextChanged+=(s,e)=>filter();filter();}
 void ShowNotes(){Header("Your journal  /  local save","Field notes",className+" build  •  Keep build ideas, farming targets and next upgrades in one place.");var box=new TextBox{Location=new Point(32,155),Size=new Size(810,430),Multiline=true,ScrollBars=ScrollBars.Vertical,BackColor=panel,ForeColor=text,BorderStyle=BorderStyle.FixedSingle,Text=build.Notes,Font=f(11)};box.TextChanged+=(s,e)=>{build.Notes=box.Text;Save();};content.Controls.Add(box);content.Controls.Add(LabelAt("Data: Reign of the Warlock build 3.3.93847. The cube collection is curated and does not include every recipe. It does not calculate skill effects or item stats.\n\nSources: D2R.ORG game data; DiabloBytes class skills; Blizzard Arreat Summit and Reign of the Warlock notes.\nYour build is stored locally in your Windows user profile.",32,610,810,90,9,muted));}
 void Save(){if(loading)return;try{Directory.CreateDirectory(Path.GetDirectoryName(save));string temp=save+".tmp";using(var fs=File.Create(temp))new XmlSerializer(typeof(Build)).Serialize(fs,build);if(File.Exists(save))File.Replace(temp,save,save+".bak",true);else File.Move(temp,save);}catch(Exception ex){MessageBox.Show("Could not save your build: "+ex.Message);}}
 void LoadBuild(){build=new Build();try{if(File.Exists(save)){using(var fs=File.OpenRead(save))build=(Build)new XmlSerializer(typeof(Build)).Deserialize(fs);if(build.Points==null||build.Points.Length!=30)build.Points=new int[30];if(build.Gear==null||build.Gear.Length!=10)build.Gear=new string[10];if(build.Inventory==null||build.Inventory.Length!=40)build.Inventory=new string[40];if(build.Runes==null||build.Runes.Length!=33)build.Runes=new int[33];}}catch{MessageBox.Show("Your saved build could not be read. A new build will be opened.");build=new Build();}}
 [STAThread] public static void Main(string[] args){Application.EnableVisualStyles();Application.SetCompatibleTextRenderingDefault(false);if(args.Length>0&&args[0]=="--self-test"){var a=new Atlas(); if(a.skills.Count!=30||a.words.Count!=99||a.Index("Abyss")<0||a.words.Any(w=>String.IsNullOrEmpty(w.Effects)))Environment.Exit(1);var fake=new System.Web.Script.Serialization.JavaScriptSerializer().Serialize(new{tag_name="v13",assets=new[]{new{name="Sanctuary Atlas Setup.exe",browser_download_url="https://github.com/example/atlas/releases/download/v13/Sanctuary.Atlas.Setup.exe",digest="sha256:"+new string('a',64),size=100}}});var parsed=AtlasUpdates.ParseRelease(fake,"example/atlas");if(parsed.Version!=13||parsed.Hash.Length!=64||AtlasUpdates.ValidRepository("../bad"))Environment.Exit(30);bool rejected=false;try{AtlasUpdates.ParseRelease(fake.Replace("https://github.com/example/atlas/","https://other.example/"),"example/atlas");}catch(InvalidOperationException){rejected=true;}if(!rejected)Environment.Exit(31);if(!a.words.Find(w=>w.Name=="Spirit").Effects.Contains("SWORDS:")||!a.words.Find(w=>w.Name=="Spirit").Effects.Contains("SHIELDS:")||!a.words.Find(w=>w.Name=="Authority").Effects.Contains("Warlock"))Environment.Exit(29);a.build=new Build(); a.build.Points[a.Index("Demonic Mastery")]=1; if(a.Valid()) Environment.Exit(2); a.build.Points[a.Index("Summon Goatman")]=1; if(!a.Valid()) Environment.Exit(3); a.build.Points[a.Index("Demonic Mastery")]=0; a.build.Points[a.Index("Summon Goatman")]=0; using(var ms=new MemoryStream()){new XmlSerializer(typeof(Build)).Serialize(ms,a.build); ms.Position=0; var b=(Build)new XmlSerializer(typeof(Build)).Deserialize(ms); if(b.Points.Length!=30||b.Gear.Length!=10||b.Inventory.Length!=40)Environment.Exit(4);} string oldXml="<Build><Level>42</Level><Quests>4</Quests><Points><int>3</int></Points><Gear><string>Spirit</string></Gear><Notes>old notes</Notes></Build>";using(var rd=new StringReader(oldXml)){var old=(Build)new XmlSerializer(typeof(Build)).Deserialize(rd);if(old.Level!=42||old.Gear[0]!="Spirit"||old.Runes.Length!=33||old.Inventory.Length!=40||old.Notes!="old notes")Environment.Exit(9);} if(a.words.Find(x=>x.Name=="Mosaic").Runes!="Mal + Gul + Amn")Environment.Exit(5); var inf=a.words.Find(x=>x.Name=="Infinity"); a.build.Runes[29]=1; a.build.Runes[22]=1; a.build.Runes[23]=1; if(!a.Missing(inf).Contains("1 x Ber"))Environment.Exit(6); a.build.Runes[29]=2; if(a.Missing(inf)!="")Environment.Exit(7); if(a.cubes.Count!=52)Environment.Exit(8); a.Show(); Application.DoEvents(); a.ShowWords();Application.DoEvents();a.search.Text="teleport";Application.DoEvents();if(!a.results.Items.Contains("Enigma"))Environment.Exit(32);a.search.Text="";Application.DoEvents();a.results.SelectedItem="Spirit";if(!a.detail.Text.Contains("SWORDS:")||!a.detail.Text.Contains("SHIELDS:"))Environment.Exit(34);a.results.SelectedItem="Authority";if(!a.detail.Text.Contains("Warlock"))Environment.Exit(35);a.results.SelectedIndex=0;using(var bmp=new Bitmap(a.Width,a.Height)){a.DrawToBitmap(bmp,new Rectangle(0,0,a.Width,a.Height));bmp.Save("work\\words-preview.png");} a.ShowRunes();Application.DoEvents();using(var bmp=new Bitmap(a.Width,a.Height)){a.DrawToBitmap(bmp,new Rectangle(0,0,a.Width,a.Height));bmp.Save("work\\runes-preview.png");}a.ShowCube();Application.DoEvents();using(var bmp=new Bitmap(a.Width,a.Height)){a.DrawToBitmap(bmp,new Rectangle(0,0,a.Width,a.Height));bmp.Save("work\\cube-preview.png");}a.dataDir=Path.Combine(Environment.CurrentDirectory,"work","class-test-"+Guid.NewGuid().ToString("N"));a.className="Warlock";a.save=Path.Combine(a.dataDir,"build.xml");a.skills.Clear();a.InitWarlockSkills();a.build=new Build();a.build.Points[a.Index("Summon Goatman")]=3;a.build.Gear[0]="Original Warlock gear";a.build.Runes[29]=2;a.build.Inventory[4]="Stored amulet";a.Save();a.SwitchClass("Amazon");if(a.build.Points.Sum()!=0||!String.IsNullOrEmpty(a.build.Gear[0]))Environment.Exit(13);a.build.Points[a.Index("Magic Arrow")]=2;a.build.Gear[0]="Amazon gear";a.Save();a.SwitchClass("Warlock");if(a.build.Points[a.Index("Summon Goatman")]!=3||a.build.Gear[0]!="Original Warlock gear"||a.build.Runes[29]!=2||a.build.Inventory[4]!="Stored amulet")Environment.Exit(14);a.SwitchClass("Amazon");if(a.build.Points[a.Index("Magic Arrow")]!=2||a.build.Gear[0]!="Amazon gear")Environment.Exit(15);a.SwitchClass("Warlock");a.SelectSlot(false,0);a.moving=true;a.SelectSlot(true,4);if(a.build.Gear[0]!="Stored amulet"||a.build.Inventory[4]!="Original Warlock gear")Environment.Exit(16);a.SwitchClass("Amazon");a.SwitchClass("Warlock");if(a.build.Gear[0]!="Stored amulet"||a.build.Inventory[4]!="Original Warlock gear")Environment.Exit(17);foreach(string cls in classes){a.className=cls;a.skills.Clear();if(cls=="Warlock")a.InitWarlockSkills();else a.InitOtherClass();if(a.skills.Count!=30||a.skills.Select(x=>x.Tree).Distinct().Count()!=3||a.skills.Select(x=>x.Name).Distinct().Count()!=30)Environment.Exit(10);foreach(var sk in a.skills){if(!new int[]{1,6,12,18,24,30}.Contains(sk.Level))Environment.Exit(11);if(sk.Requires!="")foreach(string req in sk.Requires.Split('|'))if(a.Index(req)<0)Environment.Exit(12);}}a.className="Sorceress";a.skills.Clear();a.InitOtherClass();a.build=new Build();a.ShowSkills();Application.DoEvents();var firstCard=a.skillList.Controls.OfType<GothicPanel>().First();firstCard.Controls.OfType<Button>().First(b=>b.Text=="+").PerformClick();if(a.build.Points[a.Index("Fire Bolt")]!=1)Environment.Exit(18);firstCard=a.skillList.Controls.OfType<GothicPanel>().First();firstCard.Controls.OfType<Button>().First(b=>b.Text=="−").PerformClick();if(a.build.Points[a.Index("Fire Bolt")]!=0)Environment.Exit(19);using(var bmp=new Bitmap(a.Width,a.Height)){a.DrawToBitmap(bmp,new Rectangle(0,0,a.Width,a.Height));bmp.Save("work\\classes-preview.png");}a.skillList.AutoScrollPosition=new Point(0,230);a.skillList.Invalidate(true);Application.DoEvents();using(var bmp=new Bitmap(a.Width,a.Height)){a.DrawToBitmap(bmp,new Rectangle(0,0,a.Width,a.Height));bmp.Save("work\\skills-scrolled-preview.png");}using(var before=new Bitmap("work\\classes-preview.png"))using(var after=new Bitmap("work\\skills-scrolled-preview.png")){for(int y=370;y<740;y+=13)if(before.GetPixel(850,y)!=after.GetPixel(850,y))Environment.Exit(23);}int cacheBuilds=a.skillList.BackgroundCacheBuilds;for(int i=0;i<30;i++){a.skillList.AutoScrollPosition=new Point(0,(i%2==0)?0:230);Application.DoEvents();}if(a.skillList.BackgroundCacheBuilds!=cacheBuilds)Environment.Exit(24);int previousScroll=a.skillList.AutoScrollPosition.Y;if(previousScroll>=0)Environment.Exit(20);a.Change(a.Index("Fire Bolt"),1);if(a.build.Points[a.Index("Fire Bolt")]!=1 || a.skillList.AutoScrollPosition.Y!=previousScroll)Environment.Exit(21);a.Change(a.Index("Fire Bolt"),-1);if(a.build.Points[a.Index("Fire Bolt")]!=0 || a.skillList.AutoScrollPosition.Y!=previousScroll)Environment.Exit(22);a.Size=new Size(1000,700);Application.DoEvents();a.skillList.AutoScrollPosition=new Point(0,230);a.skillList.Invalidate(true);Application.DoEvents();using(var bmp=new Bitmap(a.Width,a.Height)){a.DrawToBitmap(bmp,new Rectangle(0,0,a.Width,a.Height));bmp.Save("work\\skills-windowed-preview.png");}a.build.Gear[0]="Spirit";a.build.Inventory[0]="Lore";a.ShowGear();Application.DoEvents();using(var bmp=new Bitmap(a.Width,a.Height)){a.DrawToBitmap(bmp,new Rectangle(0,0,a.Width,a.Height));bmp.Save("work\\gear-preview.png");}a.Size=new Size(1600,1050);Application.DoEvents();a.ShowSkills();Application.DoEvents();int wide=a.skillList.Width,tall=a.skillList.Height;if(wide!=Math.Max(850,a.content.ClientSize.Width-60)||tall!=Math.Max(120,a.content.ClientSize.Height-342))Environment.Exit(25);a.ShowGear();Application.DoEvents();a.ShowSkills();Application.DoEvents();if(a.skillList.Width!=wide||a.skillList.Height!=tall)Environment.Exit(26);a.SwitchClass("Warlock");Application.DoEvents();if(a.skillList.Width!=wide||a.skillList.Height!=tall)Environment.Exit(27);using(var bmp=new Bitmap(a.Width,a.Height)){a.DrawToBitmap(bmp,new Rectangle(0,0,a.Width,a.Height));bmp.Save("work\\skills-return-preview.png");}a.ShowUpdates();Application.DoEvents();using(var bmp=new Bitmap(a.Width,a.Height)){a.DrawToBitmap(bmp,new Rectangle(0,0,a.Width,a.Height));bmp.Save("work\\updates-preview.png");}a.WindowState=FormWindowState.Maximized;Application.DoEvents();a.ShowGear();Application.DoEvents();a.ShowSkills();Application.DoEvents();if(a.skillList.Width!=Math.Max(850,a.content.ClientSize.Width-60)||a.skillList.Height!=Math.Max(120,a.content.ClientSize.Height-342))Environment.Exit(28);a.Close(); return;}var app=new Atlas();app.Shown+=(sender,e)=>app.CheckForUpdates(false);Application.Run(app);}
}




