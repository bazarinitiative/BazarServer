
# Bazar-Server

A server for decentralized micro-blog

# Features

- Record user info
- Record posts (including reply and repost)
- Record followings
- Record likes
- Record userID changes based on BazarRealIDService

# Technical

## Design for privacy under hostile environment

- Any server do NOT keep any user private data, not keep privateKey, not keep email address, etc.

## Design for unlimited users

- Every server kept limited userdata.
- Every client can upload any command of anyone to a server, as long as the command itself can be verified by publicKey.

## BazarServer

- Will forward all userCommands to every know server.
- Every server will choose what data to storage. Usually active user only.
- Every server can try retrieve data from another server.
- Any one can throw any message to any server at any time. Server is designed to survive in such chaos battlefield.

## Architecture

<img src="https://github.com/bazarinitiative/BazarServer/raw/master/doc/bazar-architect.jpg" height="50%" width="50%" />

# Developer

## Need environment variables to run this webapp

- BazarBaseUrl = 'which domain-path is this server deployed'. eg: <https://api.yourdomain.com/bazar/> or <https://bzea.azurewebsites.net/>
- BazarMongodb = 'mongodb connectionString'

## Mongodb changestream

- Because changestream has various limitation, we will not use changestream.

## BazarServer Should only run one instance for each MongoDB

- Because we need to maintain `receiveOffset` for this instance, while MongoDB do not support auto-increment id.
- We will separate reader to dedicate module for better performance. Reader can run multiple instances. receiveOffset will help reader work better.

## This is a BASE system

- Basically Available, Soft State, Eventual Consistency.
- We tolerate temporary inconsistency for simplicity and availability.

# Thanks

Inspired by Twitter, Telegram, Signal and Mastodon
