using System.ComponentModel.DataAnnotations;

namespace EasyStore.Data.Entities;

public class Sale
{
    [Key]
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public double Quantity { get; set; }
    public decimal TotalPrice { get; set; }
    public DateTime SaleDate { get; set; }
    public string GroupGuid { get; set; } = string.Empty;

    public string CashierName { get; set; } = string.Empty;
}
