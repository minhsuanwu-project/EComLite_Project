# Product Requirements Document

## EComLite

## Cover Page

- **Project Name:** EComLite
- **Student(s):** Min Hsuan Wu
- **Course:** CISC 593/594
- **Semester:** Summer 2026
- **Repository URL:** https://github.com/minhsuanwu-project/EComLite_Project
- **Current Branch:** master
- **Current Commit SHA:** a965859a695db8a70e90b048e1615986a58845a7
- **Current Release Version:** v1.0
- **Document Version:** 0.3
- **Last Updated:** 2026-07-24

## Revision History

| Version | Date | Git Commit | Description | Author |
|---|---|---|---|---|
| 0.1 | 2026-07-24 | bef575f9aec487618cb371bccca76e905bab7664 | Initial PRD created from repository evidence for authentication, catalog, cart, checkout, orders, tests, Docker, and CI. | GitHub Copilot |
| 0.3 | 2026-07-24 | a965859a695db8a70e90b048e1615986a58845a7 | Added Q1/Q2/Q3 behavior classification to Section 8 and supplied Preventative (Q2) and Responsive (Q3) functional requirements for every high-priority undesirable event (Risk Score >= 8), grounded in repository evidence with remaining gaps marked To Be Completed; extended the Section 14 traceability matrix accordingly. | Min Hsuan Wu |
| 0.2 | 2026-07-24 | bef575f9aec487618cb371bccca76e905bab7664 | Manual review corrections: fixed Section 6 risk-priority sort order; aligned Section 1 / Section 15 version plan with released tag `v1.0` and the Version 2 roadmap; added Checkout race-condition (UE-4.1-02) and mid-checkout session-expiry (UE-4.1-03) risks from the project Risk Register, with matching risk analysis, mitigation, functional requirements, and traceability; added Level-2 capability 4.2 Calculate Order Total with its undesirable event, risk, mitigation, functional requirement, and traceability; filled cover-page fields; added Idempotency Key and Order Status Lifecycle glossary terms. | Min Hsuan Wu |

## Table of Contents

1. [Product Vision](#1-product-vision)
2. [Product Scope](#2-product-scope)
3. [Software Capabilities](#3-software-capabilities)
4. [Undesirable Events](#4-undesirable-events)
5. [Risk Analysis](#5-risk-analysis)
6. [Risk Prioritization](#6-risk-prioritization)
7. [Risk Mitigation](#7-risk-mitigation)
8. [Functional Requirements](#8-functional-requirements)
9. [Quality Requirements](#9-quality-requirements)
10. [Performance Requirements](#10-performance-requirements)
11. [Assumptions](#11-assumptions)
12. [Constraints](#12-constraints)
13. [External Interfaces](#13-external-interfaces)
14. [Requirements Traceability Matrix](#14-requirements-traceability-matrix)
15. [Future Versions](#15-future-versions)
16. [Open Issues](#16-open-issues)
17. [Glossary](#17-glossary)

---

# 1. Product Vision

## Problem Statement

The project provides a lightweight e-commerce web application that allows users to browse available products, add them to a shopping cart, and complete checkout as an authenticated user. The current implementation focuses on a simple, testable, and containerized web experience for a small-scale storefront.

## Intended Users

- Registered customers who can sign in, browse products, place orders, and view order history.
- Developers and course instructors who can run the application locally or with Docker and execute automated tests.

## Stakeholders

- Student developer(s)
- Course instructor(s)
- Repository maintainers
- End users of the application

## Product Goals

- Provide a working ASP.NET Core Razor Pages storefront with user authentication.
- Support product discovery, cart management, and order placement.
- Persist orders and order items in a relational database.
- Support automated testing and containerized deployment.

## Major Features

- User registration and sign-in through ASP.NET Core Identity.
- Product catalog browsing, including product details.
- Session-based shopping cart operations.
- Transactional checkout flow that creates an order and clears the cart.
- Order history and order detail views for authenticated users.
- Docker-based deployment and CI through GitHub Actions.

## Planned Software Versions

- **Version 1.0 (Released, Git tag `v1.0`):** Core shopping flow — user authentication, product catalog, session-based shopping cart, transactional checkout, and order history.
- **Version 2.0 (Planned):** Admin Order Management Dashboard and order status lifecycle (Pending → Processing → Shipped → Delivered) with server-side state-transition validation and role-restricted admin routes.
- **Future Enhancements (Not Yet Planned):** Payment processing, inventory/stock management, and shipping/logistics integration.

---

# 2. Product Scope

## Included Functionality

- User registration and authentication.
- Product catalog listing and product detail viewing.
- Session-based cart management including add, remove, and clear operations.
- Checkout that creates an order with persisted order items.
- Order history and order detail retrieval for the authenticated user.
- Automated tests for cart behavior, checkout persistence, and order numbering.
- Containerized deployment with SQL Server and CI workflow.

## Excluded Functionality

- Admin order management dashboard.
- Order status lifecycle changes beyond the stored status value.
- Payment gateway integration.
- Inventory reservation or stock decrement logic.
- Shipping, refunds, or returns.
- Product review and rating system.

## Future Enhancements

- Add administrative capabilities for order management.
- Introduce richer order lifecycle states and notifications.
- Add payment processing and fulfillment workflows.
- Add inventory and stock reservation behavior.

---

# 3. Software Capabilities

## 3.1 Level-1 Capabilities

1. Manage User Accounts
2. Manage Product Catalog
3. Manage Shopping Cart
4. Manage Checkout
5. Manage Order History
6. Manage Application Deployment

## 3.2 Level-2 Capabilities

### 1. Manage User Accounts

1.1 Register User

1.2 Authenticate User

### 2. Manage Product Catalog

2.1 View Product Catalog

2.2 View Product Details

### 3. Manage Shopping Cart

3.1 Add Item To Cart

3.2 Remove Item From Cart

3.3 Clear Cart

### 4. Manage Checkout

4.1 Place Order

4.2 Calculate Order Total

### 5. Manage Order History

5.1 View Order History

5.2 View Order Details

### 6. Manage Application Deployment

6.1 Build Application Container

6.2 Run Application With Database

---

# 4. Undesirable Events

| UE ID | Level-2 Capability | Undesirable Event |
|---|---|---|
| UE-1.1-01 | Register User | Duplicate account registration is possible through repeated submissions. |
| UE-1.2-01 | Authenticate User | User authentication fails for a valid registered account. |
| UE-2.1-01 | View Product Catalog | Archived products appear in the public catalog. |
| UE-2.2-01 | View Product Details | A user can view a product that is archived or missing. |
| UE-3.1-01 | Add Item To Cart | A cart item is duplicated instead of increasing quantity. |
| UE-3.2-01 | Remove Item From Cart | Removing one item accidentally clears the entire cart. |
| UE-3.3-01 | Clear Cart | The cart cannot be cleared after a checkout attempt. |
| UE-4.1-01 | Place Order | An order is created without any order items. |
| UE-4.1-02 | Place Order | Concurrent or duplicate checkout submission (double-click or network retry) creates more than one order for a single cart. |
| UE-4.1-03 | Place Order | Authentication session expires mid-checkout, causing loss of the session-based cart contents or a failed order transaction. |
| UE-4.2-01 | Calculate Order Total | An incorrect order total is calculated, so the persisted order amount does not match the sum of the line items. |
| UE-5.1-01 | View Order History | A user can view orders from another user. |
| UE-5.2-01 | View Order Details | A user can view another user’s order details. |
| UE-6.1-01 | Build Application Container | The application container cannot be built in a clean environment. |
| UE-6.2-01 | Run Application With Database | The application cannot start successfully with the configured database container. |

---

# 5. Risk Analysis

| UE ID | Risk Statement | Likelihood | Impact | Risk Score |
|---|---|---:|---:|---:|
| UE-1.1-01 | Duplicate account registration could cause confusion and data integrity issues during account creation. | 2 | 3 | 6 |
| UE-1.2-01 | Authentication failure could prevent legitimate users from accessing the storefront. | 3 | 4 | 12 |
| UE-2.1-01 | Archived products may be shown to customers, reducing trust in the catalog. | 2 | 2 | 4 |
| UE-2.2-01 | Viewing archived or missing products could expose broken or outdated catalog pages. | 2 | 2 | 4 |
| UE-3.1-01 | Duplicate cart entries could lead to incorrect totals and poor user experience. | 3 | 3 | 9 |
| UE-3.2-01 | Removing an item could unintentionally clear the entire cart, harming checkout completion. | 2 | 3 | 6 |
| UE-3.3-01 | An inability to clear the cart could block recovery after failed or abandoned checkout. | 2 | 2 | 4 |
| UE-4.1-01 | Creating an order without items could corrupt order history and financial reporting. | 2 | 4 | 8 |
| UE-4.1-02 | Concurrent checkout requests could create duplicate order records because the checkout path does not enforce an idempotency key, corrupting order history and enabling potential duplicate charges. | 3 | 4 | 12 |
| UE-4.1-03 | Session expiry during checkout could discard the session-based cart or invalidate the request context, causing a failed checkout and lost cart state. | 4 | 3 | 12 |
| UE-4.2-01 | An incorrect order total would create a financial discrepancy between the amount shown to the user and the amount persisted with the order. | 2 | 4 | 8 |
| UE-5.1-01 | Cross-user order exposure would violate data isolation and privacy expectations. | 2 | 5 | 10 |
| UE-5.2-01 | Cross-user order detail access would create a serious security and privacy risk. | 2 | 5 | 10 |
| UE-6.1-01 | Container build failure would block deployment and testing. | 2 | 3 | 6 |
| UE-6.2-01 | Database startup failure would prevent the application from functioning in Docker. | 2 | 4 | 8 |

---

# 6. Risk Prioritization

| Priority | UE ID | Risk Score |
|---|---|---:|
| 1 | UE-1.2-01 | 12 |
| 2 | UE-4.1-02 | 12 |
| 3 | UE-4.1-03 | 12 |
| 4 | UE-5.1-01 | 10 |
| 5 | UE-5.2-01 | 10 |
| 6 | UE-3.1-01 | 9 |
| 7 | UE-4.1-01 | 8 |
| 8 | UE-4.2-01 | 8 |
| 9 | UE-6.2-01 | 8 |
| 10 | UE-1.1-01 | 6 |
| 11 | UE-3.2-01 | 6 |
| 12 | UE-6.1-01 | 6 |
| 13 | UE-2.1-01 | 4 |
| 14 | UE-2.2-01 | 4 |
| 15 | UE-3.3-01 | 4 |

---

# 7. Risk Mitigation

| UE ID | Risk Mitigation | Classification |
|---|---|---|
| UE-1.2-01 | Use ASP.NET Core Identity authentication flows and validate login behavior with automated tests. | Pure Software |
| UE-4.1-02 | Generate a unique idempotency key per checkout submission, enforce a uniqueness constraint at order creation inside the existing database transaction, and disable the Place Order button after the first submission. | Pure Software |
| UE-4.1-03 | Persist the cart to the database keyed by user ID (instead of session only), apply sliding expiration during checkout, and prompt re-authentication without discarding the cart. | Pure Software |
| UE-5.1-01 | Apply user-scoped query filters so order history and order detail queries only return the authenticated user’s records. | Pure Software |
| UE-5.2-01 | Enforce authorization checks on the order detail page and verify them with tests. | Pure Software |
| UE-4.1-01 | Require at least one order item before saving an order and test that invariant. | Pure Software |
| UE-4.2-01 | Calculate the order total from the unit-price snapshots captured at checkout time and verify with an automated test that the total equals the sum of the line totals. | Pure Software |
| UE-6.2-01 | Validate the SQL Server container and connection string configuration in Docker Compose. | Pure Software |
| UE-3.1-01 | Merge duplicate cart entries by product ID so quantity is accumulated rather than duplicated. | Pure Software |
| UE-1.1-01 | Use ASP.NET Core Identity validation and duplicate-account checks during registration. | Pure Software |
| UE-3.2-01 | Restrict remove operations to the specific product and verify cart state after each remove action. | Pure Software |
| UE-6.1-01 | Use the existing multi-stage Docker build and CI workflow to detect build regressions. | Pure Software |
| UE-2.1-01 | Filter archived products from catalog queries using the existing IsArchived property. | Pure Software |
| UE-2.2-01 | Return NotFound when the requested product is archived or missing. | Pure Software |
| UE-3.3-01 | Ensure the cart clear action is available and tested from the cart page. | Pure Software |

---

# 8. Functional Requirements

Each functional requirement is classified by behavior type:

- **Q1 (Desired):** what the system shall do.
- **Q2 (Preventative):** what the system shall not do, or how an undesirable event is prevented.
- **Q3 (Responsive):** how the system responds when an undesirable condition occurs.

Q2 and Q3 requirements are provided for the highest-priority undesirable events (Risk Score ≥ 8). Q2/Q3 coverage for lower-priority events (Risk Score ≤ 6) is **To Be Completed**.

| Requirement ID | Level-2 Capability | Q-Type | Functional Requirement |
|---|---|---|---|
| FR-1.1.1 | Register User | Q1 | The Identity system shall register a user account within one registration submission. |
| FR-1.2.1 | Authenticate User | Q1 | The authentication service shall authenticate a registered user within two seconds. |
| FR-1.2.2 | Authenticate User | Q2 | The authentication service shall not grant access when credentials are invalid or the account email is unconfirmed (`SignIn.RequireConfirmedAccount` is enabled), and shall not reveal whether an email is registered. Account lockout after repeated failed attempts is **To Be Completed**. |
| FR-1.2.3 | Authenticate User | Q3 | When authentication fails, the sign-in page shall display a generic error message and allow the user to retry within the same session. |
| FR-2.1.1 | View Product Catalog | Q1 | The web application shall display only non-archived products within two seconds of a catalog request. |
| FR-2.2.1 | View Product Details | Q1 | The product detail page shall display a valid product’s details within two seconds of a request. |
| FR-3.1.1 | Add Item To Cart | Q1 | The cart service shall add a new cart item or increase quantity within one request cycle. |
| FR-3.1.2 | Add Item To Cart | Q2 | The cart service shall not create a duplicate cart line for a product already in the cart; it shall increment the existing line quantity instead. |
| FR-3.1.3 | Add Item To Cart | Q3 | If the stored cart cannot be read, the cart service shall return an empty cart rather than failing the request. |
| FR-3.2.1 | Remove Item From Cart | Q1 | The cart service shall remove the requested product from the cart within one request cycle. |
| FR-3.3.1 | Clear Cart | Q1 | The cart service shall clear all cart contents within one request cycle. |
| FR-4.1.1 | Place Order | Q1 | The checkout process shall create an order with at least one order item within one request cycle. |
| FR-4.1.2 | Place Order | Q2 | The Checkout PageModel shall accept exactly one order per checkout submission by enforcing a unique idempotency key within the existing database transaction. |
| FR-4.1.3 | Place Order | Q3 | The Checkout PageModel shall reject a duplicate checkout submission and return the existing order confirmation instead of creating a new order. |
| FR-4.1.4 | Place Order | Q2 | The CartService shall persist cart contents keyed by user ID so the cart survives authentication session expiry during checkout. |
| FR-4.1.5 | Place Order | Q3 | The application shall redirect an expired-session user to re-authentication while preserving the checkout return URL and the persisted cart. |
| FR-4.1.6 | Place Order | Q2 | The Checkout PageModel shall not create or persist an order that contains zero order items. |
| FR-4.1.7 | Place Order | Q3 | If checkout is submitted with an empty cart, the application shall abort checkout, display a "cart is empty" message, and return the user to the cart page without creating an order. |
| FR-4.2.1 | Calculate Order Total | Q1 | The Checkout PageModel shall calculate the order total as the sum of each line item’s quantity multiplied by its unit-price snapshot within one request cycle. |
| FR-4.2.2 | Calculate Order Total | Q2 | The Checkout PageModel shall not persist an order whose total amount differs from the sum of its line totals (quantity × unit-price snapshot). |
| FR-4.2.3 | Calculate Order Total | Q3 | If a mismatch between the order total and the sum of line totals is detected during checkout, the application shall roll back the database transaction and not persist the order. |
| FR-5.1.1 | View Order History | Q1 | The order history page shall display only the authenticated user’s orders within one request cycle. |
| FR-5.1.2 | View Order History | Q2 | The order history page shall not return any order that does not belong to the authenticated user; all queries shall be filtered by the authenticated user’s ID. |
| FR-5.1.3 | View Order History | Q3 | If an unauthenticated request reaches the order history page, the application shall challenge the request (redirect to sign-in) and return no order data. |
| FR-5.2.1 | View Order Details | Q1 | The order details page shall display only the authenticated user’s order details within one request cycle. |
| FR-5.2.2 | View Order Details | Q2 | The order details page shall not display an order whose owner is not the authenticated user; the query shall match both the order ID and the authenticated user’s ID. |
| FR-5.2.3 | View Order Details | Q3 | If a requested order does not belong to the authenticated user or does not exist, the application shall return a Not Found response; an unauthenticated request shall be challenged. |
| FR-6.1.1 | Build Application Container | Q1 | The Docker build shall produce a runnable application image within one build invocation. |
| FR-6.2.1 | Run Application With Database | Q1 | The application shall start successfully with SQL Server in Docker within one container startup sequence. |
| FR-6.2.2 | Run Application With Database | Q2 | The application shall not be considered started unless a valid SQL Server connection string is configured for the environment. |
| FR-6.2.3 | Run Application With Database | Q3 | If the SQL Server database is unavailable at startup, the application shall surface a clear startup error in the logs rather than failing silently. Automatic startup retry or health-check is **To Be Completed**. |

---

# 9. Quality Requirements

- The application shall be built and tested with .NET 8 through the existing CI workflow on every push and pull request.
- The application shall provide automated unit and integration tests for cart operations, checkout persistence, and order numbering.
- The application shall persist orders and order items in a relational database using Entity Framework Core.
- The application shall support Docker-based deployment using the supplied Dockerfile and docker-compose configuration.
- The application shall support user-specific order isolation for authenticated users.
- The application shall expose a user-facing registration and sign-in experience through ASP.NET Core Identity.
- The application shall filter archived products from the public catalog. 
- The application shall maintain product price snapshots in order items so historical order pricing is preserved.

---

# 10. Performance Requirements

- The application shall respond to a product catalog request within two seconds under normal local execution conditions.
- The application shall respond to product detail and cart operations within two seconds under normal local execution conditions.
- The application shall complete a checkout request within one request cycle when the database is available.
- The application shall support the current local development workflow through Docker Compose without requiring manual database setup.
- The application shall execute the current automated test suite through dotnet test in the CI workflow. 

> **To Be Completed**: Specific numerical targets for concurrent users, peak throughput, and memory usage are not defined in the repository and require stakeholder confirmation.

---

# 11. Assumptions

- The application is intended for a single-storefront, lightweight e-commerce scenario.
- The application uses ASP.NET Core Razor Pages as the user interface framework.
- SQL Server is the intended relational database for production and local deployment.
- The current implementation uses session storage for the shopping cart.
- The application uses ASP.NET Core Identity for authentication and account management.
- The current repository does not define a payment processor or external shipping provider.

---

# 12. Constraints

- The application is implemented in C# using ASP.NET Core 8.
- The web application uses Razor Pages.
- The application uses Entity Framework Core for persistence.
- The application uses SQL Server as the relational database.
- The application uses Docker and Docker Compose for containerized deployment.
- The application uses xUnit and Moq for automated tests.
- The application uses GitHub Actions for CI.
- The local development configuration uses SQL Server connection settings from appsettings and Docker environment variables.

---

# 13. External Interfaces

## User Interfaces

- Razor Pages web interface for catalog, cart, checkout, and order history.
- Identity pages for registration and login.

## Hardware Interfaces

- Local development machine or container host.
- SQL Server database instance.

## Software Interfaces

- ASP.NET Core Identity.
- Entity Framework Core.
- ASP.NET Core session middleware.
- xUnit test runner.

## Communication Interfaces

- HTTP requests between browser and web application.
- Database connections to SQL Server.
- Docker container networking between the web app and database services.

## External Services

- Docker Hub base images used by the Docker build.
- GitHub Actions for CI execution.
- SQL Server container image for database deployment.

---

# 14. Requirements Traceability Matrix

| Requirement ID | Level-2 Capability | Q-Type | Requirement Description |
|---|---|---|---|
| FR-1.1.1 | Register User | Q1 | Register a user account within one submission. |
| FR-1.2.1 | Authenticate User | Q1 | Authenticate a registered user within two seconds. |
| FR-1.2.2 | Authenticate User | Q2 | Deny access on invalid credentials or unconfirmed accounts without revealing whether an email is registered. |
| FR-1.2.3 | Authenticate User | Q3 | Show a generic error and allow retry when authentication fails. |
| FR-2.1.1 | View Product Catalog | Q1 | Display only non-archived products within two seconds. |
| FR-2.2.1 | View Product Details | Q1 | Display a valid product’s details within two seconds. |
| FR-3.1.1 | Add Item To Cart | Q1 | Add a new cart item or increase quantity within one request cycle. |
| FR-3.1.2 | Add Item To Cart | Q2 | Increment the existing line instead of creating a duplicate cart line. |
| FR-3.1.3 | Add Item To Cart | Q3 | Return an empty cart if the stored cart cannot be read. |
| FR-3.2.1 | Remove Item From Cart | Q1 | Remove the requested product from the cart within one request cycle. |
| FR-3.3.1 | Clear Cart | Q1 | Clear all cart contents within one request cycle. |
| FR-4.1.1 | Place Order | Q1 | Create an order with at least one order item within one request cycle. |
| FR-4.1.2 | Place Order | Q2 | Accept exactly one order per checkout submission via a unique idempotency key. |
| FR-4.1.3 | Place Order | Q3 | Reject a duplicate submission and return the existing order confirmation. |
| FR-4.1.4 | Place Order | Q2 | Persist cart contents keyed by user ID so the cart survives session expiry. |
| FR-4.1.5 | Place Order | Q3 | Redirect an expired-session user to re-authentication, preserving cart and return URL. |
| FR-4.1.6 | Place Order | Q2 | Never persist an order that contains zero order items. |
| FR-4.1.7 | Place Order | Q3 | Abort checkout on an empty cart, show a message, and return to the cart page. |
| FR-4.2.1 | Calculate Order Total | Q1 | Calculate the order total as the sum of quantity times unit-price snapshot for each line item. |
| FR-4.2.2 | Calculate Order Total | Q2 | Never persist an order whose total differs from the sum of its line totals. |
| FR-4.2.3 | Calculate Order Total | Q3 | Roll back the transaction and not persist the order if a total mismatch is detected. |
| FR-5.1.1 | View Order History | Q1 | Display only the authenticated user’s orders within one request cycle. |
| FR-5.1.2 | View Order History | Q2 | Filter all order-history queries by the authenticated user’s ID; never return other users’ orders. |
| FR-5.1.3 | View Order History | Q3 | Challenge an unauthenticated request and return no order data. |
| FR-5.2.1 | View Order Details | Q1 | Display only the authenticated user’s order details within one request cycle. |
| FR-5.2.2 | View Order Details | Q2 | Match both order ID and the authenticated user’s ID; never display another user’s order. |
| FR-5.2.3 | View Order Details | Q3 | Return Not Found for a foreign or missing order; challenge an unauthenticated request. |
| FR-6.1.1 | Build Application Container | Q1 | Produce a runnable application image within one build invocation. |
| FR-6.2.1 | Run Application With Database | Q1 | Start the application successfully with SQL Server in Docker within one container startup sequence. |
| FR-6.2.2 | Run Application With Database | Q2 | Do not consider the application started without a valid SQL Server connection string. |
| FR-6.2.3 | Run Application With Database | Q3 | Surface a clear startup error in the logs if the database is unavailable at startup. |

---

# 15. Future Versions

## Version 1 (Released — Git tag `v1.0`)

- Core shopping flow: user authentication, product catalog, session-based shopping cart, transactional checkout, and order history. Already implemented; see Section 3.

## Version 2 (Next Planned Release)

- Add an Admin Order Management Dashboard for staff to view and manage all customers’ orders.
- Add an order status lifecycle (Pending → Processing → Shipped → Delivered) replacing the current single stored status value.
- Enforce server-side state-transition validation for order status changes.
- Add role-restricted admin routes secured with `[Authorize(Roles="Admin")]`.

## Version 3 (Tentative)

- Add customer notifications for order status changes.
- Add reporting and analytics for orders.

## Future Enhancements (Not Yet Planned)

- Payment gateway integration.
- Inventory reservation and stock decrement.
- Shipping, refunds, and returns / fulfillment workflows.

---

# 16. Open Issues

- The repository does not currently define explicit performance targets for concurrent users, peak throughput, or memory usage. **To Be Completed**.
- Checkout does not yet enforce an idempotency key to prevent duplicate order submission (see UE-4.1-02 mitigation). **To Be Completed**.
- The shopping cart is stored only in session and is not yet persisted to the database, so it does not survive session expiry (see UE-4.1-03 mitigation). **To Be Completed**.
- Payment provider integration is not yet implemented and is planned as a future enhancement. **To Be Completed**.
- A formal Admin Order Management workflow and order status lifecycle are not yet implemented and are planned for Version 2. **To Be Completed**.

---

# 17. Glossary

- **Cart Service:** The session-backed service that stores and manipulates the user’s current shopping cart.
- **Checkout:** The process of creating an order from cart contents and clearing the cart.
- **Idempotency Key:** A unique token attached to a checkout submission so that repeated or concurrent submissions of the same checkout produce only one order rather than duplicates.
- **Order Item:** A line entry representing one product purchase within an order.
- **Order Number:** A generated human-readable identifier for an order.
- **Order Status Lifecycle:** The planned sequence of order states (Pending → Processing → Shipped → Delivered) intended for Version 2, replacing the current single stored status value.
- **Product Catalog:** The collection of visible products available for browsing.
- **Razor Pages:** The ASP.NET Core page-based UI framework used by the application.
- **SQL Server:** The relational database used by the application.
- **xUnit:** The automated test framework used by the repository.
