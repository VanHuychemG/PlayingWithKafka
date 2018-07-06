# Playing with Kafka

## Setting up Kafka with Docker

### Prerequisites

* Docker for Windows - https://store.docker.com/editions/community/docker-ce-desktop-windows
* Windows Subsystem for Linux - https://docs.microsoft.com/en-us/windows/wsl/install-win10
* Docker on Windows Subsystem for Linux (Ubuntu) - https://medium.com/@sebagomez/installing-the-docker-client-on-ubuntus-windows-subsystem-for-linux-612b392a44c4

### Setting up Kafka

* https://hub.docker.com/r/confluent/kafka/

#### Stopping and removing containers

```bash
docker stop zookeeper | xargs docker rm
docker stop kafka | xargs docker rm
docker stop schema-registry | xargs docker rm
docker stop rest-proxy | xargs docker rm
docker stop schema-registry-ui | xargs docker rm
```

#### Start Zookeeper and expose port 2181 for use by the host machine

```bash
docker run -d --name zookeeper -p 2181:2181 confluent/zookeeper
```

#### Start Kafka and expose port 9092 for use by the host machine

*Also configure the broker to use the docker machine's IP address*

```bash
docker run -d --name kafka -p 9092:9092 --link zookeeper:zookeeper --env KAFKA_ADVERTISED_HOST_NAME=[DOCKER_HOST_IP] confluent/kafka
```

#### Start Schema Registry and expose port 8081 for use by the host machine

```bash
docker run -d --name schema-registry -p 8081:8081 --link zookeeper:zookeeper --link kafka:kafka confluent/schema-registry
```

#### Start REST Proxy and expose port 8082 for use by the host machine

```bash
docker run -d --name rest-proxy -p 8082:8082 --link zookeeper:zookeeper --link kafka:kafka --link schema-registry:schema-registry confluent/rest-proxy
```

#### Start Schema Registry UI and expose port 8000 for use by the host machine

```bash
docker run -d --name schema-registry-ui -p 8000:8000 -e "SCHEMAREGISTRY_URL=http://localhost:8081" landoop/schema-registry-ui
```

#### Start Kafka topics UI and expose port 8080 for use by the host machine

```bash
docker run -d -p 8080:8000 -e "KAFKA_REST_PROXY_URL=http://localhost:8082" -e "PROXY=true" landoop/kafka-topics-ui
```

*Don't forget to enable cross-origin resource sharing.*

### All-in-one solution `Lenses`

```bash
docker run -e ADV_HOST=127.0.0.1 -e EULA="https://dl.lenses.stream/d/?id=[LICENSEKEY]" --rm -p 3030:3030 -p 9092:9092 -p 2181:2181 -p 8081:8081 -p 9581:9581 -p 9582:9582 -p 9584:9584 -p 9585:9585 landoop/kafka-lenses-dev
```

### Generate C# classes from AVRO schemas

#### Avrogen

https://github.com/confluentinc/avro/releases/download/v1.7.7.4/avrogen.zip

```bash
C:\PROJECTDATA\PLAYGROUND\PlayingWithKafka\Kafka (master)
λ dotnet ..\Avrogen\avrogen.dll -s Organisation.asvc .
```