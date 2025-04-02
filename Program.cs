using TeamHeroCoderLibrary;

namespace PlayerCoder
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Connecting...");
            GameClientConnectionManager connectionManager;
            connectionManager = new GameClientConnectionManager();
            connectionManager.SetExchangePath(MyAI.FolderExchangePath);
            connectionManager.onHeroHasInitiative = MyAI.ProcessAI;
            connectionManager.StartListeningToGameClientForHeroPlayRequests();
        }
    }

    public static class MyAI
    {

        static Dictionary<string, double> SpellCosts =  new();
        static Dictionary<string, double> ItemEssenceCosts = new();

        static bool DictsInitialized = false;

        public static string FolderExchangePath = "C:/Users/Ryan/AppData/LocalLow/Wind Jester Games/Team Hero Coder";

        static Hero activeHero;

        static public void ProcessAI()
        {

            Console.WriteLine("Processing AI!");
            activeHero = TeamHeroCoder.BattleState.heroWithInitiative;


            // init dictionaries
            if (!DictsInitialized)
            {
                Console.WriteLine("Initializing Dictionaries");
                InitializeDictionaries();
                DictsInitialized = true;
            }


            

            // set default target
            Hero target = null;
            foreach (Hero hero in TeamHeroCoder.BattleState.foeHeroes)
            {
                if (hero.health > 0)
                {

                    target = hero;
                    break;

                    // not in use currently
                    //if (target == null)
                    //    target = hero;
                    //else if (hero.health < target.health)
                    //    target = hero;
                }
            }


            //--- class code ---//

            if (TeamHeroCoder.BattleState.heroWithInitiative.jobClass == HeroJobClass.Cleric)
            {
                //The character with initiative is a figher, do something here...

                Console.WriteLine("------this is a cleric------");


                // haste on self
                if (!hasStatus(activeHero, StatusEffect.Haste))
                {
                    if(AttemptCastSpell(Ability.Haste,activeHero))return;
                }
                else
                {
                    Console.WriteLine("Cleric has haste");
                }

                


                //resurrection
                Hero resTarget = FindHeroWithHealthPercentBellow(1, TeamHeroCoder.BattleState.allyHeroes);
                if (resTarget != null)
                {
                    if (AttemptCastSpell(Ability.Resurrection, resTarget)) return;
                }
                else
                {
                    Console.WriteLine("no dead heros");
                }

                

                //cure seriouse
                Hero cureTarget = FindHeroWithHealthPercentBellow(50, TeamHeroCoder.BattleState.allyHeroes);
                if (cureTarget != null)
                {
                    if (AttemptCastSpell(Ability.CureSerious, cureTarget)) return;
                }
                else
                {
                    Console.WriteLine("no allies on low health");
                }

                

                //wizard clense and buffs
                Hero Wizard = FindClassOnTeam(TeamHeroCoder.BattleState.allyHeroes, HeroJobClass.Wizard);
                //note: this will not work will with parties that have more than one wizard.
                if (Wizard != null)
                {
                    //cleanse if silenced
                    if (hasStatus(Wizard, StatusEffect.Silence))
                    {
                        if (AttemptCastSpell(Ability.QuickCleanse, Wizard)) return;
                    }
                    else
                    {
                        Console.WriteLine("Wizard is not silenced");
                    }



                    // cast faith on wizard
                    if (BuffNotBuffed(StatusEffect.Faith, Ability.Faith, Wizard)) return;

                }


                //cleans ally with debufs
                bool foundDebuffedAlly = false;
                foreach (Hero h in TeamHeroCoder.BattleState.allyHeroes)
                {
                    if (NegStatusCount(h) > 0)
                    {
                        foundDebuffedAlly = true;
                        if(AttemptCastSpell(Ability.QuickCleanse, h))return;
                    }
                }
                if (!foundDebuffedAlly) Console.WriteLine("No allies are debuffed");




                //use ether on ally
                bool foundLowAllyMana = false;
                foreach (Hero h in TeamHeroCoder.BattleState.allyHeroes)
                {
                    if(h.mana < (float)h.maxMana * 0.4f)
                    {
                        foundLowAllyMana = true;
                        if (AttemptUseItem(Item.Ether, Ability.Ether, h)) return;
                    }
                }
                if (!foundLowAllyMana) Console.WriteLine("No allies on low mana");




                //quick heal
                Hero qHealTarget = FindHeroWithHealthPercentBellow(80, TeamHeroCoder.BattleState.allyHeroes);
                if (qHealTarget != null)
                {
                    if (AttemptCastSpell(Ability.QuickHeal, qHealTarget)) return;
                }
                else
                {
                    Console.WriteLine("No allies to quick heal");
                }




            }
            else if (TeamHeroCoder.BattleState.heroWithInitiative.jobClass == HeroJobClass.Wizard)
            {
                //The character with initiative is a figher, do something here...

                Console.WriteLine("------this is a wizard------");

                if (activeHero.mana < activeHero.maxMana*0.4f)
                {
                    if(AttemptUseItem(Item.Ether,Ability.Ether,activeHero)) return;
                }
                else
                {
                    Console.WriteLine("Wizard not on low mana");
                }

                if(AttemptCastSpell(Ability.Meteor,target)) return;


            }
            else if (TeamHeroCoder.BattleState.heroWithInitiative.jobClass == HeroJobClass.Alchemist)
            {
                //The character with initiative is a figher, do something here...

                Console.WriteLine("------this is an Alchemist------");


                //dispel auto life on foe
                bool foundFoeWithAutoLife = false;
                foreach (Hero h in TeamHeroCoder.BattleState.foeHeroes)
                {
                    if (hasStatus(h, StatusEffect.AutoLife) && h.health < (float)h.maxHealth*0.6f)
                    {
                        foundFoeWithAutoLife = true;
                        if(AttemptCastSpell(Ability.Dispel,h))return;
                    }
                }
                if (!foundFoeWithAutoLife) Console.WriteLine("No enemy with auto life");



                //check use potion
                Hero allyToHeal = FindHeroWithHealthPercentBellow(30,TeamHeroCoder.BattleState.allyHeroes);
                if (allyToHeal != null)
                {
                    if (AttemptUseItem(Item.Potion, Ability.Potion, allyToHeal)) return;
                }
                else
                {
                    Console.WriteLine("No Ally needs a potion");
                }


                //check use ether
                Hero allyToEther = FindHeroWithManaPercentBellow(50, TeamHeroCoder.BattleState.allyHeroes);
                if (allyToEther != null)
                {
                    if (AttemptUseItem(Item.Ether, Ability.Ether, allyToEther)) return;
                }
                else
                {
                    Console.WriteLine("No Ally needs an Ether");
                }


                //check craft Ether
                if (GetItemCount(Item.Ether,TeamHeroCoder.BattleState.allyInventory) < 1)
                {
                    if (AttemptCraftItem(Item.Ether,Ability.CraftEther)) return;
                }
                else
                {
                    Console.WriteLine("Don't need to craft Ether");
                }


                //cleans ally with debufs
                bool foundAllyWithDebuff = false;
                foreach (Hero h in TeamHeroCoder.BattleState.allyHeroes)
                {
                    if (NegStatusCount(h) > 0)
                    {
                        foundAllyWithDebuff = true;
                        if (AttemptCastSpell(Ability.Cleanse, h)) return;
                    }
                }
                if (!foundAllyWithDebuff) Console.WriteLine("No Ally to clense");




                //check use potion again
                allyToHeal = null;
                allyToHeal = FindHeroWithHealthPercentBellow(60, TeamHeroCoder.BattleState.allyHeroes);
                if (allyToHeal != null)
                {
                    if (AttemptUseItem(Item.Potion, Ability.Potion, allyToHeal)) return;
                }
                else
                {
                    Console.WriteLine("No Ally needs a potion");
                }




                // check every hero has haste
                bool foundAllyWithoutHaste = false;
                foreach (Hero h in TeamHeroCoder.BattleState.allyHeroes)
                {
                    foundAllyWithoutHaste = true;
                    if (BuffNotBuffed(StatusEffect.Haste, Ability.Haste, h)) return;
                }
                if (!foundAllyWithoutHaste) Console.WriteLine("All allies have haste");




                //check craft Potion
                if (GetItemCount(Item.Potion, TeamHeroCoder.BattleState.allyInventory) < 1)
                {
                    if (AttemptCraftItem(Item.Potion, Ability.CraftPotion)) return;
                }
                else
                {
                    Console.WriteLine("Don't need to craft Potion");
                }




                //dispel positive perks on enemy team
                bool foundEnemyToDispel = false;
                foreach (Hero h in TeamHeroCoder.BattleState.foeHeroes)
                {
                    if (h.statusEffectsAndDurations.Count > 0)
                    {
                        if(AttemptCastSpell(Ability.Dispel, h))return;
                    }
                }
                if (!foundEnemyToDispel) Console.WriteLine("no enemies to dispel");




                //if we reach this point without performing an action, craft Ether
                if (AttemptCraftItem(Item.Ether,Ability.CraftEther)) return;
            }



            // default action for all classes
            Console.WriteLine("Performing basic attack");
            TeamHeroCoder.PerformHeroAbility(Ability.Attack, target);

        }

        static void InitializeDictionaries()
        {

            SpellCosts.Add(nameof(Ability.MagicMissile),10);
            SpellCosts.Add(nameof(Ability.Slow), 15);
            SpellCosts.Add(nameof(Ability.Petrify), 15);
            SpellCosts.Add(nameof(Ability.PoisonNova), 15);
            SpellCosts.Add(nameof(Ability.FlameStrike), 30);
            SpellCosts.Add(nameof(Ability.Fireball), 25);
            SpellCosts.Add(nameof(Ability.Meteor), 60);
            SpellCosts.Add(nameof(Ability.Doom), 15);
            SpellCosts.Add(nameof(Ability.QuickDispel), 10);

            SpellCosts.Add(nameof(Ability.CureLight), 10);
            SpellCosts.Add(nameof(Ability.CureSerious), 20);
            SpellCosts.Add(nameof(Ability.MassHeal), 20);
            SpellCosts.Add(nameof(Ability.Resurrection), 25);
            SpellCosts.Add(nameof(Ability.Haste), 15);
            SpellCosts.Add(nameof(Ability.Faith), 15);
            SpellCosts.Add(nameof(Ability.Brave), 15);
            SpellCosts.Add(nameof(Ability.AutoLife), 25);
            SpellCosts.Add(nameof(Ability.QuickCleanse), 10);
            SpellCosts.Add(nameof(Ability.QuickHeal), 15);

            SpellCosts.Add(nameof(Ability.Cleanse), 15);
            SpellCosts.Add(nameof(Ability.Dispel), 15);

            SpellCosts.Add(nameof(Ability.CraftPotion), 10);
            SpellCosts.Add(nameof(Ability.CraftRevive), 10);
            SpellCosts.Add(nameof(Ability.CraftSilenceRemedy), 10);
            SpellCosts.Add(nameof(Ability.CraftPoisonRemedy), 10);
            SpellCosts.Add(nameof(Ability.CraftPetrifyRemedy), 10);
            SpellCosts.Add(nameof(Ability.CraftFullRemedy), 10);
            SpellCosts.Add(nameof(Ability.CraftEther), 10);
            SpellCosts.Add(nameof(Ability.CraftElixir), 20);
            SpellCosts.Add(nameof(Ability.CraftMegaElixir), 25);



            ItemEssenceCosts.Add(nameof(Ability.CraftPotion), 2);
            ItemEssenceCosts.Add(nameof(Ability.CraftRevive), 2);
            ItemEssenceCosts.Add(nameof(Ability.CraftSilenceRemedy), 2);
            ItemEssenceCosts.Add(nameof(Ability.CraftPoisonRemedy), 2);
            ItemEssenceCosts.Add(nameof(Ability.CraftPetrifyRemedy), 2);
            ItemEssenceCosts.Add(nameof(Ability.CraftFullRemedy), 2);
            ItemEssenceCosts.Add(nameof(Ability.CraftEther), 2);
            ItemEssenceCosts.Add(nameof(Ability.CraftElixir), 3);
            ItemEssenceCosts.Add(nameof(Ability.CraftMegaElixir), 4);

        }


        static int GetItemCount(Item item, List<InventoryItem> inv)
        {
            foreach (InventoryItem ii in inv)
            {
                if (ii.item == item)
                {
                    return ii.count;
                }
            }

            return -1;
        }


        static bool AttemptUseItem(Item item, Ability ability, Hero target)
        {

            if (target.health <= 0)
            {
                Console.WriteLine("Item Target is Dead");
                return false;
            }

            int count = GetItemCount(item, TeamHeroCoder.BattleState.allyInventory);

            if (count != -1 && count > 0)
            {
                Console.WriteLine("Using " + item.ToString() + " on " + target.jobClass.ToString());
                TeamHeroCoder.PerformHeroAbility(ability, target);
                return true;
            }

            Console.WriteLine("not enough " + item.ToString());
            return false;
        }




        static bool AttemptCastSpell(Ability ability, Hero target)
        {

            if (hasStatus(activeHero, StatusEffect.Silence))
            {
                Console.WriteLine("Can't cast spell, hero is silenced");
                return false;
            }

            if (ability != Ability.Resurrection && target.health <= 0)
            {
                Console.WriteLine("Can't cast spell, Target is dead");
                return false;
            }

            string abilityName = ability.ToString();

            if (!SpellCosts.ContainsKey(abilityName))
            {
                Console.WriteLine(abilityName + " Spell not in dict!");
                return false;
            }

            if (activeHero.mana >= SpellCosts[abilityName])
            {
                Console.WriteLine("Casting " + abilityName + " on " + target.jobClass.ToString());
                TeamHeroCoder.PerformHeroAbility(ability, target);
                return true;
            }
            else
            {
                Console.WriteLine("not enough mana to cast " + abilityName);
                return false;
            }
        }


        static bool AttemptCraftItem(Item item, Ability ability)
        {
            string itemName = ability.ToString();

            if (!ItemEssenceCosts.ContainsKey(itemName))
            {
                Console.WriteLine(itemName + " Item not in dict!");
                return false;
            }


            if (TeamHeroCoder.BattleState.allyEssenceCount >= ItemEssenceCosts[itemName])
            {
                return AttemptCastSpell(ability, activeHero);
            }
            else
            {
                Console.WriteLine("not enough Essence to craft " + itemName);
                return false;
            }
        }

        static public bool hasStatus(Hero h, StatusEffect effect)
        {
            foreach (StatusEffectAndDuration s in h.statusEffectsAndDurations)
            {
                if (s.statusEffect == effect) return true;
            }

            return false;
        }

        static public Hero FindHeroWithHealthPercentBellow(int percent, List<Hero> team )
        {
            foreach(Hero h in team)
            {
                if (((float)h.health/(float)h.maxHealth)*100.0f < percent)
                {
                    return h;
                }
            }

            return null;
        }

        static public Hero FindHeroWithManaPercentBellow(int percent, List<Hero> team)
        {
            foreach (Hero h in team)
            {
                if (((float)h.mana / (float)h.maxMana) * 100.0f < percent)
                {
                    return h;
                }
            }

            return null;
        }

        static public Hero FindClassOnTeam(List<Hero> heroes, HeroJobClass jClass)
        {
            foreach (Hero h in heroes)
            {
                if (h.jobClass == jClass) return h;
            }

            return null;
        }

        static public bool BuffNotBuffed(StatusEffect status, Ability ability, Hero targetHero)
        {

            if (targetHero.health <= 0)
            {
                Console.WriteLine("Cannot buff dead target");
                return false;
            }
            

            if (!hasStatus(targetHero, status))
            {
                
                return AttemptCastSpell(ability, targetHero);
            }
            else
            {
                Console.WriteLine(targetHero.jobClass.ToString() + " already has " + status.ToString());
                return false;
            }
        }

        static public int NegStatusCount(Hero h)
        {

            int negCount = 0;   

            foreach (StatusEffectAndDuration se in h.statusEffectsAndDurations)
            {
                if (
                    se.statusEffect == StatusEffect.Defaith || 
                    se.statusEffect == StatusEffect.Silence || 
                    se.statusEffect == StatusEffect.Slow || 
                    se.statusEffect == StatusEffect.Debrave || 
                    se.statusEffect == StatusEffect.Doom ||
                    se.statusEffect == StatusEffect.Petrified ||
                    se.statusEffect == StatusEffect.Petrifying ||
                    se.statusEffect == StatusEffect.Poison
                    )
                {
                    negCount++;
                }
            }

            return negCount;
        }

    }
}
