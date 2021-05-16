using System;
using System.Collections.Generic;
using System.Text;

namespace Brute_Force.ViewModel
{
    /// <summary>
    /// Class for shows shelves and theres datas on ShelfProduct view.
    /// </summary>
    public class Item
    {
        public String Coord { get; set; }

        public string Name { get; set; }

        public int ProductsCount { get; set; }

        public List<String> Products { get; set; }
    }
}
