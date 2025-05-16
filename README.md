# APBD_4 - Task 9
This is the C# Web API project designed to manage application for a company that manages
warehouse.
It follows a clean, layered architecture and integrates SQL Server using
(`SqlConnection`, `SqlCommand`, `SqlTransaction`) to perform database operations, including stored procedures.

It's coded way better than the previous project: [APBD_3 – Task 8 REST API](https://github.com/x0-lf/APBD_3)

## Each layer has a clearly defined responsibility: 
- The `controller` exposes minimal logic and delegates cleanly to the `service` layer, which encapsulates all business rules and error handling.
- The `repository` layer handles direct database communication, always using parameterized queries with `SqlConnection, `SqlCommand`, and `SqlTransaction`.
- Every method operates with `async` & `await`, returns clean `DTOs`, and `commits` or `rolls back` transactions predictably.
## Endpoints available:

### `POST /api/warehouse`

Calls The WarehouseService async Task `AddProductToWarehouseAsync()`

- Validates that `Amount > 0`
- Checks if `Product` and `Warehouse` exist
- Finds a matching unfulfilled `Order` (same `IdProduct`, `Amount`, `CreatedAt < request.CreatedAt`)
- Ensures the order is not already fulfilled (in `Product_Warehouse`)
- Updates the order's `FulfilledAt` date
- Inserts a row into `Product_Warehouse` with calculated `Price = Amount × Product.Price`
- If successful, commits the database transaction, Returns `200 OK` and the new `IdProductWarehouse`
- If not successful, return those HTTP status codes below

**HTTP status codes:**:

| Code | Message                              |
|------|--------------------------------------|
| `200 OK` | Success                              |
| `400 Bad Request` | Invalid amount                       |
| `404 Not Found` | Missing product, warehouse, or order |
| `409 Conflict` | Order already fulfilled              |

---

### `POST /api/warehouse/proc`

Calls the SQL stored procedure `AddProductToWarehouse`.

The logic inside the procedure mirrors the manual implementation and:
- Returns `@@IDENTITY()` if successful
- Uses `RAISERROR` with severity 16 to signal business logic violations
- `RAISERROR('Invalid parameter: There is no order to fullfill', 16, 1)` is mapped to `409 Conflict`
- Other RAISERROR such as (invalid product, warehouse) is mapped to `404 Not Found`

**HTTP status codes**:

| Code | Message                                     |
|------|---------------------------------------------|
| `200 OK` | Success                                     |
| `400 Bad Request` | Generic procedure failure                   |
| `404 Not Found` | Missing product/warehouse                   |
| `409 Conflict` | Duplicate attempt to fulfill the same order |


## Testing the Endpoints

You can test this API endpoints using [APBD_4.http](./APBD_4.http) via Visual Studio's / JetBrains Rider built-in HTTP client or tools like Postman.

### POST request for the `AddProductToWarehouseAsync()`:

```http
POST /api/warehouse
Content-Type: application/json

{
  "IdProduct": 1,
  "IdWarehouse": 1,
  "Amount": 125,
  "CreatedAt": "2025-12-31T12:00:00Z"
}
```
### POST request for the stored procedure:
```http
POST /api/warehouse/proc
Content-Type: application/json

{
"IdProduct": 1,
"IdWarehouse": 1,
"Amount": 125,
"CreatedAt": "2025-12-31T12:00:00Z"
}
```
