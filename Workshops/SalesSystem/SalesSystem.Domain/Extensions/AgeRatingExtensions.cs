using SalesSystem.Domain.Enums;

namespace SalesSystem.Domain.Extensions;

public static class AgeRatingExtensions
{
    public static string GetLabel(this AgeRating ageRating)
    {
        return ageRating switch
        {
            AgeRating.G => "G",
            AgeRating.PG => "PG",
            AgeRating.PG-13 => "PG-13",
            AgeRating.R => "R",
            AgeRating.NC17 => "NC-17",
            _ => "Unknown"
        };
    }

    public static string GetDescription(this AgeRating ageRating)
    {
        return ageRating switch
        {
            AgeRating.G => "General audiences: All ages admitted.",
            AgeRating.PG => "Parental guidance suggested: Some material may not be suitable for children.",
            AgeRating.PG13 => "Parents strongly cautioned: Some material may be inappropriate for children under 13.",
            AgeRating.R => "Restricted: Under 17 requires accompanying parent or adult guardian.",
            AgeRating.NC17 => "Adults only: No one 17 and under admitted.",
            _ => "Unknown"
        };
    }

    public static int GetMinimumAge(this AgeRating ageRating)
    {
        return ageRating switch
        {
            AgeRating.G => 0,
            AgeRating.PG => 0,
            AgeRating.PG13 => 13,
            AgeRating.R => 17,
            AgeRating.NC17 => 17,
            _ => 0
        };
    }
}