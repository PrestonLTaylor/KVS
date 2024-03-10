# KVS

KVS is a key value store minimal scalable REST API made using C#. 

# Table of Contents

* [Building](#building)
* [Testing](#testing)
* [API Endpoints](#api-endpoints)
* [Environment Variables](#environment-variables)
* [License](#license)

# Building

To build the docker image, execute this command in the root directory:

```
docker build -t [IMAGE-TAG]:[IMAGE-VERSION] -f ./KVS/Dockerfile .
```

To build without docker, execute this command in the root directory:
```
dotnet build -c Release KVS.sln
```

# Testing

To run the unit tests, execute this command in the root directory:
```
dotnet test KVS.sln
```

# API Endpoints

## POST - Create a key value pair
```
POST /v1/

Body:
{
  "newKey": "<KEY_TO_ADD>",
  "value": "<VALUE_OF_NEW_KEY>"
}

Responses:
201 - Successfully created a key value pair
400 - Bad request, key or value was null
409 - Key already exists
```

## GET - Update a key value pair
```
GET /v1/<KEY_TO_RETRIEVE>

Responses:
200 - Successfully retrieved specified key's value, returns the value in the body
400 - Bad request, key was null
404 - Key was not found
```

## PUT - Update a key value pair
```
PUT /v1/

Body:
{
  "key": "<KEY_TO_UPDATE>",
  "newValue": "<NEW_VALUE_OF_KEY>"
}

Responses:
204 - Successfully updated specified key's value
400 - Bad request, key or value was null
404 - Key was not found
```

## DELETE - Delete a key value pair
```
DELETE /v1/

Body:
{
  "key": "<KEY_TO_DELETE>",
}

Responses:
204 - Successfully updated specified key's value
400 - Bad request, key was null
404 - Key was not found
```

# Environment Variables

## RabbitMQ Variables

`rabbit-mq-host`: Uri for a RabbitMQ endpoint

`rabbit-mq-username`: Username to log into RabbitMQ

`rabbit-mq-password`: Password to log into RabbitMQ

## PostgreSQl Variables

`POSTGRESQLCONNSTR_DefaultConnection`: [npgsql connection string](https://www.npgsql.org/doc/connection-string-parameters.html) for a postgreSQL database.

# License

This project is licensed under [MIT](https://github.com/PrestonLTaylor/PMS/blob/master/LICENSE)