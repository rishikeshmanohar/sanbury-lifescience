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
- Razorpay INR checkout lifecycle:
  - Server-side Razorpay order creation
  - Checkout signature verification
  - Webhook signature verification
  - Payment status transitions before order confirmation and invoice sync

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
- `Views/Orders/Checkout.cshtml` - Razorpay-enabled checkout page
- `Controllers/OrdersController.cs` - Checkout, callback, and webhook lifecycle actions
- `Services/RazorpayPaymentGateway.cs` - Razorpay API and signature validation logic
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

## Razorpay Setup

1. Prefer secure secrets (recommended):

```bash
dotnet user-secrets set "Razorpay:KeyId" "rzp_test_xxxxx"
dotnet user-secrets set "Razorpay:KeySecret" "your_real_secret"
dotnet user-secrets set "Razorpay:WebhookSecret" "your_real_webhook_secret"
```

2. Or set environment variables:

```bash
$env:RAZORPAY_KEY_ID="rzp_test_xxxxx"
$env:RAZORPAY_KEY_SECRET="your_real_secret"
$env:RAZORPAY_WEBHOOK_SECRET="your_real_webhook_secret"
```

3. Keep `appsettings*.json` as non-secret defaults:

```json
"Razorpay": {
  "BaseUrl": "https://api.razorpay.com",
  "KeyId": "",
  "KeySecret": "",
  "WebhookSecret": ""
}
```

4. Configure callback URL in Razorpay Checkout flow:

- `https://<your-domain>/Orders/RazorpayCallback`

5. Configure webhook endpoint in Razorpay dashboard:

- `https://<your-domain>/Orders/RazorpayWebhook`

6. Subscribe webhook events at minimum:

- `payment.captured`
- `payment.failed`

## Payment Lifecycle

- Checkout creates an internal order with pending payment state.
- Backend creates a Razorpay order in INR and stores `gateway_order_id`.
- Payment callback verifies checkout signature and capture state.
- Only after verified payment success:
  - `PaymentStatus` -> `Paid`
  - `Status` -> `Confirmed`
  - Invoice sync runs
  - Cart is cleared for the current user
- Failed or unverified payments are retained with failure/pending states for safe retry/audit.

## Notes

- Currency display in UI currently uses `INR`.
- Cart uses session state and is cleared after verified successful payment.
- This repo contains local SQLite files; avoid committing transient DB lock files (`*.db-wal`, `*.db-shm`).
