### Changed

- The Build workflow skips SonarCloud with a warning when SonarCloud rejects `SONAR_TOKEN` as invalid or expired
  (HTTP 401), so restore, build and test still run. A valid token without Browse permission on the project (HTTP 403)
  fails the build.
- `scripts/template.json` uses the keys `productPlaceholder` and `repositoryPlaceholder`.
