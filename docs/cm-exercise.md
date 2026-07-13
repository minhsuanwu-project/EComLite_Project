# Configuration Management Exercise — EComLite

This exercise demonstrates the version control and change control process
defined in Section 4.1 of the Project Proposal, applied to the EComLite
repository: https://github.com/minhsuanwu-project/EComLite_Project

## Beginner Level
- Repository established with initial commit and maintained README.md
- Basic add/commit/push workflow used throughout Version 1 development
  (auth, catalog, cart, checkout, order history)

## Intermediate Level
- Created branch `feature/cm-exercise-demo`
- Added `config.json` as a demonstration file, using the commit message
  convention `[type] description` specified in the proposal
- Opened Pull Request #1: https://github.com/minhsuanwu-project/EComLite_Project/pull/1
- Merged into `master`; feature branch deleted after merge

## Expert Level
- Added GitHub Actions workflow (`.github/workflows/ci.yml`) that runs
  `dotnet restore` and `dotnet test` on every push and pull request
- CI run #1: https://github.com/minhsuanwu-project/EComLite_Project/actions/runs/29225278415
  (or general Actions page: https://github.com/minhsuanwu-project/EComLite_Project/actions)
  — Status: Success, 13s duration
- Tagged release `v1.0`, marking completion of Version 1 (core shopping flow)
- Published GitHub Release: https://github.com/minhsuanwu-project/EComLite_Project/releases/tag/v1.0

## Alignment with Proposal (Section 4.1)
- Feature branches merged via pull request
- Commit message format `[type] description` followed
- Version tagged at completion (v1.0)
- CI pipeline verifies test suite passes before/after merge