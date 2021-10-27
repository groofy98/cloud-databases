using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models
{
    public class Applicant
    {
        public Guid Id {  get; set; }
        public string FirstName {  get; set; }
        public string LastName {  get; set; }
        public string Email {  get; set; }
        public double Income {  get; set; }
        public double Loans {  get; set; }
    }
}
