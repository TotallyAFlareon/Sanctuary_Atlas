using System;
using System.Linq;

// Full-animation breakpoints, derived from game animation data, build 3.3.93847:
// https://d2r.org/tools/breakpoints/warlock and the corresponding class pages.
// These are not cast action-frame or weapon-specific attack-speed tables.
public sealed class BreakpointTable {
 public readonly string Name,Assumption;
 public readonly int[] Thresholds;
 public readonly int StartingFrames;
 public BreakpointTable(string name,string assumption,int startingFrames,params int[] thresholds){Name=name;Assumption=assumption;StartingFrames=startingFrames;Thresholds=thresholds;}
 public string Describe(int value){
  int i=0;while(i+1<Thresholds.Length&&value>=Thresholds[i+1])i++;
  string next=i+1<Thresholds.Length?"Next: "+Thresholds[i+1]+"%  (need +"+(Thresholds[i+1]-value)+"%)":"Highest listed breakpoint reached";
  return "Reached: "+Thresholds[i]+"%  |  "+(StartingFrames-i)+" frames\r\n"+next;
 }
 public string Steps(){return String.Join("  /  ",Thresholds.Select(x=>x+"%").ToArray());}
}
public static class Breakpoints {
 static BreakpointTable T(string name,string note,int frames,params int[] values){return new BreakpointTable(name,note,frames,values);}
 public static BreakpointTable For(string cls,string kind){
  if(kind=="FCR"){
   switch(cls){
    case "Amazon":return T("Faster Cast Rate","Standard casting.",19,0,7,14,22,32,48,68,99,152);
    case "Assassin":return T("Faster Cast Rate","Standard casting; traps use attack speed.",16,0,8,16,27,42,65,102,174);
    case "Barbarian":case "Sorceress":return T("Faster Cast Rate",cls=="Sorceress"?"Standard spells; Lightning and Chain Lightning use a different table.":"Standard casting.",13,0,9,20,37,63,105,200);
    case "Druid":return T("Faster Cast Rate","Human form; shapeshift forms differ.",18,0,4,10,19,30,46,68,99,163);
    case "Warlock":case "Necromancer":case "Paladin":return T("Faster Cast Rate",cls=="Necromancer"?"Human form; vampire form differs.":"Standard casting.",15,0,9,18,30,48,75,125);
   }
  }
  if(kind=="FHR"){
   switch(cls){
    case "Amazon":return T("Faster Hit Recovery","Standard bow or one-handed animation; spear/staff can differ.",11,0,6,13,20,32,52,86,174,600);
    case "Assassin":case "Barbarian":case "Paladin":return T("Faster Hit Recovery","Standard human animation.",9,0,7,15,27,48,86,200);
    case "Druid":return T("Faster Hit Recovery","Human form with staff/polearm. Other weapons and shapeshift forms differ.",13,0,5,10,16,26,39,56,86,152,377);
    case "Warlock":case "Necromancer":return T("Faster Hit Recovery",cls=="Necromancer"?"Human form; vampire form differs.":"Standard human animation.",13,0,5,10,16,26,39,56,86,152,377);
    case "Sorceress":return T("Faster Hit Recovery","Standard human animation.",15,0,5,9,14,20,30,42,60,86,142,280);
   }
  }
  if(kind=="FBR"){
   switch(cls){
    case "Amazon":case "Assassin":case "Paladin":return T("Faster Block Rate",cls=="Paladin"?"Normal shield block; Holy Shield uses a different table.":"Normal shield block.",5,0,13,32,86,600);
    case "Barbarian":return T("Faster Block Rate","Normal shield block.",7,0,9,20,42,86,280);
    case "Sorceress":return T("Faster Block Rate","Normal shield block.",9,0,7,15,27,48,86,200);
    case "Druid":case "Warlock":case "Necromancer":return T("Faster Block Rate",cls=="Druid"?"Human form and normal shield block; shapeshift forms differ.":"Normal shield block.",11,0,6,13,20,32,52,86,174,600);
   }
  }
  throw new ArgumentException("Unknown class or breakpoint type.");
 }
}
