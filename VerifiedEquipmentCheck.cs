using System;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
class VerifiedEquipmentCheck {
 static void Expect(EquipmentCandidate x,EquipmentValidation expected){var got=VerifiedEquipment.Check(x);if(got!=expected)throw new Exception(x.Name+": expected "+expected+", got "+got);}
 static void Main(){
  var shako=new EquipmentCandidate{Name="Harlequin Crest",Base="Shako",Slot="Helm",Quality="Unique",AllSkills=2};
  Expect(shako,EquipmentValidation.ValidTrackedStats);
  shako.AllSkills=1000;Expect(shako,EquipmentValidation.Invalid);
  shako.AllSkills=2;shako.Fcr=20;Expect(shako,EquipmentValidation.Invalid);
  shako.Fcr=0;shako.Slot="Gloves";Expect(shako,EquipmentValidation.Invalid);
  shako.Slot="Helm";shako.Sockets=1;Expect(shako,EquipmentValidation.Unverified);
  shako.Sockets=0;shako.HasOtherTrackedSkillBonuses=true;Expect(shako,EquipmentValidation.Invalid);
  Expect(new EquipmentCandidate{Name="Mystery",Base="Shako",Slot="Helm",Quality="Unique"},EquipmentValidation.Unverified);
  Expect(new EquipmentCandidate{Name="Skin of the Vipermagi",Base="Serpentskin Armor",Slot="Armor",Quality="Unique",AllSkills=1,Fcr=30},EquipmentValidation.ValidTrackedStats);
  Expect(new EquipmentCandidate{Name="Magefist",Base="Light Gauntlets",Slot="Gloves",Quality="Unique",FireSkills=1,Fcr=20},EquipmentValidation.ValidTrackedStats);
  Expect(new EquipmentCandidate{Name="The Stone of Jordan",Base="Ring",Slot="Ring 2",Quality="Unique",AllSkills=1},EquipmentValidation.ValidTrackedStats);
  var shakoRecord=VerifiedEquipment.Find("Harlequin Crest");
  if(!VerifiedEquipment.FitsSlot(shakoRecord,"Helm")||VerifiedEquipment.FitsSlot(shakoRecord,"Ring 1")||!VerifiedEquipment.FitsSlot(VerifiedEquipment.Find("The Stone of Jordan"),"Ring 2"))throw new Exception("slot checks");
  if(VerifiedEquipment.ForSlot("Helm").Count!=3||VerifiedEquipment.ForSlot("Weapon").Count!=4||VerifiedEquipment.ForSlot("Belt").Count!=0)throw new Exception("slot choices");
  var build=new Build();build.Gear[2]=shakoRecord.Name;build.GearSkillBonuses[2]=VerifiedEquipment.Bonuses(shakoRecord);
  var mage=VerifiedEquipment.Find("Magefist");build.Gear[4]=mage.Name;build.GearSkillBonuses[4]=VerifiedEquipment.Bonuses(mage);
  if(build.GearSkillBonuses[2].Lines!="All Skills: +2"||build.GearSkillBonuses[4].Lines!="Fire Skills: +1"||GearSpeedBonuses.ForBuild(build).Fcr!=20)throw new Exception("preset bonuses");
  string xml;var serializer=new XmlSerializer(typeof(Build));using(var w=new StringWriter()){serializer.Serialize(w,build);xml=w.ToString();}
  Build loaded;using(var r=new StringReader(xml))loaded=(Build)serializer.Deserialize(r);
  if(loaded.GearSkillBonuses[2].VerifiedName!="Harlequin Crest"||loaded.GearSkillBonuses[4].Fcr!=20)throw new Exception("preset save round trip");
  var old=xml.Replace("<VerifiedName>Harlequin Crest</VerifiedName>","").Replace("<VerifiedName>Magefist</VerifiedName>","");
  using(var r=new StringReader(old))loaded=(Build)serializer.Deserialize(r);
  if(!String.IsNullOrEmpty(loaded.GearSkillBonuses[2].VerifiedName)||loaded.GearSkillBonuses[2].Lines!="All Skills: +2")throw new Exception("legacy migration");
  Console.WriteLine("Tracked unique-item checks passed; unsupported items remain unverified.");
 }
}
