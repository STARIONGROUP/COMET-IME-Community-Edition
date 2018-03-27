// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IElementDefinitionService.cs" company="RHEA System S.A.">
//   Copyright (c) 2017 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Composition.Services
{
    using System;
    using System.Threading.Tasks;
    using System.Windows.Media.Imaging;

    /// <summary>
    /// The purpose of the <see cref="IImageService"/> is to retrieve an image from an
    /// online resource in an asynchronous manner.
    /// </summary>
    public interface IImageService : IDisposable
    {
        /// <summary>
        /// Retrieve an image based on a provided Url
        /// </summary>
        /// <param name="uri">
        /// The uri of the image that is to be retrieved
        /// </param>
        /// <returns>
        /// A <see cref="Task{BitmapImage}"/>
        /// </returns>
        Task<BitmapImage> RetrieveImage(string uri);

        /// <summary>
        /// Retrieves an image as a byte array based on a provided Url
        /// </summary>
        /// <param name="imageUri">
        /// The uri of the image that is to be retrieved
        /// </param>
        /// <returns>
        /// The <see cref="byte"/> array that holds the image
        /// </returns>
        Task<byte[]> RetrieveImageBytes(string imageUri);
    }
}