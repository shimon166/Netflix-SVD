using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Exercise1.Model
{
    public class Rating
    {
        public int RatingNumber { get; set; }

        public Item RatedItem { get; set; }

        public User RatingUser { get; set; }
    }
}
