// -------------------------------------------------------------------------------------------------
// <copyright file="ImageService.cs" company="RHEA System S.A.">
//   Copyright (c) 2018 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4Composition.Services
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net.Http;
    using System.ComponentModel.Composition;
    using System.Threading.Tasks;
    using System.Windows.Media.Imaging;
    using NLog;

    /// <summary>
    /// The purpose of the <see cref="ImageService"/> is to retrieve an image from an
    /// online resource in an asynchronous manner.
    /// </summary>
    [Export(typeof(IImageService))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class ImageService : IImageService
    {
        /// <summary>
        /// The NLog logger
        /// </summary>
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// A Dictionary of images. The key is the uri of the image, the value is a BitMap
        /// </summary>
        private readonly Dictionary<string, BitmapImage> imageCache;

        /// <summary>
        /// A Dictionary of images. The key is the uri of the image, the value is a byte array
        /// </summary>
        private readonly Dictionary<string, byte[]> byteArrayCache;

        /// <summary>
        /// The <see cref="HttpClient"/> used to access online images.
        /// </summary>
        private HttpClient httpClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="ImageService"/> class.
        /// </summary>
        public ImageService()
        {
            this.imageCache = new Dictionary<string, BitmapImage>();
            this.byteArrayCache = new Dictionary<string, byte[]>();
        }

        /// <summary>
        /// Retrieve an image based on a provided Url
        /// </summary>
        /// <param name="uri">
        /// The uri of the image that is to be retrieved
        /// </param>
        /// <returns>
        /// A <see cref="Task{BitmapImage}"/>, or null if the image could not be accessed
        /// </returns>
        public async Task<BitmapImage> RetrieveImage(string imageUri)
        {
            var uri = new Uri(imageUri, UriKind.Absolute);

            BitmapImage bitmapImage;
            if (this.imageCache.TryGetValue(imageUri, out bitmapImage))
            {
                Logger.Debug("Image {0} retrieved from image cache", imageUri);
                return bitmapImage;
            }

            try
            {
                var client = this.QueryHttpClient();

                using (var response = await client.GetAsync(uri))
                {
                    response.EnsureSuccessStatusCode();

                    using (var inputStream = await response.Content.ReadAsStreamAsync())
                    {
                        using (var memStream = new MemoryStream())
                        {
                            await inputStream.CopyToAsync(memStream);
                            memStream.Position = 0;

                            bitmapImage = new BitmapImage();

                            bitmapImage.BeginInit();
                            bitmapImage.StreamSource = memStream;
                            bitmapImage.EndInit();
                        }
                    }
                }

                this.imageCache.Add(imageUri, bitmapImage);

                return bitmapImage;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"The image at {imageUri} could not be downloaded");
            }

            return null;
        }

        /// <summary>
        /// Retrieves an image as a byte array based on a provided Url
        /// </summary>
        /// <param name="imageUri">
        /// The uri of the image that is to be retrieved
        /// </param>
        /// <returns>
        /// The <see cref="byte"/> array that holds the image
        /// </returns>
        public async Task<byte[]> RetrieveImageBytes(string imageUri)
        {
            var uri = new Uri(imageUri, UriKind.Absolute);

            byte[] imageByteArray;
            if (this.byteArrayCache.TryGetValue(imageUri, out imageByteArray))
            {
                Logger.Debug("Image {0} retrieved from image cache", imageUri);
                return imageByteArray;
            }

            try
            {
                var client = this.QueryHttpClient();

                using (var response = await client.GetAsync(uri))
                {
                    response.EnsureSuccessStatusCode();

                    imageByteArray = await response.Content.ReadAsByteArrayAsync();

                    this.byteArrayCache.Add(imageUri, imageByteArray);

                    return imageByteArray;
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"The image at {imageUri} could not be downloaded");
            }

            return null;
        }

        /// <summary>
        /// Queries the  <see cref="HttpClient"/>
        /// </summary>
        /// <returns></returns>
        private HttpClient QueryHttpClient()
        {
            if (this.httpClient == null)
            {
                this.httpClient = new HttpClient();
            }

            return this.httpClient;
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting
        /// unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            this.imageCache.Clear();
            this.httpClient.Dispose();
        }
    }
}