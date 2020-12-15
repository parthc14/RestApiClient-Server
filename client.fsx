// #r "nuget: Akka.FSharp" 
#r "nuget: FSharp.Data, Version=3.0.1"
// open Akka.FSharp
open FSharp.Data
open FSharp.Data.HttpRequestHeaders

/// User registration command
let registerUser userName =
    let sendingJson = sprintf """{"userId": "%s","status": "false"}""" userName
    let response = Http.Request(
                    "http://localhost:5000/api/registerUser",
                    httpMethod = "POST",
                    headers = [ ContentType HttpContentTypes.Json ],
                    body = TextRequest sendingJson
    )
    let r1 = response.Body
    let response1 =
        match r1 with
        | Text a -> a
        | Binary b -> System.Text.ASCIIEncoding.ASCII.GetString b
    response1

let response = registerUser "user1"
printfn "%s" response


/// Function to post the tweet
// let postTweet userName tweet=
//     try
//         let sendingJson = sprintf """{"userId": "%s", "tweet":"%s"}""" userName tweet
//         let response = Http.Request(
//                         "http://localhost:5000/api/tweet",
//                         httpMethod = "POST",
//                         headers = [ ContentType HttpContentTypes.Json ],
//                         body = TextRequest sendingJson
//         )
//         let r1 = response.Body
//         let response1 =
//             match r1 with
//             | Text a -> a
//             | Binary b -> System.Text.ASCIIEncoding.ASCII.GetString b
//         response1
//     with
//     | _ -> 
//         printfn "Please check the userid"
//         ""

// // let tweetResponse = postTweet "test2" "myTweet From client"
// // printfn "%s" tweetResponse


// // let tweetResponse1 = postTweet " user1" "user1 tweetin from CLI"
// // printfn "%s" tweetResponse1




// /// Get tweets hashtags without '#'
// let getTweetsWithHashTag hashtagToRequest =
//     try
//         let url = "http://localhost:5000/api/hashtags/"+hashtagToRequest
//         let a = FSharp.Data.JsonValue.Load url
//         let c = a.GetProperty("tweets")
//         let mutable len=0
//         for i in c do
//             len<-len+1
//         if len > 0 then
//             printfn "Tweets with the #%s found: " hashtagToRequest
//             for i in c do
//                 printfn "%A" i
            
//     with
//     | _ -> printfn "No tweets with the #%s found" hashtagToRequest

// // getTweetsWithHashTag "UF"



// /// Get user mentions 
// let getMentions mentionedUser =
//     try
//         let url = "http://localhost:5000/api/mentions/"+mentionedUser
//         printfn "%s" url
//         let a = FSharp.Data.JsonValue.Load url
//         let c = a.GetProperty("tweets")
//         let mutable len=0
//         for i in c do
//             len<-len+1
//         if len > 0 then
//             printfn "Tweets with %s mentions found: " mentionedUser
//             for i in c do
//                 printfn "%A" i            
//     with
//     | _ -> printfn "No tweets with %s mentions found" mentionedUser



// /// Post a follow request from user to leader 
// let startFollowing userId leaderId =
//     try
//         let sendingJson = sprintf """{"userId": "%s", "leaderId":"%s"}""" userId leaderId
//         let response = Http.Request(
//                         "http://localhost:5000/api/subscribe",
//                         httpMethod = "POST",
//                         headers = [ ContentType HttpContentTypes.Json ],
//                         body = TextRequest sendingJson
//         )
//         let r1 = response.Body
//         let response1 =
//             match r1 with
//             | Text a -> a
//             | Binary b -> System.Text.ASCIIEncoding.ASCII.GetString b
//         printfn "%s now following %s" userId leaderId       
//         response1
//     with
//     | _ -> 
//         printfn "Please check the userId's of both the users"
//         ""
// startFollowing "user1" "user2"