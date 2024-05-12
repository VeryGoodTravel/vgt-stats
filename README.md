# vgt-saga-hotel

Main repository of the Hotel service.

Handles availability of hotels and their rooms,
temporarily books hotels or fully books them depending on the SAGA transaction stage.

Creates hotel information and avalilable rooms depe

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
- RABBIT_REPLIES -> Queue of the replies sent back to the orchestrator.
- RABBIT_HOTEL -> Queue of the requests sent by the orchestrator to the hotel service.
- DB_SERVER -> Database server name to use
- DB_NAME_HOTEL -> Database name to use for the hotel service
- DB_PASSWORD -> Database password to use for the database server

## Implementation documentation
XML docs of the project available in the repository in the
file [SagaHotelDocumentation.xml](SagaHotelDocumentation.xml)
