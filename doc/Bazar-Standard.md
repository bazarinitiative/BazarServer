
# Overview

  Bazar Initiative is to make social media neutral.  
  Technically we are trying to create a decentralized micro-blog, aka decentralized twitter.

# Key Concetp

## User Account

- An unique ECDSA key pair means an account.
- UserID is just a shortcut to a ECDSA publicKey. Which is generated from publicKey with a specific algorithm.
- One biological or logical user may have several userID.
- If someone lost his privateKey, there won't be any way to findback the lost privatKey.
- RealnameService will announce that a new UserID is replacing an old UserID. Clients can choose which service to trust or not.
- User Identity  
   User Identity is public/private key paire of one user account. Will be kept in client side only. We will remind user to backup his identity personally and privately.

## User Generated Content

  All UGC will keep in Bazar Federation forever, like blockchain content.

- User Info  
   Public user profile, with userID, publicKey, userName, userPic, etc.
- Post  
   Inlude post and replay.
- Repost  
   aka retweet.
- Like  
   User like a Post.
- Following  
   UserID A is following B.
- Channel  
   User created group of users. aka List in twitter, aka Public channel in Telegram.
- Delete  
   Delete things like post, like, following, etc.
- Model limits  
   UserPic max at 50 KB  
   UserRetrieve max at 10 KB  
   Others max at 1 KB  
   Media is special, should be treaded as outer link that may disappear anytime. It has separate limits.

## Verification and Trust

- UserID-PublicKey binding will never change. Because UserID is just a shortcut to a ECDSA publicKey.
- UGC is always verified by publicKey. Server will trust any UGC from any where if ECDSA signature check is passed.
- Time inside UGC is not trustworthy. A server can only trust the time of receiving.
- RealnameService is a weak point. Client can choose which service provider to trust or not.

## Anti-spam

- Every server will anti-spam base on IP and userID.
- Every server will get data from others with a limited speed, based on reputation.
- Every server can delete any data to reduce disk space, then try retrieve those data again when necessary.  
  Because ECDSA signature is the only proof, server can delete any UGC data and recover them safely.
- So spam-data basically will sleep in isolated servers.

# Fundamental Service (Developing, open source)

## Bazar Server

- Store User Generated Content (UGC) from client.
- Exchange UGC with other servers, in P2P mode.
- ECDSA signature is the only proof of UGC.
- Support client queries.

## Bazar Client

- Client may be any form. WPF, Android, iOS, H5, etc.
- Client is responsable for user secrets.
- Client can connect to any BazarServer to send UGC.
- Client can connect to any BazarServer for simple query.
- Client can connect to any Luxury Service for complex advanced service.

# Luxury Service (Not yet, should be commercial)

## Bubble service

- Decide what to broadcast. In the form of Recommendation, Search, Tag, etc. Should combine with moderation.

### Recommendation service

- Decide which user/post/channel should be seen by users. Combine with moderation.

### Search service

- Combine with Recommendation.

### Hashtag service

- Combine with Recommendation.

## Realname service

- Tells that an userID is somebody in real world. Tells that a new userID is inheriting an old userID.

## Translation service

- Translate UGC to different languages.

## Statistic service

- Display statistics of UGC, how many times have this UGC be displayed to users, etc.
