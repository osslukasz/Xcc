using kolokwium.DTO;
using kolokwium.Exceptions;
using Microsoft.Data.SqlClient;

namespace kolokwium.Services;

public class DbService :IDbService
{
    private readonly string _connectionString;

    public DbService(IConfiguration config)
    {
        _connectionString = config.GetConnectionString("DefaultConnection") ?? string.Empty;
    }
    public async Task<GetMakersDto> GetMakersAsync(string? name)
    {
        var query = """
                    SELECT 
                    m.Id as MakerId,
                    m.Name as MakerName,
                    p.Id as ProductId,
                    p.Name as ProductName,
                    p.Description as ProductDescription,
                    p.StickerPrice as StickerPrice,
                    pt.Id as TypeId,
                    pt.Name as TypeName,
                    V.Code as VendorCode,
                    V.Name as VendorName,
                    vp.amount as Amount,
                    vp.PricePerUnit as PricePerUnit
                    FROM Makers m JOIN Products p on m.Id = p.MakerId
                    JOIN ProductTypes pt ON p.ProductTypeId = pt.Id
                    JOIN VendorProducts vp on vp.ProductId = p.Id
                    JOIN Vendors v ON vp.VendorCode = v.Code
                    WHERE @Name IS NULL OR m.Name = @Name
                    """;

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        
        await using var command = new SqlCommand();
        command.Connection = connection;
        command.CommandText = query;
        
        command.Parameters.AddWithValue("@Name", string.IsNullOrEmpty(name) ? DBNull.Value : name);
        await using var reader = await command.ExecuteReaderAsync();

        GetMakersDto? result = null;
        
        var ordMakerId = reader.GetOrdinal("MakerId");
        var ordMakerName = reader.GetOrdinal("MakerName");
        var ordProductId = reader.GetOrdinal("ProductId");
        var ordProductName = reader.GetOrdinal("ProductName");
        var ordProductDescription = reader.GetOrdinal("ProductDescription");
        var ordStickerPrice = reader.GetOrdinal("StickerPrice");
        var ordTypeId = reader.GetOrdinal("TypeId");
        var ordTypeName = reader.GetOrdinal("TypeName");
        var ordVendorCode = reader.GetOrdinal("VendorCode");
        var ordVendorName = reader.GetOrdinal("VendorName");
        var ordAmount = reader.GetOrdinal("Amount");
        var ordPricePerUnit = reader.GetOrdinal("PricePerUnit");
        
        

        while (await reader.ReadAsync())
        {
	        if (result is null)
	        {
		        result = new GetMakersDto()
		        {
			        id = reader.GetInt32(ordMakerId),
			        name = reader.GetString(ordMakerName),
			        products = new List<GetProductsDto>()
		        };
	        }

	        var prodcutId = reader.GetInt32(ordProductId);

	        var product = result.products.FirstOrDefault(e => e.id.Equals(prodcutId));

	        if (product is null)
	        {
		        product = new GetProductsDto()
		        {
			        id = prodcutId,
					name = reader.GetString(ordProductName),
					description = reader.GetString(ordProductDescription),
					stickerPrice = reader.GetDecimal(ordStickerPrice),
					productType = new GetProductTypeDto()
					{
						id = reader.GetInt32(ordTypeId),
						name = reader.GetString(ordTypeName),
					},
					vendors = new List<GetVendorsDto>()
		        };
		        result.products.Add(product);
	        }

	        product.vendors.Add(new GetVendorsDto()
	        {
		        code = reader.GetString(ordVendorCode),
		        name = reader.GetString(ordVendorName),
		        amount = reader.GetInt32(ordAmount),
		        pricePerUnit = reader.GetDecimal(ordPricePerUnit)
	        });
        }

        return result ?? throw new NotFoundException("No rentals found for the specified customer.");
    }

    public async Task CreateMakerWithProdcutsAsync(CreateMakerWithProductsDto dto)
    {
	    var createMakerQuery = """
	                           INSERT INTO Makers
	                           VALUES(@Name)
	                           SELECT @@IDENTITY;
	                           """;

	    var createProductQuery = """
	                             INSERT INTO Products
	                             VALUES(@ProductName, @Description,@StickerPrice,@ProductTypeId,@MakerId);
	                             """;

	    var getProductTypeQuery = """
	                              SELECT Id
	                              FROM ProductTypes
	                              WHERE Name = @ProductTypeName;
	                              """;

	    var checkMakerQuery = """
	                          SELECT 1 
	                          FROM Makers 
	                          WHERE id = @MakerId;
	                          """;

	    await using var connection = new SqlConnection(_connectionString);
	    await connection.OpenAsync();

	    await using var transaction = await connection.BeginTransactionAsync();

	    await using var command = new SqlCommand();
	    command.Connection = connection;
	    command.Transaction = transaction as SqlTransaction;

	    try
	    {
		    command.Parameters.Clear();
		    command.CommandText = createMakerQuery;
		    command.Parameters.AddWithValue("@Name", dto.Name);

		    var productObject = await command.ExecuteScalarAsync();
		    var MakerId = Convert.ToInt32(productObject);

		    foreach (var product in dto.Products)
		    {
			    command.Parameters.Clear();
			    command.CommandText = getProductTypeQuery;
			    command.Parameters.AddWithValue("@ProductTypeName", product.Type);

			    var ProductTypeObject = await command.ExecuteScalarAsync();
			    if (ProductTypeObject == null)
			    {
				    throw new NotFoundException($"Type - {product.Type} - not found.");
			    }

			    var TypeId = Convert.ToInt32(ProductTypeObject);

			    command.Parameters.Clear();
			    command.CommandText = createProductQuery;
			    command.Parameters.AddWithValue("@ProductName", product.Name);
			    command.Parameters.AddWithValue("@Description", product.Description);
			    command.Parameters.AddWithValue("@StickerPrice", product.stickerPrice);
			    command.Parameters.AddWithValue("@ProductTypeId", TypeId);
			    command.Parameters.AddWithValue("@MakerId", MakerId);


			    await command.ExecuteNonQueryAsync();
		    }

		    await transaction.CommitAsync();
	    }
	    catch (Exception e)
	    {
		    await transaction.RollbackAsync();
		    throw;
	    }
    }
}