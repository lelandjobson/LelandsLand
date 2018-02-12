using System;
using System.Collections;
using System.Drawing;
using System.IO;
using Accord;
using Accord.Imaging;
using Accord.Imaging.Filters;
using Accord.Math;

namespace LelandsLand
{
    public class StitcherClass
    {

        #region Members

        // Images we are going to stitch together
        private Bitmap _img1;
        private Bitmap _img2;

        // Images in process
        private Bitmap _processImage1;
        private Bitmap _processImage2;
        private Bitmap _processImage3;

        private Bitmap _completeImage;

        // Fields to store our interest points in the two images
        private IntPoint[] harrisPoints1;
        private IntPoint[] harrisPoints2;

        // Fields to store our correlated points
        private IntPoint[] correlationPoints1;
        private IntPoint[] correlationPoints2;

        // The homography matrix estimated by RANSAC
        private MatrixH homography;
        #endregion

        #region Properties

        public Bitmap Img1
        {
            get
            {
                return this._img1;
            }
        }
        public Bitmap Img2
        {
            get
            {
                return this._img2;
            }
        }
        public Bitmap CompleteImage
        {
            get
            {
                return this._completeImage;
            }
        }

        #endregion

        #region ctor

        public StitcherClass(Bitmap img1, Bitmap img2, bool debugMode = false)
        {
            this._img1 = img1;
            this._img2 = img2;

            InterestPtDetector();
            Correlator();
            RansacRobustHomographer();
            GradientBlender();

            if (debugMode)
            {
                SaveProcessImages();
            }
        }
        #endregion

        #region methods

        private void SaveProcessImages()
        {
            string outPath = System.Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\aviary\processImages\";
            System.IO.Directory.CreateDirectory(outPath);

            // Write out process images
            Bitmap[] processImages = new Bitmap[] { _processImage1, _processImage2, _processImage3 };
            int i = 1;
            foreach(var processImg in processImages)
            {
                string outputFileName = outPath + "_processImg_" + i.ToString() + ".jpg";
                using (MemoryStream memory = new MemoryStream())
                {
                    using (FileStream fs = new FileStream(outputFileName, FileMode.Create, FileAccess.ReadWrite))
                    {
                        processImg.Save(memory, System.Drawing.Imaging.ImageFormat.Jpeg);
                        byte[] bytes = memory.ToArray();
                        fs.Write(bytes, 0, bytes.Length);
                    }
                }
                i++;
            }

        }

        // Interest point detection
        private void InterestPtDetector()
        {
            // Step 1: Detect feature points using Harris Corners Detector
            HarrisCornersDetector harris = new HarrisCornersDetector(0.04f, 1000f);
            harrisPoints1 = harris.ProcessImage(_img1).ToArray();
            harrisPoints2 = harris.ProcessImage(_img2).ToArray();

            // Show the marked points in the original images
            Bitmap img1mark = new PointsMarker(harrisPoints1).Apply(_img1);
            Bitmap img2mark = new PointsMarker(harrisPoints2).Apply(_img2);

            // Concatenate the two images together in a single image (just to show on screen)
            Concatenate concatenate = new Concatenate(img1mark);
            _processImage1 = concatenate.Apply(img2mark);
        }

        // Correlation Matching
        private void Correlator()
        {
            // Step 2: Match feature points using a correlation measure
            CorrelationMatching matcher = new CorrelationMatching(9);
            IntPoint[][] matches = matcher.Match(_img1, _img2, harrisPoints1, harrisPoints2);

            // Get the two sets of points
            correlationPoints1 = matches[0];
            correlationPoints2 = matches[1];

            // Concatenate the two images in a single image (just to show on screen)
            Concatenate concat = new Concatenate(_img1);
            Bitmap img3 = concat.Apply(_img2);

            // Show the marked correlations in the concatenated image
            PairsMarker pairs = new PairsMarker(
                correlationPoints1, // Add image1's width to the X points
                                    // to show the markings correctly
                correlationPoints2.Apply(p => new IntPoint(p.X + _img1.Width, p.Y)));

            _processImage2 = pairs.Apply(img3);
        }

        // Robust homography estimation
        private void RansacRobustHomographer()
        {
            // Step 3: Create the homography matrix using a robust estimator
            RansacHomographyEstimator ransac = new RansacHomographyEstimator(0.001, 0.99);
            homography = ransac.Estimate(correlationPoints1, correlationPoints2);

            // Plot RANSAC results against correlation results
            IntPoint[] inliers1 = correlationPoints1.Submatrix(ransac.Inliers);
            IntPoint[] inliers2 = correlationPoints2.Submatrix(ransac.Inliers);

            // Concatenate the two images in a single image (just to show on screen)
            Concatenate concat = new Concatenate(_img1);
            Bitmap img3 = concat.Apply(_img2);

            // Show the marked correlations in the concatenated image
            PairsMarker pairs = new PairsMarker(
                inliers1, // Add image1's width to the X points to show the markings correctly
                inliers2.Apply(p => new IntPoint(p.X + _img1.Width, p.Y)));

            _processImage3 = pairs.Apply(img3);
        }

        // Gradient blending
        private void GradientBlender()
        {
            // Step 4: Project and blend the second image using the homography
            Blend blend = new Blend(homography, _img1);
            _completeImage = blend.Apply(_img2);
        }

        #endregion
    }
}