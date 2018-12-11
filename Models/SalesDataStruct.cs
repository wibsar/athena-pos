using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Athena
{
    public struct SalesDataStruct
    {
        public User User { get; set; }
        public Customer Customer { get; set; }
        public ObservableCollection<ProductBase> Products { get; set; }
        public ReceiptType ReceiptType { get; set; }
        public double PointsObtained { get; set; }

    }
}