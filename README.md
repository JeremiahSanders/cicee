# Continuous Integration Containerized Execution Environment (CICEE)

CICEE is an opinionated orchestrator of continuous integration processes.

## Usage

### CICEE Project Structure

CICEE expects that a project repository is structured in the following manner.

* _Project repository root_
  * `ci/`
    * `ci.env` - _Optional._ Environment initialization script. Sourced during execution workflow.
    * `docker-compose.dependencies.yml` - _Optional._ Docker-compose services definitions providing dependencies for continuous integration environment. Examples: databases.
    * `docker-compose.project.yml` - _Optional._ Project-specific continuous integration environment definition. Supports declaring dependencies, specifying default environment variables, etc.
    * `Dockerfile` - _Required, unless `exec` is executed with `--image`._ - Dockerfile defining continuous integration environment.
