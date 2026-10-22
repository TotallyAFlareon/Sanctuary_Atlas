using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

// Source-neutral definitions. Each verified source must specify rounding for each line.
public sealed class SkillTooltipStat {
 public string Label, Formula, FormulaEnd, Suffix, Template;
 public bool WholeNumber, Signed, Boolean;
}
public sealed class SkillTooltipDefinition {
 public int Id, RequiredLevel;
 public string Name, Description, Prerequisites;
 public readonly List<SkillTooltipStat> Stats=new List<SkillTooltipStat>();
 public readonly List<SkillTooltipStat> Synergies=new List<SkillTooltipStat>();
 public readonly List<string> Notes=new List<string>();
}
public static class SkillTooltipRenderer {
 static string Number(decimal d,bool whole){return (whole?Decimal.Truncate(d):d).ToString("0.##",CultureInfo.InvariantCulture);}
 static string SignedNumber(decimal value,SkillTooltipStat stat){if(stat.Boolean)return value==0?"No":"Yes";return (stat.Signed&&value>0?"+":"")+Number(value,stat.WholeNumber);}
 static string Show(decimal? start,decimal? end,SkillTooltipStat stat){if(!start.HasValue)return "unavailable";return SignedNumber(start.Value,stat)+(stat.FormulaEnd==null?"":(end.HasValue?"–"+Number(end.Value,stat.WholeNumber):"–unavailable"))+(stat.Suffix??"");}
 static decimal? Evaluate(string formula,int id,SkillLevels state,IDictionary<string,decimal> modifiers){try{return SkillFormula.Evaluate(formula,id,state,modifiers);}catch(FormatException){return null;}catch(ArithmeticException){return null;}}
 static void RenderLines(StringBuilder sb,SkillTooltipDefinition skill,IEnumerable<SkillTooltipStat> stats,SkillLevels state,IDictionary<string,decimal> modifiers){foreach(var stat in stats){decimal? start=Evaluate(stat.Formula,skill.Id,state,modifiers);decimal? end=stat.FormulaEnd==null?null:Evaluate(stat.FormulaEnd,skill.Id,state,modifiers);if(stat.Template!=null&&stat.Template.Contains("{end}")){string first=start.HasValue?SignedNumber(start.Value,stat):"unavailable";string second=end.HasValue?Number(end.Value,stat.WholeNumber):"unavailable";sb.AppendLine(stat.Template.Replace("{value}",first).Replace("{end}",second));}else{string value=Show(start,end,stat);sb.AppendLine(stat.Template==null?stat.Label+": "+value:stat.Template.Replace("{value}",value));}}}
 public static string Render(SkillTooltipDefinition skill,SkillLevels levels,IDictionary<string,decimal> modifiers){
  return RenderWithModifiers(skill,levels,delegate(SkillLevels state){return modifiers;});
 }
 // Modifiers are recomputed separately for each preview state.
 public static string RenderWithModifiers(SkillTooltipDefinition skill,SkillLevels levels,Func<SkillLevels,IDictionary<string,decimal>> modifiers){
  if(skill==null)throw new ArgumentNullException("skill");if(levels==null)throw new ArgumentNullException("levels");
  if(modifiers==null)throw new ArgumentNullException("modifiers");
  var sb=new StringBuilder();int hard=levels.Hard(skill.Id), effective=levels.Effective(skill.Id);
  sb.AppendLine(skill.Name);if(!String.IsNullOrWhiteSpace(skill.Description))sb.AppendLine(skill.Description);
  sb.AppendLine("Character level required: "+skill.RequiredLevel);
  sb.AppendLine(String.IsNullOrEmpty(skill.Prerequisites)?"No prerequisite":"Requires: "+skill.Prerequisites);
  sb.AppendLine("Invested points: "+hard+" / 20");if(effective>0&&hard!=effective)sb.AppendLine("Effective skill level: "+effective);
  if(skill.Stats.Count>0){
   if(effective==0){var first=levels.Clone();first.SetHard(skill.Id,1);sb.AppendLine("\nFirst point preview:");RenderLines(sb,skill,skill.Stats,first,modifiers(first));}
   else{sb.AppendLine("\nCurrent values:");RenderLines(sb,skill,skill.Stats,levels,modifiers(levels));if(hard<20){var next=levels.Clone();next.SetHard(skill.Id,hard+1);sb.AppendLine("\nNext point:");RenderLines(sb,skill,skill.Stats,next,modifiers(next));}}
  }
  if(skill.Synergies.Count>0){var preview=levels.Clone();if(effective==0)preview.SetHard(skill.Id,1);sb.AppendLine("\nSynergy descriptions:");RenderLines(sb,skill,skill.Synergies,preview,modifiers(preview));}
  foreach(string note in skill.Notes)sb.AppendLine(note);
  return sb.ToString().TrimEnd();
 }
}

