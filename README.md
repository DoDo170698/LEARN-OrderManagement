# Order Management GraphQL Application

A full-stack local-only application demonstrating GraphQL implementation with real-time capabilities using .NET 10, HotChocolate, and Blazor Server.

## Overview

This project implements a complete Order Management system with:
- **Backend**: ASP.NET Core GraphQL API using HotChocolate
- **Frontend**: Blazor Server application with strongly-typed GraphQL client
- **Database**: Entity Framework Core with In-Memory provider
- **Real-time**: GraphQL Subscriptions over WebSocket
- **Authentication**: Mock JWT Bearer Token

### This project is implemented as **local-only**

Azure deployment is intentionally skipped, which is allowed by the exercise requirements.

---

## Tech Stack

### Backend
- **.NET 10.0** - Latest .NET framework
- **HotChocolate 14.1** - GraphQL server for .NET
- **Entity Framework Core 10.x** - ORM with In-Memory provider
- **ASP.NET Core** - Web framework

### Frontend
- **Blazor Server** - Interactive server-side rendering
- **Custom GraphQL Client** - Strongly-typed HTTP client for GraphQL
- **Bootstrap 5** - UI framework

### Patterns & Architecture
- **Repository Pattern** - Data access abstraction
- **Unit of Work** - Transaction management
- **Service Layer** - Business logic separation
- **Dependency Injection** - Built-in .NET DI container

---

## Domain Model

**Orders → OrderItems** (1:N relationship)

### Entities

#### Order
- `Id`: Guid - Unique identifier
- `OrderNumber`: string - Human-readable order number (e.g., "ORD-2025-001")
- `CustomerName`: string
- `CustomerEmail`: string
- `Status`: OrderStatus enum (Pending, Processing, Completed, Cancelled)
- `TotalAmount`: decimal - **Computed field (custom resolver)**
- `CreatedAt`: DateTime
- `UpdatedAt`: DateTime
- `Items`: Collection<OrderItem>

#### OrderItem
- `Id`: Guid
- `OrderId`: Guid - Foreign key
- `ProductName`: string
- `Quantity`: int
- `UnitPrice`: decimal
- `Subtotal`: decimal (Quantity * UnitPrice)
- `CreatedAt`: DateTime

---

## Project Structure

```
OrderManagement.sln
│
├── OrderManagement.GraphQL/          # Backend GraphQL API
│   ├── Domain/
│   │   ├── Entities/                 # Order, OrderItem
│   │   └── Enums/                    # OrderStatus
│   │
│   ├── Data/
│   │   ├── ApplicationDbContext.cs
│   │   ├── DbInitializer.cs
│   │   └── Configurations/           # EF Core configurations
│   │
│   ├── Repositories/
│   │   ├── Interfaces/               # Generic & specific repository interfaces
│   │   └── Implementations/          # Repository & UnitOfWork implementations
│   │
│   ├── Services/
│   │   ├── Interfaces/               # IOrderService
│   │   └── Implementations/          # OrderService
│   │
│   ├── GraphQL/
│   │   ├── Queries/                  # OrderQueries
│   │   ├── Mutations/                # OrderMutations
│   │   ├── Subscriptions/            # OrderSubscriptions
│   │   ├── Types/                    # OrderType, OrderItemType (with custom resolvers)
│   │   ├── Inputs/                   # CreateOrderInput, UpdateOrderInput, etc.
│   │   └── Payloads/                 # Mutation payloads & errors
│   │
│   ├── Middleware/
│   │   └── AuthenticationMiddleware.cs  # Mock JWT authentication
│   │
│   ├── Program.cs                    # Application entry point & DI configuration
│   └── appsettings.json
│
└── OrderManagement.Blazor/           # Frontend Blazor App
    ├── Services/
    │   ├── GraphQLClient.cs          # HTTP GraphQL client
    │   ├── Models.cs                 # Strongly-typed models
    │   ├── OrderService.cs           # Order operations
    │   └── OrderSubscriptionService.cs  # WebSocket subscriptions
    │
    ├── Pages/
    │   └── Orders/
    │       ├── OrderList.razor       # List all orders
    │       ├── OrderDetail.razor     # View order details
    │       ├── CreateOrder.razor     # Create new order
    │       └── EditOrder.razor       # Edit existing order
    │
    ├── Program.cs
    └── appsettings.json
```

---

## GraphQL Schema

### Queries

```graphql
type Query {
  # Get all orders
  orders: [Order!]!

  # Get order by ID (throws error if not found)
  orderById(id: UUID!): Order!

  # Get order by order number
  orderByOrderNumber(orderNumber: String!): Order

  # Get orders by status
  ordersByStatus(status: OrderStatus!): [Order!]!
}
```

### Mutations

```graphql
type Mutation {
  # Create a new order (requires authentication)
  createOrder(input: CreateOrderInput!): CreateOrderPayload!

  # Update an existing order (requires authentication)
  updateOrder(input: UpdateOrderInput!): UpdateOrderPayload!

  # Add item to an order (requires authentication)
  addOrderItem(input: AddOrderItemInput!): AddOrderItemPayload!

  # Delete an order (requires authentication)
  deleteOrder(id: UUID!): Boolean!
}
```

### Subscriptions

```graphql
type Subscription {
  # Subscribe to new order creation events
  onOrderCreated: Order!

  # Subscribe to order update events
  onOrderUpdated: Order!
}
```

### Types

```graphql
type Order {
  id: UUID!
  orderNumber: String!
  customerName: String!
  customerEmail: String!
  status: OrderStatus!
  totalAmount: Decimal!   # Custom resolver - computed from items
  createdAt: DateTime!
  updatedAt: DateTime!
  items: [OrderItem!]!
}

type OrderItem {
  id: UUID!
  orderId: UUID!
  productName: String!
  quantity: Int!
  unitPrice: Decimal!
  subtotal: Decimal!
  createdAt: DateTime!
}

enum OrderStatus {
  PENDING
  PROCESSING
  COMPLETED
  CANCELLED
}
```

---

## Getting Started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download) installed
- A GraphQL client (Banana Cake Pop, Postman, or similar)

### Running the Backend

1. Navigate to the backend directory:
   ```bash
   cd SourceCode/OrderManagement.GraphQL
   ```

2. Run the application:
   ```bash
   dotnet run
   ```

3. The GraphQL server will start at:
   ```
   http://localhost:5000/graphql
   ```

4. You should see:
   ```
   ================================
   🚀 GraphQL Server Started
   ================================
   📍 Endpoint: http://localhost:5000/graphql
   💾 Database: In-Memory
   🔑 Auth Token: LOCAL_TOKEN_12345
   💡 Authorization Header: Bearer LOCAL_TOKEN_12345
   📝 Use Banana Cake Pop or any GraphQL client
   ================================
   ```

### Running the Frontend

1. Open a new terminal and navigate to the Blazor directory:
   ```bash
   cd SourceCode/OrderManagement.Blazor
   ```

2. Run the application:
   ```bash
   dotnet run
   ```

3. The Blazor app will start (typically at `http://localhost:5001` or similar)

4. Open your browser and navigate to the displayed URL

---

## Testing the Application

### Using Banana Cake Pop (Built-in GraphQL IDE)

1. Navigate to `http://localhost:5000/graphql` in your browser
2. The Banana Cake Pop interface will open automatically

### Testing Queries (No Auth Required)

#### Get All Orders
```graphql
query {
  orders {
    id
    orderNumber
    customerName
    customerEmail
    status
    totalAmount
    createdAt
    items {
      productName
      quantity
      unitPrice
      subtotal
    }
  }
}
```

#### Get Order by ID
```graphql
query {
  orderById(id: "11111111-1111-1111-1111-111111111111") {
    orderNumber
    customerName
    totalAmount
    items {
      productName
      quantity
      subtotal
    }
  }
}
```

### Testing Mutations (Auth Required)

**IMPORTANT**: All mutations require authentication. Add this header in Banana Cake Pop:

```
Authorization: Bearer LOCAL_TOKEN_12345
```

#### Create Order
```graphql
mutation {
  createOrder(input: {
    customerName: "John Doe"
    customerEmail: "john.doe@example.com"
    items: [
      {
        productName: "MacBook Pro 16\""
        quantity: 1
        unitPrice: 2499.00
      },
      {
        productName: "Magic Mouse"
        quantity: 2
        unitPrice: 79.00
      }
    ]
  }) {
    order {
      id
      orderNumber
      totalAmount
      status
      items {
        productName
        subtotal
      }
    }
  }
}
```

#### Update Order
```graphql
mutation {
  updateOrder(input: {
    id: "11111111-1111-1111-1111-111111111111"
    status: COMPLETED
  }) {
    order {
      orderNumber
      status
      updatedAt
    }
  }
}
```

### Testing Subscriptions

#### Subscribe to Order Creation
```graphql
subscription {
  onOrderCreated {
    orderNumber
    customerName
    totalAmount
    status
  }
}
```

Then, in another tab, create a new order (using the mutation above). The subscription will receive the new order in real-time.

#### Subscribe to Order Updates
```graphql
subscription {
  onOrderUpdated {
    orderNumber
    status
    totalAmount
  }
}
```

Then, in another tab, update an order status. The subscription will receive the update in real-time.

---

## Authentication

This application uses **mock JWT authentication** for demonstration purposes only.

### Valid Token
```
LOCAL_TOKEN_12345
```

### How to Use

#### In Banana Cake Pop
1. Click "Headers" button
2. Add header:
   - Key: `Authorization`
   - Value: `Bearer LOCAL_TOKEN_12345`

#### In Postman
1. Go to "Authorization" tab
2. Select "Bearer Token"
3. Enter: `LOCAL_TOKEN_12345`

#### In Code (Frontend)
The frontend automatically includes the token in all requests:
```csharp
_httpClient.DefaultRequestHeaders.Authorization =
    new AuthenticationHeaderValue("Bearer", "LOCAL_TOKEN_12345");
```

### What Happens Without Auth?

Mutations without the auth header will return:
```json
{
  "errors": [
    {
      "message": "The current user is not authorized to access this resource.",
      "extensions": {
        "code": "AUTH_NOT_AUTHORIZED"
      }
    }
  ]
}
```

---

## Realtime Features

The application demonstrates real-time updates using GraphQL Subscriptions over WebSocket.

### How It Works

1. **Backend**: HotChocolate's in-memory pub/sub system
   - When a mutation occurs (create/update order)
   - An event is published via `ITopicEventSender`
   - Subscribed clients receive the update via WebSocket

2. **Frontend**: Blazor subscribes to updates
   - Connects to WebSocket endpoint
   - Subscribes to `onOrderCreated` and `onOrderUpdated`
   - Automatically updates the UI when events are received

### Testing Realtime

1. Open the frontend at `/orders` (Order List page)
2. Open Banana Cake Pop in another window
3. Create a new order using the mutation (with auth header)
4. Watch the frontend automatically add the new order to the list **without refreshing**

---

## Error Handling

### GraphQL Errors

The application demonstrates proper error handling:

#### Order Not Found
```graphql
query {
  orderById(id: "00000000-0000-0000-0000-000000000000") {
    orderNumber
  }
}
```

Response:
```json
{
  "errors": [
    {
      "message": "Order with ID '00000000-0000-0000-0000-000000000000' was not found.",
      "path": ["orderById"]
    }
  ]
}
```

### Frontend Error Handling

- **Loading States**: Spinners while fetching data
- **Error States**: Alert messages for failed operations
- **Validation**: Form validation before submission
- **Disabled Buttons**: Prevent double submission

---

## Custom Resolver Example

The `totalAmount` field on `Order` is a **custom resolver** that demonstrates:
- Async database access in a resolver
- Computed fields in GraphQL
- Dependency injection in resolvers

### Implementation

`OrderManagement.GraphQL/GraphQL/Types/OrderType.cs:48-52`:

```csharp
descriptor
    .Field("totalAmount")
    .Type<DecimalType>()
    .Description("Total amount calculated from all item subtotals")
    .ResolveWith<OrderResolvers>(r => r.GetTotalAmountAsync(default!, default!, default));
```

### Resolver Method

```csharp
public async Task<decimal> GetTotalAmountAsync(
    [Parent] Order order,
    [Service] IOrderService orderService,
    CancellationToken cancellationToken)
{
    return await orderService.GetOrderTotalAmountAsync(order.Id, cancellationToken);
}
```

---

## Seeded Data

The application comes with 3 pre-seeded orders in the in-memory database:

| Order Number | Customer | Status | Items | Total |
|-------------|----------|--------|-------|-------|
| ORD-2025-001 | Nguyen Van A | Completed | 2 items | $1,699.98 |
| ORD-2025-002 | Tran Thi B | Processing | 2 items | $1,448.00 |
| ORD-2025-003 | Le Van C | Pending | 1 item | $900.00 |

---

## Features Checklist

### Core Requirements
- ✅ **GraphQL thật**: HotChocolate 14.x
- ✅ **CRUD đầy đủ**: Queries + Mutations for Orders/OrderItems
- ✅ **Nested data**: Order → OrderItems (1:N)
- ✅ **Realtime**: GraphQL Subscriptions over WebSocket
- ✅ **Custom resolver**: `totalAmount` computed from OrderItems with async DB access
- ✅ **Auth mock**: Bearer token middleware (`LOCAL_TOKEN_12345`)
- ✅ **Error handling**: OrderNotFoundError with proper GraphQL error format
- ✅ **Blazor consume GraphQL**: Strongly-typed GraphQL client
- ✅ **UI đầy đủ**: List, Detail, Create, Edit pages
- ✅ **Repository Pattern**: Generic + Specific repositories + UnitOfWork
- ✅ **Local only**: In-Memory database, no cloud dependencies
- ✅ **Documentation**: Comprehensive README

### UI Features
- ✅ **Loading States**: Spinners during data fetch
- ✅ **Error States**: Alert messages for failures
- ✅ **Realtime UI**: Automatic list updates on create/update events
- ✅ **Form Validation**: Client-side validation before submission
- ✅ **Disabled Submit**: Prevent double-submission during processing

---

## AI Usage Disclosure

This project was developed with assistance from Claude (Anthropic's AI assistant) for:

- **Code Generation**: Generating boilerplate code for entities, repositories, and services
- **Architecture Design**: Structuring the application layers and design patterns
- **GraphQL Schema**: Defining queries, mutations, subscriptions, and types
- **Documentation**: Writing comprehensive README and code comments
- **Best Practices**: Applying C# 10+ features and .NET conventions

The AI was used as a productivity tool to accelerate development while maintaining code quality and architectural consistency.

---

## Troubleshooting

### Backend won't start
- Ensure .NET 10 SDK is installed: `dotnet --version`
- Check if port 5000 is available
- Run `dotnet restore` in the backend directory

### Frontend can't connect to backend
- Verify backend is running at `http://localhost:5000/graphql`
- Check console for CORS errors
- Ensure `HttpClient.BaseAddress` is correct in `Program.cs`

### Mutations return auth errors
- Verify you're sending the `Authorization` header
- Token must be: `Bearer LOCAL_TOKEN_12345`
- Check headers in your GraphQL client

### Realtime not working
- Ensure WebSocket is enabled on backend
- Check browser console for WebSocket connection errors
- Verify subscriptions are set up correctly in frontend

---

## Future Enhancements

While not required for this exercise, potential improvements include:

- Pagination for order lists
- Search and filtering
- Order cancellation with confirmation
- Item modification after order creation
- Export orders to CSV/PDF
- Dashboard with statistics
- Multiple concurrent orders support
- Optimistic UI updates

---

## License

This project is for educational purposes only.

---

## Contact

For questions or issues, please create an issue in the repository.

---

**Happy coding! 🚀**
