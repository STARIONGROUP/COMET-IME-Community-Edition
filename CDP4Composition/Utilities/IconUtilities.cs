// -----------------------------------------------------------------------------------------------
// <copyright file="IconUtilities.cs" company="RHEA System S.A.">
//   Copyright (c) 2015-2019 RHEA System S.A.
// </copyright>
// -----------------------------------------------------------------------------------------------

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
        public static BitmapSource WithOverlay(Uri iconUri, Uri overlayUri, OverlayPositionKind overlayPosition = OverlayPositionKind.TopLeft)
        {
            var source = new BitmapImage(iconUri);
            var overlay = new BitmapImage(overlayUri);

            var thingBitMapImage = BitmapImage2Bitmap(source);
            var overlayBitMapImage = BitmapImage2Bitmap(overlay, thingBitMapImage.Width / 2, thingBitMapImage.Height / 2);

            var img = new Bitmap(thingBitMapImage.Width, thingBitMapImage.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            var xOverlay = (overlayPosition == OverlayPositionKind.TopLeft) || (overlayPosition == OverlayPositionKind.BottomLeft) ? 0 : thingBitMapImage.Width / 2;
            var yOverlay = (overlayPosition == OverlayPositionKind.TopLeft) || (overlayPosition == OverlayPositionKind.TopRight) ? 0 : thingBitMapImage.Height / 2;

            using (var gr = Graphics.FromImage(img))
            {
                gr.CompositingMode = CompositingMode.SourceOver;
                gr.DrawImage(thingBitMapImage, new Point(0, 0));
                gr.DrawImage(overlayBitMapImage, new Point(xOverlay, yOverlay));
            }

            return Bitmap2BitmapSource(img);
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
                case ClassKind.ParameterTypeComponent:
                case ClassKind.ParameterType:
                    imagename = "parametertype";

                    return $"{compositionroot}{imagename}{imagesize}{imageextension}";
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
