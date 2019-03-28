using System;
using System.Collections.Generic;
using System.Text;

namespace WBS_Testing_01
{
    public class Recipe
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public float? Rating { get; set; }
        public float? Protein { get; set; }
        public float? Fat { get; set; }
        public string Date { get; set; }
        public float? Caliories { get; set; }
        public string Desc { get; set; }
        public float? Sodium { get; set; }
        public List<string> Ingredients { get; set; }
        public List<string> Directions { get; set; } = new List<string>();
        public List<string> Categories { get; set; } = new List<string>();
    }
}
