module PeopleApi.App

open System
open System.Collections.Generic
open WebSharper
open WebSharper.Sitelets

/// The types used by this application.
module Model =

    /// Data about a person. Used both for storage and JSON parsing/writing.
    /// Data type for registering a user
    type UserData = 
        { 
            userId: string
            status : string     
        }

    type LoginData = 
        {
            userId : string
        }
    
    type TweetData = 
        {
            userId : string
            tweet : string
        } 

    type FollowData = 
        {
            userId1 : string
            userId2 : string
        } 


    type HashtagMentiontag = 
        {
            hashtag : string
            mentiontag : string
        } 
    
    type SubscribeData = 
        {
            userId : string
        }

    type QueryByHashTag = 
        {
            word : string
        }

    /// The type of REST API endpoints.
    /// This defines the set of requests accepted by our API.
    type ApiEndPoint =
        /// Accepts POST requests to /registerUser UserData as JSON body
        | [<EndPoint "POST /registerUser"; Json "userData">] CreateUser of userData: UserData
        | [<EndPoint "POST /loginUser"; Json "loginData">] LoginUser of loginData: LoginData
        | [<EndPoint "POST /logoutUser"; Json "loginData">] LogoutUser of loginData: LoginData
        | [<EndPoint "POST /tweet"; Json "tweetData">] PostTweet of tweetData: TweetData
        | [<EndPoint "GET /tweets">] GetTweets
        | [<EndPoint "POST /follow"; Json "followData">] PostFollow of followData: FollowData
        | [<EndPoint "POST /subscribe"; Json "subscribeData">] PostSubscribe of subscribeData: SubscribeData
        | [<EndPoint "GET /tweets/hashtag">] GetHashtag of word: string
        | [<EndPoint "GET /tweets/mentiontag">] GetMentiontag of word: string
        | [<EndPoint "GET /tweets/hashtagMentiontag">] GetHashtagMentiontag of hashtagMentiontag: HashtagMentiontag






        
        // | [<EndPoint "POST /follow"; Json "followData">] PostFollow of followData: FollowData



        /// Accepts GET requests to /people
        // | [<EndPoint "GET /people">]
        //     GetPeople

        // /// Accepts GET requests to /people/{id}
        // | [<EndPoint "GET /people">]
        //     GetPerson of id: int

        // /// Accepts POST requests to /people with PersonData as JSON body
        // // | [<EndPoint "POST /people"; Json "personData">]
        // //     CreatePerson of personData: PersonData

        // | [<EndPoint "POST /people"; Json "registerData">]
        //     RegisterUser of registerData: RegisterData

        // /// Accepts PUT requests to /people with PersonData as JSON body
        // | [<EndPoint "PUT /people"; Json "personData">]
        //     EditPerson of personData: PersonData

        // /// Accepts DELETE requests to /people/{id}
        // | [<EndPoint "DELETE /people">]
        //     DeletePerson of id: int

    /// The type of all endpoints for the application.
    type EndPoint =
        
        /// Accepts requests to /
        | [<EndPoint "/">] Home

        /// Accepts requests to /api/...
        | [<EndPoint "/api">] Api of Cors<ApiEndPoint>

    /// Error result value.
    type Error = { error : string }

    /// Alias representing the success or failure of an operation.
    /// The Ok case contains a success value to return as JSON.
    /// The Error case contains an HTTP status and a JSON error to return.
    type ApiResult<'T> = Result<'T, Http.Status * Error>

    /// Result value for CreatePerson.
    // type Id = { id : int }
    // type Username = { username : string }
    type Resp = { resp: string }

    type SubscribeResponse = 
        {
            resp: list<string>
        }

    type HashTagResponse= 
        {
            resp: Set<string>
        }



    type TweetResp = 
        {
            userId: string 
            tweet: string
        }


    type GetAllTweets = 
        {
           userId : string list []
        }


    type people =  
        {
            finalArr : Dictionary<string, SubscribeResponse>

        } 

   

    

open Model

/// This module implements the back-end of the application.
/// It's a CRUD application maintaining a basic in-memory database of people.
module Backend =
    
    let mutable allUsers : Set<string> = Set.empty
    let mutable setOfLoggedInUsers : Set<string> = Set.empty
    
    let mutable totalUsers:int = 0 
    let mutable totalTweets:int = 0 
    let mutable loggedInUsers: int = 0
    let mutable loggedOutUsers: int = 0
    let mutable totalReTweets: int = 0



////////////////////////////

    let mutable mapTweetIdToUserName : Map<String, String> = Map.empty 
    let mutable mapUserToHashTags : Map<String, Set<string>> = Map.empty
    let mutable mapUserToMentionTags : Map<String, Set<string>> = Map.empty 
    let mutable mapUserNametoTweets: Map<String, list<string>> = Map.empty
    let mutable allUsersMap : Map<string, bool> = Map.empty
    let mutable followersMap : Map<String, list<string>> = Map.empty
    let mutable followingMap : Map<String, list<string>> = Map.empty    




    ///////////////////////////////////// 
    /// 
    /// 

    

    

    let strToBool(str: string) = 
        let final = Boolean.Parse(str)
        final

    let CreateUser (data: UserData): ApiResult<Resp> =
        printfn "\nLOG: User %A attempting to create an account. %A" data.userId data.status
        if allUsers.Contains(data.userId) then
            let err = sprintf "User already exists in the registry"
            Error(Http.Status.Forbidden, {error = err})
        else 
            allUsers <- allUsers.Add(data.userId)
            let status = strToBool(data.status)
            totalUsers <- totalUsers + 1
            loggedOutUsers <- loggedOutUsers + 1
            allUsersMap <- allUsersMap.Add(data.userId, status)
            followingMap <- followingMap.Add(data.userId, List.empty)
            followersMap <- followersMap.Add(data.userId, List.empty)
            Ok { resp = data.userId }


    let LoginUser(data:LoginData) : ApiResult<Resp> = 
        printfn "\nLOG: User %A attempting to login. %A" data.userId
        
        if allUsers.Contains(data.userId) then
            if not(setOfLoggedInUsers.Contains(data.userId)) then
                setOfLoggedInUsers <-setOfLoggedInUsers.Add(data.userId)
                let status = allUsersMap.TryFind(data.userId)
                match status with 
                | Some x ->
                    let finalStatus = not(x)
                    allUsersMap <- allUsersMap.Add(data.userId, finalStatus)
                    loggedInUsers <- loggedInUsers + 1
                    loggedOutUsers <- loggedOutUsers - 1
                    printfn "User %A Logged in" data.userId
                    Ok { resp = data.userId }
                | None ->
                    let err = sprintf "Key does not exist"
                    Error(Http.Status.Forbidden, {error = err})
            else 
                let err = sprintf "User already logged in."
                Error(Http.Status.Forbidden, {error = err})
        else
            let err = sprintf "User not registered. Please Register."
            Error(Http.Status.Forbidden, {error = err})


    let LogoutUser(data:LoginData) : ApiResult<Resp> = 
        printfn "\nLOG: User %A attempting to logout. %A" data.userId
        
        if allUsers.Contains(data.userId) then
            if (setOfLoggedInUsers.Contains(data.userId)) then
                let status = allUsersMap.TryFind(data.userId)
                match status with 
                | Some x ->
                    let finalStatus = not(x)
                    allUsersMap <- allUsersMap.Add(data.userId, finalStatus)
                    loggedInUsers <- loggedInUsers - 1
                    loggedOutUsers <- loggedOutUsers + 1
                    printfn "User %A Logged out" data.userId
                    printfn "%b" finalStatus
                    Ok { resp = data.userId }
                | None ->
                    let err = sprintf "Key does not exist"
                    Error(Http.Status.Forbidden, {error = err})
            else 
                let err = sprintf "User not logged in."
                Error(Http.Status.Forbidden, {error = err})
        else
            let err = sprintf "User not registered. Please Register."
            Error(Http.Status.Forbidden, {error = err})
                

    let splitLine = (fun (line:string)->Seq.toList(line.Split " "))


    let PostTweet(data:TweetData) : ApiResult<TweetResp> = 
        let username = data.userId
        let tweet = data.tweet
        
        printfn "Length of Tweet %i" tweet.Length
        
        if allUsers.Contains(username) then
            if setOfLoggedInUsers.Contains(username) then
                
                if mapUserNametoTweets.ContainsKey(username) then
                    let mutable temp = mapUserNametoTweets.Item(username)
                    temp <- temp @ [tweet]
                    mapUserNametoTweets <- mapUserNametoTweets.Add(username, temp)
                else 
                    mapUserNametoTweets <- mapUserNametoTweets.Add(username, list.Empty)
                    let mutable temp = mapUserNametoTweets.Item(username)
                    temp <- temp @[tweet]
                    mapUserNametoTweets <- mapUserNametoTweets.Add(username, temp)

                let res = splitLine tweet
                for value in res do
                    if value.Contains("#") then
                        
                        if mapUserToHashTags.ContainsKey(value.[1..value.Length-1]) then
                            mapUserToHashTags <- mapUserToHashTags.Add(value.[1..value.Length-1], mapUserToHashTags.[value.[1..value.Length-1]].Add(tweet))
                        else 
                            mapUserToHashTags <- mapUserToHashTags.Add(value.[1..value.Length-1], Set.empty)
                            mapUserToHashTags <- mapUserToHashTags.Add(value.[1..value.Length-1], mapUserToHashTags.[value.[1..value.Length-1]].Add(tweet))

                    if value.Contains("@") then
                        if mapUserToMentionTags.ContainsKey(value.[1..value.Length-1]) then
                            mapUserToMentionTags <- mapUserToMentionTags.Add(value.[1..value.Length-1], mapUserToMentionTags.[value.[1..value.Length-1]].Add(tweet))
                        else 
                            mapUserToMentionTags <- mapUserToMentionTags.Add(value.[1..value.Length-1], Set.empty)
                            
                            mapUserToMentionTags <- mapUserToMentionTags.Add(value.[1..value.Length-1], mapUserToMentionTags.[value.[1..value.Length-1]].Add(tweet))
                           

                printfn "Total Registered Users: %i\nTotal Online Users: %i\nTotal Offline Users: %i\nTotal Tweets: %i\nTotal ReTweets: %i\n" totalUsers loggedInUsers loggedOutUsers totalTweets totalReTweets

                printfn "User to Tweets: %A" mapUserNametoTweets
                printfn "User to Hastags: %A" mapUserToHashTags
                printfn "User to Mentions: %A" mapUserToMentionTags


                printfn "User %s Tweeted:  \"%s\" " username tweet

                
                totalTweets<- totalTweets + 1



                Ok{userId = username ; tweet =  tweet}
            else 
                let err = sprintf "User not logged in."
                Error(Http.Status.Forbidden, {error = err}) 
        else 
            let err = sprintf "User is not registered."
            Error(Http.Status.Forbidden, {error = err})

    let GetTweets () : ApiResult<string list []> =
        lock mapUserNametoTweets <| fun () ->
            mapUserNametoTweets
            |> Seq.map (fun (KeyValue(_, tweet)) -> tweet)
            |> Array.ofSeq
            |> Ok
               
    let PostFollow (data: FollowData) : ApiResult<Resp> = 
        let wantsToFollow = data.userId1
        let isFollowedBy = data.userId2

        if isFollowedBy = wantsToFollow then
            let err = sprintf "User cannot followItself."
            Error(Http.Status.Forbidden, {error = err})
        elif allUsers.Contains(wantsToFollow) then
            if allUsers.Contains(isFollowedBy) then
                if setOfLoggedInUsers.Contains(wantsToFollow) then
                    
                    let mutable temp = followersMap.Item(isFollowedBy)
                    let mutable alreadyFollowing = false
                    for user in temp do
                        if user = wantsToFollow then
                            alreadyFollowing <- true

                    let mutable temp1 = followingMap.Item(wantsToFollow)
                    
                    if alreadyFollowing then
                        let err = sprintf "%s alrready follows %s." wantsToFollow isFollowedBy
                        Error(Http.Status.Forbidden, {error = err})
                    else
                        temp <- temp @ [wantsToFollow]
                        followersMap <- followersMap.Add(isFollowedBy, temp)
                        temp1<-temp1 @ [isFollowedBy]
                        followingMap <- followingMap.Add(wantsToFollow, temp1)
                        printfn "User %s started following User %s" wantsToFollow isFollowedBy
                        let finalStr = sprintf "User %s started following %s" wantsToFollow isFollowedBy
                        printfn "Following Map: %A" followingMap
                        printfn "Followers Map: %A" followersMap
                        Ok{resp = finalStr}
                else
                    let err = sprintf "%s please login to follow %s" wantsToFollow isFollowedBy
                    Error(Http.Status.Forbidden, {error = err})
                    
            else 
                let err = sprintf "%s named user does not exist" isFollowedBy
                Error(Http.Status.Forbidden, {error = err})
        else 
            let err = sprintf "%s is not registered. Please Register" wantsToFollow
            Error(Http.Status.Forbidden, {error = err})


    let PostSubscribe(data:SubscribeData) : ApiResult<SubscribeResponse> = 
        let username = data.userId
        let mutable followersSet : list<string> = List.empty
        if allUsers.Contains(username) then
            if setOfLoggedInUsers.Contains(username) then
                
                let tempList = followingMap.TryFind(username)
                match tempList with
                | Some x-> 
                    if x.Length = 0 then
                        let err = sprintf "%s must follow first to get subscribed tweets" username
                        Error(Http.Status.Forbidden, {error = err})
                    else 
                        followersSet<- followersSet @ followingMap.Item(username)
                        let mutable finalStr : list<string> = List.empty  
                        for follower in [0..followersSet.Length-1] do
                            let mutable sendStr = ""
                            if(mapUserNametoTweets.ContainsKey(followersSet.[follower])) then
                                let mutable temp = mapUserNametoTweets.Item(followersSet.[follower])
                                for tweet in [0..temp.Length-1] do
                                    sendStr <- sendStr + sprintf "%s Tweeted: \"%s\"" followersSet.[follower] temp.[tweet]
                          
                            finalStr <- finalStr @ [sendStr]

                            
                        Ok{resp = finalStr}
                        // sender <! finalStr  

                | None -> 
                    let mutable abc : list<string> = List.empty  
                   
                    Ok{resp = abc}
                    

                




                // if(tempList.Length = 0) then
                //    sender <! sprintf "%s must follow first to get subscribed tweets" username
                // else 
                //     followers <- followers @ followingMap.Item(username)
                //     let mutable finalStr = ""   
                //     for follower in [0..followers.Length-1] do
                //         let mutable sendStr = ""
                //         if(mapUserNametoTweets.ContainsKey(followers.[follower])) then
                //             let mutable temp = mapUserNametoTweets.Item(followers.[follower])
                //             for tweet in [0..temp.Length-1] do
                //                 sendStr <- sendStr + sprintf "%s Tweeted: \"%s\"\n" followers.[follower] temp.[tweet]
                      
                //         finalStr <- finalStr + sendStr
                //     sender <! finalStr       
                                                 
            else
                let err = sprintf "%s is not logged in. Please Login" username
                Error(Http.Status.Forbidden, {error = err})
                
        else
            let err = sprintf "%s is not registered. Please Register" username
            Error(Http.Status.Forbidden, {error = err})
            

    let hashtagNotFound() : ApiResult<'T> =
        Error (Http.Status.NotFound, { error = "HashTag not found." })


    let mentiontagNotFound() : ApiResult<'T> =
        Error (Http.Status.NotFound, { error = "HashTag not found." })


    

    let GetHashtag (word: string) : ApiResult<Set<string>> =
        lock mapUserToHashTags <| fun () ->
        
        match mapUserToHashTags.TryGetValue(word) with
        | true, tweets -> Ok tweets  
        | false, _ -> hashtagNotFound()



    let GetMentiontag (word: string) : ApiResult<Set<string>> =
        lock mapUserToMentionTags <| fun () ->
        
        match mapUserToMentionTags.TryGetValue(word) with
        | true, tweets -> Ok tweets  
        | false, _ -> mentiontagNotFound()

    
    let GetHashtagMentiontag (data : HashtagMentiontag) : ApiResult<Set<string>> =
        let hashtag = data.hashtag
        let mentiontag = data.mentiontag
        match mapUserToMentionTags.TryGetValue(hashtag) with
        | true, tweets -> Ok tweets  
        | false, _ -> mentiontagNotFound()
        



    

    /// The people database.
    /// This is a dummy implementation, of course; a real-world application
    /// would go to an actual database.
    // let private people = new Dictionary<int, PersonData>()

    // /// The highest id used so far, incremented each time a person is POSTed.
    // let private lastId = ref 0

    // let personNotFound() : ApiResult<'T> =
    //     Error (Http.Status.NotFound, { error = "Person not found." })

    // let GetPeople () : ApiResult<PersonData[]> =
    //     lock people <| fun () ->
    //         people
    //         |> Seq.map (fun (KeyValue(_, person)) -> person)
    //         |> Array.ofSeq
    //         |> Ok

    // let GetPerson (id: int) : ApiResult<PersonData> =
    //     lock people <| fun () ->
    //         match people.TryGetValue(id) with
    //         | true, person -> Ok person
    //         | false, _ -> personNotFound()

    // let CreatePerson (data: PersonData) : ApiResult<Id> =
    //     lock people <| fun () ->
    //         incr lastId
    //         people.[!lastId] <- { data with id = !lastId }
    //         Ok { id = !lastId }


    // let RegisterUser(data:RegisterData) : ApiResult<Username> = 
    //     Ok{username = data.username}

    // let EditPerson (data: PersonData) : ApiResult<Id> =
    //     lock people <| fun () ->
    //         match people.TryGetValue(data.id) with
    //         | true, _ ->
    //             people.[data.id] <- data
    //             Ok { id = data.id }
    //         | false, _ -> personNotFound()

    // let DeletePerson (id: int) : ApiResult<Id> =
    //     lock people <| fun () ->
    //         match people.TryGetValue(id) with
    //         | true, _ ->
    //             people.Remove(id) |> ignore
    //             Ok { id = id }
    //         | false, _ -> personNotFound()

    // // On application startup, pre-fill the database with a few people.
    // do List.iter (CreatePerson >> ignore) [
    //     { id = 0
    //       firstName = "Alonzo"
    //       lastName = "Church"
    //       born = DateTime(1903, 6, 14)
    //       died = Some(DateTime(1995, 8, 11)) }
    //     { id = 0
    //       firstName = "Alan"
    //       lastName = "Turing"
    //       born = DateTime(1912, 6, 23)
    //       died = Some(DateTime(1954, 6, 7)) }
    //     { id = 0
    //       firstName = "Bertrand"
    //       lastName = "Russell"
    //       born = DateTime(1872, 5, 18)
    //       died = Some(DateTime(1970, 2, 2)) }
    //     { id = 0
    //       firstName = "Noam"
    //       lastName = "Chomsky"
    //       born = DateTime(1928, 12, 7)
    //       died = None }
    // ]

/// The server side website, tying everything together.
module Site =
    open WebSharper.UI
    open WebSharper.UI.Html
    open WebSharper.UI.Server

    /// Helper function to convert our internal ApiResult type into WebSharper Content.
    let JsonContent (result: ApiResult<'T>) : Async<Content<EndPoint>> =
        match result with
        | Ok value ->
            Content.Json value
        | Error (status, error) ->
            Content.Json error
            |> Content.SetStatus status
        |> Content.WithContentType "application/json"

    /// Respond to an ApiEndPoint by calling the corresponding backend function
    /// and converting the result into Content.
    let ApiContent (ep: ApiEndPoint) : Async<Content<EndPoint>> =
        match ep with
        | CreateUser userData -> JsonContent(Backend.CreateUser userData)
        | LoginUser loginData -> JsonContent(Backend.LoginUser loginData)
        | LogoutUser loginData -> JsonContent(Backend.LogoutUser loginData)
        | PostTweet tweetData -> JsonContent(Backend.PostTweet tweetData)
        | PostFollow followData-> JsonContent(Backend.PostFollow followData)
        | PostSubscribe subscribeData-> JsonContent(Backend.PostSubscribe subscribeData)
        | GetHashtag word -> JsonContent(Backend.GetHashtag word)
        | GetMentiontag word -> JsonContent(Backend.GetMentiontag word)
        | GetHashtagMentiontag hashtagMentiontag -> JsonContent(Backend.GetHashtagMentiontag hashtagMentiontag)
        | GetTweets -> JsonContent (Backend.GetTweets ())
        

        // | GetPeople ->
        //     JsonContent (Backend.GetPeople ())
        // | GetPerson id ->
        //     JsonContent (Backend.GetPerson id)
        // | RegisterUser registerData ->
        //     JsonContent (Backend.RegisterUser registerData)
        // | EditPerson personData ->
        //     JsonContent (Backend.EditPerson personData)
        // | DeletePerson id ->
        //     JsonContent (Backend.DeletePerson id)

    /// A simple HTML home page.
    // let HomePage (ctx: Context<EndPoint>) : Async<Content<EndPoint>> =
    //     // Type-safely creates the URI: "/api/people/1"
    //     let person1Link = ctx.Link (Api (Cors.Of (GetPerson 1)))
    //     Content.Page(
    //         Body = [
    //             p [] [text "API is running."]
    //             p [] [
    //                 text "Try querying: "
    //                 a [attr.href person1Link] [text person1Link]
    //             ]
    //         ]
    //     )

    /// The Sitelet parses requests into EndPoint values
    /// and dispatches them to the content function.
    let Main corsAllowedOrigins : Sitelet<EndPoint> =
        Application.MultiPage (fun ctx endpoint ->
            match endpoint with
            // | Home -> HomePage ctx
            | Api api ->
                Content.Cors api (fun allows ->
                    { allows with
                        Origins = corsAllowedOrigins
                        Headers = ["Content-Type"]
                    }
                ) ApiContent
        )
