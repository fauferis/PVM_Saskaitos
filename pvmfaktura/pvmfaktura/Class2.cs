using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pvmfaktura
{
    internal class Class2
    {
        public decimal skaiciuoti (decimal priceWithoutPVM, decimal quantity)
        {
            return priceWithoutPVM * quantity;
        }
        public decimal CalculatePVM(decimal priceWithoutPVM, decimal pvmRate, int quantity)
        {
            return (priceWithoutPVM * quantity) * pvmRate / 100;
        }

        public decimal CalculateTotal(decimal priceWithoutPVM, decimal pvm, int quantity)
        {
            return (priceWithoutPVM * quantity) + pvm;
        }
    }
}
