# Software Test Plan and Report

## EComLite

## Cover Page and Document Metadata

- Project name: EComLite
- Student/team: Min Hsuan Wu
- Course: CISC 593/594
- Semester: Summer 2026
- Repository URL: https://github.com/minhsuanwu-project/EComLite_Project
- Current branch: master
- Current commit SHA: b482203 (working tree also contains the pending idempotency, persistent-cart, and access-control changes)
- Current release/tag: v1.0
- Document version: 0.2
- Document status: Draft / Living Verification Report
- Last updated date: 2026-07-30
- Test period covered: 2026-07-24 to 2026-07-30
- Primary test framework(s): xUnit, Moq, EF Core InMemory
- CI/CD workflow status: Configured in [.github/workflows/ci.yml](../.github/workflows/ci.yml) to run `dotnet test` on push and pull_request. The workflow previously reported success without executing tests before the test project was included in the solution; that issue has since been corrected.

## Document Revision History

| Document Version | Date | Git Commit | Sections Updated | Change Description | Author/Reviewer |
|---|---|---|---|---|---|
| 0.1 | 2026-07-29 | (pending) | All sections | Initial creation from repository evidence, PRD requirements, implementation files, and current automated tests. | GitHub Copilot |
| 0.2 | 2026-07-30 | (pending) | 1, 4, 5, 6, 7, 8, 10, 11, 12, 15, 16, 17, 19, 20, 22 | Marked UE-4.1-02 (idempotency) and UE-4.1-03 (persistent cart) as implemented and tested; added AccessControlTests, CheckoutServiceTests, PersistentCartServiceTests; recomputed real coverage figures; replaced fabricated Risk-NN identifiers with UE identifiers; added FR-1.1.1 to the traceability matrix; completed the risk-mitigation matrix for all 15 undesirable events; linked a real `dotnet test` execution-evidence file. | Min Hsuan Wu |

## Table of Contents

1. [Purpose and Scope](#1-purpose-and-scope)
2. [Verification Basis](#2-verification-basis)
3. [Test Environment](#3-test-environment)
4. [Test Strategy](#4-test-strategy)
5. [Testing Levels](#5-testing-levels)
6. [Verification of Nondeterministic and Variable Behavior](#6-verification-of-nondeterministic-and-variable-behavior)
7. [Detailed Test Case Specifications](#7-detailed-test-case-specifications)
8. [Quality Requirement Verification](#8-quality-requirement-verification)
9. [Performance Testing](#9-performance-testing)
10. [CI/CD Verification](#10-cicd-verification)
11. [Test Execution Summary](#11-test-execution-summary)
12. [Test Execution Evidence](#12-test-execution-evidence)
13. [Defect Log](#13-defect-log)
14. [Regression Test Log](#14-regression-test-log)
15. [Requirements-to-Test Traceability Matrix](#15-requirements-to-test-traceability-matrix)
16. [Risk-Mitigation Verification Matrix](#16-risk-mitigation-verification-matrix)
17. [Coverage Analysis](#17-coverage-analysis)
18. [Testability Assessment](#18-testability-assessment)
19. [Release Readiness Assessment](#19-release-readiness-assessment)
20. [Known Limitations and Verification Gaps](#20-known-limitations-and-verification-gaps)
21. [Lessons Learned](#21-lessons-learned)
22. [Planned Verification Work](#22-planned-verification-work)
23. [Glossary](#23-glossary)
24. [Appendices](#appendices)

---

# 1. Purpose and Scope

## 1.1 Purpose

This document is the living verification artifact for EComLite. It records the requirements that are currently verified, the automated tests that are implemented and executed, the current CI configuration, and the remaining verification gaps.

## 1.2 Software Under Test

- System/product name: EComLite
- Release/version: v1.0 (plus in-progress Version 2 work on order status, idempotency, and persistent cart)
- Branch: master
- Major components: ASP.NET Core 8 Razor Pages web app, EF Core data layer, ASP.NET Core Identity (with roles), session-backed cart, database-backed persistent cart, idempotent checkout/order persistence, order-status lifecycle validation, Docker deployment, GitHub Actions CI.
- Major Level-2 capabilities covered by tests: 2.1 View Product Catalog, 2.2 View Product Details, 3.1 Add Item To Cart, 3.2 Remove Item From Cart, 3.3 Clear Cart, 4.1 Place Order, 4.2 Calculate Order Total, 5.1 View Order History, 5.2 View Order Details.
- Deployment form: Web application containerized with Docker; local development also supports .NET and SQL Server.
- Known external dependencies: SQL Server, ASP.NET Core Identity, EF Core, Docker Compose, GitHub Actions.

## 1.3 Test Scope

The current effort covers cart behavior, checkout persistence, idempotent duplicate-checkout handling, database-backed cart persistence, order-total calculation, order numbering, order-status transition validation, product-detail access control, and order-detail user scoping. It does not yet include browser end-to-end testing, authentication flow testing, admin dashboard testing, or performance/load testing.

## 1.4 Out-of-Scope Items

- Admin Order Management dashboard UI and role-restricted admin routes (Version 2, backend not yet exposed through pages).
- End-to-end browser automation.
- Performance and load testing.
- Production payment, inventory, and fulfillment integrations.

## 1.5 Verification Objectives

- Functional correctness for catalog, cart, checkout, order history/detail, and order numbering.
- Satisfaction of the approved PRD functional requirements and documented risk mitigations.
- Evidence for the high-risk checkout risks identified in the PRD (duplicate submission, session expiry).
- Regression prevention for the current shopping flow.
- Reproducibility of the automated test suite through the repository and CI workflow.
- Release readiness with explicit limitations where requirements are not yet implemented.

---

# 2. Verification Basis

| Verification Basis ID | Source Artifact | Version/Commit | Purpose |
|---|---|---|---|
| VB-01 | [docs/Product_Requirements_Document.md](Product_Requirements_Document.md) | v0.4 | Authoritative source for functional, quality, and performance requirements, undesirable events, and risk mitigations. |
| VB-02 | [EComLite.Web/Program.cs](../EComLite.Web/Program.cs) | Current | Application startup, database, Identity (with roles), and service registration. |
| VB-03 | [EComLite.Web/Services/CartService.cs](../EComLite.Web/Services/CartService.cs) | Current | Session-backed cart behavior. |
| VB-04 | [EComLite.Web/Services/CheckoutService.cs](../EComLite.Web/Services/CheckoutService.cs) | Current | Idempotent order creation (UE-4.1-02). |
| VB-05 | [EComLite.Web/Services/PersistentCartService.cs](../EComLite.Web/Services/PersistentCartService.cs) | Current | Database-backed cart persistence (UE-4.1-03). |
| VB-06 | [EComLite.Web/Services/OrderStatusService.cs](../EComLite.Web/Services/OrderStatusService.cs) | Current | Server-side order status transition validation. |
| VB-07 | [EComLite.Web/Pages/Cart/Index.cshtml.cs](../EComLite.Web/Pages/Cart/Index.cshtml.cs), [EComLite.Web/Pages/Products/Details.cshtml.cs](../EComLite.Web/Pages/Products/Details.cshtml.cs), [EComLite.Web/Pages/Orders/Details.cshtml.cs](../EComLite.Web/Pages/Orders/Details.cshtml.cs) | Current | Checkout, product-detail access control, and order-detail user scoping. |
| VB-08 | EComLite.Tests (CartServiceTests, CheckoutTests, OrderNumberTests, OrderStatusTransitionTests, AccessControlTests, CheckoutServiceTests, PersistentCartServiceTests) | Current | Implemented automated tests and their coverage of requirements. |
| VB-09 | [.github/workflows/ci.yml](../.github/workflows/ci.yml) | Current | CI workflow and test execution command. |

---

# 3. Test Environment

## 3.1 Hardware Environment

- Local development machine (Windows 11) and GitHub-hosted Ubuntu runner for CI.
- Processor / Memory / Storage: To Be Completed
- Dedicated devices / boards / sensors: Not applicable

## 3.2 Software Environment

| Item | Version | Purpose | Configuration Source |
|---|---|---|---|
| Operating system | Windows 11 (local); Ubuntu latest (CI) | Build/test host | [.github/workflows/ci.yml](../.github/workflows/ci.yml) |
| .NET SDK | 8.0.200 | Build and test execution | Local `dotnet --version`, [.github/workflows/ci.yml](../.github/workflows/ci.yml) |
| ASP.NET Core | 8.0 | Web application runtime | [EComLite.Web/EComLite.Web.csproj](../EComLite.Web/EComLite.Web.csproj) |
| EF Core | 8.0.x | Data access and migrations | [EComLite.Web/EComLite.Web.csproj](../EComLite.Web/EComLite.Web.csproj) |
| SQL Server | via Docker Compose | Relational database for runtime | [docker-compose.yml](../docker-compose.yml) |
| Browser | To Be Completed | Not currently covered by automated browser tests | To Be Completed |
| Test framework | xUnit 2.5.3 | Automated tests | [EComLite.Tests/EComLite.Tests.csproj](../EComLite.Tests/EComLite.Tests.csproj) |
| Mocking framework | Moq 4.20.72 | Test doubles | [EComLite.Tests/EComLite.Tests.csproj](../EComLite.Tests/EComLite.Tests.csproj) |
| In-memory database | EF Core InMemory 8.0.0 | Integration-style data tests | [EComLite.Tests/EComLite.Tests.csproj](../EComLite.Tests/EComLite.Tests.csproj) |
| Coverage tool | coverlet.collector 6.0.0 (installed, not yet run) | Code coverage | [EComLite.Tests/EComLite.Tests.csproj](../EComLite.Tests/EComLite.Tests.csproj) |
| Container runtime | Docker / Docker Compose | Containerized app and database | [Dockerfile](../Dockerfile), [docker-compose.yml](../docker-compose.yml) |

## 3.3 Test Environment Setup

1. Clone: `git clone https://github.com/minhsuanwu-project/EComLite_Project.git`
2. Target branch/tag: `git checkout master` (or `git checkout v1.0`)
3. Restore: `dotnet restore`
4. Configure the local SQL Server connection string (not committed).
5. EF Core migrations are applied automatically at startup (`db.Database.Migrate()`), including `AddIdempotencyKeyAndPersistedCart`.
6. Build: `dotnet build EComLite.sln`
7. Test: `dotnet test EComLite.Tests/EComLite.Tests.csproj --nologo`
8. Docker: `docker compose up --build`
9. Reset: `docker compose down -v`

Do not expose secret values. The optional admin account is seeded only when `AdminUser:Email`/`AdminUser:Password` are supplied via user-secrets or environment variables.

## 3.4 Test Data and Fixtures

| Test Data ID | Description | Source/Location | Used By | Reset Procedure |
|---|---|---|---|---|
| TD-01 | In-memory product fixtures | [CheckoutTests.cs](../EComLite.Tests/CheckoutTests.cs), [AccessControlTests.cs](../EComLite.Tests/AccessControlTests.cs) | Checkout, pricing, product-access tests | New context per test |
| TD-02 | Cart service session fixtures (mocked HttpContext) | [CartServiceTests.cs](../EComLite.Tests/CartServiceTests.cs) | Cart add/remove/clear tests | New service per test |
| TD-03 | Order number fixtures (fixed dates, generated GUIDs) | [OrderNumberTests.cs](../EComLite.Tests/OrderNumberTests.cs) | Order-number formatting tests | New inputs per test |
| TD-04 | Order-status transition fixtures | [OrderStatusTransitionTests.cs](../EComLite.Tests/OrderStatusTransitionTests.cs) | Lifecycle validation tests | New inputs per test |
| TD-05 | Idempotency key / cart JSON fixtures | [CheckoutServiceTests.cs](../EComLite.Tests/CheckoutServiceTests.cs), [PersistentCartServiceTests.cs](../EComLite.Tests/PersistentCartServiceTests.cs) | Idempotency and persistent-cart tests | New context per test |

---

# 4. Test Strategy

## 4.1 Risk-Based Test Prioritization

The PRD identifies checkout and authentication risks as highest priority. Testing depth follows the PRD risk scores.

| Priority | UE ID | Risk Score | Mitigation | Related Requirement IDs | Verification Status |
|---|---|---:|---|---|---|
| 1 | UE-1.2-01 | 12 | Identity authentication flows | FR-1.2.1, FR-1.2.2, FR-1.2.3 | Planned (sign-in flow tests not yet implemented) |
| 2 | UE-4.1-02 | 12 | Unique idempotency key; duplicate-submission prevention | FR-4.1.2, FR-4.1.3 | Implemented and Passed (CheckoutServiceTests) |
| 3 | UE-4.1-03 | 12 | DB-backed cart keyed by user ID | FR-4.1.4, FR-4.1.5 | Persistence Passed (PersistentCartServiceTests); re-auth redirect Planned |
| 4 | UE-5.1-01 | 10 | User-scoped order history queries | FR-5.1.1, FR-5.1.2 | Implemented and Passed (CheckoutTests) |
| 5 | UE-5.2-01 | 10 | User-scoped order detail access | FR-5.2.1, FR-5.2.2, FR-5.2.3 | Data-scoping Passed (AccessControlTests); unauthenticated-challenge Planned |
| 6 | UE-3.1-01 | 9 | Merge duplicate cart lines by product | FR-3.1.1, FR-3.1.2 | Implemented and Passed (CartServiceTests) |
| 7 | UE-4.1-01 | 8 | Require at least one order item | FR-4.1.1, FR-4.1.6 | Implemented and Passed (CheckoutTests) |
| 8 | UE-4.2-01 | 8 | Total from unit-price snapshots | FR-4.2.1 | Positive case Passed (CheckoutTests); mismatch rejection Planned |
| 9 | UE-6.2-01 | 8 | Validate DB container/connection config | FR-6.2.1, FR-6.2.2, FR-6.2.3 | Planned |

## 4.2 Requirements-Based Testing

Tests are derived from the approved PRD functional requirements. Each automated test targets a specific FR (see Section 15). Requirements not yet implemented are recorded as Planned rather than Passed.

## 4.3 Positive Testing

Valid workflows verified: cart add/aggregate/clear, successful order persistence, preserved price snapshots, correct order total, user-scoped order history, valid product-detail display, valid forward status transitions, and idempotent order creation.

## 4.4 Negative Testing

Negative/guardrail behaviors verified: non-existent cart removal, archived-product filtering (catalog and product detail), user isolation (order history and order detail), invalid status transitions, and duplicate checkout submission.

## 4.5 Boundary Value Analysis

Explicit boundary tests (quantity overflow, maximum cart size, large order counts) remain **To Be Completed**.

## 4.6 Equivalence Class Partitioning

Representative valid/invalid classes exercised: existing vs. non-existing cart products; archived vs. non-archived products; valid vs. invalid status transitions; same-user vs. different-user order access; new vs. repeated idempotency key.

## 4.7 State-Transition and Workflow Testing

Order-status transitions are verified by OrderStatusTransitionTests (valid forward steps, skips, backward moves, same-status, unknown status, terminal state). Idempotent checkout is verified by CheckoutServiceTests. Full browser workflow and authentication transitions remain Planned.

## 4.8 Regression Testing

The full automated suite (46 tests) serves as the regression suite for catalog, cart, checkout, idempotency, persistent cart, order number, order status, and user-scoping behavior.

## 4.9 Test Independence and Repeatability

Tests are isolated: unit tests use mocked sessions or pure inputs; integration tests use a fresh EF Core InMemory database per test. No test depends on execution order or shared external state.

---

# 5. Testing Levels

## 5.1 Unit Testing

Execution evidence: [docs/test-evidence/2026-07-30-dotnet-test.md](test-evidence/2026-07-30-dotnet-test.md) (46 passed, 0 failed).

| Unit Test Group | Component | Requirement IDs | Test File | Count | Status |
|---|---|---|---|---:|---|
| CartServiceTests | CartService add/remove/clear | FR-3.1.1, FR-3.1.2, FR-3.2.1, FR-3.3.1 | [CartServiceTests.cs](../EComLite.Tests/CartServiceTests.cs) | 9 | Passed |
| OrderNumberTests | Order number formatting | FR-4.1.1 (supporting) | [OrderNumberTests.cs](../EComLite.Tests/OrderNumberTests.cs) | 5 | Passed |
| OrderStatusTransitionTests | Status transition validation (Version 2 lifecycle; risk-control test, no single PRD FR) | UE-linked (order status) | [OrderStatusTransitionTests.cs](../EComLite.Tests/OrderStatusTransitionTests.cs) | 13 | Passed |

## 5.2 Integration Testing

| Integration Test Group | Components/Interfaces | Requirement IDs | Test File | Count | Status |
|---|---|---|---|---:|---|
| CheckoutTests | EF Core + order persistence, totals, catalog filter, user scoping | FR-2.1.1, FR-4.1.1, FR-4.1.6, FR-4.2.1, FR-5.1.1, FR-5.1.2 | [CheckoutTests.cs](../EComLite.Tests/CheckoutTests.cs) | 7 | Passed |
| CheckoutServiceTests | Idempotent order creation | FR-4.1.2, FR-4.1.3 | [CheckoutServiceTests.cs](../EComLite.Tests/CheckoutServiceTests.cs) | 3 | Passed |
| PersistentCartServiceTests | DB-backed cart persistence | FR-4.1.4 | [PersistentCartServiceTests.cs](../EComLite.Tests/PersistentCartServiceTests.cs) | 4 | Passed |
| AccessControlTests | Product-detail access; order-detail user scoping | FR-2.2.1, FR-5.2.1, FR-5.2.2, FR-5.2.3 | [AccessControlTests.cs](../EComLite.Tests/AccessControlTests.cs) | 5 | Passed |

## 5.3 System Testing

Browser/UI end-to-end tests for the authentication flow, admin routes, and Docker startup are **Planned / Not Run**. No end-to-end automation exists yet.

## 5.4 Acceptance Testing

Not yet defined. **Planned / To Be Completed**.

## 5.5 Regression Testing

The 46-test automated suite is the regression suite and runs in CI on push and pull_request.

---

# 6. Verification of Nondeterministic and Variable Behavior

## 6.1 Sources of Nondeterminism

| Source ID | Component | Source of Variability | Why It Exists | Verification Risk |
|---|---|---|---|---|
| ND-01 | Order number generation | GUID-based 4-character suffix in `GenerateOrderNumber` | Suffix derives from a new GUID; deterministic per GUID but not seed-controlled | Medium |
| ND-02 | Checkout | Concurrent/duplicate submission race | Double-click or retry could create two orders | High (mitigated: see UE-4.1-02) |

## 6.2 Reproducibility Controls

- Order-number tests use fixed dates and generated GUID inputs for predictable format validation.
- Checkout, idempotency, and persistent-cart tests use EF Core InMemory with explicit fixtures.
- The idempotency key (`Order.IdempotencyKey` + filtered unique index) makes duplicate submission deterministic: the same key maps to a single order. A fake clock and controlled concurrency harness remain **To Be Completed**.

## 6.3 Property-Based or Invariant Testing

| Property Test ID | Requirement IDs | Property/Invariant | Status |
|---|---|---|---|
| PBT-01 | FR-4.2.1 | Order total equals the sum of quantity x unit-price snapshot | Not Run (property-based harness To Be Completed; invariant is checked by example in CheckoutTests) |
| PBT-02 | FR-4.1.2 | At most one order exists per (user, idempotency key) | Not Run (invariant is checked by example in CheckoutServiceTests) |

## 6.4 Statistical Testing

Not applicable to the current feature set. **To Be Completed** if randomized workloads are added.

## 6.5 Failure Reproduction

No nondeterministic failures recorded. The duplicate-submission race is now guarded by a filtered unique index; the concurrency stress test that would reproduce a raw race is **To Be Completed**.

---

# 7. Detailed Test Case Specifications

## TC-2.2-01 – Product detail hides archived and missing products

| Field | Value |
|---|---|
| Test Case ID | TC-2.2-01 |
| Test Level | Integration |
| Level-2 Capability | 2.2 View Product Details |
| Requirement ID(s) | FR-2.2.1 |
| Related UE | UE-2.2-01 (Risk Score 4) |
| Objective | A live product renders; an archived or missing product returns Not Found. |
| Environment | EF Core InMemory |
| Execution Status | Passed |

### Test Procedure
| Step | Action | Expected Result | Actual Result | Step Status | Evidence |
|---:|---|---|---|---|---|
| 1 | Request a live product | Page result | Page result | Passed | [AccessControlTests.cs](../EComLite.Tests/AccessControlTests.cs) |
| 2 | Request an archived product | Not Found | Not Found | Passed | [AccessControlTests.cs](../EComLite.Tests/AccessControlTests.cs) |
| 3 | Request a missing product id | Not Found | Not Found | Passed | [AccessControlTests.cs](../EComLite.Tests/AccessControlTests.cs) |

## TC-3.1-01 – Cart add aggregates quantity

| Field | Value |
|---|---|
| Test Case ID | TC-3.1-01 |
| Test Level | Unit |
| Level-2 Capability | 3.1 Add Item To Cart |
| Requirement ID(s) | FR-3.1.1, FR-3.1.2 |
| Related UE | UE-3.1-01 (Risk Score 9) |
| Objective | Adding an existing product increments quantity instead of duplicating a line. |
| Execution Status | Passed |

### Test Procedure
| Step | Action | Expected Result | Actual Result | Step Status | Evidence |
|---:|---|---|---|---|---|
| 1 | Add a new product | One cart line | One cart line | Passed | [CartServiceTests.cs](../EComLite.Tests/CartServiceTests.cs) |
| 2 | Add the same product again | Quantity incremented | Quantity incremented | Passed | [CartServiceTests.cs](../EComLite.Tests/CartServiceTests.cs) |

## TC-4.1-01 – Checkout persists an order with items

| Field | Value |
|---|---|
| Test Case ID | TC-4.1-01 |
| Test Level | Integration |
| Level-2 Capability | 4.1 Place Order |
| Requirement ID(s) | FR-4.1.1, FR-4.1.6 |
| Related UE | UE-4.1-01 (Risk Score 8) |
| Objective | A valid checkout creates an order and persists at least one order item. |
| Execution Status | Passed |

### Test Procedure
| Step | Action | Expected Result | Actual Result | Step Status | Evidence |
|---:|---|---|---|---|---|
| 1 | Create an order with one item | Order stored with one item | Order stored with one item | Passed | [CheckoutTests.cs](../EComLite.Tests/CheckoutTests.cs) |

## TC-4.1-02 – Idempotent duplicate checkout

| Field | Value |
|---|---|
| Test Case ID | TC-4.1-02 |
| Test Level | Integration |
| Level-2 Capability | 4.1 Place Order |
| Requirement ID(s) | FR-4.1.2, FR-4.1.3 |
| Related UE | UE-4.1-02 (Risk Score 12) |
| Objective | Submitting the same checkout twice creates one order; a different key creates a distinct order. |
| Environment | EF Core InMemory (transaction warning suppressed) |
| Execution Status | Passed |

### Test Procedure
| Step | Action | Expected Result | Actual Result | Step Status | Evidence |
|---:|---|---|---|---|---|
| 1 | Place order with key K1 | New order created | Created | Passed | [CheckoutServiceTests.cs](../EComLite.Tests/CheckoutServiceTests.cs) |
| 2 | Place order again with key K1 | Existing order returned, no duplicate | Existing returned, count = 1 | Passed | [CheckoutServiceTests.cs](../EComLite.Tests/CheckoutServiceTests.cs) |
| 3 | Place order with key K2 | Second distinct order | Two orders total | Passed | [CheckoutServiceTests.cs](../EComLite.Tests/CheckoutServiceTests.cs) |

## TC-4.1-03 – Cart persists to the database and survives session loss

| Field | Value |
|---|---|
| Test Case ID | TC-4.1-03 |
| Test Level | Integration |
| Level-2 Capability | 4.1 Place Order |
| Requirement ID(s) | FR-4.1.4 |
| Related UE | UE-4.1-03 (Risk Score 12) |
| Objective | A saved cart can be reloaded by user ID; saving overwrites rather than duplicating; clearing removes it. |
| Execution Status | Passed |

### Test Procedure
| Step | Action | Expected Result | Actual Result | Step Status | Evidence |
|---:|---|---|---|---|---|
| 1 | Save a cart for a user, then load | Same items returned | Same items | Passed | [PersistentCartServiceTests.cs](../EComLite.Tests/PersistentCartServiceTests.cs) |
| 2 | Save again for the same user | One row, overwritten | One row, updated qty | Passed | [PersistentCartServiceTests.cs](../EComLite.Tests/PersistentCartServiceTests.cs) |
| 3 | Clear the cart | Load returns empty | Empty | Passed | [PersistentCartServiceTests.cs](../EComLite.Tests/PersistentCartServiceTests.cs) |

## TC-4.2-01 – Order total equals the sum of line totals

| Field | Value |
|---|---|
| Test Case ID | TC-4.2-01 |
| Test Level | Integration |
| Level-2 Capability | 4.2 Calculate Order Total |
| Requirement ID(s) | FR-4.2.1 |
| Related UE | UE-4.2-01 (Risk Score 8) |
| Objective | The persisted total equals the sum of quantity x unit-price snapshot. |
| Execution Status | Passed |

### Test Procedure
| Step | Action | Expected Result | Actual Result | Step Status | Evidence |
|---:|---|---|---|---|---|
| 1 | Create an order and recompute total | Recalculated total matches stored total | Match | Passed | [CheckoutTests.cs](../EComLite.Tests/CheckoutTests.cs), [CheckoutServiceTests.cs](../EComLite.Tests/CheckoutServiceTests.cs) |

## TC-5.1-01 – Order history scoped to the authenticated user

| Field | Value |
|---|---|
| Test Case ID | TC-5.1-01 |
| Test Level | Integration |
| Level-2 Capability | 5.1 View Order History |
| Requirement ID(s) | FR-5.1.1, FR-5.1.2 |
| Related UE | UE-5.1-01 (Risk Score 10) |
| Objective | Order history returns only the authenticated user's orders. |
| Execution Status | Passed |

### Test Procedure
| Step | Action | Expected Result | Actual Result | Step Status | Evidence |
|---:|---|---|---|---|---|
| 1 | Query orders for user-001 | Only user-001 orders | Only user-001 | Passed | [CheckoutTests.cs](../EComLite.Tests/CheckoutTests.cs) |

## TC-5.2-01 – Order detail scoped to the owning user

| Field | Value |
|---|---|
| Test Case ID | TC-5.2-01 |
| Test Level | Integration |
| Level-2 Capability | 5.2 View Order Details |
| Requirement ID(s) | FR-5.2.1, FR-5.2.2, FR-5.2.3 |
| Related UE | UE-5.2-01 (Risk Score 10) |
| Objective | The owner can read the order; another user's request for the same order id returns nothing (the page returns Not Found). |
| Execution Status | Passed |

### Test Procedure
| Step | Action | Expected Result | Actual Result | Step Status | Evidence |
|---:|---|---|---|---|---|
| 1 | Owner requests own order | Order found | Found | Passed | [AccessControlTests.cs](../EComLite.Tests/AccessControlTests.cs) |
| 2 | Different user requests the order | Nothing returned (Not Found) | Null | Passed | [AccessControlTests.cs](../EComLite.Tests/AccessControlTests.cs) |

## TC-ORD-01 – Order number format

| Field | Value |
|---|---|
| Test Case ID | TC-ORD-01 |
| Test Level | Unit |
| Level-2 Capability | 4.1 Place Order (supporting) |
| Requirement ID(s) | FR-4.1.1 (supporting) |
| Objective | Order number follows `ORD-YYYYMMDD-XXXX` with an uppercase 4-character suffix. |
| Execution Status | Passed |

### Test Procedure
| Step | Action | Expected Result | Actual Result | Step Status | Evidence |
|---:|---|---|---|---|---|
| 1 | Generate an order number | Correct format and date part | Correct | Passed | [OrderNumberTests.cs](../EComLite.Tests/OrderNumberTests.cs) |

## TC-STATUS-01 – Order status transition validation

| Field | Value |
|---|---|
| Test Case ID | TC-STATUS-01 |
| Test Level | Unit |
| Level-2 Capability | Version 2 order status lifecycle |
| Requirement ID(s) | Risk-control test (no single PRD functional requirement; verifies Version 2 lifecycle from the proposal) |
| Objective | Valid forward transitions are allowed; skips, backward moves, same-status, and unknown statuses are rejected. |
| Execution Status | Passed |

### Test Procedure
| Step | Action | Expected Result | Actual Result | Step Status | Evidence |
|---:|---|---|---|---|---|
| 1 | Validate each transition case | Allowed/rejected per lifecycle rules | Matches | Passed | [OrderStatusTransitionTests.cs](../EComLite.Tests/OrderStatusTransitionTests.cs) |

## TC-IDEMPOTENCY-SYS-01 – End-to-end duplicate submission through the page (Planned)

| Field | Value |
|---|---|
| Test Case ID | TC-IDEMPOTENCY-SYS-01 |
| Test Level | System |
| Requirement ID(s) | FR-4.1.2, FR-4.1.3 |
| Related UE | UE-4.1-02 (Risk Score 12) |
| Objective | Verify the full page-level double-submit path returns the existing order confirmation. |
| Execution Status | Planned (service-level behavior is covered by TC-4.1-02; browser-level test not yet implemented) |

## TC-CARTPERSIST-SYS-01 – End-to-end cart survival across re-authentication (Planned)

| Field | Value |
|---|---|
| Test Case ID | TC-CARTPERSIST-SYS-01 |
| Test Level | System |
| Requirement ID(s) | FR-4.1.5 |
| Related UE | UE-4.1-03 (Risk Score 12) |
| Objective | Verify that an expired session redirects to re-authentication and the cart is restored afterward. |
| Execution Status | Planned (persistence covered by TC-4.1-03; redirect/return-URL flow relies on the Identity framework and is not yet automated) |

---

# 8. Quality Requirement Verification

## Reliability
| Quality Requirement ID | Verification Method | Measurement | Acceptance Criterion | Result | Status |
|---|---|---|---|---|---|
| QR-01 | Automated suite | 46 tests, 0 failures | Core flows stable | 46/46 passed | Passed |

## Security
| Quality Requirement ID | Verification Method | Measurement | Acceptance Criterion | Result | Status |
|---|---|---|---|---|---|
| QR-02 | User-scoped data-access tests | Order history + order detail scoping | No cross-user data access | Verified at data layer | Passed |
| QR-03 | Page authorization inspection | `[Authorize]` on cart/orders pages | Unauthenticated users challenged | Attribute present; automated challenge test not yet added | Planned |

## Maintainability
| Quality Requirement ID | Verification Method | Measurement | Acceptance Criterion | Result | Status |
|---|---|---|---|---|---|
| QR-04 | Service separation + unit tests | CheckoutService, PersistentCartService, OrderStatusService | Core logic testable in isolation | Verified | Passed |

## Portability / Deployment
| Quality Requirement ID | Verification Method | Measurement | Acceptance Criterion | Result | Status |
|---|---|---|---|---|---|
| QR-05 | Docker/Compose configuration | Dockerfile, docker-compose.yml | App builds and starts with Compose | Configured; runtime execution not captured | Not Run |

---

# 9. Performance Testing

No dedicated performance harness exists. PRD performance targets remain unverified by executed benchmarks.

| Performance Test ID | Requirement ID | Metric | Acceptance Threshold | Status |
|---|---|---|---|---|
| PT-01 | FR-2.1.1, FR-2.2.1 | Response time | Under 2 seconds (normal local) | Planned |
| PT-02 | FR-4.1.1 | Checkout response | One request cycle when DB available | Planned |

---

# 10. CI/CD Verification

## 10.1 Workflow Configuration

| CI/CD Control | Configuration | File/Location | Status |
|---|---|---|---|
| Trigger | push and pull_request | [.github/workflows/ci.yml](../.github/workflows/ci.yml) | Configured |
| Runtime | .NET 8.0.x | [.github/workflows/ci.yml](../.github/workflows/ci.yml) | Configured |
| Restore | dotnet restore | [.github/workflows/ci.yml](../.github/workflows/ci.yml) | Configured |
| Test execution | dotnet test | [.github/workflows/ci.yml](../.github/workflows/ci.yml) | Configured and executing |

## 10.2 CI Execution Evidence

Local execution is captured in [docs/test-evidence/2026-07-30-dotnet-test.md](test-evidence/2026-07-30-dotnet-test.md) (46 passed, 0 failed, .NET 8.0.200).

| Run Date | Branch/PR | Workflow | Tests Run | Result | Evidence |
|---|---|---|---:|---|---|
| 2026-07-30 | master (local) | dotnet test | 46 | 46 passed / 0 failed | [test-evidence/2026-07-30-dotnet-test.md](test-evidence/2026-07-30-dotnet-test.md) |
| To Be Completed | CI run | GitHub Actions | To Be Completed | To Be Completed | Link the GitHub Actions run URL |

## 10.3 CI/CD Limitations

Prior finding: the workflow reported success without running tests because the test project was not in the solution. Corrected by adding the test project to the solution so `dotnet test` executes it. A preserved GitHub Actions run link is still **To Be Completed**.

---

# 11. Test Execution Summary

| Test Level | Planned | Implemented | Executed | Passed | Failed | Blocked | Deferred |
|---|---:|---:|---:|---:|---:|---:|---:|
| Unit | 27 | 27 | 27 | 27 | 0 | 0 | 0 |
| Integration | 19 | 19 | 19 | 19 | 0 | 0 | 0 |
| System | 2 | 0 | 0 | 0 | 0 | 0 | 2 |
| Acceptance | 0 | 0 | 0 | 0 | 0 | 0 | 0 |
| Performance | 2 | 0 | 0 | 0 | 0 | 0 | 2 |
| Property-Based | 2 | 0 | 0 | 0 | 0 | 0 | 2 |
| Regression | 46 | 46 | 46 | 46 | 0 | 0 | 0 |

- Overall pass rate: 46/46 executed tests passed.
- Unresolved critical defects: None.
- Unresolved high-risk requirements: FR-1.2.1/1.2.2/1.2.3 (authentication) have no automated tests; FR-4.1.5 (re-auth redirect) and FR-4.2.2/4.2.3 (total-mismatch rejection) are not yet implemented/tested.
- Release recommendation: Ready with Known Limitations.
- Test completion date: 2026-07-30.

---

# 12. Test Execution Evidence

| Evidence ID | Test Case ID(s) | Evidence Type | Location |
|---|---|---|---|
| EV-00 | All | Execution log (46 passed) | [test-evidence/2026-07-30-dotnet-test.md](test-evidence/2026-07-30-dotnet-test.md) |
| EV-01 | TC-3.1-01 | Test source | [CartServiceTests.cs](../EComLite.Tests/CartServiceTests.cs) |
| EV-02 | TC-4.1-01, TC-4.2-01, TC-5.1-01 | Test source | [CheckoutTests.cs](../EComLite.Tests/CheckoutTests.cs) |
| EV-03 | TC-ORD-01 | Test source | [OrderNumberTests.cs](../EComLite.Tests/OrderNumberTests.cs) |
| EV-04 | TC-STATUS-01 | Test source | [OrderStatusTransitionTests.cs](../EComLite.Tests/OrderStatusTransitionTests.cs) |
| EV-05 | TC-4.1-02, TC-4.2-01 | Test source | [CheckoutServiceTests.cs](../EComLite.Tests/CheckoutServiceTests.cs) |
| EV-06 | TC-4.1-03 | Test source | [PersistentCartServiceTests.cs](../EComLite.Tests/PersistentCartServiceTests.cs) |
| EV-07 | TC-2.2-01, TC-5.2-01 | Test source | [AccessControlTests.cs](../EComLite.Tests/AccessControlTests.cs) |
| EV-08 | CI configuration | Workflow file | [.github/workflows/ci.yml](../.github/workflows/ci.yml) |

---

# 13. Defect Log

| Defect ID | Date Found | Test Case ID | Requirement ID | Description | Severity | Status | Fix Commit | Regression Test |
|---|---|---|---|---|---|---|---|---|
| D-01 | 2026-07-28 | CI workflow | N/A | CI reported success without running tests because the test project was absent from the solution. | High | Closed | Test project registered in `EComLite.sln`; CI now runs `dotnet test` | Whole suite runs in CI |

No test failures are recorded in the current suite (46 passed, 0 failed).

---

# 14. Regression Test Log

| Regression ID | Defect ID | Requirement ID | Test Case ID | Failure Prevented | Latest Result |
|---|---|---|---|---|---|
| REG-01 | None | FR-3.1.1, FR-3.1.2, FR-4.1.1, FR-4.2.1 | TC-3.1-01, TC-4.1-01, TC-4.2-01 | Regression in cart, checkout persistence, and total calculation | Passed |
| REG-02 | None | FR-4.1.2, FR-4.1.3 | TC-4.1-02 | Duplicate order creation | Passed |
| REG-03 | None | FR-4.1.4 | TC-4.1-03 | Loss of cart on session expiry | Passed |
| REG-04 | D-01 | N/A | Whole suite | CI passing without executing tests | Passed |

---

# 15. Requirements-to-Test Traceability Matrix

| Requirement ID | Level-2 Capability | Requirement Summary | UE | Test Case IDs | Latest Status |
|---|---|---|---|---|---|
| FR-1.1.1 | Register User | Register a user account | UE-1.1-01 | (none) | Planned |
| FR-1.2.1 | Authenticate User | Authenticate a registered user | UE-1.2-01 | (none) | Planned |
| FR-1.2.2 | Authenticate User | Deny invalid/unconfirmed login | UE-1.2-01 | (none) | Planned |
| FR-1.2.3 | Authenticate User | Generic error + retry on failure | UE-1.2-01 | (none) | Planned |
| FR-2.1.1 | View Product Catalog | Show only non-archived products | UE-2.1-01 | TC (CheckoutTests archived filter) | Passed |
| FR-2.2.1 | View Product Details | Show a valid product; hide archived/missing | UE-2.2-01 | TC-2.2-01 | Passed |
| FR-3.1.1 | Add Item To Cart | Add or increase quantity | UE-3.1-01 | TC-3.1-01 | Passed |
| FR-3.1.2 | Add Item To Cart | Increment instead of duplicating | UE-3.1-01 | TC-3.1-01 | Passed |
| FR-3.1.3 | Add Item To Cart | Return empty cart if unreadable | UE-3.1-01 | (none) | Planned |
| FR-3.2.1 | Remove Item From Cart | Remove requested product | UE-3.2-01 | TC-3.1-01 (CartServiceTests) | Passed |
| FR-3.3.1 | Clear Cart | Clear all cart contents | UE-3.3-01 | TC-3.1-01 (CartServiceTests) | Passed |
| FR-4.1.1 | Place Order | Create order with >= 1 item | UE-4.1-01 | TC-4.1-01, TC-ORD-01 | Passed |
| FR-4.1.2 | Place Order | Enforce unique idempotency key | UE-4.1-02 | TC-4.1-02 | Passed |
| FR-4.1.3 | Place Order | Reject duplicate; return existing order | UE-4.1-02 | TC-4.1-02 | Passed |
| FR-4.1.4 | Place Order | Persist cart by user ID | UE-4.1-03 | TC-4.1-03 | Passed |
| FR-4.1.5 | Place Order | Redirect expired session to re-auth, keep cart | UE-4.1-03 | TC-CARTPERSIST-SYS-01 | Planned |
| FR-4.1.6 | Place Order | Never persist a zero-item order | UE-4.1-01 | TC-4.1-01 | Passed |
| FR-4.1.7 | Place Order | Abort empty-cart checkout with a message | UE-4.1-01 | (none) | Planned |
| FR-4.2.1 | Calculate Order Total | Total = sum of line totals | UE-4.2-01 | TC-4.2-01 | Passed |
| FR-4.2.2 | Calculate Order Total | Never persist a mismatched total | UE-4.2-01 | (none) | Planned |
| FR-4.2.3 | Calculate Order Total | Roll back on total mismatch | UE-4.2-01 | (none) | Planned |
| FR-5.1.1 | View Order History | Show only the user's orders | UE-5.1-01 | TC-5.1-01 | Passed |
| FR-5.1.2 | View Order History | Never return other users' orders | UE-5.1-01 | TC-5.1-01 | Passed |
| FR-5.1.3 | View Order History | Challenge unauthenticated requests | UE-5.1-01 | (none) | Planned |
| FR-5.2.1 | View Order Details | Show only the user's order details | UE-5.2-01 | TC-5.2-01 | Passed |
| FR-5.2.2 | View Order Details | Never display another user's order | UE-5.2-01 | TC-5.2-01 | Passed |
| FR-5.2.3 | View Order Details | Not Found for foreign/missing order | UE-5.2-01 | TC-5.2-01 | Passed |
| FR-6.1.1 | Build Application Container | Produce a runnable image | UE-6.1-01 | (none) | Planned |
| FR-6.2.1 | Run Application With Database | Start with SQL Server in Docker | UE-6.2-01 | (none) | Planned |
| FR-6.2.2 | Run Application With Database | Require a valid connection string | UE-6.2-01 | (none) | Planned |
| FR-6.2.3 | Run Application With Database | Surface a clear startup error | UE-6.2-01 | (none) | Planned |

Traceability notes: 17 of 31 functional requirements are verified by passing tests; the remaining 14 are Planned. TC-STATUS-01 is a risk-control test for the Version 2 order status lifecycle and does not map to a single approved PRD functional requirement.

---

# 16. Risk-Mitigation Verification Matrix

| UE ID | Risk Score | Mitigation | Classification | Implementation Evidence | Verification Test | Result |
|---|---:|---|---|---|---|---|
| UE-1.2-01 | 12 | Identity authentication flows | Pure Software | `Program.cs` Identity config | (none) | Planned |
| UE-4.1-02 | 12 | Unique idempotency key; duplicate prevention | Pure Software | `CheckoutService`, `Order.IdempotencyKey` filtered unique index | TC-4.1-02 | Passed |
| UE-4.1-03 | 12 | Persist cart by user ID | Pure Software | `PersistentCartService`, `PersistedCart` table | TC-4.1-03 | Passed |
| UE-5.1-01 | 10 | User-scoped order history queries | Pure Software | `Orders/Index` UserId filter | TC-5.1-01 | Passed |
| UE-5.2-01 | 10 | User-scoped order detail access | Pure Software | `Orders/Details` OrderId+UserId filter, NotFound, Challenge | TC-5.2-01 | Passed |
| UE-3.1-01 | 9 | Merge duplicate cart lines by product | Pure Software | `CartService.AddItem` | TC-3.1-01 | Passed |
| UE-4.1-01 | 8 | Require >= 1 order item | Pure Software | Checkout empty-cart guard | TC-4.1-01 | Passed |
| UE-4.2-01 | 8 | Total from unit-price snapshots | Pure Software | Order total computed from snapshots | TC-4.2-01 | Passed (positive) |
| UE-6.2-01 | 8 | Validate DB container/connection config | Pure Software | docker-compose service + connection env | (none) | Planned |
| UE-1.1-01 | 6 | Identity duplicate-account checks | Pure Software | ASP.NET Core Identity uniqueness | (none) | Planned |
| UE-3.2-01 | 6 | Restrict remove to specific product | Pure Software | `CartService.Remove` | TC-3.1-01 (CartServiceTests) | Passed |
| UE-6.1-01 | 6 | Multi-stage Docker build + CI | Pure Software | Dockerfile, ci.yml | (none) | Planned |
| UE-2.1-01 | 4 | Filter archived products from catalog | Pure Software | `Products/Index` `!IsArchived` | CheckoutTests archived filter | Passed |
| UE-2.2-01 | 4 | Return NotFound for archived/missing product | Pure Software | `Products/Details` NotFound | TC-2.2-01 | Passed |
| UE-3.3-01 | 4 | Cart clear action available | Pure Software | `CartService.Clear` | TC-3.1-01 (CartServiceTests) | Passed |

---

# 17. Coverage Analysis

| Coverage Type | Covered | Total | Percentage | Method | Known Gap |
|---|---:|---:|---:|---|---|
| Requirements coverage | 17 | 31 | 55% | Section 15 traceability | Authentication, empty-cart response, total-mismatch, re-auth redirect, and deployment requirements remain untested |
| Level-2 capability coverage | 9 | 13 | 69% | Section 15 + tests | Register User, Authenticate User, Build Container, Run With Database not covered |
| Risk coverage | 11 | 15 | 73% | Section 16 | UE-1.1-01, UE-1.2-01, UE-6.1-01, UE-6.2-01 not verified |
| Mitigation coverage | 11 | 15 | 73% | Section 16 | Same four mitigations not verified |
| Code coverage | To Be Completed | To Be Completed | To Be Completed | coverlet.collector installed but not yet run | Run `dotnet test --collect:"XPlat Code Coverage"` |

Code coverage is supplementary; high code coverage alone does not demonstrate requirements or system coverage.

---

# 18. Testability Assessment

| Component | Testability Issue | Impact | Improvement | Benefit |
|---|---|---|---|---|
| CheckoutService | Extracted idempotent order creation into a service | High-risk duplicate-submission behavior is now unit/integration testable | Done | Deterministic regression tests (TC-4.1-02) |
| PersistentCartService | Cart persistence isolated behind a service | Session-expiry survival is now testable without a browser | Done | TC-4.1-03 |
| OrderStatusService | Pure static validation method | State-machine rules testable in isolation | Done | TC-STATUS-01 |
| Order number generation | GUID suffix used directly | Format tested, but GUID-driven uniqueness not semantically tested | Planned: deterministic suffix strategy | Improved reproducibility |
| Authentication flow | Requires browser/Identity harness | Sign-in behavior not automated | Planned: integration harness with mocked Identity | Coverage of UE-1.2-01 |

---

# 19. Release Readiness Assessment

- Tested branch: master (working tree with pending Version 2 changes)
- Release/tag: v1.0
- Total executed tests: 46
- Failures: 0
- Blocked tests: 0
- Unresolved defects: None (D-01 closed)
- Unmet/untested requirements: authentication (FR-1.2.x), empty-cart response (FR-4.1.7), total-mismatch rejection (FR-4.2.2/4.2.3), re-auth redirect (FR-4.1.5), deployment (FR-6.x)
- CI status: Configured and executing; a preserved run link is To Be Completed
- Known limitations: no browser automation, no performance harness, no admin dashboard UI

Recommendation: **Ready with Known Limitations.**

Justification: The suite verifies catalog, cart, checkout, idempotency, persistent cart, totals, order numbering, order-status validation, and user scoping (46/46 passing). The remaining gaps are authentication flow tests, deployment tests, and browser-level end-to-end tests, all documented as Planned.

---

# 20. Known Limitations and Verification Gaps

| Gap | Impact | Planned Resolution |
|---|---|---|
| No automated authentication (sign-in) tests | UE-1.2-01 unverified | Add integration tests for login success/failure |
| Total-mismatch rejection (FR-4.2.2/4.2.3) not implemented | A corrupted total could persist | Add a total-consistency check + tests |
| Empty-cart checkout response (FR-4.1.7) not directly tested | Guard exists but is unverified by test | Add a page-level test |
| Re-auth redirect preserving return URL (FR-4.1.5) not automated | Relies on framework default | Add a browser/integration test |
| No browser end-to-end tests | User-visible workflows partly unverified | Add UI automation for checkout and auth |
| No performance tests | Response-time requirements unverified | Add benchmarks with a defined workload |
| Code coverage not measured | Coverage unknown | Run coverlet and record the report |
| Admin dashboard UI absent | Admin route authorization unverified | Implement Version 2 admin pages and tests |

---

# 21. Lessons Learned

- CI previously reported success without executing tests because the test project was not in the solution. Fixed by registering the test project so `dotnet test` runs it (D-01).
- The PRD risk analysis correctly prioritized duplicate submission and session expiry as the highest risks; both are now implemented and covered by tests (TC-4.1-02, TC-4.1-03).
- Extracting `CheckoutService` and `PersistentCartService` from the page model made the high-risk behaviors testable without a browser, confirming that testability improvements come from isolating logic behind services.
- EF Core InMemory does not enforce unique indexes or support real transactions, so the idempotency test verifies the check-existing path while the database unique index remains the concurrency backstop, verified only against real SQL Server.

---

# 22. Planned Verification Work

| Priority | Planned Work | Related Requirement/Risk | Target | Status |
|---|---|---|---|---|
| 1 | Automated authentication (sign-in) tests | FR-1.2.x, UE-1.2-01 | v2.0 | Planned |
| 2 | Total-consistency check + tests | FR-4.2.2, FR-4.2.3, UE-4.2-01 | v2.0 | Planned |
| 3 | Admin dashboard UI + role authorization tests | UE-5.2-01, admin routes | v2.0 | Planned |
| 4 | Browser end-to-end tests (checkout, auth, re-auth cart restore) | FR-4.1.5, FR-4.1.7 | v2.0 | Planned |
| 5 | Record code coverage and a CI run link | Coverage, CI evidence | v2.0 | Planned |

---

# 23. Glossary

- Idempotency key: A unique token attached to a checkout so a repeated or concurrent submission produces only one order.
- Order status lifecycle: The Version 2 sequence Pending -> Processing -> Shipped -> Delivered.
- Persistent cart: A database-stored cart keyed by user ID so it survives session expiry.
- Price snapshot: The unit price captured on the order line at checkout time.
- Regression test: A test that re-verifies existing behavior after a change.
- System test: An end-to-end test validating a user-visible workflow.

---

# Appendices

## Appendix A – Test Commands

- All tests: `dotnet test EComLite.Tests/EComLite.Tests.csproj --nologo`
- With code coverage: `dotnet test --collect:"XPlat Code Coverage"`
- Build: `dotnet build EComLite.sln`
- Docker startup: `docker compose up --build`

## Appendix B – Coverage Reports

- To Be Completed (run coverlet).

## Appendix C – CI/CD Logs

- To Be Completed (link a GitHub Actions run).

## Appendix D – Execution Evidence

- [docs/test-evidence/2026-07-30-dotnet-test.md](test-evidence/2026-07-30-dotnet-test.md) — full list of the 46 tests and their outcomes.

## Appendix E – Deferred Tests

- Authentication flow tests
- Total-mismatch rejection tests
- Browser end-to-end tests
- Performance/load tests
- Admin-route authorization tests
