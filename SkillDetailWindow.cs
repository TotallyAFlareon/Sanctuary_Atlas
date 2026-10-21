using System;
using System.Drawing;
using System.Windows.Forms;

// Research UI only: a scrollable, keyboard-accessible view for long tooltips.
public sealed class SkillDetailWindow : Form {
 readonly Label heading;
 readonly TextBox details;
 public SkillDetailWindow(){
  Text="Skill details";
  Size=new Size(540,640);MinimumSize=new Size(360,300);StartPosition=FormStartPosition.CenterParent;
  BackColor=Color.FromArgb(28,21,18);ForeColor=Color.FromArgb(236,224,199);
  heading=new Label{Dock=DockStyle.Top,Height=52,Padding=new Padding(14,14,8,0),Font=new Font("Georgia",14,FontStyle.Bold),ForeColor=Color.FromArgb(210,172,100),AccessibleName="Skill name"};
  details=new TextBox{Dock=DockStyle.Fill,Multiline=true,ReadOnly=true,ScrollBars=ScrollBars.Vertical,WordWrap=true,BorderStyle=BorderStyle.FixedSingle,BackColor=Color.FromArgb(35,27,23),ForeColor=Color.FromArgb(236,224,199),Font=new Font("Segoe UI",10),AccessibleName="Skill details",AccessibleDescription="Scrollable skill details. Use the arrow keys or Page Up and Page Down to read."};
  Controls.Add(details);Controls.Add(heading);
 }
 public string DetailsText {get{return details.Text;}}
 public void SetDetails(string name,string value){
  heading.Text=name;Text=name+" — skill details";
  string text=(value??"").Replace("\r\n","\n").Replace("\n","\r\n");
  if(details.Text==text)return;
  int selection=details.SelectionStart;details.Text=text;details.SelectionStart=Math.Min(selection,text.Length);
  if(details.Focused)details.ScrollToCaret();
 }
}

