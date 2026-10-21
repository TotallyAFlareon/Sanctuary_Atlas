using System;
using System.Collections.Generic;
using System.Globalization;

// A skill has invested points and separate bonuses. No bonus unlocks an unlearned skill.
public sealed class SkillLevels {
 readonly Dictionary<int,int> hard = new Dictionary<int,int>();
 readonly Dictionary<int,int> individualBonus = new Dictionary<int,int>();
 readonly Dictionary<int,int> treeBonus = new Dictionary<int,int>();
 readonly Dictionary<int,int> trees = new Dictionary<int,int>();
 public int AllSkillsBonus { get; set; }
 public int CharacterLevel { get; set; }
 public void SetTree(int skillId,int treeId){trees[skillId]=treeId;}
 public void SetHard(int skillId,int value){if(value<0||value>20)throw new ArgumentOutOfRangeException("value");hard[skillId]=value;}
 public void SetSkillBonus(int skillId,int value){if(value<0)throw new ArgumentOutOfRangeException("value");individualBonus[skillId]=value;}
 public void SetTreeBonus(int treeId,int value){if(value<0)throw new ArgumentOutOfRangeException("value");treeBonus[treeId]=value;}
 static int Get(Dictionary<int,int> data,int key){int value;return data.TryGetValue(key,out value)?value:0;}
 public int Hard(int skillId){return Get(hard,skillId);}
 public int Effective(int skillId){int h=Hard(skillId);if(h==0)return 0;int tree;int bonus=trees.TryGetValue(skillId,out tree)?Get(treeBonus,tree):0;return checked(h+Math.Max(0,AllSkillsBonus)+bonus+Get(individualBonus,skillId));}
 public SkillLevels Clone(){var c=new SkillLevels{AllSkillsBonus=AllSkillsBonus,CharacterLevel=CharacterLevel};foreach(var pair in hard)c.hard.Add(pair.Key,pair.Value);foreach(var pair in individualBonus)c.individualBonus.Add(pair.Key,pair.Value);foreach(var pair in treeBonus)c.treeBonus.Add(pair.Key,pair.Value);foreach(var pair in trees)c.trees.Add(pair.Key,pair.Value);return c;}
}

// Strict arithmetic, never executes code. Unsupported data fails closed.
public static class SkillFormula {
 public static decimal Evaluate(string expression,int skillId,SkillLevels levels,IDictionary<string,decimal> modifiers){
  if(expression==null)throw new ArgumentNullException("expression");if(levels==null)throw new ArgumentNullException("levels");
  return new Parser(expression,skillId,levels,modifiers,0).Parse();
 }
 sealed class Parser {
  readonly string input; readonly int id,depth; readonly SkillLevels levels; readonly IDictionary<string,decimal> modifiers; int pos; bool suppressed; readonly HashSet<string> usedReferences=new HashSet<string>(); bool hasActiveReference;
  public Parser(string source,int skill,SkillLevels state,IDictionary<string,decimal> vars,int d){if(source.Length>8192||d>12)throw new FormatException("Formula is too complex.");input=source;id=skill;levels=state;modifiers=vars;depth=d;}
  public decimal Parse(){decimal v=Expression();Space();if(pos!=input.Length)throw Error("Unexpected text");return v;}
  FormatException Error(string message){return new FormatException(message+" at position "+pos+".");}
  void Space(){while(pos<input.Length&&Char.IsWhiteSpace(input[pos]))pos++;}
  bool Match(char c){Space();if(pos<input.Length&&input[pos]==c){pos++;return true;}return false;}
  bool MatchPower(){Space();if(pos+1<input.Length&&input[pos]=='*'&&input[pos+1]=='*'){pos+=2;return true;}return false;}
  decimal Expression(){
   decimal condition=Comparison();
   if(!Match('?'))return condition;
   bool prior=suppressed;decimal yes,no;
   try{suppressed=prior||condition==0;yes=Expression();if(!Match(':'))throw Error("Expected colon");suppressed=prior||condition!=0;no=Expression();}
   finally{suppressed=prior;}
   return prior?0:(condition!=0?yes:no);
  }
  bool MatchOperator(string op){Space();if(pos+op.Length<=input.Length&&String.CompareOrdinal(input,pos,op,0,op.Length)==0){pos+=op.Length;return true;}return false;}
  decimal Comparison(){
   decimal a=Sum();
   string[] ops={"<=",">=","==","!=","<",">"};
   foreach(string op in ops)if(MatchOperator(op)){decimal b=Sum();if(suppressed)return 0;switch(op){case "<=":return a<=b?1:0;case ">=":return a>=b?1:0;case "==":return a==b?1:0;case "!=":return a!=b?1:0;case "<":return a<b?1:0;default:return a>b?1:0;}}
   return a;
  }
  decimal Sum(){decimal v=Term();for(;;){if(Match('+')){decimal a=Term();v=suppressed?0:v+a;}else if(Match('-')){decimal a=Term();v=suppressed?0:v-a;}else return v;}}
  decimal Term(){decimal v=Unary();for(;;){Space();if(pos+1<input.Length&&input[pos]=='*'&&input[pos+1]=='*')return v;if(Match('*')){decimal n=Unary();v=suppressed?0:v*n;}else if(Match('/')){decimal divisor=Unary();if(!suppressed&&divisor==0)throw Error("Division by zero");v=suppressed?0:v/divisor;}else return v;}}
  decimal Unary(){if(Match('+'))return Unary();if(Match('-'))return -Unary();return Power();}
  decimal Power(){decimal v=Primary();if(MatchPower()){decimal exp=Unary();if(suppressed)return 0;if(exp!=Decimal.Truncate(exp)||exp<-28||exp>28)throw Error("Unsupported exponent");int n=(int)exp;decimal result=1;for(int i=0;i<Math.Abs(n);i++)result=checked(result*v);return n<0?1/result:result;}return v;}
  decimal Primary(){Space();if(Match('(')){decimal v=Expression();if(!Match(')'))throw Error("Expected closing parenthesis");return v;}if(pos>=input.Length)throw Error("Expected value");if(Char.IsDigit(input[pos])||input[pos]=='.')return Number();if(Char.IsLetter(input[pos])||input[pos]=='_')return Identifier();throw Error("Expected value");}
  decimal Number(){Space();int start=pos;while(pos<input.Length&&(Char.IsDigit(input[pos])||input[pos]=='.'))pos++;decimal value;if(!Decimal.TryParse(input.Substring(start,pos-start),NumberStyles.AllowDecimalPoint,CultureInfo.InvariantCulture,out value))throw Error("Invalid number");return value;}
  decimal Identifier(){int start=pos;while(pos<input.Length&&(Char.IsLetterOrDigit(input[pos])||input[pos]=='_'))pos++;string name=input.Substring(start,pos-start).ToLowerInvariant();
   if(!Match('(')){if(name=="lvl")return suppressed?0:levels.Effective(id);if(name=="charlvl")return suppressed?0:levels.CharacterLevel;decimal value;if(modifiers!=null&&modifiers.TryGetValue(name,out value))return suppressed?0:value;if(suppressed)return 0;throw Error("Unsupported variable "+name);}
   if(name=="synergy")return Synergy();
   decimal a=Expression();if(name=="min"||name=="max"||name=="idiv"){if(!Match(','))throw Error("Expected comma");decimal b=Expression();if(!Match(')'))throw Error("Expected closing parenthesis");if(name=="min")return Math.Min(a,b);if(name=="max")return Math.Max(a,b);if(suppressed)return 0;if(b==0)throw Error("Division by zero");return Decimal.Truncate(a/b);}
   if(!Match(')'))throw Error("Expected closing parenthesis");
   if(name=="floor")return suppressed?0:Decimal.Floor(a);
   if(name=="blvl"||name=="slvl"){if(a<0||a>Int32.MaxValue||a!=Decimal.Truncate(a))throw Error("Invalid skill id");int result=suppressed?0:(name=="blvl"?levels.Hard((int)a):levels.Effective((int)a));if(suppressed)return 0;usedReferences.Add(name+":"+(int)a);if(result>0)hasActiveReference=true;return result;}
   throw Error("Unsupported function "+name);
  }
  decimal Synergy(){Space();if(!Match('\''))throw Error("Expected quoted synergy formula");int start=pos;while(pos<input.Length&&input[pos]!='\'')pos++;if(pos>=input.Length)throw Error("Unterminated synergy formula");string sub=input.Substring(start,pos-start);pos++;if(!Match(')'))throw Error("Expected closing parenthesis");if(suppressed)return 0;var child=new Parser(sub,id,levels,modifiers,depth+1);decimal result=child.Parse();if(child.usedReferences.Count>1)throw Error("Unsupported synergy expression");usedReferences.UnionWith(child.usedReferences);if(child.hasActiveReference)hasActiveReference=true;return child.usedReferences.Count==0||child.hasActiveReference?result:0;}
 }
}

public sealed class SkillPointPreview {
 public decimal? Current { get; private set; }
 public decimal? Next { get; private set; }
 public int CurrentHard { get; private set; }
 public int CurrentEffective { get; private set; }
 public static SkillPointPreview Calculate(string formula,int skillId,SkillLevels levels,IDictionary<string,decimal> modifiers){
  int hard=levels.Hard(skillId);var result=new SkillPointPreview{CurrentHard=hard,CurrentEffective=levels.Effective(skillId)};
  if(hard>0)result.Current=SkillFormula.Evaluate(formula,skillId,levels,modifiers);
  if(hard<20){var next=levels.Clone();next.SetHard(skillId,hard+1);result.Next=SkillFormula.Evaluate(formula,skillId,next,modifiers);}
  return result;
 }
}


