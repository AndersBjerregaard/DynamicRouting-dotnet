#!/bin/bash

# check dependencies are installed
type rabbitmqctl > /dev/null 2>&1 || { echo >&2 "rabbitmqctl is required but it is not installed. Aborting."; exit 1; }
type rabbitmqadmin > /dev/null 2>&1 || { echo >&2 "rabbitmqadmin is required but it is not installed. Aborting."; exit 1; }


# Wait for the RabbitMQ server to start
rabbitmqctl wait /var/lib/rabbitmq/mnesia/rabbitmq.pid

# check auth credentials are present

[ -z "$RABBITMQ_USER" ] && { echo >&2 "RABBITMQ_USER variable should be present. Aborting."; exit 1; }
[ -z "$RABBITMQ_PASS" ] && { echo >&2 "RABBITMQ_PASS variable should be present. Aborting."; exit 1; }
[ -z "$RABBITMQ_HOST" ] && { echo >&2 "RABBITMQ_HOST variable should be present. Aborting."; exit 1; }
[ -z "$RABBITMQ_PORT" ] && { echo >&2 "RABBITMQ_PORT variable should be present. Aborting."; exit 1; }

# helpers

function rabbitmqadmin_cli () {
  rabbitmqadmin \
    --vhost=/ \
    --username="${RABBITMQ_USER}" \
    --password="${RABBITMQ_PASS}" \
    --host="${RABBITMQ_HOST}" \
    --port="${RABBITMQ_PORT}" \
    ${@}
}

#Creating Exchanges
rabbitmqadmin_cli declare exchange name=DR_Exchange type=direct
#Creating Queues
rabbitmqadmin_cli declare queue name=Consumer2 durable=true
rabbitmqadmin_cli declare queue name=loadQueues durable=true
rabbitmqadmin_cli declare queue name=consumeProducer durable=true

#Creating Bindings  
rabbitmqadmin_cli declare binding source="DR_Exchange" destination_type="queue" destination="loadQueues" routing_key=""

# Start the RabbitMQ server
echo "Rabbitmq configured with success."