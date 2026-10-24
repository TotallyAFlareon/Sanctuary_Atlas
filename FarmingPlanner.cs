using System;
using System.Collections.Generic;
using System.Linq;

// Short, practical routes. Ratings describe relative risk and travel time, not a guarantee of safety.
public sealed class FarmingRoute {
 public int Act, Safety, Rank;
 public string Name, Goals, Start, Focus, Caution, GuideEntry;
 public FarmingRoute(int act,int safety,int rank,string name,string goals,string start,string focus,string caution,string guideEntry){Act=act;Safety=safety;Rank=rank;Name=name;Goals=goals;Start=start;Focus=focus;Caution=caution;GuideEntry=guideEntry;}
 public override string ToString(){return "ACT "+Act+"  •  "+Name;}
 public bool Supports(string goal){return goal=="Any goal"||Goals.IndexOf(goal,StringComparison.OrdinalIgnoreCase)>=0;}
 public string Details(){return "ACT "+Act+"  •  "+Goals.ToUpperInvariant()+"\r\n\r\nSTART\r\n"+Start+"\r\n\r\nWHY RUN IT\r\n"+Focus+"\r\n\r\nSAFETY\r\n"+Caution+"\r\n\r\nGUIDE BOOK\r\nOpen "+GuideEntry+" for the related quest and area notes.";}
}

public static class FarmingPlanner {
 public static readonly FarmingRoute[] Routes={
  new FarmingRoute(1,1,1,"Cold Plains loop","Experience|Items","Cold Plains waypoint → Burial Grounds → short Stony Field loop.","Open terrain, frequent exits and small packs make this a forgiving place to level and replace basic gear.","Keep the loop short. Leave when archer or cursed packs consume too many potions.","Cold Plains & Burial Grounds"),
  new FarmingRoute(1,2,2,"Countess rune run","Runes|Items","Black Marsh waypoint → Forgotten Tower level 5.","A compact route for early runeword ingredients and an optional boss kill.","Tower stairs are tight. Do not force the run when ghosts, archers or a bad champion pack stall your build.","Black Marsh & Forgotten Tower"),
  new FarmingRoute(1,2,3,"The Pit clear","Items|Experience","Outer Cloister waypoint → Tamoe Highland → The Pit.","A Hell item route with boss packs and two dungeon levels when your build is ready.","Mixed immunities and archers can be punishing. Skip packs you cannot clear efficiently.","Tamoe Highland & The Pit"),
  new FarmingRoute(2,1,1,"Stony Tomb clear","Experience|Items","Walk from Lut Gholein into Rocky Waste → Stony Tomb levels 1–2.","A self-contained dungeon with room to position a mercenary and summons; friendlier than narrow Maggot Lair corridors.","Clear the entrance first, use doorways to control packs, and leave dangerous uniques behind.","Rocky Waste & Stony Tomb"),
  new FarmingRoute(2,2,2,"Ancient Tunnels clear","Items|Experience","Lost City waypoint → find the trapdoor → clear Ancient Tunnels.","A classic Hell item route with packs and a golden chest once you can handle the area comfortably.","The entrance location varies. Check boss immunities and use a short reset if the map or pack mix is poor.","Lost City & Ancient Tunnels"),
  new FarmingRoute(2,2,3,"Summoner key pass","Keys|Items","Arcane Sanctuary waypoint → follow a branch to the Summoner.","A quick targeted Hell run when collecting Key of Hate chances alongside normal loot.","The paths are exposed and monster mixes vary. Treat it as a key attempt, not a full clear.","Arcane Sanctuary & Tombs"),
  new FarmingRoute(3,1,1,"Lower Kurast chest loop","Runes|Items","Lower Kurast waypoint → nearby huts, containers and racks → town portal out.","A low-combat item check focused on nearby containers and fast resets.","It is poor experience if you avoid monsters. Leave hostile packs behind when the goal is a short item run.","Lower Kurast"),
  new FarmingRoute(3,2,2,"Flayer Jungle loop","Experience|Items","Flayer Jungle waypoint → short exterior loop → return when inventory fills.","Dense outdoor groups can provide quick experience while moving through an open route.","Small fast monsters and branching paths can overwhelm fragile characters. Do not full-clear every branch.","Spider Forest & Flayer Jungle"),
  new FarmingRoute(3,3,3,"Council and Mephisto","Boss loot|Items","Durance of Hate level 2 waypoint → Mephisto; add Travincal only when it is quick.","An act-boss route for gear-focused runs with an optional Council detour.","Council Hydras and Durance packs raise the risk sharply. Separate this from a safer jungle route.","Travincal & Durance of Hate"),
  new FarmingRoute(4,2,1,"Plains and River progression loop","Experience|Items","City of the Damned or River of Flame waypoint → small open loop.","A controlled way to gain experience while progressing toward Izual, Hellforge or Chaos.","Act IV has few forgiving detours. Pull one group at a time and keep a town portal behind you.","River of Flame & Hellforge"),
  new FarmingRoute(4,3,2,"Chaos Sanctuary","Experience|Items|Boss loot","River of Flame waypoint → Chaos Sanctuary → Diablo.","High monster density, seal bosses and Diablo make this a rewarding endgame route.","This is not a low-risk farm. Bring resistances, a plan for immunities and enough damage to keep the run moving.","Chaos Sanctuary"),
  new FarmingRoute(5,2,1,"Eldritch and Shenk","Experience|Items","Frigid Highlands waypoint → Eldritch → Shenk when the approach is clear.","Two nearby elite targets with a short retreat path make a quick repeat route.","Let your mercenary and summons engage first. Do not stand under falling projectiles after Shenk dies.","Bloody Foothills & Frigid Highlands"),
  new FarmingRoute(5,2,2,"Pindleskin stop","Items|Boss loot","Anya's red portal → Nihlathak's Temple entrance → Pindleskin.","A very short elite target that fits at the start or end of a larger route.","The pack begins close to the entrance. Summon and raise defenses before moving forward.","Nihlathak's Temple & Pindleskin"),
  new FarmingRoute(5,3,3,"Nihlathak key run","Keys|Items","Anya's portal → Halls of Vaught → Nihlathak.","A targeted Hell run for Key of Destruction chances and normal drops.","Corpse Explosion is a major danger. Keep distance, control corpses and skip a bad map.","Frozen River & Halls of Vaught"),
  new FarmingRoute(5,3,4,"Worldstone and Baal","Experience|Items|Boss loot","Worldstone Keep level 2 waypoint → Throne of Destruction → Baal.","A long endgame experience route with minion waves and an act-boss finish.","Varied monster packs and immunities make this a higher-risk route until gear, resistances and clear speed are ready.","Arreat Summit & Worldstone Keep")
 };

 public static List<FarmingRoute> Find(int act,string goal,int safety){return Routes.Where(r=>(act==0||r.Act==act)&&r.Safety<=safety&&r.Supports(goal)).OrderBy(r=>r.Safety).ThenBy(r=>r.Act).ThenBy(r=>r.Rank).ToList();}
}

