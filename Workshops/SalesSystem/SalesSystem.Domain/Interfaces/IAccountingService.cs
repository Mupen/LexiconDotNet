using SalesSystem.Domain.Entities;

namespace SalesSystem.Domain.Interfaces;

public interface IAccountingService
{
    void RegisterTicketSale(IEnumerable<TicketOrderItem> tickets);
    void RegisterProductSale(IEnumerable<(Product Product, int Quantity)> items);

    decimal GetTotalTicketSales();
    decimal GetTotalProductSales();

    decimal GetTotalVat25();
    decimal GetTotalVat12();
}