using Brute_Force.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Brute_Force.Persistence
{
    /// <summary>
    /// Class of the data access.
    /// </summary>
    public class DataAccess : IDataAccess
	{
        /// <summary>
        /// Reading depot data from file.
        /// </summary>
        /// <param name="path">The path of the input file.</param>
        /// <returns>A Depot object with data.</returns>
        public async Task<Depot> LoadAsync(String path)
        {
            try
            {
                using (StreamReader reader = new StreamReader(path))
                {
                    string line = await reader.ReadLineAsync();
                    string[] numbers = line.Split(' ');
                    int _height = Int32.Parse(numbers[0]);
                    int _width = Int32.Parse(numbers[1]);
                    int _robots = Int32.Parse(numbers[2]);
                    int _maxEnergy = Int32.Parse(numbers[3]);
                    int _chargers = Int32.Parse(numbers[4]);
                    int _stations = Int32.Parse(numbers[5]);

                    Depot depot = new Depot(_height, _width)
                    {
                        Robots = _robots,
                        Chargers = _chargers,
                        Stations = _stations,
                        MaxEnergy = _maxEnergy
                    };

                    for (int i = 0; i < _height; i++)
                    {
                        for (int j = 0; j < _width; j++)
                        {
                            depot.SetValue(i, j, "0");
                        }
                    }

                    int x, y;

                    while (!reader.EndOfStream)
                    {
                        line = await reader.ReadLineAsync();
                        numbers = line.Split('-');
                        if (numbers[0] == "P")
                        {
                            string[] tmp = numbers[1].Split(':');
                            string[] coords = tmp[0].Split(',');
                            foreach (var item in coords)
                            {
                                string[] nums = item.Split(' ');
                                x = Int32.Parse(nums[0]);
                                y = Int32.Parse(nums[1]);
                                depot.SetValue(x, y, numbers[0]);
                                if (tmp.Length > 1) 
                                {
                                    string[] products = tmp[1].Split(',');
                                    foreach (var product in products)
                                    {
                                        depot.SetProductsValue(x, y, Int32.Parse(product));
                                    }   
                                }
                            }
                        }
                        else if(numbers[0] == "O")
                        {
                            string orderId = numbers[1].Split(":")[0];
                            string time = numbers[1].Split(":")[1];
                            depot.AddOrdersToOrderList(Int32.Parse(orderId), Int32.Parse(time));
                        }
                        else
                        {
                            string[] coords = numbers[1].Split(',');
                            foreach (var item in coords)
                            {
                                string[] nums = item.Split(' ');
                                x = Int32.Parse(nums[0]);
                                y = Int32.Parse(nums[1]);
                                depot.SetValue(x, y, numbers[0]);
                            }
                        }
                    }

                    return depot;
                }
            }
            catch
            {
                throw new DepotException();
            }
        }
        /// <summary>
        /// Reading depot matrix data from file.
        /// </summary>
        /// <param name="path">The path of the input file.</param>
        /// <returns>A Depot object with data.</returns>
        public async Task<Depot> LoadAsyncMatrix(String path)
        {
            try
            {
                using (StreamReader reader = new StreamReader(path))
                {
                    string line = await reader.ReadLineAsync();
                    string[] numbers = line.Split(' ');
                    int _height = Int32.Parse(numbers[0]);
                    int _width = Int32.Parse(numbers[1]);
                    int _robots = Int32.Parse(numbers[2]);
                    int _maxEnergy = Int32.Parse(numbers[3]);
                    int _chargers = Int32.Parse(numbers[4]);
                    int _stations = Int32.Parse(numbers[5]);

                    Depot depot = new Depot(_height, _width)
                    {
                        Robots = _robots,
                        Chargers = _chargers,
                        Stations = _stations,
                        MaxEnergy = _maxEnergy
                    };

                    for (int i = 0; i < _height; i++)
                    {
                        line = await reader.ReadLineAsync();
                        numbers = line.Split(' ');

                        for (int j = 0; j < _width; j++)
                        {
                            depot.SetValue(i, j, numbers[j]);
                        }
                    }

                    for (int i = 0; i < _height; i++)
                    {
                        line = await reader.ReadLineAsync();
                        numbers = line.Split(' ');

                        for (int j = 0; j < _width; j++)
                        {
                            if (numbers[j].Length > 1)
                            {
                                string[] data = numbers[j].Split(',');

                                for (int k = 0; k < data.Length; k++)
                                {
                                    depot.SetProductsValue(i, j, Int32.Parse(data[k]));
                                }
                            }
                            else
                            {
                                depot.SetProductsValue(i, j, Int32.Parse(numbers[j]));
                            }
                        }
                    }

                    return depot;
                }
            }
            catch
            {
                throw new DepotException();
            }
        }

        /// <summary>
        /// Writing depot data to file.
        /// </summary>
        /// <param name="path">The path of the output file.</param>
        /// <param name="depot">A Depot object from which the data is written to file.</param>
        public async Task SaveDepotAsync(String path, Depot depot)
        {
            try
            {
                using (StreamWriter writer = new StreamWriter(path))
                {
                    await writer.WriteAsync(depot.Height + " " + depot.Width + " " + depot.Robots + " " + depot.MaxEnergy + " " + depot.Chargers + " " + depot.Stations);

                    for (int i = 0; i < depot.Height; i++)
                    {
                        for (int j = 0; j < depot.Width; j++)
                        {
                            if (depot[i, j] != '0')
                            {
                                await writer.WriteAsync("\n" + depot[i, j] + "-" + i + " " + j);
                                if (depot[i, j] == 'P' && depot.Products[i, j].Count != 0)
                                {
                                    await writer.WriteAsync(":");
                                    for (int z = 0; z < depot.Products[i, j].Count; z++)
                                    {
                                        if (z == depot.Products[i, j].Count - 1) await writer.WriteAsync(depot.Products[i, j][z] + "");
                                        else await writer.WriteAsync(depot.Products[i, j][z] + ",");
                                    }
                                }
                            }
                        }
                    }

                    foreach(Order order in depot.Orders)
                    {
                        await writer.WriteAsync("\nO-" + order.ProductId + ":" + order.OrderTime);
                    }
                }
            }
            catch
            {

                throw new DepotException();
            }
        }

        /// <summary>
        /// Writing statistics to file.
        /// </summary>
        /// <param name="path">The path of the output file.</param>
        /// <param name="steps">The number of steps taken.</param>
        /// <param name="robots">A list containing robots.</param>
        public async Task SaveStatsAsync(String path, Int32 steps, List<Robot> robots)
        {
            try
            {
                int allEnergyUsed = 0;
                using (StreamWriter writer = new StreamWriter(path))
                {
                    writer.WriteLine("Lépések száma: " + steps);
                    for (int i = 0; i < robots.Count; i++)
                    {
                        allEnergyUsed += robots[i].UsedEnergy;
                        await writer.WriteAsync("A(z) " + (i+1) + ". robot által felhasznált energia:" + robots[i].UsedEnergy + " ");
                        await writer.WriteLineAsync();
                    }
                    await writer.WriteAsync("A robotok által felhasznált teljes energia mennyiség: " + allEnergyUsed + " ");
                }
            }
            catch
            {
                throw new DepotException();
            }
        }
    }
}
