//------------------------------------------------------------------------------
// <auto-generated>
//     Этот код создан по шаблону.
//
//     Изменения, вносимые в этот файл вручную, могут привести к непредвиденной работе приложения.
//     Изменения, вносимые в этот файл вручную, будут перезаписаны при повторном создании кода.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Kursovaya
{
    using System;
    using System.Collections.Generic;
    
    public partial class Stat
    {
        public int ID { get; set; }
        public int SessionsID { get; set; }
        public decimal Income { get; set; }
        public decimal PriceDecoration { get; set; }
        public decimal PricePersonal { get; set; }
        public decimal ClearProfit { get; set; }
        public int TicketsSold { get; set; }
    
        public virtual Sessions Sessions { get; set; }
    }
}
