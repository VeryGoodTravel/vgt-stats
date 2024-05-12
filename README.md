# vgt-saga-hotel

Main repository of the Flight service.

Handles availability of flights,
temporarily books flights or fully books them depending on the SAGA transaction stage.

Creates travel destinations information and available flights with the data provided by the scrapper.

## Data storage (PostgreSQL)

Follows CQRS.

Database consists of the 3 main tables:
 - AirportDb -> contains information about each Airport defined with the foreign key of flights offered by the airport,
 - FlightDb -> contains definition of the flights offered with their respectful amount of seats available,
 - Booking -> Defines booked flight seats. Specifies if the booking is temporary with the DateTime of the temporary booking and the time the booking takes place. Is assigned to a flight.

![Database schema](DB_FLIGHT.png)

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
- DB_NAME_FLIGHT -> Database name to use for the flight service
- DB_PASSWORD -> Database password to use for the database server
- DB_USER -> Username to use on db server connection

## Implementation documentation
XML docs of the project available in the repository in the
file [SagaFlightDocumentation.xml](SagaFlightDocumentation.xml)
