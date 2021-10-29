using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models
{
    public class MortgageOffer
    {
        public Applicant Applicant {  get; set; }        
        public Uri PDFUrl {  get; set; } // The Sas token to acces the created PDF file
    }
}
