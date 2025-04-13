using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CRM.Models.JunctionModel;

public class OrderProduct
{
    [Key]
    public Guid OrderProductId { get; set; } = Guid.NewGuid();
    public Guid OrderId { get; set; }
    [ForeignKey("OrderId")]
    public Order? Order { get; set; }

    public Guid ProductId {get; set;}
    [ForeignKey("ProductId")]
    public Product? Product { get; set; }

    public int Quantity {get; set;}


}
