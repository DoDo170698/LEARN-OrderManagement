# Order Management System

A complete Order Management System built with GraphQL, Blazor Server, and Clean Architecture principles, featuring real-time updates via GraphQL Subscriptions.

## Tech Stack

### Backend (GraphQL Server)
- **ASP.NET Core 10.0** - Modern web framework
- **HotChocolate 14.1.0** - GraphQL server implementation
- **Clean Architecture** - Separation of concerns with Domain, Application, Infrastructure layers
- **CQRS + MediatR** - Command Query Responsibility Segregation pattern
- **SQLite** - Lightweight persistent database
- **JWT Bearer Authentication** - Secure API with static token
- **GraphQL Subscriptions** - Real-time updates via WebSocket

### Frontend (Blazor)
- **Blazor Server (.NET 10.0)** - Interactive server-side rendering
- **StrawberryShake 14.1.0** - Strongly-typed GraphQL client with code generation
- **Bootstrap 5** - Modern responsive UI
- **Bootstrap Icons** - Icon library
- **Interactive Server Rendering** - Real-time UI updates

## Project Structure

```
SourceCode/
├── OrderManagement.Domain/         # Domain entities and interfaces
├── OrderManagement.Application/    # Business logic, CQRS handlers, DTOs
├── OrderManagement.Infrastructure/ # EF Core, repositories, data access
├── OrderManagement.GraphQL/        # GraphQL schema, types, mutations, subscriptions
├── OrderManagement.Blazor/         # Blazor Server UI with StrawberryShake client
└── OrderManagement.Tests/          # Unit and Integration tests
```

## Features

### CRUD Operations
- **Create Order** - Create new orders with multiple items
- **View Orders** - List all orders with status badges and totals
- **View Order Details** - See complete order information with items
- **Update Order** - Edit customer information and order status
- **Delete Order** - Remove orders (via GraphQL mutation)

### Real-time Features
- **Live Order List** - New orders appear instantly in all connected clients
- **Live Order Updates** - Order changes propagate immediately to all users
- **WebSocket Subscriptions** - Efficient real-time communication
- **Visual Status Indicator** - "Live Updates Active" badge shows connection status

### Order Management
- Order statuses: Pending, Processing, Completed, Cancelled
- Multiple order items per order
- Automatic total calculation
- Order number auto-generation
- Timestamp tracking (Created, Updated)

## Getting Started

### Prerequisites
- .NET SDK 10.0 or later
- Windows, macOS, or Linux
- Any modern web browser

### 1. Start the GraphQL Backend Server

```bash
cd SourceCode/OrderManagement.GraphQL
dotnet run
```

The server will start on **http://localhost:5118**

You should see output like:
```
================================
GraphQL Server Started
================================
GraphQL Endpoint: http://localhost:5118/graphql

Database: SQLite
Architecture: Clean Architecture + CQRS + MediatR
Auth: JWT Bearer Token (Static)

JWT Token (copy this):
eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...

Token Details:
  - User: Mock Admin User
  - Email: admin@example.com
  - Role: Admin
  - Expires: 2035 (long-lived for testing)
================================
```

### 2. Start the Blazor Frontend

Open a **new terminal** and run:

```bash
cd SourceCode/OrderManagement.Blazor
dotnet run
```

The Blazor app will start on **http://localhost:5101**

### 3. Open the Application

Navigate to **http://localhost:5101** in your browser.

The application automatically connects to the GraphQL server using the JWT token configured in `appsettings.json`.

## Configuration

### GraphQL Server (`OrderManagement.GraphQL/appsettings.json`)

```json
{
  "Jwt": {
    "SecretKey": "ThisIsASecretKeyForJWTTokenGenerationAndValidation123!",
    "Issuer": "OrderManagement.GraphQL",
    "Audience": "OrderManagement.Client",
    "ExpiryInYears": 10
  }
}
```

### Blazor Client (`OrderManagement.Blazor/appsettings.json`)

```json
{
  "GraphQL": {
    "Endpoint": "http://localhost:5118/graphql",
    "WebSocketEndpoint": "ws://localhost:5118/graphql",
    "JwtToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
  }
}
```

## Architecture Details

### Clean Architecture Layers

**Domain Layer** (`OrderManagement.Domain`)
- Pure business entities: Order, OrderItem
- Domain interfaces: IOrderRepository
- No external dependencies

**Application Layer** (`OrderManagement.Application`)
- CQRS Commands: CreateOrderCommand, UpdateOrderCommand, DeleteOrderCommand
- CQRS Queries: GetOrdersQuery, GetOrderByIdQuery
- Command/Query Handlers using MediatR
- DTOs: OrderDto, OrderItemDto, CreateOrderDto, UpdateOrderDto
- AutoMapper profiles for entity-to-DTO mapping

**Infrastructure Layer** (`OrderManagement.Infrastructure`)
- Entity Framework Core configuration
- Repository implementations
- ApplicationDbContext with SQLite database
- Database seeding with sample data

**GraphQL Layer** (`OrderManagement.GraphQL`)
- HotChocolate schema definition
- GraphQL types: OrderType, OrderItemType
- Input types: CreateOrderInput, UpdateOrderInput
- Mutations: createOrder, updateOrder, deleteOrder
- Queries: orders, orderById
- Subscriptions: onOrderCreated, onOrderUpdated
- JWT authentication configuration

**Presentation Layer** (`OrderManagement.Blazor`)
- Blazor Server pages: OrderList, OrderDetail, CreateOrder, EditOrder
- StrawberryShake GraphQL client integration
- Real-time subscription handling
- Bootstrap UI components

### CQRS Pattern

**Commands** (write operations):
- `CreateOrderCommand` → `CreateOrderCommandHandler` → Repository → Database
- `UpdateOrderCommand` → `UpdateOrderCommandHandler` → Repository → Database
- `DeleteOrderCommand` → `DeleteOrderCommandHandler` → Repository → Database

**Queries** (read operations):
- `GetOrdersQuery` → `GetOrdersQueryHandler` → Repository → DTOs
- `GetOrderByIdQuery` → `GetOrderByIdQueryHandler` → Repository → DTOs

**Subscriptions** (real-time events):
- Order mutations trigger `ITopicEventSender` to publish events
- Subscribers receive events via WebSocket
- Events: `OnOrderCreated`, `OnOrderUpdated`

### StrawberryShake Code Generation

The Blazor project uses StrawberryShake to generate strongly-typed C# clients from GraphQL operations:

**1. GraphQL Operations** (`GraphQL/OrderOperations.graphql`)
- Defines all queries, mutations, and subscriptions

**2. Build-time Code Generation**
- StrawberryShake MSBuild task downloads schema from server
- Generates C# interfaces and classes
- Creates `IOrderManagementClient` service

**3. Dependency Injection**
- `AddOrderManagementClient()` registers GraphQL client
- Configures HTTP client with JWT authentication
- Configures WebSocket client for subscriptions

**4. Usage in Pages**
```csharp
@inject IOrderManagementClient GraphQLClient

var result = await GraphQLClient.GetOrders.ExecuteAsync();
var orders = result.Data.Orders;
```

## Testing GraphQL Operations

### Using Banana Cake Pop (Built-in GraphQL IDE)

1. Navigate to **http://localhost:5118/graphql** in your browser
2. The Banana Cake Pop IDE will open
3. Click "Schema Reference" to explore available queries, mutations, and subscriptions

### Authentication

Add the JWT token to your requests:

**HTTP Header:**
```
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJtb2NrLWFkbWluLXVzZXIiLCJuYW1lIjoiTW9jayBBZG1pbiBVc2VyIiwiZW1haWwiOiJhZG1pbkBleGFtcGxlLmNvbSIsInJvbGUiOiJBZG1pbiIsImlzcyI6Ik9yZGVyTWFuYWdlbWVudC5HcmFwaFFMIiwiYXVkIjoiT3JkZXJNYW5hZ2VtZW50LkNsaWVudCIsImV4cCI6MjA1MTIyMjQwMH0.hMPIHM6t4XhdpHxdlWkdABFm0lpokhr_kNiXmruqjko
```

### Sample GraphQL Queries

**Get All Orders:**
```graphql
query GetOrders {
  orders {
    nodes {
      id
      orderNumber
      customerName
      customerEmail
      status
      totalAmount
      createdAt
      items {
        id
        productName
        quantity
        unitPrice
        subtotal
      }
    }
    totalCount
  }
}
```

**Get Order by ID:**
```graphql
query GetOrderById($id: UUID!) {
  orderById(id: $id) {
    id
    orderNumber
    customerName
    customerEmail
    status
    totalAmount
    items {
      productName
      quantity
      unitPrice
      subtotal
    }
  }
}
```

**Variables:**
```json
{
  "id": "11111111-1111-1111-1111-111111111111"
}
```

**Get Orders by Status (Filtering):**
```graphql
query GetOrdersByStatus($status: OrderStatus!) {
  orders(where: { status: { eq: $status } }) {
    nodes {
        orderNumber
        customerName
        status
        totalAmount
    }
  }
}
```

**Variables:**
```json
{
  "status": "PENDING"
}
```

### Sample GraphQL Mutations

**Create Order:**
```graphql
mutation CreateOrder($input: CreateOrderInput!) {
  createOrder(input: $input) {
    order {
      id
      orderNumber
      customerName
      totalAmount
      items {
        productName
        quantity
        unitPrice
        subtotal
      }
    }
  }
}
```

**Variables:**
```json
{
  "input": {
    "customerName": "John Doe",
    "customerEmail": "john.doe@example.com",
    "items": [
      {
        "productName": "Laptop",
        "quantity": 1,
        "unitPrice": 1299.99
      },
      {
        "productName": "Mouse",
        "quantity": 2,
        "unitPrice": 29.99
      }
    ]
  }
}
```

**Update Order:**
```graphql
mutation UpdateOrder($input: UpdateOrderInput!) {
  updateOrder(input: $input) {
    order {
      id
      orderNumber
      customerName
      customerEmail
      status
      totalAmount
    }
  }
}
```

**Variables:**
```json
{
  "input": {
    "id": "11111111-1111-1111-1111-111111111111",
    "customerName": "Jane Smith",
    "customerEmail": "jane.smith@example.com",
    "status": "PROCESSING"
  }
}
```

**Delete Order:**
```graphql
mutation DeleteOrder($id: UUID!) {
  deleteOrder(id: $id)
}
```

**Variables:**
```json
{
  "id": "11111111-1111-1111-1111-111111111111"
}
```

### GraphQL Subscriptions (Real-time Updates)

**Subscribe to New Orders:**
```graphql
subscription OnOrderCreated {
  onOrderCreated {
    id
    orderNumber
    customerName
    status
    totalAmount
  }
}
```

After subscribing, create a new order in another browser tab or via the Blazor UI. The subscription will receive the event immediately!

**Subscribe to Order Updates:**
```graphql
subscription OnOrderUpdated {
  onOrderUpdated {
    id
    orderNumber
    customerName
    status
    totalAmount
  }
}
```

After subscribing, update an order via the Blazor UI. The subscription will receive the update event instantly!

## Testing Real-time Subscriptions in Blazor

### Test 1: Real-time Order Creation

1. **Open the Order List** in two browser windows side-by-side:
   - Window 1: http://localhost:5101/orders
   - Window 2: http://localhost:5101/orders

2. **Verify Subscriptions Active:**
   - Both windows should show a green "Live Updates Active" badge at the top

3. **Create a New Order** in Window 1:
   - Click "Create New Order"
   - Fill in customer information:
     - Customer Name: "Test Customer"
     - Customer Email: "test@example.com"
   - Add at least one item:
     - Product Name: "Test Product"
     - Quantity: 1
     - Unit Price: 100
   - Click "Create Order"

4. **Watch Window 2:**
   - The new order should appear **instantly** at the top of the list
   - No page refresh required!

### Test 2: Real-time Order Updates

1. **Keep both windows open** on the orders list

2. **Edit an Order** in Window 1:
   - Click "Edit" on any order
   - Change the customer name to "Updated Customer"
   - Change the status to "Processing"
   - Click "Update Order"

3. **Watch Window 2:**
   - The order row should update **instantly** with the new values
   - The status badge color should change
   - No page refresh required!

### Test 3: Multi-Client Testing

1. **Open 3 or more browser windows** all showing http://localhost:5101/orders

2. **Create orders from different windows**

3. **All windows should update simultaneously** - this demonstrates true real-time multi-client synchronization

### WebSocket Connection Details

- **Endpoint:** ws://localhost:5118/graphql
- **Protocol:** GraphQL over WebSocket (graphql-ws)
- **Authentication:** JWT token sent in connection initialization payload
- **Auto-reconnect:** StrawberryShake handles reconnection automatically
- **Status Indicator:** Green "Live Updates Active" badge when connected

## Sample Data

The GraphQL server includes 8 pre-seeded orders with various statuses:

- **ORD-001** - Laptop and Mouse (Completed)
- **ORD-002** - Desk Chair (Processing)
- **ORD-003** - Monitor (Pending)
- **ORD-004** - Keyboard and Mouse Pad (Completed)
- **ORD-005** - USB Cable (Cancelled)
- **ORD-006** - Webcam (Processing)
- **ORD-007** - Headphones (Completed)
- **ORD-008** - Phone Charger (Pending)

## Security Considerations

This project uses **static JWT tokens** for simplicity in development and testing. In production, you should implement:

1. **Dynamic Token Generation** - Issue tokens upon successful login
2. **Token Refresh** - Implement refresh token flow
3. **Role-Based Authorization** - Add `[Authorize(Roles = "Admin")]` attributes
4. **HTTPS** - Use TLS/SSL for all communication
5. **Token Expiration** - Use shorter expiration times (e.g., 1 hour)
6. **Secure Storage** - Store tokens securely (HttpOnly cookies or secure storage)

## Troubleshooting

### "No service for type 'ISessionPool'" Error

**Problem:** WebSocket subscriptions not configured

**Solution:** Ensure `ConfigureWebSocketClient()` is called in `Program.cs`:
```csharp
.AddOrderManagementClient()
.ConfigureHttpClient(...)
.ConfigureWebSocketClient(...) // This is required for subscriptions!
```

### "Cannot provide a value for property 'OrderService'" Error

**Problem:** Old service still being injected in Razor pages

**Solution:** Replace `@inject OrderService OrderService` with `@inject IOrderManagementClient GraphQLClient`

### Build Errors "Cannot access file... in use by another process"

**Problem:** Server is still running and locking DLL files

**Solution:** Stop all running servers before building:
```bash
# Press Ctrl+C in all terminal windows running dotnet run
# Or kill processes:
taskkill /F /IM OrderManagement.GraphQL.exe
taskkill /F /IM OrderManagement.Blazor.exe
```

### Subscription Not Receiving Events

**Problem:** WebSocket connection fails or JWT token invalid

**Solution:**
1. Check browser console for WebSocket errors
2. Verify JWT token is not expired
3. Ensure GraphQL server is running on ws://localhost:5118/graphql
4. Check that `WebSocketAuthInterceptor` is sending the correct token

### "Live Updates Active" Badge Not Showing

**Problem:** Subscriptions failed to initialize

**Solution:**
1. Open browser developer console
2. Look for JavaScript/WebSocket errors
3. Verify WebSocket endpoint is correct in `appsettings.json`
4. Check that StrawberryShake WebSocket transport is installed

## Building for Production

### Backend (GraphQL Server)

```bash
cd OrderManagement.GraphQL
dotnet publish -c Release -o ./publish
```

### Frontend (Blazor)

```bash
cd OrderManagement.Blazor
dotnet publish -c Release -o ./publish
```

### Deployment Considerations

1. **Replace InMemory Database** with SQL Server, PostgreSQL, or another persistent database
2. **Configure HTTPS** with valid SSL certificates
3. **Implement Dynamic JWT** authentication with user login
4. **Add Logging** using Serilog or Application Insights
5. **Configure CORS** if frontend and backend are on different domains
6. **Set Environment Variables** for sensitive configuration
7. **Use Connection Pooling** for WebSocket subscriptions at scale

## License

This project is for educational and demonstration purposes.

## Contact

For questions or issues, please create an issue in the project repository.
