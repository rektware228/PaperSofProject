using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Xml.Schema;

namespace PaperSofProject.DB
{
    public partial class Product
    {
        public BitmapImage Images
        {
            get
            {
                if (Image != null)
                {
                    BitmapImage bitmapImage = new BitmapImage();
                    using (MemoryStream byteStream = new MemoryStream(Image))
                    {
                        bitmapImage.BeginInit();
                        bitmapImage.StreamSource = byteStream;
                        bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                        bitmapImage.EndInit();
                        bitmapImage.Freeze(); 
                    }
                    return bitmapImage;
                }
                else
                {
                    
                    try
                    {
                        var bitmap = new BitmapImage();
                        string imagePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Image","products", "picture.png");
                        bitmap.BeginInit();
                        bitmap.UriSource = new Uri(imagePath, UriKind.Absolute);
                        bitmap.CacheOption = BitmapCacheOption.OnLoad;
                        bitmap.EndInit();
                        bitmap.Freeze();
                        return bitmap;

                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Ошибка загрузки изображения: {ex.Message}");
                        return null;
                    }
                }
            }
        }


        public string Materials
        {
            get
            {
                string materials = "";
                foreach (var item in ProductMaterial)
                {
                    materials += $" {item.Material.Name} ";
                }
                return materials;
            }
        }
        public double TotalCost
        {
            get
            {
                double totalcost = 0;
                foreach (var item in ProductMaterial)
                {
                    if (item.Count_Mat_Fot_One != null)
                    {
                        totalcost += (double)(item.Count_Mat_Fot_One * item.Material.Price);
                    }
                }
                if (totalcost != 0)
                {
                    return totalcost;
                }
                else
                {
                    return (double)(Min_Price_For_Agent);
                }
            }
        }
        public SolidColorBrush BackgroundColor
        {
            get
            {
                if (ProductSale != null && ProductSale.Any())
                {
                    var lastSaleDate = ProductSale.Last().SaleDate;
                    if (lastSaleDate.Value.Month != DateTime.Now.Month || lastSaleDate.Value.Year != DateTime.Now.Year)
                    {
                        return new SolidColorBrush(Colors.Tomato);
                    }
                }
                else
                {
                    return new SolidColorBrush(Colors.Tomato);
                }
                return new SolidColorBrush(Colors.White);
            }
        }

    }
}
