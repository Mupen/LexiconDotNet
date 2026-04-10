using SalesSystem.Domain.Contracts;
using SalesSystem.Domain.Entities;
using SalesSystem.Domain.Extensions;
using SalesSystem.Domain.Enums;
using SalesSystem.Domain.Interfaces;

namespace SalesSystem.Infrastructure.Services;

public sealed class TicketPricingService : ITicketPricingService
{
    private const string CampaignCode = "MAY26";

    public Result<decimal> CalculatePrice(Movie movie, Showing showing, int age, bool customerIsPresent, bool hasAtLeastOnePayingCustomerInOrder, bool hasAccompanyingAdult, string? campaignCode)
    {
        var ageValidation = ValidateAge(age);
        if (ageValidation.IsFailure)
            return Result<decimal>.Failure(ageValidation.Error);

        var ageRestrictionValidation = ValidateAgeRestriction(movie, age, hasAccompanyingAdult);
        if (ageRestrictionValidation.IsFailure)
            return Result<decimal>.Failure(ageRestrictionValidation.Error);

        var basePriceResult = GetBasePrice(showing);
        if (basePriceResult.IsFailure)
            return basePriceResult;

        var agePriceResult = GetAgeBasedPrice(
            basePriceResult.Value,
            age,
            customerIsPresent,
            hasAtLeastOnePayingCustomerInOrder);

        if (agePriceResult.IsFailure)
            return agePriceResult;

        var discountedPrice = ApplyCampaignDiscount(showing, agePriceResult.Value, campaignCode);

        return Result<decimal>.Success(
            decimal.Round(discountedPrice, 2, MidpointRounding.AwayFromZero));
    }

    private static Result ValidateAge(int age)
    {
        if (age < 0)
        {
            return Result.Failure(
                new Error("TicketPricing.InvalidAge", "Age cannot be negative."));
        }

        return Result.Success();
    }

    private static Result ValidateAgeRestriction(Movie movie, int age, bool hasAccompanyingAdult)
    {
        return movie.AgeRating switch
        {
            AgeRating.G => Result.Success(),

            AgeRating.PG => Result.Success(),

            AgeRating.PG13 => age >= 13 || hasAccompanyingAdult
                ? Result.Success()
                : Result.Failure(new Error(
                    "TicketPricing.AgeRestricted",
                    "Customer under 13 must be accompanied by a parent or adult guardian.")),

            AgeRating.R => age >= 17 || hasAccompanyingAdult
                ? Result.Success()
                : Result.Failure(new Error(
                    "TicketPricing.AgeRestricted",
                    "Customer under 17 must be accompanied by a parent or adult guardian.")),

            AgeRating.NC17 => age >= 18
                ? Result.Success()
                : Result.Failure(new Error(
                    "TicketPricing.AgeRestricted",
                    "No one 17 or under is admitted to this movie.")),

            _ => Result.Failure(new Error(
                "TicketPricing.InvalidAgeRating",
                "Invalid movie age rating."))
        };
    }

    private static Result<decimal> GetBasePrice(Showing showing)
    {
        if (showing.StartTime == new TimeOnly(13, 0))
            return Result<decimal>.Success(105m);

        if (showing.StartTime == new TimeOnly(18, 0) || showing.StartTime == new TimeOnly(21, 0))
            return Result<decimal>.Success(130m);

        return Result<decimal>.Failure(
            new Error("TicketPricing.InvalidShowingTime", "No ticket price exists for this showing time."));
    }

    private static Result<decimal> GetAgeBasedPrice(decimal basePrice, int age, bool customerIsPresent, bool hasAtLeastOnePayingCustomerInOrder)
    {
        if (age < 6)
        {
            if (!customerIsPresent)
                return Result<decimal>.Success(90m);

            if (!hasAtLeastOnePayingCustomerInOrder)
            {
                return Result<decimal>.Failure(
                    new Error(
                        "TicketPricing.ChildRequiresPayingCustomer",
                        "A child under 6 must be accompanied by at least one paying customer."));
            }

            return Result<decimal>.Success(0m);
        }

        if (age is >= 6 and <= 11)
            return Result<decimal>.Success(customerIsPresent ? 65m : 90m);

        if (age >= 67)
            return Result<decimal>.Success(90m);

        return Result<decimal>.Success(basePrice);
    }

    private static decimal ApplyCampaignDiscount(
        Showing showing,
        decimal currentPrice,
        string? campaignCode)
    {
        if (currentPrice <= 0)
            return currentPrice;

        if (string.IsNullOrWhiteSpace(campaignCode))
            return currentPrice;

        if (!string.Equals(campaignCode.Trim(), CampaignCode, StringComparison.OrdinalIgnoreCase))
            return currentPrice;

        // Campaign valid only in May at 18:00
        if (showing.Date.Month != 5 || showing.StartTime != new TimeOnly(18, 0))
            return currentPrice;

        return currentPrice * 0.5m;
    }
}