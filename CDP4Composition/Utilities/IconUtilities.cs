// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IconUtilities.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2020 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Merlin Bieze, Naron Phou, Patxi Ozkoidi, Alexander van Delft, Mihail Militaru
//            Nathanael Smiechowski, Kamil Wojnowski
//
//    This file is part of CDP4-IME Community Edition. 
//    The CDP4-IME Community Edition is the RHEA Concurrent Design Desktop Application and Excel Integration
//    compliant with ECSS-E-TM-10-25 Annex A and Annex C.
//
//    The CDP4-IME Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or any later version.
//
//    The CDP4-IME Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//    GNU Affero General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Common.Helpers
{
    using System;
    using System.Drawing;
    using System.Drawing.Drawing2D;
    using System.IO;
    using System.Windows;
    using System.Windows.Interop;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;

    using CDP4Common.CommonData;

    using CDP4Composition.Services;

    using DevExpress.Xpf.Core;
    using DevExpress.Xpf.Core.Native;

    using Point = System.Drawing.Point;

    /// <summary>
    /// Utility class containing static method to handle icons
    /// </summary>
    public static class IconUtilities
    {
        /// <summary>
        /// The <see cref="Uri"/> to the error image overlay.
        /// </summary>
        public static readonly Uri ErrorImageUri = new Uri("pack://application:,,,/CDP4Composition;component/Resources/Images/Log/ExclamationRed_16x16.png");

        /// <summary>
        /// The <see cref="Uri"/> to the relationship image overlay.
        /// </summary>
        public static readonly Uri RelationshipOverlayUri = new Uri("pack://application:,,,/CDP4Composition;component/Resources/Images/Log/linkgreen_16x16.png");

        /// <summary>
        /// The <see cref="Uri"/> to the favorite image overlay.
        /// </summary>
        public static readonly Uri FavoriteOverlayUri = (new DXImageConverter().ConvertFrom("NewContact_16x16.png") as DXImageInfo)?.MakeUri();

        /// <summary>
        /// The <see cref="Uri"/> to the locked image overlay.
        /// </summary>
        public static readonly Uri LockedOverlayUri = (new DXImageConverter().ConvertFrom("BO_Security_Permission.png") as DXImageInfo)?.MakeUri();

        /// <summary>
        /// The <see cref="Uri"/> to the hidden image overlay.
        /// </summary>
        public static readonly Uri HiddenOverlayUri = new Uri("pack://application:,,,/CDP4Composition;component/Resources/Images/hidden_16x16.png");

        public static ImageSource ToImageSource(this Icon icon)
        {
            var imageSource = Imaging.CreateBitmapSourceFromHIcon(
                icon.Handle,
                Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions());

            return imageSource;
        }

        /// <summary>
        /// converts the <see cref="BitmapImage"/> to a <see cref="Bitmap"/>
        /// </summary>
        /// <param name="bitmapImage">
        /// the subject <see cref="BitmapImage"/>
        /// </param>
        /// <returns>
        /// the resulting <see cref="Bitmap"/>
        /// </returns>
        public static Bitmap BitmapImage2Bitmap(BitmapImage bitmapImage)
        {
            using (var outStream = new MemoryStream())
            {
                BitmapEncoder enc = new PngBitmapEncoder();
                enc.Frames.Add(BitmapFrame.Create(bitmapImage, null, null, null));
                enc.Save(outStream);

                var bitmap = new Bitmap(outStream);

                return new Bitmap(bitmap);
            }
        }

        /// <summary>
        /// converts the <see cref="BitmapImage"/> to a <see cref="Bitmap"/>
        /// </summary>
        /// <param name="bitmapImage">
        /// the subject <see cref="BitmapImage"/>
        /// </param>
        /// <param name="width">
        /// The width.
        /// </param>
        /// <param name="height">
        /// The height.
        /// </param>
        /// <returns>
        /// the resulting <see cref="Bitmap"/>
        /// </returns>
        public static Bitmap BitmapImage2Bitmap(BitmapImage bitmapImage, int width, int height)
        {
            using (var outStream = new MemoryStream())
            {
                BitmapEncoder enc = new PngBitmapEncoder();

                enc.Frames.Add(BitmapFrame.Create(bitmapImage, null, null, null));
                enc.Save(outStream);

                var bitmap = new Bitmap(outStream);

                return new Bitmap(bitmap, width, height);
            }
        }

        /// <summary>
        /// The bitmap source with the image and the error overlay.
        /// </summary>
        /// <param name="uri">
        /// The uri to the source image.
        /// </param>
        /// <returns>
        /// The <see cref="BitmapSource"/>.
        /// </returns>
        public static BitmapSource WithErrorOverlay(Uri uri)
        {
            return WithOverlay(uri, ErrorImageUri);
        }

        /// <summary>
        /// The bitmap source with the image and the overlay.
        /// </summary>
        /// <param name="iconUri">
        /// The uri to the source image.
        /// </param>
        /// <param name="overlayUri">
        /// The uri of the overlay
        /// </param>
        /// <param name="overlayPosition">
        /// The overlay position
        /// </param>
        /// <returns>
        /// The <see cref="BitmapSource"/>.
        /// </returns>
        public static BitmapSource WithOverlay(Uri iconUri, Uri overlayUri, OverlayPositionKind overlayPosition = OverlayPositionKind.TopRight)
        {
            var source = new BitmapImage(iconUri);
            var overlay = new BitmapImage(overlayUri);

            var thingBitMapImage = BitmapImage2Bitmap(source);
            var overlayBitMapImage = BitmapImage2Bitmap(overlay, (int)Math.Floor(thingBitMapImage.Width * 0.75D), (int)Math.Floor(thingBitMapImage.Height * 0.75D));

            var img = new Bitmap(
                    (int)Math.Floor(thingBitMapImage.Width * 1.4D),
                    (int)Math.Floor(thingBitMapImage.Height * 1.0D),
                System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            using (var gr = Graphics.FromImage(img))
            {
                gr.CompositingMode = CompositingMode.SourceOver;
                gr.DrawImage(thingBitMapImage, GetMainImagePoint(img, thingBitMapImage));
                gr.DrawImage(overlayBitMapImage, GetOverlayPoint(overlayPosition, img, overlayBitMapImage));
            }

            return Bitmap2BitmapSource(img);
        }

        /// <summary>
        /// Gets the starting <see cref="Point"/> where the overlay needs to be drawn
        /// </summary>
        /// <param name="overlayPosition">The <see cref="OverlayPositionKind"/></param>
        /// <param name="targetImage">The <see cref="Image"/> that the overlay is added to</param>
        /// <param name="overlayBitMapImage">The overlay<see cref="Image"/></param>
        /// <returns>Starting <see cref="Point"/> where the overlay needs to be drawn</returns>
        private static Point GetOverlayPoint(OverlayPositionKind overlayPosition, Image targetImage, Image overlayBitMapImage)
        {
            var rightXpos = targetImage.Width - overlayBitMapImage.Width;
            var bottomYpos = targetImage.Height - overlayBitMapImage.Height;

            switch (overlayPosition)
            {
                case OverlayPositionKind.BottomRight:
                    return new Point(rightXpos, bottomYpos);

                case OverlayPositionKind.TopRight:
                    return new Point(rightXpos, 0);

                default:
                    return new Point(0, 0);
            }
        }

        /// <summary>
        /// Gets the starting <see cref="Point"/> where the main image needs to be drawn
        /// </summary>
        /// <param name="targetImage">The <see cref="Image"/> that the main image is added to</param>
        /// <param name="thingBitMapImage">The main <see cref="Image"/></param>
        /// <returns>Starting <see cref="Point"/> where the main image needs to be drawn</returns>
        private static Point GetMainImagePoint(Image targetImage, Image thingBitMapImage)
        {
            return new Point((targetImage.Width / 2) - (thingBitMapImage.Width / 2), 0);
        }

        /// <summary>
        /// Converts bitmap to bitmap source.
        /// </summary>
        /// <param name="bitmap">
        /// The bitmap.
        /// </param>
        /// <returns>
        /// The <see cref="BitmapImage"/>.
        /// </returns>
        public static BitmapSource Bitmap2BitmapSource(Bitmap bitmap)
        {
            return Imaging.CreateBitmapSourceFromHBitmap(bitmap.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
        }

        /// <summary>
        /// Returns the <see cref="Uri"/> of the resource
        /// </summary>
        /// <param name="classKind">
        /// The <see cref="ClassKind"/> for which in icon needs to be provided
        /// </param>
        /// <param name="getsmallicon">
        /// Indicates whether a small or large icon should be returned.
        /// </param>
        /// <returns>
        /// A <see cref="Uri"/> that points to a resource
        /// </returns>
        public static object ImageUri(ClassKind classKind, bool getsmallicon = true)
        {
            DXImageInfo imageInfo;

            var compositionroot = "pack://application:,,,/CDP4Composition;component/Resources/Images/Thing/";
            var imagesize = getsmallicon ? "_16x16" : "_32x32";
            var imagename = string.Empty;
            var imageextension = ".png";

            switch (classKind)
            {
                case ClassKind.BinaryRelationship:
                    imagename = "LineItem";
                    imageInfo = new DXImageConverter().ConvertFrom($"{imagename}{imagesize}{imageextension}") as DXImageInfo;

                    return imageInfo.MakeUri().ToString();
                case ClassKind.MultiRelationship:
                    imagename = "Line2";
                    imageInfo = new DXImageConverter().ConvertFrom($"{imagename}{imagesize}{imageextension}") as DXImageInfo;

                    return imageInfo.MakeUri().ToString();
                case ClassKind.PossibleFiniteState:
                case ClassKind.PossibleFiniteStateList:
                    return "pack://application:,,,/CDP4Composition;component/Resources/Images/Thing/PossibleFiniteState_48x48.png";
                case ClassKind.ActualFiniteState:
                case ClassKind.ActualFiniteStateList:
                    return "pack://application:,,,/CDP4Composition;component/Resources/Images/Thing/ActualFiniteState_48x48.png";
                case ClassKind.NaturalLanguage:
                    return "pack://application:,,,/CDP4Composition;component/Resources/Images/Thing/naturallanguage.png";
                case ClassKind.Requirement:
                    return "pack://application:,,,/CDP4Composition;component/Resources/Images/Thing/requirement.png";
                case ClassKind.Book:
                    return "pack://application:,,,/CDP4Composition;component/Resources/Images/Thing/Book.png";
                case ClassKind.Page:
                    imagename = "ListBox";
                    imageInfo = new DXImageConverter().ConvertFrom($"{imagename}{imagesize}{imageextension}") as DXImageInfo;

                    return imageInfo.MakeUri().ToString();
                case ClassKind.Section:
                    imagename = "Reading";
                    imageInfo = new DXImageConverter().ConvertFrom($"{imagename}{imagesize}{imageextension}") as DXImageInfo;

                    return imageInfo.MakeUri().ToString();
                case ClassKind.BinaryNote:
                case ClassKind.TextualNote:
                    imagename = "Notes";
                    imageInfo = new DXImageConverter().ConvertFrom($"{imagename}{imagesize}{imageextension}") as DXImageInfo;

                    return imageInfo.MakeUri().ToString();
                case ClassKind.DiagramCanvas:
                    imagename = "LabelsRight";
                    imageInfo = new DXImageConverter().ConvertFrom($"{imagename}{imagesize}{imageextension}") as DXImageInfo;

                    return imageInfo.MakeUri().ToString();
                case ClassKind.SimpleUnit:
                case ClassKind.PrefixedUnit:
                case ClassKind.LinearConversionUnit:
                case ClassKind.DerivedUnit:
                case ClassKind.MeasurementUnit:
                    imagename = "measurementunit";

                    return $"{compositionroot}{imagename}{imagesize}{imageextension}";
                case ClassKind.UnitPrefix:
                    imagename = "VerticalAxisThousands";
                    imageInfo = new DXImageConverter().ConvertFrom($"{imagename}{imagesize}{imageextension}") as DXImageInfo;

                    return imageInfo.MakeUri().ToString();
                case ClassKind.RequirementsContainer:
                case ClassKind.RequirementsGroup:
                    imagename = "ListBox";
                    imageInfo = new DXImageConverter().ConvertFrom($"{imagename}{imagesize}{imageextension}") as DXImageInfo;

                    return imageInfo.MakeUri().ToString();
                case ClassKind.RequirementsSpecification:
                    imagename = "BOReport";
                    imageInfo = new DXImageConverter().ConvertFrom($"{imagename}{imagesize}{imageextension}") as DXImageInfo;

                    return imageInfo.MakeUri().ToString();
                case ClassKind.IntervalScale:
                case ClassKind.LogarithmicScale:
                case ClassKind.OrdinalScale:
                case ClassKind.RatioScale:
                case ClassKind.CyclicRatioScale:
                case ClassKind.MeasurementScale:
                    imagename = "ChartYAxisSettings";
                    imageInfo = new DXImageConverter().ConvertFrom($"{imagename}{imagesize}{imageextension}") as DXImageInfo;

                    return imageInfo.MakeUri().ToString();
                case ClassKind.Constant:
                    imagename = "RecentlyUse";
                    imageInfo = new DXImageConverter().ConvertFrom($"{imagename}{imagesize}{imageextension}") as DXImageInfo;

                    return imageInfo.MakeUri().ToString();
                case ClassKind.BinaryRelationshipRule:
                    imagename = "LineItem";
                    imageInfo = new DXImageConverter().ConvertFrom($"{imagename}{imagesize}{imageextension}") as DXImageInfo;

                    return imageInfo.MakeUri().ToString();
                case ClassKind.Organization:
                    imagename = "Organization";

                    return $"{compositionroot}{imagename}{imagesize}{imageextension}";
                case ClassKind.Rule:
                    imagename = "TreeView";
                    imageInfo = new DXImageConverter().ConvertFrom($"{imagename}{imagesize}{imageextension}") as DXImageInfo;

                    return imageInfo.MakeUri().ToString();
                case ClassKind.DecompositionRule:
                    imagename = "TreeView";
                    imageInfo = new DXImageConverter().ConvertFrom($"{imagename}{imagesize}{imageextension}") as DXImageInfo;

                    return imageInfo.MakeUri().ToString();
                case ClassKind.MultiRelationshipRule:
                    imagename = "DocumentMap";
                    imageInfo = new DXImageConverter().ConvertFrom($"{imagename}{imagesize}{imageextension}") as DXImageInfo;

                    return imageInfo.MakeUri().ToString();
                case ClassKind.ParameterizedCategoryRule:
                    imagename = "FixedWidth";
                    imageInfo = new DXImageConverter().ConvertFrom($"{imagename}{imagesize}{imageextension}") as DXImageInfo;

                    return imageInfo.MakeUri().ToString();
                case ClassKind.ReferencerRule:
                    imagename = "Tag";
                    imageInfo = new DXImageConverter().ConvertFrom($"{imagename}{imagesize}{imageextension}") as DXImageInfo;

                    return imageInfo.MakeUri().ToString();
                case ClassKind.SiteDirectory:
                    imagename = "Database";
                    imageInfo = new DXImageConverter().ConvertFrom($"{imagename}{imagesize}{imageextension}") as DXImageInfo;

                    return imageInfo.MakeUri().ToString();
                case ClassKind.EngineeringModelSetup:
                case ClassKind.EngineeringModel:
                    imagename = "Technology";
                    imageInfo = new DXImageConverter().ConvertFrom($"{imagename}{imagesize}{imageextension}") as DXImageInfo;

                    return imageInfo.MakeUri().ToString();
                case ClassKind.ParametricConstraint:
                    imagename = "ShowFormulas";
                    imageInfo = new DXImageConverter().ConvertFrom($"{imagename}{imagesize}{imageextension}") as DXImageInfo;

                    return imageInfo.MakeUri().ToString();
                case ClassKind.NotExpression:
                case ClassKind.AndExpression:
                case ClassKind.OrExpression:
                case ClassKind.ExclusiveOrExpression:
                case ClassKind.RelationalExpression:
                case ClassKind.BooleanExpression:
                    imagename = "UseInFormula";
                    imageInfo = new DXImageConverter().ConvertFrom($"{imagename}{imagesize}{imageextension}") as DXImageInfo;

                    return imageInfo.MakeUri().ToString();
                case ClassKind.File:
                    imagename = "BOFileAttachment";
                    imageInfo = new DXImageConverter().ConvertFrom($"{imagename}{imagesize}{imageextension}") as DXImageInfo;

                    return imageInfo.MakeUri().ToString();
                case ClassKind.FileRevision:
                    imagename = "Version";
                    imageInfo = new DXImageConverter().ConvertFrom($"{imagename}{imagesize}{imageextension}") as DXImageInfo;

                    return imageInfo.MakeUri().ToString();
                case ClassKind.Folder:
                case ClassKind.NotThing:
                    imagename = "BOFolder";
                    imageInfo = new DXImageConverter().ConvertFrom($"{imagename}{imagesize}{imageextension}") as DXImageInfo;

                    return imageInfo.MakeUri().ToString();
                case ClassKind.Participant:
                    imagename = "Employee";
                    imageInfo = new DXImageConverter().ConvertFrom($"{imagename}{imagesize}{imageextension}") as DXImageInfo;

                    return imageInfo.MakeUri().ToString();
                case ClassKind.Iteration:
                case ClassKind.IterationSetup:
                    imagename = "GroupFieldCollection";
                    imageInfo = new DXImageConverter().ConvertFrom($"{imagename}{imagesize}{imageextension}") as DXImageInfo;

                    return imageInfo.MakeUri().ToString();
                case ClassKind.ElementDefinition:
                    imagename = "Product";
                    imageInfo = new DXImageConverter().ConvertFrom($"{imagename}{imagesize}{imageextension}") as DXImageInfo;

                    return imageInfo.MakeUri().ToString();
                case ClassKind.ElementUsage:
                    imagename = "Version";
                    imageInfo = new DXImageConverter().ConvertFrom($"{imagename}{imagesize}{imageextension}") as DXImageInfo;

                    return imageInfo.MakeUri().ToString();
                case ClassKind.ParameterGroup:
                    imagename = "BOFolder";
                    imageInfo = new DXImageConverter().ConvertFrom($"{imagename}{imagesize}{imageextension}") as DXImageInfo;

                    return imageInfo.MakeUri().ToString();
                case ClassKind.Parameter:
                case ClassKind.SimpleParameterValue:
                    imagename = "Stepline";
                    imageInfo = new DXImageConverter().ConvertFrom($"{imagename}{imagesize}{imageextension}") as DXImageInfo;

                    return imageInfo.MakeUri().ToString();
                case ClassKind.ParameterValueSet:
                case ClassKind.ParameterOverrideValueSet:
                case ClassKind.ParameterSubscriptionValueSet:
                case ClassKind.ParameterValueSetBase:
                    imagename = "DocumentMap";
                    imageInfo = new DXImageConverter().ConvertFrom($"{imagename}{imagesize}{imageextension}") as DXImageInfo;

                    return imageInfo.MakeUri().ToString();
                case ClassKind.ParameterSubscription:
                    imagename = "LabelsCenter";
                    imageInfo = new DXImageConverter().ConvertFrom($"{imagename}{imagesize}{imageextension}") as DXImageInfo;

                    return imageInfo.MakeUri().ToString();
                case ClassKind.ParameterOverride:
                    imagename = "LabelsBelow";
                    imageInfo = new DXImageConverter().ConvertFrom($"{imagename}{imagesize}{imageextension}") as DXImageInfo;

                    return imageInfo.MakeUri().ToString();
                case ClassKind.Category:
                    imagename = "category";

                    return $"{compositionroot}{imagename}{imagesize}{imageextension}";
                case ClassKind.PersonRole:
                case ClassKind.ParticipantRole:
                    imagename = "BOUser";
                    imageInfo = new DXImageConverter().ConvertFrom($"{imagename}{imagesize}{imageextension}") as DXImageInfo;

                    return imageInfo.MakeUri().ToString();
                case ClassKind.Person:
                    imagename = "Customer";
                    imageInfo = new DXImageConverter().ConvertFrom($"{imagename}{imagesize}{imageextension}") as DXImageInfo;

                    return imageInfo.MakeUri().ToString();
                case ClassKind.PersonPermission:
                case ClassKind.ParticipantPermission:
                    imagename = "BOPermission";
                    imageInfo = new DXImageConverter().ConvertFrom($"{imagename}{imagesize}{imageextension}") as DXImageInfo;

                    return imageInfo.MakeUri().ToString();
                case ClassKind.ReferenceSource:
                    imagename = "Information";
                    imageInfo = new DXImageConverter().ConvertFrom($"{imagename}{imagesize}{imageextension}") as DXImageInfo;

                    return imageInfo.MakeUri().ToString();
                case ClassKind.EmailAddress:
                    imagename = "Mail";
                    imageInfo = new DXImageConverter().ConvertFrom($"{imagename}{imagesize}{imageextension}") as DXImageInfo;

                    return imageInfo.MakeUri().ToString();
                case ClassKind.TelephoneNumber:
                    imagename = "BOContact";
                    imageInfo = new DXImageConverter().ConvertFrom($"{imagename}{imagesize}{imageextension}") as DXImageInfo;

                    return imageInfo.MakeUri().ToString();
                case ClassKind.UserPreference:
                    imagename = "Technology";
                    imageInfo = new DXImageConverter().ConvertFrom($"{imagename}{imagesize}{imageextension}") as DXImageInfo;

                    return imageInfo.MakeUri().ToString();
                case ClassKind.DomainOfExpertise:
                    imagename = "domainofexpertise";

                    return $"{compositionroot}{imagename}{imagesize}{imageextension}";
                case ClassKind.ModelReferenceDataLibrary:
                case ClassKind.SiteReferenceDataLibrary:
                    imagename = "siteRdl";

                    return $"{compositionroot}{imagename}{imagesize}{imageextension}";
                case ClassKind.SimpleQuantityKind:
                case ClassKind.DerivedQuantityKind:
                case ClassKind.SpecializedQuantityKind:
                case ClassKind.ArrayParameterType:
                case ClassKind.BooleanParameterType:
                case ClassKind.CompoundParameterType:
                case ClassKind.DateParameterType:
                case ClassKind.DateTimeParameterType:
                case ClassKind.EnumerationParameterType:
                case ClassKind.ScalarParameterType:
                case ClassKind.TextParameterType:
                case ClassKind.TimeOfDayParameterType:
                case ClassKind.SampledFunctionParameterType:
                case ClassKind.ParameterType:
                    imagename = "NameManager";
                    imageInfo = new DXImageConverter().ConvertFrom($"{imagename}{imagesize}{imageextension}") as DXImageInfo;

                    return imageInfo.MakeUri().ToString();
                case ClassKind.ParameterTypeComponent:
                    imagename = "Between";
                    imageInfo = new DXImageConverter().ConvertFrom($"{imagename}{imagesize}{imageextension}") as DXImageInfo;

                    return imageInfo.MakeUri().ToString();
                case ClassKind.Definition:
                    imagename = "SendBehindText";
                    imageInfo = new DXImageConverter().ConvertFrom($"{imagename}{imagesize}{imageextension}") as DXImageInfo;

                    return imageInfo.MakeUri().ToString();
                case ClassKind.Option:
                    imagename = "Properties";
                    imageInfo = new DXImageConverter().ConvertFrom($"{imagename}{imagesize}{imageextension}") as DXImageInfo;

                    return imageInfo.MakeUri().ToString();
                case ClassKind.Term:
                    imagename = "TextBox";
                    imageInfo = new DXImageConverter().ConvertFrom($"{imagename}{imagesize}{imageextension}") as DXImageInfo;

                    return imageInfo.MakeUri().ToString();
                case ClassKind.Glossary:
                    imagename = "Text";
                    imageInfo = new DXImageConverter().ConvertFrom($"{imagename}{imagesize}{imageextension}") as DXImageInfo;

                    return imageInfo.MakeUri().ToString();
                case ClassKind.FileType:
                    imagename = "TextBox2";
                    imageInfo = new DXImageConverter().ConvertFrom($"{imagename}{imagesize}{imageextension}") as DXImageInfo;

                    return imageInfo.MakeUri().ToString();
                case ClassKind.Publication:
                    imagename = "CreateModelDifferences";
                    imageInfo = new DXImageConverter().ConvertFrom($"{imagename}{imagesize}{imageextension}") as DXImageInfo;

                    return imageInfo.MakeUri().ToString();
                case ClassKind.CommonFileStore:
                    imagename = "Project";
                    imageInfo = new DXImageConverter().ConvertFrom($"{imagename}{imagesize}{imageextension}") as DXImageInfo;

                    return imageInfo.MakeUri().ToString();
                case ClassKind.DomainFileStore:
                    imagename = "Project";
                    imageInfo = new DXImageConverter().ConvertFrom($"{imagename}{imagesize}{imageextension}") as DXImageInfo;

                    return imageInfo.MakeUri().ToString();
                case ClassKind.ChangeRequest:
                case ClassKind.RequestForDeviation:
                case ClassKind.RequestForWaiver:
                    imagename = "EditComment";
                    imageInfo = new DXImageConverter().ConvertFrom($"{imagename}{imagesize}{imageextension}") as DXImageInfo;

                    return imageInfo.MakeUri().ToString();
                case ClassKind.ActionItem:
                    imagename = "GroupByResource";
                    imageInfo = new DXImageConverter().ConvertFrom($"{imagename}{imagesize}{imageextension}") as DXImageInfo;

                    return imageInfo.MakeUri().ToString();
                case ClassKind.ChangeProposal:
                    imagename = "PreviousComment";
                    imageInfo = new DXImageConverter().ConvertFrom($"{imagename}{imagesize}{imageextension}") as DXImageInfo;

                    return imageInfo.MakeUri().ToString();
                case ClassKind.ReviewItemDiscrepancy:
                    imagename = "InsertComment";
                    imageInfo = new DXImageConverter().ConvertFrom($"{imagename}{imagesize}{imageextension}") as DXImageInfo;

                    return imageInfo.MakeUri().ToString();
                default:
                    // TODO: create default Thing icon to be used accross the app after the branding change. Default for now uses Iteration setup icon.
                    imagename = "Technology";
                    imageInfo = new DXImageConverter().ConvertFrom($"{imagename}{imagesize}{imageextension}") as DXImageInfo;

                    return imageInfo.MakeUri().ToString();
            }
        }
    }
}
