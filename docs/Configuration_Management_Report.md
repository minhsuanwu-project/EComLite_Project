# Configuration Management Report

## EComLite — CISC 594 Final Project

| Field | Value |
|---|---|
| Project | EComLite (lightweight e-commerce web application) |
| Student | Min Hsuan Wu (Allen Wu) |
| Instructor | Khalid Lateef, Ph.D., PMP |
| Course | CISC 594 |
| Semester | Summer 2026 |
| Repository | https://github.com/minhsuanwu-project/EComLite_Project |
| Version control system | Git / GitHub (public; instructor has read access) |
| Current release tag | v1.0 |
| Total commits | 23 |
| Report date | 2026-08-02 |

---

## 1. Purpose

This report describes how configuration management (CM) was performed on the EComLite
project: which version control system was used, how the repository and baselines are
organized, the formal change control process used to introduce every change, how releases
are tagged, and how automated verification protects the baseline.

The CM approach implements Section 4.1 (Version Control and Change Control) of the
EComLite Project Proposal.

---

## 2. Version Control System

- **Tool:** Git, hosted on GitHub.
- **Repository:** https://github.com/minhsuanwu-project/EComLite_Project (public).
- **Instructor access:** The repository is public, so the instructor has read access to all
  source code, history, branches, pull requests, tags, and CI runs.
- **Controlled items:** application source (`EComLite.Web/`), automated tests
  (`EComLite.Tests/`), database migrations (`EComLite.Web/Migrations/`), CI workflow
  (`.github/workflows/ci.yml`), container configuration (`Dockerfile`, `docker-compose.yml`),
  solution file (`EComLite.sln`), and all engineering documents (`docs/`).
- **Excluded items:** build output (`bin/`, `obj/`), IDE state, local secrets, Office lock
  files, and local tool directories are excluded through `.gitignore` so that only
  authoritative source and documents are baselined.

---

## 3. Repository and Branching Strategy

`master` is the controlled baseline. It always holds tested, working code. No new
development is committed directly to `master`; every change arrives through a branch and a
pull request.

Branch naming follows intent:

| Branch | Purpose |
|---|---|
| `master` | Controlled baseline / integration branch |
| `feature/<name>` | New functionality or code change |
| `docs/<name>` | Documentation-only change |

Branches created during the project:

| Branch | Purpose |
|---|---|
| `feature/cm-exercise-demo` | Configuration management exercise demonstration |
| `feature/v2-order-status` | Role seeding, order status lifecycle, transition validation |
| `feature/v2-idempotency-cart` | Idempotent checkout, DB-backed persistent cart, access-control tests |
| `docs/risk-register-w4` | Risk register update (Week 4 re-scoring) |

---

## 4. Formal Change Control Process

Every change to the baseline follows the same six steps.

1. **Identify the change.** The change is traced to an approved source: a requirement in the
   PRD (`docs/Product_Requirements_Document.md`), a risk in the Risk Register, or a defect.
2. **Create a branch.** Work starts on a new branch off `master`
   (`git checkout -b feature/<name>`). `master` is never edited directly.
3. **Implement and commit.** Commits use the format defined in the proposal:
   `[type] short description`, where type is `feat`, `fix`, `test`, `docs`, or `chore`.
4. **Verify on the branch.** The change is built and tested locally
   (`dotnet build`, `dotnet test`) before it is proposed for merge. GitHub Actions also runs
   the suite automatically when the branch is pushed.
5. **Review via pull request.** A pull request is opened against `master`. The PR is the
   formal change request: it states what changed, why, and what testing was performed. The
   diff and the CI result are reviewed before approval.
6. **Merge and close.** After approval and a passing CI run, the PR is merged into `master`.
   The merge commit records the change in the baseline history. The feature branch is then
   deleted.

### 4.1 Change control record

Every change to the baseline is traceable to a pull request:

| PR | Date | Branch | Change | Type |
|---|---|---|---|---|
| [#1](https://github.com/minhsuanwu-project/EComLite_Project/pull/1) | 2026-07-13 | `feature/cm-exercise-demo` | CM exercise demonstration (`config.json`) | chore |
| [#2](https://github.com/minhsuanwu-project/EComLite_Project/pull/2) | 2026-07-25 | `feature/v2-order-status` | Role seeding, order status lifecycle, server-side transition validation | feat |
| [#3](https://github.com/minhsuanwu-project/EComLite_Project/pull/3) | 2026-07-29 | `feature/v2-idempotency-cart` | Idempotent checkout, DB-backed persistent cart, access-control tests, PRD and test-plan updates | feat |
| [#4](https://github.com/minhsuanwu-project/EComLite_Project/pull/4) | 2026-08-02 | `docs/risk-register-w4` | Risk register re-scoring (R1, R2, R5) and Week 4 log entries | docs |

### 4.2 Baseline history

The merge structure below is taken directly from `git log --graph --oneline`, showing that
feature work is developed on branches and integrated into `master` through merge commits:

```
*   b08daf3 Merge pull request #4 from minhsuanwu-project/docs/risk-register-w4
|\
| * 0c46451 [docs] update risk register: R1/R2 mitigated, R5 partial, add W4 weekly log
|/
*   d1e16d3 Merge pull request #3 from minhsuanwu-project/feature/v2-idempotency-cart
|\
| * 8542f9f [feat] add idempotent checkout and DB-backed persistent cart; add access-control tests
|/
* b482203 [docs] add R6 (CI reported success without running tests) and W3 log entries
* 0b1c290 [test] add order status transition tests; register test project in solution
* 6667a91 [docs] add weekly risk log; re-score R3/R4 after v2, add R5
*   389e4c2 Merge pull request #2 from minhsuanwu-project/feature/v2-order-status
|\
| * e09f496 [feat] add role seeding, order status lifecycle, and transition validation
|/
* cf63885 Add Q2/Q3 preventative and responsive requirements for high-risk UEs
* a965859 Add Prompt-01 and reviewed PRD; ignore .claude/
* bef575f [docs] add initial risk management report draft
* 1f0e21d [docs] add CM exercise write-up aligned with proposal Section 4.1
* 61cd86a (tag: v1.0) [feat] add CI workflow to run automated tests on push/PR
*   cdc7ad1 Merge pull request #1 from minhsuanwu-project/feature/cm-exercise-demo
|\
| * eb8552a [chore] add config.json for CM exercise
|/
* 1412929 rename project
```

---

## 5. Release Identification and Tagging

A release is tagged only after the version is complete and its tests pass.

| Tag | Commit | Date | Contents |
|---|---|---|---|
| `v1.0` | `61cd86a` | 2026-07-13 | Version 1: core shopping flow — user authentication, product catalog, session cart, transactional checkout, order history — plus the CI workflow that verifies it |

The tag is published as a GitHub Release:
https://github.com/minhsuanwu-project/EComLite_Project/releases/tag/v1.0

`v2.0` is intentionally **not** tagged yet. Version 2 is partially delivered (order status
lifecycle, transition validation, role seeding, idempotent checkout, persistent cart), but
the Admin Order Management dashboard is still outstanding, so the version is not complete
and must not be baselined as a release.

---

## 6. Automated Verification of the Baseline

Continuous integration protects the baseline from regressions.

- **Workflow:** `.github/workflows/ci.yml`
- **Triggers:** every `push` and every `pull_request`
- **Steps:** checkout, set up .NET 8, `dotnet restore`, `dotnet test`
- **Current status:** 46 automated tests, 0 failures
  (evidence: `docs/test-evidence/2026-07-30-dotnet-test.md`)

### 6.1 CM finding and corrective action

A configuration defect was found and corrected during the project, and it is recorded as
risk R6 in the Risk Register.

- **Problem:** CI ran `dotnet test` against `EComLite.sln`, but the solution file registered
  only `EComLite.Web` and omitted `EComLite.Tests`. The command therefore matched zero test
  projects, exited 0, and displayed a green check while no tests ran. Multiple pull requests
  had merged under this false signal.
- **Correction:** `EComLite.Tests` was registered in `EComLite.sln` (commit `0b1c290`), so a
  solution-level `dotnet test` now discovers and executes the full suite.
- **Residual action:** asserting a minimum executed-test count, so that an empty test run can
  never report success, remains open and is tracked as R6.

This finding demonstrates why a configuration item (the solution file) must be controlled
and verified: a passing pipeline is not evidence of testing unless the tests actually run.

---

## 7. Controlled Documents

Engineering documents are version-controlled alongside the code so that requirements,
verification, and risk records evolve with the software:

| Document | Path |
|---|---|
| Product Requirements Document | `docs/Product_Requirements_Document.md` |
| Software Test Plan and Report | `docs/Software_Test_Plan_and_Report.md` |
| Risk Management Report | `docs/CISC594_Risk_Management_Report_Draft_EComLite.xlsx` |
| Test execution evidence | `docs/test-evidence/2026-07-30-dotnet-test.md` |
| CM exercise write-up | `docs/cm-exercise.md` |
| Generation prompts | `docs/prompts/` |

Each document carries its own revision history, and each revision is tied to a commit, so
any past version can be recovered.

---

## 8. Summary

| CM activity | Implementation | Evidence |
|---|---|---|
| Version control system | Git / GitHub, instructor has access | Public repository |
| Development off the master branch | 4 feature/docs branches | Branch list, Section 3 |
| Formal change control | Pull request required for every change | PRs #1–#4 |
| Merge to baseline after testing | Merged only after local tests and CI pass | Merge commits, Section 4.2 |
| Release tagging | `v1.0` tagged and released at completion of Version 1 | Tag `v1.0` at `61cd86a` |
| Automated verification | GitHub Actions on push and pull request | `ci.yml`, 46 tests passing |
| Configuration defect handling | R6 found, corrected, and tracked | Risk Register, commit `0b1c290` |

All new development was performed on branches, verified, reviewed through a pull request,
and merged into `master` only after it was working. Version 1 was tagged `v1.0` on
completion. The process described here was followed for every change in the repository
history.
