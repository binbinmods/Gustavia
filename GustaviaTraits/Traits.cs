using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using Obeliskial_Content;
using UnityEngine;
using static Gustavia.CustomFunctions;
using static Gustavia.Plugin;
using static Gustavia.DescriptionFunctions;
using static Gustavia.CharacterFunctions;
using System.Text;
using TMPro;
using Obeliskial_Essentials;
using System.Data.Common;

namespace Gustavia
{
    [HarmonyPatch]
    internal class Traits
    {
        // list of your trait IDs

        public static string[] simpleTraitList = ["trait0", "trait1a", "trait1b", "trait2a", "trait2b", "trait3a", "trait3b", "trait4a", "trait4b"];

        public static string[] myTraitList = simpleTraitList.Select(trait => subclassname.ToLower() + trait).ToArray(); // Needs testing

        public static string trait0 = myTraitList[0];
        // static string trait1b = myTraitList[1];
        public static string trait2a = myTraitList[3];
        public static string trait2b = myTraitList[4];
        public static string trait4a = myTraitList[7];
        public static string trait4b = myTraitList[8];

        // public static int infiniteProctection = 0;
        // public static int bleedInfiniteProtection = 0;
        public static bool isDamagePreviewActive = false;

        public static bool isCalculateDamageActive = false;
        public static int infiniteProctection = 0;

        public static string debugBase = "Binbin - Testing " + heroName + " ";


        public static void DoCustomTrait(string _trait, ref Trait __instance)
        {
            // get info you may need
            Enums.EventActivation _theEvent = Traverse.Create(__instance).Field("theEvent").GetValue<Enums.EventActivation>();
            Character _character = Traverse.Create(__instance).Field("character").GetValue<Character>();
            Character _target = Traverse.Create(__instance).Field("target").GetValue<Character>();
            int _auxInt = Traverse.Create(__instance).Field("auxInt").GetValue<int>();
            string _auxString = Traverse.Create(__instance).Field("auxString").GetValue<string>();
            CardData _castedCard = Traverse.Create(__instance).Field("castedCard").GetValue<CardData>();
            Traverse.Create(__instance).Field("character").SetValue(_character);
            Traverse.Create(__instance).Field("target").SetValue(_target);
            Traverse.Create(__instance).Field("theEvent").SetValue(_theEvent);
            Traverse.Create(__instance).Field("auxInt").SetValue(_auxInt);
            Traverse.Create(__instance).Field("auxString").SetValue(_auxString);
            Traverse.Create(__instance).Field("castedCard").SetValue(_castedCard);
            TraitData traitData = Globals.Instance.GetTraitData(_trait);
            List<CardData> cardDataList = [];
            List<string> heroHand = MatchManager.Instance.GetHeroHand(_character.HeroIndex);
            Hero[] teamHero = MatchManager.Instance.GetTeamHero();
            NPC[] teamNpc = MatchManager.Instance.GetTeamNPC();



            if (_trait == trait0)
            {
                // Stanza increases Healing and Mind damage by 3/Stanza
                // Done in GACM                
            }


            else if (_trait == trait2a)
            {
                if (!IsLivingHero(_character))
                {
                    return;
                }
                // trait2a
                // When you play an Attack or Small Weapon, reduce the cost of your highest cost Spell by 1. When you play a Spell, reduce the cost of your highest cost Attack or Small Weapon by 1. (3 times/turn)
                string traitName = traitData.TraitName;
                string traitId = _trait;

                int bonusActivations = _character.HaveTrait(trait4b) ? 1 : 0;
                DualityCardType(ref _character, ref _castedCard, [Enums.CardType.Small_Weapon, Enums.CardType.Attack], [Enums.CardType.Spell], traitId, bonusActivations);

            }




            else if (_trait == trait2b)
            {

                // trait2b:
                // Salient Stanza increases All Damage. 
                // When you play Attack or Small without Stanza, gain Stanza. This increases from Stanza I to II to III throughout the turn. When you play a Song, lose Stanza.
                if (!IsLivingHero(_character) || _castedCard == null)
                {
                    LogDebug("Nonliving character or null card");
                    return;
                }
                // string traitName = traitData.TraitName;
                string traitId = trait2b;

                LogDebug($"Handling Trait {trait2b}");

                bool hasStanza = _character.HasEffect("stanzai") || _character.HasEffect("stanzaii") || _character.HasEffect("stanzaiii");
                LogDebug($"Has Stanza {hasStanza} - card - {_castedCard.CardName}");
                if (hasStanza && _castedCard.HasCardType(Enums.CardType.Song))
                {
                    _castedCard.EffectRequired = "";
                    // Traverse.Create(__instance).Field("castedCard").SetValue(_castedCard);
                    // _character.SetAuraTrait(_character, "stanzaiii", 1);
                    // _character.SetAuraTrait(_character, "spellsword", 4);

                    for (int index = _character.AuraList.Count - 1; index >= 0; --index)
                    {
                        if (_character.AuraList[index] != null && (UnityEngine.Object)_character.AuraList[index].ACData != (UnityEngine.Object)null && (_character.AuraList[index].ACData.Id == "stanzai" || _character.AuraList[index].ACData.Id == "stanzaii" || _character.AuraList[index].ACData.Id == "stanzaiii"))
                        {
                            StringBuilder stringBuilder = new StringBuilder();
                            stringBuilder.Append("<s>");
                            stringBuilder.Append(Functions.UppercaseFirst(_character.AuraList[index].ACData.ACName));
                            stringBuilder.Append("</s>");
                            string text = stringBuilder.ToString();
                            Enums.CombatScrollEffectType type = !_character.AuraList[index].ACData.IsAura ? Enums.CombatScrollEffectType.Curse : Enums.CombatScrollEffectType.Aura;
                            if ((UnityEngine.Object)_character.HeroItem != (UnityEngine.Object)null)
                                _character.HeroItem.ScrollCombatText(text, type);
                            _character.AuraList.RemoveAt(index);
                        }
                    }

                    // LogDebug($"Testin1234 - Healed Stanza ");
                }
                else if (!hasStanza && (_castedCard.HasCardType(Enums.CardType.Small_Weapon) || _castedCard.HasCardType(Enums.CardType.Attack)))
                {
                    IncrementTraitActivations(traitId);
                    int activations = MatchManager.Instance.activatedTraits[traitId];
                    string stanza;
                    if (activations == 1)
                    {
                        stanza = "stanzai";
                    }
                    else if (activations == 2)
                    {
                        stanza = "stanzaii";
                    }
                    else
                    {
                        stanza = "stanzaiii";
                    }
                    _character.SetAuraTrait(_character, stanza, 1);
                    if (_character.HaveTrait(trait4b))
                    {
                        GainEnergy(_character, 1);
                    }
                }
            }

            else if (_trait == trait4a)
            {
                // trait 4a;
                // Salient Stanza applies to All Heroes. Done in GACM
                // When you apply Regen, apply 2 Bless and Sharp
                if (_character == null || _target == null || !_target.Alive || !_character.Alive)
                {
                    return;
                }
                string traitName = traitData.TraitName;
                string traitId = _trait;
                if (_auxString.ToLower() == "regeneration")
                {
                    LogDebug($"Handling Trait {traitId}: {traitName}");
                    // Not sure if _target or _character who is the source
                    _target.SetAuraTrait(_character, "bless", 2);
                    _target.SetAuraTrait(_character, "sharp", 2);
                }
            }

            else if (_trait == trait4b)
            {
                // trait 4b:
                // Secret Solo can be activated an additional time. - DONE
                // When you gain Stanza from Skittish Psalm, gain 1 Energy.
                string traitName = traitData.TraitName;
                string traitId = _trait;
                LogDebug($"Handling Trait {traitId}: {traitName}");
            }

        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(Trait), "DoTrait")]
        public static bool DoTrait(Enums.EventActivation _theEvent, string _trait, Character _character, Character _target, int _auxInt, string _auxString, CardData _castedCard, ref Trait __instance)
        {
            if ((UnityEngine.Object)MatchManager.Instance == (UnityEngine.Object)null)
                return false;
            Traverse.Create(__instance).Field("character").SetValue(_character);
            Traverse.Create(__instance).Field("target").SetValue(_target);
            Traverse.Create(__instance).Field("theEvent").SetValue(_theEvent);
            Traverse.Create(__instance).Field("auxInt").SetValue(_auxInt);
            Traverse.Create(__instance).Field("auxString").SetValue(_auxString);
            Traverse.Create(__instance).Field("castedCard").SetValue(_castedCard);
            if (Content.medsCustomTraitsSource.Contains(_trait) && myTraitList.Contains(_trait))
            {
                DoCustomTrait(_trait, ref __instance);
                return false;
            }
            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(MatchManager), "CastCard")]
        // // [HarmonyPriority(Priority.Last)]
        public static void CastCardPrefix(ref MatchManager __instance,
            ref CardItem theCardItem,
            ref CardData _card,
            ref CardData __state,
            bool _automatic = false,
            int _energy = -1,
            int _posInTable = -1,
            bool _propagate = true)
        {
            bool useCardItem = false;

            if (theCardItem != null)
            {
                useCardItem = true;
            }
            CardData _cardActive = !((UnityEngine.Object)theCardItem != (UnityEngine.Object)null) ? _card : theCardItem.CardData;
            // _castedCard = theCardItem.GetCardData();
            if (_cardActive == null)
            {
                LogDebug("CastCardPrefix - null card");
                return;
            }
            // if (_cardActive.TargetType == Enums.CardTargetType.Global && _cardActive.AddCardPlace == Enums.CardPlace.Hand || ))
            if (_cardActive.Id.StartsWith("gustaviaceaseless"))
            {
                LogDebug($"CastCardPrefix - Altering {_cardActive.Id}");
                __state = _cardActive;
                _cardActive.AddCard = 0;
            }
            else
            {
                __state = null;
            }
            if (useCardItem)
            {
                theCardItem.CardData = _cardActive;
            }
            else
            {
                _card = _cardActive;
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(MatchManager), "CastCard")]
        // // [HarmonyPriority(Priority.Last)]
        public static void CastCardPostfix(ref MatchManager __instance,
            ref List<string>[] ___HeroDeck,
            ref Dictionary<string, LogEntry> ___logDictionary,
            int ___cardsWaitingForReset,
            CardData __state,
            ref CardItem theCardItem,
            ref CardData _card,
            bool _automatic = false,
            int _energy = -1,
            int _posInTable = -1,
            bool _propagate = true)
        {
            bool useCardItem = false;

            if (theCardItem != null)
            {
                useCardItem = true;
            }
            CardData _cardActive = !((UnityEngine.Object)theCardItem != (UnityEngine.Object)null) ? _card : theCardItem.CardData;
            // _castedCard = theCardItem.GetCardData();
            if (_cardActive == null)
            {
                LogDebug("CastCardPostfix - null card");
                return;
            }
            if (__state != null)
            {
                LogDebug($"CastCardPostfix - adding card to hand - Addcard {__state.AddCard}, addcardid = {_cardActive.AddCardId}");
                List<string> stringList = [];
                stringList.Add(_cardActive.AddCardId);
                for (int index = stringList.Count - 1; index >= 0; --index)
                    ___HeroDeck[__instance.GetHeroActive()].Insert(0, stringList[index]);
                for (int index = 0; index < stringList.Count; ++index)
                {
                    if (index < 10 - __instance.CountHeroHand())
                        __instance.CreateLogEntry(true, "toHand:" + ___logDictionary.Count.ToString(), stringList[index], __instance.GetTeamHero()[__instance.GetHeroActive()], (NPC)null, (Hero)null, (NPC)null, __instance.GetCurrentRound());
                }
                // __instance.NewCard(__state.AddCard, _cardActive.AddCardFrom);
                __instance.GenerateNewCard(1, _cardActive.AddCardId, true, Enums.CardPlace.Hand, _cardActive, heroIndex: __instance.GetHeroActive());
                // while (___cardsWaitingForReset > 0)
                //     Globals.Instance.WaitForSeconds(0.1f);
                if (useCardItem)
                {
                    theCardItem.CardData = __state;
                }
                else
                {
                    _card = __state;
                }
            }



        }



        [HarmonyPostfix]
        [HarmonyPatch(typeof(AtOManager), "GlobalAuraCurseModificationByTraitsAndItems")]
        // [HarmonyPriority(Priority.Last)]
        public static void GlobalAuraCurseModificationByTraitsAndItemsPostfix(ref AtOManager __instance, ref AuraCurseData __result, string _type, string _acId, Character _characterCaster, Character _characterTarget)
        {
            // LogInfo($"GACM {subclassName}");

            Character characterOfInterest = _type == "set" ? _characterTarget : _characterCaster;
            string traitOfInterest;
            AppliesTo appliesTo;
            switch (_acId)
            {
                // trait0:
                // Stanza increases Healing and Mind damage by 3/Stanza

                // trait2b:
                // Stanza increases Healing and ALL damage by 3/Stanza

                // trait 4a:
                // Stanza increases Healing and Mind damage by 3/Stanza for all heroes

                case "stanzai":
                    traitOfInterest = trait0;
                    // __result.Removable = true;
                    // __result.GainAuraCurseConsumption = null;
                    appliesTo = characterOfInterest.HaveTrait(trait4a) ? AppliesTo.Heroes : AppliesTo.ThisHero;
                    if (IfCharacterHas(characterOfInterest, CharacterHas.Trait, traitOfInterest, appliesTo))
                    {
                        Enums.DamageType damageTypeIncreased = IfCharacterHas(characterOfInterest, CharacterHas.Trait, trait2b, appliesTo) ? Enums.DamageType.All : Enums.DamageType.Mind;
                        __result.AuraDamageType = damageTypeIncreased;
                        __result.AuraDamageIncreasedTotal = 3;
                        __result.HealDoneTotal = 3;
                    }
                    break;
                case "stanzaii":
                    traitOfInterest = trait0;
                    appliesTo = characterOfInterest.HaveTrait(trait4b) ? AppliesTo.Heroes : AppliesTo.ThisHero;
                    if (IfCharacterHas(characterOfInterest, CharacterHas.Trait, traitOfInterest, appliesTo))
                    {
                        Enums.DamageType damageTypeIncreased = IfCharacterHas(characterOfInterest, CharacterHas.Trait, trait2b, appliesTo) ? Enums.DamageType.All : Enums.DamageType.Mind;
                        __result.AuraDamageType = damageTypeIncreased;
                        __result.AuraDamageIncreasedTotal = 6;
                        __result.HealDoneTotal = 6;
                    }
                    break;
                case "stanzaiii":
                    traitOfInterest = trait0;
                    appliesTo = characterOfInterest.HaveTrait(trait4b) ? AppliesTo.Heroes : AppliesTo.ThisHero;
                    if (IfCharacterHas(characterOfInterest, CharacterHas.Trait, traitOfInterest, appliesTo))
                    {
                        Enums.DamageType damageTypeIncreased = IfCharacterHas(characterOfInterest, CharacterHas.Trait, trait2b, appliesTo) ? Enums.DamageType.All : Enums.DamageType.Mind;
                        __result.AuraDamageType = damageTypeIncreased;
                        __result.AuraDamageIncreasedTotal = 9;
                        __result.HealDoneTotal = 9;
                    }
                    break;
            }
        }



    }
}

