using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace zoo4
{

    public enum FoodType
    {
        Meat,
        Corn,
        Water,
        Empty // Becomes when container with food is eaten
    }

    public class Food
    {
        public FoodType Type;
        public int Amount;

        public void PassToAnimal(Animal animal)
        {
            // added using unit test
            if (Type == FoodType.Water)
                animal.CurrentWaterAmount += Amount;
            else
                animal.CurrentFoodAmount += Amount;
            Type = FoodType.Empty;
            Amount = 0;
        }
    }

    public class Zoo
    {
        private int[] Storage = new int[Enum.GetValues(typeof(FoodType)).Length];

        public int MeatAmount => Storage[(int)FoodType.Meat];
        public int CornAmount => Storage[(int)FoodType.Corn];
        public int WaterAmount => Storage[(int)FoodType.Water];

        private object _storageLocker = new object();
        private object _animalLocker = new object();

        public List<ZooMember> Entities = new List<ZooMember>();

        public bool HasAnimals => Entities.OfType<Animal>().Any();

        private int CurrentID = 1;

        public static int DaysPassed = 0;

        public virtual event EventHandler DayEnded;

        public Action<Animal> DieHandler;

        public Administrator Administrator;

        public Zoo()
        {
            DieHandler = (animal) =>
            {
                lock (_animalLocker)
                {
                    Log.Write($"{animal.Name} died!");
                    Entities.Remove(animal);
                    NotifyAdmin();
                }
            };
        }

        public void NotifyAdmin()
        {
            Administrator?.SyncWithZoo();
        }

        public int NextId()
        {
            int buff = CurrentID;
            CurrentID++;
            return buff;
        }

        public Food GetFood(FoodType type, int amount)
        {
            lock (_storageLocker)
            {
                if (Storage[(int)type] < amount)
                    throw new ZooException($"Not enough food of type {type}!");
                Storage[(int)type] -= amount;
            }
            return new Food { Type = type, Amount = amount };
        }

        public void AddFood(FoodType type, int amount)
        {
            lock (_storageLocker)
            {
                Storage[(int)type] += amount;
            }
        }

        public void LoadStorage()
        {
            AddFood(FoodType.Meat, 20);
            AddFood(FoodType.Corn, 20);
            AddFood(FoodType.Water, 40);
        }

        public void ReportDayEnd()
        {
            //LoadStorage();
            DayEnded?.Invoke(this, EventArgs.Empty);
            Log.Write("End of day!");
            DaysPassed++;
            NotifyAdmin();
        }
    }
}
