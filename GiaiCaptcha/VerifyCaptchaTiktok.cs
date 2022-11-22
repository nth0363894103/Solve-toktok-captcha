using OpenCvSharp;
using System;
using System.Collections.Generic;

namespace GiaiCaptcha
{
    internal class VerifyCaptchaTiktok
    {

        public static (bool Result, int X1, int Y1, int X2, int Y2, int TimeSwipe) VeryCaptcha(string fileName)
        {
            Mat imgInput = new Mat(fileName);
            double Width_imgInput = imgInput.Width;
            double Height_imgInput = imgInput.Height;
            double Height_Cloner = 0;

            if (Width_imgInput != 540)
            {
                double a = (540 - Width_imgInput) / Width_imgInput * 100;
                if (a < 0)
                {
                    string b = a.ToString();

                    Height_Cloner = imgInput.Height - (imgInput.Height * double.Parse(b.Remove(0, 1)) / 100);
                    Cv2.PyrUp(imgInput, imgInput);
                }
                else
                {
                    Height_Cloner = imgInput.Height + (imgInput.Height * a / 100);
                    Cv2.PyrDown(imgInput, imgInput);
                }
                Cv2.Resize(imgInput, imgInput, new Size(540, Math.Round(Height_Cloner)));
            }

            
            var Arrow = SearchArrow(imgInput);


            if (Arrow.X != 0)
            {
                Mat imgput = ImageManipulation(imgInput, Arrow.ArrowWidth, Arrow.ArrowHeight, Arrow.X, Arrow.Y);
                var BD = PercentCourtImg(Width_imgInput, Height_imgInput, Arrow.X, Arrow.Y); // Lấy tòa độ bắt đầu
                var KT = PercentCourtImg(Width_imgInput, Height_imgInput, (double)BoroderComparison(imgput), Arrow.Y);

                if(KT.X != 0)
                {
                    if (Width_imgInput == 1080 | Width_imgInput == 540)
                    {
                        KT.X -= 5;
                    }
                    int TimeSwipe = ((int)KT.X - (int)BD.X) * 5;
                    return (true, (int)BD.X, (int)BD.Y, (int)KT.X, (int)KT.Y, TimeSwipe);
                }
                else
                {
                    return (false, 0, 0, 0, 0, 0);
                }
                
            }
            else
            {
                return (false,0, 0, 0, 0, 0);
            }
        }

        static (double X, double Y, int ArrowWidth, int ArrowHeight) SearchArrow(Mat img)
        {
            Mat imgArrow = img.Clone();
            double X = 0;
            double Y = 0;
            Mat Arrow = new Mat();
            Cv2.CvtColor(imgArrow, imgArrow, ColorConversionCodes.BGR2GRAY);
            for (int i = 1; i < 3; i++)
            {
                Arrow = Cv2.ImRead($"VerifyCaptchaTiktok\\img\\Arrow{i}.png", 0);

                var degreeCourt = RunTemplateMatch(imgArrow, Arrow);
                if (degreeCourt.X != 0 && degreeCourt.Y != 0)
                {
                    X = degreeCourt.X;
                    Y = degreeCourt.Y;
                    break;
                }
            }
            return (X, Y, Arrow.Width, Arrow.Height);


        }

        static (double X, double Y) RunTemplateMatch(Mat refMat, Mat template, double ratio = 0.8)
        {
            int refMat_Width = refMat.Width;
            int refMat_Height = refMat.Height;

            Mat reg = refMat.MatchTemplate(template, TemplateMatchModes.CCoeffNormed);

            double minval, maxval = 0;
            OpenCvSharp.Point minloc, maxloc;
            Cv2.MinMaxLoc(reg, out minval, out maxval, out minloc, out maxloc);

            var CourtX = ((template.Width / 2) + maxloc.X);
            var CourtY = (template.Height / 2) + maxloc.Y;
            double X = (CourtX * 100) / refMat_Width;
            double Y = (CourtY * 100) / refMat_Height;

            if (maxval >= ratio)
            {


                return (X: X, Y: Y);
            }
            else
            {
                return (X: X = 0, Y: Y = 0);
            }

        }

        public static OpenCvSharp.Mat ImageManipulation(Mat img, int ArrowWidth, int ArrowHeight, double maxX, double maxY)
        {
            Mat src = img.Clone();
            Mat gray = new Mat();
            Cv2.CvtColor(src, gray, ColorConversionCodes.BGR2GRAY);
            Cv2.Threshold(gray, gray, 127, 255, ThresholdTypes.Otsu);
            Cv2.Dilate(gray, gray, new Mat(), null, 3);
            Cv2.Erode(gray, gray, new Mat(), null, 1);

            var max = PercentCourtImg(src.Width, src.Height, maxX, maxY);
            for (int x = 0; x < gray.Width; x++)
            {
                for (int y = 0; y < gray.Height; y++)
                {

                    if (x < max.X + (ArrowWidth / 2) | x > max.X + (ArrowWidth * 6) | y < max.Y - (ArrowHeight * 6) | y > max.Y)
                    {
                        Vec3b color = new Vec3b();
                        color.Item0 = 255;
                        color.Item1 = 255;
                        color.Item2 = 255;
                        gray.Set(y, x, color);
                    }

                }

            }
            return gray;
        }


        static double BoroderComparison(Mat imgInput)
        {
            OpenCvSharp.Point[][] ContoursInput = GetContours(imgInput);
            List<double> listLocation_X = new List<double>();
            List<double> listRatio = new List<double>();
            foreach (var contour in ContoursInput)
            {
                for (int i = 1; i < 7; i++)
                {
                    using (Mat subImg = Cv2.ImRead($"VerifyCaptchaTiktok\\img\\{i}.png", 0))
                    {
                        OpenCvSharp.Point[][] ContoursSub = GetContours(subImg);
                        var rect = Cv2.BoundingRect(contour);
                        if (contour.Length < 100 && contour.Length > 35 && rect.Width > 40 && rect.Width < 100 && rect.Height > 40 && rect.Height < 100)
                        {
                            var Shapes = Cv2.MatchShapes(ContoursSub[1], contour, ShapeMatchModes.I2);

                            if (Shapes != null)
                            {
                                Moments m = Cv2.Moments(contour);
                                listLocation_X.Add(m.M10 / m.M00);
                                listRatio.Add(Shapes);
                            }

                        }
                    }
                }
            }
            if (listRatio.Count != 0)
            {
                double Ratio = listRatio[0];
                double Location_X = listLocation_X[0];
                for (int i = 1; i < listRatio.Count; i++)
                {
                    if (Ratio > listRatio[i])
                    {
                        Location_X = listLocation_X[i];
                    }
                }
                double X = (Location_X * 100) / imgInput.Width;
                return X;

            }
            else
            {
                return 0;
            }


        }

        static OpenCvSharp.Point[][] GetContours(OpenCvSharp.Mat img)
        {
            OpenCvSharp.Point[][] contours;
            OpenCvSharp.HierarchyIndex[] hierarchy;
            Cv2.FindContours(img, out contours, out hierarchy, RetrievalModes.Tree, ContourApproximationModes.ApproxSimple);
            return contours;
        }
        // Tính tòa độ
        static (double X, double Y) PercentCourtImg(double Width, double Height, double percent_X, double percent_Y)
        {
            double num1 = (int)(percent_X * ((double)Width * 1.0 / 100.0));
            double num2 = (int)(percent_Y * ((double)Height * 1.0 / 100.0));
            num1 = Math.Round(num1);
            num2 = Math.Round(num2);
            return (num1, num2);
        }

    }

}


