using Amazon.Lambda.Core;
using Amazon.Lambda.S3Events;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Util;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace ThumbnailConverter;

public class Function
{
    IAmazonS3 S3Client { get; set; }

    /// <summary>
    /// Default constructor. This constructor is used by Lambda to construct the instance. When invoked in a Lambda environment
    /// the AWS credentials will come from the IAM role associated with the function and the AWS region will be set to the
    /// region the Lambda function is executed in.
    /// </summary>
    public Function()
    {
        S3Client = new AmazonS3Client();
    }

    /// <summary>
    /// Constructs an instance with a preconfigured S3 client. This can be used for testing outside of the Lambda environment.
    /// </summary>
    /// <param name="s3Client">The service client to access Amazon S3.</param>
    public Function(IAmazonS3 s3Client)
    {
        this.S3Client = s3Client;
    }

    /// <summary>
    /// This method is called for every Lambda invocation. This method takes in an S3 event object and can be used 
    /// to respond to S3 notifications.
    /// </summary>
    /// <param name="evnt">The event for the Lambda function handler to process.</param>
    /// <param name="context">The ILambdaContext that provides methods for logging and describing the Lambda environment.</param>
    /// <returns></returns>
    public async Task FunctionHandler(S3Event evnt, ILambdaContext context)
    {
        var eventRecords = evnt.Records ?? new List<S3Event.S3EventNotificationRecord>();
        var thumbnailFolder = "thumbnails/";
        foreach (var record in eventRecords)
        {
            var s3Event = record.S3;
            if (s3Event == null)
            {
                continue;
            }

            try
            {
                var bucketName = s3Event.Bucket.Name;
                var key = s3Event.Object.Key;
                var response = await this.S3Client.GetObjectAsync(bucketName, key);
                context.Logger.LogLine($"Original Size of {key}: {response.ContentLength}");
                
                using (var image = Image.Load(response.ResponseStream))
                {
                    int maxWidth = 500;
                    int maxHeight = 500;

                    image.Mutate(x => x.Resize(new ResizeOptions
                    {
                        Mode = ResizeMode.Max,
                        Size = new Size(maxWidth, maxHeight)
                    }));

                    using (var outputStream = new MemoryStream())
                    {
                        image.Save(outputStream, new JpegEncoder());
                        context.Logger.LogLine($"Thumbnail Size of {key}: {outputStream.Length}");
                        var thumbnailKey = thumbnailFolder + key.Replace("images/", "");

                        var uploadRequest = new PutObjectRequest
                        {
                            BucketName = bucketName,
                            Key = thumbnailKey,
                            InputStream = outputStream
                        };

                        await this.S3Client.PutObjectAsync(uploadRequest);
                        context.Logger.LogLine($"Thumbnail created at {thumbnailKey}");
                        context.Logger.LogLine($"Uploaded Thumbnail to {thumbnailKey}");
                    }

                    //delete original image
                    await this.S3Client.DeleteObjectAsync(bucketName, key);
                    context.Logger.LogLine($"Deleted original image {key}");
                }

                context.Logger.LogInformation(response.Headers.ContentType);
            }
            catch (Exception e)
            {
                context.Logger.LogError($"Error getting object {s3Event.Object.Key} from bucket {s3Event.Bucket.Name}. Make sure they exist and your bucket is in the same region as this function.");
                context.Logger.LogError(e.Message);
                context.Logger.LogError(e.StackTrace);
                throw;
            }
        }
    }
}