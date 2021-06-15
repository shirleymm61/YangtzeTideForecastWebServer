
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace ProcessImageBackColor
{
    class Program
    {
        static void Main(string[] args)
        {
            //循环输出
            for (int i = 0; i < 307; i++)
            {
                string fileName = $@"D:\WCHWork\Changjiang\png\{i}.png";
                string saveImg = $@"D:\pic\img\{i}.png";

                Console.WriteLine(CutImg2(fileName, saveImg, 98, 32, 1860, 1075));
            }

        }

        /// <summary>
        /// 图片剪切
        /// </summary>
        /// <param name="ImgFile">原图文件地址</param>
        /// <param name="sImgPath">缩略图保存地址</param>
        /// <param name="PointX">剪切起始点 X坐标</param>
        /// <param name="PointY">剪切起始点 Y坐标</param>
        /// <param name="CutWidth">剪切宽度</param>
        /// <param name="CutHeight">剪切高度</param>
        private static bool CutImg2(string ImgFile, string sImgPath, int PointX, int PointY, int CutWidth, int CutHeight)
        {
            try
            {
                Bitmap bmp = Image.FromFile(ImgFile) as Bitmap;

                //if (left == -1 || right == -1 || top == -1 || bottom == -1)
                //{
                //    left = 0;
                //    top = 0;
                //    right = bmp.Width;
                //    bottom = bmp.Height;
                //}

                //CutWidth = right - left;
                //CutHeight = bottom - top;

                string newImgFile = ImgFile.Replace(".png", "a.png");

                bmp.MakeTransparent(Color.White);
                bmp.Save(newImgFile, ImageFormat.Png);
                bmp.Dispose();
                FileStream fs = new FileStream(newImgFile, FileMode.Open);
                BinaryReader br = new BinaryReader(fs);
                byte[] bytes = br.ReadBytes((int)fs.Length);
                br.Close();
                fs.Close();
                MemoryStream ms = new MemoryStream(bytes);
                System.Drawing.Image imgPhoto = System.Drawing.Image.FromStream(ms);

                Bitmap bmPhoto = new Bitmap(CutWidth, CutHeight);//Format24bppRgb
                bmPhoto.SetResolution(72, 72);

                Graphics gbmPhoto = Graphics.FromImage(bmPhoto);
                gbmPhoto.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.High;
                gbmPhoto.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;

                // new Rectangle(left , top , CutHeight, CutHeight)
                gbmPhoto.DrawImage(imgPhoto, new Rectangle(0, 0, CutWidth, CutHeight), new Rectangle(PointX, PointY, CutWidth, CutHeight), GraphicsUnit.Pixel);

                bmPhoto.MakeTransparent(Color.Black);
                bmPhoto.Save(sImgPath, System.Drawing.Imaging.ImageFormat.Png);

                //PicHight = CutHeight;
                //PicWidth = CutWidth;

                imgPhoto.Dispose();
                gbmPhoto.Dispose();
                bmPhoto.Dispose();
                File.Delete(newImgFile);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return true;
        }
    }

}
