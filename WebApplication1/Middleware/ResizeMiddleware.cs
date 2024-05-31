using Newtonsoft.Json;
using System.Net;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using SkiaSharp;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
namespace MyMakler.Middleware
{
    /// <summary>
    /// Middleware для изменения размеров (и не только) фото
    /// </summary>
    public class ResizeMiddleware
    {
        struct ResizeParams
        {
            public bool HasParams;
            public int W;
            public int H;
            public bool Autorotate;
            public int Quality; // 0 - 100
            public string Format; // png, jpg, jpeg
            public string Mode;
            public static readonly string[] Modes = { 
                "pad", 
                "max", 
                "crop", 
                "stretch" 
            };

            public override string ToString()
            {
                var sb = new StringBuilder();
                sb.Append($"w: {W}, ");
                sb.Append($"h: {H}, ");
                sb.Append($"autorotate: {Autorotate}, ");
                sb.Append($"quality: {Quality}, ");
                sb.Append($"format: {Format}, ");
                sb.Append($"mode: {Mode}");

                return sb.ToString();
            }
        }
        private readonly Microsoft.AspNetCore.Hosting.IHostingEnvironment Environment;
        private readonly IMemoryCache MemoryCache;
        private readonly RequestDelegate _next;
        private static readonly string[] Suffixes = {
            "png",
            "jpg",
            "jpeg"
        };
        public ResizeMiddleware(RequestDelegate next, Microsoft.AspNetCore.Hosting.IHostingEnvironment environment, IMemoryCache memoryCache)
        {
            _next = next;
            Environment = environment;
            MemoryCache = memoryCache;
        }
        private bool IsImagePath(PathString path)
        {
            if (path == null || !path.HasValue)
                return false;

            return Suffixes.Any(x => x.EndsWith(x, StringComparison.OrdinalIgnoreCase));
        }
        public async Task Invoke(HttpContext context)
        {
            var path = context.Request.Path;

            // hand to next middleware if we are not dealing with an image
            if (context.Request.Query.Count == 0 || !IsImagePath(path))
            {
                await _next.Invoke(context);
                return;
            }
            // hand to next middleware if we are dealing with an image but it doesn't have any usable resize querystring params
            var resizeParams = GetResizeParams(path, context.Request.Query);
            if (!resizeParams.HasParams || (resizeParams.W == 0 && resizeParams.H == 0))
            {
                await _next.Invoke(context);
                return;
            }

            var imagePath = Path.Combine(
       Environment.WebRootPath,
       path.Value.Substring(5).Replace('/', Path.DirectorySeparatorChar).TrimStart(Path.DirectorySeparatorChar));

            // check file lastwrite
            var lastWriteTimeUtc = File.GetLastWriteTimeUtc(imagePath);
            if (lastWriteTimeUtc.Year == 1601) // file doesn't exist, pass to next middleware
            {
                await _next.Invoke(context);
                return;
            }

            var imageData = GetImageData(imagePath, resizeParams, lastWriteTimeUtc);

            // write to stream
            context.Response.ContentType = resizeParams.Format == "png" ? "image/png" : "image/jpeg";
            context.Response.ContentLength = imageData.Size;
            await context.Response.Body.WriteAsync(imageData.ToArray(), 0, (int)imageData.Size);

            // cleanup
            imageData.Dispose();
        }
        private ResizeParams GetResizeParams(PathString path, IQueryCollection query)
        {
            ResizeParams resizeParams = new ResizeParams();

            // before we extract, do a quick check for resize params
            resizeParams.HasParams =
                resizeParams.GetType().GetTypeInfo()
                .GetFields().Where(f => f.Name != "hasParams")
                .Any(f => query.ContainsKey(f.Name));

            // if no params present, bug out
            if (!resizeParams.HasParams)
                return resizeParams;

            // extract resize params

            if (query.ContainsKey("format"))
                resizeParams.Format = query["format"];
            else
                resizeParams.Format = path.Value.Substring(path.Value.LastIndexOf('.') + 1);

            if (query.ContainsKey("autorotate"))
                bool.TryParse(query["autorotate"], out resizeParams.Autorotate);

            int quality = 100;
            if (query.ContainsKey("quality"))
                int.TryParse(query["quality"], out quality);
            resizeParams.Quality = quality;

            int w = 0;
            if (query.ContainsKey("w"))
                int.TryParse(query["w"], out w);
            resizeParams.W = w;

            int h = 0;
            if (query.ContainsKey("h"))
                int.TryParse(query["h"], out h);
            resizeParams.H = h;

            resizeParams.Mode = "max";
            // only apply mode if it's a valid mode and both w and h are specified
            if (h != 0 && w != 0 && query.ContainsKey("mode") && ResizeParams.Modes.Any(m => query["mode"] == m))
                resizeParams.Mode = query["mode"];

            return resizeParams;
        }
        private SKData GetImageData(string imagePath, ResizeParams resizeParams, DateTime lastWriteTimeUtc)
        {
            // check cache and return if cached
            long cacheKey;
            unchecked
            {
                cacheKey = imagePath.GetHashCode() + lastWriteTimeUtc.ToBinary() + resizeParams.ToString().GetHashCode();
            }

            SKData imageData;
            byte[] imageBytes;
            bool isCached = MemoryCache.TryGetValue<byte[]>(cacheKey, out imageBytes);
            if (isCached)
            {
                return SKData.CreateCopy(imageBytes);
            }

            SKEncodedOrigin origin; // this represents the EXIF orientation
            var bitmap = LoadBitmap(File.OpenRead(imagePath), out origin); // always load as 32bit (to overcome issues with indexed color)

            // if autorotate = true, and origin isn't correct for the rotation, rotate it
            if (resizeParams.Autorotate && origin != SKEncodedOrigin.TopLeft) 
                bitmap = RotateAndFlip(bitmap, origin);

            // if either w or h is 0, set it based on ratio of original image
            if (resizeParams.H == 0)
                resizeParams.H = (int)Math.Round(bitmap.Height * (float)resizeParams.W / bitmap.Width);
            else if (resizeParams.W == 0)
                resizeParams.W = (int)Math.Round(bitmap.Width * (float)resizeParams.H / bitmap.Height);

            // if we need to crop, crop the original before resizing
            if (resizeParams.Mode == "crop")
                bitmap = Crop(bitmap, resizeParams);

            // store padded height and width
            var paddedHeight = resizeParams.H;
            var paddedWidth = resizeParams.W;

            // if we need to pad, or max, set the height or width according to ratio
            if (resizeParams.Mode == "pad" || resizeParams.Mode == "max")
            {
                var bitmapRatio = (float)bitmap.Width / bitmap.Height;
                var resizeRatio = (float)resizeParams.W / resizeParams.H;

                if (bitmapRatio > resizeRatio) // original is more "landscape"
                    resizeParams.H = (int)Math.Round(bitmap.Height * ((float)resizeParams.W / bitmap.Width));
                else
                    resizeParams.W = (int)Math.Round(bitmap.Width * ((float)resizeParams.H / bitmap.Height));
            }

            // resize
            var resizedImageInfo = new SKImageInfo(resizeParams.W, resizeParams.H, SKImageInfo.PlatformColorType, bitmap.AlphaType);
            var resizedBitmap = new SKBitmap(resizedImageInfo);
            bitmap.ScalePixels(resizedBitmap, SKFilterQuality.High);

            // optionally pad
            if (resizeParams.Mode == "pad")
                resizedBitmap = Pad(resizedBitmap, paddedWidth, paddedHeight, resizeParams.Format != "png");

            // encode
            var resizedImage = SKImage.FromBitmap(resizedBitmap);
            var encodeFormat = resizeParams.Format == "png" ? SKEncodedImageFormat.Png : SKEncodedImageFormat.Jpeg;
            imageData = resizedImage.Encode(encodeFormat, resizeParams.Quality);

            // cache the result
            MemoryCache.Set<byte[]>(cacheKey, imageData.ToArray());

            // cleanup
            resizedImage.Dispose();
            bitmap.Dispose();
            resizedBitmap.Dispose();

            return imageData;
        }
        private SKBitmap Crop(SKBitmap original, ResizeParams resizeParams)
        {
            var cropSides = 0;
            var cropTopBottom = 0;

            // calculate amount of pixels to remove from sides and top/bottom
            if ((float)resizeParams.W / original.Width < resizeParams.H / original.Height) // crop sides
                cropSides = original.Width - (int)Math.Round((float)original.Height / resizeParams.H * resizeParams.W);
            else
                cropTopBottom = original.Height - (int)Math.Round((float)original.Width / resizeParams.W * resizeParams.H);

            // setup crop rect
            var cropRect = new SKRectI
            {
                Left = cropSides / 2,
                Top = cropTopBottom / 2,
                Right = original.Width - cropSides + cropSides / 2,
                Bottom = original.Height - cropTopBottom + cropTopBottom / 2
            };

            // crop
            SKBitmap bitmap = new SKBitmap(cropRect.Width, cropRect.Height);
            original.ExtractSubset(bitmap, cropRect);
            original.Dispose();

            return bitmap;
        }
        private SKBitmap Pad(SKBitmap original, int paddedWidth, int paddedHeight, bool isOpaque)
        {
            // setup new bitmap and optionally clear
            var bitmap = new SKBitmap(paddedWidth, paddedHeight, isOpaque);
            var canvas = new SKCanvas(bitmap);
            if (isOpaque)
                canvas.Clear(new SKColor(255, 255, 255)); // we could make this color a resizeParam
            else
                canvas.Clear(SKColor.Empty);

            // find co-ords to draw original at
            var left = original.Width < paddedWidth ? (paddedWidth - original.Width) / 2 : 0;
            var top = original.Height < paddedHeight ? (paddedHeight - original.Height) / 2 : 0;

            var drawRect = new SKRectI
            {
                Left = left,
                Top = top,
                Right = original.Width + left,
                Bottom = original.Height + top
            };

            // draw original onto padded version
            canvas.DrawBitmap(original, drawRect);
            canvas.Flush();

            canvas.Dispose();
            original.Dispose();

            return bitmap;
        }
        private SKBitmap RotateAndFlip(SKBitmap original, SKEncodedOrigin origin)
        {
            // these are the origins that represent a 90 degree turn in some fashion
            var differentOrientations = new SKEncodedOrigin[]
            {
        SKEncodedOrigin.LeftBottom,
        SKEncodedOrigin.LeftTop,
        SKEncodedOrigin.RightBottom,
        SKEncodedOrigin.RightTop
            };

            // check if we need to turn the image
            bool isDifferentOrientation = differentOrientations.Any(o => o == origin);

            // define new width/height
            var width = isDifferentOrientation ? original.Height : original.Width;
            var height = isDifferentOrientation ? original.Width : original.Height;

            var bitmap = new SKBitmap(width, height, original.AlphaType == SKAlphaType.Opaque);

            // todo: the stuff in this switch statement should be rewritten to use pointers
            switch (origin)
            {
                case SKEncodedOrigin.LeftBottom:

                    for (var x = 0; x < original.Width; x++)
                        for (var y = 0; y < original.Height; y++)
                            bitmap.SetPixel(y, original.Width - 1 - x, original.GetPixel(x, y));
                    break;

                case SKEncodedOrigin.RightTop:

                    for (var x = 0; x < original.Width; x++)
                        for (var y = 0; y < original.Height; y++)
                            bitmap.SetPixel(original.Height - 1 - y, x, original.GetPixel(x, y));
                    break;

                case SKEncodedOrigin.RightBottom:

                    for (var x = 0; x < original.Width; x++)
                        for (var y = 0; y < original.Height; y++)
                            bitmap.SetPixel(original.Height - 1 - y, original.Width - 1 - x, original.GetPixel(x, y));

                    break;

                case SKEncodedOrigin.LeftTop:

                    for (var x = 0; x < original.Width; x++)
                        for (var y = 0; y < original.Height; y++)
                            bitmap.SetPixel(y, x, original.GetPixel(x, y));
                    break;

                case SKEncodedOrigin.BottomLeft:

                    for (var x = 0; x < original.Width; x++)
                        for (var y = 0; y < original.Height; y++)
                            bitmap.SetPixel(x, original.Height - 1 - y, original.GetPixel(x, y));
                    break;

                case SKEncodedOrigin.BottomRight:

                    for (var x = 0; x < original.Width; x++)
                        for (var y = 0; y < original.Height; y++)
                            bitmap.SetPixel(original.Width - 1 - x, original.Height - 1 - y, original.GetPixel(x, y));
                    break;

                case SKEncodedOrigin.TopRight:

                    for (var x = 0; x < original.Width; x++)
                        for (var y = 0; y < original.Height; y++)
                            bitmap.SetPixel(original.Width - 1 - x, y, original.GetPixel(x, y));
                    break;

            }

            original.Dispose();

            return bitmap;
        }
        private SKBitmap LoadBitmap(Stream stream, out SKEncodedOrigin origin)
        {
            using (var s = new SKManagedStream(stream))
            {
                using (var codec = SKCodec.Create(s))
                {
                    origin = codec.EncodedOrigin;
                    var info = codec.Info;
                    var bitmap = new SKBitmap(info.Width, info.Height, SKImageInfo.PlatformColorType, info.IsOpaque ? SKAlphaType.Opaque : SKAlphaType.Premul);

                    IntPtr length;
                    var result = codec.GetPixels(bitmap.Info, bitmap.GetPixels(out length));
                    if (result == SKCodecResult.Success || result == SKCodecResult.IncompleteInput)
                    {
                        return bitmap;
                    }
                    else
                    {
                        throw new ArgumentException("Unable to load bitmap from provided data");
                    }
                }
            }
        }
    }
    public static class ResizeMiddlewareExtensions
    {
        public static void UseResizeMiddleware(this IApplicationBuilder app)
        {
            app.UseMiddleware<ResizeMiddleware>();
        }
        public static void AddImageResizeMW(this IServiceCollection services)
        {
            services.AddMemoryCache();
        }
    }
}
