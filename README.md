# 🚗 Vehicle Service Management System

A comprehensive **Vehicle Service Management System** developed using **ASP.NET Core MVC, C#, Entity Framework Core, SQL Server, and Bootstrap**.

The system is designed to help vehicle service centers manage customers, vehicles, service bookings, job cards, mechanics, spare parts, invoices, and payments through a centralized web application.

---

## 📌 Project Overview

The **Vehicle Service Management System** provides an end-to-end solution for managing daily operations of a vehicle service center.

It allows service-center staff to:

* Manage customers
* Register and manage vehicles
* Manage vehicle types and brands
* Create service bookings
* Assign mechanics
* Create and manage job cards
* Manage spare parts and categories
* Generate invoices
* Track invoice items
* Manage payments
* Monitor service history
* Maintain organized service records

---

## 🎯 Objectives

The main objectives of this project are:

* Reduce manual paperwork in vehicle service centers
* Maintain centralized customer and vehicle information
* Track complete vehicle service history
* Improve service booking management
* Manage mechanics and job cards efficiently
* Maintain spare-part inventory information
* Generate accurate invoices
* Track payments and outstanding balances
* Provide a user-friendly and responsive interface

---

## 🛠️ Technologies Used

### Backend

* **C#**
* **ASP.NET Core MVC**
* **Entity Framework Core**
* **LINQ**
* **AutoMapper**
* **ASP.NET Core Identity**

### Database

* **Microsoft SQL Server**
* **Entity Framework Core Code First**
* **EF Core Migrations**

### Frontend

* **HTML5**
* **CSS3**
* **JavaScript**
* **Bootstrap**
* **Razor Views**

### Development Tools

* **Visual Studio**
* **SQL Server Management Studio**


---

## 🏗️ Architecture

The application follows a structured layered architecture:

```text
Vehicle_Service_Management_System
│
├── Application
│   ├── Services
│   └── ViewModels
│
├── Domain
│   ├── Entities
│   └── Enums
│
├── Infrastructure
│   └── Data
│       ├── ApplicationDbContext
│       ├── Configurations
│       └── Migrations
│
├── Controllers
│
├── Views
│
├── wwwroot
│   ├── css
│   ├── js
│   └── images
│
└── Program.cs
```

This separation helps keep business logic, database access, models, controllers, and presentation logic organized and maintainable.

---

## 🚘 Main Modules

### 1. Customer Management

The customer module allows staff to:

* Add customers
* Edit customer information
* View customer details
* Delete customers
* View customer service history
* Associate customers with their vehicles

---

### 2. Vehicle Management

The vehicle module manages:

* Customer vehicles
* Vehicle types
* Vehicle brands
* Registration numbers
* Vehicle models
* Manufacturing years
* Chassis numbers
* Insurance information
* Insurance expiry dates
* Vehicle active/inactive status

A vehicle is associated with a customer and can have multiple service bookings.

---

### 3. Service Booking

The service booking module allows staff to:

* Create service bookings
* Select customers
* Select vehicles
* Select service types
* Schedule service dates
* Track booking status
* Maintain booking numbers
* Manage service requests

---

### 4. Mechanic Management

The mechanic module provides functionality for:

* Adding mechanics
* Updating mechanic details
* Managing mechanic availability
* Assigning mechanics to service jobs
* Tracking mechanic-related service information

---

### 5. Job Card Management

Job cards are used to maintain detailed information about vehicle servicing.

The system can track:

* Service booking
* Assigned mechanic
* Vehicle problems
* Inspection information
* Service work
* Job status
* Service completion details

---

### 6. Spare Parts Management

The spare-part module provides management for:

* Spare parts
* Spare-part categories
* Part numbers
* Prices
* Stock information
* Active/inactive parts

Spare parts can also be associated with invoice items.

---

### 7. Invoice Management

The invoice module manages service billing.

It supports:

* Invoice creation
* Service charges
* Spare-part charges
* Invoice items
* Tax/discount calculations
* Total amount
* Paid amount
* Balance amount
* Invoice status

The invoice is linked with the corresponding service booking.

---

### 8. Payment Management

The payment module helps track:

* Invoice payments
* Payment amount
* Payment date
* Payment method
* Payment status
* Outstanding balance

---

## 🗄️ Database Structure

The application uses SQL Server with Entity Framework Core.

Important tables include:

```text
Customer
Vehicle
VehicleType
VehicleBrand
Mechanic
ServiceType
ServiceBooking
JobCard
SparePartCategorie
SparePart
Invoice
InvoiceItem
Payment
```

### Main Relationship Flow

```text
Customer
   │
   └── Vehicle
          │
          └── ServiceBooking
                  │
                  ├── JobCard
                  │
                  └── Invoice
                         │
                         ├── InvoiceItem
                         │
                         └── Payment
```

---

## 🔐 Authentication & Authorization

The application uses **ASP.NET Core Identity** for authentication and authorization.

Features include:

* User registration
* User login
* User logout
* Password management
* Role-based authorization
* Protected application areas

---

## 🔄 Application Workflow

The typical service-center workflow is:

```text
Customer Registration
        ↓
Vehicle Registration
        ↓
Service Booking
        ↓
Mechanic Assignment
        ↓
Job Card Creation
        ↓
Vehicle Inspection / Service
        ↓
Spare Parts Used
        ↓
Invoice Generation
        ↓
Payment
        ↓
Service Completed
```

---

## 📋 Key Features

* ✅ Customer Management
* ✅ Vehicle Management
* ✅ Vehicle Type Management
* ✅ Vehicle Brand Management
* ✅ Mechanic Management
* ✅ Service Type Management
* ✅ Service Booking Management
* ✅ Job Card Management
* ✅ Spare Part Management
* ✅ Spare Part Category Management
* ✅ Invoice Management
* ✅ Invoice Item Management
* ✅ Payment Management
* ✅ Authentication & Authorization
* ✅ Entity Framework Core
* ✅ SQL Server Database
* ✅ AutoMapper
* ✅ Responsive Bootstrap UI
* ✅ CRUD Operations
* ✅ Validation
* ✅ Service History Tracking

---

## ⚙️ Getting Started

### Prerequisites

Make sure the following software is installed:

* .NET SDK
* Visual Studio
* SQL Server
* SQL Server Management Studio
* Git

---

## 📥 Clone the Repository

```bash
git clone https://github.com/akshaychavan4510/Vehicle_Service_Management_System.git
```

Navigate to the project directory:

```bash
cd Vehicle_Service_Management_System
```

---

## 🗄️ Configure SQL Server

Open the application's configuration file:

```text
appsettings.json
```

Update the connection string according to your SQL Server configuration.

Example:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=YOUR_SERVER;Database=VehicleServiceDB;Trusted_Connection=True;TrustServerCertificate=True;"
}
```

Replace `YOUR_SERVER` with your SQL Server instance name.

---

## 🔨 Apply Entity Framework Migrations

Open the Package Manager Console or terminal and run:

```bash
dotnet ef database update
```

Alternatively, using Package Manager Console:

```powershell
Update-Database
```

---

## ▶️ Run the Application

Using Visual Studio:

1. Open the `.sln` solution.
2. Set the MVC project as the startup project.
3. Build the solution.
4. Run the application.

Or use:

```bash
dotnet run
```

---

## 🧪 Testing the Application

After starting the application, test the workflow in this order:

```text
1. Login
2. Add Customer
3. Add Vehicle
4. Create Service Booking
5. Assign Mechanic
6. Create Job Card
7. Add Spare Parts
8. Generate Invoice
9. Record Payment
10. Verify Service History
```

---

## 📁 Important Project Components

| Component   | Responsibility                   |
| ----------- | -------------------------------- |
| Controllers | Handle HTTP requests             |
| ViewModels  | Transfer and validate UI data    |
| Services    | Business logic                   |
| Entities    | Database/domain models           |
| DbContext   | Database communication           |
| AutoMapper  | Entity/ViewModel mapping         |
| Razor Views | User interface                   |
| Identity    | Authentication and authorization |
| SQL Server  | Data storage                     |
| EF Core     | ORM/database access              |

---

## 💡 Design Principles

The project focuses on:

* Separation of concerns
* Reusable services
* Dependency Injection
* Entity Framework Core
* ViewModel-based MVC design
* Data validation
* Maintainable code structure
* Secure authentication
* Database relationships
* Clean and organized UI

---

## 🚀 Future Enhancements

Possible future improvements include:

* 📊 Admin dashboard with service statistics
* 📧 Email notifications
* 📱 SMS notifications
* 📅 Online service appointment booking
* 📄 PDF invoice generation
* 📈 Revenue reports
* 📦 Real-time spare-part stock management
* 🔔 Service reminder notifications
* 🚗 Customer vehicle service history dashboard
* 💳 Online payment integration
* 📱 Mobile-friendly improvements
* 📊 Advanced reporting and analytics

---

## 👨‍💻 Developer

**Akshay Chavan**

MCA Graduate | Junior Software Developer

### Technical Skills

* C#
* ASP.NET Core MVC
* Entity Framework Core
* SQL Server
* HTML
* CSS
* JavaScript
* Bootstrap


---


---

**Vehicle Service Management System — Simplifying Vehicle Service Center Operations 🚗🔧**
