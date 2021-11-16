# Banlink

## Pre-requisites

Working .NET 5\
Working Docker (If using dockerfile)\
Working Neo4J instance

## Setup

Copy the `config.toml.example` to `config.toml`, fill it in.\
Make sure your neo4j instance works. It should be a bolt connection.\
Run the exe, or follow docker instructions below.

## How to use dockerfile

Forewarning, I only tested the dockerfile on Ubuntu Server 20.04.

clone this repo somewhere\
cd into the directory with the dockerfile, copy it to the directory above it\
run `docker build -t dockerfile .`\
then make sure you have `config.toml` in the same location as the dockerfile\
run `docker run` (preferably in a screen or something)

### you're done it should work ggs


i know this guide sucks if you want to rewrite it please do xoxo