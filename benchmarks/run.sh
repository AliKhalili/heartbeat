#!/bin/bash

NAME=$1
ADDRESS=$2

for c in 1 16 32 128 256; do
    echo "Running the command with $c connections to $ADDRESS..."
    for i in {1..35}; do
        echo "Running iteration $i..."
        bombardier -c $c -m PUT -H "Device-Id:1234" --http1 -a $ADDRESS -o j > results/${NAME}/result-$c-$i.json
    done
done


echo "${NAME} done!"