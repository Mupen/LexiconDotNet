using SalesSystem.Domain.Entities;
using SalesSystem.Domain.Contracts;

namespace SalesSystem.Domain.Interfaces;

public interface ITicketPricingService
{
    Result<decimal> CalculatePrice(
        Movie movie,
        Showing showing,
        int age,
        bool customerIsPresent,
        bool hasAtLeastOnePayingCustomerInOrder,
        bool hasAccompanyingAdult,
        string? campaignCode);
}