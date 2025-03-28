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
        public static string FolderExchangePath = "C:/Users/Ryan/AppData/LocalLow/Wind Jester Games/Team Hero Coder";

        static public void ProcessAI()
        {
            Console.WriteLine("Processing AI!");

            #region SampleCode

            if (TeamHeroCoder.BattleState.heroWithInitiative.jobClass == HeroJobClass.Fighter)
            {
                //The character with initiative is a figher, do something here...

                Console.WriteLine("this is a fighter");
            }
            else if (TeamHeroCoder.BattleState.heroWithInitiative.jobClass == HeroJobClass.Cleric)
            {
                //The character with initiative is a figher, do something here...

                Console.WriteLine("this is a cleric");
            }
            else if (TeamHeroCoder.BattleState.heroWithInitiative.jobClass == HeroJobClass.Wizard)
            {
                //The character with initiative is a figher, do something here...

                Console.WriteLine("this is a wizard");
            }

            foreach (InventoryItem ii in TeamHeroCoder.BattleState.allyInventory)
            {

            }

            //Find the foe with the lowest health
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

            //This is the line of code that tells FG that we want to perform the attack action and target the foe with the lowest HP
            TeamHeroCoder.PerformHeroAbility(Ability.Attack, target);

            //Searching for a poisoned hero 
            foreach (Hero hero in TeamHeroCoder.BattleState.allyHeroes)
            {
                foreach (StatusEffectAndDuration se in hero.statusEffectsAndDurations)
                {
                    if (se.statusEffect == StatusEffect.Poison)
                    {
                        //We have found a character that is poisoned, do something here...
                    }
                }
            }

            #endregion

        }
    }
}
