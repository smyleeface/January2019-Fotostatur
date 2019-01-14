using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Amazon.Lambda.Core;
using Amazon.Lambda.S3Events;
using Amazon.Rekognition;
using Amazon.Rekognition.Model;
using Amazon.S3;
using Amazon.S3.Transfer;
using MindTouch.LambdaSharp;
using Newtonsoft.Json;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using Tweetinvi;
using Image = Amazon.Rekognition.Model.Image;
using S3Object = Amazon.Rekognition.Model.S3Object;
using TweetinviModels = Tweetinvi.Models;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace Fotostatur.Fotostatur.ImageAnalyzer {

    public class Function : ALambdaFunction<S3Event, FunctionResponse> {

        //--- Fields ---
        private IAmazonRekognition _rekognitionClient;
        private IAmazonS3 _s3Client;
        private List<FoundCriterias> _foundLabels;
        private string _sourceBucket;
        private string _sourceKey;
        private float _totalScore;
        private int _criteriaFiltered;
        private string _comparingImageKey;
        private string _consumerKey;
        private string _consumerSecret;
        private string _accessToken;
        private string _accessTokenSecret;
        private string _comparingImageBucket;
        private string _headshotFileName;
        private string _filename;
        private string _tempResizedFilename;
        private string _destinationBucket;
        private float _criteriaThreshold;

        //--- Methods ---
        public override Task InitializeAsync(LambdaConfig config) {
            
            // Initializing
            _totalScore = 0;
            _criteriaFiltered = 0;
            _tempResizedFilename = "/tmp/resized.jpg";
            _criteriaThreshold = 50;
            
            // Clients
            _rekognitionClient = new AmazonRekognitionClient();
            _s3Client = new AmazonS3Client();
            
            // Environment Variables
//            _consumerKey = config.ReadText("TwitterConsumerKey");
//            _consumerSecret = config.ReadText("TwitterConsumerSecret");
//            _accessToken = config.ReadText("TwitterAccessToken");
//            _accessTokenSecret = config.ReadText("TwitterAccessSecret");
            
            // Headshot paths
            _headshotFileName = config.ReadText("HeadshotFileName");
            var headshotS3Path = config.ReadText("HeadshotPhotos").Replace("s3://", "").Split("/");
            _comparingImageBucket = headshotS3Path[0];
            _comparingImageKey = $"{headshotS3Path[1]}/{_headshotFileName}";
            return Task.CompletedTask;
        }

        public override async Task<FunctionResponse> ProcessMessageAsync(S3Event s3Event, ILambdaContext context) {
            LogInfo(JsonConvert.SerializeObject(s3Event));
            _foundLabels = new List<FoundCriterias>();
            
            // Get the Bucket name and key from the event
            GetEventInfo(s3Event);
            
            // OBJECT AND SCENE DETECTION
            var detectLabelResponse = await DetectLabels();
            ScoreLabels(detectLabelResponse);

            // TEXT IN IMAGE
            var detectTextResponse = await DetectText();
            ScoreText(detectTextResponse);

            // FACE COMPARISON
            var compareFacesResponse = await CompareFaces();
            ScoreCompare(compareFacesResponse);
           
            // FACIAL ANALYSIS
            var detectFacesResponse = await DetectFaces();
            ScoreFaces(detectFacesResponse);
            
            // Final Score
            float finalScore = 0;
            if (_criteriaFiltered > 0) {
                finalScore = CalculateFinalScore();
            }
            LogInfo($"Final Score: {finalScore}");
            
            // Post if within threshold
            if (finalScore > _criteriaThreshold) {
                // await DownloadS3Image();
                // ResizeImage();
                // UploadImage();
                // TwitterUpload();
            }
            return new FunctionResponse();
        }

        // ####################
        // ##### S3 EVENT INFO
        // ####################
        private void GetEventInfo(S3Event s3Event) {
            _sourceBucket = s3Event.Records.First().S3.Bucket.Name;
            _sourceKey = s3Event.Records.First().S3.Object.Key;
            _filename = _sourceKey.Split("/").LastOrDefault();
        }
        
        // ####################
        // ##### DETECT LABELS
        // ####################
        public async Task<DetectLabelsResponse> DetectLabels() {
            
            // TODO: detect labels from the picture
            return new DetectLabelsResponse();
        }

        public void ScoreLabels(DetectLabelsResponse detectLabelsResponse) {
            LogInfo(JsonConvert.SerializeObject(detectLabelsResponse));
            
            // TODO: determine if photo meets your label criteria
            // AddTotals("criteria label", (float) 0.1234);
        }
        
        // ####################
        // ##### DETECT TEXT
        // ####################
        private async Task<DetectTextResponse> DetectText() {
            
            // TODO: detect text in the picture
            return new DetectTextResponse();
        }

        private void ScoreText(DetectTextResponse detectTextResponse) {
            LogInfo(JsonConvert.SerializeObject(detectTextResponse));
            
            // TODO: make a criteria around detecting text in an image
            // AddTotals("criteria detect text", (float) 0.1234);
        }
        
        // ####################
        // ##### Face Compare
        // ####################
        private async Task<CompareFacesResponse> CompareFaces() {
            
            // TODO: compare face in the picture
            return new CompareFacesResponse();
        }

        private void ScoreCompare(CompareFacesResponse compareFacesResponse) {
            
            // TODO: make a criteria around comparing faces
            // AddTotals("criteria compare face", (float) 0.1234);
        }

        // ####################
        // ##### DETECT FACES
        // ####################
        public async Task<DetectFacesResponse> DetectFaces() {
            
            // TODO: detect faces in the picture
            return new DetectFacesResponse();
        }

        public void ScoreFaces(DetectFacesResponse response) {
            
            // TODO: choose one or more categories to build criteria from
//            var detail = response.FaceDetails.First();
//            var ageRange = detail.AgeRange;
//            var beard = detail.Beard;
//            var boundingBox = detail.BoundingBox;
//            var eyeglasses = detail.Eyeglasses;
//            var eyesOpen = detail.EyesOpen;
//            var gender = detail.Gender;
//            var mouthOpen = detail.MouthOpen;
//            var mustache = detail.Mustache;
//            var pose = detail.Pose;
//            var quality = detail.Quality;
//            var smile = detail.Smile;
//            var sunglasses = detail.Sunglasses;
        }

        // ####################
        // ##### CALCULATIONS
        // ####################
        public void AddTotals(string name, float points) {
            
            // track number of criterias being applied to this image
            _criteriaFiltered += 1;
            
            // track the confidence of each criteria taken from rekognition
            _totalScore += points;
            
            // (optional) keep track of the individual points earned for each criteria 
            _foundLabels.Add(new FoundCriterias {
                Name = name,
                Points = points
            });
        }
        
        private float CalculateFinalScore() {
            
            // total confidence divided by the number of criteria filtered
            return _totalScore / _criteriaFiltered;
        }
        
        // ########################################
        // ##### PROCESS IMAGE FOR UPLOAD
        // ########################################
        private async Task DownloadS3Image() {
            LogInfo("Downloading image");
            
            // TODO: Download and save image locally from S3
        }
        
        private void ResizeImage() {
            LogInfo("Resize image");
            
            // TODO: load the image and resize (https://github.com/SixLabors/ImageSharp#api)
        }

        private void UploadImage() {
            LogInfo("Upload image");
            
            // TODO: https://docs.aws.amazon.com/sdkfornet/v3/apidocs/items/S3/TTransferUtility.html
        }
        
        private void TwitterUpload() {
            
            // TODO: update local file path
            try {
                LogInfo("Twitter image");
                var bytes = File.ReadAllBytesAsync("LOCAL FILE PATH").Result;
                Auth.SetUserCredentials(_consumerKey, _consumerSecret, _accessToken, _accessTokenSecret);
                Account.UpdateProfileImage(bytes);
            }
            catch (Exception e) {
                LogError(e);
            }
        }
        
    }
    
    public class FunctionResponse {

        // this function is intentionally left empty
    }
}
