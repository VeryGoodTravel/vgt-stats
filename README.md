# vgt-stats

Main repository of the Stats service.

Handles statistics of the transactions in the SAGA process.

## Repository

This repository contains additional submodules containing shared libraries of the SAGA microservices implementations.

To update those submodules in the local branch run:

    git submodule update --remote --merge

## Configuration

### Environmental variables

- RABBIT_HOST -> Address of the rabbit server.
- RABBIT_VIRT_HOST -> Virtual host of the rabbit server.
- RABBIT_PORT -> Port of the rabbit server.
- RABBIT_USR -> Username to log in with.
- RABBIT_PASSWORD -> User password to log in with.
- BACKEND_REPLIES -> Exchange name to publish finished sagas to.
- RABBIT_STATS -> Exchange of the saga transactions notifications.
- DB_SERVER -> Database server name to use
- DB_NAME_STATS -> Database name to use for the stats service
- DB_PASSWORD -> Database password to use for the database server
- DB_USER -> Username to use on db server connection

## Implementation documentation
XML docs of the project available in the repository in the
file [StatsDocumentation.xml](SagaFlightDocumentation.xml)
