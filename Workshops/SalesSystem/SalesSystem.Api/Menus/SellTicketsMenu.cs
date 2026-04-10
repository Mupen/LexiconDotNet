using SalesSystem.Api.Helpers;
using SalesSystem.Api.Interfaces;
using SalesSystem.Api.UI;
using SalesSystem.Application.Queries.Showings;
using SalesSystem.Application.ReadModels.Showings;
using SalesSystem.Application.Requests.TicketOrders;
using SalesSystem.Application.UseCases.TicketOrders;

namespace SalesSystem.Api.Menus;

public class SellTicketsMenu : MenuBase
{
    private readonly GetAvailableShowings _availableShowings;
    private readonly SellTickets _sellTickets;

    public SellTicketsMenu(
        IUserIO ui,
        GetAvailableShowings availableShowings,
        SellTickets sellTickets) : base(ui)
    {
        _availableShowings = availableShowings
            ?? throw new ArgumentNullException(nameof(availableShowings));
        _sellTickets = sellTickets
            ?? throw new ArgumentNullException(nameof(sellTickets));
    }

    public override async Task RunAsync()
    {
        var session = new TicketSalesSession();
        string? message = null;

        while (true)
        {
            var showings = (await _availableShowings.ExecuteAsync())
                .OrderBy(s => s.MovieNumber)
                .ThenBy(s => s.Date)
                .ThenBy(s => s.StartTime)
                .ToList();

            SyncSessionSelection(session, showings);

            var input = ShowMenu(showings, session, message);
            message = null;

            var normalizedInput = UserInput.Normalize(input);

            switch (normalizedInput)
            {
                case "C":
                    {
                        var confirmMessage = await ConfirmAndCompleteTransactionAsync(session);
                        if (confirmMessage == "__RETURN__")
                            return;

                        message = confirmMessage;
                        break;
                    }

                case "H":
                    ShowHelpScreen();
                    break;

                case "X":
                    ShowCancelledScreen();
                    return;

                default:
                    message = ProcessCommand(input, showings, session);
                    break;
            }
        }
    }

    private sealed class TicketSalesSession
    {
        public int? SelectedMovieNumber { get; set; }
        public ShowingListItem? SelectedShowing { get; set; }
        public List<TicketCartItem> Cart { get; } = [];
    }

    private sealed record TicketCartItem(
        Guid ShowingId,
        int MovieNumber,
        string MovieTitle,
        DateOnly Date,
        TimeOnly StartTime,
        int SeatNumber,
        TicketAgeType AgeType,
        decimal Price);

    private enum TicketAgeType
    {
        Adult,
        Child,
        Youth,
        Senior
    }

    private string ShowMenu(List<ShowingListItem> showings, TicketSalesSession session, string? message)
    {
        ShowHeader("Ticket Sales");

        ShowActions();
        ShowSelectedMovie(showings, session);
        ShowSelectedShowing(session);
        ShowAvailableShowings(showings, session);
        ShowAvailableSeats(session);
        ShowCart(session);

        if (!string.IsNullOrWhiteSpace(message))
        {
            ShowSubHeader("Message");
            ShowMessage(message);
        }

        return ShowPrompt(prompt: "> ");
    }

    private void ShowActions()
    {
        ShowSubHeader("Actions",
            "C = Confirm | X = Cancel | H = Help");
    }

    private void ShowSelectedMovie(List<ShowingListItem> showings, TicketSalesSession session)
    {
        ShowSubHeader("Selected Movie");

        if (!session.SelectedMovieNumber.HasValue)
        {
            ShowMessage("No movie selected.");
            return;
        }

        var movieShowing = showings
            .FirstOrDefault(x => x.MovieNumber == session.SelectedMovieNumber.Value);

        if (movieShowing is null)
        {
            ShowMessage("Selected movie is no longer available.");
            return;
        }

        ShowMessage(
            $"{movieShowing.MovieNumber} | {movieShowing.MovieTitle} | {movieShowing.AgeRating}");
    }

    private void ShowSelectedShowing(TicketSalesSession session)
    {
        ShowSubHeader("Selected Showing");

        if (session.SelectedShowing is null)
        {
            ShowMessage("No showing selected.");
            return;
        }

        ShowMessage(
            $"{session.SelectedShowing.MovieTitle} | {session.SelectedShowing.Date} | {session.SelectedShowing.StartTime} | Seats Available: {session.SelectedShowing.AvailableSeats}");
    }

    private void ShowAvailableShowings(List<ShowingListItem> showings, TicketSalesSession session)
    {
        ShowSubHeader("Available Showings");

        if (!session.SelectedMovieNumber.HasValue)
        {
            ShowMessage("Select a movie first with SM:num");
            return;
        }

        var movieShowings = showings
            .Where(x => x.MovieNumber == session.SelectedMovieNumber.Value)
            .OrderBy(x => x.Date)
            .ThenBy(x => x.StartTime)
            .ToList();

        if (movieShowings.Count == 0)
        {
            ShowMessage("No available showings for selected movie.");
            return;
        }

        int index = 1;
        foreach (var showing in movieShowings)
        {
            ShowMessage(
                $"{index}. {showing.Date} | {showing.StartTime} | Seats Available: {showing.AvailableSeats}");
            index++;
        }
    }

    private void ShowAvailableSeats(TicketSalesSession session)
    {
        ShowSubHeader("Available Seats");

        if (session.SelectedShowing is null)
        {
            ShowMessage("Select a showing first with SS:num");
            return;
        }

        var takenSeats = session.Cart
            .Where(x => x.ShowingId == session.SelectedShowing.ShowingId)
            .Select(x => x.SeatNumber)
            .ToHashSet();

        var visibleSeats = Enumerable.Range(1, 54)
            .Where(seat => !takenSeats.Contains(seat))
            .ToList();

        if (visibleSeats.Count == 0)
        {
            ShowMessage("No available seats.");
            return;
        }

        for (int row = 0; row < 6; row++)
        {
            var rowSeats = visibleSeats
                .Where(seat => seat >= row * 9 + 1 && seat <= row * 9 + 9)
                .ToList();

            var content = rowSeats.Count == 0
                ? "-"
                : string.Join(", ", rowSeats);

            ShowMessage($"Row {row + 1}: {content}");
        }
    }

    private void ShowCart(TicketSalesSession session)
    {
        ShowSubHeader("Ticket Cart");

        if (session.Cart.Count == 0)
        {
            ShowMessage("Cart is empty.");
            return;
        }

        foreach (var item in session.Cart)
        {
            ShowMessage(
                $"Movie: {item.MovieTitle} | Date: {item.Date} | Time: {item.StartTime} | Seat: {item.SeatNumber} | Age: {item.AgeType} | Price: {item.Price} SEK");
        }

        ShowMessage(string.Empty);
        ShowMessage($"Total: {session.Cart.Sum(x => x.Price)} SEK");
    }

    private string ProcessCommand(string input, List<ShowingListItem> showings, TicketSalesSession session)
    {
        if (string.IsNullOrWhiteSpace(input))
            return "No command entered.";

        var trimmed = input.Trim();

        if (UserInput.IsCommand(trimmed, "LM"))
        {
            ShowMovieListScreen(showings);
            return "Returned from movie list.";
        }

        if (UserInput.IsCommand(trimmed, "LS"))
        {
            if (!session.SelectedMovieNumber.HasValue)
                return "Select a movie first.";

            ShowShowingListScreen(showings, session.SelectedMovieNumber.Value);
            return "Returned from showing list.";
        }

        if (UserInput.IsCommand(trimmed, "RM"))
        {
            session.SelectedMovieNumber = null;
            session.SelectedShowing = null;
            session.Cart.Clear();
            return "Selected movie, showing, and cart cleared.";
        }

        if (UserInput.IsCommand(trimmed, "RS"))
        {
            session.SelectedShowing = null;
            session.Cart.Clear();
            return "Selected showing and cart cleared.";
        }

        if (TryParseNumberCommand(trimmed, "SM", out int movieNumber))
        {
            var movieExists = showings.Any(x => x.MovieNumber == movieNumber);

            if (!movieExists)
                return $"Movie {movieNumber} was not found.";

            session.SelectedMovieNumber = movieNumber;
            session.SelectedShowing = null;
            session.Cart.Clear();
            return $"Selected movie {movieNumber}.";
        }

        if (TryParseNumberCommand(trimmed, "SS", out int showingIndex))
        {
            if (!session.SelectedMovieNumber.HasValue)
                return "Select a movie first.";

            var movieShowings = showings
                .Where(x => x.MovieNumber == session.SelectedMovieNumber.Value)
                .OrderBy(x => x.Date)
                .ThenBy(x => x.StartTime)
                .ToList();

            if (showingIndex <= 0 || showingIndex > movieShowings.Count)
                return "Invalid showing number.";

            session.SelectedShowing = movieShowings[showingIndex - 1];
            session.Cart.Clear();
            return $"Selected showing {showingIndex}.";
        }

        if (TryParseNumberCommand(trimmed, "AAT", out int adultSeat))
            return AddTicket(session, adultSeat, TicketAgeType.Adult, "AAT");

        if (TryParseNumberCommand(trimmed, "ACT", out int childSeat))
            return AddTicket(session, childSeat, TicketAgeType.Child, "ACT");

        if (TryParseNumberCommand(trimmed, "AYT", out int youthSeat))
            return AddTicket(session, youthSeat, TicketAgeType.Youth, "AYT");

        if (TryParseNumberCommand(trimmed, "AST", out int seniorSeat))
            return AddTicket(session, seniorSeat, TicketAgeType.Senior, "AST");

        if (TryParseNumberCommand(trimmed, "RT", out int removeSeat))
            return RemoveTicket(session, removeSeat);

        return "Unknown command. Press H for help.";
    }

    private string AddTicket(TicketSalesSession session, int seatNumber, TicketAgeType ageType, string command)
    {
        if (session.SelectedMovieNumber is null)
            return "Select a movie first.";

        if (session.SelectedShowing is null)
            return "Select a showing first.";

        if (seatNumber < 1 || seatNumber > 54)
            return "Seat number must be between 1 and 54.";

        bool seatAlreadyUsed = session.Cart.Any(x =>
            x.ShowingId == session.SelectedShowing.ShowingId &&
            x.SeatNumber == seatNumber);

        if (seatAlreadyUsed)
            return $"Seat {seatNumber} is already in cart.";

        decimal price = CalculateDraftPrice(session.SelectedShowing, ageType);

        session.Cart.Add(new TicketCartItem(
            session.SelectedShowing.ShowingId,
            session.SelectedShowing.MovieNumber,
            session.SelectedShowing.MovieTitle,
            session.SelectedShowing.Date,
            session.SelectedShowing.StartTime,
            seatNumber,
            ageType,
            price));


        return $"Added {ageType} ticket for seat {seatNumber}.";
    }

    private string RemoveTicket(TicketSalesSession session, int seatNumber)
    {
        if (session.SelectedShowing is null)
            return "Select a showing first.";

        var item = session.Cart.FirstOrDefault(x =>
            x.ShowingId == session.SelectedShowing.ShowingId &&
            x.SeatNumber == seatNumber);

        if (item is null)
            return $"Seat {seatNumber} is not in cart.";

        session.Cart.Remove(item);

        return $"Removed ticket for seat {seatNumber}.";
    }

    private async Task<string> ConfirmAndCompleteTransactionAsync(TicketSalesSession session)
    {
        if (session.SelectedShowing is null)
            return "Select a showing first.";

        if (session.Cart.Count == 0)
            return "Cart is empty.";

        bool confirmed = ShowConfirmScreen(session);

        if (!confirmed)
            return "Confirmation cancelled.";

        var selections = session.Cart
            .Where(x => x.ShowingId == session.SelectedShowing.ShowingId)
            .Select(x => MapToSelectionRequest(x))
            .ToList();

        var request = new SellTicketsRequest(
            session.SelectedShowing.ShowingId,
            selections,
            null);

        var result = await _sellTickets.ExecuteAsync(request);

        if (result.IsFailure)
            return result.Error.Message;

        ShowCompletedScreen(result.Value.TotalAmount);

        session.SelectedShowing = null;
        session.Cart.Clear();

        return "__RETURN__";
    }

    private static TicketSelectionRequest MapToSelectionRequest(TicketCartItem item)
    {
        return item.AgeType switch
        {
            TicketAgeType.Adult => new TicketSelectionRequest(
                item.SeatNumber,
                30,
                true,
                false),

            TicketAgeType.Child => new TicketSelectionRequest(
                item.SeatNumber,
                5,
                true,
                true),

            TicketAgeType.Youth => new TicketSelectionRequest(
                item.SeatNumber,
                10,
                true,
                false),

            TicketAgeType.Senior => new TicketSelectionRequest(
                item.SeatNumber,
                70,
                true,
                false),

            _ => throw new InvalidOperationException("Unsupported ticket type.")
        };
    }

    private static decimal CalculateDraftPrice(ShowingListItem showing, TicketAgeType ageType)
    {
        decimal basePrice = showing.StartTime == new TimeOnly(13, 0)
            ? 105m
            : 130m;

        return ageType switch
        {
            TicketAgeType.Adult => basePrice,
            TicketAgeType.Child => 0m,
            TicketAgeType.Youth => 65m,
            TicketAgeType.Senior => 90m,
            _ => basePrice
        };
    }

    private static bool TryParseNumberCommand(string input, string command, out int number)
    {
        number = 0;

        var prefix = command + ":";

        if (!input.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            return false;

        var numberPart = input[prefix.Length..].Trim();

        return int.TryParse(numberPart, out number);
    }

    private static void SyncSessionSelection(TicketSalesSession session, List<ShowingListItem> showings)
    {
        if (session.SelectedMovieNumber.HasValue &&
            !showings.Any(x => x.MovieNumber == session.SelectedMovieNumber.Value))
        {
            session.SelectedMovieNumber = null;
            session.SelectedShowing = null;
            session.Cart.Clear();
        }

        if (session.SelectedShowing is not null)
        {
            var updatedShowing = showings.FirstOrDefault(x => x.ShowingId == session.SelectedShowing.ShowingId);

            if (updatedShowing is null)
            {
                session.SelectedShowing = null;
                session.Cart.Clear();
            }
            else
            {
                session.SelectedShowing = updatedShowing;
            }
        }
    }

    private bool ShowConfirmScreen(TicketSalesSession session)
    {
        while (true)
        {
            _ui.Clear();
            _ui.WriteLine("========== Confirm Ticket Transaction ==========");
            _ui.WriteLine();

            foreach (var item in session.Cart)
            {
                _ui.WriteLine(
                    $"Movie: {item.MovieTitle} | Date: {item.Date} | Time: {item.StartTime} | Seat: {item.SeatNumber} | Age: {item.AgeType} | Price: {item.Price} SEK");
            }

            _ui.WriteLine();
            _ui.WriteLine($"Total: {session.Cart.Sum(x => x.Price)} SEK");
            _ui.WriteLine();
            _ui.WriteLine("C = Confirm the transaction");
            _ui.WriteLine("B = Back to ticket sales");
            _ui.WriteLine("X = Cancel transaction");
            _ui.WriteLine();
            _ui.Write("> ");

            var input = _ui.ReadLine()?.Trim().ToUpperInvariant();

            if (string.IsNullOrWhiteSpace(input))
                continue;

            if (input == "C")
                return true;

            if (input == "B" || input == "X")
                return false;
        }
    }

    private void ShowMovieListScreen(List<ShowingListItem> showings)
    {
        _ui.Clear();
        _ui.WriteLine("========== Available Movie List ==========");
        _ui.WriteLine();

        var movies = showings
            .GroupBy(x => new
            {
                x.MovieNumber,
                x.MovieTitle,
                x.YearReleased,
                x.AgeRating,
                x.Duration,
                x.Description
            })
            .OrderBy(x => x.Key.MovieNumber)
            .ToList();

        if (movies.Count == 0)
        {
            _ui.WriteLine("No movies available.");
        }
        else
        {
            foreach (var movie in movies)
            {
                _ui.WriteLine(
                    $"MovieNumber: {movie.Key.MovieNumber} | Title: {movie.Key.MovieTitle}");

                _ui.WriteLine(
                    $"Details: Year: {movie.Key.YearReleased} | Age Rating: {movie.Key.AgeRating} | Duration: {movie.Key.Duration:hh\\:mm}");

                _ui.WriteLine(
                    $"Description: {movie.Key.Description}");

                _ui.WriteLine();
            }
        }

        _ui.WriteLine("Press any key to return...");
        _ui.WriteLine();
        _ui.Write("> ");
        _ui.WaitForKey();
    }

    private void ShowShowingListScreen(List<ShowingListItem> showings, int movieNumber)
    {
        _ui.Clear();
        _ui.WriteLine("========== Showing List ==========");
        _ui.WriteLine();

        var movieShowings = showings
            .Where(x => x.MovieNumber == movieNumber)
            .OrderBy(x => x.Date)
            .ThenBy(x => x.StartTime)
            .ToList();

        if (movieShowings.Count == 0)
        {
            _ui.WriteLine("No showings available.");
        }
        else
        {
            int index = 1;
            foreach (var showing in movieShowings)
            {
                _ui.WriteLine(
                    $"{index} | {showing.Date} | {showing.StartTime} | Seats Available: {showing.AvailableSeats}");
                index++;
            }
        }

        _ui.WriteLine();
        _ui.WriteLine("Press any key to return...");
        _ui.WriteLine();
        _ui.Write("> ");
        _ui.WaitForKey();
    }

    private void ShowHelpScreen()
    {
        _ui.Clear();
        _ui.WriteLine("========== Ticket Sales Help ==========");
        _ui.WriteLine();
        _ui.WriteLine("LM               Show movie list");
        _ui.WriteLine("SM:num           Select movie");
        _ui.WriteLine("RM               Remove selected movie");
        _ui.WriteLine("LS               Show selected movie showings");
        _ui.WriteLine("SS:num           Select showing");
        _ui.WriteLine("RS               Remove selected showing");
        _ui.WriteLine("AAT:num          Add adult ticket");
        _ui.WriteLine("ACT:num          Add child ticket");
        _ui.WriteLine("AYT:num          Add youth ticket");
        _ui.WriteLine("AST:num          Add senior ticket");
        _ui.WriteLine("RT:num           Remove ticket");
        _ui.WriteLine("C                Confirm transaction");
        _ui.WriteLine("X                Cancel transaction");
        _ui.WriteLine("H                Help");
        _ui.WriteLine();
        _ui.WriteLine("Examples:");
        _ui.WriteLine("SM:1");
        _ui.WriteLine("SS:2");
        _ui.WriteLine("AAT:14");
        _ui.WriteLine("ACT:15");
        _ui.WriteLine("RT:14");
        _ui.WriteLine();
        _ui.WriteLine("Press any key to continue...");
        _ui.WriteLine();
        _ui.Write("> ");
        _ui.WaitForKey();
    }

    private void ShowCancelledScreen()
    {
        _ui.Clear();
        _ui.WriteLine("========== Cancelled Ticket Transaction ==========");
        _ui.WriteLine();
        _ui.WriteLine("Transaction cancelled.");
        _ui.WriteLine();
        _ui.WriteLine("Press any key to return to main menu...");
        _ui.WriteLine();
        _ui.Write("> ");
        _ui.WaitForKey();
    }

    private void ShowCompletedScreen(decimal total)
    {
        _ui.Clear();
        _ui.WriteLine("========== Ticket Transaction Completed ==========");
        _ui.WriteLine();
        _ui.WriteLine($"Transaction completed successfully. Total: {total} SEK");
        _ui.WriteLine();
        _ui.WriteLine("Press any key to return to main menu...");
        _ui.WriteLine();
        _ui.Write("> ");
        _ui.WaitForKey();
    }
}