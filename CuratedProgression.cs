using System;

// Read-only leveling checkpoints for curated guides. They never alter saved character builds.
public sealed class BuildCheckpoint {
 public readonly int Level;
 public readonly string Skills, Equipment, Advice;

 public BuildCheckpoint(int level, string skills, string equipment, string advice) {
  if(level<1||level>99)throw new ArgumentOutOfRangeException("level");
  Level=level;Skills=skills;Equipment=equipment;Advice=advice;
 }
 public override string ToString(){return "Level "+Level;}
}

public static class CuratedProgression {
 public static readonly BuildCheckpoint[] DemonSummoner={
  new BuildCheckpoint(6,
   "Begin with Summon Goatman and Demonic Mastery; pick up Death Mark when it becomes available. The first skill point is normally earned at level 2.",
   "Identify any +Demon skills weapon or grimoire. Prioritize life and useful resistances over a weak +skill item.",
   "Let your Goatman lead. Death Mark helps focus priority enemies. Each skill still needs its prerequisites."),
  new BuildCheckpoint(12,
   "Continue hard points in Demonic Mastery, keeping Summon Goatman learned. Try Summon Tainted when available for ranged support.",
   "Look for a two-socket nonmagical armor for Stealth at level 17. Keep a resistance shield or useful grimoire.",
   "The second and third demon thresholds depend on 5 and 10 invested points in Demonic Mastery, not +skills from equipment."),
  new BuildCheckpoint(18,
   "Try Summon Defiler at level 18 if you learned Summon Tainted. Consider Blood Oath and the Death Mark to Blood Boil path.",
   "Prepare Stealth and resistances. Don't force a runeword before finding the correct socketed base.",
   "Compare melee and ranged demons; the demon limit is shared. Blood Oath changes how incoming damage is handled."),
  new BuildCheckpoint(24,
   "Engorge becomes available at level 24 if its prerequisite chain is learned. Continue Demonic Mastery toward 10 hard points.",
   "Keep a suitable base for Spirit; Lore becomes usable at level 27 with the required runes and helm.",
   "Use Engorge on a corpse to support an injured demon, and keep Death Mark on dangerous targets."),
  new BuildCheckpoint(30,
   "Bind Demon unlocks at level 30 after its prerequisites, including Engorge and Summon Defiler. Keep investing in the core summon skills.",
   "Look for Spirit, Lore and mercenary Insight where bases and runes permit. Resolve resistance weaknesses before Nightmare.",
   "Experiment with an eligible bound demon. Champions, uniques and super uniques need higher invested Bind Demon ranks."),
  new BuildCheckpoint(45,
   "Develop Demonic Mastery and Summon Goatman, with points in Bind Demon, Blood Oath and needed prerequisites. Choose summons you actually use.",
   "Evaluate a better grimoire against the survivability of a resistance shield. Improve mercenary equipment and keep resistances current.",
   "Nightmare changes the balance between damage and survival. Do not neglect positioning or your mercenary."),
  new BuildCheckpoint(60,
   "Aim toward 20 hard points in Demonic Mastery and Summon Goatman; continue Bind Demon and Blood Oath based on your playstyle.",
   "Prepare for Hell resistance penalties. Replace weak pieces with life and resistances, even if this costs some +skills.",
   "Practice prioritizing dangerous targets and recovering lost summons before difficult Hell encounters."),
  new BuildCheckpoint(75,
   "Push the chosen core skills toward their long-term targets. Bind Demon elite thresholds use hard points: 10 champion, 15 unique, 20 super unique.",
   "Compare budget runewords with your best found items. Balance +skills, resists, life, and mercenary survival.",
   "Different demon damage types and a mercenary help against difficult immune packs; skipping a bad encounter is valid."),
  new BuildCheckpoint(90,
   "Review 20 hard points each in Demonic Mastery, Summon Goatman, Bind Demon and Blood Oath as aspirational goals, not mandatory allocations.",
   "Optimize your preferred summon setup and defensives before chasing luxury items. Keep resistance and breakpoints visible.",
   "Specialize toward pure summoning or a Blood Boil hybrid after testing which controls and summons you enjoy.")
 };

 public static BuildCheckpoint[] For(CuratedBuild build) {
  if(build!=null&&String.Equals(build.Name,"Demon Summoner",StringComparison.OrdinalIgnoreCase))
   return DemonSummoner;
  return new BuildCheckpoint[0];
 }

 public static string Preview(BuildCheckpoint checkpoint) {
  if(checkpoint==null)return "";
  return "LEVEL "+checkpoint.Level+"  /  PROGRESSION CHECKPOINT\r\n"
   +"Skill points from leveling: "+(checkpoint.Level-1)
   +" (plus quest rewards earned so far; at most 12 across all difficulties).\r\n\r\n"
   +"SKILL FOCUS\r\n"+checkpoint.Skills+"\r\n\r\n"
   +"EQUIPMENT FOCUS\r\n"+checkpoint.Equipment+"\r\n\r\n"
   +"PLAYSTYLE\r\n"+checkpoint.Advice
   +"\r\n\r\nThis is a guide preview, not an automatic skill allocation.";
 }
}
