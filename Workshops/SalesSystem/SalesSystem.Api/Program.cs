using SalesSystem.Api.IO;
using SalesSystem.Api.Menus;
using SalesSystem.Application.Interfaces;
using SalesSystem.Application.Queries.Accounting;
using SalesSystem.Application.Queries.Movies;
using SalesSystem.Application.Queries.Products;
using SalesSystem.Application.Queries.Showings;
using SalesSystem.Application.Queries.TicketOrders;
using SalesSystem.Application.UseCases.Accounting;
using SalesSystem.Application.UseCases.Movies;
using SalesSystem.Application.UseCases.Products;
using SalesSystem.Application.UseCases.Showings;
using SalesSystem.Application.UseCases.TicketOrders;
using SalesSystem.Infrastructure.Repositories;
using SalesSystem.Infrastructure.Services;

namespace SalesSystem.Api;

public class Program
{
    public static async Task Main()
    {
        // Initialize repositories
        IProductRepository productRepository = new InMemoryProductRepository();
        IMovieRepository movieRepository = new InMemoryMovieRepository();
        IShowingRepository showingRepository = new InMemoryShowingRepository();
        ITicketOrderRepository ticketOrderRepository = new InMemoryTicketOrderRepository();


        // Initialize user interface    
        var ui = new ConsoleUserIO();

        // Initialize services
        var ticketPricingService = new TicketPricingService();

        // Initialize Queries for Products
        var getAllProducts = new GetAllProducts(productRepository);
        var getAvailableProducts = new GetAvailableProducts(productRepository);
        var getProductByNumber = new GetProductByNumber(productRepository);

        // Initialize UseCases for Products
        var createProduct = new CreateProduct(productRepository);
        var updateProduct = new UpdateProduct(productRepository);
        var deleteProduct = new DeleteProduct(productRepository);
        var increaseProductStock = new IncreaseProductStock(productRepository);
        var decreaseProductStock = new DecreaseProductStock(productRepository);
        var changeProductStatus = new ChangeProductStatus(productRepository);

        // Initialize Queries for Movies
        var getAllMovies = new GetAllMovies(movieRepository);
        var getActiveMovies = new GetActiveMovies(movieRepository);
        var getMovieByNumber = new GetMovieByNumber(movieRepository);
        var getMovieById = new GetMovieById(movieRepository);

        // Initialize UseCases for Movies
        var createMovie = new CreateMovie(movieRepository);
        var updateMovie = new UpdateMovie(movieRepository);
        var changeMovieStatus = new ChangeMovieStatus(movieRepository);
        var deleteMovie = new DeleteMovie(movieRepository);

        // Initialize Queries for Showings
        var getAllShowings = new GetAllShowings(movieRepository, showingRepository);
        var getAvailableShowings = new GetAvailableShowings(movieRepository, showingRepository);
        var getShowingsByMovie = new GetShowingsByMovie(movieRepository, showingRepository);
        var getShowingsByDate = new GetShowingsByDate(movieRepository, showingRepository);
        var getShowingById = new GetShowingById(showingRepository);

        // Initialize use cases for Showings
        var createShowing = new CreateShowing(showingRepository);
        var updateShowing = new UpdateShowing(showingRepository);
        var cancelShowing = new CancelShowing(showingRepository);
        var restoreShowing = new RestoreShowing(showingRepository);

        // Initialize Queries for TicketOrders
        // var getAllTicketOrders = new GetAllTicketOrders(); // The file GetAllTicketOrders.cs is empty or unfinished
        // var getActiveTicketOrders = new GetActiveTicketOrders(); // The file GetActiveTicketOrders.cs is empty or unfinished
        // var getTicketOrderById = new GetTicketOrderById(); // The file GetTicketOrderById.cs is empty or unfinished

        // Initialize UseCases for TicketOrders
        var sellTickets = new SellTickets(movieRepository, showingRepository, ticketOrderRepository, ticketPricingService);
        var completeTicketOrder = new CompleteTicketOrder(ticketOrderRepository);
        var cancelTicketOrder = new CancelTicketOrder(ticketOrderRepository);
        var updateTicketOrder = new UpdateTicketOrder(movieRepository, showingRepository, ticketOrderRepository, ticketPricingService);

        // Initialize Queries for Accounting
        // var getAccountingSummary = new GetAccountingSummary(); // The file GetAccountingSummary.cs is empty or unfinished

        // Initialize UseCases for Accounting
        // var registerProductSale = new RegisterProductSale(); // The file RegisterProductSale.cs is empty or unfinished
        // var registerTicketSale = new RegisterTicketSale(); // The file RegisterTicketSale.cs is empty or unfinished


        // Initialize menus
        var sellTicketsMenu = new SellTicketsMenu(ui, getAvailableShowings, sellTickets);
        var sellProductsMenu = new SellProductsMenu(ui, getAvailableProducts, decreaseProductStock);
        var manageMoviesMenu = new ManageMoviesMenu(ui);
        var manageTicketsMenu = new ManageTicketsMenu(ui);
        var manageProductsMenu = new ManageProductsMenu(ui, getAllProducts, getProductByNumber, createProduct, updateProduct, deleteProduct, increaseProductStock, decreaseProductStock, changeProductStatus);
        var manageAccountingMenu = new ManageAccountingMenu(ui);

        //  Seed initial data
        await SeedProductsAsync(createProduct);
        await SeedMoviesAsync(createMovie);
        await SeedShowingsAsync(createShowing, getAllMovies);

        // Initialize main menu
        var mainMenu = new MainMenu(
            ui,
            sellTicketsMenu,
            sellProductsMenu,
            manageMoviesMenu,
            manageTicketsMenu,
            manageProductsMenu,
            manageAccountingMenu);

        // Run the main menu
        await mainMenu.RunAsync();
    }

    private static async Task SeedProductsAsync(CreateProduct createProduct)
    {
        await createProduct.ExecuteAsync(new(1, "Ahlgrens bilar", 22m, 0.12m, 20, true));
        await createProduct.ExecuteAsync(new(2, "Popcorn + Coca-Cola", 43m, 0.12m, 15, true));
    }

    private static async Task SeedMoviesAsync(CreateMovie createMovie)
    {
        await createMovie.ExecuteAsync(new(
            MovieNumber: 1,
            Title: "Furiosa: A Mad Max Saga",
            Description: "After being snatched from the Green Place, young Furiosa must survive and find her way home amid power struggles.",
            YearReleased: 2024,
            AgeRating: SalesSystem.Domain.Enums.AgeRating.R,
            Duration: new TimeSpan(2, 28, 0),
            IsActive: true));

        await createMovie.ExecuteAsync(new(
            MovieNumber: 2,
            Title: "Dog Day Afternoon",
            Description: "A bank robbery spirals into chaos as everything goes wrong.",
            YearReleased: 1975,
            AgeRating: SalesSystem.Domain.Enums.AgeRating.R,
            Duration: new TimeSpan(2, 5, 0),
            IsActive: true));

        await createMovie.ExecuteAsync(new(
            MovieNumber: 3,
            Title: "The Fall Guy",
            Description: "A stuntman investigates a conspiracy while trying to rebuild his life.",
            YearReleased: 2024,
            AgeRating: SalesSystem.Domain.Enums.AgeRating.PG13,
            Duration: new TimeSpan(2, 6, 0),
            IsActive: true));

        await createMovie.ExecuteAsync(new(
            MovieNumber: 4,
            Title: "Iron Man 3",
            Description: "Tony Stark faces a powerful terrorist and must rebuild himself.",
            YearReleased: 2013,
            AgeRating: SalesSystem.Domain.Enums.AgeRating.PG13,
            Duration: new TimeSpan(2, 10, 0),
            IsActive: true));

        await createMovie.ExecuteAsync(new(
            MovieNumber: 5,
            Title: "Civil War",
            Description: "Journalists travel across a war-torn United States.",
            YearReleased: 2024,
            AgeRating: SalesSystem.Domain.Enums.AgeRating.R,
            Duration: new TimeSpan(1, 49, 0),
            IsActive: true));

        await createMovie.ExecuteAsync(new(
            MovieNumber: 6,
            Title: "The Room Next Door",
            Description: "Two old friends reconnect under unusual circumstances.",
            YearReleased: 2024,
            AgeRating: SalesSystem.Domain.Enums.AgeRating.PG13,
            Duration: new TimeSpan(1, 47, 0),
            IsActive: true));
    }

    private static async Task SeedShowingsAsync(CreateShowing createShowing, GetAllMovies getAllMovies)
    {
        var movies = await getAllMovies.ExecuteAsync();

        var moviesByNumber = movies.ToDictionary(m => m.MovieNumber);

        // Use any week you want. Here is one simple demo week.
        var tuesday = new DateOnly(2026, 5, 5);
        var wednesday = new DateOnly(2026, 5, 6);
        var thursday = new DateOnly(2026, 5, 7);
        var friday = new DateOnly(2026, 5, 8);
        var saturday = new DateOnly(2026, 5, 9);

        await createShowing.ExecuteAsync(new(
            moviesByNumber[1].Id,
            tuesday,
            new TimeOnly(18, 0),
            54));

        await createShowing.ExecuteAsync(new(
            moviesByNumber[1].Id,
            saturday,
            new TimeOnly(21, 0),
            54));

        await createShowing.ExecuteAsync(new(
            moviesByNumber[2].Id,
            thursday,
            new TimeOnly(21, 0),
            54));

        await createShowing.ExecuteAsync(new(
            moviesByNumber[3].Id,
            tuesday,
            new TimeOnly(21, 0),
            54));

        await createShowing.ExecuteAsync(new(
            moviesByNumber[3].Id,
            friday,
            new TimeOnly(21, 0),
            54));

        await createShowing.ExecuteAsync(new(
            moviesByNumber[4].Id,
            wednesday,
            new TimeOnly(18, 0),
            54));

        await createShowing.ExecuteAsync(new(
            moviesByNumber[4].Id,
            saturday,
            new TimeOnly(13, 0),
            54));

        await createShowing.ExecuteAsync(new(
            moviesByNumber[5].Id,
            friday,
            new TimeOnly(18, 0),
            54));

        await createShowing.ExecuteAsync(new(
            moviesByNumber[5].Id,
            saturday,
            new TimeOnly(18, 0),
            54));

        await createShowing.ExecuteAsync(new(
            moviesByNumber[6].Id,
            wednesday,
            new TimeOnly(21, 0),
            54));

        await createShowing.ExecuteAsync(new(
            moviesByNumber[6].Id,
            thursday,
            new TimeOnly(18, 0),
            54));
    }
}
