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
                    if (target == null)
                        target = hero;
                    else if (hero.health < target.health)
                        target = hero;
                }
            }


            //--- class code ---//

            if (TeamHeroCoder.BattleState.heroWithInitiative.jobClass == HeroJobClass.Cleric)
            {
                //The character with initiative is a figher, do something here...

                Console.WriteLine("this is a cleric");


                // haste on self
                if (!hasStatus(activeHero, StatusEffect.Haste))
                {
                    Console.WriteLine("Cleric does not have haste, casting haste on cleric");
                    if(AttemptCastSpell(Ability.Haste,activeHero))return;
                }


                //resurrection
                Hero resTarget = FindHeroWithHealthPercentBellow(10, TeamHeroCoder.BattleState.allyHeroes);
                if (resTarget != null)
                {
                    if (AttemptCastSpell(Ability.Resurrection, resTarget)) return;
                }

                //cure seriouse
                Hero cureTarget = FindHeroWithHealthPercentBellow(50, TeamHeroCoder.BattleState.allyHeroes);
                if (cureTarget != null)
                {
                    if (AttemptCastSpell(Ability.CureSerious, cureTarget)) return;
                }


                //wizard clense and buffs
                Hero Wizard = FindClassOnTeam(TeamHeroCoder.BattleState.allyHeroes, HeroJobClass.Wizard);
                //note: this will not work will with parties that have more than one wizard.
                if (Wizard != null)
                {
                    //cleanse if silenced
                    if (hasStatus(Wizard, StatusEffect.Silence))
                    {
                        Console.WriteLine("Attempting quick cleanse on silenced wizard");
                        if (AttemptCastSpell(Ability.QuickCleanse, Wizard)) return;
                    }

                    // cast faith on wizard
                    if (BuffNotBuffed(StatusEffect.Faith,Ability.Faith,Wizard))return;

                }


                //cleans ally with debufs
                foreach (Hero h in TeamHeroCoder.BattleState.allyHeroes)
                {
                    if (h.statusEffectsAndDurations.Count > 0)
                    {
                        if(AttemptCastSpell(Ability.QuickCleanse, h))return;
                    }
                }


                //use ether on ally

                foreach (Hero h in TeamHeroCoder.BattleState.allyHeroes)
                {
                    if(h.mana < (float)h.maxMana * 0.5f)
                    {
                        if (AttemptUseItem(Item.Ether, Ability.Ether, h)) return;
                    }
                }


                //quick heal
                Hero qHealTarget = FindHeroWithHealthPercentBellow(80, TeamHeroCoder.BattleState.allyHeroes);
                if (qHealTarget != null)
                {
                    if (AttemptCastSpell(Ability.QuickHeal, qHealTarget)) return;
                }




            }
            else if (TeamHeroCoder.BattleState.heroWithInitiative.jobClass == HeroJobClass.Wizard)
            {
                //The character with initiative is a figher, do something here...

                Console.WriteLine("this is a wizard");

                if (activeHero.mana < activeHero.maxMana*0.4f)
                {
                    Console.WriteLine("using ether on wizard");
                    if(AttemptUseItem(Item.Ether,Ability.Ether,activeHero)) return;
                }

                Console.WriteLine("casting meteor");
                if(AttemptCastSpell(Ability.Meteor,target)) return;


            }
            else if (TeamHeroCoder.BattleState.heroWithInitiative.jobClass == HeroJobClass.Alchemist)
            {
                //The character with initiative is a figher, do something here...

                Console.WriteLine("this is an Alchemist");


                //dispel auto life on foe
                foreach (Hero h in TeamHeroCoder.BattleState.foeHeroes)
                {
                    if (hasStatus(h, StatusEffect.AutoLife) && h.health < (float)h.maxHealth*0.6f)
                    {
                        if(AttemptCastSpell(Ability.Dispel,h))return;
                    }
                }

                //check use potion
                Hero allyToHeal = FindHeroWithHealthPercentBellow(30,TeamHeroCoder.BattleState.allyHeroes);
                if (allyToHeal != null)
                {
                    if (AttemptUseItem(Item.Potion, Ability.Potion, allyToHeal)) return;
                }


                //check use ether
                Hero allyToEther = FindHeroWithManaPercentBellow(50, TeamHeroCoder.BattleState.allyHeroes);
                if (allyToEther != null)
                {
                    if (AttemptUseItem(Item.Ether, Ability.Ether, allyToEther)) return;
                }


                //check craft ether


            }
















            // default action for all classes
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

        }


        static bool AttemptUseItem(Item item, Ability ability, Hero target)
        {
            foreach (InventoryItem ii in TeamHeroCoder.BattleState.allyInventory)
            {
                if (ii.item == item && ii.count > 0)
                {
                    TeamHeroCoder.PerformHeroAbility(ability, target);
                    return true;
                }
            }
            return false;
        }

        static bool AttemptCastSpell(Ability ability, Hero target)
        {

            string abilityName = ability.ToString();

            if (!SpellCosts.ContainsKey(abilityName))
            {
                Console.WriteLine("Spell not in dict!");
                return false;
            }

            if (activeHero.mana >= SpellCosts[abilityName])
            {
                TeamHeroCoder.PerformHeroAbility(ability, target);
                return true;
            }
            else
            {
                Console.WriteLine("not enough mana");
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
                return false;
            }
            

            if (!hasStatus(targetHero, status))
            {
                
                return AttemptCastSpell(ability, targetHero);
            }
            else
            {
                return false;
            }
        }

    }
}
