using System;
using CRM.Models.JunctionModel;

namespace CRM.Models.ViewModel;

public class HybridModel
{
    public NavbarModel Navbar {get; set;} = new NavbarModel();
    public List<Product> Products {get; set;} = [];
    public Product? Product {get; set;}
    public List<CartProduct> CartProducts {get; set;} = [];
    public Cart? Cart {get; set;}
    public Address? Address {get; set;}
     public Order? Order {get; set;}
    public List<Order> Orders {get; set;} = [];
    public List<OrderProduct> OrderProducts {get; set;} = [];

}
