using System;
using System.Drawing;
using System.Windows.Forms;

// Experimental inline view; the contents are scrollable independently of the tree.
public sealed class SkillDetailView : GothicPanel {
 readonly Label heading;
 readonly TextBox details;
 readonly CheckBox pin;
 public SkillDetailView(){
  BackColor=Color.FromArgb(35,27,23);Padding=new Padding(6);AccessibleName="Skill details";
  var top=new Panel{Dock=DockStyle.Top,Height=46};
  heading=new Label{Dock=DockStyle.Fill,Padding=new Padding(9,10,4,0),Text="SKILL DETAILS",Font=new Font("Georgia",13,FontStyle.Bold),ForeColor=Color.FromArgb(210,172,100)};
  pin=new CheckBox{Dock=DockStyle.Right,Width=60,Text="PIN",ForeColor=Color.FromArgb(236,224,199),AccessibleName="Pin selected skill",AccessibleDescription="Keep this skill displayed while moving the mouse."};
  top.Controls.Add(heading);top.Controls.Add(pin);
  details=new TextBox{Dock=DockStyle.Fill,Multiline=true,ReadOnly=true,ScrollBars=ScrollBars.Vertical,WordWrap=true,BorderStyle=BorderStyle.FixedSingle,BackColor=Color.FromArgb(35,27,23),ForeColor=Color.FromArgb(236,224,199),Font=new Font("Segoe UI",10),AccessibleName="Skill details",AccessibleDescription="Scrollable skill details. Use the arrow keys or Page Up and Page Down to read."};
  Controls.Add(details);Controls.Add(top);SetDetails("SKILL DETAILS","Hover over a skill to see its effects.");
 }
 public bool Pinned {get{return pin.Checked;}set{pin.Checked=value;}}
 public string DetailsText {get{return details.Text;}}
 public void SetDetails(string name,string value){
  heading.Text=name;
  string text=(value??"").Replace("\r\n","\n").Replace("\n","\r\n");
  if(details.Text==text)return;
  int selection=details.SelectionStart;details.Text=text;details.SelectionStart=Math.Min(selection,text.Length);
  if(details.Focused)details.ScrollToCaret();
 }
}

