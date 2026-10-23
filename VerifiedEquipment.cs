using System;
using System.Collections.Generic;

// A deliberately small, fail-closed starting point for a future equipment
// builder. These records describe unsocketed unique items and only the stats
// Sanctuary Atlas currently models. An unknown item is not a valid item.
public enum EquipmentValidation { ValidTrackedStats, Invalid, Unverified }
public sealed class VerifiedEquipmentRecord {
 public readonly string Name, Base, Slot;
 public readonly int AllSkills, FireSkills, Fcr, Fhr, Fbr;
 public VerifiedEquipmentRecord(string name,string itemBase,string slot,int all,int fire,int fcr,int fhr,int fbr){
  Name=name;Base=itemBase;Slot=slot;AllSkills=all;FireSkills=fire;Fcr=fcr;Fhr=fhr;Fbr=fbr;
 }
}
public sealed class EquipmentCandidate {
 public string Name, Base, Slot, Quality;
 public int Sockets, AllSkills, FireSkills, Fcr, Fhr, Fbr;
 // Additional skill modifiers cannot be checked until their sources are modelled.
 public bool HasOtherTrackedSkillBonuses;
}
public static class VerifiedEquipment {
 // Sources: Blizzard's Arreat Summit item and caster equipment references.
 static readonly VerifiedEquipmentRecord[] records={
  new VerifiedEquipmentRecord("Wizardspike","Bone Knife","Weapon",0,0,50,0,0),
  new VerifiedEquipmentRecord("Suicide Branch","Burnt Wand","Weapon",1,0,50,0,0),
  new VerifiedEquipmentRecord("Spectral Shard","Blade","Weapon",0,0,50,0,0),
  new VerifiedEquipmentRecord("Hexfire","Shamshir","Weapon",0,3,0,0,0),
  new VerifiedEquipmentRecord("Harlequin Crest","Shako","Helm",2,0,0,0,0),
  new VerifiedEquipmentRecord("Peasant Crown","War Hat","Helm",1,0,0,0,0),
  new VerifiedEquipmentRecord("Tarnhelm","Skull Cap","Helm",1,0,0,0,0),
  new VerifiedEquipmentRecord("Skin of the Vipermagi","Serpentskin Armor","Armor",1,0,30,0,0),
  new VerifiedEquipmentRecord("Que-Hegan's Wisdom","Mage Plate","Armor",1,0,20,20,0),
  new VerifiedEquipmentRecord("Lidless Wall","Grim Shield","Off-hand",1,0,20,0,0),
  new VerifiedEquipmentRecord("The Stone of Jordan","Ring","Ring",1,0,0,0,0),
  new VerifiedEquipmentRecord("Bul-Kathos' Wedding Band","Ring","Ring",1,0,0,0,0),
  new VerifiedEquipmentRecord("Mara's Kaleidoscope","Amulet","Amulet",2,0,0,0,0),
  new VerifiedEquipmentRecord("Magefist","Light Gauntlets","Gloves",0,1,20,0,0)
 };
 static bool Eq(string a,string b){return String.Equals(a,b,StringComparison.OrdinalIgnoreCase);}
 public static List<VerifiedEquipmentRecord> All(){return new List<VerifiedEquipmentRecord>(records);}
 public static VerifiedEquipmentRecord Find(string name){foreach(var record in records)if(Eq(record.Name,name))return record;return null;}
 public static bool FitsSlot(VerifiedEquipmentRecord record,string slot){return record!=null&&(Eq(slot,record.Slot)||(Eq(record.Slot,"Ring")&&(Eq(slot,"Ring 1")||Eq(slot,"Ring 2"))));}
 public static List<VerifiedEquipmentRecord> ForSlot(string slot){var result=new List<VerifiedEquipmentRecord>();foreach(var record in records)if(FitsSlot(record,slot))result.Add(record);return result;}
 public static ItemSkillBonuses Bonuses(VerifiedEquipmentRecord record){
  if(record==null)throw new ArgumentNullException("record");
  var lines=new List<string>();if(record.AllSkills>0)lines.Add("All Skills: +"+record.AllSkills);if(record.FireSkills>0)lines.Add("Fire Skills: +"+record.FireSkills);
  return new ItemSkillBonuses{Lines=String.Join("\n",lines.ToArray()),Fcr=record.Fcr,Fhr=record.Fhr,Fbr=record.Fbr,VerifiedName=record.Name};
 }
 public static EquipmentValidation Check(EquipmentCandidate item){
  if(item==null||String.IsNullOrWhiteSpace(item.Name)||String.IsNullOrWhiteSpace(item.Base)||String.IsNullOrWhiteSpace(item.Slot)||String.IsNullOrWhiteSpace(item.Quality))return EquipmentValidation.Unverified;
  VerifiedEquipmentRecord match=null;foreach(var record in records)if(Eq(record.Name,item.Name)){match=record;break;}
  if(match==null)return EquipmentValidation.Unverified;
  if(!Eq(item.Quality,"Unique")||!FitsSlot(match,item.Slot))return EquipmentValidation.Invalid;
  if(!Eq(item.Base,match.Base))return EquipmentValidation.Unverified;
  // Socketed variants and upgraded bases require separate game rules.
  if(item.Sockets!=0)return EquipmentValidation.Unverified;
  if(item.HasOtherTrackedSkillBonuses||item.AllSkills!=match.AllSkills||item.FireSkills!=match.FireSkills||item.Fcr!=match.Fcr||item.Fhr!=match.Fhr||item.Fbr!=match.Fbr)return EquipmentValidation.Invalid;
  return EquipmentValidation.ValidTrackedStats;
 }
}

