## RideMate NZ - Carpool Web Application

## Overview

RideMate NZ is a web-based carpooling application built using ASP.NET Core MVC.
It allows users to offer rides, find available rides, and book seats in a simple and efficient way.

The application is deployed on Microsoft Azure with a fully automated CI/CD pipeline using GitHub Actions.

---

## Live Application

Production URL:
https://ridemate-nz.azurewebsites.net

---

## Features

### User Authentication

* Register and login functionality using ASP.NET Identity
* Secure user management with role-based structure

### Offer Ride

* Users can create rides with:

  * Source location
  * Destination
  * Date and time
  * Available seats

### Find Ride

* Search rides by:

  * From location
  * To location
* Displays matching rides

### Book Ride

* Users can book available seats
* Seat availability updates automatically

### Cancel Booking

* Users can cancel bookings
* Seats are restored back to availability

### Location Autocomplete

* Integrated Google Places API
* Restricted to New Zealand locations only

---

## Tech Stack

### Frontend

* HTML5
* CSS
* Bootstrap
* JavaScript

### Backend

* ASP.NET Core MVC (.NET 8)
* C#

### Database

* Azure SQL Database

### Authentication

* ASP.NET Core Identity

### Cloud & DevOps

* Microsoft Azure App Service
* GitHub Actions (CI/CD pipeline)

---

## Architecture

* MVC (Model-View-Controller) pattern
* Entity Framework Core for database operations
* Azure App Service for hosting
* Azure SQL for persistent storage

---

## CI/CD Pipeline

* Code pushed to GitHub triggers deployment automatically
* GitHub Actions builds and deploys application to Azure
* No manual deployment required

Workflow file location:

```
.github/workflows/
```

---

## Database Configuration

The application uses Azure SQL Database.

Connection string is configured securely in Azure App Service:

```
App Service → Configuration → Connection Strings
```

Key:

```
DefaultConnection
```

---

## Local Setup Instructions

### Prerequisites

* Visual Studio 2022
* .NET 8 SDK
* SQL Server / Azure SQL

---

### Steps

1. Clone repository:

```
git clone https://github.com/kchawla15/CarPoolingApp.git
```

2. Open solution in Visual Studio

3. Update connection string in:

```
appsettings.json
```

4. Run migrations:

```
Update-Database
```

5. Run application:

```
F5 or Ctrl + F5
```

---

## Deployment Steps (Azure)

1. Create Azure App Service
2. Connect GitHub repository via Deployment Center
3. Configure connection string in Azure
4. Automatic deployment via GitHub Actions

---

## Security Notes

* Database credentials are stored in Azure Configuration, not in code
* Google API key should be restricted to domain usage
* HTTPS enforced in production

---

## Future Improvements

* Email notifications for bookings
* Payment integration
* Ride rating system
* Admin dashboard
* Map route visualization

---

## Author

Kshitij Chawla

---

## License

This project is for educational purposes.
