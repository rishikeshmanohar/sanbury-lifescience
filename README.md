# Sanbury LifeScience Web

ASP.NET Core MVC web application for Sanbury LifeScience with a branded orange-and-white frontend and pharmacy ordering flow.

## What Is Included

- Orange-and-white marketing homepage with:
  - Hero section
  - News strip
  - Company story and leadership section
  - Trust metrics section
  - Product portfolio cards with add-to-cart
- Sticky top navigation with dropdown headline sections:
  - About Us
  - Science & Innovation
  - Career
- Large multi-column footer section with quick links, office info, and legal links
- Branded account pages:
  - Login
  - Register
- Branded cart page:
  - Item cards
  - GST and total breakdown
  - Checkout summary panel

## Tech Stack

- .NET 8
- ASP.NET Core MVC
- Razor Views
- Bootstrap (base) + custom CSS/JS theme
- Session-based cart service
- SQLite database

## Key Files

- `Views/Home/Index.cshtml` - Main landing page
- `Views/Shared/_LandingLayout.cshtml` - Shared branded layout and navigation
- `Views/Account/Login.cshtml` - Branded login page
- `Views/Account/Register.cshtml` - Branded registration page
- `Views/Cart/Index.cshtml` - Branded cart page
- `wwwroot/css/landing.css` - Main custom theme stylesheet
- `wwwroot/js/landing.js` - Navigation and reveal interactions

## Run Locally

1. Restore and build:

```bash
dotnet restore
dotnet build
```

2. Run:

```bash
dotnet run
```

3. Open the local URL shown in terminal (typically `https://localhost:xxxx` or `http://localhost:xxxx`).

## Notes

- Currency display in UI currently uses `INR`.
- Cart uses session state and is cleared per session lifecycle.
- This repo contains local SQLite files; avoid committing transient DB lock files (`*.db-wal`, `*.db-shm`).
