using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

// Keep native tooltip behavior while drawing the skill preview in the app's palette.
public sealed class SkillHoverTip : ToolTip {
 static readonly Color Gold=Color.FromArgb(210,172,100);
 static readonly Color TextColor=Color.FromArgb(236,224,199);
 static readonly TextFormatFlags Wrap=TextFormatFlags.WordBreak|TextFormatFlags.NoPadding|TextFormatFlags.NoPrefix;
 readonly Font titleFont=new Font("Georgia",11,FontStyle.Bold);
 readonly Font bodyFont=new Font("Segoe UI",9);
 readonly Font hintFont=new Font("Segoe UI",8,FontStyle.Italic);
 const int Pad=14, Header=34, Footer=28;
 public SkillHoverTip(){
  OwnerDraw=true;AutoPopDelay=30000;InitialDelay=500;ReshowDelay=100;
  Popup+=SizeTip;Draw+=DrawTip;
 }
 static string Body(string value){int i=(value??"").IndexOf('\n');return i<0?"":value.Substring(i+1).TrimStart('\r','\n');}
 static string Title(string value){value=value??"";int i=value.IndexOf('\n');return (i<0?value:value.Substring(0,i)).TrimEnd('\r');}
 void SizeTip(object sender,PopupEventArgs e){
  string text=GetToolTip(e.AssociatedControl)??"";
  Rectangle screen=Screen.FromControl(e.AssociatedControl).WorkingArea;
  int width=Math.Min(480,Math.Max(200,screen.Width-32));
  Size measured=TextRenderer.MeasureText(Body(text),bodyFont,new Size(width-Pad*2,8192),Wrap);
  int available=Math.Max(120,screen.Height-70);
  int height=Math.Min(available,Header+Footer+Pad+measured.Height);
  e.ToolTipSize=new Size(width,Math.Max(Header+Footer+Pad,height));
 }
 void DrawTip(object sender,DrawToolTipEventArgs e){
  Rectangle bounds=e.Bounds;
  using(var brush=new LinearGradientBrush(bounds,Color.FromArgb(42,31,24),Color.FromArgb(18,15,14),90f))e.Graphics.FillRectangle(brush,bounds);
  using(var border=new Pen(Color.FromArgb(164,122,67)))e.Graphics.DrawRectangle(border,0,0,bounds.Width-1,bounds.Height-1);
  using(var inner=new Pen(Color.FromArgb(78,56,35)))e.Graphics.DrawRectangle(inner,3,3,bounds.Width-7,bounds.Height-7);
  TextRenderer.DrawText(e.Graphics,Title(e.ToolTipText).ToUpperInvariant(),titleFont,new Rectangle(Pad,8,bounds.Width-Pad*2,24),Gold,TextFormatFlags.EndEllipsis|TextFormatFlags.NoPrefix);
  using(var line=new Pen(Color.FromArgb(100,74,46)))e.Graphics.DrawLine(line,Pad,Header-2,bounds.Width-Pad,Header-2);
  var body=new Rectangle(Pad,Header,bounds.Width-Pad*2,Math.Max(1,bounds.Height-Header-Footer-4));
  TextRenderer.DrawText(e.Graphics,Body(e.ToolTipText),bodyFont,body,TextColor,Wrap|TextFormatFlags.EndEllipsis);
  TextRenderer.DrawText(e.Graphics,"Select DETAIL to read the full skill.",hintFont,new Rectangle(Pad,bounds.Height-Footer,bounds.Width-Pad*2,20),Gold,TextFormatFlags.EndEllipsis|TextFormatFlags.NoPrefix);
 }
 protected override void Dispose(bool disposing){if(disposing){titleFont.Dispose();bodyFont.Dispose();hintFont.Dispose();}base.Dispose(disposing);}
}

