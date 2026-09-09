# 🏋️ Elite Fitness Booking System

A full-stack fitness and personal training booking platform designed to help users explore fitness programs, view trainers, and book training sessions through a responsive web application.

The project consists of a JavaScript-based frontend integrated with a RESTful ASP.NET Web API backend and SQL Server database.

---

## ✨ Features

### 👤 User Features

- User registration and login
- JWT-based authentication
- Browse available trainers
- Browse training programs
- Book personal training sessions
- Manage bookings
- View membership information
- Responsive user interface

### 🛠️ Backend Features

- RESTful Web API
- JWT Authentication
- Entity Framework Core
- SQL Server integration
- CRUD operations
- API-based frontend/backend communication
- Structured backend architecture

### 💳 Payment Integration

- Stripe payment integration structure
- Secure configuration using environment-specific settings
- Payment-ready architecture

---

## 🛠️ Technologies Used

### Frontend

- HTML5
- CSS3
- Bootstrap 5
- JavaScript (ES6+)

### Backend

- C#
- ASP.NET Core Web API
- Entity Framework Core
- JWT Authentication

### Database

- SQL Server

### Tools

- Visual Studio
- Git
- GitHub
- Postman

---

## 🏗️ Architecture

```text
┌──────────────────────────────┐
│          Frontend            │
│  HTML • CSS • Bootstrap      │
│        JavaScript            │
└──────────────┬───────────────┘
               │
               │ HTTP / REST API
               ▼
┌──────────────────────────────┐
│      ASP.NET Core Web API    │
│          C# / .NET           │
│                              │
│ Authentication • Business    │
│ Logic • API Endpoints        │
└──────────────┬───────────────┘
               │
               │ Entity Framework Core
               ▼
┌──────────────────────────────┐
│          SQL Server          │
│       GYMBookingDB           │
└──────────────────────────────┘
```

---

## 📂 Project Structure

```text
Elite-Fitness-Booking-System/
│
├── Frontend/
│
├── backend/
│   └── elite/
│
├── screenshots/
│
├── database/
│
├── docs/
│
├── .gitignore
└── README.md
```

---

## 🚀 Getting Started

### Prerequisites

Make sure you have the following installed:

- .NET SDK
- SQL Server
- Visual Studio
- A modern web browser

### Installation

1. Clone the repository.

```bash
git clone https://github.com/nairagamal/elite-fitness-booking-system.git
```

2. Open the backend solution in Visual Studio.

3. Configure the SQL Server connection string in `appsettings.json`.

4. Create or restore the `GYMBookingDB` database.

5. Run the ASP.NET Core Web API.

6. Open the frontend in a browser or run it using your preferred local development server.

---

## 🔐 Configuration

Sensitive credentials such as API keys, payment secrets, and production credentials should not be committed to the repository.

Use environment-specific configuration for sensitive values.

Example:

```json
{
  "Stripe": {
    "SecretKey": "YOUR_SECRET_KEY",
    "PublishableKey": "YOUR_PUBLISHABLE_KEY",
    "WebhookSecret": "YOUR_WEBHOOK_SECRET"
  }
}
```

---

## 🔌 API

The backend exposes RESTful API endpoints for:

- Authentication
- Users
- Trainers
- Training Programs
- Bookings
- Memberships
- Payments

Detailed API documentation can be found in:

`docs/API.md`

---

## 🎯 Project Goals

The project was built to practice and demonstrate:

- Full-stack web development
- RESTful API development
- ASP.NET Core Web API
- Database design and integration
- Authentication and authorization
- Frontend/backend integration
- Responsive web design
- Git and GitHub workflow

---

## 👩‍💻 Author

**Naira Gamal**

Full Stack .NET Developer

GitHub: https://github.com/nairagamal
