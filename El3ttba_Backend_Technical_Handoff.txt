# El3ttba Backend Technical Handoff

## 1. Project Overview

`El3ttba` is an ASP.NET Core Web API backend for a wholesale marketplace platform. The system supports buyers, sellers, store managers, stores, products, product images, offers, checkout, orders, reviews, shipping data, messages, authentication, authorization, and image-based product search.

The backend is built with .NET 10, Entity Framework Core, SQL Server LocalDB/SQL Server, ASP.NET Core Identity, JWT authentication, FluentValidation, Swagger, and a repository/unit-of-work data access style.

The local API base URL is:

```text
http://localhost:5191
```

Swagger UI:

```text
http://localhost:5191/swagger
```

## 2. Solution Structure

The solution file is:

```text
El3ttba.slnx
```

Projects:

```text
Elattaba.API
Elattba.Application
Elattba.Core
Elattba.InfraStructure
Elattba.Tests
```

### Elattaba.API

The API layer contains:

- Controllers
- Middleware setup
- Dependency injection setup
- Authentication and authorization configuration
- CORS configuration
- Rate limiting configuration
- Security headers
- Swagger setup
- Exception handlers
- Background service registration
- ASP.NET Identity integration

Important files:

```text
Elattaba.API/Program.cs
Elattaba.API/Extensions/ApiServiceExtensions.cs
Elattaba.API/Extensions/ApiApplicationExtensions.cs
Elattaba.API/Extensions/AuthServiceExtensions.cs
Elattaba.API/Extensions/CorsExtensions.cs
Elattaba.API/Extensions/RateLimitExtensions.cs
Elattaba.API/Extensions/SecurityHeadersExtensions.cs
```

### Elattba.Application

The application layer contains business logic and service classes. Controllers call services in this layer. Services return a shared `ServiceResult` response type.

Important areas:

```text
Products
ProductImages
Offers
Orders
Checkouts
Reviews
Stores
Users
Messages
Categories
Carriers
Governorates
ShippingRates
PricingTiers
Validation
Auth abstractions
```

### Elattba.Core

The core/domain layer contains:

- Entities
- DTOs
- Enums
- Service interfaces
- Repository interfaces

Important folders:

```text
Elattba.Core/Entities
Elattba.Core/DTOs
Elattba.Core/Enums
Elattba.Core/InterFaces
Elattba.Core/Services
```

### Elattba.InfraStructure

The infrastructure layer contains:

- EF Core DbContexts
- EF Core migrations
- Repository implementations
- UnitOfWork implementation
- ASP.NET Identity user and Identity DbContext
- Image upload management
- ONNX image embedding service

Important files:

```text
Elattba.InfraStructure/Data/El3atbaDbContext.cs
Elattba.InfraStructure/Identity/AppIdentityDbContext.cs
Elattba.InfraStructure/Identity/AppUser.cs
Elattba.InfraStructure/Repository/UnitOfWork.cs
Elattba.InfraStructure/InfraStructureRegisteration.cs
Elattba.InfraStructure/Services/ImageManagementService.cs
Elattba.InfraStructure/Services/OnnxImageEmbeddingService.cs
```

### Elattba.Tests

The test project contains unit tests for business rules and application services.

Covered areas include:

- Checkout service
- Order service
- Offer service
- Product query service
- Product image service
- Review service
- Validators

Last known successful result:

```text
39 passed, 0 failed
```

## 3. Architecture

The project follows a layered architecture:

```text
HTTP Request
   -> Controller
   -> Application Service
   -> UnitOfWork / Repository
   -> EF Core DbContext
   -> SQL Server
```

The response flow is:

```text
SQL Server / EF Core
   -> Repository
   -> Application Service
   -> ServiceResult<T>
   -> ControllerResultExtensions
   -> ResponseAPI / IActionResult
```

The API layer should stay thin. Most business decisions should live in the application layer, not in controllers.

The infrastructure layer hides database and file/model operations behind interfaces.

## 4. Domain Model

### User

`User` is the platform/domain user.

It stores:

- Email
- PasswordHash
- Phone
- Role
- Governorate
- City
- ShippingAddress
- CreatedAt

Relationships:

- Owns one store
- Can manage stores
- Has orders
- Has reviews
- Sends and receives messages

### AppUser

`AppUser` is the ASP.NET Identity user.

It stores:

- Identity login data
- FirstName
- LastName
- DomainUserId
- Optional StoreId

Important: The system has both a domain `User` and an Identity `AppUser`. They must stay synchronized through `DomainUserId`.

### Store

Represents a seller storefront.

Fields:

- OwnerId
- Optional ManagerId
- CategoryId
- StoreName
- Location
- Description
- Rating
- CreatedAt

Relationships:

- Products
- Offers
- Orders
- Reviews

Current design allows one optional manager per store.

### Product

Represents a listed item.

Fields:

- StoreId
- CategoryId
- Name
- Description
- BasePrice
- StockQuantity
- HasOffer
- RowVersion
- CreatedAt

Relationships:

- Images
- PricingTiers
- OfferProducts
- OrderItems
- Messages

`RowVersion` is used for optimistic concurrency, especially around stock updates.

### ProductImage

Represents product image data.

Key concepts:

- ImageUrl
- IsPrimary
- EmbeddingVector for image search

### Offer

Represents a promotional discount.

Can apply to:

- All products in a store
- Specific products only

Fields:

- StoreId
- DiscountPercentage
- StartDate
- EndDate
- AppliesToAllProducts

Rules:

- Discount must be greater than 0 and at most 100.
- StartDate must be before EndDate.
- Overlapping offers are prevented.

### Order

Represents a buyer order for one store.

Fields:

- BuyerId
- StoreId
- Optional CarrierId
- OrderDate
- TotalAmount
- ShippingCost
- ShippingAddressSnapshot
- Optional TrackingNumber
- PaymentMethod
- PaymentStatus
- OrderStatus
- CreatedAt

Relationships:

- OrderItems
- Review

Important: A normal order belongs to one store. Checkout can split mixed-store carts into multiple orders.

### OrderItem

Represents a product inside an order.

Contains:

- ProductId
- Quantity
- UnitPrice
- Subtotal

The unit price is captured at order time after active offers are applied.

### Review

Represents buyer feedback.

Business intent:

- Reviews are purchase-gated.
- A buyer should only review a delivered/purchased order.
- Duplicate reviews for the same order are prevented.

### Other Entities

The system also contains:

- Category
- Carrier
- Governorate
- ShippingRate
- PricingTier
- Message
- OfferProduct

## 5. Roles and Authorization

Current roles:

```text
Buyer
Seller
Admin
StoreManager
```

Authorization policies:

```text
AdminOnly
SellerOnly
BuyerOnly
```

Policy behavior:

- `AdminOnly` requires Admin.
- `SellerOnly` allows Seller and StoreManager.
- `BuyerOnly` requires Buyer.

Many application services also include store ownership checks using the current user's `StoreId`. Admin users usually bypass store-level ownership checks.

Important missing role:

- There is no active Affiliate, Marketer, Commission, Referral, or Reseller system implemented.

There is a comment in `UserRole` mentioning a possible future `Reseller`, but it is not currently implemented.

## 6. Authentication

Authentication uses:

- ASP.NET Core Identity
- JWT Bearer tokens
- HttpOnly JWT cookie fallback

Main endpoints:

```text
POST /api/Account/register
POST /api/Account/login
POST /api/Account/logout
```

Register flow:

```text
Validate governorate
Check duplicate email in domain users and Identity users
Create domain User
Create Identity AppUser
Assign role
Generate JWT
Write JWT cookie
Return AuthResponseDto
```

Login flow:

```text
Find Identity user by email
Check password
Resolve role
Resolve related store id
Generate JWT
Write JWT cookie
Return AuthResponseDto
```

JWT contains user identity, domain user id, email, role, and store id.

JWT can be read from:

- Authorization header
- Auth cookie

## 7. API Controllers

### AccountController

Handles:

- Register
- Login
- Logout

### ProductController

Endpoints:

```text
GET    /api/Product
GET    /api/Product/{id}
POST   /api/Product
POST   /api/Product/create-with-offer
POST   /api/Product/search-by-image
PUT    /api/Product/{id}
DELETE /api/Product/{id}
```

Create and update use `multipart/form-data` because products require images.

Seller-only operations:

- Create
- CreateWithOffer
- Update
- Delete

Public operations:

- Get all
- Get by id
- Search by image

### CheckoutController

Endpoint:

```text
POST /api/Checkout
```

Buyer-only.

Can accept items from multiple stores and split them into separate orders.

### OrderController

Handles:

- List orders
- Get order
- Create order
- Update order status
- Delete order

Buyer creates orders. Sellers/store managers update order status for their own stores. Admin has wider access.

### OfferController

Handles CRUD for store-wide and product-specific offers.

Seller-only for create/update/delete.

### ProductImageController

Handles image upload/update/delete and embedding rebuild.

Includes admin-only endpoint for rebuilding embeddings.

### Other Controllers

The project also contains controllers for:

- Store
- User
- Review
- Category
- Carrier
- Governorate
- Message
- OrderItem
- PricingTier
- ShippingRate

## 8. Product Flow

Creating a product:

```text
Controller receives multipart form
Converts IFormFile to ImageUploadFile
ProductService validates store/category/images
Checks seller/store authorization
Uploads images
Creates Product and ProductImages
Saves through UnitOfWork
Returns ProductDto
```

Creating product with offer:

```text
Validate product data
Validate offer discount and dates
Check no store-wide/product overlap
Upload images
Create Product
Create related Offer
Create OfferProduct link
Save
```

Product DTO includes:

- BasePrice
- OldPrice
- CurrentPrice
- DiscountPercentage
- HasActiveOffer
- Images
- PricingTiers

## 9. Offer Flow

Offers may be:

- Store-wide
- Product-specific

Offer creation validates:

- Store exists
- User can manage store
- Discount and date range are valid
- Product ids belong to same store
- No overlapping offer conflicts

Active offers are loaded by checking:

```text
StartDate <= DateTime.UtcNow && EndDate >= DateTime.UtcNow
```

Price calculation uses the best active offer, usually highest discount.

## 10. Order Flow

Normal order creation:

```text
Validate buyer
Validate store
Validate carrier if present
Validate all items
Ensure all products belong to the same store
Ensure stock is available
Load active offers for that store
Calculate unit price after discount
Decrease stock
Create OrderItems
Calculate TotalAmount
Save order
```

Important behavior:

- Stock is decremented during order creation.
- Active offer price is captured in `OrderItem.UnitPrice`.
- `Product.RowVersion` helps detect concurrent stock updates.

## 11. Checkout Flow

Checkout supports mixed-store carts.

Flow:

```text
Validate buyer
Validate carrier if present
Group requested items by product
Validate products and stock
Group items by product.StoreId
Create one Order per store
Apply active offers per store
Decrease stock
Save all orders
Return checkout reference, total amount, and order summaries
```

This is different from `OrderService.CreateAsync`, which expects all products to belong to one store.

## 12. Review Flow

Review service validates:

- Buyer exists and is allowed
- Order exists
- Order belongs to buyer
- Store matches order
- Order status allows review
- Duplicate review is prevented

Reviews are intended to be purchase-gated.

## 13. Image Search

The project supports image-based product search.

Model:

```text
Elattaba.API/Models/image-embedding.onnx
```

Core pieces:

```text
OnnxImageEmbeddingService
ProductImageEmbeddingBackgroundService
ChannelProductImageEmbeddingQueue
ProductImageService
ProductService.SearchByImageAsync
```

Flow:

```text
Upload/search image
Generate embedding vector
Compare with stored product image embeddings
Use cosine similarity
Group by product
Return top K matches
```

The product image embedding process can run in background.

## 14. Database and EF Core

There are two DbContexts:

```text
El3atbaDbContext
AppIdentityDbContext
```

### El3atbaDbContext

Contains marketplace/domain tables:

- Users
- Stores
- Products
- ProductImages
- PricingTiers
- Offers
- OfferProducts
- Orders
- OrderItems
- Reviews
- Messages
- Categories
- Carriers
- ShippingRates
- Governorates

### AppIdentityDbContext

Contains ASP.NET Identity tables:

- AspNetUsers
- AspNetRoles
- AspNetUserRoles
- etc.

Both use the same connection string in development.

Development connection string:

```text
Server=(localdb)\MSSQLLocalDB;Database=El3ttbaDb;Trusted_Connection=True;TrustServerCertificate=True;Encrypt=False;
```

## 15. Repository and UnitOfWork

The code uses repositories and a unit of work.

Main abstraction:

```text
IUnitOfWork
```

It exposes repositories like:

```text
Users
Stores
Products
ProductImages
Orders
OrderItems
Offers
OfferProducts
Reviews
Messages
Categories
Carriers
Governorates
ShippingRates
PricingTiers
```

Services generally call:

```text
await _unitOfWork.SomeRepository.SomeMethodAsync(...)
await _unitOfWork.CompleteAsync()
```

This centralizes persistence.

## 16. Validation

Validation uses FluentValidation.

Validation is registered in:

```text
Elattba.Application/Validation/ApplicationValidationRegistration.cs
Elattaba.API/Validation
```

Validation covers:

- User create/update
- Product create/update
- Product form upload DTOs
- Checkout
- Orders
- Offers
- Reviews
- Misc resources

Invalid model state returns `ValidationProblemDetails` and includes `traceId`.

## 17. Error Handling

Configured exception handlers:

```text
ConcurrencyExceptionHandler
DatabaseExceptionHandler
GlobalExceptionHandler
```

The API also uses:

```text
UseStatusCodePagesWithReExecute("/errors/{0}")
```

Service-level methods often return:

```text
ServiceResult<T>
```

Controllers convert this result to IActionResult through extension helpers.

## 18. Security, CORS, and Rate Limiting

### Security Headers

Security headers are added using:

```text
NetEscapades.AspNetCore.SecurityHeaders
```

Important Swagger note:

Swagger UI is registered before `UseApiSecurityHeaders()` in Development so that the strict default CSP does not block Swagger JavaScript/CSS.

### CORS

Allowed local frontend origins:

```text
http://localhost:4200
https://localhost:4200
http://localhost:5173
https://localhost:5173
```

CORS allows:

- Any header
- Any method
- Credentials

### Rate Limiting

Global fixed-window limiter:

```text
8 requests per minute per IP
```

Rejection status:

```text
429 Too Many Requests
```

For heavy Swagger/frontend testing this may feel strict.

## 19. Local Setup

Requirements:

- .NET SDK 10
- SQL Server LocalDB
- dotnet-ef tool

Commands:

```powershell
sqllocaldb create MSSQLLocalDB
sqllocaldb start MSSQLLocalDB

dotnet restore El3ttba.slnx

dotnet ef database update --project Elattba.InfraStructure\Elattba.InfraStructure.csproj --startup-project Elattaba.API\Elattaba.API.csproj --context AppIdentityDbContext

dotnet ef database update --project Elattba.InfraStructure\Elattba.InfraStructure.csproj --startup-project Elattaba.API\Elattaba.API.csproj --context El3atbaDbContext

dotnet build El3ttba.slnx
dotnet run --project Elattaba.API\Elattaba.API.csproj --launch-profile http
```

Open:

```text
http://localhost:5191/swagger
```

If Swagger appears blank due to browser cache:

```text
http://localhost:5191/swagger/index.html?fresh=1
```

## 20. Common Local Issues

### LocalDB error

If the app fails with LocalDB instance errors:

```powershell
sqllocaldb create MSSQLLocalDB
sqllocaldb start MSSQLLocalDB
```

Then apply migrations.

### DLL locked during build

If build fails because DLL files are locked by `Elattaba.API`, stop the running API first.

Typical error:

```text
The process cannot access the file because it is being used by another process.
```

Fix:

```powershell
Stop the API process
dotnet build El3ttba.slnx
```

### Swagger blank page

Cause:

Security headers/CSP previously blocked Swagger UI scripts, or browser cached the old page.

Fix:

```text
Open /swagger/index.html?fresh=1
```

or hard refresh:

```text
Ctrl + F5
```

## 21. Tests

Run:

```powershell
dotnet test El3ttba.slnx
```

Current known coverage:

- Checkout creates orders by store
- Checkout validates stock, buyer, quantities
- Order creation decrements stock
- Order creation uses active offers
- Order rejects invalid/multi-store scenarios
- Offer overlap rules
- Product query filtering/sorting/search/pagination
- Product image upload/update behavior
- Review creation rules
- Validators

## 22. Current Missing Features

Not currently implemented:

- Affiliate marketing
- Referral codes
- Commission tracking
- Marketer dashboard
- Affiliate payouts
- Online payment gateway
- Reseller role
- Multi-manager stores
- Production-ready secret management
- Full production deployment configuration

Reserved/future-looking areas:

- `PaymentMethod.Online` exists but real payment gateway is not implemented.
- `UserRole` has a comment about future `Reseller`, but no actual role or logic exists.

## 23. Important Risks / Technical Notes

- `User` and `AppUser` must remain synchronized.
- JWT secret in appsettings is development-only and should be moved to user secrets/environment variables for production.
- Rate limit of 8 requests/minute may be too low during frontend development.
- Checkout/order stock updates depend on EF concurrency behavior.
- Image search depends on embeddings being generated and stored.
- If product images are replaced, old image files are deleted after successful DB update.
- If product creation fails after image upload, uploaded files are cleaned up.
- Swagger security header fix should remain in place: Swagger must run before security headers in Development.

## 24. Suggested Questions for Learning the Project

Use these prompts with an AI or mentor:

1. Explain the architecture of this backend from request to database.
2. Explain why the project has both `User` and `AppUser`.
3. Explain how roles and policies work.
4. Explain how sellers are restricted to their own stores.
5. Explain the checkout flow and how it splits products by store.
6. Explain how offers affect product prices and order prices.
7. Explain how stock decrement and concurrency work.
8. Explain how image search works using embeddings.
9. Explain how EF migrations are split between Identity and domain data.
10. Explain how to add affiliate marketing and commissions to this architecture.
11. Explain how to add real online payment.
12. Explain what should be changed before production deployment.

## 25. If Adding Affiliate Marketing Later

A likely design would add:

Entities:

```text
AffiliateProfile
ReferralCode
OrderAffiliateCommission
CommissionPayout
```

Possible flow:

```text
Buyer enters referral code or arrives through affiliate link
Checkout stores referral code on created orders
After order is delivered/paid, commission is calculated
Commission record is created
Admin can approve payout
Affiliate can see dashboard totals
```

Needed API areas:

```text
AffiliateController
ReferralController
CommissionController
PayoutController
```

Needed business rules:

- Commission percentage
- Which products/stores allow commission
- When commission becomes payable
- Refund/cancel behavior
- Duplicate/self-referral prevention

## 26. One-Sentence Summary

`El3ttba` is a .NET 10 marketplace backend with Identity/JWT authentication, seller-managed stores and products, active offer pricing, multi-store checkout, order/review workflows, image upload and ONNX-based image search, EF Core persistence, and tested business services.
