using UnityEngine; 
using Mirror;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.AI;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;
using System.Data;
namespace dragon.mirror{
//The abstract keyword enables you to create classes and class members that are incomplete and must be implemented in a derived class.
public class StatModifier
{
    public enum Stat { Agility, Strength, Fortitude, Arcana, MagicResistance, PoisonResistance, DiseaseResistance, ColdResistance, FireResistance, Armor, CdReduction , Lifesteal, SpellAdd }
    public Stat TargetStat { get; private set; }
    public int Value { get; private set; }
    public int Rank { get; private set; }
    public float Duration { get; private set; }
    public float MaxDuration { get; private set; }
    public string BuffName { get; private set; }
    public bool Buff { get; private set; }
	public bool Food { get; private set; }
	public bool Potion { get; private set; }
	public string ExpirationTime { get; private set; }
	

	
    public StatModifier(Stat targetStat, int value, float duration, float maxDuration, string buffName, bool buff, int rank, bool food, bool potion, string expirationTime)
    {
        TargetStat = targetStat;
        Value = value;
        Duration = duration;
		MaxDuration = maxDuration;
		BuffName = buffName;
		Buff = buff;
		Rank = rank;
		Food = food;
		Potion = potion;
		ExpirationTime = expirationTime;
	}
}
public class StatsHandler
{
    public int Agility { get; private set; }
    public int Strength { get; private set; }
    public int Fortitude { get; private set; }
    public int Arcana { get; private set; }
    public int Armor { get; private set; }
    public int MagicResist { get; private set; }
    public int FireResist { get; private set; }
    public int ColdResist { get; private set; }
    public int DiseaseResist { get; private set; }
    public int PoisonResist { get; private set; }
    public int Dodge { get; private set; }
    public int Lifesteal { get; private set; }

	Dictionary<StatModifier, (Coroutine, bool)> ActiveBuffRoutines = new Dictionary<StatModifier, (Coroutine, bool)>();
    //private List<StatModifier> activeModifiers = new List<StatModifier>();
	//public List<StatModifier> GetModifiers(){
	//	return activeModifiers;
	//}
	public Dictionary<StatModifier, (Coroutine, bool)> GetModifiers(){
		return ActiveBuffRoutines;
	}
	MovingObject ourObject;
	public void RecheckStatsAfterConsumableOrBuff(CharacterBuffListItem buff){
		PlayerCharacter pc = ourObject.GetComponent<PlayerCharacter>();
		DateTime currentTimeUtc = DateTime.UtcNow;
        //DateTime expirationTimeUtc = currentTimeUtc.AddSeconds(30);
		//expirationTimeUtc.ToString("o");
		if(pc){
			Dictionary<StatModifier, (Coroutine, bool)> buffRoutineRemoval = new Dictionary<StatModifier, (Coroutine, bool)>();
			bool exists = false;
			if(buff.FoodBuff){
				//check all the food buffs and compare to make sure we dont have to remove
				foreach(var routine in ActiveBuffRoutines){
					if(routine.Key.Food){
						if(routine.Key.BuffName == buff.Key){
							if(DateTime.Parse(buff.Time) != DateTime.Parse(routine.Key.ExpirationTime)){
								buffRoutineRemoval.Add(routine.Key, (routine.Value.Item1, false));
								exists = true;
							} else {
								exists = true;
							}
						}
					}
				}
			} else if(buff.PotionBuff) {
				bool samePotionHaste = false;
        		bool samePotionDefense = false;
        		if(buff.Key == "Defense Potion" || buff.Key == "Greater Defense Potion"){
        		    samePotionDefense  = true;
        		}
        		if(buff.Key == "Haste Potion" || buff.Key == "Greater Haste Potion"){
        		    samePotionHaste  = true;
        		}
				foreach(var routine in ActiveBuffRoutines){
					if(routine.Key.Potion){
						if(samePotionHaste){
							if(routine.Key.BuffName == "Haste Potion" || routine.Key.BuffName == "Greater Haste Potion"){
								if(DateTime.Parse(buff.Time) != DateTime.Parse(routine.Key.ExpirationTime)){
									buffRoutineRemoval.Add(routine.Key, (routine.Value.Item1, false));
									exists = true;
								} else {
									exists = true;
								}
							}
						}
						if(samePotionDefense){
							if(routine.Key.BuffName == "Defense Potion" || routine.Key.BuffName == "Greater Defense Potion"){
								if(DateTime.Parse(buff.Time) != DateTime.Parse(routine.Key.ExpirationTime)){
									buffRoutineRemoval.Add(routine.Key, (routine.Value.Item1, false));
									exists = true;
								} else {
									exists = true;
								}
							}
						}
					}
				}
			} else {
				foreach(var routine in ActiveBuffRoutines){
					if(routine.Key.BuffName == buff.Key){
						if(DateTime.Parse(buff.Time) != DateTime.Parse(routine.Key.ExpirationTime)){
							buffRoutineRemoval.Add(routine.Key, (routine.Value.Item1, false));
							exists = true;
						} else {
							exists = true;
						}
					}
				}
			}
			//if(!exists){
			//	buffItemsToBeAdded.Add(buff);
			//}
		}
		
		// Check for food and potion buff, then check to see if our coroutine has it, -done
		// find the buffs that dont exist anymore and remove old stats from old buff if necessary -done
		
		// also add our new buff and stats to character
		// then tell client to do the same with its buffs we have to modify the same way we did it here in there so 2 birds one stone

	}
	//public string GetFoodString(string buffString){
	//	
	//}
	public void GearChange(ChangedGearMessage gearMessage){
		PlayerCharacter pc = ourObject.GetComponent<PlayerCharacter>();
		int LVL = 1;
		UnityEngine.MonoBehaviour.print($"Modifying gear change for item {gearMessage.item.GetItemName()} and we are adding if true and removing if false: {gearMessage.equipping}");
		if(gearMessage.item.GetBlockValue() != "0"){
			ourObject.shield = gearMessage.equipping;
			if(pc){
				pc.TargetRpcSetShield(gearMessage.equipping);
			}
		}
		if(gearMessage.equipping && !gearMessage.item.NFT){
			if(gearMessage.item.GetDurability() == "0"){
				//dead item
				MonoBehaviour.print("equipped a broken item");
				return;
			}
		}
		Dictionary<StatModifier.Stat, int> statModifiers = new Dictionary<StatModifier.Stat, int>();
		if(!string.IsNullOrEmpty(gearMessage.item.AGILITY_item)){
			int posValue;
			if (int.TryParse(gearMessage.item.AGILITY_item, out posValue))
			{
			    // Parsing succeeded, add the parsed value to the dictionary
			    statModifiers.Add(StatModifier.Stat.Agility, posValue);
			}
		}
		if(!string.IsNullOrEmpty(gearMessage.item.STRENGTH_item)){
			int posValue;
			if (int.TryParse(gearMessage.item.STRENGTH_item, out posValue))
			{
			    // Parsing succeeded, add the parsed value to the dictionary
			    statModifiers.Add(StatModifier.Stat.Strength, posValue);
			}
		}
		if(!string.IsNullOrEmpty(gearMessage.item.FORTITUDE_item)){
			int posValue;
			if (int.TryParse(gearMessage.item.FORTITUDE_item, out posValue))
			{
			    // Parsing succeeded, add the parsed value to the dictionary
			    statModifiers.Add(StatModifier.Stat.Fortitude, posValue);
			}
		}
		if(!string.IsNullOrEmpty(gearMessage.item.ARCANA_item)){
			int posValue;
			if (int.TryParse(gearMessage.item.ARCANA_item, out posValue))
			{
			    // Parsing succeeded, add the parsed value to the dictionary
			    statModifiers.Add(StatModifier.Stat.Arcana, posValue);
			}
		}
		if(!string.IsNullOrEmpty(gearMessage.item.Armor_item)){
			int posValue;
			if (int.TryParse(gearMessage.item.Armor_item, out posValue))
			{
			    // Parsing succeeded, add the parsed value to the dictionary
			    statModifiers.Add(StatModifier.Stat.Armor, posValue);
			}
		}
		if(!string.IsNullOrEmpty(gearMessage.item.MagicResist_item)){
			int posValue;
			if (int.TryParse(gearMessage.item.MagicResist_item, out posValue))
			{
			    // Parsing succeeded, add the parsed value to the dictionary
			    statModifiers.Add(StatModifier.Stat.MagicResistance, posValue);
			}
		}
		if(!string.IsNullOrEmpty(gearMessage.item.PoisonResist_item)){
			int posValue;
			if (int.TryParse(gearMessage.item.PoisonResist_item, out posValue))
			{
			    // Parsing succeeded, add the parsed value to the dictionary
			    statModifiers.Add(StatModifier.Stat.PoisonResistance, posValue);
			}
		}
		if(!string.IsNullOrEmpty(gearMessage.item.DiseaseResist_item)){
			int posValue;
			if (int.TryParse(gearMessage.item.DiseaseResist_item, out posValue))
			{
			    // Parsing succeeded, add the parsed value to the dictionary
			    statModifiers.Add(StatModifier.Stat.DiseaseResistance, posValue);
			}
		}
		if(!string.IsNullOrEmpty(gearMessage.item.ColdResist_item)){
			int posValue;
			if (int.TryParse(gearMessage.item.ColdResist_item, out posValue))
			{
			    // Parsing succeeded, add the parsed value to the dictionary
			    statModifiers.Add(StatModifier.Stat.ColdResistance, posValue);
			}
		}
		if(!string.IsNullOrEmpty(gearMessage.item.FireResist_item)){
			int posValue;
			if (int.TryParse(gearMessage.item.FireResist_item, out posValue))
			{
			    // Parsing succeeded, add the parsed value to the dictionary
			    statModifiers.Add(StatModifier.Stat.FireResistance, posValue);
			}
		}
        List<string> SetBonusItems = new List<string>();
		bool reCalc = false;
		if(StatAsset.Instance.IsItemInSet(gearMessage.item.GetItemName())){
			reCalc = true;
        }
		if(pc){
			foreach(var sheet in pc.assignedPlayer.GetInformationSheets()){
				if(sheet.CharacterID == pc.CharID){
					foreach(var stat in sheet.CharStatData){
						if(stat.Key == "LVL"){
							LVL = int.Parse(stat.Value);
							break;
						}
					}
					if(reCalc){
						foreach(var charItem in sheet.CharInventoryData){
							if(charItem.Value.GetNFT() && charItem.Value.GetEQUIPPED()){
								if(StatAsset.Instance.IsItemInSet(charItem.Value.GetItemName())){
									SetBonusItems.Add(charItem.Value.GetItemName());
        						}
							}
						}
						float newBonus = StatAsset.Instance.CalculateExpBonus(SetBonusItems);
						pc.ServerResetPEXPBONUS(newBonus);
						UnityEngine.MonoBehaviour.print($"New exp bonus for {pc.CharacterName} is {newBonus} + 1 for {newBonus + 1} total bonus value");
					}
					break;
				}
				
			}			
		}
		if (gearMessage.equipping)
        {
			
        	
        	if (gearMessage.slot == "Main-Hand")
        	{
				UnityEngine.MonoBehaviour.print($"{pc.CharacterName} is equipping {gearMessage.item.GetItemName()} in the main hand slot");
				ourObject.attackDelay = float.Parse(gearMessage.item.GetAttackDelay());
        	    ourObject.minDmgMH = int.Parse(gearMessage.item.GetDamageMin());
        	    ourObject.maxDmgMH = int.Parse(gearMessage.item.GetDamageMax());
				ourObject.weaponType = gearMessage.item.GetItemSpecificClass();
				if(gearMessage.item.GetItemName() == "Staff Of Protection"){
					int Bonus = 1 * LVL;
                	CurrentStats[StatModifier.Stat.Armor] += Bonus;
				}
				if(gearMessage.item.GetItemName() == "Vampiric Dagger"){
					int Bonus = 1;
					if(LVL > 3){
						Bonus = (LVL * 1) / 3;
					}
					ourObject.BonusLeechEffect += Bonus;
					ourObject.BonusLeechWeapon = true;
				}
				if(gearMessage.item.GetItemSpecificClass() == "Bow"){
                    pc.Bow = true;
                }
                if(gearMessage.item.GetItemName() == "Sword Of Fire"){
                    pc.SoFire = true;
					float revealRange = 10f;
					if(pc.GetInsidePC()){
						revealRange = 6f;
					}
					pc.RpcEquippedLightItem(revealRange);
                    pc.BonusFireWeapon = true;
                    pc.BonusFireEffect += 1 * LVL;
                }
                if(gearMessage.item.GetItemName() == "Acidic Axe"){
                    pc.BonusPoisonWeapon = true;
                    pc.BonusPoisonEffect += 1 * LVL;
                }
                if(gearMessage.item.GetItemName() == "Bow Of Power"){
                    pc.BonusMagicWeapon = true;
                    pc.BonusMagicEffect += 1 * LVL;
                    pc.Bow = true;
                }
                if(gearMessage.item.GetItemName() == "Frozen Greatsword"){
                    pc.BonusColdWeapon = true;
                    pc.BonusColdEffect += 2 * LVL;
                }
                if(gearMessage.item.GetItemName() == "Greatspear Of Dragonslaying"){
                    pc.BonusDragonWeapon = true;
                    pc.BonusDragonEffect += 5 * LVL;
                }
                if(gearMessage.item.GetItemName() == "Mace Of Healing"){
                    pc.healingIncrease += 1 * LVL;
                }
                if(gearMessage.item.GetItemName() == "Spear Of Dragonslaying"){
                    pc.BonusDragonWeapon = true;
                    pc.BonusDragonEffect += 2 * LVL;
                }
                if(gearMessage.item.GetItemName() == "Thunder Infused Greathammer"){
                    pc.BonusMagicWeapon = true;
                    pc.BonusMagicEffect += 2 * LVL;
                }
                if(gearMessage.item.GetItemName() == "Venomous Greataxe"){
                    pc.BonusPoisonWeapon = true;
                    pc.BonusPoisonEffect += 2 * LVL;
                }
        	}
			
        	if (gearMessage.slot == "Off-Hand")
        	{
				UnityEngine.MonoBehaviour.print($"{pc.CharacterName} is equipping {gearMessage.item.GetItemName()} in the off hand slot");

				ourObject.attackDelayOH = float.Parse(gearMessage.item.GetAttackDelay());
        	    ourObject.minDmgOH = int.Parse(gearMessage.item.GetDamageMin());
        	    ourObject.maxDmgOH = int.Parse(gearMessage.item.GetDamageMax());
				ourObject.duelWielding = true;
				ourObject.weaponTypeOH = gearMessage.item.GetItemSpecificClass();
				if(gearMessage.item.GetItemName() == "Vampiric Dagger"){
					int Bonus = 1;
					if(LVL > 3){
						Bonus = (LVL * 1) / 3;
					}
					ourObject.BonusLeechEffect += Bonus;
					ourObject.BonusLeechWeapon = true;
				}
                if(gearMessage.item.GetItemName() == "Sword Of Fire"){
                    pc.SoFire = true;
					float revealRange = 10f;
					if(pc.GetInsidePC()){
						revealRange = 6f;
					}
					pc.RpcEquippedLightItem(revealRange);
                    pc.BonusFireWeapon = true;
                    pc.BonusFireEffect += 1 * LVL;
                }
                if(gearMessage.item.GetItemName() == "Acidic Axe"){
                    //print($"Had Axe");
                    pc.BonusPoisonWeapon = true;
                    pc.BonusPoisonEffect += 1 * LVL;
                }
                if(gearMessage.item.GetItemName() == "Mace Of Healing"){
                    pc.healingIncrease += 1 * LVL;
                }
                if(gearMessage.item.GetItemName() == "Spear Of Dragonslaying"){
                    pc.BonusDragonWeapon = true;
                    pc.BonusDragonEffect += 2 * LVL;
                }
				if(gearMessage.item.GetItemName() == "Torch"){
            	    pc.Torch = true;
					float revealRange = 8f;
					if(pc.GetInsidePC()){
						revealRange = 5f;
					}
					pc.RpcEquippedLightItem(revealRange);
				}
        	}
        	if (!string.IsNullOrEmpty(gearMessage.item.GetBlockChance()))
        	{
        	    ourObject.shieldChance += int.Parse(gearMessage.item.GetBlockChance());
        	}
        	if (!string.IsNullOrEmpty(gearMessage.item.GetBlockValue()))
        	{
        	    ourObject.shieldValue += int.Parse(gearMessage.item.GetBlockValue());
        	}
        	if (!string.IsNullOrEmpty(gearMessage.item.GetParry()))
        	{
        	    ourObject.parry += int.Parse(gearMessage.item.GetParry());
        	}
        	if (!string.IsNullOrEmpty(gearMessage.item.GetPenetration()))
        	{
        	    ourObject.penetration += int.Parse(gearMessage.item.GetPenetration());
        	}
        } else {
			if(gearMessage.slot == "Main-Hand"){
				UnityEngine.MonoBehaviour.print($"{pc.CharacterName} is unequipping {gearMessage.item.GetItemName()} in the main hand slot");

				ourObject.weaponType = "Fist";
				if(ourObject.duelWielding){
					ourObject.duelWielding = false;
					ourObject.weaponTypeOH = null;
				}
				ourObject.attackDelay = 60f;
				ourObject.minDmgMH = 7 + (CurrentStats[StatModifier.Stat.Strength]/ 20);
				ourObject.maxDmgMH = 3;
				if(gearMessage.item.GetItemName() == "Staff Of Protection"){
					int Bonus = 1 * LVL;
                	CurrentStats[StatModifier.Stat.Armor] -= Bonus;
				}
				if(gearMessage.item.GetItemName() == "Vampiric Dagger"){
					int Bonus = 1;
					if(LVL > 3){
						Bonus = (LVL * 1) / 3;
					}
					ourObject.BonusLeechEffect -= Bonus;
					if(ourObject.BonusLeechEffect <= 0){
						ourObject.BonusLeechWeapon = false;
					}
				}
				if(gearMessage.item.GetItemSpecificClass() == "Bow"){
                    pc.Bow = false;
                }
                if(gearMessage.item.GetItemName() == "Sword Of Fire"){
					//float RevealingRange = inside ? 4f : 7f;
					//if (SoF) {
					//    RevealingRange = inside ? 6f : 9f;
					//}
        			//if (torch) {
					//    RevealingRange = inside ? 5f : 8f;
					//}
					float RevealingRange = pc.GetInsidePC() ? 4f : 7f;
					if (pc.SoFire ) {
	                    pc.SoFire = false;
						if(pc.GetInsidePC()){
							if(pc.Torch){
								RevealingRange = 5f;
							} else {
								RevealingRange = 4f;
							}
						} else {
							if(pc.Torch){
								RevealingRange = 8f;
							} else {
								RevealingRange = 7f;
							}
						}
					}
					pc.RpcEquippedLightItem(RevealingRange);
                    pc.BonusFireEffect -= 1 * LVL;
					if(pc.BonusFireEffect <= 0){
	                    pc.BonusFireWeapon = false;
					}
                }
                if(gearMessage.item.GetItemName() == "Acidic Axe"){
                    pc.BonusPoisonEffect -= 1 * LVL;
					if(pc.BonusPoisonEffect <= 0){
	                    pc.BonusPoisonWeapon = false;
					}
                }
                if(gearMessage.item.GetItemName() == "Bow Of Power"){
                    pc.BonusMagicEffect -= 1 * LVL;
					if(pc.BonusMagicEffect <= 0){
	                    pc.BonusMagicWeapon = false;
					}
                    pc.Bow = false;
                }
                if(gearMessage.item.GetItemName() == "Frozen Greatsword"){
                    pc.BonusColdEffect -= 2 * LVL;
					if(pc.BonusColdEffect <= 0){
						pc.BonusColdWeapon = false;
					}
                }
                if(gearMessage.item.GetItemName() == "Greatspear Of Dragonslaying"){
                    pc.BonusDragonEffect -= 5 * LVL;
					if(pc.BonusDragonEffect <= 0){
						pc.BonusDragonWeapon = false;
					}
                }
                if(gearMessage.item.GetItemName() == "Mace Of Healing"){
                    pc.healingIncrease -= 1 * LVL;
                }
                if(gearMessage.item.GetItemName() == "Spear Of Dragonslaying"){
                    pc.BonusDragonEffect -= 2 * LVL;
					if(pc.BonusDragonEffect <= 0){
						pc.BonusDragonWeapon = false;
					}
                }
                if(gearMessage.item.GetItemName() == "Thunder Infused Greathammer"){
                    pc.BonusMagicEffect -= 2 * LVL;
					if(pc.BonusMagicEffect <= 0){
						pc.BonusMagicWeapon = false;
					}
                }
                if(gearMessage.item.GetItemName() == "Venomous Greataxe"){
                    pc.BonusPoisonEffect -= 2 * LVL;
					if(pc.BonusPoisonEffect <= 0){
						pc.BonusPoisonWeapon = false;
					}
                }
			}
			if(gearMessage.slot == "Off-Hand"){
				UnityEngine.MonoBehaviour.print($"{pc.CharacterName} is unequipping {gearMessage.item.GetItemName()} in the main hand slot");

				if(ourObject.duelWielding){
					ourObject.duelWielding = false;
					ourObject.weaponTypeOH = null;
					ourObject.attackDelayOH = 0;
					ourObject.maxDmgOH = 0;
					ourObject.minDmgOH = 0;
				}
					if(gearMessage.item.GetItemName() == "Vampiric Dagger"){
						int Bonus = 1;
						if(LVL > 3){
							Bonus = (LVL * 1) / 3;
						}
						ourObject.BonusLeechEffect -= Bonus;
						if(ourObject.BonusLeechEffect <= 0){
							ourObject.BonusLeechWeapon = false;
						}
					}
					if(gearMessage.item.GetItemName() == "Torch"){
						float RevealingRange = pc.GetInsidePC() ? 4f : 7f;
						if (pc.Torch ) {
	            	        pc.Torch = false;
							if(pc.GetInsidePC()){
								if(pc.SoFire){
									RevealingRange = 6f;
								} else {
									RevealingRange = 4f;
								}
							} else {
								if(pc.SoFire){
									RevealingRange = 9f;
								} else {
									RevealingRange = 7f;
								}
							}
						}
						pc.RpcEquippedLightItem(RevealingRange);
					}
                	if(gearMessage.item.GetItemName() == "Sword Of Fire"){
						float RevealingRange = pc.GetInsidePC() ? 4f : 7f;
						if (pc.SoFire ) {
	            	        pc.SoFire = false;
							if(pc.GetInsidePC()){
								if(pc.Torch){
									RevealingRange = 5f;
								} else {
									RevealingRange = 4f;
								}
							} else {
								if(pc.Torch){
									RevealingRange = 8f;
								} else {
									RevealingRange = 7f;
								}
							}
						}
						pc.RpcEquippedLightItem(RevealingRange);
                	    pc.BonusFireEffect -= 1 * LVL;
						if(pc.BonusFireEffect <= 0){
	            	        pc.BonusFireWeapon = false;
						}
                	}
                	if(gearMessage.item.GetItemName() == "Acidic Axe"){
                	    pc.BonusPoisonEffect -= 1 * LVL;
						if(pc.BonusPoisonEffect <= 0){
	            	        pc.BonusPoisonWeapon = false;
						}
                	}
                	if(gearMessage.item.GetItemName() == "Mace Of Healing"){
                	    pc.healingIncrease -= 1 * LVL;
                	}
                	if(gearMessage.item.GetItemName() == "Spear Of Dragonslaying"){
                	    pc.BonusDragonEffect -= 2 * LVL;
						if(pc.BonusDragonEffect <= 0){
							pc.BonusDragonWeapon = false;
						}
                	}
			}
        	if (!string.IsNullOrEmpty(gearMessage.item.GetBlockChance()))
        	{
        	    ourObject.shieldChance = 0;
        	}
        	if (!string.IsNullOrEmpty(gearMessage.item.GetBlockValue()))
        	{
        	    ourObject.shieldValue = 0;
        	}
        	if (!string.IsNullOrEmpty(gearMessage.item.GetParry()))
        	{
        	    ourObject.parry -= int.Parse(gearMessage.item.GetParry());
        	}
        	if (!string.IsNullOrEmpty(gearMessage.item.GetPenetration()))
        	{
        	    ourObject.penetration -= int.Parse(gearMessage.item.GetPenetration());
        	}
        }
		foreach (var statModifier in statModifiers)
    {
		
        if (gearMessage.equipping)
        {
            if (CurrentStats.ContainsKey(statModifier.Key))
            {
                CurrentStats[statModifier.Key] += statModifier.Value;
				if(statModifier.Key == StatModifier.Stat.Fortitude){
					int newFortitude = CurrentStats[statModifier.Key];
					int oldFortitude = newFortitude - statModifier.Value;
					int oldBaseResistance = oldFortitude / 50;
					int newBaseResistance = newFortitude / 50;
					int newResistance = newBaseResistance - oldBaseResistance;
					if(newResistance >= 1){
                		CurrentStats[StatModifier.Stat.MagicResistance] += newResistance;
                		CurrentStats[StatModifier.Stat.DiseaseResistance] += newResistance;
                		CurrentStats[StatModifier.Stat.PoisonResistance] += newResistance;
                		CurrentStats[StatModifier.Stat.FireResistance] += newResistance;
                		CurrentStats[StatModifier.Stat.ColdResistance] += newResistance;
					}
				}
				Debug.Log($"Changed stat to {CurrentStats[statModifier.Key]}");
            }
            else
            {
                CurrentStats.Add(statModifier.Key, statModifier.Value);
				if(statModifier.Key == StatModifier.Stat.Fortitude){
					int newFortitude = CurrentStats[statModifier.Key];
					int oldFortitude = newFortitude - statModifier.Value;
					int oldBaseResistance = oldFortitude / 50;
					int newBaseResistance = newFortitude / 50;
					int newResistance = newBaseResistance - oldBaseResistance;
					if(newResistance >= 1){
                		CurrentStats[StatModifier.Stat.MagicResistance] += newResistance;
                		CurrentStats[StatModifier.Stat.DiseaseResistance] += newResistance;
                		CurrentStats[StatModifier.Stat.PoisonResistance] += newResistance;
                		CurrentStats[StatModifier.Stat.FireResistance] += newResistance;
                		CurrentStats[StatModifier.Stat.ColdResistance] += newResistance;
					}
				}
				Debug.Log($"Changed stat to {CurrentStats[statModifier.Key]}");
            }
        }
        else
        {
            if (CurrentStats.ContainsKey(statModifier.Key))
            {
                // Check if subtracting would result in a negative value
                if (CurrentStats[statModifier.Key] - statModifier.Value >= 0)
                {
                    CurrentStats[statModifier.Key] -= statModifier.Value;
					if(statModifier.Key == StatModifier.Stat.Fortitude){
						int newFortitude = CurrentStats[statModifier.Key];
						int oldFortitude = newFortitude + statModifier.Value;
						int oldBaseResistance = oldFortitude / 50;
						int newBaseResistance = newFortitude / 50;
						int newResistance = oldBaseResistance - newBaseResistance;
						if(newResistance >= 1){
                			CurrentStats[StatModifier.Stat.MagicResistance] -= newResistance;
                			CurrentStats[StatModifier.Stat.DiseaseResistance] -= newResistance;
                			CurrentStats[StatModifier.Stat.PoisonResistance] -= newResistance;
                			CurrentStats[StatModifier.Stat.FireResistance] -= newResistance;
                			CurrentStats[StatModifier.Stat.ColdResistance] -= newResistance;
						}
						Debug.Log($"Changed stat to {CurrentStats[statModifier.Key]}");
					}
                }
            }
        }
	}
	}
	public int GetStat(string statName){
		float statAmount = 0;
		foreach(var stat in CurrentStats){
			if(stat.Key == StatModifier.Stat.Strength && statName == "Strength"){
				statAmount += stat.Value;
			}
			if(stat.Key == StatModifier.Stat.Agility && statName == "Agility"){
				statAmount += stat.Value;
			}
			if(stat.Key == StatModifier.Stat.Fortitude && statName == "Fortitude"){
				statAmount += stat.Value;
			}
			if(stat.Key == StatModifier.Stat.Arcana && statName == "Arcana"){
				statAmount += stat.Value;
			}
			if(stat.Key == StatModifier.Stat.Armor && statName == "Armor"){
				statAmount += stat.Value;
			}
			if(stat.Key == StatModifier.Stat.MagicResistance && statName == "MagicResistance"){
				statAmount += stat.Value;
			}
			if(stat.Key == StatModifier.Stat.PoisonResistance && statName == "PoisonResistance"){
				statAmount += stat.Value;
			}
			if(stat.Key == StatModifier.Stat.DiseaseResistance && statName == "DiseaseResistance"){
				statAmount += stat.Value;
			}
			if(stat.Key == StatModifier.Stat.ColdResistance && statName == "ColdResistance"){
				statAmount += stat.Value;
			}
			if(stat.Key == StatModifier.Stat.FireResistance && statName == "FireResistance"){
				statAmount += stat.Value;
			}
			if(stat.Key == StatModifier.Stat.Agility && statName == "Dodge"){
				statAmount += stat.Value;
			}
			if(stat.Key == StatModifier.Stat.Lifesteal && statName == "Lifesteal"){
				statAmount += stat.Value;
			}
			
		}
		if(statName == "Dodge"){
			statAmount /= 20f;
			statAmount += Dodge;
		}
		return (int)statAmount;
	}
	Dictionary<StatModifier.Stat, int> CurrentStats = new Dictionary<StatModifier.Stat, int>();
	public void SetInitialStats(MovingObject obj, int agility, int strength, int fortitude, int arcana, int armor, int mr, int fr, int cr, int dr, int pr, int dodge, int lifesteal)
    {
		ourObject = obj;
        Agility = agility;
        Strength = strength;
        Fortitude = fortitude;
        Arcana = arcana;
		Armor = armor;
		MagicResist = mr;
		FireResist = fr;
		ColdResist = cr;
		DiseaseResist = dr;
		PoisonResist = pr;
		ourObject.SetAgility(Agility);
		Dodge = dodge;
		Lifesteal = lifesteal;
		CurrentStats.Add(StatModifier.Stat.Agility, Agility);
		CurrentStats.Add(StatModifier.Stat.Strength, Strength);
		CurrentStats.Add(StatModifier.Stat.Fortitude, Fortitude);
		CurrentStats.Add(StatModifier.Stat.Arcana, Arcana);
		CurrentStats.Add(StatModifier.Stat.Armor, Armor);
		CurrentStats.Add(StatModifier.Stat.MagicResistance, MagicResist);
		CurrentStats.Add(StatModifier.Stat.PoisonResistance, FireResist);
		CurrentStats.Add(StatModifier.Stat.DiseaseResistance, DiseaseResist);
		CurrentStats.Add(StatModifier.Stat.ColdResistance, ColdResist);
		CurrentStats.Add(StatModifier.Stat.FireResistance, FireResist);
		CurrentStats.Add(StatModifier.Stat.Lifesteal, Lifesteal);
    }
	public void DispelAllDebuffs(){
		
		PlayerCharacter pc = ourObject.GetComponent<PlayerCharacter>();
		List<StatModifier> debuffs = new List<StatModifier>();
		foreach(var activeBuff in ActiveBuffRoutines){
			if(!activeBuff.Key.Buff){
				debuffs.Add(activeBuff.Key);
			}
		}
		if(debuffs.Count > 0){
			for(int debuffRemoving = 0; debuffRemoving < debuffs.Count; debuffRemoving++){
        		StatModifier statBeingModed = debuffs[debuffRemoving];
        		Debug.Log("Selected Debuff: " + statBeingModed.BuffName);
				if(pc){
					for (int i = 0; i < pc.assignedPlayer.GetInformationSheets().Count; i++)
        			{
        		    	if (pc.assignedPlayer.GetInformationSheets()[i].CharacterID == pc.CharID)
        		    	{
							for (int e = 0; e < pc.assignedPlayer.GetInformationSheets()[i].CharBuffData.Count; e++)
        					{
								if(pc.assignedPlayer.GetInformationSheets()[i].CharBuffData[e].Key == statBeingModed.BuffName){
									pc.assignedPlayer.ServerRemoveBuff(pc.CharID, pc.assignedPlayer.GetInformationSheets()[i].CharBuffData[e]);
									break;
								}
							}
							break;
						}
					}
				}
				bool wasChangedAlready = false;
				string statType = string.Empty;
				if(statBeingModed.TargetStat == StatModifier.Stat.Agility){
					statType = "Agility";
				}
				if(statBeingModed.TargetStat == StatModifier.Stat.Strength){
					statType = "Strength";
				}
				if(statBeingModed.TargetStat == StatModifier.Stat.Fortitude){
					statType = "Fortitude";
				}
				if(statBeingModed.TargetStat == StatModifier.Stat.Arcana){
					statType = "Arcana";
				}
				if(statBeingModed.TargetStat == StatModifier.Stat.Armor){
					statType = "Armor";
				}
				if(statBeingModed.TargetStat == StatModifier.Stat.MagicResistance){
					statType = "MagicResistance";
				}
				if(statBeingModed.TargetStat == StatModifier.Stat.FireResistance){
					statType = "FireResistance";
				}
				if(statBeingModed.TargetStat == StatModifier.Stat.ColdResistance){
					statType = "ColdResistance";
				}
				if(statBeingModed.TargetStat == StatModifier.Stat.DiseaseResistance){
					statType = "DiseaseResistance";
				}
				if(statBeingModed.TargetStat == StatModifier.Stat.PoisonResistance){
					statType = "PoisonResistance";
				}
				if(statBeingModed.TargetStat == StatModifier.Stat.Lifesteal){
					statType = "Lifesteal";
				}
				if(statBeingModed.BuffName == "Turn Undead"){
        			ourObject.ServerRemoveStatus("Fear", statBeingModed.BuffName, false);
					wasChangedAlready = true;
				}
				if(statBeingModed.BuffName == "Root"){
        			ourObject.ServerRemoveStatus("Root", statBeingModed.BuffName, false);
					wasChangedAlready = true;
				}
				if(statBeingModed.BuffName == "Stun"){
        			ourObject.ServerRemoveStatus("Stun", statBeingModed.BuffName, false);
					wasChangedAlready = true;
				}
				if(statBeingModed.BuffName == "Aneurysm"){
        			ourObject.ServerRemoveStatus("Stun", statBeingModed.BuffName, false);
					wasChangedAlready = true;
				}
				if(statBeingModed.BuffName == "Gravity Stun"){
        			ourObject.ServerRemoveStatus("Stun", statBeingModed.BuffName, false);
					wasChangedAlready = true;
				}
				if(statBeingModed.BuffName == "Knockback"){
        			ourObject.ServerRemoveStatus("Stun", statBeingModed.BuffName, false);
					wasChangedAlready = true;
				}
				if(statBeingModed.BuffName == "Silence Shot"){
        			ourObject.ServerRemoveStatus("Silence", statBeingModed.BuffName, false);
					wasChangedAlready = true;
				}
				if(statBeingModed.BuffName == "Enthrall"){
        			ourObject.ServerRemoveStatus("Mesmerize", statBeingModed.BuffName, false);
					wasChangedAlready = true;
				}
				if(statBeingModed.BuffName == "Mesmerize"){
        			ourObject.ServerRemoveStatus("Mesmerize", statBeingModed.BuffName, false);
					wasChangedAlready = true;
				}
				if(statBeingModed.BuffName == "Blind"){
        			ourObject.ServerRemoveStatus("Blind", statBeingModed.BuffName, false);
					wasChangedAlready = true;
				}
				if(!wasChangedAlready)
				ourObject.ClientUpdateStatChangesRemove(statBeingModed.BuffName, statType, false);
			}
		}
	}
	bool GetDispelableBuff(string spell){
		if(spell == "Ice Block"){
			return false;
		}
		
		return true;
	}
	public void DispelBuff(bool buff){
		PlayerCharacter pc = ourObject.GetComponent<PlayerCharacter>();
		Mob mob = ourObject.GetComponent<Mob>();
		string unitName = string.Empty;
		string buffName = string.Empty;
		string buffType = string.Empty;
		if(pc){
			unitName = pc.CharacterName;
		} else {
		    unitName = StatAsset.Instance.GetMobName(StatAsset.Instance.GetEnemyToPrefabMapping(), mob.NAME);
		}
		if(buff){
			buffType = "buff";
		} else {
			buffType = "debuff";
		}
		MonoBehaviour.print($"Starting a dispel on {unitName}, we are removing a {buffType}");
		if(buff){
			List<StatModifier> buffs = new List<StatModifier>();
			foreach(var activeBuff in ActiveBuffRoutines){
				if(activeBuff.Key.Buff && GetDispelableBuff(activeBuff.Key.BuffName)){
					buffs.Add(activeBuff.Key);
				}
			}
			if(buffs.Count > 0){
				int randomIndex = UnityEngine.Random.Range(0, buffs.Count);
           		StatModifier statBeingModed = buffs[randomIndex];
            	Debug.Log("Selected Buff: " + statBeingModed.BuffName);
				if(pc){
					for (int i = 0; i < pc.assignedPlayer.GetInformationSheets().Count; i++)
        			{
        		    	if (pc.assignedPlayer.GetInformationSheets()[i].CharacterID == pc.CharID)
        		    	{
							for (int e = 0; e < pc.assignedPlayer.GetInformationSheets()[i].CharBuffData.Count; e++)
        					{
								if(pc.assignedPlayer.GetInformationSheets()[i].CharBuffData[e].Key == statBeingModed.BuffName){
									pc.assignedPlayer.ServerRemoveBuff(pc.CharID, pc.assignedPlayer.GetInformationSheets()[i].CharBuffData[e]);
									break;
								}
							}
							break;
						}
					}
				}
				string statType = string.Empty;
				if(statBeingModed.TargetStat == StatModifier.Stat.Agility){
					statType = "Agility";
				}
				if(statBeingModed.TargetStat == StatModifier.Stat.Strength){
					statType = "Strength";
				}
				if(statBeingModed.TargetStat == StatModifier.Stat.Fortitude){
					statType = "Fortitude";
				}
				if(statBeingModed.TargetStat == StatModifier.Stat.Arcana){
					statType = "Arcana";
				}
				if(statBeingModed.TargetStat == StatModifier.Stat.Armor){
					statType = "Armor";
				}
				if(statBeingModed.TargetStat == StatModifier.Stat.MagicResistance){
					statType = "MagicResistance";
				}
				if(statBeingModed.TargetStat == StatModifier.Stat.FireResistance){
					statType = "FireResistance";
				}
				if(statBeingModed.TargetStat == StatModifier.Stat.ColdResistance){
					statType = "ColdResistance";
				}
				if(statBeingModed.TargetStat == StatModifier.Stat.DiseaseResistance){
					statType = "DiseaseResistance";
				}
				if(statBeingModed.TargetStat == StatModifier.Stat.PoisonResistance){
					statType = "PoisonResistance";
				}
				if(statBeingModed.TargetStat == StatModifier.Stat.Lifesteal){
					statType = "Lifesteal";
				}
				if(statBeingModed.BuffName == "Hide"){
            		ourObject.ServerRemoveStatus("Stealthed", statBeingModed.BuffName, true);
					return;
				}
				if(statBeingModed.BuffName == "Invisibility"){
            		ourObject.ServerRemoveStatus("Stealthed", statBeingModed.BuffName, true);
					return;
				}
				if(statBeingModed.BuffName == "Sneak"){
            		ourObject.ServerRemoveStatus("Stealthed", statBeingModed.BuffName, true);
					return;
				}
				if(statBeingModed.BuffName == "Absorb"){
            		ourObject.ServerRemoveStatus("Absorb", statBeingModed.BuffName, true);
					return;
				}
				if(statBeingModed.BuffName == "Rune"){
            		ourObject.ServerRemoveStatus("Absorb", statBeingModed.BuffName, true);
					return;
				}
				if(statBeingModed.BuffName == "Protect"){
					ourObject.StopCoroutine(ourObject.ProtectRoutine);
					ourObject.ServerRemoveStatus("Protect", statBeingModed.BuffName, true);
					ourObject.protectingMO = null;
					ourObject.protectLvl = 0;
            		ourObject.ServerRemoveStatus("Protect", statBeingModed.BuffName, true);
					
					return;
				}
				if(statBeingModed.BuffName == "Cover"){
            		ourObject.ServerRemoveStatus("Protect", statBeingModed.BuffName, true);
					return;
				}
				if(statBeingModed.BuffName == "Refresh"){
            		ourObject.ServerRemoveStatus("Refresh", statBeingModed.BuffName, true);
					return;
				}
				if(statBeingModed.BuffName == "Offensive Stance"){
            		ourObject.ServerRemoveStatus("Refresh", statBeingModed.BuffName, true);
					return;
				}
				if(statBeingModed.BuffName == "Tank Stance"){
            		ourObject.ServerRemoveStatus("Refresh", statBeingModed.BuffName, true);
					return;
				}
				if(statBeingModed.BuffName == "Magic Burst"){
            		ourObject.ServerRemoveStatus("Refresh", statBeingModed.BuffName, true);
					return;
				}
				if(statBeingModed.BuffName == "Nature's Precision"){
            		ourObject.ServerRemoveStatus("Refresh", statBeingModed.BuffName, true);
					return;
				}
				ourObject.ClientUpdateStatChangesRemove(statBeingModed.BuffName, statType, true);
			}
		} else {
			List<StatModifier> debuffs = new List<StatModifier>();
			foreach(var activeBuff in ActiveBuffRoutines){
				if(!activeBuff.Key.Buff && GetDispelableBuff(activeBuff.Key.BuffName)){
					debuffs.Add(activeBuff.Key);
				}
			}
			if(debuffs.Count > 0){
				int randomIndex = UnityEngine.Random.Range(0, debuffs.Count);
           		StatModifier statBeingModed = debuffs[randomIndex];
            	Debug.Log("Selected debuff: " + statBeingModed.BuffName);
				if(pc){
					for (int i = 0; i < pc.assignedPlayer.GetInformationSheets().Count; i++)
        			{
        		    	if (pc.assignedPlayer.GetInformationSheets()[i].CharacterID == pc.CharID)
        		    	{
							for (int e = 0; e < pc.assignedPlayer.GetInformationSheets()[i].CharBuffData.Count; e++)
        					{
								if(pc.assignedPlayer.GetInformationSheets()[i].CharBuffData[e].Key == statBeingModed.BuffName){
									pc.assignedPlayer.ServerRemoveBuff(pc.CharID, pc.assignedPlayer.GetInformationSheets()[i].CharBuffData[e]);
									break;
								}
							}
							break;
						}
					}
				}
				string statType = string.Empty;
				if(statBeingModed.TargetStat == StatModifier.Stat.Agility){
					statType = "Agility";
				}
				if(statBeingModed.TargetStat == StatModifier.Stat.Strength){
					statType = "Strength";
				}
				if(statBeingModed.TargetStat == StatModifier.Stat.Fortitude){
					statType = "Fortitude";
				}
				if(statBeingModed.TargetStat == StatModifier.Stat.Arcana){
					statType = "Arcana";
				}
				if(statBeingModed.TargetStat == StatModifier.Stat.Armor){
					statType = "Armor";
				}
				if(statBeingModed.TargetStat == StatModifier.Stat.MagicResistance){
					statType = "MagicResistance";
				}
				if(statBeingModed.TargetStat == StatModifier.Stat.FireResistance){
					statType = "FireResistance";
				}
				if(statBeingModed.TargetStat == StatModifier.Stat.ColdResistance){
					statType = "ColdResistance";
				}
				if(statBeingModed.TargetStat == StatModifier.Stat.DiseaseResistance){
					statType = "DiseaseResistance";
				}
				if(statBeingModed.TargetStat == StatModifier.Stat.PoisonResistance){
					statType = "PoisonResistance";
				}
				if(statBeingModed.TargetStat == StatModifier.Stat.Lifesteal){
					statType = "Lifesteal";
				}
				if(statBeingModed.BuffName == "Thorns"){
            		ourObject.ServerRemoveStatus("Thorns", statBeingModed.BuffName, false);
					return;
				}
				if(statBeingModed.BuffName == "Turn Undead"){
            		ourObject.ServerRemoveStatus("Fear", statBeingModed.BuffName, false);
					return;
				}
				if(statBeingModed.BuffName == "Root"){
            		ourObject.ServerRemoveStatus("Root", statBeingModed.BuffName, false);
					return;
				}
				if(statBeingModed.BuffName == "Stun"){
            		ourObject.ServerRemoveStatus("Stun", statBeingModed.BuffName, false);
					return;
				}
				if(statBeingModed.BuffName == "Aneurysm"){
            		ourObject.ServerRemoveStatus("Stun", statBeingModed.BuffName, false);
					return;
				}
				if(statBeingModed.BuffName == "Gravity Stun"){
            		ourObject.ServerRemoveStatus("Stun", statBeingModed.BuffName, false);
					return;
				}
				if(statBeingModed.BuffName == "Knockback"){
            		ourObject.ServerRemoveStatus("Stun", statBeingModed.BuffName, false);
					return;
				}
				if(statBeingModed.BuffName == "Silence Shot"){
            		ourObject.ServerRemoveStatus("Silence", statBeingModed.BuffName, false);
					return;
				}
				if(statBeingModed.BuffName == "Enthrall"){
            		ourObject.ServerRemoveStatus("Mesmerize", statBeingModed.BuffName, false);
					return;
				}
				if(statBeingModed.BuffName == "Mesmerize"){
            		ourObject.ServerRemoveStatus("Mesmerize", statBeingModed.BuffName, false);
					return;
				}
				if(statBeingModed.BuffName == "Blind"){
            		ourObject.ServerRemoveStatus("Blind", statBeingModed.BuffName, false);
					return;
				}
				ourObject.ClientUpdateStatChangesRemove(statBeingModed.BuffName, statType, false);
				
			}
		}
		
	}
public void ApplyStatModifier(StatModifier modifier, MonoBehaviour monoBehaviour, Action<bool> callback)
{
	bool foodBuff = false;
	bool potionBuff = false;
	if(modifier.Food){
		foodBuff = true;
	}
	if(modifier.Potion){
		potionBuff = true;
	}
	int igniteAmount = 1;
	Dictionary<StatModifier, (Coroutine, bool)> removingRoutines = new Dictionary<StatModifier, (Coroutine, bool)>();
	foreach(var activeBuff in ActiveBuffRoutines){
		if(activeBuff.Value.Item2 == true){
			removingRoutines.Add(activeBuff.Key, activeBuff.Value);
		}
	}
	if(foodBuff){
		foreach(var activeBuff in ActiveBuffRoutines){
			if(activeBuff.Key.Food && activeBuff.Key.BuffName != modifier.BuffName && activeBuff.Key.TargetStat == modifier.TargetStat){
				removingRoutines.Add(activeBuff.Key, activeBuff.Value);
				
				//ChangeStat(activeBuff.Key.TargetStat, -activeBuff.Key.Value);
    	        //monoBehaviour.StopCoroutine(activeBuff.Value.Item2);
    	        //ActiveBuffRoutines.Remove(activeBuff.Key.BuffName);
			}
		}
	}
	if(potionBuff){
		foreach(var activeBuff in ActiveBuffRoutines){
			if(activeBuff.Key.Potion && activeBuff.Key.TargetStat == modifier.TargetStat && modifier.TargetStat == StatModifier.Stat.Lifesteal){
				if(!removingRoutines.ContainsKey(activeBuff.Key))
				removingRoutines.Add(activeBuff.Key, activeBuff.Value);
			}

			if(activeBuff.Key.Potion && activeBuff.Key.BuffName != modifier.BuffName && activeBuff.Key.TargetStat == modifier.TargetStat){
				if(!removingRoutines.ContainsKey(activeBuff.Key))
				removingRoutines.Add(activeBuff.Key, activeBuff.Value);
				//ChangeStat(activeBuff.Key.TargetStat, -activeBuff.Key.Value);
    	        //monoBehaviour.StopCoroutine(activeBuff.Value.Item2);
    	        //ActiveBuffRoutines.Remove(activeBuff.Key.BuffName);
			}
		}
	}
	if(removingRoutines.Count > 0){
		foreach(var removalroutine in removingRoutines){
			ChangeStat(removalroutine.Key.TargetStat, -removalroutine.Key.Value);
    	    monoBehaviour.StopCoroutine(removalroutine.Value.Item1);
    	    ActiveBuffRoutines.Remove(removalroutine.Key);
			string statType = string.Empty;
			if(modifier.TargetStat == StatModifier.Stat.Agility){
				statType = "Agility";
			}
			if(modifier.TargetStat == StatModifier.Stat.Strength){
				statType = "Strength";
			}
			if(modifier.TargetStat == StatModifier.Stat.Fortitude){
				statType = "Fortitude";
			}
			if(modifier.TargetStat == StatModifier.Stat.Arcana){
				statType = "Arcana";
			}
			if(modifier.TargetStat == StatModifier.Stat.Armor){
				statType = "Armor";
			}
			if(modifier.TargetStat == StatModifier.Stat.MagicResistance){
				statType = "MagicResistance";
			}
			if(modifier.TargetStat == StatModifier.Stat.FireResistance){
				statType = "FireResistance";
			}
			if(modifier.TargetStat == StatModifier.Stat.ColdResistance){
				statType = "ColdResistance";
			}
			if(modifier.TargetStat == StatModifier.Stat.DiseaseResistance){
				statType = "DiseaseResistance";
			}
			if(modifier.TargetStat == StatModifier.Stat.PoisonResistance){
				statType = "PoisonResistance";
			}
			if(modifier.TargetStat == StatModifier.Stat.Lifesteal){
				statType = "Lifesteal";
			}
			bool needsAServerRemove = false;
				if(modifier.BuffName == "Hide"){
            		ourObject.ServerRemoveStatus("Stealthed", modifier.BuffName, modifier.Buff);
					needsAServerRemove = true;
				}
				if(modifier.BuffName == "Invisibility"){
            		ourObject.ServerRemoveStatus("Stealthed", modifier.BuffName, modifier.Buff);
					needsAServerRemove = true;
				}
				if(modifier.BuffName == "Sneak"){
            		ourObject.ServerRemoveStatus("Stealthed", modifier.BuffName, modifier.Buff);
					needsAServerRemove = true;
				}
				if(modifier.BuffName == "Absorb"){
            		ourObject.ServerRemoveStatus("Absorb", modifier.BuffName, modifier.Buff);
					needsAServerRemove = true;
				}
				if(modifier.BuffName == "Rune"){
            		ourObject.ServerRemoveStatus("Absorb", modifier.BuffName, modifier.Buff);
					needsAServerRemove = true;
				}
				if(modifier.BuffName == "Protect"){
            		ourObject.ServerRemoveStatus("Protect", modifier.BuffName, modifier.Buff);
					needsAServerRemove = true;
				}
				if(modifier.BuffName == "Turn Undead"){
            		ourObject.ServerRemoveStatus("Fear", modifier.BuffName, modifier.Buff);
					needsAServerRemove = true;
				}
				if(modifier.BuffName == "Root"){
            		ourObject.ServerRemoveStatus("Root", modifier.BuffName, modifier.Buff);
					needsAServerRemove = true;
				}
				if(modifier.BuffName == "Stun"){
            		ourObject.ServerRemoveStatus("Stun", modifier.BuffName, modifier.Buff);
					needsAServerRemove = true;
				}
				if(modifier.BuffName == "Aneurysm"){
            		ourObject.ServerRemoveStatus("Stun", modifier.BuffName, modifier.Buff);
					needsAServerRemove = true;
				}
				if(modifier.BuffName == "Gravity Stun"){
            		ourObject.ServerRemoveStatus("Stun", modifier.BuffName, modifier.Buff);
					needsAServerRemove = true;
				}
				if(modifier.BuffName == "Knockback"){
            		ourObject.ServerRemoveStatus("Stun", modifier.BuffName, modifier.Buff);
					needsAServerRemove = true;
				}
				if(modifier.BuffName == "Silence Shot"){
            		ourObject.ServerRemoveStatus("Silence", modifier.BuffName, modifier.Buff);
					needsAServerRemove = true;
				}
				if(modifier.BuffName == "Enthrall"){
            		ourObject.ServerRemoveStatus("Mesmerize", modifier.BuffName, modifier.Buff);
					needsAServerRemove = true;
				}
				if(modifier.BuffName == "Sleep"){
            		ourObject.ServerRemoveStatus("Mesmerize", modifier.BuffName, modifier.Buff);
					needsAServerRemove = true;
				}
				if(modifier.BuffName == "Mesmerize"){
            		ourObject.ServerRemoveStatus("Mesmerize", modifier.BuffName, modifier.Buff);
					needsAServerRemove = true;
				}
				if(modifier.BuffName == "Refresh"){
            		ourObject.ServerRemoveStatus("Refresh", modifier.BuffName, modifier.Buff);
					needsAServerRemove = true;
				}
				if(modifier.BuffName == "Blind"){
            		ourObject.ServerRemoveStatus("Blind", modifier.BuffName, modifier.Buff);
					needsAServerRemove = true;
				}
				if(!needsAServerRemove)
			ourObject.ClientUpdateStatChangesRemove(modifier.BuffName, statType, modifier.Buff);
		}
	} else {
		if(modifier.BuffName == "Ignite"){
			foreach(var modifieryBuff in ActiveBuffRoutines){
				if(modifieryBuff.Key.BuffName == modifier.BuffName){
					removingRoutines.Add(modifieryBuff.Key, modifieryBuff.Value);
					igniteAmount = modifieryBuff.Key.Value;
					break;
				}
			}
			foreach(var removalroutine in removingRoutines){
				ChangeStat(removalroutine.Key.TargetStat, -removalroutine.Key.Value);
    		    monoBehaviour.StopCoroutine(removalroutine.Value.Item1);
    		    ActiveBuffRoutines.Remove(removalroutine.Key);
				string statType = string.Empty;
				if(modifier.TargetStat == StatModifier.Stat.FireResistance){
					statType = "FireResistance";
				}
				ourObject.ClientUpdateStatChangesRemove(modifier.BuffName, statType, false);
			}
		} 
		else {
			
			foreach(var modifieryBuff in ActiveBuffRoutines){
				if(modifieryBuff.Key.BuffName == modifier.BuffName && modifieryBuff.Key.TargetStat == modifier.TargetStat){
					if(modifieryBuff.Key.Value <= modifier.Value){
						removingRoutines.Add(modifieryBuff.Key, modifieryBuff.Value);
					} else {
						callback(false);
        	    		return;
					}
				}
			}
			foreach(var removalroutine in removingRoutines){
				ChangeStat(removalroutine.Key.TargetStat, -removalroutine.Key.Value);
    		    monoBehaviour.StopCoroutine(removalroutine.Value.Item1);
    		    ActiveBuffRoutines.Remove(removalroutine.Key);
				string statType = string.Empty;
				if(modifier.TargetStat == StatModifier.Stat.Agility){
					statType = "Agility";
				}
				if(modifier.TargetStat == StatModifier.Stat.Strength){
					statType = "Strength";
				}
				if(modifier.TargetStat == StatModifier.Stat.Fortitude){
					statType = "Fortitude";
				}
				if(modifier.TargetStat == StatModifier.Stat.Arcana){
					statType = "Arcana";
				}
				if(modifier.TargetStat == StatModifier.Stat.Armor){
					statType = "Armor";
				}
				if(modifier.TargetStat == StatModifier.Stat.MagicResistance){
					statType = "MagicResistance";
				}
				if(modifier.TargetStat == StatModifier.Stat.FireResistance){
					statType = "FireResistance";
				}
				if(modifier.TargetStat == StatModifier.Stat.ColdResistance){
					statType = "ColdResistance";
				}
				if(modifier.TargetStat == StatModifier.Stat.DiseaseResistance){
					statType = "DiseaseResistance";
				}
				if(modifier.TargetStat == StatModifier.Stat.PoisonResistance){
					statType = "PoisonResistance";
				}
				if(modifier.TargetStat == StatModifier.Stat.Lifesteal){
					statType = "Lifesteal";
				}
				bool needsAServerRemove = false;
				if(modifier.BuffName == "Hide"){
            		ourObject.ServerRemoveStatus("Stealthed", modifier.BuffName, modifier.Buff);
					needsAServerRemove = true;
				}
				if(modifier.BuffName == "Invisibility"){
            		ourObject.ServerRemoveStatus("Stealthed", modifier.BuffName, modifier.Buff);
					needsAServerRemove = true;
				}
				if(modifier.BuffName == "Sneak"){
            		ourObject.ServerRemoveStatus("Stealthed", modifier.BuffName, modifier.Buff);
					needsAServerRemove = true;
				}
				if(modifier.BuffName == "Absorb"){
            		ourObject.ServerRemoveStatus("Absorb", modifier.BuffName, modifier.Buff);
					needsAServerRemove = true;
				}
				if(modifier.BuffName == "Rune"){
            		ourObject.ServerRemoveStatus("Absorb", modifier.BuffName, modifier.Buff);
					needsAServerRemove = true;
				}
				if(modifier.BuffName == "Protect"){
            		ourObject.ServerRemoveStatus("Protect", modifier.BuffName, modifier.Buff);
					needsAServerRemove = true;
				}
				if(modifier.BuffName == "Turn Undead"){
            		ourObject.ServerRemoveStatus("Fear", modifier.BuffName, modifier.Buff);
					needsAServerRemove = true;
				}
				if(modifier.BuffName == "Root"){
            		ourObject.ServerRemoveStatus("Root", modifier.BuffName, modifier.Buff);
					needsAServerRemove = true;
				}
				if(modifier.BuffName == "Stun"){
            		ourObject.ServerRemoveStatus("Stun", modifier.BuffName, modifier.Buff);
					needsAServerRemove = true;
				}
				if(modifier.BuffName == "Aneurysm"){
            		ourObject.ServerRemoveStatus("Stun", modifier.BuffName, modifier.Buff);
					needsAServerRemove = true;
				}
				if(modifier.BuffName == "Gravity Stun"){
            		ourObject.ServerRemoveStatus("Stun", modifier.BuffName, modifier.Buff);
					needsAServerRemove = true;
				}
				if(modifier.BuffName == "Knockback"){
            		ourObject.ServerRemoveStatus("Stun", modifier.BuffName, modifier.Buff);
					needsAServerRemove = true;
				}
				if(modifier.BuffName == "Silence Shot"){
            		ourObject.ServerRemoveStatus("Silence", modifier.BuffName, modifier.Buff);
					needsAServerRemove = true;
				}
				if(modifier.BuffName == "Enthrall"){
            		ourObject.ServerRemoveStatus("Mesmerize", modifier.BuffName, modifier.Buff);
					needsAServerRemove = true;
				}
				if(modifier.BuffName == "Mesmerize"){
            		ourObject.ServerRemoveStatus("Mesmerize", modifier.BuffName, modifier.Buff);
					needsAServerRemove = true;
				}
				if(modifier.BuffName == "Refresh"){
            		ourObject.ServerRemoveStatus("Refresh", modifier.BuffName, modifier.Buff);
					needsAServerRemove = true;
				}
				if(modifier.BuffName == "Blind"){
            		ourObject.ServerRemoveStatus("Blind", modifier.BuffName, modifier.Buff);
					needsAServerRemove = true;
				}
				if(!needsAServerRemove)
				ourObject.ClientUpdateStatChangesRemove(modifier.BuffName, statType, modifier.Buff);
			}
		}
		
	}
	callback(true);
    Coroutine routine = monoBehaviour.StartCoroutine(HandleStatModifier(modifier));
	ActiveBuffRoutines.Add(modifier, (routine, false));
}
bool SlowSpellStatCheck(string spellName){
		bool animationReq = false;
		if(spellName == "Ice"){
			animationReq = true;
		}
		if(spellName == "Ice Blast"){
			animationReq = true;
		}
		if(spellName == "Ice Block"){
			animationReq = true;
		}
		if(spellName == "Blizzard"){
			animationReq = true;
		}
		if(spellName == "Brain Freeze"){
			animationReq = true;
		}
		if(spellName == "Slow"){
			animationReq = true;
		}
		if(spellName == "Tendon Slice"){
			animationReq = true;
		}
		if(spellName == "Crippling Shot"){
			animationReq = true;
		}
		if(spellName == "FrozenGreatsword"){
			animationReq = true;
		}
		if(spellName == "Snare"){
			animationReq = true;
		}
		return animationReq;
	}
	int serverSlowSpells = 0;

private IEnumerator HandleStatModifier(StatModifier modifier)
{
    // Find the existing active modifier with the same spellName
	if(SlowSpellStatCheck(modifier.BuffName) && !modifier.Buff){
		if (serverSlowSpells == 0) {
			ourObject.agent.speed = 1.5f;
			ourObject.agent.acceleration = 10f;
		}
		serverSlowSpells++;
	}
	ChangeStat(modifier.TargetStat, modifier.Value);
    yield return new WaitForSeconds(modifier.Duration);
    ChangeStat(modifier.TargetStat, -modifier.Value);
	if(SlowSpellStatCheck(modifier.BuffName) && !modifier.Buff){
		Mob mob = ourObject.GetComponent<Mob>();
		serverSlowSpells--;
		if (serverSlowSpells == 0) {
			if(mob){
				ourObject.agent.speed = 3.5f;
				ourObject.agent.acceleration = 17f;
			} else {
				ourObject.agent.speed = 2f;
				ourObject.agent.acceleration = 25f;
			}
			
		}
		ourObject.ClientUpdateStatChangesRemove(modifier.BuffName, "Agility", false);
	}
	
	if (ActiveBuffRoutines.ContainsKey(modifier))
    {
        ActiveBuffRoutines[modifier] = (ActiveBuffRoutines[modifier].Item1, true);
    }
}

    private void ChangeStat(StatModifier.Stat stat, int value)
    {
		if(stat == StatModifier.Stat.SpellAdd){
			return;		
		}
		Mob mob = ourObject.GetComponent<Mob>();
		float maxValue = 75f;
		if(mob){
			maxValue = 100f;
		}
		if(value > 0){
			if(stat == StatModifier.Stat.ColdResistance){
				float resistanceAmount = ourObject.GetColdResist();
				resistanceAmount += value;
    		    float maxResistAvailable = Mathf.Min(resistanceAmount, maxValue);
				value = (int)maxResistAvailable;
			}
			if(stat == StatModifier.Stat.FireResistance){
				float resistanceAmount = ourObject.GetFireResist();
				resistanceAmount += value;
    		    float maxResistAvailable = Mathf.Min(resistanceAmount, maxValue);
				value = (int)maxResistAvailable;
			}
			if(stat == StatModifier.Stat.MagicResistance){
				float resistanceAmount = ourObject.GetMagicResist();
				resistanceAmount += value;
    		    float maxResistAvailable = Mathf.Min(resistanceAmount, maxValue);
				value = (int)maxResistAvailable;
			}
			if(stat == StatModifier.Stat.DiseaseResistance){
				float resistanceAmount = ourObject.GetDiseaseResist();
				resistanceAmount += value;
    		    float maxResistAvailable = Mathf.Min(resistanceAmount, maxValue);
				value = (int)maxResistAvailable;
			}
			if(stat == StatModifier.Stat.PoisonResistance){
				float resistanceAmount = ourObject.GetPoisonResist();
				resistanceAmount += value;
    		    float maxResistAvailable = Mathf.Min(resistanceAmount, maxValue);
				value = (int)maxResistAvailable;
			}
			
		}
		if(stat == StatModifier.Stat.Fortitude){
			if(value > 0){
				ourObject.max_hp += value;
			} else {
				ourObject.max_hp -= -1 * value;
			}
		}
		if(stat == StatModifier.Stat.Lifesteal){
			ourObject.BonusLeechEffect += value;
			if(ourObject.BonusLeechEffect > 0){
				ourObject.BonusLeechWeapon = true;
			} else {
				ourObject.BonusLeechWeapon = false;
			}
		} else {
			
        	CurrentStats[stat] += value;
		}
		if(stat == StatModifier.Stat.Arcana){
			ourObject.max_mp = CurrentStats[stat] / 7;
		}
		if(stat == StatModifier.Stat.Agility){
			ourObject.SetAgility(CurrentStats[stat]);
		}
        //switch (stat)
        //{
        //    case StatModifier.Stat.Agility:
		//		// Apply the modifier value to the appropriate stat
        //		CurrentStats[stat] += value;
        //        Agility += value;
		//		ourObject.SetAgility(Agility);
        //        break;
        //    case StatModifier.Stat.Strength:
        //        Strength += value;
        //        break;
        //    case StatModifier.Stat.Fortitude:
        //        Fortitude += value;
        //        break;
        //    case StatModifier.Stat.Arcana:
        //        Arcana += value;
        //        break;
        //}
    }
}
public class Buff
{
    public string Stat { get; private set; }
    public float Duration { get; private set; }
    public float MaxDuration { get; private set; }
    public int Value { get; private set; }
    public string SpellName { get; private set; }
    public int Rank { get; private set; }
    public bool IsBuff { get; private set; }
    public DateTime StartTime { get; private set; }
    public bool Food { get; private set; }
    public bool Potion { get; private set; }
	public MovingObject owner { get; private set; }


    public Buff(string stat, float duration, float maxDuration, int value, string spellName, bool isBuff, DateTime start, int rank, bool _food, bool _potion, MovingObject _owner)
    {
        Stat = stat;
        Duration = duration;
		MaxDuration = maxDuration;
        Value = value;
        SpellName = spellName;
        IsBuff = isBuff;
		StartTime = start;
		Rank = rank;
		Food = _food;
		Potion = _potion;
		owner = _owner;
    }
}
public class CCAnimationData {
    public int Count { get; set; } = 0;
    public GameObject AnimationObject { get; set; } = null;
}
public abstract class MovingObject : NetworkBehaviour
{
	//Integrations
	//EndIntegrations
	//[SyncVar]
	//[SerializeField] public bool friendly;
	public string weaponType = string.Empty;
	public string weaponTypeOH = string.Empty;
	public float DmgSteroid = 1f; //Multiply dmg by this and just add percentage as a .01 value for 1%
	public float StaffSteroid = 1f; //Multiply dmg by this and just add percentage as a .01 value for 1%
	public bool StaffDruid; //Multiply dmg by this and just add percentage as a .01 value for 1%
	private const string CastingQ = "CastingQ";
    private const string CastingE = "CastingE";
    private const string CastingR = "CastingR";
    private const string CastingF = "CastingF";
    private const string Selected = "Selected";
	public NavMeshAgent agent;
	Color32 RedColorRef = new Color32 (208,70,72, 255);
	Color32 YellowColorRef = new Color32 (218, 212, 94, 255);
	Color32 GrayColorRef = new Color32 (128, 128, 128, 255);
	Color32 GreenColorRef = new Color32(58, 255, 0, 255);
	Color32 icyBlueColorRef = new Color32(0, 255, 255, 255);
	Color32 slowColorRef = new Color32(128, 255, 0, 255);
	[SyncVar]
	public MovingObject Target;
	[SerializeField] GameObject ArrowPrefab;
	[SerializeField] GameObject SpellCasterAutoAttackPrefab;
    public static UnityEvent HoverNoise = new UnityEvent();
	[SerializeField] private Sprite TombStoneSprite;
	[SerializeField] private Sprite BloodyDeathSprite;
	[SyncVar]
	[SerializeField] public bool Dying = false;
	
    public Color originalColor;
    public LayerMask blockingLayer;         //OLD PHASE THIS OUT NOT BEING USED Layer on which collision will be checked.
	public LayerMask mobCollisionLayer;		//New collision detection layer for movingobjects
    [SerializeField] public CircleCollider2D circleCollider2D;       //The BoxCollider2D component attached to this object.
    private Rigidbody2D rb2D;               //The Rigidbody2D component attached to this object.
	public AudioMgr audioMgr;
	private AudioSource aud;
	public Sprite offSprite;
	public Sprite mainSprite;
    //Game Mechanics Variables
	[SerializeField] public int TIER;
	[SerializeField] public float EXPERIENCE;
	[SerializeField] public int CLASSPOINTS;
	[SerializeField] public int minDmgMH;                  //minimum damage on a hit
    [SerializeField] public int maxDmgMH;                  //maximum damage on a hit
	//resistances
	[SerializeField] public int FireResist;
	[SerializeField] public int ColdResist;
	[SerializeField] public int MagicResist;
	[SerializeField] public int DiseaseResist;
	[SerializeField] public int PoisonResist;
	[SerializeField] public int parry;
	[SerializeField] public int strength;
	[SerializeField] public int agility;
	[SerializeField] public int arcana;
	[SerializeField] public int fortitude;
	[SerializeField] public int armor;                   //armor points to remove from damage received
	[SerializeField] public float shieldBlockBuffValue;                   //armor points to remove from damage received
	[SerializeField] public int thornValue;                   //armor points to remove from damage received
	//[SyncVar] 
	public int Agility;
	[SerializeField] public string mobType;
	[SerializeField] public bool Living;
    [SerializeField] public static UnityEvent<CombatLogEntry> CombatEntryAddition = new UnityEvent<CombatLogEntry>();
    [SerializeField] public static UnityEvent<CombatLogEntry> GainEXPCP = new UnityEvent<CombatLogEntry>();
    [SerializeField] public static UnityEvent<string> ImproperCheckText = new UnityEvent<string>();
    [SerializeField] public static UnityEvent<NetworkConnectionToClient, int, string>  TakeDamageCharacter = new UnityEvent<NetworkConnectionToClient, int, string>();
    [SerializeField] public static UnityEvent<NetworkConnectionToClient, string, TurnManager>  DeathCharacter = new UnityEvent<NetworkConnectionToClient, string, TurnManager>();
    [SerializeField] public static UnityEvent<MovingObject>  RemoveCharReserve = new UnityEvent<MovingObject>();
    [SerializeField] public static UnityEvent<MovingObject, bool>  RemoveFogParticipant = new UnityEvent<MovingObject, bool>();
    [SerializeField] public static UnityEvent<MovingObject>  TargetWindowSet = new UnityEvent<MovingObject>();
    [SerializeField] public static UnityEvent<MovingObject>  TargetWasSet = new UnityEvent<MovingObject>();
    [SerializeField] public static UnityEvent<MovingObject>  CharmPetCheck = new UnityEvent<MovingObject>();
    [SerializeField] public static UnityEvent  BuffAdded = new UnityEvent();
    [SerializeField] public static UnityEvent<MovingObject, MovingObject, string, Vector2>  MovedToCast = new UnityEvent<MovingObject, MovingObject, string, Vector2>();
    [SerializeField] public static UnityEvent<MovingObject, Match>  DeadChar = new UnityEvent<MovingObject, Match>();
    [SerializeField] public static UnityEvent<MovingObject, Match>  UnStealthedChar = new UnityEvent<MovingObject, Match>();
    [SerializeField] public static UnityEvent<MovingObject>  CancelCast = new UnityEvent<MovingObject>();
    [SerializeField] public static UnityEvent<MovingObject, float, int> ShowTraps = new UnityEvent<MovingObject, float, int>();
    public static UnityEvent<int> Speaker = new UnityEvent<int>();
	public Vector2 _TargetAcquired;
	[SerializeField] public int riposteLvl = 0;
	[SerializeField] public float riposteChance = 0f;
	[SerializeField] public int doubleAttackLvl = 0;
	[SerializeField] public float doubleAttackChance = 0f;
	[SerializeField] public int criticalStrikeMeleeLvl = 0;
	[SerializeField] public float criticalStrikeMeleeChance = 0f;
	[SerializeField] public int criticalStrikeMeleeUndeadLvl = 0;
	[SerializeField] public float criticalStrikeMeleeUndeadChance = 0f;
	[SerializeField] public int criticalStrikeDmgSpellLvl = 0;
    [SerializeField] public int criticalStrikeHealSpellLvl = 0;
	[SerializeField] public float criticalStrikeDmgSpellChance = 0f;
	[SerializeField] public float criticalStrikeHealSpellChance = 0f;
	public bool BonusFireWeapon = false;
	public int BonusFireEffect = 0;
	public bool BonusColdWeapon = false;
	//public bool FrozenColdWeapon = false;
	public int BonusColdEffect = 0;
	public bool BonusMagicWeapon = false;
	public int BonusMagicEffect = 0;
	public bool BonusPoisonWeapon = false;
	public int BonusPoisonEffect = 0;
	public bool BonusDiseaseWeapon = false;
	public int BonusDiseaseEffect = 0;
	public bool BonusLeechWeapon = false;
	public int BonusLeechEffect = 0;
	public bool BonusDragonWeapon = false;
	public int BonusDragonEffect = 0;
	[SerializeField] public int dodge;
	[SerializeField] public int penetration;
	[SerializeField] public int penetrationOH;
	[SerializeField] public int healingIncrease = 0;
	[SerializeField] public int healingReceivedIncrease = 0;
	[SerializeField] public int healingReduction = 0;
	[SerializeField] public bool duelWielding = false;
	[SerializeField] public bool ThreatMod = false;
	[SerializeField] public float ThreatModifier = 0;
	[SerializeField] public int minDmgOH;                  //minimum damage on a hit
    [SerializeField] public int maxDmgOH;                  //maximum damage on a hit
	[SerializeField] public bool shield = false;
	[SerializeField] public int shieldValue = 0;
	[SerializeField] public int shieldChance = 0;
	//[SyncVar]
	[SerializeField] public float attackDelay;
	//[SyncVar]
	[SerializeField] public float attackDelayOH;
	public Vector2 Origin;
	public Vector2 PublicOrigin;
	[SyncVar]
	[SerializeField] public bool InAir = false;
	[SyncVar]
	[SerializeField] public string SpellQ = "Empty";
	[SyncVar]
	[SerializeField] public bool SpellQCoolDown;
	[SyncVar]
	[SerializeField] public float CooldownQ = 0f;
	[SyncVar]
	[SerializeField] public string SpellE = "Empty";
	[SyncVar]
	[SerializeField] public bool SpellECoolDown;
	[SyncVar]
	[SerializeField] public float CooldownE = 0f;
	[SyncVar]
	[SerializeField] public string SpellR = "Empty";
	[SyncVar]
	[SerializeField] public bool SpellRCoolDown;
	[SyncVar]
	[SerializeField] public float CooldownR = 0f;
	[SyncVar]
	[SerializeField] public string SpellF = "Empty";
	[SyncVar]
	[SerializeField] public bool SpellFCoolDown;
	[SyncVar]
	[SerializeField] public float CooldownF = 0f;
   	public Transform ctSliderTransformParent;
	public Transform ctSliderTransform;
	[SerializeField] public Slider ctSlider;
	public Image ctImage;
	//HealthBar variables
	public Transform healthBarTransformParent;
	public Transform healthBarTransform;
	[SerializeField] public Slider healthBarSlider;
	//public Image hpImage;
	public Transform magicPointBarTransformParent;
	public Transform magicPointBarTransform;
	[SerializeField] public Slider magicPointBarSlider;
	//public Image mpImage;
	//State Variables
	//True while character moves one space
	//[SerializeField] public bool LerpInProgress = false;
	//True while character is moving
	[SyncVar]
	[SerializeField] public bool moving = false;
	[SyncVar]
	[SerializeField] public bool Casting = false;
	//True while character is doing an attack bump
	//True until movingToAttack completely finishes;
	[SerializeField] public float moveTime = .15f;
	public TurnManager curatorTM;
    

    private  string GREENHEALCOLOR = "3AE300";
    private string deathhexColor = "8B0000";
    private string normalHithexColor = "FFFFFF";
    private string criticalHitHexColor = "E3CA00";
	private string hpTrapHexColor = "FF0F00";
	private string mpTrapHexColor = "004CFF";//also the blue color we need fo rpsrite
	float BlindAmount = 0f;
	[SerializeField] public bool Blind = false;
	[SyncVar] [SerializeField] public bool Snared = false;
	[SyncVar] [SerializeField] public bool Mesmerized = false;
	[SyncVar] [SerializeField] public bool Feared = false;
	[SyncVar] [SerializeField] public bool Stunned = false;
	[SyncVar] [SerializeField] public bool Silenced = false;
	[SerializeField] public bool resting = false;
	[SyncVar] public MovingObject CharmedPET;
	[SyncVar] public MovingObject CharmedOWNER;
	public bool InvisibilityUndead = false;
	public bool Invisibility = false;
	public bool Hide = false;
	public bool Sneak = false;
	[SyncVar]
    [SerializeField] public bool Energized = false;
	[SyncVar]
    [SerializeField] public float stamina = 0f; 
	[SyncVar]
	[SerializeField] public int max_hp;                  //hp and mp variables
	[SyncVar]
    [SerializeField] public int cur_hp;
	[SyncVar]
	[SerializeField] public int max_mp;                  //hp and mp variables
	[SyncVar]
    [SerializeField] public int cur_mp;
    [SerializeField] GameObject PopUpTextPrefab;
	[SerializeField] private int attackDelayEnemy;
    [SerializeField] GameObject SelectedUnitCircle;
    [SerializeField] GameObject SelectedCircle;
    [SerializeField] GameObject TargetCircle;
	private Animator animator;
	public Animator GetAnimator(){
		return animator;
	}
	[SerializeField] float attackRange;
	public float GetAttackRange(){
		return attackRange;
	}
	private StatsHandler statsHandler;
	
	public StatsHandler GetStatHandler(){
		return statsHandler;
	}
	Coroutine spellIncreaseRoutine;
	public float spellIncrease = 0f;
	[Server]
	public void ServerSetupCharmedSetting(MovingObject charmedPetTarget){
		CharmedPET = charmedPetTarget;
		charmedPetTarget.CharmedOWNER = this;
		PlayerCharacter pcCheck = GetComponent<PlayerCharacter>();
		if(pcCheck){
			StartCoroutine(ServerSendRefreshCharm());
		}
	}
	IEnumerator ServerSendRefreshCharm(){
		yield return new WaitForSeconds(.1f);
		TargetRefreshCharmedPetIfActive();
	}
	[TargetRpc]
	void TargetRefreshCharmedPetIfActive(){
		CharmPetCheck.Invoke(this);
	}
	public void ServerspellIncrease(int value){
		if(spellIncreaseRoutine != null){
			StopCoroutine(spellIncreaseRoutine);
			spellIncreaseRoutine = null;
		}
		spellIncrease = value;
		spellIncreaseRoutine = StartCoroutine(ServerspellIncreaseRoutine(30f));
	}
	public IEnumerator ServerspellIncreaseRoutine(float duration){
		float elapsedTime = 0;
    	while (elapsedTime < duration)
    	{
    	    elapsedTime += Time.deltaTime;
    	    yield return null;
    	}
		spellIncrease = 0;
		spellIncreaseRoutine = null;
	}
	private float hitChance = 90f;
	private int NPRank = 1;
	private int NPHits = 0;
	Coroutine NaturesPrecisionRoutine;
	
	public void ServerNaturesPrecision(int value, int spellRank){
		if(NaturesPrecisionRoutine != null){
			StopCoroutine(NaturesPrecisionRoutine);
			NaturesPrecisionRoutine = null;
		}
		NPRank = spellRank;
		NPHits = value;
		NaturesPrecisionRoutine = StartCoroutine(ServerNaturesPrecisionRoutine(30f));
	}
	public IEnumerator ServerNaturesPrecisionRoutine(float duration){
		float elapsedTime = 0;
    	while (elapsedTime < duration)
    	{
    	    elapsedTime += Time.deltaTime;
    	    yield return null;
    	}
		ServerRemoveStatus("Nature's Precision", "Nature's Precision", true);
		NPRank = 1;
		NPHits = 0;
		NaturesPrecisionRoutine = null;
	}
	private int MBRank = 1;
	private int MBHits = 0;
	Coroutine MagicBurstRoutine;
	public bool CanUseMagicBurst(){
		if(MBHits > 0){
			MBHits -= 1;
			return true;
		} else {
			return false;
		}
	}
	public void ServerMagicBurst(int spellRank){
		if(MagicBurstRoutine != null){
			StopCoroutine(MagicBurstRoutine);
			MagicBurstRoutine = null;
		}
		MBRank = spellRank;
		MBHits = 1;
		MagicBurstRoutine = StartCoroutine(ServerMagicBurstRoutine(30f));
	}
	public IEnumerator ServerMagicBurstRoutine(float duration){
		float elapsedTime = 0;
    	while (elapsedTime < duration && MBHits > 0)
    	{
    	    elapsedTime += Time.deltaTime;
    	    yield return null;
    	}
		ServerRemoveStatus("Magic Burst", "Magic Burst", true);
		MBRank = 1;
		MBHits = 0;
		MagicBurstRoutine = null;
	}
	public (bool, float) GetHitChance(){
		float newHitChance = hitChance;
		if(Blind){
			newHitChance -= BlindAmount;
		}
		bool NPTrigger = false;
		if(NPHits > 0){
			newHitChance = 100f;
			NPHits --;
			if(NPHits < 0){
				NPHits = 0;
			}
			NPTrigger = true;
		}
		return(NPTrigger, newHitChance);
	}
	//create the stamps
    private List<Buff> activeBuffs = new List<Buff>();
	//Dictionary<string, (Buff, Coroutine)> ActiveBuffRoutines = new Dictionary<string, (Buff, Coroutine)>();
	Dictionary<Buff, Coroutine> ActiveBuffRoutines = new Dictionary<Buff, Coroutine>();

	//public Dictionary<string, (Buff, Coroutine)> GetBuffs(){
	//	return ActiveBuffRoutines;
	//}
	public Dictionary<Buff, Coroutine> GetBuffs(){
		return ActiveBuffRoutines;
	}
	[SerializeField] GameObject CombatMezAnimation;

	[SerializeField] GameObject CombatStunAnimation;

	[SerializeField] GameObject CombatRootAnimation;
	[SerializeField] GameObject CombatSilenceAnimation;
	[SerializeField] GameObject CombatCharmAnimation;
	[SerializeField] GameObject CombatFearAnimation;
	private int activeSlowSpells = 0;
	private int activeColdSpells = 0;
	bool AnimationRequiredSpell(string spellName){
		bool animationReq = false;
		if(spellName == "Mesmerize"){
			animationReq = true;
		}
		if(spellName == "Rest"){
			animationReq = true;
		}
		if(spellName == "Enthrall"){
			animationReq = true;
		}
		if(spellName == "Sleep"){
			animationReq = true;
		}
		if(spellName == "Stun"){
			animationReq = true;
		}
		if(spellName == "Root"){
			animationReq = true;
		}
		if(spellName == "Shackle"){
			animationReq = true;
		}
		if(spellName == "Engulfing Roots"){
			animationReq = true;
		}
		if(spellName == "Knockback"){
			animationReq = true;
		}
		if(spellName == "Silence Shot"){
			animationReq = true;
		}
		if(spellName == "Shuriken"){
			animationReq = true;
		}
		if(spellName == "Charm"){
			animationReq = true;
		}
		if(spellName == "Turn Undead"){
			animationReq = true;
		}
		if(spellName == "Roar"){
			animationReq = true;
		}
		if(spellName == "Gravity Stun"){
			animationReq = true;
		}
		if(spellName == "Aneurysm"){
			animationReq = true;
		}
		return animationReq;
	}
	bool ColdSpell(string spellName){
		bool animationReq = false;
		if(spellName == "Ice"){
			animationReq = true;
		}
		if(spellName == "Ice Blast"){
			animationReq = true;
		}
		if(spellName == "Ice Block"){
			animationReq = true;
		}
		if(spellName == "Blizzard"){
			animationReq = true;
		}
		if(spellName == "Brain Freeze"){
			animationReq = true;
		}
		if(spellName == "FrozenGreatsword"){
			animationReq = true;
		}
		return animationReq;
	}
	bool SlowSpell(string spellName){
		bool animationReq = false;
		if(spellName == "Slow"){
			animationReq = true;
		}
		if(spellName == "Tendon Slice"){
			animationReq = true;
		}
		if(spellName == "Crippling Shot"){
			animationReq = true;
		}
		if(spellName == "Snare"){
			animationReq = true;
		}
		return animationReq;
	}
Dictionary<string, CCAnimationData> AnimationObjectsActive = new Dictionary<string, CCAnimationData>();
	public void AddBuff(string stat, float duration, float maxDuration, int value, string buffName, bool buff, DateTime initialTime, int rank, bool potion, bool food)
{
//   		print("AddBuff called on " + buffName);
Buff newBuff = new Buff(stat, duration, maxDuration, value, buffName, buff, initialTime, rank, potion, food, this);
    //List<string> buffsToRemove = new List<string>();
	Dictionary<Buff, Coroutine> CheckActiveBuffRoutines = new Dictionary<Buff, Coroutine>();
	//if(foodBuff){
	//	foreach(var activeBuff in ActiveBuffRoutines){
	//		if(activeBuff.Key.Food && activeBuff.Key.BuffName != modifier.BuffName && activeBuff.Key.TargetStat == modifier.TargetStat){
	//			removingRoutines.Add(activeBuff.Key, activeBuff.Value);
	//			
	//			//ChangeStat(activeBuff.Key.TargetStat, -activeBuff.Key.Value);
    //	        //monoBehaviour.StopCoroutine(activeBuff.Value.Item2);
    //	        //ActiveBuffRoutines.Remove(activeBuff.Key.BuffName);
	//		}
	//	}
	//}
	//if(potionBuff){
	//	foreach(var activeBuff in ActiveBuffRoutines){
	//		if(activeBuff.Key.TargetStat == modifier.TargetStat && modifier.TargetStat == StatModifier.Stat.Lifesteal){
	//			if(!removingRoutines.ContainsKey(activeBuff.Key))
	//			removingRoutines.Add(activeBuff.Key, activeBuff.Value);
	//		}
//
	//		if(activeBuff.Key.Potion && activeBuff.Key.BuffName != modifier.BuffName && activeBuff.Key.TargetStat == modifier.TargetStat){
	//			if(!removingRoutines.ContainsKey(activeBuff.Key))
	//			removingRoutines.Add(activeBuff.Key, activeBuff.Value);
	//			//ChangeStat(activeBuff.Key.TargetStat, -activeBuff.Key.Value);
    //	        //monoBehaviour.StopCoroutine(activeBuff.Value.Item2);
    //	        //ActiveBuffRoutines.Remove(activeBuff.Key.BuffName);
	//		}
	//	}
	//}

    // Identify buffs to remove
	foreach(var buffCheck in ActiveBuffRoutines){
		if(buffCheck.Key.SpellName == buffName && buffCheck.Key.Stat == stat ){
			//if(buffCheck.Key.Value <= value){
				if (activeBuffs.Contains(buffCheck.Key))
            	{
            	    activeBuffs.Remove(buffCheck.Key);
            	}
				CheckActiveBuffRoutines.Add(buffCheck.Key, buffCheck.Value);
			//}
		}
		if(potion){
			if(buffCheck.Key.Potion && buffCheck.Key.Stat == stat ){
				if (activeBuffs.Contains(buffCheck.Key))
        	    {
        	        activeBuffs.Remove(buffCheck.Key);
        	    }
				CheckActiveBuffRoutines.Add(buffCheck.Key, buffCheck.Value);
			}
		}
		if(food){
			if(buffCheck.Key.Food){
				if (activeBuffs.Contains(buffCheck.Key))
        	    {
        	        activeBuffs.Remove(buffCheck.Key);
        	    }
				CheckActiveBuffRoutines.Add(buffCheck.Key, buffCheck.Value);
			}
		}
		
	}
	var routinesList = CheckActiveBuffRoutines.Values.ToList();

	// Now you can iterate using a for loop
	for (int i = 0; i < routinesList.Count; i++)
	{
	    Coroutine routineToCancel = routinesList[i];
	    StopCoroutine(routineToCancel);
	}
	foreach (var key in CheckActiveBuffRoutines.Keys.ToList()) // ToList() to avoid collection modification issues
	{
	    ActiveBuffRoutines.Remove(key);
	}

	Coroutine newCoroutine = StartCoroutine(RemoveBuffAfterDuration(newBuff));
    activeBuffs.Add(newBuff);
    ActiveBuffRoutines[newBuff] = newCoroutine;
	BuffAdded.Invoke();
	if(AnimationRequiredSpell(buffName)){
		print($"Spell name was {buffName} that we are adding");

		if (!AnimationObjectsActive.ContainsKey(stat)) {
    	    AnimationObjectsActive[stat] = new CCAnimationData();
    	}
    	AnimationObjectsActive[stat].Count++;
    	// Instantiate and show the animation if it's the first CC buff of this type
    	if (AnimationObjectsActive[stat].Count == 1) {
    	    GameObject animationObject = InstantiateAnimationObject(stat); // Assuming this method instantiates the object
    	    AnimationObjectsActive[stat].AnimationObject = animationObject;
    	}
	}
	if(ColdSpell(buffName) && !buff){
		if (activeColdSpells == 0) {
            // Store the original color only if this is the first cold spell
            // Change the sprite color to icy blue
            GetComponent<SpriteRenderer>().color = icyBlueColorRef; // Adjust RGB values to your preferred icy blue
        }
        activeColdSpells++;
			print($"activeColdSpells was {activeColdSpells} after adding");

	}
	if(SlowSpell(buffName) && !buff){
		if (activeSlowSpells == 0) {
            // Store the original color only if this is the first Slow spell
            // Change the sprite color to icy blue
            GetComponent<SpriteRenderer>().color = slowColorRef; // Adjust RGB values to your preferred icy blue
        }
        activeSlowSpells++;
			print($"activeSlowSpells was {activeSlowSpells} after adding");

	}
}
GameObject InstantiateAnimationObject(string stat){
	GameObject returnObj = null;
	if(stat == "Mesmerize"){
		returnObj = Instantiate(CombatMezAnimation, transform);
		AnimationControlVisual av = returnObj.GetComponent<AnimationControlVisual>();
		av.PassOwner(this, true);
	}
	if(stat == "Stun"){
		returnObj = Instantiate(CombatStunAnimation, transform);
		AnimationControlVisual av = returnObj.GetComponent<AnimationControlVisual>();
		av.PassOwner(this, true);
	}
	if(stat == "Root"){
		returnObj = Instantiate(CombatRootAnimation, transform);
		AnimationControlVisual av = returnObj.GetComponent<AnimationControlVisual>();
		av.PassOwner(this, false);
	}
	if(stat == "Silence"){
		returnObj = Instantiate(CombatSilenceAnimation, transform);
		AnimationControlVisual av = returnObj.GetComponent<AnimationControlVisual>();
		av.PassOwner(this, true);
	}
	if(stat == "Charm"){
		returnObj = Instantiate(CombatCharmAnimation, transform);
		AnimationControlVisual av = returnObj.GetComponent<AnimationControlVisual>();
		av.PassOwner(this, true);
	}
	if(stat == "Fear"){
		returnObj = Instantiate(CombatFearAnimation, transform);
		AnimationControlVisual av = returnObj.GetComponent<AnimationControlVisual>();
		av.PassOwner(this, true);
	}
	return returnObj;
}
	public void RemoveBuff(string buffName, string stat, bool buff){
		print($"RemoveBuff called on buff name {buffName} which had stat {stat} and buff/debuff is {buff}");
		Dictionary<Buff, Coroutine> CheckActiveBuffRoutines = new Dictionary<Buff, Coroutine>();

    // Identify buffs to remove
	foreach(var buffCheck in ActiveBuffRoutines){
		if(buffCheck.Key.SpellName == buffName && buffCheck.Key.Stat == stat ){
			if (activeBuffs.Contains(buffCheck.Key))
            {
                activeBuffs.Remove(buffCheck.Key);
            }
			CheckActiveBuffRoutines.Add(buffCheck.Key, buffCheck.Value);
		}
		//if(buffName == "Enthrall" || buffName == "Mesmerize" || buffName == "Sleep"){
		//	if(buffCheck.Key.SpellName == "Enthrall" && buffName == "Mesmerize"){
		//		if (activeBuffs.Contains(buffCheck.Key))
        //	    {
        //	        activeBuffs.Remove(buffCheck.Key);
        //	    }
		//		CheckActiveBuffRoutines.Add(buffCheck.Key, buffCheck.Value);
		//	}
		//	if(buffName == "Enthrall" && buffCheck.Key.SpellName == "Mesmerize"){
		//		if (activeBuffs.Contains(buffCheck.Key))
        //	    {
        //	        activeBuffs.Remove(buffCheck.Key);
        //	    }
		//		CheckActiveBuffRoutines.Add(buffCheck.Key, buffCheck.Value);
		//	}
		//}
	}
	var routinesList = CheckActiveBuffRoutines.Values.ToList();

	// Now you can iterate using a for loop
	for (int i = 0; i < routinesList.Count; i++)
	{
	    Coroutine routineToCancel = routinesList[i];
	    StopCoroutine(routineToCancel);
	}
	foreach (var key in CheckActiveBuffRoutines.Keys.ToList()) // ToList() to avoid collection modification issues
	{
	    ActiveBuffRoutines.Remove(key);
	}
	if (AnimationRequiredSpell(buffName) && AnimationObjectsActive.ContainsKey(stat)) {
		//print($"Spell name was {buffName} that we are removing");
        AnimationObjectsActive[stat].Count--;
        
        // If it's the last active CC buff of this type, destroy the animation object and remove from the dictionary
        if (AnimationObjectsActive[stat].Count == 0) {
            GameObject animationObject = AnimationObjectsActive[stat].AnimationObject;
            if (animationObject != null) {
                Destroy(animationObject);
            }
            AnimationObjectsActive.Remove(stat);
        }
    }
		//if (ActiveBuffRoutines.TryGetValue(buffName, out var existingTuple))
    	//{
		//	
    	//    if(activeBuffs.Contains(existingTuple.Item1)){
    	//        activeBuffs.Remove(existingTuple.Item1);
    	//    }
    	//    StopCoroutine(existingTuple.Item2);
    	//    ActiveBuffRoutines.Remove(buffName);
    	//}
		BuffAdded.Invoke();
		if(ColdSpell(buffName) && !buff){
			activeColdSpells--;
			print($"activeColdSpells was {activeColdSpells} after removing");

        	// If it's the last active cold spell, revert the sprite color
        	if (activeColdSpells == 0) {
        	    GetComponent<SpriteRenderer>().color = originalColor;
        	}
		}
		if(SlowSpell(buffName) && !buff){
			activeSlowSpells--;
			print($"activeSlowSpells was {activeSlowSpells} after removing");

        	// If it's the last active cold spell, revert the sprite color
        	if (activeSlowSpells == 0) {
        	    GetComponent<SpriteRenderer>().color = originalColor;
        	}
		}
    }
	
	bool FoodCheck(string buffName){
		bool foodcheck = false;
		return foodcheck;
	}
   
    private IEnumerator RemoveBuffAfterDuration(Buff buff){
		PlayerCharacter pc = GetComponent<PlayerCharacter>();
        if (GetComponent<NetworkIdentity>().hasAuthority){
            // Invoke the event
			if(pc)
            PlayerCharacter.CombatRefresh.Invoke(pc.CharID);
        }
        yield return new WaitForSeconds(buff.Duration);
        if(activeBuffs.Contains(buff)){
            activeBuffs.Remove(buff);
        }
        if (GetComponent<NetworkIdentity>().hasAuthority){
			if(pc)
            PlayerCharacter.CombatRefresh.Invoke(pc.CharID);
        }
        if (ActiveBuffRoutines.ContainsKey(buff))
        {
            ActiveBuffRoutines.Remove(buff);
        }
    }
	[Server]
	public void SetFriendly(List<MovingObject> friends)
	{
		FriendlyList = friends;
		StartCoroutine(BriefPauseFriendlySet());
	}
	IEnumerator BriefPauseFriendlySet(){
		yield return new WaitForSeconds(8f);
		RpcSetFriendly(FriendlyList);
	}
	[Server]
	public void SetFriendlyCharmFinished(List<MovingObject> friends)
	{
		FriendlyList = friends;
		RpcSetFriendly(friends);
	}
	[ClientRpc]
	public void RpcSetFriendly(List<MovingObject> friends){
		FriendlyList = friends;
	}
	//public SyncList<MovingObject> FriendlyList = new SyncList<MovingObject>();
	public List<MovingObject> FriendlyList = new List<MovingObject>();
	public List<MovingObject> GetFriendlyList(){
		return FriendlyList;
	}
	public bool GetFriendly(MovingObject moCheck){
		if(FriendlyList.Contains(moCheck)){
			//print($"{moCheck.name} was friendly for {this.name}");
			return true;
		}
			//print($"{moCheck.name} was not on our friendly list for {this.name}");
		return false;
	}
	[Server]
	public void ServerClearFriendly(){
		FriendlyList.Clear();
		RpcClearFriendly();
	}
	[ClientRpc]
	public void RpcClearFriendly(){
		FriendlyList.Clear();
	}
	[Server]
	public void ServerAddFriendly(MovingObject moCheck){
		if(!FriendlyList.Contains(moCheck)){
			FriendlyList.Add(moCheck);
			print($"Adding {moCheck.gameObject.name} from {gameObject.name}'s friend list");
		}
		RpcAddFriendly(moCheck);
	}
	[ClientRpc]
	public void RpcAddFriendly(MovingObject moCheck){
		if(!FriendlyList.Contains(moCheck)){
			print($"Adding {moCheck.gameObject.name} from {gameObject.name}'s friend list");
			FriendlyList.Add(moCheck);
		}
	}
	[Server]
	public void ServerRemoveFriendly(MovingObject moCheck){
		if(FriendlyList.Contains(moCheck)){
			FriendlyList.Remove(moCheck);
			print($"Removing {moCheck.gameObject.name} from {gameObject.name}'s friend list");
		}
		RpcRemoveFriendly(moCheck);
	}
	[ClientRpc]
	public void RpcRemoveFriendly(MovingObject moCheck){
		if(FriendlyList.Contains(moCheck)){
			FriendlyList.Remove(moCheck);
			print($"Removing {moCheck.gameObject.name} from {gameObject.name}'s friend list");
		}
	}
    public List<Buff> GetCharacterBuffList(){
        return activeBuffs;
    }
	Coroutine AnimatingSpriteCO;
	//[SyncVar]
    //[SerializeField] public bool //RadiusLock = false;
	public float GetAttacKRange(){
		return attackRange;
	}
	[Server]
	public void SetAgility(int agility){
		if(GetComponent<PlayerCharacter>()){
			Agility = agility;
		}
	}
	[Command]
	public void CmdStopMoving(){
		if(agent.enabled){
			agent.isStopped = true;
			agent.ResetPath();
		}
		//moving = false;
		//RadiusLock = false;
	}
	[Server]
	public void ServerStopMoving(){
		if(agent.enabled){
			agent.isStopped = true;
			agent.ResetPath();
		}
		//moving = false;
		//isWalking = false;
		//RpcUpdateWalkingState(false);
		//RadiusLock = false;
	}
	[Server]
	public void ServerStopMoveReset(){
		if(agent.enabled){
			agent.isStopped = true;
			agent.ResetPath();
			agent.isStopped = false;
		}
		//moving = false;
		//isWalking = false;
		//RpcUpdateWalkingState(false);
		//RadiusLock = false;
	}
	[Server]
	public void ServerPrepareMovement(){
		if(agent.enabled){
			agent.isStopped = false;
		}
		moving = true;
		isWalking = true;
		RpcUpdateWalkingState(true);
		//RadiusLock = false;
	}
	[Command]
	public void CmdRemoveTarget(){
		if(Target != null){
			//print($"Removing target {Target.gameObject.name} from {this.gameObject.name}");
		}
		Target = null;
		TargetTargetterResetBool();
	}
	[Command]
	public void CmdClearAllStopMoving(){
		if(Target != null){
			//print($"Removing target {Target.gameObject.name} from {this.gameObject.name}");
		}
		Target = null;
		if(agent.enabled){
			agent.isStopped = true;
			agent.ResetPath();
		}
		//moving = false;
		//isWalking = false;

		//RadiusLock = false;
		TargetTargetterResetBool();
	}
	[TargetRpc]
	void TargetTargetterResetBool(){
		CombatPartyView.instance.ResetTargetWindow();
	}
	public bool SelectedCircleActive(){
		return SelectedCircle.activeInHierarchy;
	}
	public int GetDodge(){
		int dodge = statsHandler.GetStat("Dodge");
		//int agil = statsHandler.Agility;
		//dodge += agil/20;
		return dodge;
	}
	public int GetDodgeEnemy(){
		int dodge = statsHandler.GetStat("Dodge");

		//int dodge = statsHandler.Dodge;
		return dodge;
	}
	public int GetStrength(){
		return statsHandler.GetStat("Strength");
		//return statsHandler.Strength;
	}
	public int GetAgility(){
		return statsHandler.GetStat("Agility");

		//return statsHandler.Agility;
	}
	public int GetFortitude(){
		return statsHandler.GetStat("Fortitude");

		//return statsHandler.Fortitude;
	}
	public int GetArcana(){
		return statsHandler.GetStat("Arcana");

		//return statsHandler.Arcana;
	}
	public int GetArmor(){
		return statsHandler.GetStat("Armor");

		//return statsHandler.Armor;
	}
	public int GetMagicResist(){
		return statsHandler.GetStat("MagicResistance");

		//return statsHandler.MagicResist;
	}
	public int GetFireResist(){
		return statsHandler.GetStat("FireResistance");

		//return statsHandler.FireResist;
	}
	public int GetColdResist(){
		return statsHandler.GetStat("ColdResistance");

		//return statsHandler.ColdResist;
	}
	public int GetDiseaseResist(){
		return statsHandler.GetStat("DiseaseResistance");

		//return statsHandler.DiseaseResist;
	}
	public int GetPoisonResist(){
		return statsHandler.GetStat("PoisonResistance");

		//return statsHandler.PoisonResist;
	}
	public int GetLifesteal(){
		return statsHandler.GetStat("Strength");
		//return statsHandler.Strength;
	}
	public Vector3 lastPosition;
	public float accumulatedDistance;
	float startingRadius = .5f;
	float startingSpeed = .5f;
	float slowSpeed = 2f;
	float slowAcceleration = 10f;

	float startingAcceleration = .5f;
	float startingStoppingDistance = .5f;

	public override void OnStartClient()
	{
	    base.OnStartClient();
		animator = GetComponent<Animator>();
	
	    // Remove the range detector collider
		Mob mob = GetComponent<Mob>();
		if(!mob){
			return;
		}
	    CircleCollider2D[] circleColliders = GetComponents<CircleCollider2D>();
    	foreach (CircleCollider2D circleCollider in circleColliders)
    	{
			if(circleCollider.radius > 1f){
    	    	circleCollider.enabled = false;
			}
    	}
	}
	protected virtual void Awake()
	{
		agent = GetComponent<NavMeshAgent>();
		agent.updateRotation = false;
		agent.updateUpAxis = false;
		startingRadius = agent.radius;
		startingSpeed = agent.speed;
		startingAcceleration = agent.acceleration;
		startingStoppingDistance = agent.stoppingDistance;

	}
	public bool FROZEN = false;
            #if UNITY_SERVER //|| UNITY_EDITOR

	void FreezeChar(ScenePlayer sPlayer){
		PlayerCharacter PC = GetComponent<PlayerCharacter>();
		if(PC){
			if(sPlayer == PC.assignedPlayer){
				FROZEN = true;
				StopAllHOTAndDOTCoroutines();
				//print("FROZE THIS CHARACTER!!");
			}
		}
	}
		#endif
	//Protected, virtual functions can be overridden by inheriting classes.
    protected virtual void Start ()
    {
            #if UNITY_SERVER //|| UNITY_EDITOR

		if(isServer)
		{
			directionPos = transform.position;
			Mob.MobDiedRemovePossibleTarget.AddListener(ProcessMobDeath);
			MovingObject.DeadChar.AddListener(ProcessCharacterDeath);
			PlayFabServer.charFreeze.AddListener(FreezeChar);
			PlayFabServer.charGearChangeMatch.AddListener(ServerChangingStatInMatch);
			PlayFabServer.tactGearChangeMatch.AddListener(ServerChangingStatInMatchTactician);
			PlayFabServer.ConsumedItemInMatch.AddListener(ServerBuildingBuffNotStart);
			
			if(GetComponent<Mob>()){
				cur_hp = fortitude;
				max_hp = fortitude;
				cur_mp = (int)(arcana / 7.0f);
				max_mp = (int)(arcana / 7.0f);
				attackDelayEnemy = 100;
				dodge += agility / 20;
				
			}
			statsHandler = new StatsHandler();
			lastPosition = agent.transform.position;
			rightFace = false;
    		accumulatedDistance = 0;
			
		}
		#endif
		//if(!isServer && GetComponent<PlayerCharacter>()){
		//	StartCoroutine(VisionSpark());
		//}
		if(!isServer)
		{
			ScenePlayer.TargetHighlightReset.AddListener(UnTargettedMO);
			ScenePlayer.CancelAllCastsOwned.AddListener(CheckIfOwnerBeforeAllCancellation);
			MovingObject.TargetWindowSet.AddListener(CheckTargetCircle);
			SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        	spriteRenderer.color = originalColor;
			if(GetComponent<Mob>()){
				AnimatingSpriteCO = StartCoroutine(AnimatingSprite());

			}
		}	
    }
//	public static UnityEvent<string, ScenePlayer, Match, CharacterBuffListItem> ConsumedItemInMatch = new UnityEvent<string, ScenePlayer, Match, CharacterBuffListItem>();
//AddRpcCall(null, null, false, true, "Blocked", null, false, ownerName, targetName, this, target, Vector2.zero);
	 //ChangedGearMessage gearMessage = new ChangedGearMessage(ID, damagedItem.Value.EQUIPPEDSLOT, sPlayer.currentMatch, false, damagedItem.Value);
                                //charGearChangeMatch.Invoke(gearMessage); 
	[Server]
	
	void ServerChangingStatInMatch(ChangedGearMessage gearMessage){
		PlayerCharacter PC = GetComponent<PlayerCharacter>();
		if(PC){
			if(PC.CharID == gearMessage.charID){
				if(PC.assignedMatch == gearMessage.match){
					statsHandler.GearChange(gearMessage);
					TargetRefreshSpells();
				}
			}
		}
	}
	[TargetRpc]
	void TargetRefreshSpells(){
		CombatPartyView.instance.CheckMovingObject(this);
	}
	[Server]
	void ServerChangingStatInMatchTactician(ChangedGearMessage gearMessage, ScenePlayer sPlayer){
		PlayerCharacter PC = GetComponent<PlayerCharacter>();
		if(PC){
			if(PC.assignedPlayer == sPlayer){
				if(PC.assignedMatch == gearMessage.match){
					gearMessage.charID = PC.CharID;
					statsHandler.GearChange(gearMessage);
					TargetRefreshSpells();
				}
			}
		}
	}
	[Server]
	void ServerBuildingBuffNotStart(string serial, ScenePlayer sPlayer, Match match, CharacterBuffListItem newBuff){
		ServerBuildingBuff(serial, sPlayer, match, newBuff, false);
	}
	[Server]
	public void ServerBuildingBuff(string serial, ScenePlayer sPlayer, Match match, CharacterBuffListItem newBuff, bool start){
		string ownerName = string.Empty;
		PlayerCharacter pc = GetComponent<PlayerCharacter>();
		if(pc){
			ownerName = pc.CharacterName;
		}
		if(newBuff.PotionBuff && !start){
			AddRpcCall(newBuff.Key, null, false, true, "Drink", null, false, ownerName, ownerName, this, this, Vector2.zero);
		}
		if(newBuff.FoodBuff && !start){
			AddRpcCall(newBuff.Key, null, false, true, "Eat", null, false, ownerName, ownerName, this, this, Vector2.zero);
		}
		if(newBuff.Key == "Hide" || newBuff.Key == "Energy Potion" || newBuff.Key == "Rejuvenation Potion" || newBuff.Key == "Healing Potion" || newBuff.Key == "Antidote Potion" || newBuff.Key == "Magic Potion"){
			if(newBuff.Key == "Healing Potion" ){
				cur_hp += int.Parse(newBuff.Value);
				if(cur_hp > max_hp){
					cur_hp = max_hp;
				}
			}
			if(newBuff.Key == "Magic Potion" ){
				cur_mp += int.Parse(newBuff.Value);
				if(cur_mp > max_mp){
					cur_mp = max_mp;
				}
			}
			if(newBuff.Key == "Rejuvenation Potion" ){
				cur_hp += 20;
				if(cur_hp > max_hp){
					cur_hp = max_hp;
				}
				cur_mp += 3;
				if(cur_mp > max_mp){
					cur_mp = max_mp;
				}
			}
			if(newBuff.Key == "Antidote Potion" ){
				ServerCurePoisonAntidote();
			}
			return;
		}
		if(pc){
			DateTime expirationTimeUtc = DateTime.UtcNow;
			if (string.IsNullOrEmpty(newBuff.Time) || !DateTime.TryParseExact(newBuff.Time, "o", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out expirationTimeUtc))
			{
			    // If newBuff.Time is null, empty, or cannot be parsed, set expirationTimeUtc to a default future time.
			    //expirationTimeUtc = expirationTimeUtc;
			}

			DateTime currentTimeUtc = DateTime.UtcNow;

			// Calculate the duration as the difference between the expiration time and the current time.
			TimeSpan durationUntilExpiration = expirationTimeUtc - currentTimeUtc;

			// Ensure durationUntilExpiration is not negative.
			if (durationUntilExpiration.TotalSeconds < 0)
			{
			    durationUntilExpiration = TimeSpan.Zero; // Set to zero if expiration time is past.
			}

			// Get the total seconds left until expiration. 
			// Note: You might want to ensure this value is not negative before using it.
			float secondsLeft = (float)durationUntilExpiration.TotalSeconds;

			if (secondsLeft < 0)
			{
			    secondsLeft = 0; // Ensure non-negative duration; adjust based on your needs
			}
			if(pc.assignedPlayer == sPlayer && pc.CharID == serial && pc.assignedMatch == match){
				foreach(var sheet in sPlayer.GetInformationSheets()){
					if(sheet.CharacterID == serial){
						//foreach(var stat in sheet.CharStatData){
						//	if(stat.Key == "currentHP"){
						//		cur_hp = int.Parse(stat.Value);
						//	}
						//	if(stat.Key == "currentMP"){
						//		cur_mp = int.Parse(stat.Value);
						//	}
						//}
						
						int buffStrength = 0;
        				int buffFortitude = 0;
        				int buffAgility = 0;
        				int buffArcana = 0;
        				int buffArmor = 0;
        				int buffMagicResist = 0;
        				int buffDiseaseResist = 0;
        				int buffPoisonResist = 0;
        				int buffFireResist = 0;
        				int buffColdResist = 0;
        				int buffLifesteal = 0;
						//get values of each stat being changed and send through the applystatmodifier to give to the character nearly there!
						Dictionary<string, int> attributeDictionary = new Dictionary<string, int>();
						string[] attributes = newBuff.Value.Split('|');
                    	foreach (string attribute in attributes) {
                    	    string[] parts = attribute.Trim().Split(' ');
                    	    if (parts.Length == 2) {
                    	        // Try to parse the value and attribute name
                    	        if (int.TryParse(parts[0], out int value)) {
                    	            string attributeName = parts[1].Trim();
                    	            switch (attributeName) {
                    	                case "Strength":
                    	                    buffStrength += value;
											attributeDictionary.Add("Strength", buffStrength);
                    	                    break;
                    	                case "Fortitude":
                    	                    buffFortitude += value;
											attributeDictionary.Add("Fortitude", buffFortitude);
                    	                    break;
                    	                case "Agility":
                    	                    buffAgility += value;
											attributeDictionary.Add("Agility", buffAgility);
                    	                    break;
                    	                case "Arcana":
                    	                    buffArcana += value;
											attributeDictionary.Add("Arcana", buffArcana);
                    	                    break;
                    	                case "Armor":
                    	                    buffArmor += value;
											attributeDictionary.Add("Armor", buffArmor);
                    	                    break;
                    	                case "Magic Resist":
                    	                    buffMagicResist += value;
											attributeDictionary.Add("Magic Resistance", buffMagicResist);
                    	                    break;
                    	                case "Disease Resist":
                    	                    buffDiseaseResist += value;
											attributeDictionary.Add("Disease Resistance", buffDiseaseResist);
                    	                    break;
                    	                case "Poison Resist":
                    	                    buffPoisonResist += value;
											attributeDictionary.Add("Poison Resistance", buffPoisonResist);
                    	                    break;
                    	                case "Fire Resist":
                    	                    buffFireResist += value;
											attributeDictionary.Add("Fire Resistance", buffFireResist);
                    	                    break;
                    	                case "Lifesteal":
                    	                    buffLifesteal += value;
											//BonusLeechEffect += value;
											attributeDictionary.Add("Lifesteal", buffLifesteal);
                    	                    break;
                    	                case "Cold Resist":
                    	                    buffColdResist += value;
											attributeDictionary.Add("Cold Resistance", buffColdResist);
                    	                    break;
                    	            }
                    	        }
                    	    }
                    	}
						foreach(var addAtribute in attributeDictionary){
							ApplyStatChange(addAtribute.Key, secondsLeft, float.Parse(newBuff.Duration), addAtribute.Value, newBuff.Key, true, newBuff.Rank, newBuff.FoodBuff, newBuff.PotionBuff, newBuff.Time);

						}
						break;
					}
				}
			}
		}
	}
	List<int> DistributeOverTimeValues(int DmgValue, float duration, int tickInterval) {
        
    int parts = Mathf.Min(Mathf.CeilToInt(duration / tickInterval), DmgValue); // Ensure parts are at most DmgValue
    int valuePerPart = DmgValue / parts; // Base damage value per part
    int remainderValue = DmgValue % parts; // Remaining damage to distribute

    List<int> values = new List<int>(new int[parts]);

    // Distribute the base value evenly over the parts
    for (int i = 0; i < parts; i++) {
        values[i] = valuePerPart;
    }

    // Distribute the remainder evenly over the first parts
    for (int i = 0; i < remainderValue; i++) {
        values[i]++;
    }

    return values;
}
	public bool charging = false;
	[Server]
	public void ServerMOCharge(MovingObject target){

		StopATTACKINGTOMOVE();
		if(agent.enabled)
		agent.isStopped = true;
    	agent.ResetPath();
		charging = true;
		if(agent.enabled)
		agent.isStopped = false;
		agent.acceleration = 25f;
        agent.speed = 8f;
		agent.radius = .125f;
		StartCoroutine(ChargingUnit(target));
	}
	[Command]
	public void CmdMoveToCast(MovingObject target, string mode, float rangeToCast, Vector2 mousePosition){
		ServerMoveToCast(target, mode, rangeToCast, mousePosition);
	}
	Coroutine MoveToCast;
	[Server]
	public void ServerMoveToCast(MovingObject target, string mode, float rangeToCast, Vector2 mousePosition){
		StopATTACKINGTOMOVE();
		StopCASTINGTOMOVE();
		if(Casting){
			Casting = false;
			RpcCancelMOCast();
		}
		
		agent.ResetPath();
		//print($"{rangeToCast} is max range of this spell");
		MoveToCast = StartCoroutine(MovingToCast(target, mode, rangeToCast, mousePosition));
	}
	private bool isWalking = false;
	public bool rightFace;
	public bool ready = false;
	public void SetMobReady(){
		
		ready = true;
	}
	public void SetPlayerReady(){
		
		ready = true;
		PlayerCharacter pc = GetComponent<PlayerCharacter>();
		if(pc){
			pc.StartProcessBeginningBuffsServer();
		}
	}
	private Vector3 directionPos;
private float updateCooldown = .1f; // 1 second cooldown
private float timeSinceLastUpdate = 0.0f;
private float lastMovementDirectionX;
	private float resetTimer = 0f;
	[Server]
	public void ServerResetTimer(){
		resetTimer = 0f;
	}
	int lastHealth = 1;
	bool died = false;
	[ClientRpc]
    public void RpcTurnToFaceTarget(Vector3 targetPosition){
		SpriteRenderer sRend = GetComponent<SpriteRenderer>();
    			// Flip the sprite if necessary
		bool rightfacing = false;
		if(transform.position.x < targetPosition.x){
			rightfacing = true;
		}
    	sRend.flipX = rightfacing;	
    }
	protected virtual void Update()
	{
	
		// Update the cooldown timer
    		timeSinceLastUpdate += Time.deltaTime;

		if (isServer && Energized && !Dying && agent.enabled && ready)
		{
			Mob mobCheck = GetComponent<Mob>();
			if (agent.hasPath && agent.enabled)
			{
				if(mobCheck){
					if(mobCheck.Resetting){
						float distanceFromOrigin = Vector2.Distance(Origin, transform.position);
						if(distanceFromOrigin <= 1f){
							//reset this object and its group
							//figure out group
							mobCheck.ResetCheckForNearByTargets();
						}
					}
				}
				if (moving && Casting)
				{
					ServerStopCasting();
				}
				if(!isWalking){
					isWalking = true;
					moving = true;
					agent.isStopped = false;
					//RpcUpdateWalkingState(true);
				}
				//if(timeSinceLastUpdate >= updateCooldown && isWalking){
				//	timeSinceLastUpdate = 0;
				//	ServerUpdateDirection();
				//}
				//accumulatedDistance += Vector3.Distance(agent.transform.position, lastPosition); // Update the accumulated distance
                //if (accumulatedDistance >= .5f)
                //{
				//	if(timeSinceLastUpdate >= updateCooldown){
				//		timeSinceLastUpdate = 0;
				//		ServerUpdateDirection();
				//	}
                //    accumulatedDistance = 0; // Reset the accumulated distance
                //    Vector3 updateLocation = new Vector3(Mathf.FloorToInt(transform.position.x) + 0.5f, Mathf.FloorToInt(transform.position.y) + 0.5f, 0);
                //    NewFogUpdate(updateLocation); // Run NewFogUpdate
                //}
				//ChangedDirection(lastPosition);
                lastPosition = agent.transform.position; // Update the last position
            }
			
			if (!agent.hasPath && agent.enabled || agent.velocity.sqrMagnitude == 0f && agent.enabled){
            	//RadiusLock = false;
				//ChangedDirection(lastPosition);
                if (moving) // If the agent was previously moving
                {
                    moving = false;
                    // If agent has moved at least 1 unit since last update
                    accumulatedDistance = 0; // Reset the accumulated distance
	            	lastPosition = agent.transform.position; // Update the last position
					isWalking = false;
					//RpcUpdateWalkingState(false); // Notify all clients
					timeSinceLastUpdate = 0;
                }
				//if(timeSinceLastUpdate >= updateCooldown){
				//	Vector3 updateLocation = new Vector3(Mathf.FloorToInt(transform.position.x) + 0.5f, Mathf.FloorToInt(transform.position.y) + 0.5f, 0);
                //	NewFogUpdate(updateLocation); // Run NewFogUpdate
				//	timeSinceLastUpdate = 0;
				//}
                lastPosition = agent.transform.position; // Update the last position
				//Vector3 updateLocation = new Vector3(Mathf.FloorToInt(transform.position.x) + 0.5f, Mathf.FloorToInt(transform.position.y) + 0.5f, 0);
                //NewFogUpdate(updateLocation); // Run NewFogUpdate
            }
			//if (agent.velocity.sqrMagnitude < 0.1f && !agent.pathPending)
        	//{
        	//    agent.ResetPath();
        	//}
			if(mobCheck){
				if(mobCheck.Resetting){
    				resetTimer += Time.deltaTime;
					if(resetTimer > 2.5f){
						float distanceFromOrigin = Vector2.Distance(Origin, transform.position);
						if(distanceFromOrigin <= 1f){
							//reset this object and its group
							//figure out group
							mobCheck.ResetCheckForNearByTargets();
						} else {
							if(!agent.hasPath){
								ServerMoveToTargetPosition(mobCheck.ModifyOriginRandomlyNonStatic(Origin));
							}
						}
					}
				}
			}
        }
		if(!isServer && ReadyToPlay){
			if(lastHealth > cur_hp && !Dying && !Hide && !Sneak && !Invisibility && !InvisibilityUndead){
				ChangeColorForAnimation("E33A00", 1f);
				//Mob mobcheck = GetComponent<Mob>();
				//if(mobcheck){
				//	mobcheck.DamageOccured();
				//}
			}
			if(cur_hp > lastHealth && !Dying && !Hide && !Sneak && !Invisibility && !InvisibilityUndead){
				ChangeColorForAnimation("3AE300", 1f);
			}
			if(Dying && !died){
				died = true;
				SpawnDeathNpc(spawnedDead);
				//if(spawnedDead){
				//	SpawnDeath();
				//} else {
				//	SpawnDeathNpc();
				//}
			}
			healthBarSlider.value = (float)cur_hp / (float)max_hp;
			lastHealth = cur_hp;
			magicPointBarSlider.value = (float)cur_mp / (float)max_mp;
			if(moving && animator != null && animator.runtimeAnimatorController != null && !animator.GetBool("IsWalking")){
				RpcUpdateWalkingStateTest(true);
			}
			if(!moving && animator != null && animator.runtimeAnimatorController != null && animator.GetBool("IsWalking")){
				RpcUpdateWalkingStateTest(false);
			}
			if(moving){
				ChangedDirection(lastPosition);
			}
			if(timeSinceLastUpdate >= updateCooldown){
				lastPosition = agent.transform.position;
				timeSinceLastUpdate = 0;
			}
			PlayerCharacter PC = GetComponent<PlayerCharacter>();
			Mob mob = GetComponent<Mob>();
			if(Hide){
				ClientHide();
			}
			if(Sneak){
				ClientHide();
			}
			if(Invisibility){
				ClientHide();
			}
			if(InvisibilityUndead){
				ClientHide();
			}
			if(PC){
				if(PC.assignedPlayer == ScenePlayer.localPlayer){
					if(stamina < 0)
        			{
        				ctSlider.value = stamina/-100f;
						ctImage.color = YellowColorRef;
        			}
        			else
        			{
        			    ctSlider.value = stamina/250f;
        				ctImage.color = GrayColorRef;
        			}
				} else {
					if(stamina < 0)
        			{
        				ctSlider.value = stamina/-100f;
						if(ScenePlayer.localPlayer.GetFriendlyList().Contains(this)){
							ctImage.color = GreenColorRef;
						} else {
							ctImage.color = RedColorRef;
						}
        			}
        			else
        			{
        			    ctSlider.value = stamina/250f;
        				ctImage.color = GrayColorRef;
        			}
				}
			}
			if(mob){
				if(stamina < 0)
        		{
					if(ScenePlayer.localPlayer.GetFriendlyList().Contains(this)){
						if(ctSlider.enabled){
							ctSlider.value = stamina/-100f;
							ctImage.color = GreenColorRef;
						}
					}
        		}
        		else
        		{
					if(ScenePlayer.localPlayer.GetFriendlyList().Contains(this)){
						if(ctSlider.enabled){
							ctSlider.value = stamina/250f;
        				ctImage.color = GrayColorRef;
						}
					}
        		    
        		}
			}
		}
	}
	bool spawnedDead = false;
    
    [ClientRpc]
    public void RpcReadyToPlay(bool dead){
        spawnedDead = dead;
        ReadyToPlay = true;
    }
	[Command]
	public void CmdCancelMovementToCast	(){
		ServerCancelMovementToCast();
	}
	[Server]
	public void ServerCancelMovementToCast(){
		StartCoroutine(ServerStopWalkingToCast());
	}
	IEnumerator ServerStopWalkingToCast(){
		if (agent == null) 
    	{
    	    Debug.LogWarning("NavMeshAgent not found.");
			agent = GetComponent<NavMeshAgent>();
			if (agent == null) 
    		{
    		    Debug.LogWarning("NavMeshAgent not found again.");
    		    yield break;
    		}
    	}
		Vector3 lastDestination = agent.destination; // Save the last set destination\
		if(agent.enabled)
    	agent.isStopped = true;
    	while (Casting)
    	{
    	    yield return null;
    	}
		if(agent.enabled)
    	agent.isStopped = false;
    	// Check if you need to set the destination again. 
    	// You may want to add more conditions to decide this.
    	if (lastDestination != null) 
    	{
			if(agent.enabled)
    	    agent.SetDestination(lastDestination);
    	}
	}
	[Command]
    public void CmdCastAOESpellMOB(string mode, Vector2 mousePosition){
		if(Casting){
			Casting = false;
        	RpcCancelCastAnimation();
		}
        
        
        string _spell = string.Empty;
        if(mode == CastingQ){
            _spell = SpellQ;
            if(SpellQCoolDown){
                return;
            }
        }
        if(mode == CastingE){
            _spell = SpellE;
            if(SpellECoolDown){
                return;
            }
        }
        if(mode == CastingR){
            _spell = SpellR;
            if(SpellRCoolDown){
                return;
            }
        }
        if(mode == CastingF){
            _spell = SpellF;
            if(SpellFCoolDown){
                return;
            }
        }
        var nameMatch = System.Text.RegularExpressions.Regex.Match(_spell, @"^\D*");
        string spell = nameMatch.Value.Trim(); // Trim any trailing spaces
        int _spellRank = 1;
        // Extract spell rank
        var rankMatch = System.Text.RegularExpressions.Regex.Match(_spell, @"\d+$");
        if (rankMatch.Success) {
            _spellRank = int.Parse(rankMatch.Value); // Parse the rank number
        }
        int cost = StatAsset.Instance.GetSpellCost(spell);
        float cooldown = StatAsset.Instance.GetSpellCooldown(spell, _spellRank, 0);
        if(cur_mp < cost){
			return;
		}
        //print($"about to set the cooldown time {cooldown} for {CharacterName} using {spell} and the mode is {mode}");
        if(mode == CastingQ){
            RunSetAbilityCooldownX(cooldown, false, "Q");
        }
        if(mode == CastingE){
            RunSetAbilityCooldownX(cooldown, false, "E");
        }
        if(mode == CastingR){
            RunSetAbilityCooldownX(cooldown, false, "R");
        }
        if(mode == CastingF){
            RunSetAbilityCooldownX(cooldown, false, "F");
        }
        bool hostile = !StatAsset.Instance.DetermineFriendly(spell);
        ProcessAOESpell(spell, _spellRank, cost, mousePosition, hostile);
    }
    
    [Command]
	public void CmdInstantCastSpellMOB(string mode, MovingObject target){
        // all single target or instant cast spells
		if(Casting){
			Casting = false;
        	RpcCancelCastAnimation();
		}
        if(Invisibility){
		    ServerRemoveStatus("Stealthed", "Invisibility", true);
        }
        string _spell = string.Empty;
        if(mode == CastingQ){
            _spell = SpellQ;
            if(SpellQCoolDown){
                return;
            }
        }
        if(mode == CastingE){
            _spell = SpellE;
            if(SpellECoolDown){
                return;
            }
        }
        if(mode == CastingR){
            _spell = SpellR;
            if(SpellRCoolDown){
                return;
            }
        }
        if(mode == CastingF){
            _spell = SpellF;
            if(SpellFCoolDown){
                return;
            }
        }
        Vector3 direction = target.transform.position - transform.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        // Determine the direction string based on the angle
        string directionString = GetDirectionString(angle);
		Mob mob = GetComponent<Mob>();
		PlayerCharacter pc = GetComponent<PlayerCharacter>();
		string ourName = string.Empty;
		if(mob){
			ourName = mob.NAME;
		}
		if(pc){
			ourName = pc.CharacterName;
		}
        Debug.Log(ourName + " is facing direction: " + directionString);		
		if(DirectionString != directionString){
			DirectionString = directionString;
			RpcSetDirectionFacing(directionString);
		}
	    //bool newRightFace = direction.x >= 0;
	    //if (newRightFace != rightFace)
	    //{
	    //	rightFace = newRightFace;
	    //	RpcUpdateFacingDirection(newRightFace);
	    //}
        
        
        
        print($"Made it to CmdCastSpell");
        var nameMatch = System.Text.RegularExpressions.Regex.Match(_spell, @"^\D*");
        string spell = nameMatch.Value.Trim(); // Trim any trailing spaces
        int _spellRank = 1;
        // Extract spell rank
        var rankMatch = System.Text.RegularExpressions.Regex.Match(_spell, @"\d+$");
        if (rankMatch.Success) {
            _spellRank = int.Parse(rankMatch.Value); // Parse the rank number
        }
        if(spell != "Resurrect"){
            if(target.Dying){
                TargetCannotCastOnDead();
                return;
            }
        }
        RpcCastingSpell(spell);

        float selfCast = StatAsset.Instance.SelfCasted(spell);
        bool selfCasted = false;
        if(selfCast == 1){
            selfCasted = true;
        }
        if(!StatAsset.Instance.InSpellRange(this, target, mode, new Vector2(), out float finalRange) && !selfCasted){
            print($"target {target.gameObject.name} was out of range of spell {_spell}");
            return;
        }
		int cost = StatAsset.Instance.GetSpellCost(spell);
        float cdReductionPercentage = 0;
        float cooldown = StatAsset.Instance.GetSpellCooldown(spell, _spellRank, cdReductionPercentage);
        if(cur_mp < cost){
			return;
		}
        //print($"about to set the cooldown time {cooldown} for {CharacterName} using {spell} and the mode is {mode}");
        if(mode == CastingQ){
            RunSetAbilityCooldownX(cooldown, false, "Q");
        }
        if(mode == CastingE){
            RunSetAbilityCooldownX(cooldown, false, "E");
        }
        if(mode == CastingR){
            RunSetAbilityCooldownX(cooldown, false, "R");
        }
        if(mode == CastingF){
            RunSetAbilityCooldownX(cooldown, false, "F");
        }
		ProcessSpellCast(mode, this, target, cost);
        if(spell != "Charge"){
            //RpcCastingSpell(spell, null);

			//string ownerName = string.Empty;
        	//string targetName = string.Empty;
        	//if(pc){
        	//   ownerName = pc.CharacterName;
        	//} else {
			//	ownerName = StatAsset.Instance.GetMobName(StatAsset.Instance.GetEnemyToPrefabMapping(), mob.NAME);
        	//}
        	//if(target){
        	//    PlayerCharacter pcChecktarget = target.GetComponent<PlayerCharacter>();
        	//    if(pcChecktarget){
        	//       targetName = pcChecktarget.CharacterName;
        	//    } else {
        	//        Mob mobCheckTarget = target.GetComponent<Mob>();
			//    	targetName = StatAsset.Instance.GetMobName(StatAsset.Instance.GetEnemyToPrefabMapping(), mobCheckTarget.NAME);
        	//    }
        	//}
			//AddRpcCall(null, null, false, true, spell, weaponType, false, ownerName, targetName);
        }

		//AOE
		//Aoe list enchanter GravityStun 5x5, ResistMagic 5x5
		//Aoe list priest turn undead 6x6, Groupheal 2x2
		//aoe list wizard IceBlast 3x3, IceSpear 1x3, FireBall 3x3, Meteor shower 6x6, Blizzard 4x4
		//aoe list archer 
		//aoe list fighter
		//aoe list rogue 
		//Single target
		//curatorTM.UpdatePlayerCastedDmgSpell();
		//Find the spell cost of each spell d
		//RpcAnimateSpell(spell, mousePosition);

	}
	/*
	public bool InSpellRange(MovingObject caster, MovingObject target, string mode){
        int lvl = 1;
		PlayerCharacter pc = caster.GetComponent<PlayerCharacter>();
		if(pc){
			lvl = caster.GetComponent<PlayerCharacter>().Level;
		}
        float range = 0f;
        float baseRange = 0f;
        bool inRange = false;
        string _spellname = string.Empty;
        if(mode == CastingQ){
            _spellname = caster.SpellQ;
        }
        if(mode == CastingE){
            _spellname = caster.SpellE;
        }
        if(mode == CastingR){
            _spellname = caster.SpellR;
        }
        if(mode == CastingF){
            _spellname = caster.SpellF;
        }
        var nameMatch = System.Text.RegularExpressions.Regex.Match(_spellname, @"^\D*");
        string spell = nameMatch.Value.Trim(); // Trim any trailing spaces
        int _spellRank = 1;
        // Extract spell rank
        var rankMatch = System.Text.RegularExpressions.Regex.Match(_spellname, @"\d+$");
        if (rankMatch.Success) {
            _spellRank = int.Parse(rankMatch.Value); // Parse the rank number
        }
        if (spell == "Aimed Shot")
            baseRange = 6f;
        if (spell == "Bandage Wound")
            baseRange = 1.5f;
        if (spell == "Head Shot")
            baseRange = 5f;
        if (spell == "Silence Shot")
            baseRange = 5f;
        if (spell == "Crippling Shot")
            baseRange = 5f;
        if (spell == "Dash")
            baseRange = 3f;
        if (spell == "Identify Enemy")
            baseRange = 5f;
        if (spell == "Double Shot")
            baseRange = 6f;
        if (spell == "Fire Arrow")
            baseRange = 6f;
        if (spell == "Penetrating Shot")
            baseRange = 6f;
        if (spell == "Sleep")
            baseRange = 5f;

        // Enchanter Skills
        if (spell == "Mesmerize")
            baseRange = 5f;
        if (spell == "Haste")
            baseRange = 5f;
        if (spell == "Root")
            baseRange = 5f;
        if (spell == "Invisibility")
            baseRange = 5f;
        if (spell == "Rune")
            baseRange = 5f;
        if (spell == "Slow")
            baseRange = 5f;
        if (spell == "Magic Sieve")
            baseRange = 5f;
        if (spell == "Aneurysm")
            baseRange = 5f;
        if (spell == "Gravity Stun")
            baseRange = 5f;
        if (spell == "Weaken")
            baseRange = 5f;
        if (spell == "Resist Magic")
            baseRange = 5f;
        if (spell == "Purge")
            baseRange = 5f;
        if (spell == "Charm")
            baseRange = 5f;
        if (spell == "Mp Transfer")
            baseRange = 1.5f;

        // Fighter Skills
        if (spell == "Charge")
            baseRange = 5f;
        if (spell == "Bash")
            baseRange = 1.5f;
        if (spell == "Intimidating Roar")
            baseRange = 1.5f;
        if (spell == "Protect")
            baseRange = 1.5f;
        if (spell == "Knockback")
            baseRange = 1.5f;
        if (spell == "Throw Stone")
            baseRange = 5f;
        if (spell == "Heavy Swing")
            baseRange = 1.5f;
        if (spell == "Taunt")
            baseRange = 1.5f;
        // Priest Skills
        if (spell == "Holy Bolt")
            baseRange = 5f;
        if (spell == "Heal")
            baseRange = 5f;
        if (spell == "Cure Poison")
            baseRange = 5f;
        if (spell == "Dispel")
            baseRange = 5f;
        if (spell == "Fortitude")
            baseRange = 5f;
        if (spell == "Turn Undead")
            baseRange = 5f;
        if (spell == "Undead Protection")
            baseRange = 3f;
        if (spell == "Smite")
            baseRange = 5f;
        if (spell == "Shield Bash")
            baseRange = 1.5f;
        if (spell == "Greater Heal")
            baseRange = 3f;
        if (spell == "Group Heal")
            baseRange = 4f;
        if (spell == "Resurrect")
            baseRange = 5f;

        // Rogue Skills
        if (spell == "Shuriken")
            baseRange = 5f;
        if (spell == "Picklock")
            baseRange = 1.5f;
        if (spell == "Steal")
            baseRange = 1.5f;
        if (spell == "Tendon Slice")
            baseRange = 1.5f;
        if (spell == "Backstab")
            baseRange = 1.5f;
        if (spell == "Blind")
            baseRange = 5f;
        if (spell == "Poison")
            baseRange = 1.5f;
        // Wizard Skills
        if (spell == "Ice")
            baseRange = 5f;
        if (spell == "Fire")
            baseRange = 5f;
        if (spell == "Blizzard")
            baseRange = 6f;
        if (spell == "Magic Burst")
            baseRange = 1.5f;
        if (spell == "Teleport")
            baseRange = 7f;
        if (spell == "Meteor Shower")
            baseRange = 7f;
        if (spell == "Ice Block")
            baseRange = 1.5f;
        if (spell == "Ice Blast")
            baseRange = 5f;
        if (spell == "Incinerate")
            baseRange = 5f;
        if (spell == "Brain Freeze")
            baseRange = 5f;
		if (spell == "Drain")
            baseRange = 5f;
		if (spell == "Harden")
            baseRange = 1f;
		if (spell == "Spit")
            baseRange = 5f;
        if (spell == "Light")
            baseRange = 5f;
        if (spell == "Magic Missile")
            baseRange = 5f;

        // Druid Skills
        if (spell == "Rejuvenation")
            baseRange = 5f;
        if (spell == "Swarm Of Insects")
            baseRange = 5f;
        if (spell == "Thorns")
            baseRange = 5f;
        if (spell == "Nature's Protection")
            baseRange = 5f;
        if (spell == "Strength")
            baseRange = 5f;
        if (spell == "Snare")
            baseRange = 5f;
        if (spell == "Engulfing Roots")
            baseRange = 5f;
        if (spell == "Shapeshift")
            baseRange = 1.5f;
        if (spell == "Tornado")
            baseRange = 5f;
        if (spell == "Chain Lightning")
            baseRange = 5f;
        if (spell == "Greater Rejuvenation")
            baseRange = 5f;
        if (spell == "Solar Flare")
            baseRange = 7f;
        if (spell == "Evacuate")
            baseRange = 4f;

        // Paladin Skills
        if (spell == "Holy Swing")
            baseRange = 5f;
        if (spell == "Divine Armor")
            baseRange = 5f;
        if (spell == "Flash Of Light")
            baseRange = 5f;
        if (spell == "Undead Slayer")
            baseRange = 0f;
        if (spell == "Stun")
            baseRange = 5f;
        if (spell == "Celestial Wave")
            baseRange = 5f;
        if (spell == "Angelic Shield")
            baseRange = 1.5f;
        if (spell == "Cleanse")
            baseRange = 5f;
        if (spell == "Consecrated Ground")
            baseRange = 5f;
        if (spell == "Divine Wrath")
            baseRange = 1.5f;
        if (spell == "Cover")
            baseRange = 1.5f;
        if (spell == "Shackle")
            baseRange = 5f;
        if (spell == "Lay On Hands")
            baseRange = 1.5f;
        range = (baseRange * ((_spellRank - 1) * .004f) + baseRange);
        
        float distance = Vector2.Distance(caster.transform.position, target.transform.position);

        if(distance <= range)
        {
            inRange = true;
            
        }
        else
        {
            inRange = false;
        }
        print($"{baseRange} is the base range for the spell {spell}");
        return inRange;
    }
	*/
	[TargetRpc]
    public void TargetCannotCastOnDead(){
        ImproperCheckText.Invoke("Cannot cast on a dead target");
    }
	/*
    float SelfCasted(string spell)
    {
        float result = 0f;
        if(spell == "Divine Armor"){
            result = 1f;
        }
		if(spell == "Harden"){
            result = 1f;
        }
        if(spell == "Ice Block"){
            result = 1f;
        }
        if(spell == "Light"){
            result = 1f;
        }
        if(spell == "Solar Flare"){
            result = 1f;
        }
        if(spell == "Nature's Precision"){
            result = 1f;
        }
        if(spell == "Block"){
            result = 1f;
        }
        if(spell == "Tank Stance"){
            result = 1f;
        }
        if(spell == "Offensive Stance"){
            result = 1f;
        }
        if(spell == "Intimidating Roar"){
            result = 1f;
        }
        if(spell == "Sneak"){
            result = 1f;
        }
        if(spell == "Angelic Shield"){
            result = 1f;
        }
        if(spell == "Celestial Wave"){
            result = 1f;
        }
        if(spell == "Shapeshift"){
            result = 1f;
        }
        if(spell == "Rush"){
            result = 1f;
        }
        if(spell == "Track"){
            result = 1f;
        }
        if(spell == "Undead Protection"){
            result = 1f;
        }
        if(spell == "Turn Undead"){
            result = 1f;
        }
        if(spell == "Magic Burst"){
            result = 1f;
        }
        if(spell == "Consecrated Ground"){
            result = 1f;
        }
        if(spell == "Detect Traps"){
            result = 1f;
        }
        if(spell == "Hide"){
            result = 1f;
        }
        return result;
    }
	*/
    [Command]
	public void CmdInstantCastSpellSelfCastMOB(string mode, Vector3 target){
        // all single target or instant cast spells
        if(Invisibility){
		    ServerRemoveStatus("Stealthed", "Invisibility", true);
        }
        if(Casting){
			Casting = false;
        	RpcCancelCastAnimation();
		}
        string _spell = string.Empty;
        if(mode == CastingQ){
            _spell = SpellQ;
            if(SpellQCoolDown){
                return;
            }
        }
        if(mode == CastingE){
            _spell = SpellE;
            if(SpellECoolDown){
                return;
            }
        }
        if(mode == CastingR){
            _spell = SpellR;
            if(SpellRCoolDown){
                return;
            }
        }
        if(mode == CastingF){
            _spell = SpellF;
            if(SpellFCoolDown){
                return;
            }
        }
        Vector3 movementDirection = target - transform.position;
		//Vector2 direction = new Vector2(targetPosition.x - transform.position.x, targetPosition.y - transform.position.y);
        float angle = Mathf.Atan2(movementDirection.y, movementDirection.x) * Mathf.Rad2Deg;
        // Determine the direction string based on the angle
        string directionString = GetDirectionString(angle);
        Mob mob = GetComponent<Mob>();
		PlayerCharacter pc = GetComponent<PlayerCharacter>();
		string ourName = string.Empty;
		if(mob){
			ourName = mob.NAME;
		}
		if(pc){
			ourName = pc.CharacterName;
		}
        Debug.Log(ourName + " is facing direction: " + directionString);	
		if(DirectionString != directionString){
			DirectionString = directionString;
			RpcSetDirectionFacing(directionString);
		}
	    //bool newRightFace = movementDirection.x >= 0;
	    //if (newRightFace != rightFace)
	    //{
	    //	rightFace = newRightFace;
	    //	RpcUpdateFacingDirection(newRightFace);
	    //}
        //if(!StatAsset.Instance.InSpellRange(this, target, mode)){
        //    print($"target {target.gameObject.name} was out of range of spell {_spell}");
        //    return;
        //}
        print($"Made it to CmdCastSpell");
        var nameMatch = System.Text.RegularExpressions.Regex.Match(_spell, @"^\D*");
        string spell = nameMatch.Value.Trim(); // Trim any trailing spaces
        int _spellRank = 1;
        // Extract spell rank
        var rankMatch = System.Text.RegularExpressions.Regex.Match(_spell, @"\d+$");
        if (rankMatch.Success) {
            _spellRank = int.Parse(rankMatch.Value); // Parse the rank number
        }
		int cost = StatAsset.Instance.GetSpellCost(spell);
        float cdReductionPercentage = 0;
        float cooldown = StatAsset.Instance.GetSpellCooldown(spell, _spellRank, cdReductionPercentage);
        if(cur_mp < cost){
			return;
		}
        //print($"about to set the cooldown time {cooldown} for {CharacterName} using {spell} and the mode is {mode}");
        if(mode == CastingQ){
            RunSetAbilityCooldownX(cooldown, false, "Q");
        }
        if(mode == CastingE){
            RunSetAbilityCooldownX(cooldown, false, "E");
        }
        if(mode == CastingR){
            RunSetAbilityCooldownX(cooldown, false, "R");
        }
        if(mode == CastingF){
            RunSetAbilityCooldownX(cooldown, false, "F");
        }
        bool hostile = !StatAsset.Instance.DetermineFriendly(spell);
		ProcessAOESpell(spell, _spellRank, cost, target, hostile);
		//AOE
		//Aoe list enchanter GravityStun 5x5, ResistMagic 5x5
		//Aoe list priest turn undead 6x6, Groupheal 2x2
		//aoe list wizard IceBlast 3x3, IceSpear 1x3, FireBall 3x3, Meteor shower 6x6, Blizzard 4x4
		//aoe list archer 
		//aoe list fighter
		//aoe list rogue 
		//Single target
		//curatorTM.UpdatePlayerCastedDmgSpell();
		//Find the spell cost of each spell d
		//RpcAnimateSpell(spell, mousePosition);

	}
	[Server]
	public void ServerCastSpellMOB(string mode, MovingObject target){
        // all single target or instant cast spells
		if(Casting){
			Casting = false;
        	RpcCancelCastAnimation();
		}

		Mob mob = GetComponent<Mob>();

        string _spell = string.Empty;
        if(mode == CastingQ){
            _spell = SpellQ;
            if(SpellQCoolDown){
                return;
            }
        }
        if(mode == CastingE){
            _spell = SpellE;
            if(SpellECoolDown){
                return;
            }
        }
        if(mode == CastingR){
            _spell = SpellR;
            if(SpellRCoolDown){
                return;
            }
        }
        if(mode == CastingF){
            _spell = SpellF;
            if(SpellFCoolDown){
                return;
            }
        }
        Vector3 movementDirection = target.transform.position - transform.position;
		//Vector2 direction = new Vector2(targetPosition.x - transform.position.x, targetPosition.y - transform.position.y);
        float angle = Mathf.Atan2(movementDirection.y, movementDirection.x) * Mathf.Rad2Deg;
        // Determine the direction string based on the angle
        string directionString = GetDirectionString(angle);
		PlayerCharacter pc = GetComponent<PlayerCharacter>();
		string ourName = string.Empty;
		if(mob){
			ourName = mob.NAME;
		}
		if(pc){
			ourName = pc.CharacterName;
		}
        Debug.Log(ourName + " is facing direction: " + directionString);	
		if(DirectionString != directionString){
			DirectionString = directionString;
			RpcSetDirectionFacing(directionString);
		}
	    //bool newRightFace = movementDirection.x >= 0;
	    //if (newRightFace != rightFace)
	    //{
	    //	rightFace = newRightFace;
	    //	RpcUpdateFacingDirection(newRightFace);
	    //}
        if(!StatAsset.Instance.InSpellRange(this, target, mode, new Vector2(), out float finalRange)){
            print($"target {target.gameObject.name} was out of range of spell {_spell}");
            return;
        }
        print($"Made it to ServerCastSpellMOB");
        var nameMatch = System.Text.RegularExpressions.Regex.Match(_spell, @"^\D*");
        string spell = nameMatch.Value.Trim(); // Trim any trailing spaces
        int _spellRank = 1;
        // Extract spell rank
        var rankMatch = System.Text.RegularExpressions.Regex.Match(_spell, @"\d+$");
        if (rankMatch.Success) {
            _spellRank = int.Parse(rankMatch.Value); // Parse the rank number
        }
        if(spell != "Resurrect"){
            if(target.Dying){
				mob.DamageTaken();
                return;
            }
        }
        RpcCastingSpell(spell);

		int cost = StatAsset.Instance.GetSpellCost(spell);
        float cdReductionPercentage = 0;
        float cooldown = StatAsset.Instance.GetSpellCooldown(spell, _spellRank, cdReductionPercentage);
        if(cur_mp < cost){
			mob.DamageTaken();
			return;
		}
		if(spell == "Harden"){
			cooldown += 180f;
		}
        //print($"about to set the cooldown time {cooldown} for {CharacterName} using {spell} and the mode is {mode}");
        if(mode == CastingQ){
            RunSetAbilityCooldownX(cooldown, false, "Q");
        }
        if(mode == CastingE){
            RunSetAbilityCooldownX(cooldown, false, "E");
        }
        if(mode == CastingR){
            RunSetAbilityCooldownX(cooldown, false, "R");
        }
        if(mode == CastingF){
            RunSetAbilityCooldownX(cooldown, false, "F");
        }
		if(StatAsset.Instance.SkillShot(spell)){
			bool hostile = !StatAsset.Instance.DetermineFriendly(spell);
			ProcessAOESpell(spell, _spellRank, cost, target.transform.position, hostile);
		} else {
			ProcessSpellCast(mode, this, target, cost);
		}
	}
	
	[Command]
	public void CmdCastSpellMOB(string mode, MovingObject target){
        // all single target or instant cast spells
		if(Casting){
			Casting = false;
        	RpcCancelCastAnimation();
		}
        string _spell = string.Empty;
        if(mode == CastingQ){
            _spell = SpellQ;
            if(SpellQCoolDown){
                return;
            }
        }
        if(mode == CastingE){
            _spell = SpellE;
            if(SpellECoolDown){
                return;
            }
        }
        if(mode == CastingR){
            _spell = SpellR;
            if(SpellRCoolDown){
                return;
            }
        }
        if(mode == CastingF){
            _spell = SpellF;
            if(SpellFCoolDown){
                return;
            }
        }
        Vector3 movementDirection = target.transform.position - transform.position;
		//Vector2 direction = new Vector2(targetPosition.x - transform.position.x, targetPosition.y - transform.position.y);
        float angle = Mathf.Atan2(movementDirection.y, movementDirection.x) * Mathf.Rad2Deg;
        // Determine the direction string based on the angle
        string directionString = GetDirectionString(angle);
        Mob mob = GetComponent<Mob>();
		PlayerCharacter pc = GetComponent<PlayerCharacter>();
		string ourName = string.Empty;
		if(mob){
			ourName = mob.NAME;
		}
		if(pc){
			ourName = pc.CharacterName;
		}
        Debug.Log(ourName + " is facing direction: " + directionString);	
		if(DirectionString != directionString){
			DirectionString = directionString;
			RpcSetDirectionFacing(directionString);
		}
	    //bool newRightFace = movementDirection.x >= 0;
	    //if (newRightFace != rightFace)
	    //{
	    //	rightFace = newRightFace;
	    //	RpcUpdateFacingDirection(newRightFace);
	    //}
        if(!StatAsset.Instance.InSpellRange(this, target, mode, new Vector2(), out float finalRange)){
            print($"target {target.gameObject.name} was out of range of spell {_spell}");
            return;
        }
        print($"Made it to CmdCastSpell");
        var nameMatch = System.Text.RegularExpressions.Regex.Match(_spell, @"^\D*");
        string spell = nameMatch.Value.Trim(); // Trim any trailing spaces
        int _spellRank = 1;
        // Extract spell rank
        var rankMatch = System.Text.RegularExpressions.Regex.Match(_spell, @"\d+$");
        if (rankMatch.Success) {
            _spellRank = int.Parse(rankMatch.Value); // Parse the rank number
        }
        if(spell != "Resurrect"){
            if(target.Dying){
                TargetCannotCastOnDead();
                return;
            }
        }
        RpcCastingSpell(spell);

		int cost = StatAsset.Instance.GetSpellCost(spell);
        float cdReductionPercentage = 0;
        float cooldown = StatAsset.Instance.GetSpellCooldown(spell, _spellRank, cdReductionPercentage);
        if(cur_mp < cost){
			return;
		}
        //print($"about to set the cooldown time {cooldown} for {CharacterName} using {spell} and the mode is {mode}");
        if(mode == CastingQ){
            RunSetAbilityCooldownX(cooldown, false, "Q");
        }
        if(mode == CastingE){
            RunSetAbilityCooldownX(cooldown, false, "E");
        }
        if(mode == CastingR){
            RunSetAbilityCooldownX(cooldown, false, "R");
        }
        if(mode == CastingF){
            RunSetAbilityCooldownX(cooldown, false, "F");
        }
		ProcessSpellCast(mode, this, target, cost);
	}
	void CheckIfOwnerBeforeAllCancellation(ScenePlayer sPlayer){
		if(Dying){
			return;
		}
		if(sPlayer.GetComponent<NetworkIdentity>().hasAuthority){
			CmdCancelSpell();
		}
	}
	[Command]
	public void CmdCancelSpell	(){
		if(Casting){
			ServerStopCasting();
		}
	}
	[Server]
	public void ServerStopCasting(){
		if(Casting){
			Casting = false;
			RpcCancelMOCast();
			StopCASTINGTOMOVE();
			PlayerCharacter playerCharacter = GetComponent<PlayerCharacter>();
			if(playerCharacter){
				RpcCancelCastAnimation();
			}
		}
		
	}
	[Server]
	public void HarvestStart(){
		
		if (agent == null) 
    	{
    	    Debug.LogWarning("NavMeshAgent not found.");
			agent = GetComponent<NavMeshAgent>();
			if (agent == null) 
    		{
    		    Debug.LogWarning("NavMeshAgent not found again.");
    		    return;
    		}
    	}
		if(agent.enabled)
    	agent.isStopped = true;
		if(agent.enabled)
    	agent.ResetPath();
		Casting = false;
		//moving = false;
		//RpcCancelMovingForHarvest();
		//PlayerCharacter playerCharacter = GetComponent<PlayerCharacter>();
		//if(playerCharacter){
		//	RpcCancelCastAnimation();
		//}
	}
	[ClientRpc]
	void RpcCancelMovingForHarvest(){
		CancelCast.Invoke(this);
		PlayerCharacter pc = GetComponent<PlayerCharacter>();
		if(pc)
		animator.SetTrigger("endHarvest");
		animator.SetBool("IsWalking", false);
	}
	[ClientRpc]
	void RpcCancelMOCast(){
		CancelCast.Invoke(this);
		PlayerCharacter pc = GetComponent<PlayerCharacter>();
		if(pc)
		animator.SetTrigger("endHarvest");
	}
	[ClientRpc]
	void RpcCancelMOCastMOB(){
		CancelCast.Invoke(this);
	}
	void ProcessCharacterDeath(MovingObject deadMO, Match match){
		
		if(deadMO == this) { return; }
		PlayerCharacter pc = GetComponent<PlayerCharacter>();
		Mob mob = GetComponent<Mob>();
		if(pc){
			if(match == pc.assignedMatch){
				if(Target == deadMO){
					StopATTACKINGMob();
					Target = null;
					//need to tell client to remove
				}
			}
		} else if(mob){
			if(match == mob.assignedMatch){
				if(!mob.Aggro){
					print($"{mob.NAME} was not aggroed");
					return;
				}
				if(mob.threatList.ContainsKey(deadMO)){
					mob.threatList.Remove(deadMO);
				}
				//if(Target == deadMO){
				//	StopATTACKINGMob();
				//	Target = null;
				//	MovingObject newTarget = mob.GetHighestThreat();
				//	if(newTarget)
				//	{
				//		mob.Target = newTarget;
				//	}
				//}
				mob.DamageTaken();

			}
		}
	}
	void ProcessMobDeath(MovingObject deadMO, Match match){
		
		if(!deadMO){
			return;
		}
		if(deadMO == this){
			return;
		}
		if(Dying){
			return;
		}
		if(!Target){
			return;
		}
		PlayerCharacter pc = GetComponent<PlayerCharacter>();
		if(pc){
			if(match == pc.assignedMatch){
				if(Target == deadMO){
					StopATTACKINGMob();
					Target = null;
					//need to tell client to remove
				}
			}
		}
	}
	
	Coroutine ATTACKING;
	public Coroutine GetATTACKING(){
		return ATTACKING;
	}
	[Server]
	public void SetATTACKING(MovingObject target, bool directedByPlayer){
		if(Dying){
			return;
		}
		if(target.FROZEN){
			return;
		}
		Mob mob = GetComponent<Mob>();
		if(agent.enabled){
			agent.ResetPath();
		}
		if(Casting){
			Casting = false;
			if(agent.enabled){
				agent.isStopped = false;
			}
			RpcCancelMOCast();
		}
		if(agent.enabled){
			agent.isStopped = false;
		}
		if(ATTACKING != null){
			StopCoroutine(ATTACKING);
			ATTACKING = null;
		}
		if(mob){
			if(directedByPlayer){
				ATTACKING = StartCoroutine(AttackWithMob(target, directedByPlayer));
			} else {
				mob.Aggro = true;
				MovingObject newTarget = mob.GetHighestThreat();
				if(newTarget){
					ATTACKING = StartCoroutine(AttackWithMob(newTarget, directedByPlayer));
				}
			}
			
		} else {
			ATTACKING = StartCoroutine(AttackWithCharacter(target));
		}
	}
	void StopATTACKING(MovingObject target, Match match){
		Mob mob = GetComponent<Mob>();
		PlayerCharacter pc = GetComponent<PlayerCharacter>();
		bool sameMatch = false;
		if(pc){
			if(match == pc.assignedMatch){
				sameMatch = true;
			}

		} else if(mob) {
			if(match == mob.assignedMatch){
				sameMatch = true;
			}
		}
		if(sameMatch){
			if(Target != null){
				if(Target == target){
					if(ATTACKING != null){
						StopCoroutine(ATTACKING);
						moving = false;
						if(agent.enabled)
						agent.isStopped = true;
						Target = null;
					}
				}
			}
		}
	}
	public void StopATTACKINGTOMOVE(){
		if(ATTACKING != null){
			StopCoroutine(ATTACKING);
			ATTACKING = null;
			//isWalking = true;
			//RpcUpdateWalkingState(true);
		}
		//moving = false;
		//agent.isStopped = true;
	}
	public void StopCASTINGTOMOVE(){
		if(MoveToCast != null){
			StopCoroutine(MoveToCast);
			MoveToCast = null;
			//isWalking = true;
			//RpcUpdateWalkingState(true);
		}
		//moving = false;
		//agent.isStopped = true;
	}
	public void StopATTACKINGMob(){
		if(ATTACKING != null){
			StopCoroutine(ATTACKING);
			ATTACKING = null;
		}
	}
	MovingObject GetFreshTarget(bool friendly, bool heal){
		MovingObject targetAcquired = null;
		List<MovingObject> PossibleTargets = new List<MovingObject>();

		if(friendly){
			PossibleTargets.Add(this);
		}
		void AddPossibleTarget(MovingObject possibleTarget)
    	{
    	    if (possibleTarget == this) return;

    	    float distanceToTarget = Vector2.Distance(transform.position, possibleTarget.transform.position);
    	    if (HasLineOfSight(transform.position, possibleTarget.transform.position) && distanceToTarget < 10f)
    	    {
    	        if (friendly)
    	        {
    	            if (GetFriendly(possibleTarget))
    	            {
    	                if (!heal || (heal && (float)possibleTarget.cur_hp / (float)possibleTarget.max_hp <= 0.8f))
    	                {
    	                    PossibleTargets.Add(possibleTarget);
    	                }
    	            }
    	        }
    	        else
    	        {
    	            if (!GetFriendly(possibleTarget))
    	            {
    	                PossibleTargets.Add(possibleTarget);
    	            }
    	        }
    	    }
    	}
		foreach(var possibleTarget in curatorTM.GetENEMYList()){
			if(possibleTarget == this){
				continue;
			}
	    	float distanceToTarget = Vector2.Distance(transform.position, possibleTarget.transform.position);
			if(friendly){
				if(GetFriendly(possibleTarget)){
					if(HasLineOfSight(transform.position, possibleTarget.transform.position) && distanceToTarget < 10f){
						PossibleTargets.Add(possibleTarget);
					}
				}
			} else {
				if(!GetFriendly(possibleTarget)){
					if(HasLineOfSight(transform.position, possibleTarget.transform.position) && distanceToTarget < 10f){
						PossibleTargets.Add(possibleTarget);
					}
				}
			}
		}
		foreach (var possibleTarget in curatorTM.GetENEMYList())
	    {
	        AddPossibleTarget(possibleTarget);
	    }

	    foreach (var possibleTarget in curatorTM.GetPCList())
	    {
	        AddPossibleTarget(possibleTarget);
	    }

	    if (PossibleTargets.Count > 0)
	    {
	        System.Random random = new System.Random();
	        int randomIndex = random.Next(PossibleTargets.Count);
	        targetAcquired = PossibleTargets[randomIndex];
	    }

	    return targetAcquired;
	}
	Coroutine MobCasting;
	IEnumerator MobCastSpell(MovingObject target, string spell, int spellRank, float spellRange, float castTime, string mode){
		Mob MobCheck = GetComponent<Mob>();
		Casting = true;
		bool selfCasted = false;
		bool hostile = !StatAsset.Instance.DetermineFriendly(spell);
		float checkCast = StatAsset.Instance.SelfCasted(spell);
		if(checkCast > 0f){
			selfCasted = true;
			target = this;
		}
		bool heal = false;
		if(spell == "Heal" || spell == "Greater Heal"){
			heal = true;
		}
		if(!selfCasted){
			if(!hostile){
				if(!GetFriendly(target)){
					//find new target
					target = GetFreshTarget(hostile, heal);
					if(target == null){
						if(MobCheck){
							MobCheck.DamageTaken();
						}
						MobCasting = null;
						yield break;
					}
				}
			} else {
				if(GetFriendly(target)){
					//find new target
					target = MobCheck.GetHighestThreat();
					if(target == null){
						if(MobCheck){
							MobCheck.DamageTaken();
						}
						MobCasting = null;
						yield break;
					}
				}
			}
		}
		MobCheck.RpcMobCasting(castTime, spell);
		print($"waiting for {castTime} seconds to cast spell {spell}");
	    yield return new WaitForSeconds(castTime);
		
		if(target.Dying){
			if(MobCheck){
				MobCheck.DamageTaken();
			}
			MobCasting = null;
			yield break;
		}
		if(Dying){
			MobCasting = null;
			yield break;
		}
		Vector2 mousePosition = target.transform.position;
	    float distanceToTarget = Vector2.Distance(transform.position, target.transform.position);
		if(distanceToTarget > spellRange && !selfCasted){
			print($"waiting for {castTime} seconds to cast spell {spell}");
			ServerMoveToCastMob(target, mode, spellRange, mousePosition);
			MobCasting = null;
			yield break;
		} else {
			print($"CASTING spell {spell} after moving close enough");
			ServerCastSpellMOB(mode, target);
			MobCasting = null;

		}
		

	}
	public IEnumerator MovingToCast(MovingObject target, string mode, float spellRange, Vector2 mousePosition){
		if(target){
			if(target != Target){
				Target = target;
			}
			Mob MobCheck = GetComponent<Mob>();
			string _spell = string.Empty;
			if(mode == CastingQ){
    		    _spell = SpellQ;
    		}
    		if(mode == CastingE){
    		    _spell = SpellE;
    		}
    		if(mode == CastingR){
    		    _spell = SpellR;
    		}
    		if(mode == CastingF){
    		    _spell = SpellF;
    		}
			var nameMatch = System.Text.RegularExpressions.Regex.Match(_spell, @"^\D*");
        	string spell = nameMatch.Value.Trim(); // Trim any trailing spaces
        	int _spellRank = 1;
        	// Extract spell rank
        	var rankMatch = System.Text.RegularExpressions.Regex.Match(_spell, @"\d+$");
        	if (rankMatch.Success) {
        	    _spellRank = int.Parse(rankMatch.Value); // Parse the rank number
        	}

			ServerMoveToTargetPosition(target.transform.position);
			float checkTime = .1f;
			float timerCheck = 0;
	    	while (true)
	    	{
				bool failedToTarget = false;
				if(MobCheck){
					if(Target == null){
						failedToTarget = true;
					} else {
						if(Target.FROZEN){
							failedToTarget = true;
						}
						if(Target.Dying){
							failedToTarget = true;
						}
        				if(Target.Hide && !MobCheck.SneakTrueSight ){
							failedToTarget = true;
						}
						if(Target.Sneak && !MobCheck.SneakTrueSight){
							failedToTarget = true;
						}
						if(Target.Invisibility && !MobCheck.InvisTrueSight){
							failedToTarget = true;
						}
						if(Target.InvisibilityUndead && !MobCheck.InvisUndeadTrueSight){
							failedToTarget = true;
						}
					}
				}
				
				if(target == null || failedToTarget){
					if(agent.enabled){
						agent.isStopped = true;
						agent.ResetPath();
					}
					if(GetATTACKING() != null)
        			{
        			    StopATTACKINGTOMOVE();
        			}
					MoveToCast = null;
					MobCheck.DamageTaken();
        			//RadiusLock = false;
					yield break;
				}
	    	    // Iterate through each unit
	    	    // Compute the distance to the target
	    	    float distanceToTarget = Vector2.Distance(transform.position, target.transform.position);
				Debug.Log($"Distance to Target: {distanceToTarget}, Spell Range: {spellRange}");
				
	    	    if (distanceToTarget <= spellRange && HasLineOfSight(transform.position, target.transform.position))
	    	    {
	    	        // If it is, stop moving and start attacking
					if(agent.enabled)
					agent.isStopped = true;
					if(agent.enabled)
					agent.ResetPath();
					Vector3 movementDirection = target.transform.position - transform.position;
					//Vector2 direction = new Vector2(targetPosition.x - transform.position.x, targetPosition.y - transform.position.y);
        			float angle = Mathf.Atan2(movementDirection.y, movementDirection.x) * Mathf.Rad2Deg;
        			// Determine the direction string based on the angle
        			string directionString = GetDirectionString(angle);
        			Mob mob = GetComponent<Mob>();
					PlayerCharacter pc = GetComponent<PlayerCharacter>();
					string ourName = string.Empty;
					if(mob){
						ourName = mob.NAME;
					}
					if(pc){
						ourName = pc.CharacterName;
					}
        			Debug.Log(ourName + " is facing direction: " + directionString);	
					if(DirectionString != directionString){
						DirectionString = directionString;
						RpcSetDirectionFacing(directionString);
					}
					//bool newRightFace = movementDirection.x >= 0;
					//RpcUpdateFacingDirection(newRightFace);
					if(!MobCheck){
						TargetCastSpell(target, mode, mousePosition);
					} else {
						float castTimeBase = StatAsset.Instance.GetCastTime(spell, 1, _spellRank);
						
						MobCasting = StartCoroutine(MobCastSpell(target, spell, _spellRank, spellRange, castTimeBase, mode));	
	    	    
					}
					MoveToCast = null;
					yield break;
	    	    } else {
					timerCheck += Time.deltaTime;
					if(timerCheck > .5f){
						timerCheck = 0f;
						ServerMoveToTargetPosition(target.transform.position);
					}

				}
	    	    yield return new WaitForSeconds(checkTime);
	    	}
		} else {
			//treat like aoe
			Mob MobCheck = GetComponent<Mob>();
			string _spell = string.Empty;
			if(mode == CastingQ){
    		    _spell = SpellQ;
    		}
    		if(mode == CastingE){
    		    _spell = SpellE;
    		}
    		if(mode == CastingR){
    		    _spell = SpellR;
    		}
    		if(mode == CastingF){
    		    _spell = SpellF;
    		}
			var nameMatch = System.Text.RegularExpressions.Regex.Match(_spell, @"^\D*");
        	string spell = nameMatch.Value.Trim(); // Trim any trailing spaces
        	int _spellRank = 1;
        	// Extract spell rank
        	var rankMatch = System.Text.RegularExpressions.Regex.Match(_spell, @"\d+$");
        	if (rankMatch.Success) {
        	    _spellRank = int.Parse(rankMatch.Value); // Parse the rank number
        	}
			ServerMoveToTargetPosition(mousePosition);
			float checkTime = .1f;
			float timerCheck = 0;
	    	while (true)
	    	{
				if(Dying){
					yield break;
				}
	    	    // Compute the distance to the target
	    	    float distanceToTarget = Vector2.Distance(transform.position, mousePosition);
				Debug.Log($"Distance to Target: {distanceToTarget}, Spell Range: {spellRange}");
				
	    	    if (distanceToTarget <= spellRange && HasLineOfSight(transform.position, mousePosition))
	    	    {
	    	        // If it is, stop moving and start attacking
					if(agent.enabled)
					agent.isStopped = true;
					if(agent.enabled)
					agent.ResetPath();
					Vector3 movementDirection = new Vector3(mousePosition.x, mousePosition.y, transform.position.z) - transform.position;
					//Vector2 direction = new Vector2(targetPosition.x - transform.position.x, targetPosition.y - transform.position.y);
        			float angle = Mathf.Atan2(movementDirection.y, movementDirection.x) * Mathf.Rad2Deg;
        			// Determine the direction string based on the angle
        			string directionString = GetDirectionString(angle);
        			Mob mob = GetComponent<Mob>();
					PlayerCharacter pc = GetComponent<PlayerCharacter>();
					string ourName = string.Empty;
					if(mob){
						ourName = mob.NAME;
					}
					if(pc){
						ourName = pc.CharacterName;
					}
        			Debug.Log(ourName + " is facing direction: " + directionString);	
					if(DirectionString != directionString){
						DirectionString = directionString;
						RpcSetDirectionFacing(directionString);
					}
					//bool newRightFace = movementDirection.x >= 0;
					//RpcUpdateFacingDirection(newRightFace);
					if(!MobCheck){
						TargetCastSpell(target, mode, mousePosition);
					} else {
						float castTimeBase = StatAsset.Instance.GetCastTime(spell, 1, _spellRank);
						
						MobCasting = StartCoroutine(MobCastSpell(target, spell, _spellRank, spellRange, castTimeBase, mode));	
	    	    
					}
					MoveToCast = null;
					yield break;
	    	    } else {
					timerCheck += Time.deltaTime;
					if(timerCheck > .5f){
						timerCheck = 0f;
						ServerMoveToTargetPosition(mousePosition);
					}

				}
	    	    yield return new WaitForSeconds(checkTime);
	    	}
		}
	}
	
	[TargetRpc]
	void TargetCastSpell(MovingObject target, string mode, Vector2 mousePosition){
		//MovedToCast.Invoke(this, target, mode );
		//print($"Mode for auto cast TargetCastSpell is {mode}");
		ScenePlayer.localPlayer.AutoCastForCharacter(this, target, mode, mousePosition);
	}
	[ClientRpc]
	public void RpcAnimateDash(){
		AnimationAndSound("Dash", null, null);
	}
	public IEnumerator ChargingUnit(MovingObject target){
		PlayerCharacter pc = GetComponent<PlayerCharacter>();
		bool blockSpellTriggered = false;
		bool HasRiposted = false;
		int riposteLvl = 0;
		float riposteChance = 20f;
		int criticalStrikeMeleeLvl = 0;
		bool wasStealthed = false;
		float criticalStrikeMeleeChance = 0f;
		if(Hide){
			wasStealthed = true;
		}
		PlayerCharacter pcTargetCheck = target.GetComponent<PlayerCharacter>();
		if(pcTargetCheck){
			//We need to now set this owner in combat.
			curatorTM.CombatCalled();
			if(pcTargetCheck.ClassType == "Fighter"){
				for(int _char = 0; _char < pcTargetCheck.assignedPlayer.GetInformationSheets().Count; _char++){
            		if(pcTargetCheck.assignedPlayer.GetInformationSheets()[_char].CharacterID == pcTargetCheck.CharID){
            		    for(int ability = 0; ability < pcTargetCheck.assignedPlayer.GetInformationSheets()[_char].CharSpellData.Count; ability++){
							if(pcTargetCheck.assignedPlayer.GetInformationSheets()[_char].CharSpellData[ability].Key == "EastT3EndSkill"){
								HasRiposted = true;
								var abilityRankString = System.Text.RegularExpressions.Regex.Match(pcTargetCheck.assignedPlayer.GetInformationSheets()[_char].CharSpellData[ability].Value, @"\d+$");
                        		if (abilityRankString.Success) {
                        		    riposteLvl = int.Parse(abilityRankString.Value); // Parse the rank number
									break;
                        		}
							}
						}
						break;
					}
				}
			}
			if(riposteLvl > 0){
                //UtilityDescription.text = $"{(2f + (rankModifier * .1)).ToString("F2")} % chance riposte next attack with a regular attack.";
				riposteChance += (riposteLvl * .2f);
			}
		}
		if(pc){
			if(pc.ClassType == "Fighter"){
				for(int _char = 0; _char < pc.assignedPlayer.GetInformationSheets().Count; _char++){
					if(pc.assignedPlayer.GetInformationSheets()[_char].CharacterID == pc.CharID){
            		    for(int ability = 0; ability < pc.assignedPlayer.GetInformationSheets()[_char].CharSpellData.Count; ability++){
							if(pc.assignedPlayer.GetInformationSheets()[_char].CharSpellData[ability].Key == "WestT3BottomSkill"){
								var abilityRankString = System.Text.RegularExpressions.Regex.Match(pc.assignedPlayer.GetInformationSheets()[_char].CharSpellData[ability].Value, @"\d+$");
                        		if (abilityRankString.Success) {
                        		    criticalStrikeMeleeLvl = int.Parse(abilityRankString.Value); // Parse the rank number
                        		}
							}
						}
					}
				}
			}
			if(criticalStrikeMeleeLvl > 0){
				criticalStrikeMeleeChance += 15f;
				criticalStrikeMeleeChance += (criticalStrikeMeleeLvl * .2f);
			}
		}
		if(target != Target){
			Target = target;
		}
		ServerMoveToTargetPosition(target.transform.position);
		float checkTime = .1f;
	    while (true)
	    {
			if(Target == null || target.Dying){
				if(agent.enabled){
					agent.isStopped = true;
					agent.ResetPath();
				}
				agent.speed = startingSpeed;
				agent.acceleration = startingAcceleration;
				charging = false;
				if(GetATTACKING() != null)
        		{
        		    StopATTACKINGTOMOVE();
        		}
        		//RadiusLock = false;
				yield break;
			}
	        // Iterate through each unit
	        // Compute the distance to the target
	        float distanceToTarget = Vector2.Distance(transform.position, target.transform.position);
	        if (distanceToTarget <= attackRange)
	        {
				agent.speed = startingSpeed;
				agent.acceleration = startingAcceleration;
				charging = false;
	            // If it is, stop moving and start attacking
				if(agent.enabled)
				agent.isStopped = true;
				if(agent.enabled)
    			agent.ResetPath();
				if( target != null){
				if(!target.Dying){
					Vector3 movementDirection = target.transform.position - transform.position;
					//Vector2 direction = new Vector2(targetPosition.x - transform.position.x, targetPosition.y - transform.position.y);
        			float angle = Mathf.Atan2(movementDirection.y, movementDirection.x) * Mathf.Rad2Deg;
        			// Determine the direction string based on the angle
        			string directionString = GetDirectionString(angle);
        			Mob mob = GetComponent<Mob>();
					string ourName = string.Empty;
					if(mob){
						ourName = mob.NAME;
					}
					if(pc){
						ourName = pc.CharacterName;
					}
        			Debug.Log(ourName + " is facing direction: " + directionString);	
					if(DirectionString != directionString){
						DirectionString = directionString;
						RpcSetDirectionFacing(directionString);
					}
					//bool newRightFace = movementDirection.x >= 0;
					//if (newRightFace != rightFace)
					//{
					//	rightFace = newRightFace;
					//	RpcUpdateFacingDirection(newRightFace);
					//}
					bool dmgDealt = true;
					bool wasCrit = false;
					
					if(IsCriticalHit()){
						wasCrit = true;
					}
					
					if(target.mobType == "Undead"){
						if(IsCriticalHitUndead()){
							wasCrit = true;
						}
					}
					int value = StatAsset.Instance.GetAutoAttack(this, target, wasCrit);
					//lientSetTrigger("Attack");
					//RpcCastingSpell("chargeEnd", null);
					string ownerName = string.Empty;
        	string targetName = string.Empty;
        	if(pc){
        	   ownerName = pc.CharacterName;
        	} else {
				ownerName = StatAsset.Instance.GetMobName(StatAsset.Instance.GetEnemyToPrefabMapping(), mob.NAME);
        	}
        	if(target){
        	    PlayerCharacter pcChecktarget = target.GetComponent<PlayerCharacter>();
        	    if(pcChecktarget){
        	       targetName = pcChecktarget.CharacterName;
        	    } else {
        	        Mob mobCheckTarget = target.GetComponent<Mob>();
			    	targetName = StatAsset.Instance.GetMobName(StatAsset.Instance.GetEnemyToPrefabMapping(), mobCheckTarget.NAME);
        	    }
        	}
					AddRpcCall(null, null, false, true, "chargeEnd", weaponType, false, ownerName, targetName, this, target, Vector2.zero);

					//if(attackRange <= 3){
            		//	//StartCoroutine(BumpTowards(target.transform.position));
					//} else {
					//	curatorTM.PlayerSpawnRangedAttack(this, target, attackRange);
					//}
					bool missedTarget = false;
					(bool NPProc, float missCalc) = GetHitChance();
					if(!pc){
						missCalc += 5f;
					} else {
						//get char weapon skill
						//for now we will just use 95%
						missCalc += 5f;//sudo number for weapon
						//get skill from weapon, which will be the skill / level of character * 10, so if skill is 50 and you are level 6 the chance to hit would be
						// 90 + (5 * (50/(6 * 10))) == 94.16% hit chance
						//cap 100% hit chance
					}
					int randomNumber = UnityEngine.Random.Range(1, 101);
        			// Compare the random number to the threshold
        			if (randomNumber > missCalc)
        			{
						missedTarget = true;
						dmgDealt = false;
            	    	//target.RpcCastingSpell("Miss", null);
						
						target.AddRpcCall(null, null, false, true, "Miss", weaponType, false, ownerName, targetName, this, target, Vector2.zero);

        			}
        			else
        			{
        			    missedTarget = false;
        			}
					bool dodged = StatAsset.Instance.GetDodge(target);
            	    if(dodged && !missedTarget){
						dmgDealt = false;
            	        //target.RpcCastingSpell("Dodge", null);
						target.AddRpcCall(null, null, false, true, "Dodge", weaponType, false, ownerName, targetName, this, target, Vector2.zero);

            	    }
            	    bool parried = StatAsset.Instance.GetParry(target);
            	    if(parried && !dodged && !missedTarget){
						dmgDealt = false;
						bool Riposted = false;
							float roll = UnityEngine.Random.Range(1f, 101f);
							if(roll < riposteChance && HasRiposted){
								Riposted = true;
							}
							if(Riposted && HasRiposted)
							target.ServerRiposte(this);
						if(pcTargetCheck){
            	        	//target.RpcCastingSpell("Parry", weaponType);
						target.AddRpcCall(null, null, false, true, "Parry", target.weaponType, false, ownerName, targetName, this, target, Vector2.zero);

						} else {
            	        	//target.RpcCastingSpell("Parry", null);
						target.AddRpcCall(null, null, false, true, "Parry", null, false, ownerName, targetName, this, target, Vector2.zero);

						}
            	    }
					bool blockedAll = false;
            	    bool blocked = StatAsset.Instance.GetBlock(target);
					if(target.GetBlockSpell()){
						blockSpellTriggered = true;
						ServerTriggeredBlock();
						blocked = true;
					}
            	    if(!parried && !dodged && blocked && !missedTarget){
            	        value = value - target.shieldValue;
						if(blockSpellTriggered){
							value = 0;
						}
            	        if(value <= 0){
							blockedAll = true;
							dmgDealt = false;
            	            //target.RpcCastingSpell("Blocked", null);
						target.AddRpcCall(null, null, false, true, "Blocked", null, false, ownerName, targetName, this, target, Vector2.zero);

            	        }
            	    }
            	    if(value <= 0 && !parried && !dodged && !blockedAll && !missedTarget){
						dmgDealt = false;
            	        //target.RpcSpawnPopUpAbsorbed();
			target.AddRpcCall("absorbed", null, false, false, null, null, false, ownerName, targetName, this, target, Vector2.zero);

            	    }
					int threat = value;
                    if(BonusColdWeapon){
						DateTime currentTimeUtc = DateTime.UtcNow;
                        DateTime expirationTimeUtc = currentTimeUtc.AddSeconds(30);
                        target.ApplyStatChange("Agility", 30f, 30f, 20, "FrozenGreatsword", false, 0, false, false, expirationTimeUtc.ToString("o")); 
                        target.DamageDealt(this, BonusColdEffect, false, false, BonusColdEffect, COLDDAMAGECOLOR, false, null, "Ice");
						//string attacker = string.Empty;
                		//string defender = string.Empty;
                		//string content = "attacked";
                		//string amount = BonusColdEffect.ToString();
                		//PlayerCharacter pcAttackerCheck = GetComponent<PlayerCharacter>();
                		//Mob mobAttackerCheck = GetComponent<Mob>();
                		//PlayerCharacter pcDefenderCheck = target.GetComponent<PlayerCharacter>();
                		//Mob mobDefenderCheck = target.GetComponent<Mob>();
                		//if(pcAttackerCheck != null){
                		//    attacker = pcAttackerCheck.CharacterName;
                		//} else {
                		//    attacker = mobAttackerCheck.NAME;
                		//}
                		//if(pcDefenderCheck != null){
                		//    defender = pcDefenderCheck.CharacterName;
                		//} else {
                		//    defender = mobDefenderCheck.NAME;
                		//}
						//int type = 200; 
						//int element = 4;
                		//CombatLogNet cNet = new CombatLogNet(attacker, defender, content, amount, type, element, wasCrit);
                		//RpcSpawnCombatLog(cNet);
                    }
                    if(BonusFireWeapon){
                        target.DamageDealt(this, BonusFireEffect, false, false, BonusFireEffect, FIREDAMAGECOLOR, false, null, "Fire");
						//string attacker = string.Empty;
                		//string defender = string.Empty;
                		//string content = "attacked";
                		//string amount = BonusFireEffect.ToString();
                		//PlayerCharacter pcAttackerCheck = GetComponent<PlayerCharacter>();
                		//Mob mobAttackerCheck = GetComponent<Mob>();
                		//PlayerCharacter pcDefenderCheck = target.GetComponent<PlayerCharacter>();
                		//Mob mobDefenderCheck = target.GetComponent<Mob>();
                		//if(pcAttackerCheck != null){
                		//    attacker = pcAttackerCheck.CharacterName;
                		//} else {
                		//    attacker = mobAttackerCheck.NAME;
                		//}
                		//if(pcDefenderCheck != null){
                		//    defender = pcDefenderCheck.CharacterName;
                		//} else {
                		//    defender = mobDefenderCheck.NAME;
                		//}
						//int type = 201; 
						//int element = 3;
                		//CombatLogNet cNet = new CombatLogNet(attacker, defender, content, amount, type, element, wasCrit);
                		//RpcSpawnCombatLog(cNet);
						
                    }
                    if(BonusPoisonWeapon){
                        target.DamageDealt(this, BonusPoisonEffect, false, false, BonusPoisonEffect, POISONDAMAGECOLOR, false, null, "Poison");
						//string attacker = string.Empty;
                		//string defender = string.Empty;
                		//string content = "attacked";
                		//string amount = BonusPoisonEffect.ToString();
                		//PlayerCharacter pcAttackerCheck = GetComponent<PlayerCharacter>();
                		//Mob mobAttackerCheck = GetComponent<Mob>();
                		//PlayerCharacter pcDefenderCheck = target.GetComponent<PlayerCharacter>();
                		//Mob mobDefenderCheck = target.GetComponent<Mob>();
                		//if(pcAttackerCheck != null){
                		//    attacker = pcAttackerCheck.CharacterName;
                		//} else {
                		//    attacker = mobAttackerCheck.NAME;
                		//}
                		//if(pcDefenderCheck != null){
                		//    defender = pcDefenderCheck.CharacterName;
                		//} else {
                		//    defender = mobDefenderCheck.NAME;
                		//}
						//int type = 202; 
						//int element = 2;
                		//CombatLogNet cNet = new CombatLogNet(attacker, defender, content, amount, type, element, wasCrit);
                		//RpcSpawnCombatLog(cNet);
                    }
                    if(BonusDiseaseWeapon){
                        target.DamageDealt(this, BonusDiseaseEffect, false, false, BonusDiseaseEffect, DISEASEDAMAGECOLOR, false, null, "Disease");
						//string attacker = string.Empty;
                		//string defender = string.Empty;
                		//string content = "attacked";
                		//string amount = BonusDiseaseEffect.ToString();
                		//PlayerCharacter pcAttackerCheck = GetComponent<PlayerCharacter>();
                		//Mob mobAttackerCheck = GetComponent<Mob>();
                		//PlayerCharacter pcDefenderCheck = target.GetComponent<PlayerCharacter>();
                		//Mob mobDefenderCheck = target.GetComponent<Mob>();
                		//if(pcAttackerCheck != null){
                		//    attacker = pcAttackerCheck.CharacterName;
                		//} else {
                		//    attacker = mobAttackerCheck.NAME;
                		//}
                		//if(pcDefenderCheck != null){
                		//    defender = pcDefenderCheck.CharacterName;
                		//} else {
                		//    defender = mobDefenderCheck.NAME;
                		//}
						//int type = 203; 
						//int element = 5;
                		//CombatLogNet cNet = new CombatLogNet(attacker, defender, content, amount, type, element, wasCrit);
                		//RpcSpawnCombatLog(cNet);
                    }
                    if(BonusMagicWeapon){
                        target.DamageDealt(this, BonusMagicEffect, false, false, BonusMagicEffect, MAGICDAMAGECOLOR, false, null, "Magic");
						//string attacker = string.Empty;
                		//string defender = string.Empty;
                		//string content = "attacked";
                		//string amount = BonusMagicEffect.ToString();
                		//PlayerCharacter pcAttackerCheck = GetComponent<PlayerCharacter>();
                		//Mob mobAttackerCheck = GetComponent<Mob>();
                		//PlayerCharacter pcDefenderCheck = target.GetComponent<PlayerCharacter>();
                		//Mob mobDefenderCheck = target.GetComponent<Mob>();
                		//if(pcAttackerCheck != null){
                		//    attacker = pcAttackerCheck.CharacterName;
                		//} else {
                		//    attacker = mobAttackerCheck.NAME;
                		//}
                		//if(pcDefenderCheck != null){
                		//    defender = pcDefenderCheck.CharacterName;
                		//} else {
                		//    defender = mobDefenderCheck.NAME;
                		//}
						//int type = 204; 
						//int element = 5;
                		//CombatLogNet cNet = new CombatLogNet(attacker, defender, content, amount, type, element, wasCrit);
                		//RpcSpawnCombatLog(cNet);
                    }
					if(BonusLeechWeapon){
                	    target.DamageDealt(this, BonusLeechEffect, false, false, BonusLeechEffect, MAGICDAMAGECOLOR, false, null, "Leech");
                	    cur_hp = cur_hp + BonusLeechEffect;
						if(cur_hp > max_hp){
							cur_hp = max_hp;
						}
						
						//string attacker = string.Empty;
                		//string defender = string.Empty;
                		//string content = "attacked";
                		//string amount = BonusLeechEffect.ToString();
                		//PlayerCharacter pcAttackerCheck = GetComponent<PlayerCharacter>();
                		//Mob mobAttackerCheck = GetComponent<Mob>();
                		//PlayerCharacter pcDefenderCheck = target.GetComponent<PlayerCharacter>();
                		//Mob mobDefenderCheck = target.GetComponent<Mob>();
                		//if(pcAttackerCheck != null){
                		//    attacker = pcAttackerCheck.CharacterName;
                		//} else {
                		//    attacker = mobAttackerCheck.NAME;
                		//}
                		//if(pcDefenderCheck != null){
                		//    defender = pcDefenderCheck.CharacterName;
                		//} else {
                		//    defender = mobDefenderCheck.NAME;
                		//}
						//int type = 205; 
						//int element = 5;
                		//CombatLogNet cNet = new CombatLogNet(attacker, defender, content, amount, type, element, wasCrit);
                		//RpcSpawnCombatLog(cNet);
                	}
                    if(ThreatMod){
                        threat = (int)(threat * ThreatModifier);
                    }
            	    if(dmgDealt){
							string magicString = normalHithexColor;
						if(wasCrit){
							magicString = criticalHitHexColor;
						}
            	        target.DamageDealt(this, value, wasCrit, false, threat, magicString, true, weaponType, "Autoattack");
					//	string attacker = string.Empty;
                	//string defender = string.Empty;
                	//string content = "Charge";
                	//string amount = value.ToString();
                	//PlayerCharacter pcAttackerCheck = GetComponent<PlayerCharacter>();
                	//Mob mobAttackerCheck = GetComponent<Mob>();
                	//PlayerCharacter pcDefenderCheck = target.GetComponent<PlayerCharacter>();
                	//Mob mobDefenderCheck = target.GetComponent<Mob>();
                	//if(pcAttackerCheck != null){
                	//    attacker = pcAttackerCheck.CharacterName;
                	//} else {
                	//    attacker = mobAttackerCheck.NAME;
                	//}
                	//if(pcDefenderCheck != null){
                	//    defender = pcDefenderCheck.CharacterName;
                	//} else {
                	//    defender = mobDefenderCheck.NAME;
                	//}
                	//(int type, int element) = StatAsset.Instance.GetSpellType("Charge");
                	//CombatLogNet cNet = new CombatLogNet(attacker, defender, content, amount, type, element, wasCrit);
                	//target.RpcSpawnCombatLog(cNet);

            	    }
					if(duelWielding){
						bool wasCritOH = false;
					
						if(IsCriticalHit()){
							wasCritOH = true;
						}
						if(target.mobType == "Undead"){
							if(IsCriticalHitUndead()){
								wasCritOH = true;
							}
						}	
						int valueOH = 0;
						bool dmgDealtOH = true;
                        valueOH = StatAsset.Instance.GetAutoAttackOffhand(this, target, wasCritOH);
						bool missedTargetOH = false;
						(bool NPProcOH, float missCalcOH) = GetHitChance();
						if(!pc){
							missCalcOH += 5f;
						} else {
							//get char weapon skill
							//for now we will just use 95%
							missCalcOH += 5f;//sudo number for weapon
							//get skill from weapon, which will be the skill / level of character * 10, so if skill is 50 and you are level 6 the chance to hit would be
							// 90 + (5 * (50/(6 * 10))) == 94.16% hit chance
							//cap 100% hit chance
						}
						int randomNumberOH = UnityEngine.Random.Range(1, 101);
        				// Compare the random number to the threshold
        				if (randomNumberOH > missCalcOH)
        				{
        				    missedTargetOH = true;
							dmgDealtOH = false;
            	    	   // target.RpcCastingSpell("Miss", null);
						target.AddRpcCall(null, null, false, true, "Miss", null, false, ownerName, targetName, this, target, Vector2.zero);

        				}
        				else
        				{
        				    missedTargetOH = false;
        				}
						bool dodgedOH = StatAsset.Instance.GetDodge(target);
            	    	if(dodgedOH && !missedTargetOH){
							dmgDealtOH = false;
            	    	    //target.RpcCastingSpell("Dodge", null);
						target.AddRpcCall(null, null, false, true, "Dodge", null, false, ownerName, targetName, this, target, Vector2.zero);

            	    	}
            	    	bool parriedOH = StatAsset.Instance.GetParry(target);
            	    	if(parriedOH && !dodgedOH && !missedTargetOH){
							bool Riposted = false;
							float roll = UnityEngine.Random.Range(1f, 101f);
							if(roll < riposteChance && HasRiposted){
								Riposted = true;
							}
							if(Riposted && HasRiposted)
							target.ServerRiposte(this);
							dmgDealtOH = false;
							if(pcTargetCheck){
            	        		//target.RpcCastingSpell("Parry", weaponType);
						target.AddRpcCall(null, null, false, true, "Parry", target.weaponType, false, ownerName, targetName, this, target, Vector2.zero);

							} else {
            	        		//target.RpcCastingSpell("Parry", null);
						target.AddRpcCall(null, null, false, true, "Parry", null, false, ownerName, targetName, this, target, Vector2.zero);

							}
            	    	}
						bool blockedAllOH = false;
            	    	bool blockedOH = StatAsset.Instance.GetBlock(target);
						if(blockSpellTriggered){
							ServerTriggeredBlock();

							blockedOH = true;
						}
            	    	if(!parriedOH && !dodgedOH && blockedOH && !missedTargetOH){
            	    	    valueOH -= target.shieldValue;
							if(blockSpellTriggered){
								valueOH = 0;
							}
            	    	    if(valueOH <= 0){
								blockedAllOH = true;
								dmgDealtOH = false;
            	    	        //target.RpcCastingSpell("Blocked", null);
						target.AddRpcCall(null, null, false, true, "Blocked", null, false, ownerName, targetName, this, target, Vector2.zero);

            	    	    }
            	    	}
            	    	if(valueOH <= 0 && !parriedOH && !dodgedOH && !blockedAllOH && !missedTargetOH){
							dmgDealtOH = false;
			target.AddRpcCall("absorbed", null, false, false, null, null, false, ownerName, targetName, this, target, Vector2.zero);
            	    	    //target.RpcSpawnPopUpAbsorbed();
            	    	}
						if(wasStealthed){
							valueOH *= 2;
						}
                    	int threatOH = valueOH;
						if(ThreatMod){
							threatOH = (int)(threatOH * ThreatModifier);
                    	}
						if(dmgDealtOH){
							string magicString = normalHithexColor;
						if(wasCritOH){
							magicString = criticalHitHexColor;
						}
            	        	target.DamageDealt(this, valueOH, wasCritOH, false, threatOH, magicString, true, weaponTypeOH, "AutoattackOH");
							//string _attacker = string.Empty;
                			//string _defender = string.Empty;
                			//string _content = "attacked";
                			//string _amount = valueOH.ToString();
                			//PlayerCharacter _pcAttackerCheck = GetComponent<PlayerCharacter>();
                			//Mob _mobAttackerCheck = GetComponent<Mob>();
                			//PlayerCharacter _pcDefenderCheck = target.GetComponent<PlayerCharacter>();
                			//Mob _mobDefenderCheck = target.GetComponent<Mob>();
                			//if(_pcAttackerCheck != null){
                			//    _attacker = _pcAttackerCheck.CharacterName;
                			//} else {
                			//    _attacker = _mobAttackerCheck.NAME;
                			//}
                			//if(_pcDefenderCheck != null){
                			//    _defender = _pcDefenderCheck.CharacterName;
                			//} else {
                			//    _defender = _mobDefenderCheck.NAME;
                			//}
							//int _type = 105; 
							//int _element = 5;
                			//CombatLogNet _cNet = new CombatLogNet(_attacker, _defender, _content, _amount, _type, _element, wasCritOH);
                			//RpcSpawnCombatLog(_cNet);
							//StartCoroutine(DelayedWeaponAttackSound(wasCritOH));
						}
                    }
					
					//RpcAnimateChargeEnd(target);
					
					agent.radius = startingRadius;
					SetATTACKING(target, true);
		    	}
			}
				
				yield break;
	        }
	        yield return new WaitForSeconds(checkTime);
	    }
	}
	public IEnumerator AttackWithCharacter(MovingObject target)
	{
		if(target != Target){
			Target = target;
		}
		float checkTime = .5f;
	    while (true)
	    {
			if(target == null){
				Target = null;
				ATTACKING = null;
				yield break;
			}
			if(target.Dying){
				Target = null;
				ATTACKING = null;
				yield break;
			}
			if(Dying){
				ATTACKING = null;
				yield break;
			}
			checkTime = .25f;
			if(charging){
				checkTime = .1f;
			}
	        // Iterate through each unit
	        // Compute the distance to the target
	        float distanceToTarget = Vector2.Distance(transform.position, target.transform.position);
	        if (distanceToTarget <= attackRange && HasLineOfSight(transform.position, target.transform.position))
	        {
				ServerStopMoving();
					Vector3 movementDirection = target.transform.position - transform.position;
				//bool newRightFace = movementDirection.x >= 0;
				//RpcUpdateFacingDirection(newRightFace);
				if(stamina == -100f){
					if(Dying){
						ATTACKING = null;
						yield break;
					}
					PlayerCharacter pc = GetComponent<PlayerCharacter>();
					Vector2 direction = new Vector2(target.transform.position.x - transform.position.x, target.transform.position.y - transform.position.y);
        			float angle = Mathf.Atan2(movementDirection.y, movementDirection.x) * Mathf.Rad2Deg;
        			// Determine the direction string based on the angle
        			string directionString = GetDirectionString(angle);
        			Debug.Log(pc.CharacterName + " is facing direction: " + directionString);
					if(DirectionString != directionString){
						DirectionString = directionString;
						RpcSetDirectionFacing(directionString);
					}
	            	StartAttacking(target, true);
				}
	        }
	        else
	        {
				if(distanceToTarget > attackRange && !Snared || !Snared && !HasLineOfSight(transform.position, target.transform.position)){
					moving = true;
					if(agent.enabled){
						agent.isStopped = false;
					}
					Vector3 updateLocation = new Vector3(Mathf.FloorToInt(transform.position.x) + 0.5f, Mathf.FloorToInt(transform.position.y) + 0.5f, 0);
        			//NewFogUpdate(updateLocation); // Run NewFogUpdate
					Vector3 directionToTarget = (target.transform.position - transform.position).normalized;
    				// Find a position that is 0.6 units away from the target in the direction of the current transform
    				Vector3 destination = target.transform.position - directionToTarget * 0.6f;
					if(!HasLineOfSight(transform.position, target.transform.position)){
						destination = target.transform.position;
					}
    				// Move to the new destination instead of directly to the target
    				ServerMoveToTargetPosition(destination);
				}
	            // If it's not, make sure it's still moving towards the target
	        }
	        // Wait until next frame
	        yield return new WaitForSeconds(checkTime);
	    }
	}
	[Server]
	public void ServerMoveToCastMob(MovingObject target, string mode, float rangeToCast, Vector2 mousePosition){
		StopATTACKINGTOMOVE();
		StopCASTINGTOMOVE();
		if(Casting){
			Casting = false;
			RpcCancelMOCastMOB();
		}
		
		agent.ResetPath();
		//print($"{rangeToCast} is max range of this spell");
		MoveToCast = StartCoroutine(MovingToCast(target, mode, rangeToCast, mousePosition));
	}
	void ServerMobCastingDpsSpell(Match match, MovingObject target, string spell, int spellRank, int cost){
		curatorTM.UpdatePlayerCastedOffensiveSpellSingleTargetDPS(match, this, target, spell, spellRank, cost);
	}
	public IEnumerator AttackWithMob(MovingObject target, bool directedByPlayer)
	{
		Mob mob = GetComponent<Mob>();
		if(target != Target){
			Target = target;
		}
		float checkTime = .5f;
		int randomCheckAmount = (int)UnityEngine.Random.Range(8f, 15f);
	    while (true)
	    {
			if(target == null){
				ATTACKING = null;
				yield break;
			}
			if(Dying){
				ATTACKING = null;
				yield break;
			}
			if(mob){
	        	//float distanceFromOrigin = Vector2.Distance(Origin, transform.position);
				//if(distanceFromOrigin >= 10f){
				//	//reset this object and its group
				//	//figure out group
				//	mob.ResettingMob();
				//	yield break;
				//}
				//
				if(!directedByPlayer){
					MovingObject threatHighestTarget = mob.GetHighestThreat();
					if(threatHighestTarget){
						Target = threatHighestTarget;
						target = Target;
					} else {
						mob.ResettingMob();
					}
				}
				
			}
			checkTime = .25f;
			if(charging){
				checkTime = .1f;
			}
			bool failedToTarget = false;
				if(mob){
					if(Target == null || mob.FROZEN){
						failedToTarget = true;
					} else {
						if(Target.FROZEN){
							failedToTarget = true;
						}
						if(Target.Dying){
							failedToTarget = true;
						}
        				if(Target.Hide && !mob.SneakTrueSight ){
							failedToTarget = true;
						}
						if(Target.Sneak && !mob.SneakTrueSight){
							failedToTarget = true;
						}
						if(Target.Invisibility && !mob.InvisTrueSight){
							failedToTarget = true;
						}
						if(Target.InvisibilityUndead && !mob.InvisUndeadTrueSight){
							failedToTarget = true;
						}
					}
				}
				
			if(Target == null || failedToTarget){
				//if(agent.enabled){
		 		//	agent.isStopped = true;
    			//	agent.ResetPath();
				//}
				if(mob){
					if(mob.threatList.ContainsKey(target)){
						mob.threatList.Remove(target);
					}
					mob.TransitionToState(new Mob.OnGuardState(), "OnGuardState", mob.curatorTM); 
				}
				ATTACKING = null;
				yield break;
			}
	        // Iterate through each unit
	        // Compute the distance to the target
			if(stamina == -100f && HasLineOfSight(transform.position, target.transform.position) && mob.CanCast() && cur_mp > 0){
				if(mob.DecideToCast() && cur_mp > 0){
					//print($"Mob Has decided to cast");
					List<string> possibleSpellsToCast = new List<string>();
					if(!SpellQCoolDown){
						if(SpellQ != "None"){
							var nameMatch = System.Text.RegularExpressions.Regex.Match(SpellQ, @"^\D*");
        					string spell = nameMatch.Value.Trim(); // Trim any trailing spaces
							int cost = StatAsset.Instance.GetSpellCost(spell);
							if(cost <= cur_mp){
								possibleSpellsToCast.Add("Q");
							}
						}
					}
					if(!SpellECoolDown){
						if(SpellE != "None"){
							var nameMatch = System.Text.RegularExpressions.Regex.Match(SpellE, @"^\D*");
        					string spell = nameMatch.Value.Trim(); // Trim any trailing spaces
							int cost = StatAsset.Instance.GetSpellCost(spell);
							if(cost <= cur_mp){
								possibleSpellsToCast.Add("E");
							}
						}
					}
					if(!SpellRCoolDown){
						if(SpellR != "None"){
							var nameMatch = System.Text.RegularExpressions.Regex.Match(SpellR, @"^\D*");
        					string spell = nameMatch.Value.Trim(); // Trim any trailing spaces
							int cost = StatAsset.Instance.GetSpellCost(spell);
							if(cost <= cur_mp){
								possibleSpellsToCast.Add("R");
							}
						}
					}
					if(!SpellFCoolDown){
						if(SpellF != "None"){
							var nameMatch = System.Text.RegularExpressions.Regex.Match(SpellF, @"^\D*");
        					string spell = nameMatch.Value.Trim(); // Trim any trailing spaces
							int cost = StatAsset.Instance.GetSpellCost(spell);
							if(cost <= cur_mp){
								possibleSpellsToCast.Add("F");
							}
						}
					}
					
					if(possibleSpellsToCast.Count > 0){
						string OurSpellChoice = string.Empty;
						System.Random random = new System.Random();
						int randomIndex = random.Next(possibleSpellsToCast.Count);
						OurSpellChoice = possibleSpellsToCast[randomIndex];
						string mode = CastingQ;
						if(OurSpellChoice == "Q"){
							OurSpellChoice = SpellQ;
							mode = CastingQ;
						}
						if(OurSpellChoice == "E"){
							OurSpellChoice = SpellE;
							mode = CastingE;
						}
						if(OurSpellChoice == "R"){
							OurSpellChoice = SpellR;
							mode = CastingR;
						}
						if(OurSpellChoice == "F"){
							OurSpellChoice = SpellF;
							mode = CastingF;
						}
						float spellRange = StatAsset.Instance.SpellRange(this, mode);
						var nameMatch = System.Text.RegularExpressions.Regex.Match(OurSpellChoice, @"^\D*");
        				string spell = nameMatch.Value.Trim(); // Trim any trailing spaces
        				int _spellRank = 1;
        				// Extract spell rank
        				var rankMatch = System.Text.RegularExpressions.Regex.Match(OurSpellChoice, @"\d+$");
        				if (rankMatch.Success) {
        				    _spellRank = int.Parse(rankMatch.Value); // Parse the rank number
        				}
						print($"Mob Has decided to cast and it choose {OurSpellChoice}");
						bool hostile = !StatAsset.Instance.DetermineFriendly(spell);

						float castTimeBase = StatAsset.Instance.GetCastTime(spell, 1, _spellRank);
						float selfCastFloat = StatAsset.Instance.SelfCasted(spell);
						bool heal = false;
						if(spell == "Heal" || spell == "Greater Heal"){
							heal = true;
						}
						bool selfCasted = false;
						if(selfCastFloat > 0f){
							selfCasted = true;
						}
						if(!selfCasted){
							if(!hostile){
								if(!GetFriendly(target)){
									//find new target
									target = GetFreshTarget(hostile, heal);
									if(target == null){
										if(mob){
											mob.DamageTaken();
										}
										yield break;
									}
								}
							} else {
								if(GetFriendly(target)){
									//find new target
									target = mob.GetHighestThreat();
									if(target == null){
										if(mob){
											mob.DamageTaken();
										}
										yield break;
									}
								}
							}
						} else {
							target = this;
						}
						float distanceToTarget = Vector2.Distance(transform.position, target.transform.position);
						Vector2 mousePosition = target.transform.position;
						if(spellRange > distanceToTarget){
							ServerMoveToCastMob(target, mode, spellRange, mousePosition);
							ATTACKING = null;
							yield break;
						} else {
							MobCasting = StartCoroutine(MobCastSpell(target, spell, _spellRank, spellRange, castTimeBase, mode));	
							ATTACKING = null;
							yield break;
						}

					}
				} else {
					float distanceToTarget = Vector2.Distance(transform.position, target.transform.position);
	        		if (distanceToTarget <= attackRange)
	        		{
						ServerStopMoving();
						Vector3 movementDirection = target.transform.position - transform.position;
						//Vector2 direction = new Vector2(targetPosition.x - transform.position.x, targetPosition.y - transform.position.y);
        				
						//bool newRightFace = movementDirection.x >= 0;
						//RpcUpdateFacingDirection(newRightFace);
						if(stamina == -100f && HasLineOfSight(transform.position, target.transform.position)){
							float angle = Mathf.Atan2(movementDirection.y, movementDirection.x) * Mathf.Rad2Deg;
        					// Determine the direction string based on the angle
        					string directionString = GetDirectionString(angle);
        					Debug.Log(mob.NAME + " is facing direction: " + directionString);
							if(DirectionString != directionString){
								DirectionString = directionString;
								RpcSetDirectionFacing(directionString);
							}
	        		    	StartAttacking(target, true);
						} 
	        		} else {
						if(distanceToTarget > attackRange && !Snared){
							moving = true;
							if(agent.enabled){
								agent.isStopped = false;
							}
							Vector3 updateLocation = new Vector3(Mathf.FloorToInt(transform.position.x) + 0.5f, Mathf.FloorToInt(transform.position.y) + 0.5f, 0);
        					//NewFogUpdate(updateLocation); // Run NewFogUpdate
							Vector3 directionToTarget = (target.transform.position - transform.position).normalized;
    						// Find a position that is 0.6 units away from the target in the direction of the current transform
    						Vector3 destination = target.transform.position - directionToTarget * 0.6f;
    						// Move to the new destination instead of directly to the target
    						ServerMoveToTargetPosition(destination);
						}
	        		    // If it's not, make sure it's still moving towards the target
	        		}
				}
	        	//StartAttacking(target);
			} else {
				float distanceToTarget = Vector2.Distance(transform.position, target.transform.position);
				
	        	if (distanceToTarget <= attackRange && HasLineOfSight(transform.position, target.transform.position))
	        	{
					ServerStopMoving();
					//Vector2 direction = new Vector2(targetPosition.x - transform.position.x, targetPosition.y - transform.position.y);
		
        			// Determine the direction string based on the angle
        			
					if(stamina == -100f && HasLineOfSight(transform.position, target.transform.position)){
						Vector3 movementDirection = target.transform.position - transform.position;
        				float angle = Mathf.Atan2(movementDirection.y, movementDirection.x) * Mathf.Rad2Deg;
						string directionString = GetDirectionString(angle);
        				Debug.Log(mob.NAME + " is facing direction: " + directionString);
						if(DirectionString != directionString){
							DirectionString = directionString;
							RpcSetDirectionFacing(directionString);
						}
						//bool newRightFace = movementDirection.x >= 0;
						//RpcUpdateFacingDirection(newRightFace);
	        	    	StartAttacking(target, true);
					} else {
						//make sure we arent stacked on top if mob
						//if(mob){
						//	if(checkCount == 0){
						//		checkCount ++;
						//		Vector3 newPosition = CheckForStacking();//(target, attackRange);
						//		if (newPosition != transform.position) // Assuming FindNewPosition returns Vector3.zero if no suitable position is found
        				//		{
    					//			ServerMoveToTargetPosition(false, newPosition);
						//			Vector2 direction = new Vector2(target.transform.position.x - transform.position.x, target.transform.position.y - transform.position.y);
        				//			float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        				//			// Determine the direction string based on the angle
        				//			string directionString = GetDirectionString(angle);
						//			string ourName = string.Empty;
						//			if(mob){
						//				ourName = mob.NAME;
						//			}
        				//			Debug.Log(ourName + " is facing direction: " + directionString);
						//			if(DirectionString != directionString){
						//				DirectionString = directionString;
						//				RpcSetDirectionFacing(directionString);
						//			}
						//		}
						//	}
						//	if(checkCount >= 1){
						//		checkCount ++;
						//		if(checkCount > randomCheckAmount){
						//			checkCount = 0;
						//		}
						//	}
						//}
					}
	        	}
	        	else
	        	{
					if(distanceToTarget > attackRange && !Snared || !Snared && !HasLineOfSight(transform.position, target.transform.position)){
						moving = true;
						if(agent.enabled){
							agent.isStopped = false;
						}
						Vector3 updateLocation = new Vector3(Mathf.FloorToInt(transform.position.x) + 0.5f, Mathf.FloorToInt(transform.position.y) + 0.5f, 0);
        				//NewFogUpdate(updateLocation); // Run NewFogUpdate
						Vector3 directionToTarget = (target.transform.position - transform.position).normalized;
    					// Find a position that is 0.6 units away from the target in the direction of the current transform
    					Vector3 destination = target.transform.position - directionToTarget * 0.6f;
						if(!HasLineOfSight(transform.position, target.transform.position)){
							destination = target.transform.position;
						}
    					// Move to the new destination instead of directly to the target
    					ServerMoveToTargetPosition(destination);
					}
	        	    // If it's not, make sure it's still moving towards the target
	        	}
			}
	        
	        // Wait until next frame
	        yield return new WaitForSeconds(checkTime);
	    }
	}
	// Method to find a new position around the target within the attack range
	Vector3 FindNewPosition(Vector3 targetPosition, List<Vector3> DONOTUSELIST, float range){
		if(!DONOTUSELIST.Contains(targetPosition)){
		    DONOTUSELIST.Add(targetPosition);
		}
	    const int attempts = 30; // Number of attempts to find a suitable position
	    agent = GetComponent<NavMeshAgent>();
	    for (int i = 0; i < attempts; i++)
	    {
	        Vector3 randomDirection = UnityEngine.Random.insideUnitSphere * range;
	        randomDirection += targetPosition;
	        NavMeshHit hit;
	        if (NavMesh.SamplePosition(randomDirection, out hit, range, agent.areaMask)) // Use agent.areaMask to match the agent's NavMesh area
	        {
	            // Check if the position is not too close to other mobs
            	bool tooClose = DONOTUSELIST.Any(donotUsePosition => 
            	    Vector3.Distance(hit.position, donotUsePosition) <= (donotUsePosition == targetPosition ? 0.9f : 0.8f));
            	if (!tooClose)
            	{
            	    // Found a suitable position that's not too close to DONOTUSELIST positions
            	    return hit.position;
            	}
	        }
	    }
	    return targetPosition; // No suitable position found
	}
/*
	[Server]
public Vector3 CheckForStacking() {
    List<Mob> matchMobListCheck = curatorTM.GetENEMYList();
    List<PlayerCharacter> matchPlayerCharacterListCheck = curatorTM.GetPCList();
    List<Vector3> DONOTUSELIST = new List<Vector3>();
    List<MovingObject> allObjectsListCheck = new List<MovingObject>();

    // Combine both lists into one
    //allObjectsListCheck.AddRange(matchMobListCheck);
    //allObjectsListCheck.AddRange(matchPlayerCharacterListCheck);
	foreach (var potentialStackingMember in matchMobListCheck) {
        if (potentialStackingMember == null || potentialStackingMember.Dying || potentialStackingMember == this) {
            continue; // Skip null, dying objects, and the current object itself
        }

        float distance = Vector2.Distance(new Vector2(transform.position.x, transform.position.y), 
            new Vector2(potentialStackingMember.transform.position.x, potentialStackingMember.transform.position.y));
        if (distance <= .55f) {
            DONOTUSELIST.Add(potentialStackingMember.transform.position);
        }
    }
    // Check the distance between the current object and all other objects
    foreach (var potentialStackingMember in matchPlayerCharacterListCheck) {
        if (potentialStackingMember == null || potentialStackingMember.Dying) {
            continue; // Skip null, dying objects, and the current object itself
        }

        float distance = Vector2.Distance(new Vector2(transform.position.x, transform.position.y), 
            new Vector2(potentialStackingMember.transform.position.x, potentialStackingMember.transform.position.y));
        if (distance <= .55f) {
            DONOTUSELIST.Add(potentialStackingMember.transform.position);
        }
    }

    Vector3 newPosition = transform.position;
    // If there are too many objects near the target, find a new position
    if (DONOTUSELIST.Count > 0) {
        newPosition = FindNewPosition(transform.position, DONOTUSELIST, 1f);
    }

    return newPosition;
}
[Server]
public Vector3 CheckForStackingMove(Vector3 target) {
    List<Mob> matchMobListCheck = curatorTM.GetENEMYList();
    List<PlayerCharacter> matchPlayerCharacterListCheck = curatorTM.GetPCList();
    List<Vector3> DONOTUSELIST = new List<Vector3>();
    List<MovingObject> allObjectsListCheck = new List<MovingObject>();

    // Combine both lists into one
    allObjectsListCheck.AddRange(matchMobListCheck);
    allObjectsListCheck.AddRange(matchPlayerCharacterListCheck);

    // Check the distance between the current object and all other objects
    foreach (var potentialStackingMember in allObjectsListCheck) {
        if (potentialStackingMember == null || potentialStackingMember.Dying || potentialStackingMember == this) {
            continue; // Skip null, dying objects, and the current object itself
        }

        float distance = Vector2.Distance(new Vector2(target.x, target.y), 
            new Vector2(potentialStackingMember.transform.position.x, potentialStackingMember.transform.position.y));
        if (distance <= .25f) {
            DONOTUSELIST.Add(potentialStackingMember.transform.position);
        }
    }
    Vector3 newPosition = transform.position;
    // If there are too many objects near the target, find a new position
    if (DONOTUSELIST.Count > 0) {
        newPosition = FindNewPosition(target, DONOTUSELIST, .4f);
    }

    return newPosition;
}
*/

	public bool IsCriticalHit()
    {
        // Generate a random number between 0 and 100
        float randomNumber = UnityEngine.Random.Range(0f, 100f);

        // Compare with criticalValue
        return randomNumber <= criticalStrikeMeleeChance;
	}
	public bool IsCriticalHitUndead()
    {
        // Generate a random number between 0 and 100
        float randomNumber = UnityEngine.Random.Range(0f, 100f);

        // Compare with criticalValue
        return randomNumber <= criticalStrikeMeleeUndeadChance;
	}
	public void ProcAttack(MovingObject target, bool chargeStam){
		StartAttacking(target, false);
	}
	[Server]
	void StartAttacking(MovingObject target, bool chargeStam){
		bool blockSpellTriggered = false;
		if(target == null)
		return;
		if(target.Dying){
			return;
		}
		Mob mob = GetComponent<Mob>();
		if(mob){
			criticalStrikeMeleeChance = 0f;
			doubleAttackChance = 0f;
			riposteChance = 0f;
		}
		PlayerCharacter pc = GetComponent<PlayerCharacter>();
		string ownerName = string.Empty;
        string targetName = string.Empty;
        if(pc){
           ownerName = pc.CharacterName;
        } else {
			ownerName = StatAsset.Instance.GetMobName(StatAsset.Instance.GetEnemyToPrefabMapping(), mob.NAME);
        }
        if(target){
            PlayerCharacter pcChecktarget = target.GetComponent<PlayerCharacter>();
            if(pcChecktarget){
               targetName = pcChecktarget.CharacterName;
            } else {
                Mob mobCheckTarget = target.GetComponent<Mob>();
		    	targetName = StatAsset.Instance.GetMobName(StatAsset.Instance.GetEnemyToPrefabMapping(), mobCheckTarget.NAME);
            }
        }
		//Vector3 updateLocation = new Vector3(Mathf.FloorToInt(transform.position.x) + 0.5f, Mathf.FloorToInt(transform.position.y) + 0.5f, 0);
        //NewFogUpdate(updateLocation); // Run NewFogUpdate
		PlayerCharacter pcTargetCheck = target.GetComponent<PlayerCharacter>();
		bool isBehind = false;
		if(IsBehindAnotherObject(target.transform.position, target.GetFacingDirection())){
			isBehind = true;
			string attackerName = string.Empty;
			if(mob){
				attackerName = mob.NAME;
			} else {
				attackerName = pc.CharacterName;
			}
			print($"{attackerName} was behind our target");
		}
		if(pcTargetCheck){
			//We need to now set this owner in combat.
			curatorTM.CombatCalled();
		}
		if(pc){
			curatorTM.CombatCalled();
		}
		if( target != null){
				if(target.FROZEN){
					return;
				}
				if(!target.Dying){
					Vector3 movementDirection = target.transform.position - transform.position;
					//Vector2 direction = new Vector2(targetPosition.x - transform.position.x, targetPosition.y - transform.position.y);
        			float angle = Mathf.Atan2(movementDirection.y, movementDirection.x) * Mathf.Rad2Deg;
        			// Determine the direction string based on the angle
        			string directionString = GetDirectionString(angle);
					string ourName = string.Empty;
					if(mob){
						ourName = mob.NAME;
					}
					if(pc){
						ourName = pc.CharacterName;
					}
        			Debug.Log(ourName + " is facing direction: " + directionString);	
					if(DirectionString != directionString){
						DirectionString = directionString;
						RpcSetDirectionFacing(directionString);
					}
					//bool newRightFace = movementDirection.x >= 0;
					//RpcUpdateFacingDirection(newRightFace);

					//if (newRightFace != rightFace)
					//{
					//	rightFace = newRightFace;
					//	RpcUpdateFacingDirection(newRightFace);
					//}
					bool dmgDealt = true;
					bool wasCrit = false;
					if(IsCriticalHit()){
						wasCrit = true;
					}
					if(target.mobType == "Undead"){
						if(IsCriticalHitUndead()){
							wasCrit = true;
						}
					}
					bool wasStealthed = false;
					if(Hide){
						wasStealthed = true;
					}
					int value = StatAsset.Instance.GetAutoAttack(this, target, wasCrit);
					//ClientAttackSound(weaponType, wasCrit);
					if(attackRange <= 3){
						if(mob){
	            			StartCoroutine(BumpTowards(target.transform.position));
						} 
						bool Riposted = false;
						float roll = UnityEngine.Random.Range(1f, 101f);
						if(roll < target.riposteChance){
							Riposted = true;
						}
						if(Riposted)
						target.ServerRiposte(this);
					} else {
						curatorTM.PlayerSpawnRangedAttack(this, target, attackRange, wasCrit);
					}

					bool missedTarget = false;
					(bool NPProc, float missCalc) = GetHitChance();
					float valueIncrease = ((NPRank - 1) * 1f);
					int roundedValue = (int)Math.Round(value + valueIncrease);
					if(NPProc){
						value = roundedValue;
					}
					if(!pc){
						missCalc += 5f;
					} else {
						//get char weapon skill
						//for now we will just use 95%
						missCalc += 5f;//sudo number for weapon
						//get skill from weapon, which will be the skill / level of character * 10, so if skill is 50 and you are level 6 the chance to hit would be
						// 90 + (5 * (50/(6 * 10))) == 94.16% hit chance
						//cap 100% hit chance
					}
					int randomNumber = UnityEngine.Random.Range(1, 101);
					if(target.Stunned || target.Mesmerized || target.Snared){
						randomNumber = 0;
					}
        			// Compare the random number to the threshold
        			if (randomNumber > missCalc)
        			{
						
        			    missedTarget = true;
						dmgDealt = false;
            	    	//target.RpcCastingSpell("Miss", null);
						target.AddRpcCall(null, null, false, true, "Miss", null, false, ownerName, targetName, this, target, Vector2.zero);

						//string attacker = string.Empty;
                		//string defender = string.Empty;
                		//string content = "attacked";
                		//string amount = value.ToString();
                		//PlayerCharacter pcAttackerCheck = GetComponent<PlayerCharacter>();
                		//Mob mobAttackerCheck = GetComponent<Mob>();
                		//PlayerCharacter pcDefenderCheck = target.GetComponent<PlayerCharacter>();
                		//Mob mobDefenderCheck = target.GetComponent<Mob>();
                		//if(pcAttackerCheck != null){
                		//    attacker = pcAttackerCheck.CharacterName;
                		//} else {
                		//    attacker = mobAttackerCheck.NAME;
                		//}
                		//if(pcDefenderCheck != null){
                		//    defender = pcDefenderCheck.CharacterName;
                		//} else {
                		//    defender = mobDefenderCheck.NAME;
                		//}
						//int type = 110; 
						//int element = 5;
                		//CombatLogNet cNet = new CombatLogNet(attacker, defender, content, amount, type, element, wasCrit);
                		//RpcSpawnCombatLog(cNet);
        			}
					bool dodged = StatAsset.Instance.GetDodge(target);
            	    if(dodged && !missedTarget && !isBehind){
						dmgDealt = false;
            	       // target.RpcCastingSpell("Dodge", null);
						target.AddRpcCall(null, null, false, true, "Dodge", null, false, ownerName, targetName, this, target, Vector2.zero);
						//string attacker = string.Empty;
                		//string defender = string.Empty;
                		//string content = "attacked";
                		//string amount = value.ToString();
                		//PlayerCharacter pcAttackerCheck = GetComponent<PlayerCharacter>();
                		//Mob mobAttackerCheck = GetComponent<Mob>();
                		//PlayerCharacter pcDefenderCheck = target.GetComponent<PlayerCharacter>();
                		//Mob mobDefenderCheck = target.GetComponent<Mob>();
                		//if(pcAttackerCheck != null){
                		//    attacker = pcAttackerCheck.CharacterName;
                		//} else {
                		//    attacker = mobAttackerCheck.NAME;
                		//}
                		//if(pcDefenderCheck != null){
                		//    defender = pcDefenderCheck.CharacterName;
                		//} else {
                		//    defender = mobDefenderCheck.NAME;
                		//}
						//int type = 111; 
						//int element = 5;
                		//CombatLogNet cNet = new CombatLogNet(attacker, defender, content, amount, type, element, wasCrit);
                		//RpcSpawnCombatLog(cNet);
            	    }
            	    bool parried = StatAsset.Instance.GetParry(target);
            	    if(parried && !dodged && !missedTarget && !isBehind){
						dmgDealt = false;
						
						if(pcTargetCheck){
						target.AddRpcCall(null, null, false, true, "Parry", target.weaponType, false, ownerName, targetName, this, target, Vector2.zero);
            	        	
							//target.RpcCastingSpell("Parry", weaponType);
							
							//string attacker = string.Empty;
                			//string defender = string.Empty;
                			//string content = "attacked";
                			//string amount = value.ToString();
                			//PlayerCharacter pcAttackerCheck = GetComponent<PlayerCharacter>();
                			//Mob mobAttackerCheck = GetComponent<Mob>();
                			//PlayerCharacter pcDefenderCheck = target.GetComponent<PlayerCharacter>();
                			//Mob mobDefenderCheck = target.GetComponent<Mob>();
                			//if(pcAttackerCheck != null){
                			//    attacker = pcAttackerCheck.CharacterName;
                			//} else {
                			//    attacker = mobAttackerCheck.NAME;
                			//}
                			//if(pcDefenderCheck != null){
                			//    defender = pcDefenderCheck.CharacterName;
                			//} else {
                			//    defender = mobDefenderCheck.NAME;
                			//}
							//int type = 112; 
							//int element = 5;
                			//CombatLogNet cNet = new CombatLogNet(attacker, defender, content, amount, type, element, wasCrit);
                			//RpcSpawnCombatLog(cNet);
						} else {
						target.AddRpcCall(null, null, false, true, "Parry", null, false, ownerName, targetName, this, target, Vector2.zero);
            	        	
							//target.RpcCastingSpell("Parry", null);
							//string attacker = string.Empty;
                			//string defender = string.Empty;
                			//string content = "attacked";
                			//string amount = value.ToString();
                			//PlayerCharacter pcAttackerCheck = GetComponent<PlayerCharacter>();
                			//Mob mobAttackerCheck = GetComponent<Mob>();
                			//PlayerCharacter pcDefenderCheck = target.GetComponent<PlayerCharacter>();
                			//Mob mobDefenderCheck = target.GetComponent<Mob>();
                			//if(pcAttackerCheck != null){
                			//    attacker = pcAttackerCheck.CharacterName;
                			//} else {
                			//    attacker = mobAttackerCheck.NAME;
                			//}
                			//if(pcDefenderCheck != null){
                			//    defender = pcDefenderCheck.CharacterName;
                			//} else {
                			//    defender = mobDefenderCheck.NAME;
                			//}
							//int type = 112; 
							//int element = 5;
                			//CombatLogNet cNet = new CombatLogNet(attacker, defender, content, amount, type, element, wasCrit);
                			//RpcSpawnCombatLog(cNet);
						}
            	    }
					bool blockedAll = false;
            	    bool blocked = StatAsset.Instance.GetBlock(target);
					if(target.GetBlockSpell() && !isBehind){
						ServerTriggeredBlock();
						blocked = true;
						blockSpellTriggered = true;
					}
            	    if(!parried && !dodged && blocked && !missedTarget && !isBehind){
            	        value = value - target.shieldValue;
						if(blockSpellTriggered){
							value = 0;
						}
            	        if(value <= 0){
							blockedAll = true;
							dmgDealt = false;
						target.AddRpcCall(null, null, false, true, "Blocked", null, false, ownerName, targetName, this, target, Vector2.zero);
            	            
							//target.RpcCastingSpell("Blocked", null);
						//	string attacker = string.Empty;
                		//string defender = string.Empty;
                		//string content = "attacked";
                		//string amount = value.ToString();
                		//PlayerCharacter pcAttackerCheck = GetComponent<PlayerCharacter>();
                		//Mob mobAttackerCheck = GetComponent<Mob>();
                		//PlayerCharacter pcDefenderCheck = target.GetComponent<PlayerCharacter>();
                		//Mob mobDefenderCheck = target.GetComponent<Mob>();
                		//if(pcAttackerCheck != null){
                		//    attacker = pcAttackerCheck.CharacterName;
                		//} else {
                		//    attacker = mobAttackerCheck.NAME;
                		//}
                		//if(pcDefenderCheck != null){
                		//    defender = pcDefenderCheck.CharacterName;
                		//} else {
                		//    defender = mobDefenderCheck.NAME;
                		//}
						//int type = 113; 
						//int element = 5;
                		//CombatLogNet cNet = new CombatLogNet(attacker, defender, content, amount, type, element, wasCrit);
                		//RpcSpawnCombatLog(cNet);
            	        }
            	    }
            	    if(value <= 0 && !parried && !dodged && !blockedAll && !missedTarget){
						dmgDealt = false;
            	        //target.RpcSpawnPopUpAbsorbed();
			target.AddRpcCall("absorbed", null, false, false, null, null, false, ownerName, targetName, this, target, Vector2.zero);

            	    }
					int threat = value;
                    if(BonusColdWeapon){
						DateTime currentTimeUtc = DateTime.UtcNow;
                        DateTime expirationTimeUtc = currentTimeUtc.AddSeconds(30);
                        target.ApplyStatChange("Agility", 30f, 30f, 20, "FrozenGreatsword", false, 0, false, false, expirationTimeUtc.ToString("o"));
                        target.DamageDealt(this, BonusColdEffect, false, false, BonusColdEffect, COLDDAMAGECOLOR, false, null, "Ice");
						//string attacker = string.Empty;
                		//string defender = string.Empty;
                		//string content = "attacked";
                		//string amount = BonusColdEffect.ToString();
                		//PlayerCharacter pcAttackerCheck = GetComponent<PlayerCharacter>();
                		//Mob mobAttackerCheck = GetComponent<Mob>();
                		//PlayerCharacter pcDefenderCheck = target.GetComponent<PlayerCharacter>();
                		//Mob mobDefenderCheck = target.GetComponent<Mob>();
                		//if(pcAttackerCheck != null){
                		//    attacker = pcAttackerCheck.CharacterName;
                		//} else {
                		//    attacker = mobAttackerCheck.NAME;
                		//}
                		//if(pcDefenderCheck != null){
                		//    defender = pcDefenderCheck.CharacterName;
                		//} else {
                		//    defender = mobDefenderCheck.NAME;
                		//}
						//int type = 200; 
						//int element = 5;
                		//CombatLogNet cNet = new CombatLogNet(attacker, defender, content, amount, type, element, wasCrit);
                		//RpcSpawnCombatLog(cNet);
                    }
                    if(BonusFireWeapon){
                        target.DamageDealt(this, BonusFireEffect, false, false, BonusFireEffect, FIREDAMAGECOLOR, false, null, "Fire");
						//string attacker = string.Empty;
                		//string defender = string.Empty;
                		//string content = "attacked";
                		//string amount = BonusFireEffect.ToString();
                		//PlayerCharacter pcAttackerCheck = GetComponent<PlayerCharacter>();
                		//Mob mobAttackerCheck = GetComponent<Mob>();
                		//PlayerCharacter pcDefenderCheck = target.GetComponent<PlayerCharacter>();
                		//Mob mobDefenderCheck = target.GetComponent<Mob>();
                		//if(pcAttackerCheck != null){
                		//    attacker = pcAttackerCheck.CharacterName;
                		//} else {
                		//    attacker = mobAttackerCheck.NAME;
                		//}
                		//if(pcDefenderCheck != null){
                		//    defender = pcDefenderCheck.CharacterName;
                		//} else {
                		//    defender = mobDefenderCheck.NAME;
                		//}
						//int type = 201; 
						//int element = 5;
                		//CombatLogNet cNet = new CombatLogNet(attacker, defender, content, amount, type, element, wasCrit);
                		//RpcSpawnCombatLog(cNet);
                    }
                    if(BonusPoisonWeapon){
                        target.DamageDealt(this, BonusPoisonEffect, false, false, BonusPoisonEffect, POISONDAMAGECOLOR, false, null, "Poison");
						//string attacker = string.Empty;
                		//string defender = string.Empty;
                		//string content = "attacked";
                		//string amount = BonusPoisonEffect.ToString();
                		//PlayerCharacter pcAttackerCheck = GetComponent<PlayerCharacter>();
                		//Mob mobAttackerCheck = GetComponent<Mob>();
                		//PlayerCharacter pcDefenderCheck = target.GetComponent<PlayerCharacter>();
                		//Mob mobDefenderCheck = target.GetComponent<Mob>();
                		//if(pcAttackerCheck != null){
                		//    attacker = pcAttackerCheck.CharacterName;
                		//} else {
                		//    attacker = mobAttackerCheck.NAME;
                		//}
                		//if(pcDefenderCheck != null){
                		//    defender = pcDefenderCheck.CharacterName;
                		//} else {
                		//    defender = mobDefenderCheck.NAME;
                		//}
						//int type = 202; 
						//int element = 5;
                		//CombatLogNet cNet = new CombatLogNet(attacker, defender, content, amount, type, element, wasCrit);
                		//RpcSpawnCombatLog(cNet);
                    }
                    if(BonusDiseaseWeapon){
                        target.DamageDealt(this, BonusDiseaseEffect, false, false, BonusDiseaseEffect, DISEASEDAMAGECOLOR, false, null, "Disease");
						//string attacker = string.Empty;
                		//string defender = string.Empty;
                		//string content = "attacked";
                		//string amount = BonusDiseaseEffect.ToString();
                		//PlayerCharacter pcAttackerCheck = GetComponent<PlayerCharacter>();
                		//Mob mobAttackerCheck = GetComponent<Mob>();
                		//PlayerCharacter pcDefenderCheck = target.GetComponent<PlayerCharacter>();
                		//Mob mobDefenderCheck = target.GetComponent<Mob>();
                		//if(pcAttackerCheck != null){
                		//    attacker = pcAttackerCheck.CharacterName;
                		//} else {
                		//    attacker = mobAttackerCheck.NAME;
                		//}
                		//if(pcDefenderCheck != null){
                		//    defender = pcDefenderCheck.CharacterName;
                		//} else {
                		//    defender = mobDefenderCheck.NAME;
                		//}
						//int type = 203; 
						//int element = 5;
                		//CombatLogNet cNet = new CombatLogNet(attacker, defender, content, amount, type, element, wasCrit);
                		//RpcSpawnCombatLog(cNet);
                    }
                    if(BonusMagicWeapon){
                        target.DamageDealt(this, BonusMagicEffect, false, false, BonusMagicEffect, MAGICDAMAGECOLOR, false, null, "Magic");
						//string attacker = string.Empty;
                		//string defender = string.Empty;
                		//string content = "attacked";
                		//string amount = BonusMagicEffect.ToString();
                		//PlayerCharacter pcAttackerCheck = GetComponent<PlayerCharacter>();
                		//Mob mobAttackerCheck = GetComponent<Mob>();
                		//PlayerCharacter pcDefenderCheck = target.GetComponent<PlayerCharacter>();
                		//Mob mobDefenderCheck = target.GetComponent<Mob>();
                		//if(pcAttackerCheck != null){
                		//    attacker = pcAttackerCheck.CharacterName;
                		//} else {
                		//    attacker = mobAttackerCheck.NAME;
                		//}
                		//if(pcDefenderCheck != null){
                		//    defender = pcDefenderCheck.CharacterName;
                		//} else {
                		//    defender = mobDefenderCheck.NAME;
                		//}
						//int type = 204; 
						//int element = 5;
                		//CombatLogNet cNet = new CombatLogNet(attacker, defender, content, amount, type, element, wasCrit);
                		//RpcSpawnCombatLog(cNet);
                    }
					if(BonusLeechWeapon){
                	    target.DamageDealt(this, BonusLeechEffect, false, false, BonusLeechEffect, MAGICDAMAGECOLOR, false, null, "Leech");
                	    cur_hp = cur_hp + BonusLeechEffect;
						if(cur_hp > max_hp){
							cur_hp = max_hp;
						}
						//string attacker = string.Empty;
                		//string defender = string.Empty;
                		//string content = "attacked";
                		//string amount = BonusLeechEffect.ToString();
                		//PlayerCharacter pcAttackerCheck = GetComponent<PlayerCharacter>();
                		//Mob mobAttackerCheck = GetComponent<Mob>();
                		//PlayerCharacter pcDefenderCheck = target.GetComponent<PlayerCharacter>();
                		//Mob mobDefenderCheck = target.GetComponent<Mob>();
                		//if(pcAttackerCheck != null){
                		//    attacker = pcAttackerCheck.CharacterName;
                		//} else {
                		//    attacker = mobAttackerCheck.NAME;
                		//}
                		//if(pcDefenderCheck != null){
                		//    defender = pcDefenderCheck.CharacterName;
                		//} else {
                		//    defender = mobDefenderCheck.NAME;
                		//}
						//int type = 205; 
						//int element = 5;
                		//CombatLogNet cNet = new CombatLogNet(attacker, defender, content, amount, type, element, wasCrit);
                		//RpcSpawnCombatLog(cNet);
                	}
                    if(ThreatMod){
                        threat = (int)(threat * ThreatModifier);
                    }
            	    if(dmgDealt || NPProc ){
						
						bool DoubleAttack = false;
						float roll = UnityEngine.Random.Range(1f, 101f);
						if(roll < doubleAttackChance){
							DoubleAttack = true;
						}
						if(DoubleAttack)
						ServerDoubleAttackMH(target);
						string magicString = normalHithexColor;
						if(wasCrit){
							magicString = criticalHitHexColor;
						}
            	        target.DamageDealt(this, value, wasCrit, false, threat, magicString, true, weaponType, "Autoattack");
						if(mob){
							if(mob.NAME.Contains("Spider")){
								float poisonRoll = UnityEngine.Random.Range(1f, 101f);
								string PoisonType = "No Proc";//5
								if(mob.TIER == 1){
									if(poisonRoll <= 5f){
										PoisonType = "Lesser Poison";//5
									}
								}
								if(mob.TIER == 2){
									if(poisonRoll <= 7f){
										PoisonType = "Moderate Poison";//7
									}
								}
								if(mob.TIER >= 3){
									if(poisonRoll <= 10f){
										PoisonType = "Greater Poison";//10
									}
								}
								if(PoisonType != "No Proc")
								curatorTM.UpdatePlayerCastedOffensiveSpellSingleTargetDPS(mob.assignedMatch, this, target, PoisonType, 1, 0);
							}
						}
						if(target.thornValue > 0){
							DamageDealt(target, target.thornValue, false, true, target.thornValue, MAGICDAMAGECOLOR, false, null, "Thorns");
						}
						//string attacker = string.Empty;
                		//string defender = string.Empty;
                		//string content = "attacked";
                		//string amount = value.ToString();
                		//PlayerCharacter pcAttackerCheck = GetComponent<PlayerCharacter>();
                		//Mob mobAttackerCheck = GetComponent<Mob>();
                		//PlayerCharacter pcDefenderCheck = target.GetComponent<PlayerCharacter>();
                		//Mob mobDefenderCheck = target.GetComponent<Mob>();
                		//if(pcAttackerCheck != null){
                		//    attacker = pcAttackerCheck.CharacterName;
                		//} else {
                		//    attacker = mobAttackerCheck.NAME;
                		//}
                		//if(pcDefenderCheck != null){
                		//    defender = pcDefenderCheck.CharacterName;
                		//} else {
                		//    defender = mobDefenderCheck.NAME;
                		//}
						//int type = 0; 
						//int element = 5;
                		//CombatLogNet cNet = new CombatLogNet(attacker, defender, content, amount, type, element, wasCrit);
                		//RpcSpawnCombatLog(cNet);
            	    }
					int valueOH = 0;
					bool dmgDealtOH = true;
					if(duelWielding){
						bool wasCritOH = false;
						if(IsCriticalHit()){
							wasCritOH = true;
						}
						if(target.mobType == "Undead"){
							if(IsCriticalHitUndead()){
								wasCritOH = true;
							}
						}	
                        valueOH = StatAsset.Instance.GetAutoAttackOffhand(this, target, wasCritOH);
						bool missedTargetOH = false;
						(bool NPProcOH, float missCalcOH) = GetHitChance();
						int roundedValueOH = (int)Math.Round(value + valueIncrease);
						if(NPProc){
							valueOH = roundedValueOH;
						}
						if(!pc){
							missCalcOH += 5f;
						} else {
							if(chargeStam){
								ChargeSwingDelayOH();
							}
							//get char weapon skill
							//for now we will just use 95%
							missCalcOH += 5f;//sudo number for weapon
							//get skill from weapon, which will be the skill / level of character * 10, so if skill is 50 and you are level 6 the chance to hit would be
							// 90 + (5 * (50/(6 * 10))) == 94.16% hit chance
							//cap 100% hit chance
						}
						int randomNumberOH = UnityEngine.Random.Range(1, 101);
        				// Compare the random number to the threshold
        				if (randomNumberOH > missCalcOH)
        				{
							missedTargetOH = true;
							dmgDealtOH = false;
            	    	   // target.RpcCastingSpell("Miss", null);
						target.AddRpcCall(null, null, false, true, "Miss", null, false, ownerName, targetName, this, target, Vector2.zero);

						//	string attacker = string.Empty;
                		//string defender = string.Empty;
                		//string content = "attacked";
                		//string amount = value.ToString();
                		//PlayerCharacter pcAttackerCheck = GetComponent<PlayerCharacter>();
                		//Mob mobAttackerCheck = GetComponent<Mob>();
                		//PlayerCharacter pcDefenderCheck = target.GetComponent<PlayerCharacter>();
                		//Mob mobDefenderCheck = target.GetComponent<Mob>();
                		//if(pcAttackerCheck != null){
                		//    attacker = pcAttackerCheck.CharacterName;
                		//} else {
                		//    attacker = mobAttackerCheck.NAME;
                		//}
                		//if(pcDefenderCheck != null){
                		//    defender = pcDefenderCheck.CharacterName;
                		//} else {
                		//    defender = mobDefenderCheck.NAME;
                		//}
						//int type = 110; 
						//int element = 5;
                		//CombatLogNet cNet = new CombatLogNet(attacker, defender, content, amount, type, element, wasCrit);
                		//RpcSpawnCombatLog(cNet);
        				}
        				else
        				{
        				    missedTargetOH = false;
        				}
						if(wasStealthed){
							valueOH *= 2;
						}
						bool dodgedOH = StatAsset.Instance.GetDodge(target);
            	    	if(dodgedOH && !missedTargetOH){
							dmgDealtOH = false;
            	    	    //target.RpcCastingSpell("Dodge", null);
						target.AddRpcCall(null, null, false, true, "Dodge", null, false, ownerName, targetName, this, target, Vector2.zero);

						//	string attacker = string.Empty;
                		//string defender = string.Empty;
                		//string content = "attacked";
                		//string amount = value.ToString();
                		//PlayerCharacter pcAttackerCheck = GetComponent<PlayerCharacter>();
                		//Mob mobAttackerCheck = GetComponent<Mob>();
                		//PlayerCharacter pcDefenderCheck = target.GetComponent<PlayerCharacter>();
                		//Mob mobDefenderCheck = target.GetComponent<Mob>();
                		//if(pcAttackerCheck != null){
                		//    attacker = pcAttackerCheck.CharacterName;
                		//} else {
                		//    attacker = mobAttackerCheck.NAME;
                		//}
                		//if(pcDefenderCheck != null){
                		//    defender = pcDefenderCheck.CharacterName;
                		//} else {
                		//    defender = mobDefenderCheck.NAME;
                		//}
						//int type = 111; 
						//int element = 5;
                		//CombatLogNet cNet = new CombatLogNet(attacker, defender, content, amount, type, element, wasCrit);
                		//RpcSpawnCombatLog(cNet);
            	    	}
            	    	bool parriedOH = StatAsset.Instance.GetParry(target);
            	    	if(parriedOH && !dodgedOH && !missedTargetOH && !isBehind){
							dmgDealtOH = false;
							//bool Riposted = false;
							//float roll = UnityEngine.Random.Range(1f, 101f);
							//if(roll < riposteChance){
							//	Riposted = true;
							//}
							//if(Riposted)
							//target.ServerRiposte(this);
							if(pcTargetCheck){
						target.AddRpcCall(null, null, false, true, "Parry", target.weaponType, false, ownerName, targetName, this, target, Vector2.zero);
								
            	        		//target.RpcCastingSpell("Parry", pc.weaponType);
						//		string attacker = string.Empty;
                		//string defender = string.Empty;
                		//string content = "attacked";
                		//string amount = value.ToString();
                		//PlayerCharacter pcAttackerCheck = GetComponent<PlayerCharacter>();
                		//Mob mobAttackerCheck = GetComponent<Mob>();
                		//PlayerCharacter pcDefenderCheck = target.GetComponent<PlayerCharacter>();
                		//Mob mobDefenderCheck = target.GetComponent<Mob>();
                		//if(pcAttackerCheck != null){
                		//    attacker = pcAttackerCheck.CharacterName;
                		//} else {
                		//    attacker = mobAttackerCheck.NAME;
                		//}
                		//if(pcDefenderCheck != null){
                		//    defender = pcDefenderCheck.CharacterName;
                		//} else {
                		//    defender = mobDefenderCheck.NAME;
                		//}
						//int type = 112; 
						//int element = 5;
                		//CombatLogNet cNet = new CombatLogNet(attacker, defender, content, amount, type, element, wasCrit);
                		//RpcSpawnCombatLog(cNet);
							} else {
						target.AddRpcCall(null, null, false, true, "Parry", null, false, ownerName, targetName, this, target, Vector2.zero);
            	        		//target.RpcCastingSpell("Parry", null);
						//		string attacker = string.Empty;
                		//string defender = string.Empty;
                		//string content = "attacked";
                		//string amount = value.ToString();
                		//PlayerCharacter pcAttackerCheck = GetComponent<PlayerCharacter>();
                		//Mob mobAttackerCheck = GetComponent<Mob>();
                		//PlayerCharacter pcDefenderCheck = target.GetComponent<PlayerCharacter>();
                		//Mob mobDefenderCheck = target.GetComponent<Mob>();
                		//if(pcAttackerCheck != null){
                		//    attacker = pcAttackerCheck.CharacterName;
                		//} else {
                		//    attacker = mobAttackerCheck.NAME;
                		//}
                		//if(pcDefenderCheck != null){
                		//    defender = pcDefenderCheck.CharacterName;
                		//} else {
                		//    defender = mobDefenderCheck.NAME;
                		//}
						//int type = 112; 
						//int element = 5;
                		//CombatLogNet cNet = new CombatLogNet(attacker, defender, content, amount, type, element, wasCrit);
                		//RpcSpawnCombatLog(cNet);
							}
            	    	}
						bool blockedAllOH = false;
            	    	bool blockedOH = StatAsset.Instance.GetBlock(target);
						if(blockSpellTriggered && !isBehind){
							blockedOH = true;
						}
            	    	if(!parriedOH && !dodgedOH && blockedOH && !missedTargetOH && !isBehind){
            	    	    valueOH -= target.shieldValue;
							if(blockSpellTriggered){
								valueOH = 0;
							}
            	    	    if(valueOH <= 0){
								blockedAllOH = true;
								dmgDealtOH = false;
						target.AddRpcCall(null, null, false, true, "Blocked", null, false, ownerName, targetName, this, target, Vector2.zero);
            	    	        //target.RpcCastingSpell("Blocked", null);
						//		string attacker = string.Empty;
                		//string defender = string.Empty;
                		//string content = "attacked";
                		//string amount = value.ToString();
                		//PlayerCharacter pcAttackerCheck = GetComponent<PlayerCharacter>();
                		//Mob mobAttackerCheck = GetComponent<Mob>();
                		//PlayerCharacter pcDefenderCheck = target.GetComponent<PlayerCharacter>();
                		//Mob mobDefenderCheck = target.GetComponent<Mob>();
                		//if(pcAttackerCheck != null){
                		//    attacker = pcAttackerCheck.CharacterName;
                		//} else {
                		//    attacker = mobAttackerCheck.NAME;
                		//}
                		//if(pcDefenderCheck != null){
                		//    defender = pcDefenderCheck.CharacterName;
                		//} else {
                		//    defender = mobDefenderCheck.NAME;
                		//}
						//int type = 113; 
						//int element = 5;
                		//CombatLogNet cNet = new CombatLogNet(attacker, defender, content, amount, type, element, wasCrit);
                		//RpcSpawnCombatLog(cNet);
            	    	    }
            	    	}
            	    	if(valueOH <= 0 && !parriedOH && !dodgedOH && !blockedAllOH && !missedTargetOH){
							dmgDealtOH = false;
            	    	    //target.RpcSpawnPopUpAbsorbed();
			target.AddRpcCall("absorbed", null, false, false, null, null, false, ownerName, targetName, this, target, Vector2.zero);

            	    	}
                    	int threatOH = valueOH;
						if(ThreatMod){
							threatOH = (int)(threatOH * ThreatModifier);
                    	}
						if(dmgDealtOH|| NPProc){
							bool DoubleAttack = false;
							float roll = UnityEngine.Random.Range(1f, 101f);
							if(roll < doubleAttackChance){
								DoubleAttack = true;
							}
							if(DoubleAttack)
							ServerDoubleAttackOH(target);
						string magicString = normalHithexColor;
						if(wasCritOH){
							magicString = criticalHitHexColor;
						}
            	        	target.DamageDealt(this, valueOH, wasCritOH, false, threatOH, magicString, true, weaponTypeOH, "AutoattackOH");
							if(target.thornValue > 0){
								DamageDealt(target, target.thornValue, false, true, target.thornValue, MAGICDAMAGECOLOR, false, null, "Thorns");
							}
							//string _attacker = string.Empty;
                			//string _defender = string.Empty;
                			//string _content = "attacked";
                			//string _amount = valueOH.ToString();
                			//PlayerCharacter _pcAttackerCheck = GetComponent<PlayerCharacter>();
                			//Mob _mobAttackerCheck = GetComponent<Mob>();
                			//PlayerCharacter _pcDefenderCheck = target.GetComponent<PlayerCharacter>();
                			//Mob _mobDefenderCheck = target.GetComponent<Mob>();
                			//if(_pcAttackerCheck != null){
                			//    _attacker = _pcAttackerCheck.CharacterName;
                			//} else {
                			//    _attacker = _mobAttackerCheck.NAME;
                			//}
                			//if(_pcDefenderCheck != null){
                			//    _defender = _pcDefenderCheck.CharacterName;
                			//} else {
                			//    _defender = _mobDefenderCheck.NAME;
                			//}
							//int _type = 105; 
							//int _element = 5;
                			//CombatLogNet _cNet = new CombatLogNet(_attacker, _defender, _content, _amount, _type, _element, wasCritOH);
                			//RpcSpawnCombatLog(_cNet);
							//StartCoroutine(DelayedWeaponAttackSound(wasCritOH));
						}
                    }
					
					if(chargeStam){
						if(pc){
							ChargeSwingDelay();
						} else {
							mob.AddStaminaMob(mob.GetAttackDelayEnemy());
						}
					}
					
					return;
		    	}
			}
	}
	public string GetSpellType(string spell){
        string spellType = "";
        // Archer Skills
        if (spell == "Aimed Shot")
            spellType = "aimedCast";
        if (spell == "Bandage Wound")
            spellType = "bandageCast";
        if (spell == "Head Shot")
            spellType = "aimedCast";
        if (spell == "Silence Shot")
            spellType = "aimedCast";
        if (spell == "Crippling Shot")
            spellType = "aimedCast";
        if (spell == "Dash")
            spellType = "dash";
        if (spell == "Identify Enemy")
            spellType = "IDEnemy";
        if (spell == "Double Shot")
            spellType = "doubleShot";
        if (spell == "Natures Precision")
            spellType = "naturesPrecision";
        if (spell == "Fire Arrow")
            spellType = "aimedCast";
        if (spell == "Penetrating Shot")
            spellType = "aimedCast";
        if (spell == "Sleep")
            spellType = "sleep";

        // Enchanter Skills
        if (spell == "Mesmerize")
            spellType = "mesmerizeCast";
        if (spell == "Haste")
            spellType = "hasteCast";
        if (spell == "Root")
            spellType = "mesmerizeCast";
        if (spell == "Invisibility")
            spellType = "hasteCast";
        if (spell == "Rune")
            spellType = "hasteCast";
        if (spell == "Slow")
            spellType = "mesmerizeCast";
        if (spell == "Magic Sieve")
            spellType = "mesmerizeCast";
        if (spell == "Aneurysm")
            spellType = "mesmerizeCast";
        if (spell == "Gravity Stun")
            spellType = "gravityStun";
        if (spell == "Weaken")
            spellType = "mesmerizeCast";
        if (spell == "Resist Magic")
            spellType = "hasteCast";
        if (spell == "Purge")
            spellType = "mesmerizeCast";
        if (spell == "Charm")
            spellType = "mesmerizeCast";
        if (spell == "Mp Transfer")
            spellType = "hasteCast";

        // Fighter Skills
        if (spell == "Charge")
            spellType = "chargeCast";
		if (spell == "chargeEnd")
            spellType = "chargeStrike";
        if (spell == "Bash")
            spellType = "shieldBash";
        if (spell == "Intimidating Roar")
            spellType = "taunt";
        if (spell == "Protect")
            spellType = "protect";
        if (spell == "Knockback")
            spellType = "heavySwing";
        if (spell == "Throw Stone")
            spellType = "throwStone";
        if (spell == "Heavy Swing")
            spellType = "heavySwing";
        if (spell == "Taunt")
            spellType = "taunt";
        if (spell == "Block")
            spellType = "blockCast";
        if (spell == "Tank Stance")
            spellType = "tankStance";
        if (spell == "Offensive Stance")
            spellType = "tankStance";
        if (spell == "Critical Strike")
            spellType = "criticalStrike";
        if (spell == "Riposte")
            spellType = "riposte";
        if (spell == "Double Attack")
            spellType = "doubleSwing";

        // Priest Skills
        if (spell == "Holy Bolt")
            spellType = "holyBolt";
        if (spell == "Heal")
            spellType = "healingMagic";
        if (spell == "Cure Poison")
            spellType = "healingMagic";
        if (spell == "Dispel")
            spellType = "holyBolt";
        if (spell == "Fortitude")
            spellType = "undeadProtection";
        if (spell == "Turn Undead")
            spellType = "turnUndead";
        if (spell == "Undead Protection")
            spellType = "undeadProtection";
        if (spell == "Smite")
            spellType = "holyBolt";
        if (spell == "Shield Bash")
            spellType = "shieldBash";
        if (spell == "Greater Heal")
            spellType = "healingMagic";
        if (spell == "Group Heal")
            spellType = "healingMagic";
        if (spell == "Regeneration")
            spellType = "healingMagic";
        if (spell == "Resurrect")
            spellType = "healingMagic";

        // Rogue Skills
        if (spell == "Shuriken")
            spellType = "shuriken";
        if (spell == "Hide")
            spellType = "sneak";
        if (spell == "Picklock")
            spellType = "applyPoison";
        if (spell == "Steal")
            spellType = "steal";
        if (spell == "Detect Traps")
            spellType = "applyPoison";
        if (spell == "Tendon Slice")
            spellType = "tendonSlice";
        if (spell == "Backstab")
            spellType = "backstab";
        if (spell == "Rush")
            spellType = "rush";
        if (spell == "Blind")
            spellType = "blind";
        if (spell == "Poison")
            spellType = "applyPoison";
		if (spell == "Sneak")
            spellType = "sneak";
        // Wizard Skills
        if (spell == "Ice")
            spellType = "iceMagic";
        if (spell == "Fire")
            spellType = "fireMagic";
		if (spell == "Fireball")
            spellType = "fireMagic";
        if (spell == "Blizzard")
            spellType = "iceMagic";
        if (spell == "Magic Burst")
            spellType = "magicMagic";
        if (spell == "Teleport")
            spellType = "magicMagic";
        if (spell == "Meteor Shower")
            spellType = "fireMagic";
        if (spell == "Ice Block")
            spellType = "iceMagic";
        if (spell == "Ice Blast")
            spellType = "iceMagic";
        if (spell == "Incinerate")
            spellType = "fireMagic";
        if (spell == "Brain Freeze")
            spellType = "iceMagic";
        if (spell == "Light")
            spellType = "fireMagic";
        if (spell == "Magic Missile")
            spellType = "magicMagic";
        //Druid skills
        if (spell == "Rejuvenation")
            spellType = "";
        if (spell == "Swarm Of Insects")
            spellType = "";
        if (spell == "Thorns")
            spellType = "";
        if (spell == "Nature's Protection")
            spellType = "";
        if (spell == "Strength")
            spellType = "";
        if (spell == "Snare")
            spellType = "";
        if (spell == "Engulfing Roots")
            spellType = "";
        if (spell == "Shapeshift")
            spellType = "";
        if (spell == "Tornado")
            spellType = "";
        if (spell == "Chain Lightning")
            spellType = "";
        if (spell == "Greater Rejuvenation")
            spellType = "";
        //Paladin skills
         if (spell == "Holy Swing")
            spellType = "";
        if (spell == "Divine Armor")
            spellType = "";
        if (spell == "Flash Of Light")
            spellType = "";
        if (spell == "Undead Slayer")
            spellType = "";
        if (spell == "Stun")
            spellType = "";
        if (spell == "Celestial Wave")
            spellType = "";
        if (spell == "Angelic Shield")
            spellType = "";
        if (spell == "Cleanse")
            spellType = "";
        if (spell == "Consecrated Ground")
            spellType = "";
        if (spell == "Divine Wrath")
            spellType = "";
        if (spell == "Cover")
            spellType = "";
        if (spell == "Shackle")
            spellType = "";

        return spellType;

    }
	[ClientRpc]
    public void ClientSetBool(string boolName, bool condition)
    {
        animator.SetBool(boolName, condition);
    }
	public void PlayerSetBool(string boolName, bool condition)
    {
        animator.SetBool(boolName, condition);
    }
	[Server]
	public void ServerDoubleAttackOH(MovingObject riposteTarget){
		bool blockSpellTriggered = false;
		float distanceToTarget = Vector2.Distance(transform.position, riposteTarget.transform.position);
	    if (distanceToTarget <= attackRange)
	    {
			Mob mob = GetComponent<Mob>();
			PlayerCharacter pc = GetComponent<PlayerCharacter>();
			string ownerName = string.Empty;
        	string targetName = string.Empty;
        	if(pc){
        	   ownerName = pc.CharacterName;
        	} else {
				ownerName = StatAsset.Instance.GetMobName(StatAsset.Instance.GetEnemyToPrefabMapping(), mob.NAME);
        	}
        	PlayerCharacter pcChecktarget = riposteTarget.GetComponent<PlayerCharacter>();
        	if(pcChecktarget){
        	   targetName = pcChecktarget.CharacterName;
        	} else {
        	    Mob mobCheckTarget = riposteTarget.GetComponent<Mob>();
				targetName = StatAsset.Instance.GetMobName(StatAsset.Instance.GetEnemyToPrefabMapping(), mobCheckTarget.NAME);
        	}
			int criticalStrikeMeleeLvl = 0;
			float criticalStrikeMeleeChance = 0f;
			Vector3 updateLocation = new Vector3(Mathf.FloorToInt(transform.position.x) + 0.5f, Mathf.FloorToInt(transform.position.y) + 0.5f, 0);
        	//NewFogUpdate(updateLocation); // Run NewFogUpdate
			if(pc){
				//We need to now set this owner in combat.
				curatorTM.CombatCalled();
				if(pc.ClassType == "Fighter"){
					for(int _char = 0; _char < pc.assignedPlayer.GetInformationSheets().Count; _char++){
        	    		if(pc.assignedPlayer.GetInformationSheets()[_char].CharacterID == pc.CharID){
        	    		    for(int ability = 0; ability < pc.assignedPlayer.GetInformationSheets()[_char].CharSpellData.Count; ability++){

								if(pc.assignedPlayer.GetInformationSheets()[_char].CharSpellData[ability].Key == "WestT3BottomSkill"){
									var abilityRankString = System.Text.RegularExpressions.Regex.Match(pc.assignedPlayer.GetInformationSheets()[_char].CharSpellData[ability].Value, @"\d+$");
        	                		if (abilityRankString.Success) {
        	                		    criticalStrikeMeleeLvl = int.Parse(abilityRankString.Value); // Parse the rank number
        	                		}
									break;
								}
							}
							break;
						}
					}
				}
				if(criticalStrikeMeleeLvl > 0){
					criticalStrikeMeleeChance += (criticalStrikeMeleeLvl * .2f);
				}
			}
			if( riposteTarget != null){
				if(riposteTarget.FROZEN){
					return;
				}
				if(!riposteTarget.Dying){
					Vector3 movementDirection = riposteTarget.transform.position - transform.position;
					//Vector2 direction = new Vector2(targetPosition.x - transform.position.x, targetPosition.y - transform.position.y);
        			float angle = Mathf.Atan2(movementDirection.y, movementDirection.x) * Mathf.Rad2Deg;
        			// Determine the direction string based on the angle
        			string directionString = GetDirectionString(angle);
					string ourName = string.Empty;
					if(mob){
						ourName = mob.NAME;
					}
					if(pc){
						ourName = pc.CharacterName;
					}
        			Debug.Log(ourName + " is facing direction: " + directionString);	
					if(DirectionString != directionString){
						DirectionString = directionString;
						RpcSetDirectionFacing(directionString);
					}
					//bool newRightFace = movementDirection.x >= 0;
					//RpcUpdateFacingDirection(newRightFace);
					bool wasCrit = false;
					if(IsCriticalHit()){
						wasCrit = true;
					}
					if(riposteTarget.mobType == "Undead"){
						if(IsCriticalHitUndead()){
							wasCrit = true;
						}
					}
					//ClientAttackSound(weaponTypeOH, wasCrit);
					if(attackRange <= 3){
						if(mob){
	            			StartCoroutine(BumpTowards(riposteTarget.transform.position));
						} 
					} else {
						curatorTM.PlayerSpawnRangedAttack(this, riposteTarget, attackRange, wasCrit);
					}
					int valueOH = 0;
					bool dmgDealtOH = true;
					if(duelWielding){
						
                        valueOH = StatAsset.Instance.GetAutoAttackOffhand(this, riposteTarget, wasCrit);
						bool missedTargetOH = false;
						(bool NPProcOH, float missCalcOH) = GetHitChance();

						if(!pc){
							missCalcOH += 5f;
						} else {
							//get char weapon skill
							//for now we will just use 95%
							missCalcOH += 5f;//sudo number for weapon
							//get skill from weapon, which will be the skill / level of character * 10, so if skill is 50 and you are level 6 the chance to hit would be
							// 90 + (5 * (50/(6 * 10))) == 94.16% hit chance
							//cap 100% hit chance
						}
						int randomNumberOH = UnityEngine.Random.Range(1, 101);
        				// Compare the random number to the threshold
        				if (randomNumberOH > missCalcOH)
        				{
							missedTargetOH = true;
							dmgDealtOH = false;
            	    	    
						riposteTarget.AddRpcCall(null, null, false, true, "Miss", null, false, ownerName, targetName, this, riposteTarget, Vector2.zero);
							//riposteTarget.RpcCastingSpell("Miss", null);
        				}
        				else
        				{
        				    missedTargetOH = false;
        				}
						bool dodgedOH = StatAsset.Instance.GetDodge(riposteTarget);
            	    	if(dodgedOH && !missedTargetOH){
							dmgDealtOH = false;
            	    	    
						riposteTarget.AddRpcCall(null, null, false, true, "Dodge", null, false, ownerName, targetName, this, riposteTarget, Vector2.zero);
							//riposteTarget.RpcCastingSpell("Dodge", null);
            	    	}
            	    	bool parriedOH = StatAsset.Instance.GetParry(riposteTarget);
            	    	if(parriedOH && !dodgedOH && !missedTargetOH){
							dmgDealtOH = false;
							if(pcChecktarget){
								riposteTarget.AddRpcCall(null, null, false, true, "Parry", riposteTarget.weaponType, false, ownerName, targetName, this, riposteTarget, Vector2.zero);
								//riposteTarget.RpcCastingSpell("Parry", pc.weaponType);
							} else {
								riposteTarget.AddRpcCall(null, null, false, true, "Parry", null, false, ownerName, targetName, this, riposteTarget, Vector2.zero);
								//riposteTarget.RpcCastingSpell("Parry", null);
							}
            	    	}
						bool blockedAllOH = false;
            	    	bool blockedOH = StatAsset.Instance.GetBlock(riposteTarget);
						if(riposteTarget.GetBlockSpell()){
							ServerTriggeredBlock();
							blockSpellTriggered = true;
							blockedOH = true;
						}
            	    	if(!parriedOH && !dodgedOH && blockedOH && !missedTargetOH){
            	    	    valueOH -= riposteTarget.shieldValue;
							if(blockSpellTriggered){
								valueOH = 0;
							}
            	    	    if(valueOH <= 0){
								blockedAllOH = true;
								dmgDealtOH = false;
            	    	        
						riposteTarget.AddRpcCall(null, null, false, true, "Blocked", null, false, ownerName, targetName, this, riposteTarget, Vector2.zero);
								//riposteTarget.RpcCastingSpell("Blocked", null);
            	    	    }
            	    	}
            	    	if(valueOH <= 0 && !parriedOH && !dodgedOH && !blockedAllOH && !missedTargetOH){
							dmgDealtOH = false;
            	    	    //riposteTarget.RpcSpawnPopUpAbsorbed();
							riposteTarget.AddRpcCall("absorbed", null, false, false, null, null, false, ownerName, targetName, this, riposteTarget, Vector2.zero);

            	    	}
                    	int threatOH = valueOH;
						if(ThreatMod){
							threatOH = (int)(threatOH * ThreatModifier);
                    	}
						if(dmgDealtOH){
							string magicString = normalHithexColor;
						if(wasCrit){
							magicString = criticalHitHexColor;
						}
            	        	riposteTarget.DamageDealt(this, valueOH, wasCrit, false, threatOH, magicString, true, weaponTypeOH, "DoubleAttackOH");
								if(riposteTarget.thornValue > 0){
								DamageDealt(riposteTarget, riposteTarget.thornValue, false, true, riposteTarget.thornValue, MAGICDAMAGECOLOR, false, null, "Thorns");
							}
						}
                    }
					//string attacker = string.Empty;
                	//string defender = string.Empty;
                	//string content = "attacked";
                	//string amount = valueOH.ToString();
                	//PlayerCharacter pcAttackerCheck = GetComponent<PlayerCharacter>();
                	//Mob mobAttackerCheck = GetComponent<Mob>();
                	//PlayerCharacter pcDefenderCheck = riposteTarget.GetComponent<PlayerCharacter>();
                	//Mob mobDefenderCheck = riposteTarget.GetComponent<Mob>();
                	//if(pcAttackerCheck != null){
                	//    attacker = pcAttackerCheck.CharacterName;
                	//} else {
                	//    attacker = mobAttackerCheck.NAME;
                	//}
                	//if(pcDefenderCheck != null){
                	//    defender = pcDefenderCheck.CharacterName;
                	//} else {
                	//    defender = mobDefenderCheck.NAME;
                	//}
					//int type = 102; 
					//int element = 5;
                	//CombatLogNet cNet = new CombatLogNet(attacker, defender, content, amount, type, element, wasCrit);
                	//RpcSpawnCombatLog(cNet);
					return;
		    	}
			}
	    }
	}
	[Server]
	public void ServerDoubleAttackMH(MovingObject riposteTarget){
		bool blockSpellTriggered = false;
		float distanceToTarget = Vector2.Distance(transform.position, riposteTarget.transform.position);
	    if (distanceToTarget <= attackRange)
	    {
			Mob mob = GetComponent<Mob>();
			PlayerCharacter pc = GetComponent<PlayerCharacter>();
			string ownerName = string.Empty;
        	string targetName = string.Empty;
        	if(pc){
        	   ownerName = pc.CharacterName;
        	} else {
				ownerName = StatAsset.Instance.GetMobName(StatAsset.Instance.GetEnemyToPrefabMapping(), mob.NAME);
        	}
        	PlayerCharacter pcChecktarget = riposteTarget.GetComponent<PlayerCharacter>();
        	if(pcChecktarget){
        	   targetName = pcChecktarget.CharacterName;
        	} else {
        	    Mob mobCheckTarget = riposteTarget.GetComponent<Mob>();
				targetName = StatAsset.Instance.GetMobName(StatAsset.Instance.GetEnemyToPrefabMapping(), mobCheckTarget.NAME);
        	}
			int criticalStrikeMeleeLvl = 0;
			float criticalStrikeMeleeChance = 0f;
			Vector3 updateLocation = new Vector3(Mathf.FloorToInt(transform.position.x) + 0.5f, Mathf.FloorToInt(transform.position.y) + 0.5f, 0);
        	//NewFogUpdate(updateLocation); // Run NewFogUpdate
			if(pc){
				//We need to now set this owner in combat.
				curatorTM.CombatCalled();
				if(pc.ClassType == "Fighter"){
					for(int _char = 0; _char < pc.assignedPlayer.GetInformationSheets().Count; _char++){
        	    		if(pc.assignedPlayer.GetInformationSheets()[_char].CharacterID == pc.CharID){
        	    		    for(int ability = 0; ability < pc.assignedPlayer.GetInformationSheets()[_char].CharSpellData.Count; ability++){
								if(pc.assignedPlayer.GetInformationSheets()[_char].CharSpellData[ability].Key == "WestT3BottomSkill"){
									var abilityRankString = System.Text.RegularExpressions.Regex.Match(pc.assignedPlayer.GetInformationSheets()[_char].CharSpellData[ability].Value, @"\d+$");
        	                		if (abilityRankString.Success) {
        	                		    criticalStrikeMeleeLvl = int.Parse(abilityRankString.Value); // Parse the rank number
        	                		}
									break;
								}
							}
							break;
						}
					}
				}
				if(criticalStrikeMeleeLvl > 0){
					criticalStrikeMeleeChance += (criticalStrikeMeleeLvl * .2f);
				}
			}
			if( riposteTarget != null){
				if(riposteTarget.FROZEN){
					return;
				}
				if(!riposteTarget.Dying){
					Vector3 movementDirection = riposteTarget.transform.position - transform.position;
					//Vector2 direction = new Vector2(targetPosition.x - transform.position.x, targetPosition.y - transform.position.y);
        			float angle = Mathf.Atan2(movementDirection.y, movementDirection.x) * Mathf.Rad2Deg;
        			// Determine the direction string based on the angle
        			string directionString = GetDirectionString(angle);
					string ourName = string.Empty;
					if(mob){
						ourName = mob.NAME;
					}
					if(pc){
						ourName = pc.CharacterName;
					}
        			Debug.Log(ourName + " is facing direction: " + directionString);	
					if(DirectionString != directionString){
						DirectionString = directionString;
						RpcSetDirectionFacing(directionString);
					}
					//bool newRightFace = movementDirection.x >= 0;
					//RpcUpdateFacingDirection(newRightFace);

					//if (newRightFace != rightFace)
					//{
					//	rightFace = newRightFace;
					//	RpcUpdateFacingDirection(newRightFace);
					//}
					bool dmgDealt = true;
					float criticalValue = 0;
					bool wasCrit = false;
					if(IsCriticalHit()){
						wasCrit = true;
					}
					if(riposteTarget.mobType == "Undead"){
						if(IsCriticalHitUndead()){
							wasCrit = true;
						}
					}	
					int value = StatAsset.Instance.GetAutoAttack(this, riposteTarget, wasCrit);

					//ClientAttackSound(weaponType, wasCrit);

					if(attackRange <= 3){
						if(mob){
	            			StartCoroutine(BumpTowards(riposteTarget.transform.position));
						} 
					} else {
						curatorTM.PlayerSpawnRangedAttack(this, riposteTarget, attackRange, wasCrit);
					}
					bool missedTarget = false;
					(bool NPProc, float missCalc) = GetHitChance();
					if(!pc){
						missCalc += 5f;
					} else {
						//get char weapon skill
						//for now we will just use 95%
						missCalc += 5f;//sudo number for weapon
						//get skill from weapon, which will be the skill / level of character * 10, so if skill is 50 and you are level 6 the chance to hit would be
						// 90 + (5 * (50/(6 * 10))) == 94.16% hit chance
						//cap 100% hit chance
					}
					int randomNumber = UnityEngine.Random.Range(1, 101);
        			// Compare the random number to the threshold
        			if (randomNumber > missCalc)
        			{
						
        			    missedTarget = true;
						dmgDealt = false;
						riposteTarget.AddRpcCall(null, null, false, true, "Miss", null, false, ownerName, targetName, this, riposteTarget, Vector2.zero);
            	    	//riposteTarget.RpcCastingSpell("Miss", null);
        			}
					bool dodged = StatAsset.Instance.GetDodge(riposteTarget);
            	    if(dodged && !missedTarget){
						dmgDealt = false;
						riposteTarget.AddRpcCall(null, null, false, true, "Dodge", null, false, ownerName, targetName, this, riposteTarget, Vector2.zero);
            	        //riposteTarget.RpcCastingSpell("Dodge", null);
            	    }
            	    bool parried = StatAsset.Instance.GetParry(riposteTarget);
            	    if(parried && !dodged && !missedTarget){
						dmgDealt = false;
						if(pcChecktarget){
						riposteTarget.AddRpcCall(null, null, false, true, "Parry", riposteTarget.weaponType, false, ownerName, targetName, this, riposteTarget, Vector2.zero);
            	        	//riposteTarget.RpcCastingSpell("Parry", pc.weaponType);
						} else {
						riposteTarget.AddRpcCall(null, null, false, true, "Parry", null, false, ownerName, targetName, this, riposteTarget, Vector2.zero);
            	        	//riposteTarget.RpcCastingSpell("Parry", null);
						}
            	    }
					bool blockedAll = false;
            	    bool blocked = StatAsset.Instance.GetBlock(riposteTarget);
					if(riposteTarget.GetBlockSpell()){
						ServerTriggeredBlock();
						blockSpellTriggered = true;
						blocked = true;
					}
            	    if(!parried && !dodged && blocked && !missedTarget){
            	        value = value - riposteTarget.shieldValue;
						if(blockSpellTriggered){
							value = 0;
						}
            	        if(value <= 0){
							blockedAll = true;
							dmgDealt = false;
						riposteTarget.AddRpcCall(null, null, false, true, "Blocked", null, false, ownerName, targetName, this, riposteTarget, Vector2.zero);
            	            //riposteTarget.RpcCastingSpell("Blocked", null);
            	        }
            	    }
            	    if(value <= 0 && !parried && !dodged && !blockedAll && !missedTarget){
						dmgDealt = false;
			riposteTarget.AddRpcCall("absorbed", null, false, false, null, null, false, ownerName, targetName, this, riposteTarget, Vector2.zero);
            	        //riposteTarget.RpcSpawnPopUpAbsorbed();
            	    }
					int threat = value;
                    if(BonusColdWeapon){
						DateTime currentTimeUtc = DateTime.UtcNow;
                        DateTime expirationTimeUtc = currentTimeUtc.AddSeconds(30);
                        riposteTarget.ApplyStatChange("Agility", 30f, 30f, 20, "FrozenGreatsword", false, 0, false, false, expirationTimeUtc.ToString("o"));
                        riposteTarget.DamageDealt(this, BonusColdEffect, false, false, BonusColdEffect, COLDDAMAGECOLOR, false, null, "Ice");
                    }
                    if(BonusFireWeapon){
                        riposteTarget.DamageDealt(this, BonusFireEffect, false, false, BonusFireEffect, FIREDAMAGECOLOR, false, null, "Fire");
                    }
                    if(BonusPoisonWeapon){
                        riposteTarget.DamageDealt(this, BonusPoisonEffect, false, false, BonusPoisonEffect, POISONDAMAGECOLOR, false, null, "Poison");
                    }
                    if(BonusDiseaseWeapon){
                        riposteTarget.DamageDealt(this, BonusDiseaseEffect, false, false, BonusDiseaseEffect, DISEASEDAMAGECOLOR, false, null, "Disease");
                    }
                    if(BonusMagicWeapon){
                        riposteTarget.DamageDealt(this, BonusMagicEffect, false, false, BonusMagicEffect, MAGICDAMAGECOLOR, false, null, "Magic");
                    }
					if(BonusLeechWeapon){
                	    riposteTarget.DamageDealt(this, BonusLeechEffect, false, false, BonusLeechEffect, MAGICDAMAGECOLOR, false, null, "Leech");
                	    cur_hp = cur_hp + BonusLeechEffect;
						if(cur_hp > max_hp){
							cur_hp = max_hp;
						}
                	}
                    if(ThreatMod){
                        threat = (int)(threat * ThreatModifier);
                    }
            	    if(dmgDealt){
						string magicString = normalHithexColor;
						if(wasCrit){
							magicString = criticalHitHexColor;
						}
            	        riposteTarget.DamageDealt(this, value, wasCrit, false, threat, magicString, true, weaponType, "DoubleAttackMH");
						if(riposteTarget.thornValue > 0){
							DamageDealt(riposteTarget, riposteTarget.thornValue, false, true, riposteTarget.thornValue, MAGICDAMAGECOLOR, false, null, "Thorns");
						}
            	    }
					//string attacker = string.Empty;
                	//string defender = string.Empty;
                	//string content = "attacked";
					//if(!dmgDealt){
					//	value = 0;
					//}
                	//string amount = value.ToString();
                	//PlayerCharacter pcAttackerCheck = GetComponent<PlayerCharacter>();
                	//Mob mobAttackerCheck = GetComponent<Mob>();
                	//PlayerCharacter pcDefenderCheck = riposteTarget.GetComponent<PlayerCharacter>();
                	//Mob mobDefenderCheck = riposteTarget.GetComponent<Mob>();
                	//if(pcAttackerCheck != null){
                	//    attacker = pcAttackerCheck.CharacterName;
                	//} else {
                	//    attacker = mobAttackerCheck.NAME;
                	//}
                	//if(pcDefenderCheck != null){
                	//    defender = pcDefenderCheck.CharacterName;
                	//} else {
                	//    defender = mobDefenderCheck.NAME;
                	//}
					//int type = 101; 
					//int element = 5;
                	//CombatLogNet cNet = new CombatLogNet(attacker, defender, content, amount, type, element, wasCrit);
                	//RpcSpawnCombatLog(cNet);
					return;
		    	}
			}
	        }
	}
	[Server]
	public void ServerRiposte(MovingObject riposteTarget){
		bool blockSpellTriggered = false;
		if(Stunned || Mesmerized){
			return;
		}
		if(riposteTarget.IsBehindAnotherObject(transform.position, DirectionString)){
			return;
		}
		print("Riposted attack!!");
		float distanceToTarget = Vector2.Distance(transform.position, riposteTarget.transform.position);
	    if (distanceToTarget <= attackRange)
	    {
			Mob mob = GetComponent<Mob>();
			PlayerCharacter pc = GetComponent<PlayerCharacter>();
			string ownerName = string.Empty;
        	string targetName = string.Empty;
        	if(pc){
        	   ownerName = pc.CharacterName;
        	} else {
				ownerName = StatAsset.Instance.GetMobName(StatAsset.Instance.GetEnemyToPrefabMapping(), mob.NAME);
        	}
            PlayerCharacter pcChecktarget = riposteTarget.GetComponent<PlayerCharacter>();
            if(pcChecktarget){
               targetName = pcChecktarget.CharacterName;
            } else {
                Mob mobCheckTarget = riposteTarget.GetComponent<Mob>();
		    	targetName = StatAsset.Instance.GetMobName(StatAsset.Instance.GetEnemyToPrefabMapping(), mobCheckTarget.NAME);
            }
			Vector3 updateLocation = new Vector3(Mathf.FloorToInt(transform.position.x) + 0.5f, Mathf.FloorToInt(transform.position.y) + 0.5f, 0);
			if(pc){
				curatorTM.CombatCalled();
			}
			if( riposteTarget != null){
				if(riposteTarget.FROZEN){
					return;
				}
				if(!riposteTarget.Dying){
					bool dmgDealt = true;
					bool wasCrit = false;
					if(IsCriticalHit()){
						wasCrit = true;
					}
					if(riposteTarget.mobType == "Undead"){
						if(IsCriticalHitUndead()){
							wasCrit = true;
						}
					}	
					int value = StatAsset.Instance.GetAutoAttack(this, riposteTarget, wasCrit);
					//ClientAttackSound(weaponType, wasCrit);
					if(attackRange <= 3){
						if(mob){
	            			StartCoroutine(BumpTowards(riposteTarget.transform.position));
						} 
					} else {
						curatorTM.PlayerSpawnRangedAttack(this, riposteTarget, attackRange, wasCrit);
					}
					bool missedTarget = false;
					(bool NPProc, float missCalc) = GetHitChance();
					if(!pc){
						missCalc += 5f;
					} else {
						//get char weapon skill
						//for now we will just use 95%
						missCalc += 5f;//sudo number for weapon
						//get skill from weapon, which will be the skill / level of character * 10, so if skill is 50 and you are level 6 the chance to hit would be
						// 90 + (5 * (50/(6 * 10))) == 94.16% hit chance
						//cap 100% hit chance
					}
					int randomNumber = UnityEngine.Random.Range(1, 101);
        			// Compare the random number to the threshold
        			if (randomNumber > missCalc)
        			{
        			    missedTarget = true;
						dmgDealt = false;
						riposteTarget.AddRpcCall(null, null, false, true, "Miss", null, false, ownerName, targetName, this, riposteTarget, Vector2.zero);
            	    	
						//riposteTarget.RpcCastingSpell("Miss", null);
        			}
					bool dodged = StatAsset.Instance.GetDodge(riposteTarget);
            	    if(dodged && !missedTarget){
						dmgDealt = false;
						riposteTarget.AddRpcCall(null, null, false, true, "Dodge", null, false, ownerName, targetName, this, riposteTarget, Vector2.zero);
            	        
						//riposteTarget.RpcCastingSpell("Dodge", null);
            	    }
            	    bool parried = StatAsset.Instance.GetParry(riposteTarget);
            	    if(parried && !dodged && !missedTarget){
						dmgDealt = false;
						if(pcChecktarget){
							riposteTarget.AddRpcCall(null, null, false, true, "Parry", riposteTarget.weaponType, false, ownerName, targetName, this, riposteTarget, Vector2.zero);
							//riposteTarget.RpcCastingSpell("Parry", pc.weaponType);
						} else {
							riposteTarget.AddRpcCall(null, null, false, true, "Parry", null, false, ownerName, targetName, this, riposteTarget, Vector2.zero);
							//riposteTarget.RpcCastingSpell("Parry", null);
						}
            	    }
					bool blockedAll = false;
            	    bool blocked = StatAsset.Instance.GetBlock(riposteTarget);
					if(riposteTarget.GetBlockSpell()){
						ServerTriggeredBlock();
						blockSpellTriggered = true;
						blocked = true;
					}
            	    if(!parried && !dodged && blocked && !missedTarget){
            	        value = value - riposteTarget.shieldValue;
						if(blockSpellTriggered){
							value = 0;
						}
            	        if(value <= 0){
							blockedAll = true;
							dmgDealt = false;
						riposteTarget.AddRpcCall(null, null, false, true, "Blocked", null, false, ownerName, targetName, this, riposteTarget, Vector2.zero);
            	            
							//riposteTarget.RpcCastingSpell("Blocked", null);
            	        }
            	    }
            	    if(value <= 0 && !parried && !dodged && !blockedAll && !missedTarget){
						dmgDealt = false;
						riposteTarget.AddRpcCall("absorbed", null, false, false, null, null, false, ownerName, targetName, this, riposteTarget, Vector2.zero);
            	        //riposteTarget.RpcSpawnPopUpAbsorbed();
            	    }
					int threat = value;
                    if(BonusColdWeapon){
						DateTime currentTimeUtc = DateTime.UtcNow;
                        DateTime expirationTimeUtc = currentTimeUtc.AddSeconds(30);
                        riposteTarget.ApplyStatChange("Agility", 30f, 30f, 20, "FrozenGreatsword", false, 0, false, false, expirationTimeUtc.ToString("o"));
                        riposteTarget.DamageDealt(this, BonusColdEffect, false, false, BonusColdEffect, COLDDAMAGECOLOR, false, null, "Ice");
                    }
                    if(BonusFireWeapon){
                        riposteTarget.DamageDealt(this, BonusFireEffect, false, false, BonusFireEffect, FIREDAMAGECOLOR, false, null, "Fire");
                    }
                    if(BonusPoisonWeapon){
                        riposteTarget.DamageDealt(this, BonusPoisonEffect, false, false, BonusPoisonEffect, POISONDAMAGECOLOR, false, null, "Poison");
                    }
                    if(BonusDiseaseWeapon){
                        riposteTarget.DamageDealt(this, BonusDiseaseEffect, false, false, BonusDiseaseEffect, DISEASEDAMAGECOLOR, false, null, "Disease");
                    }
                    if(BonusMagicWeapon){
                        riposteTarget.DamageDealt(this, BonusMagicEffect, false, false, BonusMagicEffect, MAGICDAMAGECOLOR, false, null, "Magic");
                    }
					if(BonusLeechWeapon){
                	    riposteTarget.DamageDealt(this, BonusLeechEffect, false, false, BonusLeechEffect, MAGICDAMAGECOLOR, false, null, "Leech");
                	    cur_hp = cur_hp + BonusLeechEffect;
						if(cur_hp > max_hp){
							cur_hp = max_hp;
						}
                	}
                    if(ThreatMod){
                        threat = (int)(threat * ThreatModifier);
                    }
            	    if(dmgDealt){
						string magicString = normalHithexColor;
						if(wasCrit){
							magicString = criticalHitHexColor;
						}
            	        riposteTarget.DamageDealt(this, value, wasCrit, false, threat, magicString, true, weaponType, "RiposteMH");
            	    }
					int valueOH = 0;
					bool dmgDealtOH = true;
					if(duelWielding){
                        bool wasCritOH = false;
						if(IsCriticalHit()){
							wasCritOH = true;
						}
						if(riposteTarget.mobType == "Undead"){
							if(IsCriticalHitUndead()){
								wasCritOH = true;
							}
						}	
						valueOH = StatAsset.Instance.GetAutoAttackOffhand(this, riposteTarget, wasCritOH);
						bool missedTargetOH = false;
						(bool NPProcOH, float missCalcOH) = GetHitChance();
						if(!pc){
							missCalcOH += 5f;
						} else {
							//get char weapon skill
							//for now we will just use 95%
							missCalcOH += 5f;//sudo number for weapon
							//get skill from weapon, which will be the skill / level of character * 10, so if skill is 50 and you are level 6 the chance to hit would be
							// 90 + (5 * (50/(6 * 10))) == 94.16% hit chance
							//cap 100% hit chance
						}
						int randomNumberOH = UnityEngine.Random.Range(1, 101);
        				// Compare the random number to the threshold
        				if (randomNumberOH > missCalcOH)
        				{
							missedTargetOH = true;
							dmgDealtOH = false;
						riposteTarget.AddRpcCall(null, null, false, true, "Miss", null, false, ownerName, targetName, this, riposteTarget, Vector2.zero);
            	    	    
							//riposteTarget.RpcCastingSpell("Miss", null);
        				}
        				else
        				{
        				    missedTargetOH = false;
        				}
						
						bool dodgedOH = StatAsset.Instance.GetDodge(riposteTarget);
            	    	if(dodgedOH && !missedTargetOH){
							dmgDealtOH = false;
						riposteTarget.AddRpcCall(null, null, false, true, "Dodge", null, false, ownerName, targetName, this, riposteTarget, Vector2.zero);
            	    	    
							//riposteTarget.RpcCastingSpell("Dodge", null);
            	    	}
            	    	bool parriedOH = StatAsset.Instance.GetParry(riposteTarget);
            	    	if(parriedOH && !dodgedOH && !missedTargetOH){
							dmgDealtOH = false;
							if(pcChecktarget){
								riposteTarget.AddRpcCall(null, null, false, true, "Parry", riposteTarget.weaponType, false, ownerName, targetName, this, riposteTarget, Vector2.zero);
								//riposteTarget.RpcCastingSpell("Parry", pc.weaponType);
							} else {
								riposteTarget.AddRpcCall(null, null, false, true, "Parry", null, false, ownerName, targetName, this, riposteTarget, Vector2.zero);
								//riposteTarget.RpcCastingSpell("Parry", null);
							}
            	    	}
						bool blockedAllOH = false;
            	    	bool blockedOH = StatAsset.Instance.GetBlock(riposteTarget);
						if(blockSpellTriggered){
							blockedOH = true;
						}
            	    	if(!parriedOH && !dodgedOH && blockedOH && !missedTargetOH){
            	    	    valueOH -= riposteTarget.shieldValue;
							if(blockSpellTriggered){
								valueOH = 0;
							}
            	    	    if(valueOH <= 0){
								blockedAllOH = true;
								dmgDealtOH = false;
						riposteTarget.AddRpcCall(null, null, false, true, "Blocked", null, false, ownerName, targetName, this, riposteTarget, Vector2.zero);
            	    	        
								//riposteTarget.RpcCastingSpell("Blocked", null);
            	    	    }
            	    	}
            	    	if(valueOH <= 0 && !parriedOH && !dodgedOH && !blockedAllOH && !missedTargetOH){
							dmgDealtOH = false;
			riposteTarget.AddRpcCall("absorbed", null, false, false, null, null, false, ownerName, targetName, this, riposteTarget, Vector2.zero);
            	    	    //riposteTarget.RpcSpawnPopUpAbsorbed();
            	    	}
                    	int threatOH = valueOH;
						if(ThreatMod){
							threatOH = (int)(threatOH * ThreatModifier);
                    	}
						if(dmgDealtOH){
							string magicString = normalHithexColor;
							if(wasCritOH){
								magicString = criticalHitHexColor;
							}
            	        	riposteTarget.DamageDealt(this, valueOH, wasCritOH, false, threatOH, magicString, true, weaponTypeOH, "RiposteOH");
							if(riposteTarget.thornValue > 0){
								DamageDealt(riposteTarget, riposteTarget.thornValue, false, true, riposteTarget.thornValue, MAGICDAMAGECOLOR, false, null, "Thorns");
							}
							//string _attacker = string.Empty;
                			//string _defender = string.Empty;
                			//string _content = "attacked";
                			//string _amount = valueOH.ToString();
                			//PlayerCharacter _pcAttackerCheck = GetComponent<PlayerCharacter>();
                			//Mob _mobAttackerCheck = GetComponent<Mob>();
                			//PlayerCharacter _pcDefenderCheck = riposteTarget.GetComponent<PlayerCharacter>();
                			//Mob _mobDefenderCheck = riposteTarget.GetComponent<Mob>();
                			//if(_pcAttackerCheck != null){
                			//    _attacker = _pcAttackerCheck.CharacterName;
                			//} else {
                			//    _attacker = _mobAttackerCheck.NAME;
                			//}
                			//if(_pcDefenderCheck != null){
                			//    _defender = _pcDefenderCheck.CharacterName;
                			//} else {
                			//    _defender = _mobDefenderCheck.NAME;
                			//}
							//int _type = 103; 
							//int _element = 5;
                			//CombatLogNet _cNet = new CombatLogNet(_attacker, _defender, _content, _amount, _type, _element, wasCritOH);
                			//RpcSpawnCombatLog(_cNet);
							//StartCoroutine(DelayedWeaponAttackSound(wasCritOH));
						}
                    }
					string attacker = string.Empty;
                	string defender = string.Empty;
                	string content = "attacked";
					if(!dmgDealt){
						value = 0;
					}
					if(!dmgDealtOH){
						
					} else {
						value += valueOH;
					}
                	//string amount = value.ToString();
                	//PlayerCharacter pcAttackerCheck = GetComponent<PlayerCharacter>();
                	//Mob mobAttackerCheck = GetComponent<Mob>();
                	//PlayerCharacter pcDefenderCheck = riposteTarget.GetComponent<PlayerCharacter>();
                	//Mob mobDefenderCheck = riposteTarget.GetComponent<Mob>();
                	//if(pcAttackerCheck != null){
                	//    attacker = pcAttackerCheck.CharacterName;
                	//} else {
                	//    attacker = mobAttackerCheck.NAME;
                	//}
                	//if(pcDefenderCheck != null){
                	//    defender = pcDefenderCheck.CharacterName;
                	//} else {
                	//    defender = mobDefenderCheck.NAME;
                	//}
					//int type = 100; 
					//int element = 5;
                	//CombatLogNet cNet = new CombatLogNet(attacker, defender, content, amount, type, element, wasCrit);
                	//RpcSpawnCombatLog(cNet);
					return;
		    	}
			}
	    }
	}
	IEnumerator DelayedWeaponAttackSound(bool wasCrit){
		yield return new WaitForSeconds(.33f);
		//ClientAttackSound(weaponTypeOH, wasCrit);
	}
	IEnumerator DelayedWeaponAttackSoundOH(bool wasCrit){
		yield return new WaitForSeconds(.33f);
		//ClientAttackSound(weaponTypeOH, wasCrit);
	}
	
	[ClientRpc]
	 public void RpcCastingSpell(string spell)
    {
		if(spell == "Charge"){
			return;
		}
		string spellType = GetSpellType(spell);
		//"iceMagic", "fireMagic", "magicMagic", //wizard
		//"healingMagic", "holyBolt", "undeadProtection", "shieldBash", "turnUndead",//priest
		//"mesmerizeCast", "gravityStun", "hasteCast",//enchanter
		//"aimedCast", "bandageCast", "doubleShot", "IDEnemy", "dash", "sleep", "naturesPrecision",//archer
		//"chargeCast", "chargeStrike", "shieldBash", "heavySwing", "criticalStrike", "protect", "taunt", "tankStance", "blockCast", "riposte", "doubleSwing", //fighter
		//"shuriken", "backstab", "tendonSlice", "applyPoison", "blind", "doubleAttack", "steal" //rogue
		//"Block", "Dodge", "Parry",
		//wizard
		//bools
		animator = GetComponent<Animator>();

		if(spellType == "fireMagic"){
        	animator.SetBool("fireMagic", true);
		}
		if(spellType == "iceMagic"){
        	animator.SetBool("iceMagic", true);
		}
		if(spellType == "magicMagic"){
        	animator.SetBool("magicMagic", true);
		}
		//priest
		//bools
		if(spellType == "healingMagic"){
        	animator.SetBool("healingMagic", true);
		}
		if(spellType == "holyBolt"){
        	animator.SetBool("holyBolt", true);
		}
		if(spellType == "undeadProtection"){
        	animator.SetBool("undeadProtection", true);
		}
		if(spellType == "turnUndead"){
        	animator.SetBool("turnUndead", true);
		}
		//triggers
		if(spellType == "shieldBash"){
        	animator.SetTrigger("shieldBash");
		}
		//archer
		//bools
		if(spellType == "aimedCast"){
        	animator.SetBool("aimedCast", true);
		}
		if(spellType == "IDEnemy"){
        	animator.SetBool("IDEnemy", true);
		}
		if(spellType == "doubleShot"){
        	animator.SetBool("doubleShot", true);
		}
		if(spellType == "naturesPrecision"){
        	animator.SetBool("naturesPrecision", true);
		}
		if(spellType == "sleep"){
        	animator.SetBool("sleep", true);
		}
		//triggers
		if(spellType == "dash"){
        	animator.SetTrigger("dash");
		}
		if(spellType == "bandageCast"){
        	animator.SetTrigger("bandageCast");
		}
		//fighter
		//bools
		if(spellType == "taunt"){
        	animator.SetBool("taunt", true);
		}
		if(spellType == "heavySwing"){
        	animator.SetBool("heavySwing", true);
		}
		if(spellType == "throwStone"){
        	animator.SetBool("throwStone", true);
		}
		if(spellType == "tankStance"){
        	animator.SetBool("tankStance", true);
		}
		if(spellType == "blockCast"){
        	animator.SetBool("blockCast", true);
		}
		//triggers
		if(spellType == "chargeCast"){
        	animator.SetTrigger("chargeCast");
		}
		if(spellType == "chargeStrike"){
        	animator.SetTrigger("chargeStrike");
		}
		if(spellType == "criticalStrike"){
        	animator.SetTrigger("criticalStrike");
		}
		if(spellType == "protect"){
        	animator.SetTrigger("protect");
		}
		if(spellType == "riposte"){
        	animator.SetTrigger("riposte");
		}
		if(spellType == "doubleSwing"){
        	animator.SetTrigger("doubleSwing");
		}
		//enchanter
		//bools
		if(spellType == "mesmerizeCast"){
        	animator.SetBool("mesmerizeCast", true);
		}
		if(spellType == "hasteCast"){
        	animator.SetBool("hasteCast", true);
		}
		if(spellType == "gravityStun"){
        	animator.SetBool("gravityStun", true);
		}
		//rogue
		//bools
		if(spellType == "shuriken"){
        	animator.SetBool("shuriken", true);
		}
		if(spellType == "applyPoison"){
        	animator.SetBool("applyPoison", true);
		}
		if(spellType == "blind"){
        	animator.SetBool("blind", true);
		}
		if(spellType == "steal"){
        	animator.SetBool("steal", true);
		}
		if(spellType == "tendonSlice"){
        	animator.SetBool("tendonSlice", true);
		}
		//if(spellType == "sneak"){
        //	animator.SetBool("sneak", true);
		//}
		//triggers
		if(spellType == "backstab"){
        	animator.SetTrigger("backstab");
		}
		if(spellType == "doubleAttack"){
        	animator.SetTrigger("doubleAttack");
		}
		if(spellType == "rush"){
        	animator.SetTrigger("rush");
		}
    }
    public void AnimationAndSound(string spell, string weaponType, MovingObject attacker)
    {
		if(spell == "Charge"){
			return;
		}
		//print("spell name we were told to cast was " + spell);
		string spellType = GetSpellType(spell);
		//print("spellType we were told to cast was " + spellType);

		//"iceMagic", "fireMagic", "magicMagic", //wizard
		//"healingMagic", "holyBolt", "undeadProtection", "shieldBash", "turnUndead",//priest
		//"mesmerizeCast", "gravityStun", "hasteCast",//enchanter
		//"aimedCast", "bandageCast", "doubleShot", "IDEnemy", "dash", "sleep", "naturesPrecision",//archer
		//"chargeCast", "chargeStrike", "shieldBash", "heavySwing", "criticalStrike", "protect", "taunt", "tankStance", "blockCast", "riposte", "doubleSwing", //fighter
		//"shuriken", "backstab", "tendonSlice", "applyPoison", "blind", "doubleAttack", "steal" //rogue
		//"Block", "Dodge", "Parry",
		if(weaponType == "NFT Sword"){
			weaponType = "Sword";
		}
		if(weaponType == "NFT Bow"){
			weaponType = "Bow";
		}
		if(weaponType == "NFT Greatspear"){
			weaponType = "Greatspear";
		}
		if(weaponType == "NFT Axe"){
			weaponType = "Axe";
		}
		if(weaponType == "NFT Mace"){
			weaponType = "Mace";
		}
		if(weaponType == "NFT Spear"){
			weaponType = "Spear";
		}
		if(weaponType == "NFT Staff"){
			weaponType = "Staff";
		}
		if(weaponType == "NFT Greathammer"){
			weaponType = "Greathammer";
		}
		if(weaponType == "NFT Dagger"){
			weaponType = "Dagger";
		}
		if(weaponType == "NFT Greataxe"){
			weaponType = "Greataxe";
		}
		if(weaponType == "NFT Greatsword"){
			weaponType = "Greatsword";
		}
		//wizard
		//bools
		animator = GetComponent<Animator>();

		if(spellType == "fireMagic"){
        	animator.SetBool("fireMagic", true);
		}
		if(spellType == "iceMagic"){
        	animator.SetBool("iceMagic", true);
		}
		if(spellType == "magicMagic"){
        	animator.SetBool("magicMagic", true);
		}
		//priest
		//bools
		if(spellType == "healingMagic"){
        	animator.SetBool("healingMagic", true);
		}
		if(spellType == "holyBolt"){
        	animator.SetBool("holyBolt", true);
		}
		if(spellType == "undeadProtection"){
        	animator.SetBool("undeadProtection", true);
		}
		if(spellType == "turnUndead"){
        	animator.SetBool("turnUndead", true);
		}
		//triggers
		if(spellType == "shieldBash"){
        	animator.SetTrigger("shieldBash");
		}
		//archer
		//bools
		if(spellType == "aimedCast"){
        	animator.SetBool("aimedCast", true);
		}
		if(spellType == "IDEnemy"){
        	animator.SetBool("IDEnemy", true);
		}
		if(spellType == "doubleShot"){
        	animator.SetBool("doubleShot", true);
		}
		if(spellType == "naturesPrecision"){
        	animator.SetBool("naturesPrecision", true);
		}
		if(spellType == "sleep"){
        	animator.SetBool("sleep", true);
		}
		//triggers
		if(spellType == "dash"){
        	animator.SetTrigger("dash");
		}
		if(spellType == "bandageCast"){
        	animator.SetTrigger("bandageCast");
		}
		//fighter
		//bools
		if(spellType == "taunt"){
        	animator.SetBool("taunt", true);
		}
		if(spellType == "heavySwing"){
        	animator.SetBool("heavySwing", true);
		}
		if(spellType == "throwStone"){
        	animator.SetBool("throwStone", true);
		}
		if(spellType == "tankStance"){
        	animator.SetBool("tankStance", true);
		}
		if(spellType == "blockCast"){
        	animator.SetBool("blockCast", true);
		}
		//triggers
		if(spellType == "chargeCast"){
        	animator.SetTrigger("chargeCast");
		}
		if(spellType == "chargeStrike"){
        	animator.SetTrigger("chargeStrike");
		}
		if(spellType == "criticalStrike"){
        	animator.SetTrigger("criticalStrike");
		}
		if(spellType == "protect"){
        	animator.SetTrigger("protect");
		}
		if(spellType == "riposte"){
        	animator.SetTrigger("riposte");
		}
		if(spellType == "doubleSwing"){
        	animator.SetTrigger("doubleSwing");
		}
		//enchanter
		//bools
		if(spellType == "mesmerizeCast"){
        	animator.SetBool("mesmerizeCast", true);
		}
		if(spellType == "hasteCast"){
        	animator.SetBool("hasteCast", true);
		}
		if(spellType == "gravityStun"){
        	animator.SetBool("gravityStun", true);
		}
		//rogue
		//bools
		if(spellType == "shuriken"){
        	animator.SetBool("shuriken", true);
		}
		if(spellType == "applyPoison"){
        	animator.SetBool("applyPoison", true);
		}
		if(spellType == "blind"){
        	animator.SetBool("blind", true);
		}
		if(spellType == "steal"){
        	animator.SetBool("steal", true);
		}
		if(spellType == "tendonSlice"){
        	animator.SetBool("tendonSlice", true);
		}
		//if(spellType == "sneak"){
        //	animator.SetBool("sneak", true);
		//}
		//triggers
		if(spellType == "backstab"){
        	animator.SetTrigger("backstab");
		}
		if(spellType == "doubleAttack"){
        	animator.SetTrigger("doubleAttack");
		}
		if(spellType == "rush"){
        	animator.SetTrigger("rush");
		}
		if(spell == "Parry" || spell == "Dodge" || spell == "Blocked" || spell == "Miss" || spell == "Eat" || spell == "Drink"){
			AudioMgr audio = GetComponent<AudioMgr>();
			
			PlayerCharacter pc = GetComponent<PlayerCharacter>();
			if(spell == "Drink"){
				audio.PlaySound("Drink");
				if(pc){
			    	animator.SetTrigger("drink");
				}
				return;
			}
			if(spell == "Eat"){
				audio.PlaySound("Eat");
				if(pc){
			    	animator.SetTrigger("eat");
				}
				return;
			}

			GameObject spawnTextPopUp = StatAsset.Instance.GetObject(this);//Instantiate(PopUpTextPrefab, transform.position, Quaternion.identity);
        	AbilityPopUp abilityDisplay = spawnTextPopUp.GetComponent<AbilityPopUp>();
			if(spell == "Miss"){
				if(attacker != null){
					if(attacker.HasTriggerParameter("Attack")){
						//print($"Sending Attack for this unit");
	    			    attacker.GetAnimator().SetTrigger("Attack");
					}
				}
				audio.PlaySound("Miss");
        		abilityDisplay.AbilityPopUpBuild("Miss", normalHithexColor);
			}
			if(spell == "Dodge"){
				audio.PlaySound("Dodge");
				if(attacker != null){
					if(attacker.HasTriggerParameter("Attack")){
						//print($"Sending Attack for this unit");
	    			    attacker.GetAnimator().SetTrigger("Attack");
					}
				}
        		abilityDisplay.AbilityPopUpBuild("Dodged", normalHithexColor);
				if(pc){
					//if(pc.ClassType == "Fighter"){
			        	animator.SetTrigger("Dodge");
					//}
				}
			}
			if(spell == "Parry"){
				if(pc){
					if(attacker != null){
						if(attacker.HasTriggerParameter("Attack")){
							//print($"Sending Attack for this unit");
	    				    attacker.GetAnimator().SetTrigger("Attack");
						}
					}
					if(!string.IsNullOrEmpty(weaponType)){
						string parrySoundName = weaponType + "Parry";
						audio.PlaySound(parrySoundName);
					}
					if(pc.ClassType == "Fighter"){
			        	animator.SetTrigger("Parry");
					}
				} else {
					audio.PlaySound("Parry");
				}
        		abilityDisplay.AbilityPopUpBuild("Parried", normalHithexColor);
			}
			if(spell == "Blocked"){
				if(attacker != null){
					if(attacker.HasTriggerParameter("Attack")){
						//print($"Sending Attack for this unit");
	    			    attacker.GetAnimator().SetTrigger("Attack");
					}
				}
				audio.PlaySound("Block");
        		abilityDisplay.AbilityPopUpBuild("Blocked", normalHithexColor);
				if(pc){
					if(pc.ClassType == "Fighter"){
			        	animator.SetTrigger("Block");
					}
				}
			}
		}
    }
	bool HasBoolParameter(Animator animator, string paramName)
	{
	    foreach (AnimatorControllerParameter param in animator.parameters)
	    {
	        if (param.name == paramName && param.type == AnimatorControllerParameterType.Bool)
	        {
	            return true;
	        }
	    }
	    return false;
	}
	// Method to check if an Animator has a specific trigger parameter
	bool HasTriggerParameter(string paramName)
	{
		if (animator == null){
			animator = GetComponent<Animator>();
			if(animator == null){
				print("Animator is null in HasTriggerParameter method.");
    	    	return false;
			}
    	}
	    foreach (AnimatorControllerParameter param in animator.parameters){
	        if (param.name == paramName && param.type == AnimatorControllerParameterType.Trigger){
	            return true;
	        }
	    }
	    return false;
	}
	[ClientRpc]
	public void RpcCancelOnNodeDeath()
	{
		print("Starting rpc cancel cast animation");
		PlayerCharacter pc = GetComponent<PlayerCharacter>();
		if(pc == null){
			return;
		}
		string Class = pc.ClassType;
		List<string> parameters = new List<string> {
			"skinning", "mining", "foraging", "chopping", "prospecting"		
		};
	    foreach (string parameterName in parameters)
	    {
	        if (HasBoolParameter(animator, parameterName))
	        {
	            animator.SetBool(parameterName, false);
	        }
	        else
	        {
	            Debug.LogWarning($"Animator does not have a boolean parameter named '{parameterName}'");
	        }
	    }
	    // Reset triggers
	    //List<string> triggers = new List<string> {
		//	"endHarvest"
		//};
	    //foreach (string triggerName in triggers)
	    //{
	    //    if (HasTriggerParameter(triggerName))
	    //    {
	    //        animator.SetTrigger(triggerName);
	    //    }
	    //    else
	    //    {
	    //        Debug.LogWarning($"Animator does not have a trigger named '{triggerName}'");
	    //    }
	    //}
		print("Ending rpc cancel cast animation");

	}
	public void EndSpellCasted()
	{
		CancelCast.Invoke(this);
		print("Starting rpc cancel cast animation");
		PlayerCharacter pc = GetComponent<PlayerCharacter>();
		if(pc == null){
			return;
		}
		string Class = pc.ClassType;
		List<string> parameters = new List<string> {
			"skinning", "mining", "foraging", "chopping", "prospecting"		
		};
		if(Class == "Fighter"){
			parameters.Add("taunt");
			parameters.Add("heavySwing");
			parameters.Add("throwStone");
			parameters.Add("tankStance");
			parameters.Add("blockCast");
		}
		if(Class == "Priest"){
			parameters.Add("healingMagic");
			parameters.Add("holyBolt");
			parameters.Add("undeadProtection");
			parameters.Add("turnUndead");
		}
		if(Class == "Rogue"){
			parameters.Add("shuriken");
			parameters.Add("applyPoison");
			parameters.Add("blind");
			parameters.Add("steal");
			parameters.Add("tendonSlice");
			parameters.Add("sneak");
		}
		if(Class == "Wizard"){
			parameters.Add("fireMagic");
			parameters.Add("iceMagic");
			parameters.Add("magicMagic");
		}
		if(Class == "Archer"){
			parameters.Add("aimedCast");
			parameters.Add("IDEnemy");
			parameters.Add("doubleShot");
			parameters.Add("naturesPrecision");
			parameters.Add("sleep");
		}
		if(Class == "Enchanter"){
			parameters.Add("mesmerizeCast");
			parameters.Add("hasteCast");
			parameters.Add("gravityStun");
		}
	    foreach (string parameterName in parameters)
	    {
	        if (HasBoolParameter(animator, parameterName))
	        {
	            animator.SetBool(parameterName, false);
	        }
	        else
	        {
	            Debug.LogWarning($"Animator does not have a boolean parameter named '{parameterName}'");
	        }
	    }
	    // Reset triggers
	    List<string> triggers = new List<string> {
			"endCast",
			"endHarvest"
		};
	    foreach (string triggerName in triggers)
	    {
	        if (HasTriggerParameter(triggerName))
	        {
	            animator.SetTrigger(triggerName);
	        }
	        else
	        {
	            Debug.LogWarning($"Animator does not have a trigger named '{triggerName}'");
	        }
	    }
		print("Ending rpc cancel cast animation");

	}
	[ClientRpc]
	public void RpcCancelCastAnimation()
	{
		CancelCast.Invoke(this);
		//print("Starting rpc cancel cast animation");
		PlayerCharacter pc = GetComponent<PlayerCharacter>();
		if(pc == null){
			return;
		}
		string Class = pc.ClassType;
		List<string> parameters = new List<string> {
			"skinning", "mining", "foraging", "chopping", "prospecting"		
		};
		if(Class == "Fighter"){
			parameters.Add("taunt");
			parameters.Add("heavySwing");
			parameters.Add("throwStone");
			parameters.Add("tankStance");
			parameters.Add("blockCast");
		}
		if(Class == "Priest"){
			parameters.Add("healingMagic");
			parameters.Add("holyBolt");
			parameters.Add("undeadProtection");
			parameters.Add("turnUndead");
		}
		if(Class == "Rogue"){
			parameters.Add("shuriken");
			parameters.Add("applyPoison");
			parameters.Add("blind");
			parameters.Add("steal");
			parameters.Add("tendonSlice");
			parameters.Add("sneak");
		}
		if(Class == "Wizard"){
			parameters.Add("fireMagic");
			parameters.Add("iceMagic");
			parameters.Add("magicMagic");
		}
		if(Class == "Archer"){
			parameters.Add("aimedCast");
			parameters.Add("IDEnemy");
			parameters.Add("doubleShot");
			parameters.Add("naturesPrecision");
			parameters.Add("sleep");
		}
		if(Class == "Enchanter"){
			parameters.Add("mesmerizeCast");
			parameters.Add("hasteCast");
			parameters.Add("gravityStun");
		}
	    foreach (string parameterName in parameters)
	    {
	        if (HasBoolParameter(animator, parameterName))
	        {
	            animator.SetBool(parameterName, false);
	        }
	        else
	        {
	            Debug.LogWarning($"Animator does not have a boolean parameter named '{parameterName}'");
	        }
	    }
	    // Reset triggers
	    List<string> triggers = new List<string> {
			"endCast",
			"endHarvest"
		};
	    foreach (string triggerName in triggers)
	    {
	        if (HasTriggerParameter(triggerName))
	        {
	            animator.SetTrigger(triggerName);
	        }
	        else
	        {
	            Debug.LogWarning($"Animator does not have a trigger named '{triggerName}'");
	        }
	    }
		//print("Ending rpc cancel cast animation");

	}
	public void NoHitsLeftOnNode()
	{
		print("Starting rpc cancel cast animation");
		PlayerCharacter pc = GetComponent<PlayerCharacter>();
		if(pc == null){
			return;
		}
		string Class = pc.ClassType;
		List<string> parameters = new List<string> {
			"skinning", "mining", "foraging", "chopping", "prospecting"		
		};
		//List<string> parameters = new List<string>();
		if(Class == "Fighter"){
			parameters.Add("taunt");
			parameters.Add("heavySwing");
			parameters.Add("throwStone");
			parameters.Add("tankStance");
			parameters.Add("blockCast");
		}
		if(Class == "Priest"){
			parameters.Add("healingMagic");
			parameters.Add("holyBolt");
			parameters.Add("undeadProtection");
			parameters.Add("turnUndead");
		}
		if(Class == "Rogue"){
			parameters.Add("shuriken");
			parameters.Add("applyPoison");
			parameters.Add("blind");
			parameters.Add("steal");
			parameters.Add("tendonSlice");
			parameters.Add("sneak");
		}
		if(Class == "Wizard"){
			parameters.Add("fireMagic");
			parameters.Add("iceMagic");
			parameters.Add("magicMagic");
		}
		if(Class == "Archer"){
			parameters.Add("aimedCast");
			parameters.Add("IDEnemy");
			parameters.Add("doubleShot");
			parameters.Add("naturesPrecision");
			parameters.Add("sleep");
		}
		if(Class == "Enchanter"){
			parameters.Add("mesmerizeCast");
			parameters.Add("hasteCast");
			parameters.Add("gravityStun");
		}
	    foreach (string parameterName in parameters)
	    {
	        if (HasBoolParameter(animator, parameterName))
	        {
	            animator.SetBool(parameterName, false);
	        }
	        else
	        {
	            Debug.LogWarning($"Animator does not have a boolean parameter named '{parameterName}'");
	        }
	    }
	    // Reset triggers
	    List<string> triggers = new List<string> {
			"endCast",
			"endHarvest"
		};
	    foreach (string triggerName in triggers)
	    {
	        if (HasTriggerParameter(triggerName))
	        {
	            animator.SetTrigger(triggerName);
	        }
	        else
	        {
	            Debug.LogWarning($"Animator does not have a trigger named '{triggerName}'");
	        }
	    }
		print("Ending rpc cancel cast animation");

	}
	public void NonRPCCancelCastAnimation()
	{
		CancelCast.Invoke(this);
		print("Starting rpc cancel cast animation");
		PlayerCharacter pc = GetComponent<PlayerCharacter>();
		if(pc == null){
			return;
		}
		string Class = pc.ClassType;
		//List<string> parameters = new List<string> {
		//	"skinning", "mining", "foraging", "chopping", "prospecting"		
		//};
		List<string> parameters = new List<string>();
		if(Class == "Fighter"){
			parameters.Add("taunt");
			parameters.Add("heavySwing");
			parameters.Add("throwStone");
			parameters.Add("tankStance");
			parameters.Add("blockCast");
		}
		if(Class == "Priest"){
			parameters.Add("healingMagic");
			parameters.Add("holyBolt");
			parameters.Add("undeadProtection");
			parameters.Add("turnUndead");
		}
		if(Class == "Rogue"){
			parameters.Add("shuriken");
			parameters.Add("applyPoison");
			parameters.Add("blind");
			parameters.Add("steal");
			parameters.Add("tendonSlice");
			parameters.Add("sneak");
		}
		if(Class == "Wizard"){
			parameters.Add("fireMagic");
			parameters.Add("iceMagic");
			parameters.Add("magicMagic");
		}
		if(Class == "Archer"){
			parameters.Add("aimedCast");
			parameters.Add("IDEnemy");
			parameters.Add("doubleShot");
			parameters.Add("naturesPrecision");
			parameters.Add("sleep");
		}
		if(Class == "Enchanter"){
			parameters.Add("mesmerizeCast");
			parameters.Add("hasteCast");
			parameters.Add("gravityStun");
		}
	    foreach (string parameterName in parameters)
	    {
	        if (HasBoolParameter(animator, parameterName))
	        {
	            animator.SetBool(parameterName, false);
	        }
	        else
	        {
	            Debug.LogWarning($"Animator does not have a boolean parameter named '{parameterName}'");
	        }
	    }
		/*
	    // Reset triggers
	    List<string> triggers = new List<string> {
			"endCast",
			"endHarvest"
		};
	    foreach (string triggerName in triggers)
	    {
	        if (HasTriggerParameter(triggerName))
	        {
	            animator.SetTrigger(triggerName);
	        }
	        else
	        {
	            Debug.LogWarning($"Animator does not have a trigger named '{triggerName}'");
	        }
	    }
		*/
		print("Ending rpc cancel cast animation");

	}
	[ClientRpc]
    private void ClientSetTrigger(string trigger)
    {
		if(HasTriggerParameter(trigger)){
	        animator.SetTrigger(trigger);
		}
    }
	//[ClientRpc]
   	private void ClientAttackSound(string weaponType, bool criticalStrike, int attackType)
    {
		PlayerCharacter pcCheck = GetComponent<PlayerCharacter>();
		if( pcCheck){
			if(attackType == 1){
				if(criticalStrike){
					if(HasTriggerParameter("CriticalAttack")){
						//print($"Sending Critical Attack for this unit");
	    			    animator.SetTrigger("CriticalAttack");
					}
				} else {
					if(HasTriggerParameter("Attack")){
						//print($"Sending Attack for this unit");
	    			    animator.SetTrigger("Attack");
					}
				}
			}
			if(attackType == 2){
				if(HasTriggerParameter("DoubleAttack")){
					//print($"Sending Critical Attack for this unit");
	    		    animator.SetTrigger("DoubleAttack");
				}
			}
			if(attackType == 3){
				
			}
		}
		if(weaponType == "NFT Sword"){
			weaponType = "Sword";
		}
		if(weaponType == "NFT Bow"){
			weaponType = "Bow";
		}
		if(weaponType == "NFT Greatspear"){
			weaponType = "Greatspear";
		}
		if(weaponType == "NFT Axe"){
			weaponType = "Axe";
		}
		if(weaponType == "NFT Mace"){
			weaponType = "Mace";
		}
		if(weaponType == "NFT Spear"){
			weaponType = "Spear";
		}
		if(weaponType == "NFT Staff"){
			weaponType = "Staff";
		}
		if(weaponType == "NFT Greathammer"){
			weaponType = "Greathammer";
		}
		if(weaponType == "NFT Dagger"){
			weaponType = "Dagger";
		}
		if(weaponType == "NFT Greataxe"){
			weaponType = "Greataxe";
		}
		if(weaponType == "NFT Greatsword"){
			weaponType = "Greatsword";
		}
		AudioMgr sound = GetComponent<AudioMgr>();
		if(weaponType == "Sword"){
			if(criticalStrike){
				sound.PlaySound("SwordCrit");
			} else {
				sound.PlaySound("Sword");
			}
		}
		if(weaponType == "Axe"){
			if(criticalStrike){
				sound.PlaySound("AxeCrit");
			} else {
				sound.PlaySound("Axe");
			}
		}
		if(weaponType == "Dagger"){
			if(criticalStrike){
				sound.PlaySound("DaggerCrit");
			} else {
				sound.PlaySound("Dagger");
			}
		}
		if(weaponType == "Mace"){
			if(criticalStrike){
				sound.PlaySound("MaceCrit");
			} else {
				sound.PlaySound("Mace");
			}
		}
		if(weaponType == "Spear"){
			if(criticalStrike){
				sound.PlaySound("SpearCrit");
			} else {
				sound.PlaySound("Spear");
			}
		}
		if(weaponType == "Staff"){
			if(criticalStrike){
				sound.PlaySound("StaffCrit");
			} else {
				sound.PlaySound("Staff");
			}
		}
		//if(weaponType == "Bow"){
		//	if(criticalStrike){
		//		sound.PlaySound("BowCrit");
		//	} else {
		//		sound.PlaySound("Bow");
		//	}
		//}
		if(weaponType == "Greathammer"){
			if(criticalStrike){
				sound.PlaySound("GreathammerCrit");
			} else {
				sound.PlaySound("Greathammer");
			}
		}
		if(weaponType == "Greataxe"){
			if(criticalStrike){
				sound.PlaySound("GreataxeCrit");
			} else {
				sound.PlaySound("Greataxe");
			}
		}
		if(weaponType == "Greatsword"){
			if(criticalStrike){
				sound.PlaySound("GreatswordCrit");
			} else {
				sound.PlaySound("Greatsword");
			}
		}
		if(weaponType == "Greatspear"){
			if(criticalStrike){
				sound.PlaySound("GreatspearCrit");
			} else {
				sound.PlaySound("Greatspear");
			}
		}
		if(weaponType == "Bite"){
			sound.PlaySound("Bite");
		}
		if(weaponType == "Claw"){
			sound.PlaySound("Claw");
		}
		if(weaponType == "ClawBite"){
			sound.PlaySound("ClawBite");
		}
		if(weaponType == "ElectricAttack"){
			sound.PlaySound("ElectricAttack");
		}
		if(weaponType == "FireAttack"){
			sound.PlaySound("FireAttack");
		}
		if(weaponType == "Fist"){
			sound.PlaySound("Fist");
		}
    }
	public float GetStartSpeed(){
		return startingSpeed;
	}
	public float GetStartAcceleration(){
		return startingAcceleration;
	}
	string DirectionString = "East";
	public string GetFacingDirection(){
		return DirectionString;
	}
	public void SetFacingDirection(string newDirection){
		DirectionString = newDirection;
	}
	[Server]
	public void ServerMoveToTargetPosition(Vector3 targetPosition){
		if(agent.enabled){
			if(Stunned || Feared || Snared || Mesmerized)
			agent.isStopped = false;
			agent.ResetPath();
			agent.SetDestination(new Vector3(targetPosition.x, targetPosition.y, transform.position.z));
			ServerResetTimer();
		}
	}
	void ChangedDirection(Vector3 _lastPosition){
		Vector2 direction = new Vector2(transform.position.x - _lastPosition.x, transform.position.y - _lastPosition.y);
    	float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
    	string directionString = GetDirectionString(angle);
		if(DirectionString != directionString){
			DirectionString = directionString;
			//if(!isServer){
			//	SpriteRenderer sRend = GetComponent<SpriteRenderer>();
			//	bool rightFacing = DirectionString == "North" || DirectionString == "Northeast" || DirectionString == "East" || DirectionString == "Southeast";
    		//	// Flip the sprite if necessary
    		//	sRend.flipX = rightFacing;	
			//}
			//RpcSetDirectionFacing(directionString);
		}
		if(!isServer){
			SpriteRenderer sRend = GetComponent<SpriteRenderer>();
			bool rightFacing = direction.x > 0;//DirectionString == "North" || DirectionString == "Northeast" || DirectionString == "East" || DirectionString == "Southeast";
    		// Flip the sprite if necessary
    		sRend.flipX = rightFacing;	
			//Vector3 updateLocation = new Vector3(Mathf.FloorToInt(transform.position.x) + 0.5f, Mathf.FloorToInt(transform.position.y) + 0.5f, 0);
        	FogUpdateTest();
		}
	}
	[ClientRpc]
	public void RpcSetDirectionFacing(string directionString){
		SpriteRenderer sRend = GetComponent<SpriteRenderer>();
		DirectionString = directionString;
		bool rightFacing = directionString == "North" || directionString == "Northeast" || directionString == "East" || directionString == "Southeast";
    	// Flip the sprite if necessary
    	sRend.flipX = rightFacing;	
	}
	public string GetDirectionString(float angle) {
	    if(angle < 0) angle += 360; // Ensure the angle is in the range [0, 360)
	    if(angle >= 337.5f || angle < 22.5f) return "East";
	    if(angle >= 22.5f && angle < 67.5f) return "Northeast";
	    if(angle >= 67.5f && angle < 112.5f) return "North";
	    if(angle >= 112.5f && angle < 157.5f) return "Northwest";
	    if(angle >= 157.5f && angle < 202.5f) return "West";
	    if(angle >= 202.5f && angle < 247.5f) return "Southwest";
	    if(angle >= 247.5f && angle < 292.5f) return "South";
	    if(angle >= 292.5f && angle < 337.5f) return "Southeast";
	    return "Unknown"; // Should never reach here
	}
public bool IsBehindAnotherObject(Vector3 otherObjectPosition, string otherObjectDirectionString) {
    // Determine if this object is behind the other object based on the facing direction
    switch (otherObjectDirectionString) {
        case "North":
			//if(DirectionString == "North" || DirectionString == "Northeast" || DirectionString == "Northwest"){
	            return transform.position.y < otherObjectPosition.y;
			//} else {
			//	return false;
			//}
        case "South":
			//if(DirectionString == "South" || DirectionString == "Southeast" || DirectionString == "Southwest"){
            	return transform.position.y > otherObjectPosition.y;
			//} else {
			//	return false;
			//}
        case "East":
			//if(DirectionString == "East" || DirectionString == "Southeast" || DirectionString == "Northeast"){
            	return transform.position.x < otherObjectPosition.x;
			//} else {
			//	return false;
			//}
        case "West":
			//if(DirectionString == "West" || DirectionString == "Northwest" || DirectionString == "Southwest"){
            	return transform.position.x > otherObjectPosition.x;
			//} else {
			//	return false;
			//}
        case "Northeast":
			//if(DirectionString == "Northeast" || DirectionString == "North" || DirectionString == "East"){
            	return (transform.position.x < otherObjectPosition.x) || (transform.position.y < otherObjectPosition.y);
			//} else {
			//	return false;
			//}
        case "Northwest":
			//if(DirectionString == "Northwest" || DirectionString == "North" || DirectionString == "West"){
            	return (transform.position.x > otherObjectPosition.x) || (transform.position.y < otherObjectPosition.y);
			//} else {
			//	return false;
			//}
        case "Southeast":
			//if(DirectionString == "Southeast" || DirectionString == "South" || DirectionString == "East"){
            	return (transform.position.x < otherObjectPosition.x) || (transform.position.y > otherObjectPosition.y);
			//} else {
			//	return false;
			//}
        case "Southwest":
			//if(DirectionString == "Southwest" || DirectionString == "South" || DirectionString == "West"){
            	return (transform.position.x > otherObjectPosition.x) || (transform.position.y > otherObjectPosition.y);
			//} else {
			//	return false;
			//}
        default:
            return false; // Unknown direction
    }
}
/*
public bool IsBehindAnotherObject(Vector3 otherObjectPosition, string otherObjectDirectionString) {
    // Calculate the direction vector from this object to the other object
    Vector2 directionToOther = new Vector2(otherObjectPosition.x - transform.position.x, otherObjectPosition.y - transform.position.y);
    float angleToOther = Mathf.Atan2(directionToOther.y, directionToOther.x) * Mathf.Rad2Deg;
    
    // Get the angle range for the other object's facing direction
    float minAngle, midAngle, maxAngle;
    GetDirectionAngles(otherObjectDirectionString, out minAngle, out midAngle, out maxAngle);

    // Calculate the mid-angle of our direction
    float ourMidAngle;
    GetDirectionAngles(DirectionString, out _, out ourMidAngle, out _);

    // Calculate the relative angle between our facing direction and the other object
    float relativeAngle = (ourMidAngle - angleToOther + 360) % 360;

    // Consider behind if the relative angle is within 90-270 degrees range
    return relativeAngle > 90 && relativeAngle < 270;
}
*/
	[Server]
	public void ServerMoveFeared(Vector3 targetPosition){
		if(agent.enabled){
			if(Stunned || Snared || Mesmerized)
			agent.isStopped = false;
			agent.ResetPath();

			agent.SetDestination(new Vector3(targetPosition.x, targetPosition.y, transform.position.z));
			//moving = true;
			//isWalking = true;

			//ServerPrepareMovement();
		}
	}
	[Server]
	public void ServerMoveToTarget(MovingObject target){
		if(agent.enabled){
			agent.isStopped = false;
			agent.ResetPath();
			agent.SetDestination(target.transform.position);
		}
	}
	[Server]
	public void ServerMoveToAttackTarget(MovingObject target){
		if(agent.enabled){
			agent.isStopped = false;
			agent.ResetPath();
			agent.SetDestination(target.transform.position);
		}
	}
	void CheckTargetCircle(MovingObject moCheck){

		if(moCheck != this){
			////print($"{gameObject.name} was NOT moCheck!!");
			UnTargettedMO();
			return;
		}
		////print($"{gameObject.name} was ******** moCheck!!");
	}
	public string ProtectSpellName = string.Empty;
	public int protectLvl = 0;
	public MovingObject protectingMO;
	public bool GetProtected(){
		if(protectingMO == null){
			return false;
		}
		if(protectingMO.Dying){
			return false;
		}
		float protectChance = 50f + ((protectLvl - 1) * .5f);
		int roll = UnityEngine.Random.Range(1,101);
		if(roll < protectChance){
			return true;
		} else {
			return false;
		}
	}
	string UndeadProtectionSpellName = string.Empty;
	int UndeadProtectionLvl = 1;
	public int UndeadArmorBonus(){
		float UndeadBonus = 5f + ((UndeadProtectionLvl - 1) * 0.1f);
		if(UndeadProtectionRoutine == null){
			UndeadBonus = 0f;
		}
		return (int)UndeadBonus;
	}
	[Server]
	public void UndeadProtectionIncrease(string spell, int spellRank, float duration){

		if(UndeadProtectionRoutine != null){
			StopCoroutine(UndeadProtectionRoutine);
			ServerRemoveStatus("Undead Protection", UndeadProtectionSpellName, true);
			UndeadProtectionLvl = 1;
		}
		UndeadProtectionSpellName = spell;
		UndeadProtectionLvl = spellRank;
		UndeadProtectionRoutine = StartCoroutine(SetUndeadProtectionIncrease(duration));
	}
	public IEnumerator SetUndeadProtectionIncrease(float duration){
		string spellNameBeforeCancel = UndeadProtectionSpellName;
		float elapsedTime = 0;
    	while (elapsedTime < duration)
    	{
    	    elapsedTime += Time.deltaTime;
    	    yield return null;
    	}
		ServerRemoveStatus("Undead Protection", spellNameBeforeCancel, true);
		UndeadProtectionLvl = 1;
		UndeadProtectionSpellName = string.Empty;
		UndeadProtectionRoutine = null;
	}
	[Server]
	public void ServerProtectShield(float duration, string spell, int spellRank, MovingObject protector){
		string spellNameBeforeCancel = ProtectSpellName;
		if(ProtectRoutine != null){
			StopCoroutine(ProtectRoutine);
			ServerRemoveStatus("Protect", spellNameBeforeCancel, true);
			protectingMO = null;
			protectLvl = 0;
		}
		protectingMO = protector;
		ProtectSpellName = spell;
		protectLvl = spellRank;
		ProtectRoutine = StartCoroutine(SetProtectShield(duration));
	}
	public IEnumerator SetProtectShield(float duration){
		string spellNameBeforeCancel = ProtectSpellName;
		float elapsedTime = 0;
    	while (elapsedTime < duration)
    	{
    	    elapsedTime += Time.deltaTime;
    	    yield return null;
    	}
		ServerRemoveStatus("Protect", spellNameBeforeCancel, true);
		protectingMO = null;
		protectLvl = 0;
		ProtectRoutine = null;
	}
	float absorbSpellShield = 0f;
	string absorbSpellName = string.Empty;
	[Server]
	public void ServerAbsorbShield(float shield, float duration, string spell){
		string spellNameBeforeCancel = absorbSpellName;
		if(absorbRoutine != null){
			StopCoroutine(absorbRoutine);
			ServerRemoveStatus("Absorb", spellNameBeforeCancel, true);
		}
		absorbSpellName = spell;
		absorbRoutine = StartCoroutine(SetAbsorbShield(shield, duration));
	}
	public IEnumerator SetAbsorbShield(float shield, float duration){
		string spellNameBeforeCancel = absorbSpellName;
		absorbSpellShield = shield;
		float elapsedTime = 0;
    	while (elapsedTime < duration && absorbSpellShield > 0)
    	{
    	    elapsedTime += Time.deltaTime;
    	    yield return null;
    	}
		absorbSpellShield = 0f;
		ServerRemoveStatus("Absorb", spellNameBeforeCancel, true);
		absorbRoutine = null;
	}
	public Coroutine UndeadProtectionRoutine;
	public Coroutine ProtectRoutine;
	Coroutine absorbRoutine;

	float cooldownRecoveryRate = 1f;
	[Server]
	public void ServerRefreshEffectInitiated(float secondsOfRefresh){
		if(cooldownRefresh != null){
			StopCoroutine(cooldownRefresh);
		}
		cooldownRefresh = StartCoroutine(SetCooldownRefresh(secondsOfRefresh));
	}
	Coroutine cooldownRefresh;

	public IEnumerator SetCooldownRefresh(float duration){
		cooldownRecoveryRate = 2f;
		yield return new WaitForSeconds(duration);
		cooldownRecoveryRate = 1f;
		cooldownRefresh = null;
	}

	[Server]
	public void RunSetAbilityCooldownX(float duration, bool setup, string buttonPressed){
		print($"used button {buttonPressed} and spells are Q {SpellQ}, E {SpellE}, R {SpellR} and F {SpellF} with a duration of {duration} and a setup of {setup}");

		if(buttonPressed == "Q"){
			if(cooldownQ != null){
				StopCoroutine(cooldownQ);
			}
			cooldownQ = StartCoroutine(SetAbilityCoolDownQ(duration, setup));
		}	
		if(buttonPressed == "E"){
			if(cooldownE != null){
				StopCoroutine(cooldownE);
			}
			cooldownE = StartCoroutine(SetAbilityCoolDownE(duration, setup));
		}
		if(buttonPressed == "R"){
			if(cooldownR != null){
				StopCoroutine(cooldownR);
			}
			cooldownR = StartCoroutine(SetAbilityCoolDownR(duration, setup));
		}
		if(buttonPressed == "F"){
			if(cooldownF != null){
				StopCoroutine(cooldownF);
			}
			cooldownF = StartCoroutine(SetAbilityCoolDownF(duration, setup));
		}
	}
	Coroutine cooldownQ;
	Coroutine cooldownE;
	Coroutine cooldownR;
	Coroutine cooldownF;

	public IEnumerator SetAbilityCoolDownQ(float duration, bool setup){
		////print($"Starting Cooldown for {SpellQ} at {duration}");
        SpellQCoolDown = true;
        CooldownQ = duration;
		PlayerCharacter pc = GetComponent<PlayerCharacter>();
		if(pc && !curatorTM.GetDuelMode()){
			if(!setup){
				//duration += 1500f;
            	string dateTimeWithZone = DateTime.UtcNow.ToString("o");
        		DateTime timeNow = DateTime.Parse(dateTimeWithZone, null, System.Globalization.DateTimeStyles.RoundtripKind);
				DateTime updatedTime = timeNow.AddSeconds(duration); // Add duration in seconds

				//DateTime initialTime = DateTime.Parse(dateTimeWithZone); // Convert to DateTime object
				//DateTime updatedTime = initialTime.AddSeconds(duration); // Add duration in seconds
				string newDateTimeWithZone = updatedTime.ToString("o");
				print("NEW DATA TIME IS " + newDateTimeWithZone + " FOR OUR COOLDOWN SPELL Q");
				CharacterCooldownListItem coolie = (new CharacterCooldownListItem {
					SpellnameFull = SpellQ,
					Value = newDateTimeWithZone,
					//Position = "SPELLQ",
					//PKey = "COOLDOWNQ"
				});
				//print($"{coolie.SpellnameFull} is the spell we are putting on cooldown");
				pc.assignedPlayer.ServerCooldownSave(pc.CharID, coolie);
			}		
		}
        while (CooldownQ > 0f)
        {
            CooldownQ -= Time.deltaTime * cooldownRecoveryRate;
            yield return null;
        }
        CooldownQ = 0f;
        SpellQCoolDown = false;
    }
	[Server]
	public IEnumerator SetAbilityCoolDownE(float duration, bool setup){
		////print($"Starting Cooldown for {SpellE} at {duration}");
        SpellECoolDown = true;
        CooldownE = duration;
		PlayerCharacter pc = GetComponent<PlayerCharacter>();
		if(pc && !curatorTM.GetDuelMode()){
			if(!setup){
				string dateTimeWithZone = DateTime.UtcNow.ToString("o");
				//DateTime initialTime = DateTime.Parse(dateTimeWithZone); // Convert to DateTime object
				//DateTime updatedTime = initialTime.AddSeconds(duration); // Add duration in seconds
				DateTime timeNow = DateTime.Parse(dateTimeWithZone, null, System.Globalization.DateTimeStyles.RoundtripKind);
				DateTime updatedTime = timeNow.AddSeconds(duration); // Add duration in seconds
				string newDateTimeWithZone = updatedTime.ToString("o");
				print("NEW DATA TIME IS " + newDateTimeWithZone + " FOR OUR COOLDOWN SPELL E");
				CharacterCooldownListItem coolie = (new CharacterCooldownListItem {
					SpellnameFull = SpellE,
					Value = newDateTimeWithZone,
					//Position = "SPELLE",
					//PKey = "COOLDOWNE"
				});
				//print($"{coolie.SpellnameFull} is the spell we are putting on cooldown");
				pc.assignedPlayer.ServerCooldownSave(pc.CharID, coolie);
			}		
		}
        while (CooldownE > 0f)
        {
            CooldownE -= Time.deltaTime * cooldownRecoveryRate;
            yield return null;
        }
        CooldownE = 0f;
        SpellECoolDown = false;
    }
	[Server]
	public IEnumerator SetAbilityCoolDownR(float duration, bool setup){
		////print($"Starting Cooldown for {SpellR} at {duration}");
        SpellRCoolDown = true;
        CooldownR = duration;
		PlayerCharacter pc = GetComponent<PlayerCharacter>();
		if(pc && !curatorTM.GetDuelMode()){
			if(!setup){
				string dateTimeWithZone = DateTime.UtcNow.ToString("o");
				//DateTime initialTime = DateTime.Parse(dateTimeWithZone); // Convert to DateTime object
				//DateTime updatedTime = initialTime.AddSeconds(duration); // Add duration in seconds
				DateTime timeNow = DateTime.Parse(dateTimeWithZone, null, System.Globalization.DateTimeStyles.RoundtripKind);
				DateTime updatedTime = timeNow.AddSeconds(duration); // Add duration in seconds
				string newDateTimeWithZone = updatedTime.ToString("o");
				print("NEW DATA TIME IS " + newDateTimeWithZone + " FOR OUR COOLDOWN SPELL R");

				CharacterCooldownListItem coolie = (new CharacterCooldownListItem {
					SpellnameFull = SpellR,
					Value = newDateTimeWithZone,
					//Position = "SPELLR",
					//PKey = "COOLDOWNR"
				});
				//print($"{coolie.SpellnameFull} is the spell we are putting on cooldown");
				pc.assignedPlayer.ServerCooldownSave(pc.CharID, coolie);
			}		
		}
        while (CooldownR > 0f)
        {
            CooldownR -= Time.deltaTime * cooldownRecoveryRate;
            yield return null;
        }
        CooldownR = 0f;
        SpellRCoolDown = false;
    }
	[Server]
	public IEnumerator SetAbilityCoolDownF(float duration, bool setup){
		////print($"Starting Cooldown for {SpellF} at {duration}");
        SpellFCoolDown = true;
        CooldownF = duration;
		PlayerCharacter pc = GetComponent<PlayerCharacter>();
		if(pc && !curatorTM.GetDuelMode()){
			if(!setup){
				string dateTimeWithZone = DateTime.UtcNow.ToString("o");
				//DateTime initialTime = DateTime.Parse(dateTimeWithZone); // Convert to DateTime object
				//DateTime updatedTime = initialTime.AddSeconds(duration); // Add duration in seconds
				DateTime timeNow = DateTime.Parse(dateTimeWithZone, null, System.Globalization.DateTimeStyles.RoundtripKind);
				DateTime updatedTime = timeNow.AddSeconds(duration); // Add duration in seconds
				string newDateTimeWithZone = updatedTime.ToString("o");
				print("NEW DATA TIME IS " + newDateTimeWithZone + " FOR OUR COOLDOWN SPELL F");
				CharacterCooldownListItem coolie = (new CharacterCooldownListItem {
					SpellnameFull = SpellF,
					Value = newDateTimeWithZone,
					//Position = "SPELLF",
					//PKey = "COOLDOWNF"
				});
				//print($"{coolie.SpellnameFull} is the spell we are putting on cooldown");
				pc.assignedPlayer.ServerCooldownSave(pc.CharID, coolie);
			}		
		}
        while (CooldownF > 0f)
        {
            CooldownF -= Time.deltaTime * cooldownRecoveryRate;
            yield return null;
        }
        CooldownF = 0f;
        SpellFCoolDown = false;
    }
	
	bool hasBlock = false;
	public bool GetBlockSpell(){
		return hasBlock;
	}
	[Server]
	void ServerTriggeredBlock(){
		print("Procked the block trigger attempting to remove the buff");
		if(blockCoroutine != null){
			StopCoroutine(blockCoroutine);
			blockCoroutine = null;
		}
		hasBlock = false;
		ServerRemoveStatus("Block", "Block", true);
	}
	Coroutine blockCoroutine;

	public void GiveBlock(string spellName, float duration){
		if(blockCoroutine != null){
			StopCoroutine(blockCoroutine);
			blockCoroutine = null;
		}
		blockCoroutine = StartCoroutine(MOBlock(duration, spellName));
	}
	public IEnumerator MOBlock(float duration, string spellName){
		hasBlock = true;
		Mob mob = GetComponent<Mob>();
		PlayerCharacter pc = GetComponent<PlayerCharacter>();
    	float elapsedTime = 0;
		Vector3 originalPosition = transform.position;
    	while (elapsedTime < duration && hasBlock)
    	{
            elapsedTime += Time.deltaTime;
			yield return null;
    	}
		ServerRemoveStatus("Block", spellName, true);
    	hasBlock = false;
		if(mob)
		mob.DamageTaken();
		blockCoroutine = null;
	}
	string immunitySpell = string.Empty;
	Coroutine immunityCoroutine;
	bool immune = false;
	public bool GetImmune(){
		return immune;
	}
	public void SetImmmuneServer(float duration, string spellName, bool frozen){
		Stunned = false;
        Mesmerized = false;
        Feared = false;
        Snared = false;
        Silenced = false;
		immunitySpell = spellName;
		if(immunityCoroutine != null){
			StopCoroutine(immunityCoroutine);
		}
		immunityCoroutine = StartCoroutine(MOImmunity(duration, spellName, frozen));
	}
	
	public IEnumerator MOImmunity(float duration, string spellName, bool frozen){
		immune = true;
		Mob mob = GetComponent<Mob>();
		PlayerCharacter pc = GetComponent<PlayerCharacter>();
    	float elapsedTime = 0;
		Vector3 originalPosition = transform.position;
		if(frozen){
        Snared = true;
			if(agent){
				agent.enabled = true;
				agent.isStopped = true;
			}
		}
    	while (elapsedTime < duration && immune)
    	{
			if(frozen){
				if(agent){
					agent.enabled = true;
					agent.isStopped = true;
				}
			}
            elapsedTime += Time.deltaTime;
			yield return null;
    	}
		if(agent){
			agent.enabled = true;
			agent.isStopped = false;
		}
		//ServerRemoveStatus("Immunity", spellName);
    	immune = false;
		if(frozen){
			Snared = false;
		}
		if(mob)
		mob.DamageTaken();
		immunityCoroutine = null;
	}

	string fearSpell = string.Empty;
	Coroutine fearCoroutine;
	[Server]
	public void FearThis(float duration, string spellName){
		fearSpell = spellName;
		if(fearCoroutine != null){
			StopCoroutine(fearCoroutine);
		}
		StopATTACKINGMob();
		fearCoroutine = StartCoroutine(MOFeared(duration, spellName));
	}
	public IEnumerator MOFeared(float duration, string spellName){
		Feared = true;
		Mob mob = GetComponent<Mob>();
		PlayerCharacter pc = GetComponent<PlayerCharacter>();
    	float elapsedTime = 0;
		Vector3 originalPosition = transform.position;
    	while (elapsedTime < duration && Feared)
    	{
    	   // Generate a random target position within 5 meters of the original position
            Vector3 randomDirection = UnityEngine.Random.insideUnitSphere * 5f;
            randomDirection += originalPosition;

            NavMeshHit navHit;
            NavMesh.SamplePosition(randomDirection, out navHit, 5f, NavMesh.AllAreas);

            // Move to the random position
            ServerMoveFeared(navHit.position);

            // Move for a random duration between 1 and 3 seconds
            float moveDuration = UnityEngine.Random.Range(1f, 3f);
            float moveElapsedTime = 0;
            while (moveElapsedTime < moveDuration && Feared)
            {
                moveElapsedTime += Time.deltaTime;
                yield return null;
            }

            // Stop for a random duration between 1 and 3 seconds
            agent.isStopped = true;
            float stopDuration = UnityEngine.Random.Range(1f, 3f);
            yield return new WaitForSeconds(stopDuration);
            agent.isStopped = false;

            elapsedTime += moveElapsedTime + stopDuration;
    	}
		ServerRemoveStatus("Fear", spellName, false);
    	Feared = false;
		if(mob)
		mob.DamageTaken();
		fearCoroutine = null;
	}
	string stunSpell = string.Empty;

	Coroutine stunCoroutine;
	[Server]
	public void StunThis(float duration, string spellName){
		if(stunCoroutine != null){
			StopCoroutine(stunCoroutine);
		}
		StopATTACKINGMob();
		Stunned = true;
		stunCoroutine = StartCoroutine(MOStunned(duration, spellName));
	}
	public IEnumerator MOStunned(float duration, string spellName){
		Stunned = true;
		Mob mob = GetComponent<Mob>();
		PlayerCharacter pc = GetComponent<PlayerCharacter>();
		if(agent.enabled)
		agent.isStopped = true;
    	float elapsedTime = 0;
    	while (elapsedTime < duration && Stunned)
    	{
			if(!agent.isStopped){
				if(agent.enabled)
				agent.isStopped = true;
				
			}
    	    elapsedTime += Time.deltaTime;
    	    yield return null;
    	}
		ServerRemoveStatus("Stun", spellName, false);
		if(agent.enabled)
		agent.isStopped = false;
    	Stunned = false;
		if(mob)
		mob.DamageTaken();
		stunCoroutine = null;
	}
	string BlindSpell = string.Empty;

	Coroutine BlindCoroutine;
	[Server]
	public void BlindThis(float duration, string spellName, int spellRank){
		BlindAmount = 5f + ((spellRank - 1) * .2f);
		BlindSpell = spellName;
		if(BlindCoroutine != null){
			StopCoroutine(BlindCoroutine);
		}
		//StopATTACKINGMob();
		Blind = true;
		BlindCoroutine = StartCoroutine(MOBlinded(duration, spellName));
	}
	public IEnumerator MOBlinded(float duration, string spellName){
		Mob mob = GetComponent<Mob>();
		PlayerCharacter pc = GetComponent<PlayerCharacter>();
    	float elapsedTime = 0;
    	while (elapsedTime < duration && Blind)
    	{
    	    elapsedTime += Time.deltaTime;
    	    yield return null;
    	}
		ServerRemoveStatus("Blind", spellName, false);
		if(agent.enabled)
    	Blind = false;
		if(mob)
		mob.DamageTaken();
		BlindCoroutine = null;

	}
	string rootSpell = string.Empty;

	Coroutine rootCoroutine;
	[Server]
	public void RootThis(float duration, string spellName){
		rootSpell = spellName;
		if(rootCoroutine != null){
			StopCoroutine(rootCoroutine);
		}
		//StopATTACKINGMob();
		Snared = true;
		rootCoroutine = StartCoroutine(MORooted(duration, spellName));
	}
	public IEnumerator MORooted(float duration, string spellName){
		Mob mob = GetComponent<Mob>();
		PlayerCharacter pc = GetComponent<PlayerCharacter>();
		if(agent.enabled)
		agent.isStopped = true;
    	float elapsedTime = 0;
    	while (elapsedTime < duration && Snared)
    	{
			if(!agent.isStopped){
				if(agent.enabled)
				agent.isStopped = true;
			}
    	    elapsedTime += Time.deltaTime;
    	    yield return null;
    	}
		ServerRemoveStatus("Root", spellName, false);
		if(agent.enabled)
		agent.isStopped = false;
    	Snared = false;
		if(mob)
		mob.DamageTaken();
		rootCoroutine = null;

	}
	Coroutine silenceCoroutine;
	[Server]
	public void SilenceThis(float duration, string spellName){
		if(silenceCoroutine != null){
			StopCoroutine(silenceCoroutine);
		}
		StopATTACKINGMob();
		Silenced = true;
		silenceCoroutine = StartCoroutine(MOSilenced(duration, spellName));
	}
	public IEnumerator MOSilenced(float duration, string spellName){
		Silenced = true;
		Mob mob = GetComponent<Mob>();
		PlayerCharacter pc = GetComponent<PlayerCharacter>();
    	float elapsedTime = 0;
    	while (elapsedTime < duration && Silenced)
    	{
    	    elapsedTime += Time.deltaTime;
    	    yield return null;
    	}
		ServerRemoveStatus("Silence", spellName, false);
    	Silenced = false;
		if(mob)
		mob.DamageTaken();
		silenceCoroutine = null;
	}
	public IEnumerator MOResting(float duration, string spellName){
		Mesmerized = true;
		Mob mob = GetComponent<Mob>();
		PlayerCharacter pc = GetComponent<PlayerCharacter>();
		if(agent.enabled)
		agent.isStopped = true;
    	float elapsedTime = 0;
    	while (elapsedTime < duration && Mesmerized)
    	{
			if(!agent.isStopped){
				if(agent.enabled)
				agent.isStopped = true;
			}
    	    elapsedTime += Time.deltaTime;
    	    yield return null;
    	}
		ServerRemoveStatus("Mesmerize", spellName, false);
		agent.isStopped = false;
    	Mesmerized = false;
		if(mob)
		mob.DamageTaken();
		mesmerizeCoroutine = null;

	}
	Coroutine mesmerizeCoroutine;
	[Server]
	public void RestThis(float duration, string spellName){
		if(mesmerizeCoroutine != null){
			StopCoroutine(mesmerizeCoroutine);
		}
		StopATTACKINGMob();
		Mesmerized = true;
		mesmerizeCoroutine = StartCoroutine(MORest(duration, spellName));
	}
	public IEnumerator MORest(float duration, string spellName){
		Mesmerized = true;
		Mob mob = GetComponent<Mob>();
		PlayerCharacter pc = GetComponent<PlayerCharacter>();
		if(agent.enabled)
		agent.isStopped = true;
    	float elapsedTime = 0;
    	while (elapsedTime < duration && Mesmerized)
    	{
			if(!agent.isStopped){
				if(agent.enabled)
				agent.isStopped = true;
			}
    	    elapsedTime += Time.deltaTime;
    	    yield return null;
    	}
		ServerRemoveStatus("Mesmerize", spellName, false);
		agent.isStopped = false;
    	Mesmerized = false;
		if(mob)
		mob.DamageTaken();
		mesmerizeCoroutine = null;
	}
	[Server]
	public void MesmerizeThis(float duration, string spellName){
		if(mesmerizeCoroutine != null){
			StopCoroutine(mesmerizeCoroutine);
			ServerRemoveStatus("Mesmerize", spellName, false);
		}
		StopATTACKINGMob();
		Mesmerized = true;
		mesmerizeCoroutine = StartCoroutine(MOMesmerized(duration, spellName));
	}
	Coroutine shieldBlockBuffCoroutine;
	string priorshieldBlockBuffName = string.Empty;
	[Server]
	public void ShieldUpThis(float duration, string spellName, float shieldBlockBuffDmg){
		if(shield){
			if(shieldBlockBuffCoroutine != null){
				StopCoroutine(shieldBlockBuffCoroutine);
				ServerRemoveStatus("ShieldBlockChance", priorshieldBlockBuffName, true);
				shieldChance -= (int)shieldBlockBuffValue;
			}
			shieldBlockBuffValue = shieldBlockBuffDmg;
			shieldChance += (int)shieldBlockBuffValue;
			shieldBlockBuffCoroutine = StartCoroutine(MOshieldBlockBuffs(duration, spellName));
		}

	}
	public IEnumerator MOshieldBlockBuffs(float duration, string spellName){
		priorshieldBlockBuffName = spellName;
    	float elapsedTime = 0;
    	while (elapsedTime < duration)
    	{
    	    elapsedTime += Time.deltaTime;
    	    yield return null;
    	}
		ServerRemoveStatus("ShieldBlockChance", spellName, true);
		shieldBlockBuffValue = 0;
		priorshieldBlockBuffName = string.Empty;
		shieldBlockBuffCoroutine = null;
	}
	Coroutine thornCoroutine;
	string priorThornName = string.Empty;
	[Server]
	public void ThornsThis(float duration, string spellName, int thornDmg){
		thornValue = thornDmg;
		if(thornCoroutine != null){
			StopCoroutine(thornCoroutine);
			ServerRemoveStatus("Thorns", priorThornName, true);
		}
		thornCoroutine = StartCoroutine(MOThorns(duration, spellName));
	}
	public IEnumerator MOThorns(float duration, string spellName){
		priorThornName = spellName;
    	float elapsedTime = 0;
    	while (elapsedTime < duration)
    	{
    	    elapsedTime += Time.deltaTime;
    	    yield return null;
    	}
		ServerRemoveStatus("Thorns", spellName, true);
		thornValue = 0;
		priorThornName = string.Empty;
		thornCoroutine = null;
	}
	[Server]
	public void SleepThis(float duration, string spellName){
		if(mesmerizeCoroutine != null){
			StopCoroutine(mesmerizeCoroutine);
		}
		StopATTACKINGMob();
		Mesmerized = true;
		mesmerizeCoroutine = StartCoroutine(MOSleep(duration, spellName));
	}
	[Server]
	public void EnthrallThis(float duration, string spellName){
		if(mesmerizeCoroutine != null){
			StopCoroutine(mesmerizeCoroutine);
		}
		StopATTACKINGMob();
		Mesmerized = true;
		mesmerizeCoroutine = StartCoroutine(MOMesmerizedEnthrall(duration, spellName));
	}
	public IEnumerator MOMesmerizedEnthrall(float duration, string spellName){
		Mesmerized = true;
		Mob mob = GetComponent<Mob>();
		PlayerCharacter pc = GetComponent<PlayerCharacter>();
		if(agent.enabled)
		agent.isStopped = true;
    	float elapsedTime = 0;
    	while (elapsedTime < duration && Mesmerized)
    	{
			if(!agent.isStopped){
				if(agent.enabled)
				agent.isStopped = true;
			}
    	    elapsedTime += Time.deltaTime;
    	    yield return null;
    	}
		ServerRemoveStatus("Enthrall", spellName, false);
		agent.isStopped = false;
    	Mesmerized = false;
		if(mob)
		mob.DamageTaken();
		mesmerizeCoroutine = null;
	}
	public IEnumerator MOMesmerized(float duration, string spellName){
		Mesmerized = true;
		Mob mob = GetComponent<Mob>();
		PlayerCharacter pc = GetComponent<PlayerCharacter>();
		if(agent.enabled)
		agent.isStopped = true;
    	float elapsedTime = 0;
    	while (elapsedTime < duration && Mesmerized)
    	{
			if(!agent.isStopped){
				if(agent.enabled)
				agent.isStopped = true;
			}
    	    elapsedTime += Time.deltaTime;
    	    yield return null;
    	}
		ServerRemoveStatus("Mesmerize", spellName, false);
		agent.isStopped = false;
    	Mesmerized = false;
		if(mob)
		mob.DamageTaken();
		mesmerizeCoroutine = null;
	}
	public IEnumerator MOSleep(float duration, string spellName){
		Mesmerized = true;
		Mob mob = GetComponent<Mob>();
		PlayerCharacter pc = GetComponent<PlayerCharacter>();
		if(agent.enabled)
		agent.isStopped = true;
    	float elapsedTime = 0;
    	while (elapsedTime < duration && Mesmerized)
    	{
			if(!agent.isStopped){
				if(agent.enabled)
				agent.isStopped = true;
			}
    	    elapsedTime += Time.deltaTime;
    	    yield return null;
    	}
		ServerRemoveStatus("Mesmerize", spellName, false);
		agent.isStopped = false;
    	Mesmerized = false;
		if(mob)
		mob.DamageTaken();
		mesmerizeCoroutine = null;

	}
	//interrupt code
	[Server]
	public void Interrupted(TurnManager curator){
		if(curator == curatorTM){
			//kill cast on enemy
			if(Casting){
				Casting = false;
				RpcCancelMOCast();
			}
			if(MobCasting != null){
				StopCoroutine(MobCasting);
				MobCasting = null;
				Mob mob = GetComponent<Mob>();
				if(mob){
					mob.DamageTaken();
				}
			}
		}
	}
	/*
	[Server]
    public void ApplyStatModifier(StatModifier modifier)
    {
        statsHandler.ApplyStatModifier(modifier, this);
    }
	*/
	[Server] //this is where we should put crowd control
public void ApplyStatModifier(StatModifier modifier, string stat)
{
	bool foodPotion = false;
	if(modifier.Food || modifier.Potion){
		foodPotion = true;
	}
    statsHandler.ApplyStatModifier(modifier, this, (success) => {
    	// Inform the client about the stat change, and perhaps include whether it was successful
		if (!success){
    	    // Send a specific message to the client indicating the failure
    	    TargetSendFailureMessageToClient("Failed buff");
    	} else {
			
			ClientUpdateStatChangesAdd(stat, modifier.Duration, modifier.MaxDuration, modifier.Value, modifier.BuffName, modifier.Buff, modifier.Rank, foodPotion);
		}
    });
}
[TargetRpc]
public void TargetSendFailureMessageToClient(string message)
{
	ImproperCheckText.Invoke(message);
    // Implement code to send the message to the client here
    // This could be another RPC call or any other method of informing the client
}
	[Server]
	public void StartingHideCounter(float value){
		StartCoroutine(HideCounter(value));
	}
	[Server]
	public void StartingSneak(){
		Sneak = true;
	}
	IEnumerator HideCounter(float value){
		float timer = value;
		while(Hide && timer > 0){
			yield return new WaitForSeconds(1f); // Pause for 1 second
        	timer--; // Decrement the timer
		}
		ServerRemoveStatus("Stealthed","Hide", true);
		PlayerCharacter pc = GetComponent<PlayerCharacter>();
		if(pc){
			UnStealthedChar.Invoke(this, pc.assignedMatch);
		}
		RpcUnhide();
	}
	[Server]
	public void StartingInvisibilityCounter(float value){
		StartCoroutine(InvisibilityCounter(value));
	}
	IEnumerator InvisibilityCounter(float value){
		float timer = value;
		while(Invisibility && timer > 0){
			yield return new WaitForSeconds(1f); // Pause for 1 second
        	timer--; // Decrement the timer
		}
		ServerRemoveStatus("Stealthed","Invisibility", true);
		PlayerCharacter pc = GetComponent<PlayerCharacter>();
		if(pc){
			UnStealthedChar.Invoke(this, pc.assignedMatch);
		}
		RpcUnhide();
	}
	[Server]
	public void ServerAbsorbEnded(){
		PlayerCharacter pc = GetComponent<PlayerCharacter>();
		if(pc){
			pc.assignedPlayer.ServerRemoveNonBuffSpell(pc.CharID, "Absorb");
			pc.ClientUpdateStatChangesRemove("Absorb", "Absorb", true);
		}
	}
	[Server]
	public void ServerRemoveStatus(string statusRemoval, string spellName, bool buff){
		if(spellName == "Hide"){
			PlayerCharacter pc = GetComponent<PlayerCharacter>();
			if(pc){
				UnStealthedChar.Invoke(this, pc.assignedMatch);
			}
			ClientUpdateStatChangesRemove(spellName, "Stealthed", buff);
			RpcUnhide();
			Hide = false;
		}
		if(statusRemoval == "Undead Protection"){
			ClientUpdateStatChangesRemove(spellName, "Undead Protection", buff);
		}
		if(statusRemoval == "Thorns"){
			ClientUpdateStatChangesRemove(spellName, "Thorns", buff);
		}
		if(spellName == "Invisibility"){
			PlayerCharacter pc = GetComponent<PlayerCharacter>();
			if(pc){
				UnStealthedChar.Invoke(this, pc.assignedMatch);
			}
			ClientUpdateStatChangesRemove(spellName, "Stealthed", buff);
			RpcUnhide();
			Invisibility = false;
		}
		if(spellName == "Sneak"){
			Sneak = false;
			PlayerCharacter pc = GetComponent<PlayerCharacter>();
			if(pc){
				UnStealthedChar.Invoke(this, pc.assignedMatch);
			}
			RpcUnhide();
			ClientUpdateStatChangesRemove(spellName, "Stealthed", buff);
		}
		if(statusRemoval == "Immunity"){
			ClientUpdateStatChangesRemove(spellName, "Immunity", buff);
			if(agent.enabled){
				agent.ResetPath();
				agent.isStopped = false;
			}
		}
		if(statusRemoval == "Ice Block"){
			ClientUpdateStatChangesRemove(spellName, "Ice Block", buff);
			if(agent.enabled){
				agent.ResetPath();
				agent.isStopped = false;
			}
		}
		
		if(statusRemoval == "ShieldBlockChance"){
			ClientUpdateStatChangesRemove(spellName, "ShieldBlockChance", buff);
		}
		if(statusRemoval == "Charm"){
			ClientUpdateStatChangesRemove(spellName, "Charm", buff);
		}
		if(statusRemoval == "Offensive Stance"){
			ClientUpdateStatChangesRemove(spellName, "Offensive Stance", buff);
		}
		if(statusRemoval == "Tank Stance"){
			ClientUpdateStatChangesRemove(spellName, "Tank Stance", buff);
		}
		if(statusRemoval == "Magic Burst"){
			ClientUpdateStatChangesRemove(spellName, "Magic Burst", buff);
		}
		if(statusRemoval == "Nature's Precision"){
			ClientUpdateStatChangesRemove(spellName, "Nature's Precision", buff);
		}
		if(statusRemoval == "Absorb"){
			ClientUpdateStatChangesRemove(spellName, "Absorb", buff);
		}
		if(statusRemoval == "Protect"){
			ClientUpdateStatChangesRemove(spellName, "Protect", buff);
		}
		if(statusRemoval == "Fear"){
			ClientUpdateStatChangesRemove(spellName, "Fear", buff);
		}
		if(statusRemoval == "Root"){
			ClientUpdateStatChangesRemove(spellName, "Root", buff);
		}
		if(statusRemoval == "Stun"){
			ClientUpdateStatChangesRemove(spellName, "Stun", buff);
		}
		if(statusRemoval == "Silence"){
			ClientUpdateStatChangesRemove(spellName, "Silence", buff);
		}
		if(statusRemoval == "Enthrall"){
			ClientUpdateStatChangesRemove(spellName, "Mesmerize", buff);
		}
		if(statusRemoval == "Sleep"){
			ClientUpdateStatChangesRemove(spellName, "Mesmerize", buff);
		}
		if(statusRemoval == "Mesmerize"){
			ClientUpdateStatChangesRemove(spellName, "Mesmerize", buff);
		}
		if(statusRemoval == "Refresh"){
			ClientUpdateStatChangesRemove(spellName, "Refresh", buff);
		}
		if(statusRemoval == "Blind"){
			ClientUpdateStatChangesRemove(spellName, "Blind", buff);
		}
		if(statusRemoval == "Magic Burst"){
			ClientUpdateStatChangesRemove(spellName, "Magic Burst", buff);
		}
		if(statusRemoval == "Block"){
			ClientUpdateStatChangesRemove(spellName, "Block", buff);
		}
		
		
	}
	[ClientRpc]
	public void RpcUnhide(){
		Debug.Log("RpcUnhide called.");
		ClientUnhide();
	}
	[ClientRpc]
	public void RpcHideCast(){
		Debug.Log("RpcHideCast called.");
		ClientHide();
	}
	public void ClientUnhide(){
		SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
		if(spriteRenderer.color != originalColor){
        	spriteRenderer.color = originalColor;
		}
	}
	public void ClientHide(){
		float alphaAdjuster = 0.5f;
		if(!ScenePlayer.localPlayer.GetFriendlyList().Contains(this)){
			alphaAdjuster = 0f;
		}
		Color Hidden = new Color(1.0f, 1.0f, 1.0f, alphaAdjuster);
		SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
		if(spriteRenderer.color != Hidden){
        	spriteRenderer.color = Hidden;
		}
	}
	public int GetAttackDelayEnemy(){
		return attackDelayEnemy;
	}
	// Change from Coroutine to a custom struct.
private struct HOTInfo
{
    public Coroutine Coroutine;
    public int Rank;
	public int valueCount;
	public void ChangeValue(bool add){
		if(add){
			valueCount ++;
		} else {
			valueCount --;
		}
	}
}
public struct DOTInfo
{
	public string spellName;
    public Coroutine Coroutine;
    public int Rank;
	public int valueCount;
	public void ChangeValue(bool add){
		if(add){
			valueCount ++;
		} else {
			valueCount --;
		}
		print($"Changed value to {valueCount}");
	}
}
private Dictionary<string, HOTInfo> activeHOTCoroutines = new Dictionary<string, HOTInfo>();
	//private Dictionary<string, Coroutine> activeHOTCoroutines = new Dictionary<string, Coroutine>();
private List<DOTInfo> activeDOTs = new List<DOTInfo>();
[Server]
public void StopAllHOTAndDOTCoroutines()
{
    // Stop all HOT coroutines
    foreach (var hot in activeHOTCoroutines.Values)
    {
        if (hot.Coroutine != null)
        {
            StopCoroutine(hot.Coroutine);
        }
    }
    activeHOTCoroutines.Clear();

    // Stop all DOT coroutines
    foreach (var dot in activeDOTs)
    {
        if (dot.Coroutine != null)
        {
            StopCoroutine(dot.Coroutine);
        }
    }
    activeDOTs.Clear();
}
	[Server]
	public bool ServerHOT(string spellName, int spellRank, float duration, List<int> values, string ownerName, MovingObject ogcaster){
		//StartCoroutine(ServerHealOverTime(duration, values));
		if (activeHOTCoroutines.ContainsKey(spellName))
    	{
    	    // Check if the rank of the new spell is less than the active one.
    	    if (spellRank < activeHOTCoroutines[spellName].Rank)
    	    {
    	        return false; // Do not refresh.
    	    }
    	    StopCoroutine(activeHOTCoroutines[spellName].Coroutine);
    	    activeHOTCoroutines.Remove(spellName);
    	}
		ogcaster.RpcOverTimeAnimation(spellName, this);
    	// Start the new coroutine.
        string targetName = string.Empty;
		Mob mob = GetComponent<Mob>();
        if(mob){
		    targetName = StatAsset.Instance.GetMobName(StatAsset.Instance.GetEnemyToPrefabMapping(), mob.NAME);
        } else {
			PlayerCharacter PlayerHit = GetComponent<PlayerCharacter>();
			if(PlayerHit){
               targetName = PlayerHit.CharacterName;
			}
		}
    	RpcPostFullOverTime(spellName, ownerName, targetName, values.Sum().ToString(), spellName);
		Coroutine newCoroutine = StartCoroutine(ServerHealOverTime(spellName, duration, values, ownerName, ogcaster));
    	HOTInfo hotInfo = new HOTInfo { Coroutine = newCoroutine, Rank = spellRank };
    	activeHOTCoroutines.Add(spellName, hotInfo);
		return true;
	}
	[Server]
	public void RemoveServerRest(string spellName){
		//StartCoroutine(ServerHealOverTime(duration, values));
		resting = false;
		if (activeHOTCoroutines.ContainsKey(spellName))
    	{
    	    // Check if the rank of the new spell is less than the active one.
    	    StopCoroutine(activeHOTCoroutines[spellName].Coroutine);
    	    activeHOTCoroutines.Remove(spellName);
    	}
    	// Start the new coroutine.
	}
	[Server]
	public void ServerCleansePoisonDisease(int spellRank)//add disease spells in here later
	{
		List< DOTInfo> poisonOrDisease = new List< DOTInfo>();
		foreach(var dot in activeDOTs){
			if(StatAsset.Instance.GetDotType(dot.spellName) == "Poison" || StatAsset.Instance.GetDotType(dot.spellName) == "Disease"){
				poisonOrDisease.Add(dot);
			}
		}
		for(int i = 0; i < poisonOrDisease.Count; i++){
			ClientUpdateStatChangesRemove(poisonOrDisease[i].spellName, "DOT", false);
			StopCoroutine(poisonOrDisease[i].Coroutine);
	    	activeDOTs.Remove(poisonOrDisease[i]);
		}
		
	}
	[Server]
	public void ServerCurePoisonAntidote()
	{
		List< DOTInfo> tempPoisonDots = new List< DOTInfo>();
		foreach(var dot in activeDOTs){
			if(StatAsset.Instance.GetDotType(dot.spellName) == "Poison"){
				tempPoisonDots.Add(dot);
			}
		}
			//sort by values for this one
		for(int i = 0; i < tempPoisonDots.Count; i++){
			ClientUpdateStatChangesRemove(tempPoisonDots[i].spellName, "DOT", false);
			StopCoroutine(tempPoisonDots[i].Coroutine);
	    	activeDOTs.Remove(tempPoisonDots[i]);
		}
	}
	[Server]
	public void ServerCurePoison(int spellRank)
	{
		List< DOTInfo> tempPoisonDots = new List< DOTInfo>();
		foreach(var dot in activeDOTs){
			if(StatAsset.Instance.GetDotType(dot.spellName) == "Poison"){
				tempPoisonDots.Add(dot);
			}
		}
			//sort by values for this one
		for(int i = 0; i < tempPoisonDots.Count; i++){
			ClientUpdateStatChangesRemove(tempPoisonDots[i].spellName, "DOT", false);
			StopCoroutine(tempPoisonDots[i].Coroutine);
	    	activeDOTs.Remove(tempPoisonDots[i]);
		}
		if(ImmunityRoutine != null){
			StopCoroutine(ImmunityRoutine);
			ImmunityRoutine = null;
		}
		ImmunityRoutine = StartCoroutine(PoisonImmunity(spellRank));
	}
	
	bool ImmuneToPoison = false;
	public bool GetImmuneToPoison(){
		return ImmuneToPoison;
	}
	Coroutine ImmunityRoutine;
	IEnumerator PoisonImmunity(int spellRank){
		ImmuneToPoison = true;
		yield return new WaitForSeconds(spellRank);
		ImmuneToPoison = false;
	}
	[Server]
public bool ServerDOT(string spellName, int spellRank, float duration, List<int> values, MovingObject spellOwner)
{
	Mob mob = GetComponent<Mob>();
	PlayerCharacter pc = GetComponent<PlayerCharacter>();
	if(spellName == "Greater Poison" || spellName == "Moderate Poison" || spellName == "Lesser Poison"){
		if(GetImmuneToPoison()){
			return false;
		}
		int maxStackP = 5;
		int curStackP = 0;
		List< DOTInfo> tempPoisonDots = new List< DOTInfo>();
		foreach(var dot in activeDOTs){
			if(dot.spellName == "Greater Poison" && spellName == "Greater Poison"){
				curStackP ++;
				tempPoisonDots.Add(dot);
			}
			if(dot.spellName == "Moderate Poison" && spellName == "Moderate Poison"){
				curStackP ++;
				tempPoisonDots.Add(dot);
			}
			if(dot.spellName == "Lesser Poison" && spellName == "Lesser Poison"){
				curStackP ++;
				tempPoisonDots.Add(dot);
			}
		}
		if(curStackP >= maxStackP){
			//sort by values for this one
			tempPoisonDots = tempPoisonDots.OrderBy(dot => dot.valueCount).ToList();
			StopCoroutine(tempPoisonDots[0].Coroutine);
	        activeDOTs.Remove(tempPoisonDots[0]);
		}
	} else {
		for(int dot = 0; dot < activeDOTs.Count; dot++){
			if(activeDOTs[dot].spellName == spellName){
				StopCoroutine(activeDOTs[dot].Coroutine);
	        	activeDOTs.Remove(activeDOTs[dot]);
				break;
			}
		}
	}
	//spellOwner.RpcOverTimeAnimation(spellName, this);
	string PoisonCheck = spellName;
	if(spellName == "Poison"){
		if(mob){
			PoisonCheck = "Moderate Poison";
		}
	}
	string targetName = string.Empty;
	string ownerName = string.Empty;
	Mob castingMOB = spellOwner.GetComponent<Mob>();
	PlayerCharacter castingPC = spellOwner.GetComponent<PlayerCharacter>();
    if(castingPC){
       ownerName = castingPC.CharacterName;
    } else {
		ownerName = StatAsset.Instance.GetMobName(StatAsset.Instance.GetEnemyToPrefabMapping(), castingMOB.NAME);
    }
    if(pc){
       targetName = pc.CharacterName;
    } else {
		targetName = StatAsset.Instance.GetMobName(StatAsset.Instance.GetEnemyToPrefabMapping(), mob.NAME);
    }
    RpcPostFullOverTime(PoisonCheck, ownerName, targetName, values.Sum().ToString(), PoisonCheck);
    // Start the new DOT coroutine.
    Coroutine newDOT = StartCoroutine(ServerDamageOverTime(spellName, duration, values, spellOwner));
    DOTInfo dotInfo = new DOTInfo {spellName = spellName, Coroutine = newDOT, Rank = spellRank, valueCount = values.Count};
    activeDOTs.Add(dotInfo);
    return true; // Successfully applied the DOT.
}
	IEnumerator ServerDamageOverTime(string spellName, float duration, List<int> values, MovingObject spellOwner)
	{
		string magicString = MAGICDAMAGECOLOR;
	    int numberOfValues = values.Count;
		int tickCounter = 0; // Initialize the tick counter
	    // The time interval is now 1 second
	    float timeInterval = 1f;  
	    for (int i = 0; i < numberOfValues; i++)
	    {
			if(Dying){
				StopAllHOTAndDOTCoroutines();
			}
			if(spellName == "Greater Poison"){
				magicString = POISONDAMAGECOLOR;
			}
			if(spellName == "Moderate Poison"){
				magicString = POISONDAMAGECOLOR;
			}
			if(spellName == "Lesser Poison"){
				magicString = POISONDAMAGECOLOR;
			}
		        this.DamageDealt(spellOwner, values[i], false, true, 0, magicString, false, null, spellName);
				if (tickCounter % 3 == 0)
        		{
        		    RpcDotSound();
        		}
        		tickCounter++;
		        // Wait for the calculated time interval before checking the next value
				for(int dot = 0; dot < activeDOTs.Count; dot++){
					if(activeDOTs[dot].spellName == spellName){
						activeDOTs[dot].ChangeValue(false);
						break;
					}
				}
		        yield return new WaitForSeconds(timeInterval);
		}
		for(int dot = 0; dot < activeDOTs.Count; dot++){
			if(activeDOTs[dot].spellName == spellName){
	        	activeDOTs.Remove(activeDOTs[dot]);
				break;
			}
		}
	}
	IEnumerator ServerHealOverTime(string spellName, float duration, List<int> values, string ownerName, MovingObject ogCaster){
        ////print("ServerHealOverTime Moving before turn manager");
    	int numberOfValues = values.Count;
    	// The time interval is now 1 second
    	float timeInterval = 3f;  
		Mob mob = GetComponent<Mob>();
		PlayerCharacter pc = GetComponent<PlayerCharacter>();
    	string targetName = string.Empty;
    	if(pc){
    	   targetName = pc.CharacterName;
    	} else {
			targetName = StatAsset.Instance.GetMobName(StatAsset.Instance.GetEnemyToPrefabMapping(), mob.NAME);
    	}
    	for (int i = 0; i < numberOfValues; i++){
			if(Dying){
				StopAllHOTAndDOTCoroutines();
			}
    	    ////print("Hot tick Moving before turn manager");
    	    cur_hp += values[i];
    	    if(cur_hp > max_hp){
    	        cur_hp = max_hp;
    	    }
			if(pc){
				curatorTM.HOTTICK(pc);
			}
			string valueString = values[i].ToString();
			AddRpcCall(valueString, GREENHEALCOLOR, false, true, spellName, weaponType, false, ownerName, targetName, ogCaster, this, Vector2.zero);
    	    //RpcSpawnMagicalEffectDmg(valueString, GREENHEALCOLOR, false);
			//RpcClientRpcChangeColor("3AE300",1f);
			RpcHotSound();
    	    // Wait for the calculated time interval before checking the next value
    	    yield return new WaitForSeconds(timeInterval);
    	}
		activeHOTCoroutines.Remove(spellName);
	}
	
//[ClientRpc]
//	public void RpcAnimateChargeEnd(MovingObject target){
//		ItemAssets.Instance.CastChargeEndAnimation(this, target);
//	}
[ClientRpc]
public void RpcHotSound(){
	SpriteRenderer sRend = GetComponent<SpriteRenderer>();
	if(sRend.enabled){
		Speaker.Invoke(55);
	}
}
bool dotsoundTriggered = false;
[ClientRpc]
public void RpcDotSound(){
	SpriteRenderer sRend = GetComponent<SpriteRenderer>();
	if(sRend.enabled && !dotsoundTriggered){
		Speaker.Invoke(56);
		dotsoundTriggered = true;
	} else if(sRend.enabled && dotsoundTriggered){
		dotsoundTriggered = false;
	}
}
[ClientRpc]
	public void RpcAnimateTacticianSpellEvacuate(){
		ItemAssets.Instance.CastingTacticianSpellEvacuate(this);
	}
	[ClientRpc]
	public void RpcAnimateTacticianSpell(string spell, MovingObject target){
		ItemAssets.Instance.CastingTacticianSpellAnimation(target, spell);
	}
	
	//[ClientRpc]
	public void RpcAnimateSpell(string spell, MovingObject target){
		ItemAssets.Instance.CastingSpellAnimation(spell, this, target);
		PlayerCharacter pc = GetComponent<PlayerCharacter>();
		if(pc){
			if(spell == "Bite"){
			//activate the trigger for the animator
			}
			if(spell == "Armor Bite"){
			//activate the trigger for the animator
			}
			if(spell == "Focus"){
			//activate the trigger for the animator
			}
		}
	}
	[ClientRpc]
	public void RpcAnimateSpellAOECasted(string spell, Vector2 position){
		ItemAssets.Instance.CastingSpellAnimationAOE(spell, this, position);
	}
	[ClientRpc]
	public void RpcAnimateSpellAOEHeal(List<MovingObject> positions){
		foreach(var position in positions){
			ItemAssets.Instance.CastingSpellAnimation("Heal", this, position);
		}
	}
	//[ClientRpc]
	public void RpcAnimateSpellAOE(string spell, Vector2 position){
		ItemAssets.Instance.CastingSpellAnimationAOE(spell, this, position);
		PlayerCharacter pc = GetComponent<PlayerCharacter>();
		if(pc){
			if(spell == "Roar"){
			//activate the trigger for the animator
			}
			if(spell == "Fire Breath"){
			//activate the trigger for the animator
			}
			if(spell == "Tail Whip"){
			//activate the trigger for the animator
			}
			
		}
	}
	[ClientRpc]
	public void RpcAnimateOverTime(string spell,float duration, MovingObject target){
		ItemAssets.Instance.CastingSpellAnimationOverTime(spell, this, target, duration);
	}
	[ClientRpc]
	public void RpcAnimateSpellSelfCasted(string spell, MovingObject selfCastedObj){
		ItemAssets.Instance.CastingSpellAnimationSelfCasted(spell, selfCastedObj);
	}
	[ClientRpc]
	public void RpcAnimateEnemySpell(string spell, MovingObject targetPosition){
		ItemAssets.Instance.CastingSpellAnimationEnemy(spell, this, targetPosition);
	}
	public bool hoveringOver = false;
	IEnumerator AnimatingSprite(){
		SpriteRenderer sRend = GetComponent<SpriteRenderer>();
		while(true){
			if(!hoveringOver){
				if(activeColdSpells > 0){
					sRend.color = icyBlueColorRef;
				} else {
					sRend.color = originalColor;
				}
				sRend.sprite = mainSprite;
			}
			
			//if(activeColdSpells > 0){
			//	sRend.color = icyBlueColorRef;
			//} else {
			//	sRend.color = originalColor;
			//}
			yield return new WaitForSeconds(.5f);
			if(!hoveringOver){
				if(activeColdSpells > 0){
					sRend.color = icyBlueColorRef;
				} else {
					sRend.color = originalColor;
				}
				sRend.sprite = offSprite;
			}
			//if(activeColdSpells > 0){
			//	sRend.color = icyBlueColorRef;
			//} else {
			//	sRend.color = originalColor;
			//}
			//sRend.sprite = offSprite;
			//if(activeColdSpells > 0){
			//	sRend.color = icyBlueColorRef;
			//} else {
			//	sRend.color = originalColor;
			//}
			yield return new WaitForSeconds(.5f);
		}
	}
	[ClientRpc]
	public void RpcClientRpcChangeColor(string hexColor, float duration){
		ChangeColorForAnimation(hexColor, duration);
	}
	public void ChangeColorForAnimation(string hexColor, float duration){
		if(coloredSpriteAnimation != null){
			StopCoroutine(coloredSpriteAnimation);
			coloredSpriteAnimation = null;
		}
		coloredSpriteAnimation = StartCoroutine(ColoredSpriteRoutine(hexColor, duration));
		
	}
	Coroutine coloredSpriteAnimation;
	IEnumerator ColoredSpriteRoutine(string hexColor, float duration){
		if(Dying){
			yield break;
		}
		SpriteRenderer sRend = GetComponent<SpriteRenderer>();
		if(!sRend){
			yield break;
		}
		Color newColor;
        if(hexColor != null){
                if (ColorUtility.TryParseHtmlString("#" + hexColor, out newColor))
            {
                //if(hexColor == "E3CA00"){
                //    text.fontSize =  1.5f;
                //}
                sRend.color = newColor;
            }
            else
            {
                Debug.LogError("Invalid hex color value.");
            }
        }
		yield return new WaitForSeconds(duration);
		sRend.color = originalColor;
		coloredSpriteAnimation = null;
	}
	[Server]
	public void SetStatsServer(){
		PlayerCharacter pc = GetComponent<PlayerCharacter>();
		if(pc){
			//print($"Movingobject {gameObject.name} is setting stats and has {dodge} dodge chance {strength} strength {fortitude} fortitude {arcana} arcana {armor} armor {MagicResist} MagicResist {FireResist} FireResist and {agility} agility");
		}
		statsHandler.SetInitialStats(this, agility, strength, fortitude, arcana, armor, MagicResist, FireResist, ColdResist, DiseaseResist, PoisonResist, dodge, BonusLeechEffect);
	} 
	/*

	[Server]
	public IEnumerator EnergyUpdater(){
		////print($"ENERGIZEDDDDDDD this mob {gameObject.name}");
		PlayerCharacter pcCheck = GetComponent<PlayerCharacter>();
		Mob mob = GetComponent<Mob>();
		float rechargeTime = .5f;
		float haste = 0f;
		Energized = true;
        stamina = 0f;
		int dodge = 0;
		if(mob){
			dodge = mob.dodge;
		}
		if(pcCheck){
			ClientSparkVision(pcCheck.assignedPlayer.OurNode.GetVision());
		}
		statsHandler.SetInitialStats(this, agility, strength, fortitude, arcana, armor, MagicResist, FireResist, ColdResist, DiseaseResist, PoisonResist, dodge);
		while (Energized)
    	{
    	    if (!moving && !Casting && !Mesmerized && !Stunned && !Feared)
    	    {
				if(mob){
					if(GetAgility() > 101){
						haste = Mathf.Floor((GetAgility() - 100) / 2);
					} else {
						haste = 0f;
					}
				}
				if(pcCheck){
					if(GetAgility() > 101){
						haste = Mathf.Floor((GetAgility() - 100) / 2);
					} else {
						haste = 0f;
					}
				}
				
				rechargeTime = .5f / (1f + (haste / 100f));
				stamina -= 5f;
        		stamina = Mathf.Clamp(stamina, -100f, 250f);
				if(stamina <= 0){
    	            RpcClientCheckColor();
    	        } else {
					RpcChangeToGray();
				}
				
				if(mob != null && stamina == -100f ){
					if(mob.PatrolPath == null){

						if(mob.Resetting){
							moving = true;
							curatorTM.ResetSingleMob(mob);
						}
						if(mob.threatList.Count == 0 && stamina == -100f && !mob.Resetting && mob.Searching ){
							foreach(var pc in curatorTM.GetPCList()){
								if(AreTilesWithinVision(mob.Vision, pc.transform.position, mob.transform.position) && HasLineOfSight(pc.transform.position, mob.transform.position)){
									curatorTM.AggroEntireGroup(mob.groupNumber, mob);
									break;
								}
							}
						}
						if(mob.threatList.Count > 0 && stamina == -100f && mob.SwitchFlip && !mob.Resetting && !mob.Dying ){
							mob.SwitchFlip = false;
							curatorTM.EnemyAction(mob.assignedMatch, mob);
						}
					} else {
						//add patrol logic
						curatorTM.PatrolMovement(mob.assignedMatch, mob);
					}
				}
				yield return new WaitForSeconds(rechargeTime);
    	    } else {
				yield return null;
			}
    	}
    }
	*/
	[Server]
	public void ServerRangedProcess(MovingObject shooter){
		//print($"ServerRangedProcess {shooter.name} shot as {gameObject.name}");
		float distance = Vector2.Distance(shooter.transform.position, transform.position);
        float duration = distance / 1f * 0.05f;
		//RpcClientRangedAttack(shooter, duration);
	}
	/*
	[ClientRpc]
	public void RpcClientRangedAttack(MovingObject shooter){
		//print($"RpcClientRangedAttack {shooter.name} shot as {gameObject.name}");

		float distance = Vector2.Distance(shooter.transform.position, transform.position);
        float duration = distance / 1f * 0.05f;
		//print("spawning arrow on client!");
		AudioMgr sound = GetComponent<AudioMgr>();
		sound.PlaySound("bow draw");
		GameObject arrowHit = Instantiate(ArrowPrefab, shooter.transform.position, Quaternion.identity);
		Vector2 direction = transform.position - shooter.transform.position;
		float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 225f; // -90f to adjust for sprite pointing downwards
    	arrowHit.transform.rotation = Quaternion.Euler(0f, 0f, angle);
		arrowHit.GetComponent<ArrowProjectile>().TravelToTarget(new Vector2(transform.position.x, transform.position.y), duration);
	}
	*/
	[ClientRpc]
	public void RpcClientRangedAttack(MovingObject target, bool crit){
		//print($"RpcClientRangedAttack {target.name} shot as {gameObject.name}");

		float distance = Vector2.Distance(target.transform.position, transform.position);
        float duration = distance / 1f * 0.05f;
		//print("spawning arrow on client!");
		//AudioMgr sound = GetComponent<AudioMgr>();
		//sound.PlaySound("bow draw");
		GameObject arrowHit = Instantiate(ArrowPrefab, transform.position, Quaternion.identity);
		//Vector2 direction = transform.position - target.transform.position;
		Vector2 direction = target.transform.position - transform.position;

		float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 225f; // -90f to adjust for sprite pointing downwards
    	arrowHit.transform.rotation = Quaternion.Euler(0f, 0f, angle);
		arrowHit.GetComponent<ArrowProjectile>().TravelToTarget(target, duration, crit);
	}
	[ClientRpc]
	public void RpcClientCasterAuto(MovingObject target){
		//print($"RpcClientCasterAuto {target.name} shot as {gameObject.name}");
		float distance = Vector2.Distance(target.transform.position, transform.position);
        float duration = distance / 1f * 0.05f;
		AudioMgr sound = GetComponent<AudioMgr>();
		//sound.PlaySound("bow draw");

		GameObject magicAuto = Instantiate(SpellCasterAutoAttackPrefab, transform.position, Quaternion.identity);
		magicAuto.transform.SetParent(transform);
		Vector2 direction = target.transform.position - transform.position;
		//Vector2 direction = transform.position - target.transform.position;
		float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 225f; // -90f to adjust for sprite pointing downwards
    	magicAuto.transform.rotation = Quaternion.Euler(0f, 0f, angle);
		magicAuto.transform.position = new Vector3(transform.position.x, transform.position.y -.25f, 0);
		magicAuto.GetComponent<SpellCasterAA>().TravelToTarget(target, duration);
	}
	/*
	public bool HasLineOfSight(Vector2 start, Vector2 end)
    {
        int x0 = Mathf.FloorToInt(start.x);
        int y0 = Mathf.FloorToInt(start.y);
        int x1 = Mathf.FloorToInt(end.x);
        int y1 = Mathf.FloorToInt(end.y);
        int dx = Mathf.Abs(x1 - x0);
        int dy = Mathf.Abs(y1 - y0);
        int sx = x0 < x1 ? 1 : -1;
        int sy = y0 < y1 ? 1 : -1;
        int err = dx - dy;
        while (true)
        {
            Vector2 current = new Vector2(x0 + .5f, y0 + .5f);
            if (curatorTM.GetComponent<Pathfinding>().reservationWalls.Contains(current)){
                return false;
            }
            if (x0 == x1 && y0 == y1){
                return true;
            }
            int e2 = 2 * err;
            if (e2 > -dy){
                err -= dy;
                x0 += sx;
            }
            if (e2 < dx){
                err += dx;
                y0 += sy;
            }
        }
    }
	*/
	public bool HasLineOfSight(Vector2 start, Vector2 end)
    {
        int x0 = Mathf.FloorToInt(start.x);
        int y0 = Mathf.FloorToInt(start.y);
        int x1 = Mathf.FloorToInt(end.x);
        int y1 = Mathf.FloorToInt(end.y);
        int dx = Mathf.Abs(x1 - x0);
        int dy = Mathf.Abs(y1 - y0);
        int sx = x0 < x1 ? 1 : -1;
        int sy = y0 < y1 ? 1 : -1;
        int err = dx - dy;
        while (true)
        {
            Vector2 current = new Vector2(x0 + .5f, y0 + .5f);
            if (curatorTM.reservationWalls.Contains(current)){
                return false;
            }
            if (x0 == x1 && y0 == y1){
                return true;
            }
            int e2 = 2 * err;
            if (e2 > -dy){
                err -= dy;
                x0 += sx;
            }
            if (e2 < dx){
                err += dx;
                y0 += sy;
            }
        }
    }
	bool AreTilesWithinVision(float vision, Vector2 tile1, Vector2 tile2)
	{
	    float xDiff = Mathf.Abs(tile1.x - tile2.x);
	    float yDiff = Mathf.Abs(tile1.y - tile2.y);
	    return xDiff <= vision && yDiff <= vision;
	}
	bool AreTilesWithinSix(Vector2 tile1, Vector2 tile2)
	{
	    float xDiff = Mathf.Abs(tile1.x - tile2.x);
	    float yDiff = Mathf.Abs(tile1.y - tile2.y);
	    return xDiff <= 5 && yDiff <= 5;
	}
	bool MeleeRange(Vector2 tile1, Vector2 tile2)
	{
	    float xDiff = Mathf.Abs(tile1.x - tile2.x);
	    float yDiff = Mathf.Abs(tile1.y - tile2.y);
	    return xDiff <= 1 && yDiff <= 1;
	}
	bool AreTilesWithinTen(Vector2 tile1, Vector2 tile2)
	{
	    float xDiff = Mathf.Abs(tile1.x - tile2.x);
	    float yDiff = Mathf.Abs(tile1.y - tile2.y);
	    return xDiff <= 10 && yDiff <= 10;
	}
	//[ClientRpc]
	//public void RpcClientCheckColor(){
	//	ClientChangeBackToOriginal();
	//}
	//[ClientRpc]
    //public void RpcChangeToGray()
    //{
    //    ClientChangeToGray();
    //}
	public void ClientChangeBackToOriginal(){
		SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
		if(spriteRenderer.color != originalColor){
        	spriteRenderer.color = originalColor;
		}
	}
	public void ClientChangeToGray(){
		Color gray = new Color(0.5f, 0.5f, 0.5f, 1.0f);
		SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
		if(spriteRenderer.color == originalColor){
        	spriteRenderer.color = gray;
		}
	}
	public IEnumerator DelaySetup()
	{
		yield return new WaitForSeconds(.05f);
		//SetUpCharacter();
		
	}
	public void SetUpCharacter(TurnManager turnManager, Vector2 origin, Vector2 publicorigin)
	{
		if(isServer)
		{
            Origin = origin;
			PublicOrigin = publicorigin;
			if(!GetComponent<PlayerCharacter>()){
				////print($"Mobs Origin is {Origin}");
			}
			curatorTM = turnManager;
			//Material mat = GetComponent<Renderer>().material;
			//mat = new Material(mat); 
        	//rb2D = GetComponent <Rigidbody2D> ();                                               		//Get a component reference to this object's Rigidbody2D
			mainSprite = transform.GetComponent<SpriteRenderer>().sprite;
		}
	}
	
	/*
	if(stamina < 0)
        		{
        			ctSlider.value = stamina/-100f;
					ctImage.color = new Color32 (218, 212, 94, 255);
        		}
        		else
        		{
        		    ctSlider.value = stamina/250f;
        			ctImage.color = new Color32 (208,70,72, 255);
        		}
	*/
	[Server]
	public void ChargeSwingDelay()
    {
        stamina += attackDelay;
		
    }
	[Server]
	public void ChargeSwingDelayOH()
    {
        stamina += attackDelayOH;
    }
	[Server]
	public void ChargeSpellDelay(float value)
    {
        stamina += value;
    }
	[ClientRpc]
	public void StartTrapVision(float duration, int spellRank){
		ShowTraps.Invoke(this, duration, spellRank);
	}
	[Server]
	public void ChargeMoveDelay(float charge)
    {
        stamina += charge;
    }
	
	 
	public const string FIREDAMAGECOLOR = "FF4500";
    public const string POISONDAMAGECOLOR = "32CD32";
    public const string DRAGONDAMAGECOLOR = "FF8C00";
    public const string LEECHDAMAGECOLOR = "8B0000";
    public const string MAGICDAMAGECOLOR = "9370DB";
    public const string COLDDAMAGECOLOR = "00BFFF";
    public const string DISEASEDAMAGECOLOR = "8B4513";
	[ClientRpc]
	public void RpcSpawnPopUp(float value, bool criticalStrike){
		string amount = value.ToString();
		GameObject spawnTextPopUp = StatAsset.Instance.GetObject(this);//Instantiate(PopUpTextPrefab, transform.position, Quaternion.identity);
        AbilityPopUp abilityDisplay = spawnTextPopUp.GetComponent<AbilityPopUp>();
        
		if(!criticalStrike){
        	abilityDisplay.AbilityPopUpBuild(amount, normalHithexColor);
			//sound.PlaySound("blunt hit");
		} else {
        	abilityDisplay.AbilityPopUpBuild(amount, criticalHitHexColor);
			//sound.PlaySound("Critical");
		}
	}
	[ClientRpc]
	public void RpcSpawnTrapDmg(int health, int mana){
		if(health > 0){
			GameObject spawnTextPopUpHP = Instantiate(PopUpTextPrefab, transform.position, Quaternion.identity);
        	AbilityPopUp abilityDisplayHP = spawnTextPopUpHP.GetComponent<AbilityPopUp>();
        	abilityDisplayHP.TrapPopUpHp(health, hpTrapHexColor);
		}
		
		if(mana > 0){
			GameObject spawnTextPopUpMP = Instantiate(PopUpTextPrefab, transform.position, Quaternion.identity);
        	AbilityPopUp abilityDisplayMP = spawnTextPopUpMP.GetComponent<AbilityPopUp>();
        	abilityDisplayMP.TrapPopUpHp(mana, mpTrapHexColor);
		}
	}
	//[ClientRpc]
	//public void RpcSpawnPopUpDodged(){
	//	AudioMgr audio = GetComponent<AudioMgr>();
	//	audio.PlaySound("Dodge");
	//	GameObject spawnTextPopUp = StatAsset.Instance.GetObject(this);//Instantiate(PopUpTextPrefab, transform.position, Quaternion.identity);
    //    AbilityPopUp abilityDisplay = spawnTextPopUp.GetComponent<AbilityPopUp>();
    //    
    //    abilityDisplay.AbilityPopUpBuild("Dodged", normalHithexColor);
	//}
	//[ClientRpc]
	//public void RpcSpawnPopUpParried(){
	//	AudioMgr audio = GetComponent<AudioMgr>();
	//	audio.PlaySound("Parry");
	//	GameObject spawnTextPopUp = StatAsset.Instance.GetObject(this);//Instantiate(PopUpTextPrefab, transform.position, Quaternion.identity);
    //    AbilityPopUp abilityDisplay = spawnTextPopUp.GetComponent<AbilityPopUp>();
    //    
    //    abilityDisplay.AbilityPopUpBuild("Parried", normalHithexColor);
	//	
	//}
	//[ClientRpc]
	//public void RpcSpawnPopUpBlocked(){
	//	AudioMgr audio = GetComponent<AudioMgr>();
	//	audio.PlaySound("Block");
	//	GameObject spawnTextPopUp = StatAsset.Instance.GetObject(this);//Instantiate(PopUpTextPrefab, transform.position, Quaternion.identity);
    //    AbilityPopUp abilityDisplay = spawnTextPopUp.GetComponent<AbilityPopUp>();
    //    
    //    abilityDisplay.AbilityPopUpBuild("Blocked", normalHithexColor);
	//}
	//[ClientRpc]
	//public void RpcSpawnPopUpAbsorbed(){
	//	AudioMgr audio = GetComponent<AudioMgr>();
	//	audio.PlaySound("absorb");
	//	GameObject spawnTextPopUp = StatAsset.Instance.GetObject(this);//Instantiate(PopUpTextPrefab, transform.position, Quaternion.identity);
    //    AbilityPopUp abilityDisplay = spawnTextPopUp.GetComponent<AbilityPopUp>();
    //    
    //    abilityDisplay.AbilityPopUpBuild("Absorbed", normalHithexColor);
	//	
	//}
	[ClientRpc]
	public void RpcSpawnPopUpImmune(){
		AudioMgr audio = GetComponent<AudioMgr>();
		audio.PlaySound("absorb");
		GameObject spawnTextPopUp = StatAsset.Instance.GetObject(this);//Instantiate(PopUpTextPrefab, transform.position, Quaternion.identity);
        AbilityPopUp abilityDisplay = spawnTextPopUp.GetComponent<AbilityPopUp>();
        
        abilityDisplay.AbilityPopUpBuild("Immune", normalHithexColor);
	}
	//[ClientRpc]
	//public void RpcSpawnMagicalEffectDmg(string value, string hexColorCode, bool critical){
	//	////print($"Spawning magical {hexColorCode}");
	//	AudioMgr audio = GetComponent<AudioMgr>();
	//	//audio.PlaySound("moving");
	//	GameObject spawnTextPopUp = StatAsset.Instance.GetObject(this);//Instantiate(PopUpTextPrefab, transform.position, Quaternion.identity);
    //    AbilityPopUp abilityDisplay = spawnTextPopUp.GetComponent<AbilityPopUp>();
    //    
    //    abilityDisplay.MagicalWeapoNEffect(value, hexColorCode, critical);
	//}
	public void SpawnDmgEffect(string spellName, string attacker, string value, string hexColorCode, bool critical){
		////print($"Spawning magical {hexColorCode}");
		AudioMgr audio = GetComponent<AudioMgr>();
		//audio.PlaySound("moving");
		GameObject spawnTextPopUp = StatAsset.Instance.GetObject(this);//Instantiate(PopUpTextPrefab, transform.position, Quaternion.identity);
        AbilityPopUp abilityDisplay = spawnTextPopUp.GetComponent<AbilityPopUp>();
        
        abilityDisplay.MagicalWeapoNEffect(value, hexColorCode, critical);
		Mob mobCheck = GetComponent<Mob>();
		PlayerCharacter pcCheck = GetComponent<PlayerCharacter>();
		string defender = string.Empty;
		if(pcCheck){
			defender = pcCheck.CharacterName;
		}
		if(mobCheck){
			defender = StatAsset.Instance.GetMobName(StatAsset.Instance.GetEnemyToPrefabMapping(), mobCheck.NAME);
		}
        string content = "attacked";
		(int type, int element) = StatAsset.Instance.GetSpellType(spellName);
        CombatLogEntry entry = new CombatLogEntry(attacker, defender, spellName, value, type, element, critical);
		//installing blizzard and other aoe dmg effects so it just says this target was hit for X dmg by spell
		CombatEntryAddition.Invoke(entry);
	}
	
	public void SpawnDeath(){
		CancelCast.Invoke(this);

		Mob mobCheck = GetComponent<Mob>();
		AudioMgr audio = GetComponent<AudioMgr>();
		CircleCollider2D[] circleColliders = GetComponents<CircleCollider2D>();
    	foreach (CircleCollider2D circleCollider in circleColliders)
    	{
    	    circleCollider.enabled = false;
    	}
		SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
		if(AnimatingSpriteCO != null){
				StopCoroutine(AnimatingSpriteCO);
			}
		if (spriteRenderer != null)
    	{
    	    //spriteRenderer.enabled = false; // Hide the sprite
    	    if (mobCheck != null)
    	    {
    	        spriteRenderer.sprite = BloodyDeathSprite;
	        	spriteRenderer.sortingOrder = 9; // Change the sorting order 
    	        if (TIER == 1) { audio.PlaySound("SmallDeath"); }
    	        if (TIER == 2) { audio.PlaySound("MediumDeath"); }
    	        if (TIER == 3) { audio.PlaySound("LargeDeath"); }
				if(CombatPartyView.instance.GetSelected() == mobCheck){
        		    CombatPartyView.instance.TurnOffSelectedWindow();
					UnselectedMO();
        		}
        		if(CombatPartyView.instance.GetTarget() == mobCheck){
        		    CombatPartyView.instance.TurnOffTarget(mobCheck);
					UnTargettedMO();
        		}
				UnTargettedMO();
				UnselectedMO();
				if(SelectedUnitCircle != null){
					UnselectedUnit();
				}
				mobCheck.CheckHoverObject();
    	    }
    	}
		gameObject.layer = LayerMask.NameToLayer("Death");
		healthBarSlider.gameObject.SetActive(false);
    	magicPointBarSlider.gameObject.SetActive(false);
    	ctSlider.gameObject.SetActive(false);
		//GameObject spawnTextPopUp = StatAsset.Instance.GetObject(this);//Instantiate(PopUpTextPrefab, transform.position, Quaternion.identity);
        //AbilityPopUp abilityDisplay = spawnTextPopUp.GetComponent<AbilityPopUp>();
        //abilityDisplay.DeathPopUp("Death", deathhexColor);
		BoxCollider2D deathCollider = GetComponent<BoxCollider2D>();
		if(deathCollider){
			Destroy(deathCollider);
		}

	}
	public void SpawnDeathNpc(bool spawned){
		foreach(var animationObj in AnimationObjectsActive){
			if(animationObj.Value.AnimationObject != null){
				Destroy(animationObj.Value.AnimationObject);
			}
		}
		AnimationObjectsActive.Clear();
        GetComponent<SpriteRenderer>().color = originalColor;
		CancelCast.Invoke(this);
		Mob mobCheck = GetComponent<Mob>();
		AudioMgr audio = GetComponent<AudioMgr>();
		SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
		if(mobCheck){
			CircleCollider2D[] circleColliders = GetComponents<CircleCollider2D>();
    		foreach (CircleCollider2D circleCollider in circleColliders){
    		    circleCollider.enabled = false;
    		}
			if(AnimatingSpriteCO != null){
				StopCoroutine(AnimatingSpriteCO);
			}
			if (spriteRenderer != null)
    		{
    		    //spriteRenderer.enabled = false; // Hide the sprite
    		    if (mobCheck != null)
    		    {
    		        spriteRenderer.sprite = BloodyDeathSprite;
	    	    	spriteRenderer.sortingOrder = 9; // Change the sorting order 
    		        if (TIER == 1) { audio.PlaySound("SmallDeath"); }
    		        if (TIER == 2) { audio.PlaySound("MediumDeath"); }
    		        if (TIER == 3) { audio.PlaySound("LargeDeath"); }
					if(CombatPartyView.instance.GetSelected() == mobCheck){
        			    CombatPartyView.instance.TurnOffSelectedWindow();
						UnselectedMO();
        			}
        			if(CombatPartyView.instance.GetTarget() == mobCheck){
        			    CombatPartyView.instance.TurnOffTarget(mobCheck);
						UnTargettedMO();
        			}
					UnTargettedMO();
					UnselectedMO();
					if(SelectedUnitCircle != null){
						UnselectedUnit();
					}
					mobCheck.CheckHoverObject();
					//string attacker = string.Empty;
					//string defender = StatAsset.Instance.GetMobName(StatAsset.Instance.GetEnemyToPrefabMapping(), mobCheck.NAME);
                    //string content = "attacked";
                    //string amount = string.Empty;
		            //int type = 666; 
		            //int element = 5;
                    //CombatLogEntry entry = new CombatLogEntry(attacker, defender, content, amount, type, element, false);
		            //GainEXPCP.Invoke(entry);
					//if(logEntry.Type == 666){
        			//    textComponent.text = $"<color=white>{logEntry.Defender} was</color> <color=red>slain</color> <color=white>by {logEntry.Attacker}</color>";
        			//    return;
        			//}
					GameObject spawnTextPopUp = StatAsset.Instance.GetObject(this);//Instantiate(PopUpTextPrefab, transform.position, Quaternion.identity);
        			AbilityPopUp abilityDisplay = spawnTextPopUp.GetComponent<AbilityPopUp>();
					
        			abilityDisplay.EXPCPPopUP(mobCheck.CLASSPOINTS, (int)mobCheck.EXPERIENCE, mobCheck.TIER, mobCheck.mobType);
    		    }
    		}
			gameObject.layer = LayerMask.NameToLayer("Death");
			healthBarSlider.gameObject.SetActive(false);
    		magicPointBarSlider.gameObject.SetActive(false);
    		ctSlider.gameObject.SetActive(false);
		} else {
			PlayerCharacter pc = GetComponent<PlayerCharacter>();
			animator.enabled = false;
			if (spriteRenderer != null)
    		{
    		    spriteRenderer.sprite = TombStoneSprite;
	    	    spriteRenderer.sortingOrder = 9; // Change the sorting order 
				spriteRenderer.flipX = false;
    		}
			//if(pc.ClassType == "Fighter" || pc.ClassType == "Fighter" || pc.ClassType == "Fighter" ){
			//}
			if(!spawned){
	    	    audio.PlaySound("CharDeath");
			}
			//gameObject.layer = LayerMask.NameToLayer("Death");
			healthBarSlider.gameObject.SetActive(false);
    		magicPointBarSlider.gameObject.SetActive(false);
    		ctSlider.gameObject.SetActive(false);
			string attacker = string.Empty;
			string defender = pc.CharacterName;
        	string content = "attacked";
        	string amount = string.Empty;
			int type = 666; 
			int element = 5;
        	CombatLogEntry entry = new CombatLogEntry(attacker, defender, content, amount, type, element, false);
			GainEXPCP.Invoke(entry);
		}
		
	}
	[Server]
	public void ApplyStatChange(string stat, float duration, float maxDuration, int value, string buffName, bool buff, int rank, bool food, bool potion, string expirationTime){
		if(!buff){
			value = value * -1;
		}
		
		StatModifier statChange = new StatModifier(StatModifier.Stat.SpellAdd, value, duration, maxDuration, buffName, buff, rank, food, potion, expirationTime);;
		if(stat == "Strength"){
			statChange = new StatModifier(StatModifier.Stat.Strength, value, duration, maxDuration, buffName, buff, rank, food, potion, expirationTime);
		}
		if(stat == "Agility"){
			statChange = new StatModifier(StatModifier.Stat.Agility, value, duration, maxDuration, buffName, buff, rank, food, potion, expirationTime);
		}
		if(stat == "Fortitude"){
			statChange = new StatModifier(StatModifier.Stat.Fortitude, value, duration, maxDuration, buffName, buff, rank, food, potion, expirationTime);
		}
		if(stat == "Arcana"){
			statChange = new StatModifier(StatModifier.Stat.Arcana, value, duration, maxDuration, buffName, buff, rank, food, potion, expirationTime);
		}
		if(stat == "Armor"){
			statChange = new StatModifier(StatModifier.Stat.Armor, value, duration, maxDuration, buffName, buff, rank, food, potion, expirationTime);
		}
		if(stat == "MagicResistance"){
			statChange = new StatModifier(StatModifier.Stat.MagicResistance, value, duration, maxDuration, buffName, buff, rank, food, potion, expirationTime);
		}
		if(stat == "PoisonResistance"){
			statChange = new StatModifier(StatModifier.Stat.PoisonResistance, value, duration, maxDuration, buffName, buff, rank, food, potion, expirationTime);
		}
		if(stat == "DiseaseResistance"){
			statChange = new StatModifier(StatModifier.Stat.DiseaseResistance, value, duration, maxDuration, buffName, buff, rank, food, potion, expirationTime);
		}
		if(stat == "ColdResistance"){
			statChange = new StatModifier(StatModifier.Stat.ColdResistance, value, duration, maxDuration, buffName, buff, rank, food, potion, expirationTime);
		}
		if(stat == "FireResistance"){
			statChange = new StatModifier(StatModifier.Stat.FireResistance, value, duration, maxDuration, buffName, buff, rank, food, potion, expirationTime);
		}
		if(stat == "Lifesteal"){
			statChange = new StatModifier(StatModifier.Stat.Lifesteal, value, duration, maxDuration, buffName, buff, rank, food, potion, expirationTime);
		}
		ApplyStatModifier(statChange, stat);
	}
	bool CheckForFood(string buffName) {
	    bool food = false;
	    // List of all food items to check against
	    string[] foodItems = {
	        "Arudine Pie", "Snake Pie", "Rat Skewer", "Sauerkraut",
	        "Lobster Jerky", "Shishkabob", "Sticky Rat Skewer"
	    };

	    // Check if the buffName is in the array of foodItems
	    foreach (string item in foodItems) {
	        if (buffName == item) {
	            food = true;
	            break;  // Exit loop once a match is found
	        }
	    }
	    return food;
	}
	bool CheckForPotion(string buffName) {
	    bool potion = false;
	    // List of all potion types to check against
	    string[] potionTypes = {
	        "Healing Potion", "Magic Potion", "Haste Potion",
	        "Greater Haste Potion", "Antidote", "Defense Potion",
	        "Greater Defense Potion", "Energy Potion", "Lifesteal Potion",
	        "Rejuvenation Potion"
	    };

	    // Check if the buffName is in the array of potionTypes
	    foreach (string type in potionTypes) {
	        if (buffName == type) {
	            potion = true;
	            break;  // Exit loop once a match is found
	        }
	    }
	    return potion;
	}
	[ClientRpc]
	public void ClientUpdateStatChangesAdd(string stat, float duration, float maxDuration, int value, string buffName, bool buff, int rank, bool potionFood){
		//we need to time stamp these buffs so timer is displayed properly
		string dateTimeWithZone = DateTime.UtcNow.ToString("o");
		DateTime initialTime = DateTime.Parse(dateTimeWithZone); // Convert to DateTime object
		bool potion = CheckForPotion(buffName);;
		bool food = CheckForFood(buffName);;
		
		AddBuff(stat, duration, maxDuration, value, buffName, buff, initialTime, rank, potion, food);
	}
	[ClientRpc]
	public void ClientUpdateStatChangesRemove(string buffName, string stat, bool buff){
		//we need to time stamp these buffs so timer is displayed properly
		RemoveBuff(buffName, stat, buff);
	}
	//[ClientRpc]
	//public void RpcSpawnCombatLog(CombatLogNet message){
	//	//CombatLogEntry entry = new CombatLogEntry(message.Attacker, message.Defender, message.Content, message.Amount, message.Type, message.Element, message.Critical);
	//	//CombatEntryAddition.Invoke(entry);
	//}
	[ClientRpc]
	public void RpcCombatLogDeath(CombatLogNet message){
		//CombatLogEntry entry = new CombatLogEntry(message.Attacker, message.Defender, message.Content, message.Amount, message.Type, message.Element, false);
		//CombatEntryAddition.Invoke(entry);
	}
	[ClientRpc]
	public void RpcGainEXPCP(string attacker, string defender, string content, string amount, int type, int element){//(CombatLogNet message){
		//CombatLogEntry entry = new CombatLogEntry(attacker, defender, content, amount, type, element, false);
		//GainEXPCP.Invoke(entry);
	}
	[Server]
	public void ServerResurrected(){
		if(agent){
			agent.enabled = true;
			agent.isStopped = false;
		}
		CircleCollider2D circleCollider = GetComponent<CircleCollider2D>();
		PlayerCharacter pc = GetComponent<PlayerCharacter>();
		if(circleCollider)
		circleCollider.enabled = true;
		bool inside = curatorTM.GetInside();
        float RevealingRange = inside ? 4f : 7f;
		if (pc.SoFire) {
		    RevealingRange = inside ? 6f : 9f;
		}
		RpcCharRes(RevealingRange);
        //RpcClientRpcChangeColor("3AE300",1f);
	}
	[Server]
	public void ServerSpawnDeath(){
		if(agent.enabled)
				agent.isStopped = true;
		Dying = true;
		CircleCollider2D circleCollider = GetComponent<CircleCollider2D>();
		circleCollider.enabled = false;
		//print("Spawned a server death");
		//RpcCharDeathSpawn();
	}
	
[Server]
public void DamageDealtAOE(MovingObject hittingObject, int value, float threat)
{
	if (Dying)
		return;
	if(FROZEN){
		return;
	}
	if(GetImmune()){
		RpcSpawnPopUpImmune();
		return;
	}
	Mob mob = this.GetComponent<Mob>();
	PlayerCharacter charCheck = GetComponent<PlayerCharacter>();
	if(Invisibility){
		ServerRemoveStatus("Stealthed", "Invisibility", true);
	}
	if (mob){
		if(mob.Resetting){
			//AddRpcCall("absorbed", null, false, false, null, null, false);
			//RpcSpawnPopUpAbsorbed();
			return;
		}
	}
	if(absorbSpellShield > 0f){
		int savedValue = value;
		value -= (int)absorbSpellShield;
		if(value < 0){
			value = 0;
		}
		absorbSpellShield -= savedValue;
		if(absorbSpellShield < 0){
			absorbSpellShield = 0f;
		}
	}
	if(Feared){
		int roll = UnityEngine.Random.Range(1,101);
		if(roll < 20){
			if(fearCoroutine != null){
				StopCoroutine(fearCoroutine);
			}
			ServerRemoveStatus("Fear", fearSpell, false);
			if(agent.enabled){
				agent.isStopped = false;
				agent.ResetPath();
			}
			if(mob)
			mob.DamageTaken();
			fearCoroutine = null;
		}
	}
	if(Snared){
		int roll = UnityEngine.Random.Range(1,101);
		if(roll < 20){
			if(rootCoroutine != null){
				StopCoroutine(rootCoroutine);
			}
			ServerRemoveStatus("Root", rootSpell, false);
			if(agent.enabled)
			agent.isStopped = false;
    		Snared = false;
			if(mob)
			mob.DamageTaken();
			rootCoroutine = null;
		}
	}
	curatorTM.CombatCalled();
	cur_hp -= value;
	if (Mesmerized){
		Mesmerized = false;
		if(resting){
			resting = false;
		}
		if(agent.enabled)
		agent.isStopped = false;
	}
	if (cur_hp <= 0){
		cur_hp = 0;
		if (mob){
			mob.Die();
			return;
		} else {
			if(charCheck){
				if(!Dying){
					ServerStopCasting();
					Dying = true;
					string attacker = string.Empty;
            		string defender = string.Empty;
            		string content = "attacked";
            		string amount = value.ToString();
            		PlayerCharacter pcAttackerCheck = hittingObject.GetComponent<PlayerCharacter>();
            		Mob mobAttackerCheck = hittingObject.GetComponent<Mob>();
            		PlayerCharacter pcDefenderCheck = GetComponent<PlayerCharacter>();
            		Mob mobDefenderCheck = GetComponent<Mob>();
            		if(pcAttackerCheck != null){
            		    attacker = pcAttackerCheck.CharacterName;
            		} else {
            		    attacker = mobAttackerCheck.NAME;
            		}
            		if(pcDefenderCheck != null){
            		    defender = pcDefenderCheck.CharacterName;
            		} else {
            		    defender = mobDefenderCheck.NAME;
            		}
					int type = 666; 
					int element = 5;
            		CombatLogNet cNet = new CombatLogNet(attacker, defender, content, amount, type, element, false);
					charCheck.RpcCombatLogDeath(cNet);
					charCheck.DeathEXP();
					StopATTACKINGMob();
					if(agent.enabled)
					agent.isStopped = true;
					//agent.enabled = false;
					CircleCollider2D circleCollider = GetComponent<CircleCollider2D>();
					circleCollider.enabled = false;
					DeathCharacter.Invoke(charCheck.assignedPlayer.connectionToClient, charCheck.CharID, curatorTM);
					RemoveCharReserve.Invoke(this);
					//RevokePlayerAuthority();
					RpcCharDeath();
					DeadChar.Invoke(this, charCheck.assignedMatch);
					Target = null;
				}
			}
		}
	} else {
		if(charCheck  && !curatorTM.GetDuelMode()){
	        TakeDamageCharacter.Invoke(charCheck.assignedPlayer.connectionToClient, cur_hp, charCheck.CharID);
		}
	}
	if (mob != null && !hittingObject.Dying && !Dying){
		if(!mob.threatList.ContainsKey(hittingObject)){
			mob.threatList.Add(hittingObject, threat);
		} else {
			mob.threatList[hittingObject] += threat;
		}
		mob.DamageTaken();
	}
}
    private Queue<RpcPopUpSpawner> rpcQueue = new Queue<RpcPopUpSpawner>();
	public IEnumerator SendRpcQueue()
    {
		float interval = 0.1f;
        while (true)
        {
			int count = 0;
            if (rpcQueue.Count > 0)
            {
				
                var effectsToSend = new List<RpcPopUpSpawner>();
                while (rpcQueue.Count > 0 && count < 1)
                {
                    effectsToSend.Add(rpcQueue.Dequeue());
					count ++;
                }

                RpcSpawnRpcPopUpSpawnerBatch(effectsToSend.ToArray());
            }

            yield return new WaitForSeconds(interval);
        }
    }
	[ClientRpc]
	public void RpcOverTimeAnimation(string spell, MovingObject reciever){
		RpcAnimateSpell(spell, reciever);
	}
	[ClientRpc]
	public void RpcPostFullOverTime(string spell, string attacker, string defender, string amount, string content){
        int type = 1; 
		int element = 5;
		(type, element) = StatAsset.Instance.GetSpellType(spell);
        CombatLogEntry entry = new CombatLogEntry(attacker, defender, content, amount, type, element, false);
		CombatEntryAddition.Invoke(entry);
	}
	[ClientRpc]
    public void RpcSpawnRpcPopUpSpawnerBatch(RpcPopUpSpawner[] effects)
    {
		AudioMgr audio = GetComponent<AudioMgr>();
		string attacker = string.Empty;
		string defender = string.Empty;
        string amount = string.Empty;
        string content = string.Empty;
        int type = 1; 
		int element = 5;
		foreach (var effect in effects){
			if(!string.IsNullOrEmpty(effect.spell) && effect.spell == "HealShrine"){
				attacker = effect.SpellOwner;
					defender = effect.SpellTarget;
            		amount = effect.value;
            		content = effect.spell;
				//Healed for 20 hp 1 mp"
        		RpcAnimateSpell("Heal", this);
				type = 7;
                CombatLogEntry entry = new CombatLogEntry(attacker, defender, content, amount, type, element, effect.critical);
		        CombatEntryAddition.Invoke(entry);
				GameObject spawnTextPopUp = StatAsset.Instance.GetObject(this);//Instantiate(PopUpTextPrefab, transform.position, Quaternion.identity);
        		AbilityPopUp abilityDisplay = spawnTextPopUp.GetComponent<AbilityPopUp>();
        		abilityDisplay.MagicalWeapoNEffect("20HP 1MP", effect.magical, effect.critical);
				continue;
			}
			
			if(effect.isSpell){
				if(effect.targetArea == Vector2.zero){
					attacker = effect.SpellOwner;
					defender = effect.SpellTarget;
            		amount = effect.value;
            		content = effect.spell;
					
					if(effect.spell == "Blocked" || effect.spell == "Parry" || effect.spell == "Dodge" || effect.spell == "Miss" || effect.spell == "Eat" || effect.spell == "Drink"){
						AnimationAndSound(effect.spell, effect.weaponTypePopUp, effect.intiator);
						if(effect.spell == "Eat" || effect.spell == "Drink"){
							continue;
						}
					} else {
						if(!StatAsset.Instance.SkillShot(effect.spell)){
							//effect.intiator.RpcCastingSpell(effect.spell, effect.weaponTypePopUp);
							if(!StatAsset.Instance.IsTacticianSpell(effect.spell)){
								if(effect.spell != "Greater Poison" && effect.spell != "Lesser Poison" && effect.spell != "Poison" ){//&& effect.spell != "Spit"
									effect.intiator.RpcAnimateSpell(effect.spell, effect.reciever);
								}
								GameObject spawnTextPopUp = StatAsset.Instance.GetObject(this);//Instantiate(PopUpTextPrefab, transform.position, Quaternion.identity);
        						AbilityPopUp abilityDisplay = spawnTextPopUp.GetComponent<AbilityPopUp>();
        						abilityDisplay.MagicalWeapoNEffect(effect.value, effect.magical, effect.critical);
							}
						}
					}
					if(StatAsset.Instance.OvertimeCheck(effect.spell)){
						continue;
					}
					(type, element) = StatAsset.Instance.GetSpellType(effect.spell);
					if(effect.spell == "Poison"){
						if(effect.intiator != null){
							Mob mob = effect.intiator.GetComponent<Mob>();
							if(mob){
								effect.spell = "Moderate Poison";
			            		content = effect.spell;
							}
						}
					}
					
                	CombatLogEntry entry = new CombatLogEntry(attacker, defender, content, amount, type, element, effect.critical);
		        	CombatEntryAddition.Invoke(entry);
					
					continue;
				} else {
					//effect.intiator.RpcCastingSpell(effect.spell, effect.weaponTypePopUp);
					effect.intiator.RpcAnimateSpellAOE(effect.spell, effect.targetArea);
					continue;
				}
			}
			attacker = effect.SpellOwner;
			defender = effect.SpellTarget;
            amount = effect.value;
			if(!string.IsNullOrEmpty(effect.spell))
            content = effect.spell;
			if(effect.value == "absorbed"){
				audio.PlaySound("absorb");
				GameObject spawnTextPopUp = StatAsset.Instance.GetObject(this);//Instantiate(PopUpTextPrefab, transform.position, Quaternion.identity);
        		AbilityPopUp abilityDisplay = spawnTextPopUp.GetComponent<AbilityPopUp>();

        		abilityDisplay.AbilityPopUpBuild("Absorbed", normalHithexColor);
			} else {
				if(effect.isWeaponNoise){
					int attackType = 1;
					if(effect.spell.Contains("Double")){
						attackType = 2;
					}
					effect.intiator.ClientAttackSound(effect.weaponTypePopUp, effect.critical, attackType);
				}
				
				if(!string.IsNullOrEmpty(effect.spell)){
					if(effect.spell.Contains("DoubleAttack")){
						continue;
					}
					(type, element) = StatAsset.Instance.GetSpellType(effect.spell);
					if(effect.spell == "Ice")
					type = 200;
					if(effect.spell == "Fire")
					type = 201;
					if(effect.spell == "Poison")
					type = 202;
					if(effect.spell == "Disease")
					type = 203;
					if(effect.spell == "Magic")
					type = 204;
					if(effect.spell == "Leech")
					type = 205;
				}
                CombatLogEntry entry = new CombatLogEntry(attacker, defender, content, amount, type, element, effect.critical);
		        CombatEntryAddition.Invoke(entry);
				GameObject spawnTextPopUp = StatAsset.Instance.GetObject(this);//Instantiate(PopUpTextPrefab, transform.position, Quaternion.identity);
        		AbilityPopUp abilityDisplay = spawnTextPopUp.GetComponent<AbilityPopUp>();
        		abilityDisplay.MagicalWeapoNEffect(effect.value, effect.magical, effect.critical);
			}
            // Handle the magical effect damage on the client side
            //Debug.Log($"Received Magical Effect: Value={effect.value}, Magical={effect.magical}, Critical={effect.critical}");
        }
    }
	public void AddRpcCall(string value, string magical, bool critical, bool _isSpell, string _spell, string _weaponType, bool autoAttackNoise, string spellOwner, string spellTarget,
	MovingObject _intiator, MovingObject _reciever, Vector2 targetAoeArea)
    {
        rpcQueue.Enqueue(new RpcPopUpSpawner(value, magical, critical, _isSpell, _spell, _weaponType, autoAttackNoise, spellOwner, spellTarget, _intiator, _reciever, targetAoeArea));
    }
[System.Serializable]
public class RpcPopUpSpawner
{
    public string value;
    public string magical;
    public bool critical;
	public bool isSpell;
	public string spell;
	public string weaponTypePopUp;
	public bool isWeaponNoise;
	public string SpellOwner;
	public string SpellTarget;
	public MovingObject intiator;
	public MovingObject reciever;
	public Vector2 targetArea;

	public RpcPopUpSpawner(){}
    public RpcPopUpSpawner(string value, string magical, bool critical, bool _isSpell, string _spell, string _weaponTypePopUp, bool weaponNoise, string spellOwner, string spellTarget,
	MovingObject _intiator, MovingObject _reciever, Vector2 _targetArea)
    {
        this.value = value;
        this.magical = magical;
        this.critical = critical;
        this.isSpell = _isSpell;
		this.spell = _spell;
        this.weaponTypePopUp = _weaponTypePopUp;
		this.isWeaponNoise = weaponNoise;
		this.SpellOwner = spellOwner;
		this.SpellTarget = spellTarget;
		this.intiator = _intiator;
		this.reciever = _reciever;
		this.targetArea = _targetArea;
    }
}
[Server]
public void DamageDealt(MovingObject hittingObject, int value, bool critical, bool castedSpell, float threat, string magical, bool autoAttack, string weaponTypeSent, string spellName)
{
	if (Dying)
		return;
	if(FROZEN){
		return;
	}
	if(GetImmune()){
		RpcSpawnPopUpImmune();
		return;
	}
	if(GetProtected()){
		if(protectingMO != null){
			if(!protectingMO.Dying){
				protectingMO.DamageDealt(hittingObject, value, critical, castedSpell, threat, magical, autoAttack, weaponTypeSent, spellName);
				print($"{protectingMO.gameObject.name} protected {gameObject.name} from damage!!");
				return;
			}
		}
	}
	
	Mob mob = this.GetComponent<Mob>();
	PlayerCharacter charCheck = GetComponent<PlayerCharacter>();
	Mob mobHitter = hittingObject.GetComponent<Mob>();
	PlayerCharacter charHitter = hittingObject.GetComponent<PlayerCharacter>();
	string ownerName = string.Empty;
    string targetName = string.Empty;
    if(charHitter){
       ownerName = charHitter.CharacterName;
    } else {
		ownerName = StatAsset.Instance.GetMobName(StatAsset.Instance.GetEnemyToPrefabMapping(), mobHitter.NAME);
    }
    if(charCheck){
       targetName = charCheck.CharacterName;
    } else {
		targetName = StatAsset.Instance.GetMobName(StatAsset.Instance.GetEnemyToPrefabMapping(), mob.NAME);
    }
	if(Invisibility){
		ServerRemoveStatus("Stealthed", "Invisibility", true);
	}
	if (mob){
		if(mob.Resetting){
			AddRpcCall("absorbed", null, false, false, null, null, false, ownerName, targetName, hittingObject, this, Vector2.zero);
			//RpcSpawnPopUpAbsorbed();
			return;
		}
	}
	if(absorbSpellShield > 0f){
		int savedValue = value;
		value -= (int)absorbSpellShield;
		if(value < 0){
			value = 0;
		}
		absorbSpellShield -= savedValue;
		if(absorbSpellShield < 0){
			absorbSpellShield = 0f;
		}
	}
	if(Feared){
		int roll = UnityEngine.Random.Range(1,101);
		if(roll < 20){
			if(fearCoroutine != null){
				StopCoroutine(fearCoroutine);
			}
			ServerRemoveStatus("Fear", fearSpell, false);
			if(agent.enabled){
				agent.isStopped = false;
				agent.ResetPath();
			}
			if(mob)
			mob.DamageTaken();
			fearCoroutine = null;
		}
	}
	if(Snared){
		int roll = UnityEngine.Random.Range(1,101);
		if(roll < 20){
			if(rootCoroutine != null){
				StopCoroutine(rootCoroutine);
			}
			ServerRemoveStatus("Root", rootSpell, false);
			if(agent.enabled)
			agent.isStopped = false;
    		Snared = false;
			if(mob)
			mob.DamageTaken();
			rootCoroutine = null;
		}
	}
	curatorTM.CombatCalled();
	cur_hp -= value;
	if (Mesmerized){
		Mesmerized = false;
		if(resting){
			resting = false;
		}
		if(agent.enabled)
		agent.isStopped = false;
	}
	
	if (!string.IsNullOrEmpty(magical) && cur_hp > 0 && !autoAttack && !castedSpell){
		////print("This was a magical pop-up from an NFT doing some damage");
		//RpcSpawnMagicalEffectDmg(value.ToString(), magical, critical);
		AddRpcCall(value.ToString(), magical, critical, false, spellName, weaponTypeSent, autoAttack, ownerName, targetName, hittingObject, this, Vector2.zero);

	}
	if(autoAttack){
		string color = normalHithexColor;
		if(critical){
			color = criticalHitHexColor;
		}
		AddRpcCall(value.ToString(), color, critical, false, spellName, weaponTypeSent, autoAttack, ownerName, targetName, hittingObject, this, Vector2.zero);
	}

	if (castedSpell){
		AddRpcCall(value.ToString(), magical, critical, castedSpell, spellName, weaponTypeSent, autoAttack, ownerName, targetName, hittingObject, this, Vector2.zero);
	}
	if (cur_hp <= 0){
		cur_hp = 0;
		ServerStopCasting();
		if (mob){
			mob.Die();
			return;
		} else {
			if(charCheck){
				if(!Dying){
					Dying = true;
					string attacker = string.Empty;
            		string defender = string.Empty;
            		string content = "attacked";
            		string amount = value.ToString();
            		PlayerCharacter pcAttackerCheck = hittingObject.GetComponent<PlayerCharacter>();
            		Mob mobAttackerCheck = hittingObject.GetComponent<Mob>();
            		PlayerCharacter pcDefenderCheck = GetComponent<PlayerCharacter>();
            		Mob mobDefenderCheck = GetComponent<Mob>();
            		if(pcAttackerCheck != null){
            		    attacker = pcAttackerCheck.CharacterName;
            		} else {
            		    attacker = mobAttackerCheck.NAME;
            		}
            		if(pcDefenderCheck != null){
            		    defender = pcDefenderCheck.CharacterName;
            		} else {
            		    defender = mobDefenderCheck.NAME;
            		}
					int type = 666; 
					int element = 5;
            		CombatLogNet cNet = new CombatLogNet(attacker, defender, content, amount, type, element, false);
					charCheck.RpcCombatLogDeath(cNet);
					charCheck.DeathEXP();
					StopATTACKINGMob();
					if(agent.enabled)
					agent.isStopped = true;
					//agent.enabled = false;
					CircleCollider2D circleCollider = GetComponent<CircleCollider2D>();
					circleCollider.enabled = false;
					DeathCharacter.Invoke(charCheck.assignedPlayer.connectionToClient, charCheck.CharID, curatorTM);
					RemoveCharReserve.Invoke(this);
					//RevokePlayerAuthority();
					RpcCharDeath();
					DeadChar.Invoke(this, charCheck.assignedMatch);
					Target = null;
				}
			}
		}
	} else {
		//if(value > 0){
		//	RpcClientRpcChangeColor("E33A00", 1f);
		//}
		if(charCheck  && !curatorTM.GetDuelMode()){
	        TakeDamageCharacter.Invoke(charCheck.assignedPlayer.connectionToClient, cur_hp, charCheck.CharID);
		}
	}
	if (mob != null && !hittingObject.Dying && !Dying){
		if(!mob.threatList.ContainsKey(hittingObject)){
			mob.threatList.Add(hittingObject, threat);
		} else {
			mob.threatList[hittingObject] += threat;
		}
		Mob hittingMob = hittingObject.GetComponent<Mob>();
		if(hittingMob){
			if(hittingMob.GetCharmedController() != null){
				if(!mob.threatList.ContainsKey(hittingMob.GetCharmedController())){
					mob.threatList.Add(hittingMob.GetCharmedController(), threat);
				} else {
					mob.threatList[hittingMob.GetCharmedController()] += threat/4;
				}
			}
		}
		mob.DamageTaken();
	}
}
[Server]
public void DamageDealtPlayer(ScenePlayer sPlayer, int value, float criticalValue, bool showPopUp, float threat, string magical, string spellName)
{
	if (Dying)
		return;
	if(FROZEN){
		return;
	}
	if(Invisibility){
		ServerRemoveStatus("Stealthed", "Invisibility", true);
	}
	Mob mob = this.GetComponent<Mob>();
	PlayerCharacter charCheck = GetComponent<PlayerCharacter>();
	string ownerName = sPlayer.playerName;
    string targetName = string.Empty;
    if(charCheck){
       targetName = charCheck.CharacterName;
    } else {
		targetName = StatAsset.Instance.GetMobName(StatAsset.Instance.GetEnemyToPrefabMapping(), mob.NAME);
    }
	if (mob)
	{
		if(mob.Resetting){
			AddRpcCall("absorbed", null, false, false, null, null, false, ownerName, targetName, null, this, Vector2.zero);
			//RpcSpawnPopUpAbsorbed();
			return;
		}
	}
	if(absorbSpellShield > 0f){
		int savedValue = value;
		value -= (int)absorbSpellShield;
		if(value < 0){
			value = 0;
		}
		absorbSpellShield -= savedValue;
		if(absorbSpellShield < 0){
			absorbSpellShield = 0f;
		}
	}
	
	curatorTM.CombatCalled();
	bool critical = false; //Random.value >= criticalValue;
	cur_hp -= value;
	
	if (Mesmerized){
		Mesmerized = false;
		agent.isStopped = false;
		if(resting){
			RemoveServerRest("Rest");
		}
	}
	if (!string.IsNullOrEmpty(magical))
	{
		////print("This was a magical pop-up from an NFT doing some damage");
		AddRpcCall(value.ToString(), magical, false, true, spellName, null, false, ownerName, targetName, null, this, Vector2.zero);
		//RpcSpawnMagicalEffectDmg(value.ToString(), magical, false);
	}

	if (showPopUp)
	{
		if (value <= 0)
			AddRpcCall("absorbed", null, false, false, null, null, false, ownerName, targetName, null, this, Vector2.zero);
			//RpcSpawnPopUpAbsorbed();
		else
			RpcSpawnPopUp(value, critical);
	}

	if (cur_hp <= 0)
	{
		ServerStopCasting();
		if (mob)
		{
			//RpcSpawnDeath();
			mob.Die();
			
			return;
		}
		else
		{
			cur_hp = 0;
			if(charCheck){
				if(!Dying){
					Dying = true;
					string attacker = string.Empty;
            		string defender = string.Empty;
            		string content = "attacked";
            		string amount = value.ToString();
            		PlayerCharacter pcDefenderCheck = GetComponent<PlayerCharacter>();
            		Mob mobDefenderCheck = GetComponent<Mob>();
					attacker = sPlayer.playerName;
            		if(pcDefenderCheck != null){
            		    defender = pcDefenderCheck.CharacterName;
            		} else {
            		    defender = mobDefenderCheck.NAME;
            		}
					int type = 666; 
					int element = 5;
            		CombatLogNet cNet = new CombatLogNet(attacker, defender, content, amount, type, element, false);
					charCheck.RpcCombatLogDeath(cNet);
					charCheck.DeathEXP();
					StopATTACKINGMob();
					if(agent.enabled)
					agent.isStopped = true;
					//agent.enabled = false;
					CircleCollider2D circleCollider = GetComponent<CircleCollider2D>();
					circleCollider.enabled = false;
					DeathCharacter.Invoke(charCheck.assignedPlayer.connectionToClient, charCheck.CharID, curatorTM);
					RemoveCharReserve.Invoke(this);
					//RevokePlayerAuthority();
					RpcCharDeath();
					DeadChar.Invoke(this, charCheck.assignedMatch);
					
					Target = null;
				}
			}
		}
	} else {
		if(charCheck && !curatorTM.GetDuelMode()){
	        TakeDamageCharacter.Invoke(charCheck.assignedPlayer.connectionToClient, cur_hp, charCheck.CharID);
		}
	}
}

[Server]
void RevokePlayerAuthority()
{
    NetworkIdentity identity = this.GetComponent<NetworkIdentity>();

    // Ensure it's running on server, has authority, and is assigned to a client
    if (NetworkServer.active && identity.hasAuthority && identity.connectionToClient != null)
    {
        identity.RemoveClientAuthority();
    }
}
	[ClientRpc]
	public void RpcCharDeath(){
		AudioMgr audio = GetComponent<AudioMgr>();
		animator.enabled = false;
		SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
		if (spriteRenderer != null)
    	{
			//if(AnimatingSpriteCO != null){
			//	StopCoroutine(AnimatingSpriteCO);
			//}
			//Play animation for death so it goes from cloud tomb to flashing tomb yeah babyyyy
    	    spriteRenderer.sprite = TombStoneSprite;
	        spriteRenderer.sortingOrder = 9; // Change the sorting order 
			spriteRenderer.flipX = false;
    	    audio.PlaySound("CharDeath");
    	}
		//gameObject.layer = LayerMask.NameToLayer("Death");
		healthBarSlider.gameObject.SetActive(false);
    	magicPointBarSlider.gameObject.SetActive(false);
    	ctSlider.gameObject.SetActive(false);
		GameObject spawnTextPopUp = StatAsset.Instance.GetObject(this);//Instantiate(PopUpTextPrefab, transform.position, Quaternion.identity);
        AbilityPopUp abilityDisplay = spawnTextPopUp.GetComponent<AbilityPopUp>();
        
        abilityDisplay.DeathPopUp("Death", deathhexColor);
		StartCoroutine(DelayedVisionDeath());
		ScenePlayer.localPlayer.RemoveSelectedCharacter(this.gameObject);
		UnTargettedMO();
		UnselectedMO();
		if(SelectedUnitCircle != null){
			UnselectedUnit();
		}
	}
	[ClientRpc]
	public void RpcCharDeathSpawn(){
		AudioMgr audio = GetComponent<AudioMgr>();
		print("toggling client RPC death on spawn");
		animator.enabled = false;
		SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
		if (spriteRenderer != null)
    	{
			//if(AnimatingSpriteCO != null){
			//	StopCoroutine(AnimatingSpriteCO);
			//}
			//Play animation for death so it goes from cloud tomb to flashing tomb yeah babyyyy
    	    spriteRenderer.sprite = TombStoneSprite;
	        spriteRenderer.sortingOrder = 9; // Change the sorting order 
			spriteRenderer.flipX = false;
    	}
		//gameObject.layer = LayerMask.NameToLayer("Death");
		healthBarSlider.gameObject.SetActive(false);
    	magicPointBarSlider.gameObject.SetActive(false);
    	ctSlider.gameObject.SetActive(false);
	}
	[ClientRpc]
	public void RpcEquippedLightItem(float RevealingRange){
		print($"new reveal range for character {gameObject.name} is now {RevealingRange}");
		FoggyWar fogofWar = GameObject.Find("FogMachine").GetComponent<FoggyWar>();
		if(ScenePlayer.localPlayer.GetFriendlyList().Contains(this)){
		    fogofWar.AddCharacter(this.gameObject, RevealingRange);
		    fogofWar.UpdateFogOfWar(this.gameObject, transform.position);
        } else {
            fogofWar.AddEnemyPlayerCharacterToEnemies(this);
        }
	}
	[ClientRpc]
	public void RpcCharRes(float RevealingRange){
		SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
		if (spriteRenderer != null)
    	{
	        spriteRenderer.sortingOrder = 15; // Change the sorting order 
			spriteRenderer.flipX = false;
    	}
		if(ContainerUIButtons.Instance != null){
			ContainerUIButtons.Instance.PlayResSound();
		}
		//gameObject.layer = LayerMask.NameToLayer("Death");
		if(ScenePlayer.localPlayer.GetFriendly(this)){
			healthBarSlider.gameObject.SetActive(true);
    		magicPointBarSlider.gameObject.SetActive(true);
    		ctSlider.gameObject.SetActive(true);
		}
		animator.enabled = true;
		GameObject spawnTextPopUp = StatAsset.Instance.GetObject(this);//Instantiate(PopUpTextPrefab, transform.position, Quaternion.identity);
        AbilityPopUp abilityDisplay = spawnTextPopUp.GetComponent<AbilityPopUp>();
        abilityDisplay.DeathPopUp("Resurrected", GREENHEALCOLOR);
		FoggyWar fogofWar = GameObject.Find("FogMachine").GetComponent<FoggyWar>();
		//print($"Sending AddCharacter to {CharacterName} in the fog machaine for {RevealingRange} vision!!");
        if(ScenePlayer.localPlayer.GetFriendlyList().Contains(this)){
		    fogofWar.AddCharacter(this.gameObject, RevealingRange);
		    fogofWar.UpdateFogOfWar(this.gameObject, transform.position);
        } else {
            fogofWar.AddEnemyPlayerCharacterToEnemies(this);
        }
		died = false;
		spawnedDead = false;
	}
	IEnumerator DelayedVisionDeath(){
		yield return new WaitForSeconds(5f);
		RemoveFogParticipant.Invoke(this, false);
	}
	[ClientRpc]
	public void RpcMobEXPCPDisplay(int CP, int EXP, int tier, string type){
		//StartCoroutine(RpcMobEXPCPDisplayDeath(CP, EXP, tier, type));
		//Mob mob = GetComponent<Mob>();
		//if(mob){
			//GameObject spawnTextPopUp = StatAsset.Instance.GetObject(this);//Instantiate(PopUpTextPrefab, transform.position, Quaternion.identity);
        	//AbilityPopUp abilityDisplay = spawnTextPopUp.GetComponent<AbilityPopUp>();
        	//abilityDisplay.EXPCPPopUP(CP, EXP, tier, type);
		//}
	}
	IEnumerator RpcMobEXPCPDisplayDeath(int CP, int EXP, int tier, string type){
		float randomCheckAmount = UnityEngine.Random.Range(.1f, 1f);
		yield return new WaitForSeconds(randomCheckAmount);		
		Mob mob = GetComponent<Mob>();
		if(mob){
			GameObject spawnTextPopUp = StatAsset.Instance.GetObject(this);//Instantiate(PopUpTextPrefab, transform.position, Quaternion.identity);
        	AbilityPopUp abilityDisplay = spawnTextPopUp.GetComponent<AbilityPopUp>();
        
        	abilityDisplay.EXPCPPopUP(CP, EXP, tier, type);
		}
	}
	bool ReadyToPlay = false;
    
	
	//[Server]
	//public void DamageDealtToPlayer(int value){
	//	bool critical = Random.value >= 0.5f;
	//	
	//	cur_hp = cur_hp - value;
	//	if(cur_hp <= 0){
	//		cur_hp = max_hp;
	//	}
	//	if(value <= 0){
	//		RpcSpawnPopUpAbsorbed();
	//	} else {
	//		RpcSpawnPopUp(value, critical);
	//	}
	//}
	//[Server]
	//public void HealingReceived(int value){
	//	cur_hp = cur_hp + value;
	//	if(cur_hp > max_hp){
	//		cur_hp = max_hp;
	//		return;
	//	}
	//}
	private float lastFogUpdate = -1f;
private float fogUpdateInterval = 0.5f;  // Interval in seconds
[ClientRpc]
public void NewFogUpdate(Vector2 newspot)
{
    float currentTime = Time.time;

    if (currentTime - lastFogUpdate < fogUpdateInterval)
    {
        return;
    }

    lastFogUpdate = currentTime;  // Update the last called time

    if (newspot == null)
    {
        return;
    }

    Mob mob = GetComponent<Mob>();
    GameObject fogMachine = GameObject.Find("FogMachine");
    if (fogMachine)
    {
        FoggyWar fogofWar = fogMachine.GetComponent<FoggyWar>();
        if (fogofWar)
        {
        	if(ScenePlayer.localPlayer.GetFriendlyList().Contains(this)){
			    fogofWar.UpdateFogOfWar(this.gameObject, transform.position);
        	} else {
        	    fogofWar.AddEnemyPlayerCharacterToEnemies(this);
        	}
        }
    }
}
public void FogUpdateTest()
{
    GameObject fogMachine = GameObject.Find("FogMachine");
    if (fogMachine)
    {
        FoggyWar fogofWar = fogMachine.GetComponent<FoggyWar>();
        if (fogofWar)
        {
        	if(ScenePlayer.localPlayer.GetFriendlyList().Contains(this)){
			    fogofWar.UpdateFogOfWar(this.gameObject, transform.position);
        	} else {
        	    fogofWar.AddEnemyPlayerCharacterToEnemies(this);
        	}
        }
    }
}
/*
	[ClientRpc]
	public void NewFogUpdate(Vector2 newspot)
	{
		if(newspot == null){
			return;
		}
		Mob mob = GetComponent<Mob>();
		GameObject fogMachine = GameObject.Find("FogMachine");
		if(fogMachine){
			FoggyWar fogofWar = fogMachine.GetComponent<FoggyWar>();
			if(fogofWar){
				if(mob){
					fogofWar.UpdateMob(mob);
				} else {
					fogofWar.UpdateFogOfWar(this.gameObject, newspot);
				}
			}
		}
	}
	*/
	[Server]
	void ServerUpdateDirection() {
	    // Check if we have a path and it has at least one corner
	    NavMeshAgent navMeshAgent = GetComponent<NavMeshAgent>();
	    if (navMeshAgent.hasPath && navMeshAgent.path.corners.Length > 1) {
	        // Use the second corner to look ahead. The first corner is the current position.
	        Vector3 nextCorner = navMeshAgent.path.corners[1];
	        Vector3 direction = (nextCorner - transform.position).normalized;
			Vector3 directionToNextCorner = nextCorner - transform.position;
	        // Here, adapt this part to your needs. This is just a simple directional check.
	        if (Mathf.Abs(directionToNextCorner.x) > Mathf.Abs(directionToNextCorner.y))
            {
                RpcUpdateFacingDirection(directionToNextCorner.x > 0);
            }
			// Simplified directional check for left or right movement.
        	float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        	// Determine the direction string based on the angle
        	string directionString = GetDirectionString(angle);
        	Mob mob = GetComponent<Mob>();
			PlayerCharacter pc = GetComponent<PlayerCharacter>();
			string ourName = string.Empty;
			if(mob){
				ourName = mob.NAME;
			}
			if(pc){
				ourName = pc.CharacterName;
			}
        	Debug.Log(ourName + " is facing direction: " + directionString);	
			if(DirectionString != directionString){
				DirectionString = directionString;
				RpcSetDirectionFacing(directionString);
			}
        	//bool facingRight = direction.x > 0;
        	//RpcUpdateFacingDirection(facingRight);
		}
	}
	[ClientRpc]
	void RpcUpdateWalkingState(bool isWalking)
	{
		if(GetComponent<Mob>()){
			return;
		}
	    // This will be executed on all clients, synchronizing the animation state
	    animator.SetBool("IsWalking", isWalking);
	}
	void RpcUpdateWalkingStateTest(bool isWalking)
	{
		if(GetComponent<Mob>()){
			return;
		}
	    // This will be executed on all clients, synchronizing the animation state
	    animator.SetBool("IsWalking", isWalking);
	}
	[ClientRpc]
	public void RpcUpdateFacingDirection(bool rightFacing)
	{
		SpriteRenderer sRend = GetComponent<SpriteRenderer>();
		sRend.flipX = rightFacing; // Flip if facing left
	}
	[ClientRpc]
	public void RpcPlayWalkingSound(){
		AudioMgr audio = GetComponent<AudioMgr>();
		audio.PlaySound("moving");
	}
	public void TryHealingFountain(){
		CmdHealingFountain();
	}
	[Command]
	void CmdHealingFountain(){
		PlayerCharacter pc = GetComponent<PlayerCharacter>();
		if(pc){
			curatorTM.HealingShrineClicked(pc);
		}
	}
    protected void Movement (Vector3 end)
    {
        StartCoroutine(LerpPosition(end, moveTime));
    }
    //Function used to move this game object to the target position smoothly over a number of seconds
    IEnumerator LerpPosition(Vector2 targetPosition, float duration)
    {
		//LerpInProgress = true;
        float time = 0;
        Vector2 startPosition = transform.position;
        //////print($"{this.gameObject.name} player is moving to position: {targetPosition} from {startPosition}");
        while (time < duration)
        {
            transform.position = Vector2.Lerp(startPosition, targetPosition, time / duration);
            time += Time.deltaTime;
            yield return null;
        }
        transform.position = targetPosition;
		//LerpInProgress = false;
		////print($"{targetPosition} is the new target position that should be the char pos in foggywar");
		if(GetComponent<PlayerCharacter>()){
			//NewFogUpdate(targetPosition);
		} else {
			Mob mob = GetComponent<Mob>();
			if(mob){
				mob.ClientMobFog();
			}
		}
    }
    //used for a slight bumping animation towards an adjacent target
	[Server]
    public IEnumerator BumpTowards(Vector2 end)
    {
		//RpcPlaySwingSound();
		float radius = agent.radius;
		//agent.radius = 0;
		//agent.enabled = false;
		Vector2 start = transform.position;
		Vector2 target = new Vector2((end.x - start.x)*.1f, (end.y - start.y)*.1f);
		//transform.position = start + target;
		yield return new WaitForSeconds(moveTime);
		//agent.enabled = true;
		//transform.position = start;
		//agent.radius = radius;
    }
	[ClientRpc]
	void RpcPlaySwingSound(){
		AudioMgr sound = GetComponent<AudioMgr>();
		sound.PlaySound("blunt hit");
	}
	public void SelectedUnit(){
		Mob mob = GetComponent<Mob>();
		if(mob){
			SelectedCircle.SetActive(true);
		} else {
	        SelectedUnitCircle.SetActive(true);
        	SelectedCircle.SetActive(false);
		}
	}
	public void UnselectedUnit(){
        SelectedCircle.SetActive(false);
	    SelectedUnitCircle.SetActive(false);
		//Mob mob = GetComponent<Mob>();
		//if(mob){
		//	SelectedCircle.SetActive(false);
		//} else {
	    //    SelectedUnitCircle.SetActive(false);
        //	SelectedCircle.SetActive(false);
		//}
		//if(SelectedUnitCircle != null){
	    //    SelectedUnitCircle.SetActive(false);
		//}
	}
	[ClientRpc]
	public void RpcCharmFaded(){
		if(SelectedUnitCircle.activeInHierarchy){
	    	SelectedUnitCircle.SetActive(false);
        	SelectedCircle.SetActive(true);
		}
		RemoveFogParticipant.Invoke(this, true);
	}
	public void SelectedMO(){
		Mob mob = GetComponent<Mob>();
		if(mob){
			if(mob.charmedController != null){
				if(SelectedUnitCircle != null){
	    		    SelectedUnitCircle.SetActive(true);
				} else {
        			SelectedCircle.SetActive(true);
				}
			} else {
        		SelectedCircle.SetActive(true);
			}
		} else {
        	SelectedCircle.SetActive(true);
		}
    }
    public void UnselectedMO(){
			SelectedCircle.SetActive(false);
	        SelectedUnitCircle.SetActive(false);

		//Mob mob = GetComponent<Mob>();
		//if(mob){
		//	SelectedCircle.SetActive(false);
		//	if(SelectedUnitCircle != null){
	    //	    SelectedUnitCircle.SetActive(false);
		//	}
		//} else {
	    //    SelectedUnitCircle.SetActive(false);
        //	SelectedCircle.SetActive(false);
		//}
        //SelectedCircle.SetActive(false);
    }
	public void TargettedMO(){
		Mob mob = GetComponent<Mob>();
		if(mob){
			SelectedCircle.SetActive(true);
		} else {
	        TargetCircle.SetActive(true);
		}
		TargetWindowSet.Invoke(this);
    }
    public void UnTargettedMO(){
        Mob mob = GetComponent<Mob>();
		if(mob){
			SelectedCircle.SetActive(false);
		} else {
	        TargetCircle.SetActive(false);
		}
    }


	[Command]
	public void CmdSetTarget(MovingObject target){
		Target = target;
		//print($"Setting target {Target.gameObject.name} for {this.gameObject.name}");
		TargetSendNewTarget(target);
	}
	[TargetRpc]
	void TargetSendNewTarget(MovingObject target){
		TargetWasSet.Invoke(target);
	}
   private Coroutine Lerping;
public bool isMoving = false;
public float travelTime = .75f;
	public void EvacuateSpellCastTact(ScenePlayer castingPlayer){
		CmdEvacuateSpellCastTact(castingPlayer);
	}
	[Command]
	public void CmdEvacuateSpellCastTact(ScenePlayer castingPlayer){
		ServerEvacuateSpellCastTact(castingPlayer);
	}
	[Server]
	public void ServerEvacuateSpellCastTact(ScenePlayer castingPlayer){
        curatorTM.CastingEvacuate(castingPlayer.currentMatch, castingPlayer);
	}

	
[Server]
    public void ProcessAOESpell(string spell, int spellRank, int cost, Vector2 mousePosition, bool offensive){
        //CastingAOEHostileSpell
		PlayerCharacter pc = GetComponent<PlayerCharacter>();
		Mob mob = GetComponent<Mob>();
		Match match = null;
		if(pc){
			match = pc.assignedMatch;
		}
		if(mob){
			match = mob.assignedMatch;
		}
        //RpcCastingSpell(spell);
        if(offensive){
            curatorTM.CastingAOEHostileSpell(match, this, mousePosition, spell, spellRank, cost);
        } else {
            curatorTM.CastingAOEFriendlySpell(match, this, mousePosition, spell, spellRank, cost);
        }
    }
[Server]
	public void ProcessSpellCast(string mode, MovingObject castingCharacter, MovingObject target, int cost){
		string _spellname = string.Empty;
		if(mode == CastingQ){
            _spellname = SpellQ;
        }
        if(mode == CastingE){
            _spellname = SpellE;
        }
        if(mode == CastingR){
            _spellname = SpellR;
        }
        if(mode == CastingF){
            _spellname = SpellF;
        }
		PlayerCharacter pc = GetComponent<PlayerCharacter>();
		Mob mob = GetComponent<Mob>();
		Match match = null;
		if(pc){
			match = pc.assignedMatch;
		}
		if(mob){
			match = mob.assignedMatch;
		}
		var nameMatch = System.Text.RegularExpressions.Regex.Match(_spellname, @"^\D*");
        string spell = nameMatch.Value.Trim(); // Trim any trailing spaces
        int _spellRank = 1;
        // Extract spell rank
        var rankMatch = System.Text.RegularExpressions.Regex.Match(_spellname, @"\d+$");
        if (rankMatch.Success) {
            _spellRank = int.Parse(rankMatch.Value); // Parse the rank number
        }
        //RpcCastingSpell(spell);d
		if(spell == "Harden"){//good to go
			curatorTM.UpdatePlayerSelfCasted(match, this, spell, _spellRank, cost);
		    return;
    	}
		if(spell == "Offensive Stance"){//good to go
			curatorTM.UpdatePlayerSelfCasted(match, this, spell, _spellRank, cost);
		    return;
    	}
		if(spell == "Aimed Shot"){
		    curatorTM.UpdatePlayerCastedOffensiveSpellSingleTargetDPS(match, this, target, spell, _spellRank, cost);
		    return;
		}
		if(spell == "Bandage Wound"){
			curatorTM.UpdatePlayerCastedHealSpellSingleTarget(match, this, target, spell, _spellRank, cost);
		    return;
		}
		if(spell == "Regeneration"){
			curatorTM.UpdatePlayerCastedHealSpellSingleTarget(match, this, target, spell, _spellRank, cost);
		    return;
		}
		//Wizard
		if(spell == "Ice"){
			curatorTM.UpdatePlayerCastedOffensiveSpellSingleTargetDPS(match, this, target, spell, _spellRank, cost);
		    return;
		}
		if(spell == "Fire"){
			curatorTM.UpdatePlayerCastedOffensiveSpellSingleTargetDPS(match, this, target, spell, _spellRank, cost);
		    return;
		}
		//Rogue
		if(spell == "Shuriken"){
			curatorTM.UpdatePlayerCastedOffensiveSpellSingleTargetDPS(match, this, target, spell, _spellRank, cost);
		    return;
		}
		if(spell == "Hide"){
			curatorTM.UpdatePlayerSelfCasted(match, this, spell, _spellRank, cost);
		    return;
		}
		//Priest
		if(spell == "Holy Bolt"){
			curatorTM.UpdatePlayerCastedOffensiveSpellSingleTargetDPS(match, this, target, spell, _spellRank, cost);
		    return;
		}
		if(spell == "Heal"){
			curatorTM.UpdatePlayerCastedHealSpellSingleTarget(match, this, target, spell, _spellRank, cost);
		    return;
		}
		//Druid
		if(spell == "Swarm Of Insects"){
			curatorTM.UpdatePlayerCastedOffensiveSpellSingleTargetDPS(match, this, target, spell, _spellRank, cost);
		    return;
		}
		if(spell == "Rejuvenation"){
			curatorTM.UpdatePlayerCastedHealSpellSingleTarget(match, this, target, spell, _spellRank, cost);
		    return;
		}
		//Paladin
		if(spell == "Holy Swing"){
			curatorTM.UpdatePlayerCastedOffensiveSpellSingleTargetDPS(match, this, target, spell, _spellRank, cost);
		    return;
		}
		//if(spell == "Divine Armor"){
		//	curatorTM.UpdatePlayerSelfCasted(match, this, spell, _spellRank, cost);
		//    return;
		//}
		//Fighter
		if(spell == "Charge"){
			curatorTM.UpdatePlayerCastedOffensiveSpellSingleTargetDPS(match, this, target, spell, _spellRank, cost);
		    return;
		}
		if(spell == "Bash"){
			curatorTM.UpdatePlayerCastedOffensiveSpellSingleTargetDPS(match, this, target, spell, _spellRank, cost);
		    return;
		}
		//Enchanter
		if(spell == "Mesmerize"){
			curatorTM.UpdateCrowdControlSpell(match, this, target, spell, _spellRank, cost);
		    return;
		}
		if(spell == "Haste"){
			curatorTM.UpdateBuffSpell(match, this, target, spell, _spellRank, cost);
		    return;
		}
		
		if(spell == "Head Shot"){
		    curatorTM.UpdatePlayerCastedOffensiveSpellSingleTargetDPS(match, this, target, spell, _spellRank, cost);
		    return;
		}
		if(spell == "Silence Shot"){
		    curatorTM.UpdatePlayerCastedOffensiveSpellSingleTargetDPS(match, this, target, spell, _spellRank, cost);
		    return;
		}
		if(spell == "Crippling Shot"){
		    curatorTM.UpdatePlayerCastedOffensiveSpellSingleTargetDPS(match, this, target, spell, _spellRank, cost);
		    return;
		}
		
		if(spell == "Identify Enemy"){
		    curatorTM.UpdateCrowdControlSpell(match, this, target, spell, _spellRank, cost);
		    return;
		}
		//if(spell == "Track"){
		//    curatorTM.UpdatePlayerCastedOffensiveSpell(match, this, target, spell, _spellRank, cost);
		//    return;
		//}
		if(spell == "Fire Arrow"){
		    curatorTM.UpdatePlayerCastedOffensiveSpellSingleTargetDPS(match, this, target, spell, _spellRank, cost);
		    return;
		}
		if(spell == "Penetrating Shot"){
		    curatorTM.UpdatePlayerCastedOffensiveSpellSingleTargetDPS(match, this, target, spell, _spellRank, cost);
		    return;
		}
		if(spell == "Sleep"){
		    curatorTM.UpdateCrowdControlSpell(match, this, target, spell, _spellRank, cost);
		    return;
		}
		//if(spell == "Perception"){
		//    curatorTM.UpdateBuffSpell(match, this, target, spell, _spellRank, cost);
		//    return;
		//}
		if(spell == "Double Shot"){
		    curatorTM.UpdatePlayerCastedOffensiveSpellSingleTargetDPS(match, this, target, spell, _spellRank, cost);
		    return;
		}
		
		//if(spell == "Nature's Precision"){
		//    curatorTM.UpdateBuffSpell(match, this, target, spell, _spellRank, cost);
		//    return;
		//}
		
		
		// Enchanter Spells
		if(spell == "Root"){
		    curatorTM.UpdateCrowdControlSpell(match, this, target, spell, _spellRank, cost);
		    return;
		}
		if(spell == "Invisibility"){
		    curatorTM.UpdateBuffSpell(match, this, target, spell, _spellRank, cost);
		    return;
		}
		if(spell == "Rune"){
		    curatorTM.UpdateBuffSpell(match, this, target, spell, _spellRank, cost);
		    return;
		}
		if(spell == "Slow"){
		    curatorTM.UpdateCrowdControlSpell(match, this, target, spell, _spellRank, cost);
		    return;
		}
		if(spell == "Magic Sieve"){
		    curatorTM.UpdateCrowdControlSpell(match, this, target, spell, _spellRank, cost);
		    return;
		}
		if(spell == "Aneurysm"){
		    curatorTM.UpdatePlayerCastedOffensiveSpellSingleTargetDPS(match, this, target, spell, _spellRank, cost);
		    return;
		}
		if(spell == "Weaken"){
		    curatorTM.UpdateCrowdControlSpell(match, this, target, spell, _spellRank, cost);
		    return;
		}
		if(spell == "Resist Magic"){
		    curatorTM.UpdateBuffSpell(match, this, target, spell, _spellRank, cost);
		    return;
		}
		if(spell == "Purge"){
		    curatorTM.UpdateBuffSpell(match, this, target, spell, _spellRank, cost);
		    return;
		}
		if(spell == "Charm"){
		    curatorTM.UpdateCrowdControlSpell(match, this, target, spell, _spellRank, cost);
		    return;
		}
		if(spell == "Mp Transfer"){
		    curatorTM.UpdateBuffSpell(match, this, target, spell, _spellRank, cost);
		    return;
		}
		
		// Fighter Spells
		
		if(spell == "Protect"){
		    curatorTM.UpdateBuffSpell(match, this, target, spell, _spellRank, cost);
		    return;
		}
		if(spell == "Knockback"){
		    curatorTM.UpdatePlayerCastedOffensiveSpellSingleTargetDPS(match, this, target, spell, _spellRank, cost);
		    return;
		}
		if(spell == "Throw Stone"){
		    curatorTM.UpdatePlayerCastedOffensiveSpellSingleTargetDPS(match, this, target, spell, _spellRank, cost);
		    return;
		}
		if(spell == "Heavy Swing"){
		    curatorTM.UpdatePlayerCastedOffensiveSpellSingleTargetDPS(match, this, target, spell, _spellRank, cost);
		    return;
		}
		if(spell == "Taunt"){
		    curatorTM.UpdatePlayerCastedOffensiveSpellSingleTargetDPS(match, this, target, spell, _spellRank, cost);
		    return;
		}
		
		// Priest Spells
		if(spell == "Cure Poison"){
		    curatorTM.UpdatePlayerCastedHealSpellSingleTarget(match, this, target, spell, _spellRank, cost);
		    return;
		}
		if(spell == "Dispel"){
		    curatorTM.UpdateBuffSpell(match, this, target, spell, _spellRank, cost);
		}
		if(spell == "Fortitude"){
		    curatorTM.UpdateBuffSpell(match, this, target, spell, _spellRank, cost);
		    return;
		}
		if(spell == "Smite"){
		    curatorTM.UpdatePlayerCastedOffensiveSpellSingleTargetDPS(match, this, target, spell, _spellRank, cost);
		    return;
		}
		if(spell == "Shield Bash"){
		    curatorTM.UpdatePlayerCastedOffensiveSpellSingleTargetDPS(match, this, target, spell, _spellRank, cost);
		    return;
		}
		if(spell == "Greater Heal"){
		    curatorTM.UpdatePlayerCastedHealSpellSingleTarget(match, this, target, spell, _spellRank, cost);
		    return;
		}
		if(spell == "Resurrect"){
		    curatorTM.UpdatePlayerCastedHealSpellSingleTarget(match, this, target, spell, _spellRank, cost);
		    return;
		}
		
		// Rogue Spells
		//if(spell == "Picklock"){
		//    curatorTM.UpdatePlayerCastedOffensiveSpell(match, this, target, spell, _spellRank, cost);
		//    return;
		//}
		if(spell == "Steal"){
		    curatorTM.UpdateCrowdControlSpell(match, this, target, spell, _spellRank, cost);
		    return;
		}
		if(spell == "Tendon Slice"){
		    curatorTM.UpdatePlayerCastedOffensiveSpellSingleTargetDPS(match, this, target, spell, _spellRank, cost);
		    return;
		}
		if(spell == "Backstab"){
		    curatorTM.UpdatePlayerCastedOffensiveSpellSingleTargetDPS(match, this, target, spell, _spellRank, cost);
		    return;
		}
		if(spell == "Blind"){
		    curatorTM.UpdateCrowdControlSpell(match, this, target, spell, _spellRank, cost);
		    return;
		}
		if(spell == "Poison"){
		    curatorTM.UpdatePlayerCastedOffensiveSpellSingleTargetDPS(match, this, target, spell, _spellRank, cost);
		    return;
		}
		// Wizard Spells
		if(spell == "Light"){
		    curatorTM.UpdateBuffSpell(match, this, target, spell, _spellRank, cost);
		    return;
		}
		if(spell == "Magic Missile"){
		    curatorTM.UpdatePlayerCastedOffensiveSpellSingleTargetDPS(match, this, target, spell, _spellRank, cost);
		    return;
		}
		if(spell == "Incinerate"){
		    curatorTM.UpdatePlayerCastedOffensiveSpellSingleTargetDPS(match, this, target, spell, _spellRank, cost);
		    return;
		}
		if(spell == "Brain Freeze"){
		    curatorTM.UpdatePlayerCastedOffensiveSpellSingleTargetDPS(match, this, target, spell, _spellRank, cost);
		    return;
		}
		//wyvern
		if (spell == "Bite"){
		    curatorTM.UpdatePlayerCastedOffensiveSpellSingleTargetDPS(match, this, target, spell, _spellRank, cost);
		    return;
		}
		if (spell == "Armor Bite"){
		    curatorTM.UpdatePlayerCastedOffensiveSpellSingleTargetDPS(match, this, target, spell, _spellRank, cost);
		    return;
		}
		//enemy spells exclusive
		if (spell == "Drain"){
		    curatorTM.UpdatePlayerCastedOffensiveSpellSingleTargetDPS(match, this, target, spell, _spellRank, cost);
		    return;
		}
		if (spell == "Spit"){
		    curatorTM.UpdatePlayerCastedOffensiveSpellSingleTargetDPS(match, this, target, spell, _spellRank, cost);
		    return;
		}
		if (spell == "Lesser Poison"){
		    curatorTM.UpdatePlayerCastedOffensiveSpellSingleTargetDPS(match, this, target, spell, _spellRank, cost);
		    return;
		}
		if (spell == "Greater Poison"){
		    curatorTM.UpdatePlayerCastedOffensiveSpellSingleTargetDPS(match, this, target, spell, _spellRank, cost);
		    return;
		}

		


		
		// Druid Spells
		
		if(spell == "Thorns"){
		    curatorTM.UpdateBuffSpell(match, this, target, spell, _spellRank, cost);
		    return;
		}
		if(spell == "Nature's Protection"){
		    curatorTM.UpdateBuffSpell(match, this, target, spell, _spellRank, cost);
		    return;
		}
		if(spell == "Strength"){
		    curatorTM.UpdateBuffSpell(match, this, target, spell, _spellRank, cost);
		    return;
		}
		if(spell == "Snare"){
		    curatorTM.UpdateCrowdControlSpell(match, this, target, spell, _spellRank, cost);
		    return;
		}
		if(spell == "Engulfing Roots"){
		    curatorTM.UpdateCrowdControlSpell(match, this, target, spell, _spellRank, cost);
		    return;
		}
		if(spell == "Chain Lightning"){
		    curatorTM.UpdatePlayerCastedOffensiveSpellSingleTargetDPS(match, this, target, spell, _spellRank, cost);
		    return;
		}
		if(spell == "Greater Rejuvenation"){
		    curatorTM.UpdatePlayerCastedHealSpellSingleTarget(match, this, target, spell, _spellRank, cost);
		    return;
		}
		// Paladin Spells
		
		if(spell == "Flash Of Light"){
		    curatorTM.UpdateCrowdControlSpell(match, this, target, spell, _spellRank, cost);
		    return;
		}
		if(spell == "Cleanse"){
		    curatorTM.UpdatePlayerCastedHealSpellSingleTarget(match, this, target, spell, _spellRank, cost);
		    return;
		}
		if(spell == "Divine Wrath"){
		    curatorTM.UpdatePlayerCastedOffensiveSpellSingleTargetDPS(match, this, target, spell, _spellRank, cost);
		    return;
		}
		if(spell == "Cover"){
		    curatorTM.UpdateBuffSpell(match, this, target, spell, _spellRank, cost);
		    return;
		}
		if(spell == "Shackle"){
		    curatorTM.UpdateCrowdControlSpell(match, this, target, spell, _spellRank, cost);
		    return;
		}
		if(spell == "Lay On Hands"){
		    curatorTM.UpdatePlayerCastedHealSpellSingleTarget(match, this, target, spell, _spellRank, cost);
		    return;
		}
	}
	
    
}}