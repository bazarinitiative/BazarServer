
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

## Content-Server

- Will forward all userEvents to every know server.
- Every server will choose what data to storage. Usually active user only.
- Every server can try retrieve data from another server.
- Any one can throw any message to any server at any time. Server is designed to survive in such chaos battlefield.

## BazarModerationService (optional, commercial)

- We will provide a simple opensource demo here. You may need a private commercial implementation for commercial condition.

## BazarRealIDService (optional, commercial)

- BazarRealIDService will announce that one key-pair refers to Michael Jackson. Client can believe or not.
- Any Bazar-Server will NOT keep user privateKey, so it would be trouble if user lost his privateKey.
- BazarRealIDService will announce that someone had change his key-pair from A to B.
- Bazar-Server can choose which BazarRealIDService to believe.

## BazarRecommendationService (optional, commercial)

- Bazar-Server will carry basic Recommendation functionalities for search, user recommend, post recommend, etc 
- BazarRecommendationService will provide complex recommendation for user and UGC

# Developer

## Need environment variables to run this webapp

- BazarBaseUrl = 'which domain-path is this server deployed'. eg: <https://api.yourdomain.com/bazar/> or <https://bzea.azurewebsites.net/>
- BazarMongodb = 'mongodb connectionString'
- BazarMail = 'emailAddr_password_mailServerAddr_mailServerPort_enableSsl'. eg: aaa@bbb.com_pwdpwd_smtp.bbb.com_25_false

## Mongodb changestream

- Because changestream has various limitation, we will not use changestream.

## Bazar-Server Should only run one instance

- Because we need to maintain `receiveOffset` for this instance, while MongoDB do not support auto-increment id.
- We will separate reader to dedicate module for better performance. Reader can run multiple instances. receiveOffset will help reader work better.

## This is a BASE system

- Basically Available, Soft State, Eventual Consistency.
- We tolerate temporary inconsistency for simplicity and availability.

# Thanks

Inspired by Twitter, Signal and Mastodon
