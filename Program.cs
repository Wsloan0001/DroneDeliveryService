using System;
using System.Collections.Generic;
using System.Linq;

namespace DroneDeliveryService
{
    class Program
    {
        static void Main(string[] args)
        {
            DroneService droneService = new DroneService();
            PackageService packageService = new PackageService();
            bool showMenu = true;
            string menuInput = "";
            while (showMenu)
            {
                Console.WriteLine("1: Enter drone");
                Console.WriteLine("2: Enter drone list");
                Console.WriteLine("3: Enter package");
                Console.WriteLine("4: Enter package list");
                Console.WriteLine("5: Run");
                Console.WriteLine("0: Exit");
                menuInput = Console.ReadLine();
                switch (menuInput)
                {
                    case "1":
                        {
                            droneService.CreateAddDroneDialog();
                            break;
                        }
                    case "2":
                        {
                            droneService.CreateAddDronesByListDialog();
                            break;
                        }
                    case "3":
                        {
                            packageService.CreatePackageDialog();
                            break;
                        }
                    case "4":
                        {
                            packageService.CreateAddPackagesByListDialog();
                            break;
                        }
                    case "5":
                        {
                            GenerateService.GenerateOutput(droneService.GetDrones(), packageService.GetPackages());
                            break;
                        }
                    case "0":
                        showMenu = false;
                        break;
                    default:
                        Console.Clear();
                        break;
                }
            }
            Environment.Exit(0);
        }
    }

    public static class EnumerableExtensions
    {
        //Returns every combination of elements
        public static IEnumerable<IEnumerable<T>> Combinations<T>(this IEnumerable<T> elements, int k)
        {
            return k == 0 ? new[] { new T[0] } :
              elements.SelectMany((e, i) =>
                elements.Skip(i + 1).Combinations(k - 1).Select(c => (new[] { e }).Concat(c)));
        }
    }

    public class Drone
    {
        public string name { get; set; }
        public int weightLimit { get; set; }
        public List<List<Package>> trips { get; set; }
        public Drone()
        {
            trips = new List<List<Package>>();
        }
    }

    public class DroneService
    {
        List<Drone> drones { get; set; }
        public DroneService()
        {
            drones = new List<Drone>();
        }
        public void CreateAddDroneDialog()
        {
            if(drones.Count() >= 100)
            {
                Console.Clear();
                Console.WriteLine("Maximum Drone Count Reach of 100\n");
            }
            else
            {
                Drone drone = new Drone();
                Console.Clear();
                Console.WriteLine("Enter Drone Name:");
                drone.name = Console.ReadLine();
                Console.WriteLine("Enter Drone Weight Limit:");
                drone.weightLimit = Int32.Parse(Console.ReadLine());
                addDrone(drone);
                Console.Clear();
            }
        }

        public void CreateAddDronesByListDialog()
        {
            Console.Clear();
            Console.WriteLine("DroneName, MaximumWeight, ect.");
            Console.WriteLine("Enter Drone List:");
            var listString = Console.ReadLine();
            var list = listString.Split(',').Select(s => s.Trim()).ToList();
            while(list.Count() > 0)
            {
                var drone = new Drone();
                var droneData = list.Take(2).ToList();
                drone.name = droneData[0];
                drone.weightLimit = int.Parse(droneData[1]);
                addDrone(drone);
                list.RemoveRange(0, 2);
            }
        }

        public void addDrone(Drone drone)
        {
            drones.Add(drone);
        }

        public List<Drone> GetDrones()
        {
            return drones;
        }
    }

    public class Package
    {
        public string location { get; set; }
        public int weight { get; set; }
    }

    public class PackageService
    {

        List<Package> packages { get; set; }
        public PackageService()
        {
            packages = new List<Package>();
        }
        public void CreatePackageDialog()
        {
            Package package = new Package();
            Console.Clear();
            Console.WriteLine("Enter Package Location:");
            package.location = Console.ReadLine();
            Console.WriteLine("Enter Package Weight:");
            package.weight = Int32.Parse(Console.ReadLine());
            addPackage(package);
            Console.Clear();
        }

        public void CreateAddPackagesByListDialog()
        {
            Console.Clear();
            Console.WriteLine("LocationName, PackageWeight, ect.");
            Console.WriteLine("Enter Package List:");
            var listString = Console.ReadLine();
            var list = listString.Split(',').Select(s => s.Trim()).ToList();
            while (list.Count() > 0)
            {
                var package = new Package();
                var droneData = list.Take(2).ToList();
                package.location = droneData[0];
                package.weight = int.Parse(droneData[1]);
                addPackage(package);
                list.RemoveRange(0, 2);
            }
        }
        public void addPackage(Package package)
        {
            packages.Add(package);
        }

        public List<Package> GetPackages()
        {
            return packages;
        }
    }

    public static class GenerateService
    {
        public static void GenerateOutput(List<Drone> droneList, List<Package> packageList)
        {
            var drones = droneList.OrderBy(d => d.weightLimit).ToList();
            var packages = packageList;
            int maxWeight = drones.Max(d => d.weightLimit);

            Console.Clear();
            Console.WriteLine("Generating output...\n");
            
            //Remove any packages that cant be carried
            List<Package> undeliverables = packages.Where(p => p.weight > maxWeight).ToList();
            packages.RemoveAll(p => p.weight > maxWeight);

            //Goes through the list of drones ordered from smallest weight limit to greatest and finds the most packages a the drone can carry.
            while (packages.Count() > 0)
            {
                foreach (Drone drone in drones)
                {
                    var packagesForDroneLst = ClosestWeightList(packages, drone.weightLimit);
                    foreach (Package package in packagesForDroneLst)
                    {
                        packages.Remove(package);
                    }
                    if (packagesForDroneLst.Count() > 0)
                    {
                        drone.trips.Add(packagesForDroneLst);
                    }
                    if (packages.Count() == 0)
                    {
                        break;
                    }
                }
            }

            GenerateDroneOutput(drones);
            GenerateUnDeliverableOutput(undeliverables);
        }

        private static void GenerateDroneOutput(List<Drone> drones)
        {
            foreach (Drone drone in drones)
            {
                int tripNum = 1;
                Console.WriteLine($"Drone:{drone.name} Weight Limit:{drone.weightLimit}");
                foreach (var trip in drone.trips)
                {
                    Console.WriteLine($"Trip #{tripNum} Total Weight:{trip.Sum(x => x.weight)}");
                    foreach (var package in trip)
                    {
                        Console.WriteLine($"  Location:{package.location} Weight:{package.weight}");
                    }
                    tripNum += 1;
                }
                Console.WriteLine("\n");
            }
        }

        private static void GenerateUnDeliverableOutput(List<Package> undeliverables)
        {
            if (undeliverables.Count() > 0)
            {
                Console.WriteLine("Packages over weight for delivery:");
                foreach (Package package in undeliverables)
                {
                    Console.WriteLine($"Location: {package.location} Weight: {package.weight}");
                }
                Console.WriteLine("\n");
            }
        }

        //Find and return the greatest total of packages a weight can hold from list of packages
        private static List<Package> ClosestWeightList(List<Package> packages, int maxWeight)
        {
            var target = Enumerable.Range(1, packages.Count)
                .SelectMany(p => packages.Combinations(p))
                .Where(p => p.Sum(x => x.weight) <= maxWeight)
                .OrderByDescending(p => p.Count()).FirstOrDefault();

            return target != null ? target.ToList() : new List<Package>();
        }
    }
}
