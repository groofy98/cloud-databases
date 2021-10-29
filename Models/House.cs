using System;
using System.Collections.Generic;

namespace Models
{
    public class House
    {
        public Guid Id { get; set; }

        public double Price { get; set; }
        
        public string HouseNumber { get; set; }
        
        public string PostalCode { get; set; }
        
        public string Street { get; set; }
        
        public string Town { get; set; }

        public List<Image> Images { get; set; } = new ();
    }
}
