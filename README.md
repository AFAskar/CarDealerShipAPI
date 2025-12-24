# Car Dealership API

A .NET 9 Web API for managing a car dealership system. This project demonstrates CRUD operations, role-based access control (RBAC), and OTP (One-Time Password) security for sensitive actions.

## Features

- **User Management**: Register and Login with JWT Authentication.
- **Role-Based Access**: Separate capabilities for `Admin` and `Customer` roles.
- **Vehicle Management**: Admins can add and update vehicles. Customers can browse and filter vehicles.
- **Sales System**: Customers can request to purchase vehicles. Admins approve or reject sales.
- **OTP Security**: Sensitive actions (Login, Register, Update Vehicle, Purchase Request) are protected by a simulated OTP system.

## Tech Stack

- .NET 9 ASP.NET Core Web API
- Entity Framework Core (SQLite)
- ASP.NET Core Identity
- Scalar (OpenAPI/Swagger UI)

## Getting Started

### Prerequisites

- .NET 9 SDK

### Running the Application

1. Clone the repository.
2. Navigate to the project directory:

   ```bash
   cd CarDealership.Api
   ```

3. Run the application:

   ```bash
   dotnet run
   ```

The database will be automatically created and seeded on the first run.

4. Open the API documentation:
   - Scalar UI: `http://localhost:5056/docs` (or the port shown in your terminal)
   - Swagger/OpenAPI: `http://localhost:5056/openapi/v1.json`

## API Endpoints

### Authentication

- `POST /api/auth/register`: Register a new user. Returns an OTP in the console.
- `POST /api/auth/login`: Login. Returns an OTP in the console.
- `POST /api/auth/verify-otp`: Verify OTP to receive a JWT token.

### Vehicles

- `GET /api/vehicles`: Browse vehicles (Filter by make, model, year, price).
- `GET /api/vehicles/{id}`: Get vehicle details.
- `POST /api/vehicles`: (Admin) Add a new vehicle.
- `PUT /api/vehicles/{id}`: (Admin) Update vehicle details. **Requires `X-OTP` header**.

### Sales

- `POST /api/sales/request`: (Customer) Request to buy a vehicle. **Requires `X-OTP` header**.
- `POST /api/sales/process`: (Admin) Approve or reject a sale.
- `GET /api/sales/history`: (Customer) View purchase history.

### Users

- `GET /api/user`: (Admin) View all registered customers.

## OTP Flow

For actions requiring OTP (Login, Register, Update Vehicle, Purchase Request):

1. Initiate the action (e.g., call `POST /api/auth/login`).
2. Check the application console logs for the simulated OTP code (e.g., `[OTP] Login OTP for user@example.com: 123456`).
3. **For Auth**: Call `POST /api/auth/verify-otp` with the code to get your JWT.
4. **For Protected Actions**: Retry the request with the header `X-OTP: 123456`.

## Default Users

- **Admin**: `admin@dealership.com` / `Admin123!`
